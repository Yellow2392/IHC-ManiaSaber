# 003 — Generación rítmica de cubos · spec

**Estado:** ✅ Implementado
**Requisitos:** RF-05 (Generación de cubos sincronizada), RF-06 (Distribución en 4 carriles),
RF-07 (Diferenciación por color/sable)

## Objetivo

Convertir un beatmap de osu!mania en una secuencia de **cubos que viajan hacia el usuario en sincronía
con la música**, distribuidos en cuatro carriles y diferenciados por color según el sable con el que
deben golpearse. Es el corazón de la mecánica: la fuente del reto rítmico.

## Porqué

La sincronía precisa entre estímulo auditivo y aparición del cubo (RNF-03, ±10 ms) es lo que produce
la sensación de "tocar la música" y habilita el estado de *flow* (P1, P5). Los 4 carriles mapean a
posiciones naturalmente alcanzables (P3) y el color comunica de forma inmediata qué mano usar (P2/P6).

## Historias de usuario

- **HU-03.1** — Como jugadora quiero que los cubos aparezcan al ritmo de la canción para sentir que
  mis golpes están "dentro" de la música.
- **HU-03.2** — Como jugadora quiero ver los cubos venir con anticipación suficiente para reaccionar a
  tiempo, sin que aparezcan de golpe.
- **HU-03.3** — Como jugadora quiero que cada cubo me indique con su color qué sable usar, sin tener
  que leer instrucciones.

## Criterios de aceptación

### RF-05 — Generación sincronizada ✅
- **Dado** un mapa cargado, **cuando** transcurre el tiempo de audio, **entonces** cada cubo se crea
  en `tiempoGolpe − approachTime`, de modo que llega al punto de golpeo justo en su beat.
- **Antes** de que suene la música hay un *lead-in* (`leadInDelay`) para que los primeros cubos se
  vean venir; durante ese silencio se usa un cronómetro interno y luego se conmuta a `musicTheme.time`.
- **Cuando** se agotan las notas, **entonces** se registra fin de spawn.

### RF-06 — Distribución en 4 carriles ✅
- Cada nota se ubica en uno de **4 carriles** según su columna `x` en el `.osu` (≤128, ≤256, ≤384, >384).
- La distribución es **determinista** (no aleatoria): refleja exactamente el beatmap.
- Los cubos se mueven desde su `Spawnpoint` hasta `Spawnpoint + hitPointOffset` en `approachTime`.

### RF-07 — Diferenciación por color/sable ✅
- Existen **2 tipos de cubo** (A = izquierdo, B = derecho), instanciados desde prefabs distintos.
- En **acordes** (notas en el mismo instante, ≤ 5 ms) los tipos **alternan** A/B para repartir el
  esfuerzo entre ambas manos.
- El tipo asignado se transfiere al cubo (`tipoCuboAsignado`) para validar el golpe (ver feature 004).

## Fuera de alcance

- Notas *hold* (mantenidas): se leen para estimar duración pero no generan mecánica de mantener.
- Patrones generados proceduralmente: el mapa proviene íntegro del `.osu`.
- Selección de carril por color (el color depende del tipo A/B, no del carril).

## Dependencias

- `AudioManager.musicTheme` como reloj de sincronía.
- `GameSceneManager` provee el texto del `.osu` y el `AudioClip` ya cargado.
- Prefabs de cubo con `CubeHit` y `CubeMovement`.
