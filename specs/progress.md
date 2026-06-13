# Estado de avance — Matriz RF / RNF

Estado del proyecto al **13 de junio de 2026**. Leyenda: ✅ implementado · 🟡 parcial · ⬜ pendiente.

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
| RF-10 | Registro de fallos | Must | ⬜ | `CubeMovement.cs` destruye el cubo al pasar **sin** contabilizar fallo |
| RF-11 | Puntuación en tiempo real | Should | ✅ | `CubeHit.cs` (puntaje por precisión, mín. 10) + `ScoreManager.cs` (UI) |
| RF-12 | Pausa de partida | Should | ⬜ | No implementado |
| RF-13 | Opciones en pausa (continuar/reiniciar/menú) | Should | ⬜ | No implementado |
| RF-14 | Pantalla de resultados (puntaje, precisión, aciertos/fallos) | Must | ⬜ | `GameSceneManager.GameOver()` solo desactiva UI; falta panel de resultados |
| RF-15 | Acciones post-partida (reiniciar / volver) | Should | ⬜ | No implementado |

**Resumen:** 8 ✅ · 2 🟡 · 5 ⬜ (de 15). Los *Must Have* del núcleo de juego (RF-01, 03, 05, 06, 07, 08)
están cubiertos; quedan pendientes RF-10 y RF-14 (*Must*) y el ciclo de fin de partida.

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
| 13 | RF-09, RF-12–15 + háptica | 🟡 En progreso — falta pausa, resultados y háptica |
| 14 | RF-02, RF-04 + optimización | RF-04 entregado (tarjetas con metadata y portada); resto pendiente |
| 15 | Demo + métricas HEART | Pendiente |

## Deuda técnica y riesgos

- **Fallos no contabilizados (RF-10):** `CubeMovement` destruye los cubos no golpeados sin notificar
  a `ScoreManager`; sin esto no hay precisión real (RF-14) ni métrica *Task Success* (HEART).
- **Doble ruta de colisión:** coexisten `CubeHit` (golpeo con timing + puntaje) y `SaberCollison`
  (destrucción simple por tag `Cube`). Conviene unificar para evitar comportamiento ambiguo.
- **Calibración manual de timing:** `approachTime = 2.221`, `leadInDelay = 3.5` y `hitPointOffset`
  están afinados a mano (`CubeSpawnManager`); un cambio de escala de escena los rompe.
- **Háptica ausente (RF-09/RNF-04):** falta `OVRInput.SetControllerVibration` en `CubeHit`.
- **Persistencia de servicios:** `ScoreManager` no usa `DontDestroyOnLoad`; al añadir resultados en
  otra escena habrá que decidir cómo transportar el puntaje final.

## Próximos pasos sugeridos (orden recomendado)

1. **RF-10** — contabilizar fallos en `CubeMovement` → habilita precisión.
2. **RF-09 / RNF-04** — háptica en el golpe acertado.
3. **RF-14 / RF-15** — pantalla de resultados (puntaje, precisión, aciertos/fallos) + acciones.
4. **RF-12 / RF-13** — pausa con continuar/reiniciar/menú.
5. **RF-02** — pista ambiental de menú.
6. **RNF-01..06** — profiling en dispositivo y pruebas de confort/usabilidad (métricas HEART, sem 15).
