# 002 — Selección de canciones · spec

**Estado:** ✅ Implementado
**Requisitos:** RF-03 (Lista de canciones), RF-04 (Información de canción)

## Objetivo

Presentar al usuario el catálogo de canciones disponibles como **tarjetas** ricas en información
(título, artista, BPM, duración, dificultad y portada), permitiéndole elegir una y pasar a la
partida. El catálogo se construye automáticamente a partir de los paquetes de canciones presentes en
el proyecto, sin configuración manual por canción.

## Porqué

El usuario debe poder decidir su canción "favorita" con la información mínima necesaria para elegir
(P6: solo lo estrictamente necesario), apoyando la heurística de *control y libertad*. La carga
automática desde archivos `.zip` permite ampliar el catálogo solo agregando archivos.

## Historias de usuario

- **HU-02.1** — Como jugadora quiero ver la lista de canciones disponibles para escoger cuál jugar.
- **HU-02.2** — Como jugadora quiero ver el título, artista, BPM y duración de cada canción para
  decidir con criterio antes de entrar.
- **HU-02.3** — Como jugadora quiero ver la portada y un indicador de dificultad de cada canción para
  reconocerla y calibrar el reto de un vistazo.
- **HU-02.4** — Como jugadora, al seleccionar una canción quiero entrar directamente a la partida con
  esa canción cargada.

## Criterios de aceptación

### RF-03 — Lista de canciones ✅
- **Dado** que existe al menos un `.zip` válido en `Resources/MusicFiles/ZipFiles/`, **cuando** se
  abre `MenuSongs`, **entonces** se genera una tarjeta por canción encontrada.
- **Cuando** no hay archivos, **entonces** se registra un *warning* y la lista queda vacía sin
  romper la escena.

### RF-04 — Información de canción ✅
- Cada tarjeta muestra: **título**, **artista** (o "Artista desconocido"), **BPM** (o "BPM --"),
  **duración** `mm:ss` (o "--:--") y **dificultad** con insignia de color.
- **Cuando** el `.osu` referencia una imagen de fondo, **entonces** la tarjeta muestra la **portada**
  (recortada al centro para no deformarla); si no, muestra un *placeholder*.
- **Cuando** un campo falta o el archivo está corrupto, **entonces** se usa un valor de respaldo
  (el nombre del archivo como título) sin lanzar excepción.

### Selección
- **Cuando** el usuario pulsa una tarjeta, **entonces** se fija `CancionSeleccionada` (nombre del zip)
  y se carga `GameScene`.

## Fuera de alcance

- Filtros, búsqueda, ordenamiento o paginación del catálogo.
- Previsualización de audio al pasar el cursor.
- Selección de dificultad concreta dentro de una canción (se elige automáticamente: se prioriza el
  `.osu` marcado `[BEGINNER]` o, en su defecto, el de menor tamaño; ver [`overview.md`](../overview.md)).

## Dependencias

- `OsuZipReader` para leer metadata; `SongMetadata` como contrato de datos.
- Prefab de tarjeta (`SongCard.prefab`) con `SongCardController` cableado.
