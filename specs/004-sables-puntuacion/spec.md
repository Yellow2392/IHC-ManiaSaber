# 004 — Sables, impacto y puntuación · spec

**Estado:** 🟡 Parcial
**Requisitos:** RF-08 (Detección de impacto ✅), RF-09 (Feedback multimodal 🟡), RF-10 (Registro de
fallos ⬜), RF-11 (Puntuación en tiempo real ✅)

## Objetivo

Permitir que el usuario **golpee los cubos con los sables** acoplados a sus controladores, validar el
golpe según el sable correcto y la precisión rítmica, y traducirlo en **puntaje en tiempo real** con
**feedback inmediato**. Es donde la acción física del usuario se convierte en consecuencia en el juego.

## Porqué

El golpe es el momento de mayor agencia: aquí se cierra el bucle percepción→acción→recompensa. El
feedback multimodal (P2) y el mapping 1:1 (P3) son los que generan la sensación de "ser uno con la
canción". La puntuación por precisión recompensa el ajuste rítmico (P1) en lugar de la fuerza o el
ángulo.

## Historias de usuario

- **HU-04.1** — Como jugadora quiero golpear un cubo con el sable y que el sistema reconozca el acierto.
- **HU-04.2** — Como jugadora quiero que solo cuente si uso el sable del color correcto, para que el
  reto tenga sentido.
- **HU-04.3** — Como jugadora quiero recibir confirmación inmediata (visual, sonora y de vibración)
  de cada acierto.
- **HU-04.4** — Como jugadora quiero ver mi puntaje subir en tiempo real, premiando mi precisión.
- **HU-04.5** — Como jugadora quiero que los cubos que dejo pasar cuenten como fallo, para conocer mi
  desempeño real.

## Criterios de aceptación

### RF-08 — Detección de impacto ✅
- **Dado** un cubo activo, **cuando** un sable entra en su colisionador (`OnTriggerEnter`) **y** el
  `tipoSable` coincide con `tipoCuboAsignado`, **entonces** se evalúa el golpe.
- El golpe solo es válido si la diferencia entre el tiempo de audio actual y `tiempoGolpeExacto` está
  dentro de `margenErrorMaximo` (0.2 s). Fuera de ventana, el contacto no cuenta como acierto.
- **Cuando** el golpe es válido, **entonces** el cubo se destruye.

### RF-11 — Puntuación en tiempo real ✅
- El puntaje de cada acierto se interpola por **precisión**: `precisión = 1 − (|Δt| / margen)`,
  `puntos = max(10, round(precisión × 100))` (acierto perfecto = 100, mínimo 10).
- **Cuando** se acierta, **entonces** `ScoreManager` acumula y actualiza el texto de puntaje visible.

### RF-09 — Feedback multimodal 🟡
- **Visual:** existe corte/estallido del cubo (`Slicer` con EzySlice / `CubeExplode`).
- **Sonoro:** `AudioManager` dispone de `sliceSound` para el efecto de corte.
- **Háptico (pendiente):** debe emitirse vibración del controlador al acertar, en ≤ 16 ms (RNF-04).
  Hoy **no** hay llamada de vibración (`OVRInput.SetControllerVibration`) en el golpe.

### RF-10 — Registro de fallos ⬜
- **Cuando** un cubo cruza el punto de golpeo sin ser golpeado, **debe** contabilizarse como fallo.
- Hoy `CubeMovement` **destruye** el cubo al llegar al final sin notificar a `ScoreManager`: el fallo
  no se registra. Requisito **pendiente**.

## Fuera de alcance

- Combos / multiplicadores de racha.
- Penalización por golpear con el sable equivocado.
- Cálculo de precisión global de la canción (depende de RF-10; se consolida en feature 005).

## Dependencias

- `Saber.tipoSable` debe estar correctamente asignado en cada controlador (0 = izquierdo/A, 1 = derecho/B).
- `AudioManager.musicTheme.time` como referencia temporal del golpe.
- `ScoreManager` presente en la escena de juego.

## Notas de implementación

- Coexisten dos rutas de colisión: `CubeHit` (golpeo con timing + puntaje, **ruta principal**) y
  `SaberCollison` (destrucción simple por tag `Cube`, ruta alternativa/legacy). Conviene unificarlas
  para evitar comportamiento ambiguo (ver deuda técnica en [`progress.md`](../progress.md)).
