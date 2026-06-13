# Visión general de la arquitectura

Documento técnico transversal. Describe cómo encajan las escenas, los scripts y los datos. Cada
feature (`00X-*/plan.md`) profundiza en su parte; aquí está el mapa completo.

## Stack

- **Unity 2022.3.62f3** (LTS), C#.
- **Meta XR SDK** (OVR + Interaction SDK) para tracking de cabeza/manos, controladores y render VR.
- **Meta Quest 2** como dispositivo objetivo.
- **EzySlice** (`Assets/ezy-slice-master`) para el corte geométrico de mallas (efecto de sable).
- Beatmaps de **osu!mania** (`.zip`) como fuente de canciones, mapas y portadas.

## Flujo de navegación entre escenas

```
┌──────────────────┐   "Jugar"    ┌──────────────┐  click en card  ┌──────────────┐
│  MenuPrincipal   │ ───────────▶ │  MenuSongs   │ ──────────────▶ │  GameScene   │
│  (RF-01, RF-02)  │              │ (RF-03,04)   │  (canción elegida)│ (RF-05..15)  │
└──────────────────┘              └──────────────┘                 └──────────────┘
        │  "Salir"                                                         │ fin de canción
        ▼                                                                  ▼
   Application.Quit()                                              GameOver() (UI off)
```

La canción seleccionada se transmite entre escenas mediante la propiedad estática
`SongMenuManager.CancionSeleccionada` (el nombre del `.zip` sin extensión).

### Escenas del proyecto

| Escena | Rol |
|---|---|
| `MenuPrincipal.unity` | Pantalla de inicio: jugar / salir. |
| `MenuSongs.unity` | Lista de canciones con tarjetas (metadata + portada). |
| `GameScene.unity` | Partida: spawn de cubos, sables, puntaje, temporizador. |
| `OVRCameraRigScene.unity`, `SceneMetaXRInteraction.unity`, `SampleScene.unity` | Escenas de prueba / integración del SDK (no forman parte del flujo de producción). |

## Mapa de scripts

`Assets/Scripts/`

| Script | Responsabilidad | Feature |
|---|---|---|
| `MenuManager.cs` | Navegación del menú principal (jugar/salir). | 001 |
| `AudioManager.cs` | **Singleton** de audio (música, SFX, clics). Persiste entre escenas. | 001, 003 |
| `SongMenuManager.cs` | Escanea `ZipFiles/`, genera tarjetas, fija la canción elegida y carga `GameScene`. | 002 |
| `SongCardController.cs` | Pinta una tarjeta de canción (título, artista, BPM, duración, dificultad, portada). | 002 |
| `SongMetadata.cs` | DTO con los datos de una canción. | 002 |
| `OsuZipReader.cs` | Lee metadata y portada desde el `.zip` **sin extraerlo**. Estático y a prueba de fallos. | 002 |
| `GameSceneManager.cs` | Orquesta la partida: extrae `.osu`+audio del zip, carga audio, arranca el spawner, temporizador y barra de progreso. | 003, 005 |
| `CubeSpawnManager.cs` | Parsea `[HitObjects]`, asigna carriles y tipo A/B, y spawnea cubos sincronizados al audio. | 003 |
| `CubeMovement.cs` | Interpola cada cubo desde el spawn hasta el punto de golpeo en `approachTime`. | 003 |
| `CubeHit.cs` | Detecta el golpe del sable correcto dentro de la ventana de timing y calcula puntaje. | 004 |
| `Saber.cs` | Identidad del sable (`tipoSable`: 0 = izquierdo/A, 1 = derecho/B). | 004 |
| `SaberCollison.cs` | Colisión simple alternativa (destruye objetos con tag `Cube`). | 004 |
| `ScoreManager.cs` | **Singleton** por-escena: acumula y muestra el puntaje. | 004 |
| `IRONHEAD/Slicer.cs` | Corte físico de mallas con EzySlice (efecto visual del sable). | 004 |
| `IRONHEAD/CubeExplode.cs`, `CubeKiller.cs`, `Bullet.cs`, `Shooting.cs`, … | Assets importados (modo disparo/shatter). Reutilizados parcialmente; no todos activos. | — |
| `IRONHEAD/ButtonPushClick.cs` | Botón físico 3D pulsable (cuenta regresiva de inicio). | 001/005 |
| `NotUsing/*` | Código archivado, fuera de uso. | — |

## Singletons y ciclo de vida

- **`AudioManager.instance`** — creado en `Awake`, con `DontDestroyOnLoad`. Expone 4 `AudioSource`:
  `musicTheme`, `sliceSound`, `gunSound`, `buttonClickSound`. Es el reloj de referencia del
  *gameplay*: el tiempo de la canción (`musicTheme.time`) sincroniza el spawn y la detección de golpe.
- **`ScoreManager.Instance`** — creado en `Awake`, **sin** `DontDestroyOnLoad` (vive solo en `GameScene`).

## Pipeline de datos: de un `.zip` a una partida jugable

```
Assets/Resources/MusicFiles/ZipFiles/<cancion>.zip   (paquete osu!mania)
        │
        ├─(menú)──▶ OsuZipReader.LeerMetadata() ──▶ SongMetadata ──▶ SongCardController (tarjeta)
        │
        └─(partida)─▶ GameSceneManager.ProcesarZipYJugar():
                         1. Extrae el .osu elegido (prioriza [BEGINNER], si no el más pequeño)
                         2. Extrae el audio (archivo "audio.mp3" / "audio.ogg")
                         3. Carga el AudioClip vía UnityWebRequestMultimedia (archivo temporal)
                         4. CubeSpawnManager.InicializarMapaDesdeTexto(.osu)
                         5. Temporizador + barra de progreso; GameOver al terminar
```

### Dependencia del formato osu!mania

El sistema entiende un subconjunto del formato `.osu`:

- **`[Metadata]`** → `Title`, `TitleUnicode`, `Artist`, `ArtistUnicode`, `Version` (dificultad).
- **`[TimingPoints]`** → BPM = `60000 / beatLength` del primer punto **no heredado** (`beatLength > 0`).
- **`[Events]`** → imagen de fondo (`0,0,"archivo.jpg",...`) usada como portada.
- **`[HitObjects]`** → `x,y,time,type,...`. La columna **x** define el carril (4 carriles por rangos
  de x: ≤128, ≤256, ≤384, >384). `time` (ms) define el instante de golpeo. La duración de la canción
  se estima con el último `time` (incluye colas de notas *hold*, `type & 128`).

## Estructura de carpetas relevante

```
Assets/
  Scripts/            ← lógica C# (ver mapa arriba)
  Scenes/             ← MenuPrincipal, MenuSongs, GameScene (+ escenas de prueba)
  Prefabs/            ← sable.prefab, SongCard.prefab, SongButton.prefab, ButtonVisual.prefab
  Resources/
    MusicFiles/
      ZipFiles/       ← canciones .zip (osu!mania)  ← fuente de verdad de las canciones
      AudioFiles/     ← audio suelto (pruebas)
  Models/ Materials/ Textures/ Shaders/ ← arte y assets
  ezy-slice-master/   ← librería de corte de mallas
  Oculus/ XR/ Samples/ InteractionSDK/ ← Meta XR SDK
specs/                ← esta documentación
```
