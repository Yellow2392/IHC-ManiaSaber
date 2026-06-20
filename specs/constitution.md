# Constitución — Mania Saber

Principios **no negociables** que rigen el diseño y la implementación. Cualquier `spec.md` o
`plan.md` debe ser coherente con esta constitución; si una decisión la contradice, se documenta
explícitamente como excepción justificada en el `plan.md` correspondiente.

Estos principios derivan directamente de la planificación previa (marco teórico de IHC, principios
de diseño de interacción, heurísticas de usabilidad y requisitos no funcionales).

---

## P1 — Reducción de la carga cognitiva

La diferenciación frente a Beat Saber es **eliminar la direccionalidad estricta del corte**. Los
cubos son omnidireccionales: cualquier ángulo de impacto es válido. El reto recae en la **precisión
rítmica (temporal)**, nunca en la interpretación visual de flechas o íconos en movimiento.

- No se introducen indicadores que exijan lectura en VR durante el *gameplay*.
- La información en pantalla durante la partida se limita a lo imprescindible (puntaje, progreso).

## P2 — Interacción multimodal y feedback inmediato

Todo impacto exitoso debe confirmarse por **múltiples canales sensoriales sincronizados**: visual
(estallido/corte), auditivo (efecto de impacto) y **háptico** (vibración del controlador). El
feedback es el principal mecanismo de *visibilidad del estado del sistema*.

## P3 — Mapping físico 1:1 y entrada por controladores

Relación natural y directa entre el movimiento del brazo del usuario y el del sable. Se usan
**controladores físicos** (no *hand tracking*) por su precisión, baja latencia y capacidad háptica.
Los 4 carriles se mapean a posiciones naturalmente alcanzables frente al jugador.

## P4 — Confort primero: mitigación del *motion sickness*

- Experiencia **de pie y estática**: el usuario no se desplaza; el entorno (las notas) fluye hacia él.
- Sin locomoción artificial.
- Activos importantes en la **zona óptima de interacción** (entre 0.5 m y 1.0 m, y entre 0° y 30°
  bajo el horizonte) para evitar fatiga de cuello y brazos.

## P5 — Rendimiento como requisito de presencia

El rendimiento no es un "extra": una caída de FPS o un exceso de latencia rompe la presencia y
provoca malestar. Son límites duros del producto:

| Atributo | Umbral | Origen |
|---|---|---|
| Tasa de refresco | ≥ 72 FPS estables | RNF-01 |
| Latencia movimiento→render | ≤ 20 ms | RNF-02 |
| Sincronía audio↔cubo | ± 10 ms respecto al beat | RNF-03 |
| Respuesta háptica | ≤ 16 ms tras impacto | RNF-04 |
| Curva de aprendizaje | < 30 s para entender la mecánica | RNF-05 |
| Transición entre pantallas | < 3 s | RNF-06 |
| Plataforma objetivo | Meta Quest 2 | RNF-07 |

## P6 — Minimalismo estético dirigido a la atención

Estética *cyberpunk*/neón con alto contraste sobre fondo oscuro **al servicio de la función**: el
contraste y el movimiento guían la atención visual sin necesidad de texto invasivo. Interfaces
principales sin sobrecarga de opciones.

## P7 — Heurísticas de usabilidad aplicadas

1. **Visibilidad del estado** — puntaje y progreso siempre disponibles en la visión periférica superior.
2. **Relación con el mundo real** — "golpear" objetos con sables y sus sonidos emulan la física real.
3. **Control y libertad** — el usuario elige su canción y el ángulo de corte que prefiera.
4. **Consistencia y estándares** — convenciones de juegos de ritmo + mapeo estándar de mandos Quest.
5. **Diseño estético y minimalista** — inmersión sin saturar las interfaces.

---

## Restricciones técnicas (stack fijado)

- **Motor:** Unity **2022.3.62f3** (LTS).
- **Lenguaje:** C#.
- **XR:** Meta XR SDK / Oculus Integration (OVR), Meta XR Interaction SDK.
- **Plataforma:** Meta Quest 2 (Android / OpenXR).
- **Formato de canciones:** beatmaps de **osu!mania** empaquetados como `.zip` (estilo `.osz`),
  ubicados en `Assets/Resources/MusicFiles/ZipFiles/`. El sistema lee `.osu`, audio (`.mp3`/`.ogg`)
  y portada sin depender de un editor de mapas propio.

## Convenciones de ingeniería

- **Idioma del dominio:** el código y la documentación usan español (nombres de clases, campos y
  comentarios). Mantener esta consistencia.
- **Robustez ante datos externos:** el parseo de `.osu`/`.zip` nunca debe lanzar excepciones al
  llamador; ante un campo ausente o corrupto se usa un valor de respaldo y se registra un *warning*
  (ver `OsuZipReader`).
- **Singletons de servicios:** `AudioManager` persiste entre escenas (`DontDestroyOnLoad`);
  `ScoreManager` es por-escena. Documentar cualquier nuevo singleton en [`overview.md`](overview.md).
- **Trazabilidad:** cada feature mapea sus requisitos (RF/RNF) en su `spec.md` y su estado en
  [`progress.md`](progress.md).
