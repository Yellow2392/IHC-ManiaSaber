# Pendiente manual en el Editor — Pausa (RF-12 / RF-13)

La lógica de pausa está en `Assets/Scripts/PauseManager.cs` y ya integrada en
`GameSceneManager`. Para que funcione en juego **falta cablear la escena en el
Editor de Unity** (no es editable por código). Todo esto se hace en
`Assets/Scenes/GameScene.unity`.

## 1. Crear el canvas `UI_Pause`

1. Duplicar `UI_ScoreFinal` (ya es world-space, escala `0.006`, posicionado frente
   al jugador) y renombrar el duplicado a **`UI_Pause`**.
2. Dejar dentro solo:
   - `Image_Background` (negro, alpha ~0.8) — se reutiliza el del duplicado.
   - Un texto título (TextMeshProUGUI): **"PAUSA"**.
   - Tres textos de pista (TextMeshProUGUI), uno por acción:
     - **"A — Continuar"**
     - **"B — Reiniciar"**
     - **"X — Menú"**
   - Eliminar los textos de puntaje/récord y el `BackButton` heredados de `UI_ScoreFinal`.
3. Dejar `UI_Pause` **desactivado** por defecto (checkbox del GameObject en off),
   igual que `UI_ScoreFinal`. `PauseManager` lo activa/desactiva.

## 2. Crear el GameObject `PauseManager`

1. Crear un GameObject vacío llamado **`PauseManager`**.
2. Añadirle el componente **`PauseManager`** (script).
3. Asignar campos en el Inspector:
   - `Panel Pausa` → arrastrar **`UI_Pause`**.
   - `Escena Menu` = `MenuSongs` (valor por defecto, confirmar).
   - `Escena Juego` = `GameScene` (valor por defecto, confirmar).

## 3. Conectar con `GameSceneManager`

1. Seleccionar el GameObject que tiene el componente `GameSceneManager`.
2. Asignar el campo **`Pause Manager`** → arrastrar el GameObject `PauseManager`.
   (Si se omite, `GameSceneManager` lo resuelve por `PauseManager.Instance`, pero
   conviene asignarlo explícitamente.)

## 4. Verificación (en el Editor o en Quest 2)

1. Entrar a `GameScene`, dejar que arranque la canción y pulsar **☰** (control izq.):
   - Los cubos se detienen y **no aparecen nuevos** (valida `musicTheme.Pause()`).
   - El audio y el contador/barra se congelan.
   - Aparece `UI_Pause`.
2. **A** → reanuda en el mismo punto, sin desfase audio↔cubo (repetir varias veces).
3. **B** → recarga la misma canción desde 0, a velocidad normal.
4. **X** → vuelve a `MenuSongs`, audio detenido.
5. Dejar terminar la canción → pantalla de resultados; pulsar ☰ **no** debe pausar.

## Notas de diseño

- La selección de opciones usa **botones del control** (no UI clicable) porque con
  `Time.timeScale = 0` la física (FixedUpdate) se detiene y el poke/raycaster no
  responde. Por eso el panel muestra **pistas de botones**, no botones clicables.
- Los métodos `Continuar()`, `Reiniciar()`, `VolverAlMenu()` de `PauseManager` son
  públicos: si más adelante se habilita un rig de puntero, pueden cablearse a
  `Button.onClick` sin tocar código.
