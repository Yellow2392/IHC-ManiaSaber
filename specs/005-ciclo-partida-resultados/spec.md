# 005 — Ciclo de partida y resultados · spec

**Estado:** 🟡 Parcial
**Requisitos:** RF-12 (Pausa ⬜), RF-13 (Opciones en pausa ⬜), RF-14 (Pantalla de resultados ⬜),
RF-15 (Acciones post-partida ⬜). Base de ciclo (temporizador + progreso + fin) ✅.

## Objetivo

Gestionar el **ciclo de vida de una partida**: arranque con la canción elegida, indicación de
progreso, posibilidad de **pausar**, y un **cierre con resultados** que resuma el desempeño y ofrezca
acciones (reiniciar / volver al menú). Cierra el "arco de historia" de la experiencia.

## Porqué

El cierre es donde el usuario evalúa su desempeño y decide continuar — clave para *Engagement* y
*Retention* (HEART). La pausa da *control y libertad* (heurística 3). El progreso visible cumple
*visibilidad del estado del sistema* (P7). Hoy el ciclo arranca y termina, pero sin pausa ni
resultados ricos: el bucle de re-jugar queda incompleto.

## Historias de usuario

- **HU-05.1** — Como jugadora quiero ver cuánto falta de la canción mediante una barra de progreso y
  un tiempo.
- **HU-05.2** — Como jugadora quiero pausar la partida en cualquier momento (p. ej. para descansar el
  brazo) y reanudar, reiniciar o salir al menú.
- **HU-05.3** — Como jugadora, al terminar la canción quiero ver mi puntaje total, mi precisión y mis
  aciertos/fallos.
- **HU-05.4** — Como jugadora quiero, desde los resultados, reiniciar la canción o volver a la
  selección con un botón grande y accesible.

## Criterios de aceptación

### Base del ciclo ✅
- **Dado** que se eligió una canción, **cuando** carga `GameScene`, **entonces** se extrae su `.osu` y
  audio, se carga el `AudioClip` y arranca el mapa.
- Durante la partida se muestra una **barra de progreso** (`fillAmount = musicTheme.time / duración`)
  y un **tiempo** (cuenta regresiva `mm:ss`).
- **Cuando** la canción termina, **entonces** se ejecuta fin de partida (`GameOver`) desactivando el
  spawner y la UI del temporizador.

### RF-12 — Pausa ⬜
- **Cuando** el usuario invoca pausa, **debe** detenerse la música, el spawn y el movimiento de cubos,
  y mostrarse un panel de pausa. *(No implementado.)*

### RF-13 — Opciones en pausa ⬜
- Desde la pausa, el usuario **debe** poder **continuar**, **reiniciar** la canción o **volver al
  menú principal**. *(No implementado.)*

### RF-14 — Pantalla de resultados ⬜
- Al finalizar, **debe** mostrarse un panel con **puntaje total**, **precisión** y **conteo de
  aciertos/fallos**. Hoy `GameOver()` solo **oculta** la UI; no hay panel de resultados ni cálculo de
  precisión (depende de RF-10, registro de fallos). *(Pendiente.)*

### RF-15 — Acciones post-partida ⬜
- Desde los resultados, **debe** poder **reiniciar** la canción o **volver** a la selección.
  *(No implementado.)*

## Fuera de alcance

- Tablas de clasificación / rango global persistente.
- Guardado de mejores puntajes entre sesiones.
- Repetición por secciones de la canción.

## Dependencias

- `SongMenuManager.CancionSeleccionada` para saber qué canción cargar.
- `AudioManager.musicTheme` (reloj y fin de canción) y `ScoreManager` (puntaje final).
- **RF-10** (registro de fallos) es prerequisito de la **precisión** mostrada en RF-14.
- Para reiniciar/volver: recarga de escena o navegación con `SceneManager`. Definir cómo transportar
  el puntaje final si los resultados viven en otra escena (`ScoreManager` no es persistente hoy).
