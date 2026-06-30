# Pendiente manual en el Editor de Unity — RF-14 / RF-15

La **lógica de código** de RF-10, RF-14 y RF-15 ya está implementada y commiteada. Lo único que
falta es el **cableado en el Editor** sobre la escena `Assets/Scenes/GameScene.unity`, panel
`UI_ScoreFinal` (= `panelResultados`, fileID `546668396`). No se puede hacer por CLI con seguridad.

## RF-14 — Textos de resultados
- [ ] Crear un texto **TextMeshPro** para la **precisión** dentro de `UI_ScoreFinal`.
- [ ] Crear un texto **TextMeshPro** para **aciertos/fallos** dentro de `UI_ScoreFinal`.
- [ ] En el inspector del `ScoreManager` de la escena, asignar:
  - `precisionText` → el texto de precisión.
  - `aciertosFallosText` → el texto de aciertos/fallos.
  - (Ya estaban cableados `finalScoreText` y `highScoreText`.)

## RF-15 — Botones post-partida
- [ ] Añadir el componente `ResultadosController` a un objeto de la escena (p. ej. el propio
      `UI_ScoreFinal`). Verificar que `escenaSeleccion` = `"MenuSongs"`.
- [ ] Crear un **UI Button** "Reintentar" dentro de `UI_ScoreFinal` (grande y accesible, HU-05.4).
- [ ] Crear un **UI Button** "Volver a canciones" dentro de `UI_ScoreFinal`.
- [ ] Cablear `onClick`:
  - Botón "Reintentar" → `ResultadosController.Reintentar()`.
  - Botón "Volver a canciones" → `ResultadosController.VolverASeleccion()`.
- [ ] Replicar la configuración de interacción de los botones de `MenuSongs` (Canvas world-space +
      `GraphicRaycaster` + puntero/sable VR) para que se puedan activar igual que en el menú.
- [ ] Confirmar que `MenuSongs` está en **Build Settings → Scenes In Build** (necesario para
      `SceneManager.LoadScene("MenuSongs")`).

## Verificación end-to-end (en el Editor)
1. Entrar a `GameScene` desde `MenuSongs` (o poner un zip real en el fallback de
   `GameSceneManager.cs:34`).
2. Jugar: golpear bien algunos cubos y dejar pasar otros adrede.
3. Al terminar la canción, el panel debe mostrar: puntaje, récord, **precisión** y **aciertos/fallos**
   coherentes (sumar a mano: el total debe cuadrar con el nº de notas del mapa).
4. **Reintentar** → recarga la misma canción desde cero (contadores en 0).
5. **Volver a canciones** → carga `MenuSongs`.
6. Consola sin `NullReference` (todo bien cableado).
