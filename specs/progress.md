# Estado de avance — Matriz RF / RNF

Estado del proyecto al **20 de junio de 2026**. Leyenda: ✅ implementado · 🟡 parcial · ⬜ pendiente.

## Requisitos funcionales

| RF | Requisito | Tipo | Estado | Dónde / Nota |
|----|-----------|------|:--:|------|
| RF-01 | Pantalla de inicio (jugar / salir) | Must | ✅ | `MenuManager.cs`, `MenuPrincipal.unity` |
| RF-02 | Música de menú | Nice | 🟡 | `AudioManager.cs` expone fuentes (`musicTheme`, `buttonClickSound`); cableado de la pista de menú depende de la escena |
| RF-03 | Lista de canciones (≥ 1) | Must | ✅ | `SongMenuManager.cs` escanea `ZipFiles/` (≈9 canciones disponibles) |
| RF-04 | Info de canción (título, BPM, duración) | Nice | ✅ | `SongCardController.cs` + `OsuZipReader.cs`: título, artista, BPM, duración, dificultad, portada |
| RF-05 | Generación de cubos sincronizada | Must | ✅ | `CubeSpawnManager.cs` (spawn según `tiempoGolpe − approachTime` sobre `musicTheme.time`) |
| RF-06 | Distribución en 4 posiciones | Must | ✅ | `CubeSpawnManager.cs` mapeo de columna x → carril 0-3 |
| RF-07 | Diferenciación de cubos por color/sable | Must | ✅ | Prefabs A/B + asignación `tipoCubo` (alterna en acordes) |
| RF-08 | Detección de impacto | Must | ✅ | `CubeHit.cs` (`OnTriggerEnter` + match `Saber.tipoSable`) |
| RF-09 | Feedback visual/sonoro/háptico al acertar | Should | 🟡 | Corte visual (`Slicer`/`CubeExplode`) presente; **háptica no implementada** (sin `OVRInput.SetControllerVibration`) |
| RF-10 | Registro de fallos | Must | ✅ | `CubeHit`/`CubeMovement` registran acierto/fallo en `ScoreManager` (flag `resuelto` evita doble conteo) |
| RF-11 | Puntuación en tiempo real | Should | ✅ | `CubeHit.cs` (puntaje por precisión, mín. 10) + `ScoreManager.cs` (UI) |
| RF-12 | Pausa de partida | Should | ✅ | `PauseManager.cs`: toggle con botón ☰ (`OVRInput.Button.Start`); congela `Time.timeScale=0` **+** `musicTheme.Pause()`; canvas `UI_Pause` y refs cableados en `GameScene` ✅ |
| RF-13 | Opciones en pausa (continuar/reiniciar/menú) | Should | ✅ | `PauseManager.cs`: A=continuar, B=reiniciar (recarga `GameScene`), X=menú (`MenuSongs`); métodos públicos listos para cablear a `Button.onClick` |
| RF-14 | Pantalla de resultados (puntaje, precisión, aciertos/fallos) | Must | 🟡 | `ScoreManager.FinalizarPartida()` calcula y pinta puntaje, récord, precisión y aciertos/fallos; **falta crear/cablear** `precisionText`/`aciertosFallosText` en `UI_ScoreFinal` (Editor) |
| RF-15 | Acciones post-partida (reiniciar / volver) | Should | 🟡 | `ResultadosController.cs` (`Reintentar`/`VolverASeleccion`); **falta crear/cablear** los botones en `UI_ScoreFinal` (Editor) |

**Resumen:** 11 ✅ · 4 🟡 · 0 ⬜ (de 15). Los *Must Have* del núcleo de juego (RF-01, 03, 05, 06, 07,
08, 10) están cubiertos, y la pausa (RF-12/RF-13) quedó implementada y cableada (`PauseManager` +
`UI_Pause`). RF-14 y RF-15 quedan 🟡: la lógica de código está completa y solo resta el cableado de
UI/botones en el Editor (ver spec 005). Pendientes reales: háptica (RF-09) y pista de menú (RF-02).

## Requisitos no funcionales

