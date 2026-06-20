# 001 — Menú principal y navegación · spec

**Estado:** ✅ Implementado (RF-01) · 🟡 Parcial (RF-02)
**Requisitos:** RF-01 (Pantalla de inicio), RF-02 (Música de menú)

## Objetivo

Ofrecer la primera interfaz que ve el usuario al colocarse el visor: un punto de entrada claro y
minimalista que permita **empezar a jugar** o **salir** con el mínimo esfuerzo de interacción,
estableciendo desde el inicio la estética neón y el tono inmersivo de la experiencia.

## Porqué

Es la puerta de entrada a la experiencia y el primer contacto con los principios de diseño: un panel
flotante curvo, botones grandes accesibles con el puntero del controlador y movimiento mínimo del
usuario (P3, P4, P6). Reduce la fricción inicial para alcanzar rápido el estado de *flow*.

## Historias de usuario

- **HU-01.1** — Como jugadora (Valeria), al ponerme el visor quiero ver de inmediato una pantalla de
  bienvenida con la opción de jugar, para entrar a la experiencia sin instrucciones previas.
- **HU-01.2** — Como jugadora quiero poder salir de la aplicación desde el menú, para cerrar la
  sesión cuando termine.
- **HU-01.3** — Como jugadora quiero escuchar una pista ambiental mientras navego el menú, para
  sentirme inmersa antes de empezar a jugar.

## Criterios de aceptación

### RF-01 — Pantalla de inicio ✅
- **Dado** que la aplicación inicia, **cuando** carga `MenuPrincipal`, **entonces** se muestra un
  panel con las acciones disponibles (jugar / salir).
- **Cuando** el usuario activa "Jugar", **entonces** se carga la escena de selección de canciones
  (`MenuSongs`).
- **Cuando** el usuario activa "Salir", **entonces** la aplicación se cierra (`Application.Quit()`).

### RF-02 — Música de menú 🟡
- **Dado** que el usuario está en un menú, **cuando** navega, **entonces** suena una pista ambiental.
- **Cuando** el usuario pulsa un botón, **entonces** se reproduce un *click* de confirmación
  (auditivo) — *implementado vía `AudioManager.buttonClickSound`*.
- *Pendiente:* asegurar que la pista ambiental de menú esté cableada y suene de forma continua.

## Fuera de alcance

- Configuración de opciones de juego (volumen, dificultad global): existe un `panelOpciones` previsto
  en `MenuManager` pero su contenido no es parte de esta entrega.
- Persistencia de preferencias del usuario.

## Dependencias

- `AudioManager` (singleton) debe existir antes de reproducir audio de menú.
- La navegación depende de los nombres de escena exactos (`MenuSongs`, `GameScene`).
