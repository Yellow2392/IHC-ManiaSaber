# Mania Saber — Documentación Spec-Driven

Juego de ritmo en Realidad Virtual (híbrido entre **Beat Saber** y **osu!mania**) desarrollado en
Unity con el Meta XR SDK para **Meta Quest 2**.

> Curso: Interacción Humano-Computador (UTEC, 2026-1) · Profesor: Teófilo Chambilla Aquino
> Equipo: Gonzalo Suárez · Jorge Tenorio · Yuri Escobar · Luis Maquera

## Qué es esta carpeta

Esta carpeta documenta el **avance real** del proyecto siguiendo el enfoque de
*Spec-Driven Development* (SDD). En lugar de describir la implementación con prosa suelta, cada
capacidad del sistema se describe como una **especificación versionada** que vive junto al código,
de modo que la documentación y la implementación evolucionan en el mismo repositorio.

El punto de partida es la **planificación previa** (informe LaTeX entregado en semana 10), de la que
se heredan los requisitos funcionales (RF), no funcionales (RNF), la persona, las métricas HEART y
los principios de diseño. Aquí esos requisitos se traducen a especificaciones ejecutables y se
contrastan con lo que ya está construido en el código.

## Estructura

La documentación tiene dos capas: **fundamentos** (transversales) y **especificaciones** (una por
capacidad). No se incluyen documentos de plan ni de tareas: el *cómo* técnico vive en `overview.md` y
el estado de avance en `progress.md`.

### Fundamentos

| Documento | Contenido |
|---|---|
| [`constitution.md`](constitution.md) | Principios no negociables del producto y de ingeniería. Toda spec debe respetarlos. |
| [`overview.md`](overview.md) | Arquitectura: escenas, flujo de navegación, mapa de scripts, singletons y pipeline de datos. |
| [`progress.md`](progress.md) | Matriz de estado RF/RNF (✅/🟡/⬜), avance por semana y deuda técnica. |

### Especificaciones

Una carpeta `00X-<feature>/` por capacidad, cada una con un único `spec.md`.

### Features

| # | Feature | Requisitos | Estado |
|---|---|---|---|
| [001](001-menu-principal/spec.md) | Menú principal y navegación | RF-01, RF-02 | ✅ Implementado |
| [002](002-seleccion-canciones/spec.md) | Selección de canciones | RF-03, RF-04 | ✅ Implementado |
| [003](003-generacion-cubos/spec.md) | Generación rítmica de cubos | RF-05, RF-06, RF-07 | ✅ Implementado |
| [004](004-sables-puntuacion/spec.md) | Sables, impacto y puntuación | RF-08, RF-09, RF-10, RF-11 | 🟡 Parcial |
| [005](005-ciclo-partida-resultados/spec.md) | Ciclo de partida y resultados | RF-12, RF-13, RF-14, RF-15 | 🟡 Parcial |

## Convención de cada feature

Cada `spec.md` describe el *qué* y el *porqué*: objetivo, requisitos cubiertos (RF), historias de
usuario, criterios de aceptación (estilo *Dado/Cuando/Entonces*), alcance y dependencias. El detalle
técnico (*cómo*) y el estado se consultan en los fundamentos:

- **Arquitectura y mapeo a código** → [`overview.md`](overview.md).
- **Estado por requisito y deuda técnica** → [`progress.md`](progress.md).

## Leyenda de estado

- ✅ **Implementado** — cubierto en el código y verificable en la build.
- 🟡 **Parcial** — base funcional presente; faltan sub-requisitos.
- ⬜ **Pendiente** — planificado, aún no implementado.