| RNF | Atributo | Umbral | Estado | Nota |
|-----|----------|--------|:--:|------|
| RNF-01 | Tasa de refresco | ≥ 72 FPS | 🟡 | Por verificar con profiling en dispositivo |
| RNF-02 | Latencia visual | ≤ 20 ms | 🟡 | Provista por el runtime de Quest; sin medición propia |
| RNF-03 | Sincronía audio↔cubo | ± 10 ms | 🟡 | Spawn anclado a `musicTheme.time`; `approachTime`/`leadInDelay` calibrados manualmente |
| RNF-04 | Respuesta háptica | ≤ 16 ms | ⬜ | Depende de RF-09 (háptica aún no implementada) |
| RNF-05 | Curva de aprendizaje | < 30 s | 🟡 | Diseño omnidireccional lo favorece; pendiente validación con usuarios |
| RNF-06 | Tiempo de carga entre pantallas | < 3 s | 🟡 | Carga de audio del zip vía `UnityWebRequest`; medir con canciones grandes |
| RNF-07 | Compatibilidad Meta Quest 2 | — | ✅ | Proyecto configurado con Meta XR SDK / Quest |

## Avance por semana (planificado vs. real)

Cronograma planificado: semanas 10–15. Reconstrucción del avance real a partir del historial de git:

| Sem. | Plan (entregable) | Real (commits) |
|------|-------------------|----------------|
| 10 | RF-01, RF-03 + setup Unity/Meta XR | Interfaz principal y botones; sincronía de audio de la primera canción (`feat: Audio sync on first song`) |
| 11 | RF-05, RF-06 + sincronía audio-cubo | Lista de canciones + spawn correcto de cubos; lectura mp3/ogg; `Cube Spawn times fixed` |
| 12 | RF-07, RF-08, RF-10, RF-11 + 72 FPS/latencia | Sables atados a controladores; primeros pasos de colisiones; cubos sincronizados |
| 13 | RF-09, RF-12–15 + háptica | 🟡 En progreso — RF-12/RF-13 (pausa + opciones) y RF-14 (resultados) entregados; falta háptica (RF-09) y RF-15 |
| 14 | RF-02, RF-04 + optimización | RF-04 entregado (tarjetas con metadata y portada); resto pendiente |
| 15 | Demo + métricas HEART | Pendiente |

## Deuda técnica y riesgos

- ~~**Fallos no contabilizados (RF-10):**~~ Resuelto: `CubeHit` y `CubeMovement` notifican
  acierto/fallo a `ScoreManager` (flag `resuelto` evita doble conteo). Ya hay precisión real (RF-14).
- **Doble ruta de colisión:** coexisten `CubeHit` (golpeo con timing + puntaje) y `SaberCollison`
  (destrucción simple por tag `Cube`). Conviene unificar para evitar comportamiento ambiguo.
- **Calibración manual de timing:** `approachTime = 2.221`, `leadInDelay = 3.5` y `hitPointOffset`
  están afinados a mano (`CubeSpawnManager`); un cambio de escala de escena los rompe.
- **Háptica ausente (RF-09/RNF-04):** falta `OVRInput.SetControllerVibration` en `CubeHit`.
- **Persistencia de servicios:** `ScoreManager` no usa `DontDestroyOnLoad`. No es problema hoy porque
  los resultados viven en la misma `GameScene`; reintentar la recarga y los contadores vuelven a 0
  (correcto). Solo habría que revisarlo si los resultados se movieran a otra escena.
- **Selección de opciones de pausa por botones (RF-13):** con `Time.timeScale = 0` la física se
  detiene, así que el menú de pausa no usa UI clicable (poke/raycaster) sino botones del control
  (`OVRInput`: A/B/X) con pistas en pantalla. Los métodos `Continuar/Reiniciar/VolverAlMenu` de
  `PauseManager` son públicos y quedan listos para cablear a `Button.onClick` si en el futuro se
  habilita un rig de puntero. El canvas `UI_Pause` y la referencia `pauseManager` en
  `GameSceneManager` ya quedaron cableados en `GameScene` (ver
  [`setup-pausa-editor.md`](setup-pausa-editor.md), marcado como completado).

## Próximos pasos sugeridos (orden recomendado)

1. **RF-14 / RF-15 (cierre)** — en el Editor: crear y cablear `precisionText`/`aciertosFallosText` y
   los botones Reintentar/Volver en `UI_ScoreFinal` (la lógica de código ya está lista).
2. **RF-09 / RNF-04** — háptica en el golpe acertado.
3. ~~**RF-12 / RF-13** — pausa con continuar/reiniciar/menú.~~ ✅ Implementado y cableado en `GameScene` (`PauseManager.cs` + canvas `UI_Pause`).
4. **RF-02** — pista ambiental de menú.
5. **RNF-01..06** — profiling en dispositivo y pruebas de confort/usabilidad (métricas HEART, sem 15).
