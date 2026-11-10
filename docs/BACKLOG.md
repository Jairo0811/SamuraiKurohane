# SAMURAI KUROHANE — Backlog priorizado

**Versión:** 1.0 · **Fecha:** 2026-11-10

Prioridades: **P0** bloqueante · **P1** siguiente bloque · **P2** fase siguiente · **P3** más adelante.
Estimaciones en días-persona a jornada completa. Ver supuestos en [`ROADMAP.md`](ROADMAP.md) §1.

---

## P0 — Bloqueantes

| ID | Trabajo | Est. | Depende de | Estado |
|---|---|---|---|---|
| SK-001 | **Autorizar y ejecutar la fusión de `feature/imperial-district-prototype` a `main`** con `--allow-unrelated-histories`, conservando `README.md` | 0,5 | **Decisión del director** | Pendiente |
| SK-002 | Abrir el proyecto en Unity 6000.6.1f1, compilar y registrar todos los errores de consola | 0,5 | SK-001 | Pendiente |
| SK-003 | Entrar en Play Mode y verificar: plataformas alcanzables, ángulos del rig, lluvia, partículas, post-procesado URP | 1 | SK-002 | Pendiente |
| SK-004 | Capturar imagen y grabación corta del juego funcionando; adjuntarlas al repositorio | 0,5 | SK-003 | Pendiente |
| SK-005 | Corregir lo que revelen SK-002 a SK-004 | 1–5 | SK-003 | Pendiente |
| SK-006 | Decidir alcance objetivo: Opción A, B o C del roadmap | — | **Decisión del director** | Pendiente |

> Hasta cerrar SK-002/003/004, **ninguna afirmación sobre el funcionamiento del prototipo
> está verificada**. Todo el plan posterior asume que funciona.

---

## P1 — Fase 1: Fundamentos jugables

### Infraestructura

| ID | Trabajo | Est. | Depende de |
|---|---|---|---|
| SK-010 | Crear los 5 ensamblados (`Core`, `Runtime`, `Editor`, `Tests.EditMode`, `Tests.PlayMode`) y mover los scripts actuales sin modificarlos | 2 | SK-005 |
| SK-011 | Ensamblado `Core` de C# puro + primera prueba que pase, ejecutable desde consola | 2 | SK-010 |
| SK-012 | Mover buffer de entrada y coyote time a `Core` con pruebas | 2 | SK-011 |
| SK-013 | Extraer `Vitals` (vida + postura) a `Core`, compartido jugador/enemigo; eliminar duplicación | 3 | SK-011 |
| SK-014 | Extraer ventanas de parry, contraataque y cancelación a `Core` con pruebas | 3 | SK-013 |
| SK-015 | Script de validación en consola que ejecute la suite de `Core` sin Unity | 1 | SK-011 |
| SK-016 | Limpieza: eliminar `Assets/TutorialInfo`, `Readme.asset`, `SampleScene`, `HubForceResolve`; corregir el puntero LFS roto de `URP.png` | 1 | SK-001 |
| SK-017 | Decidir y aplicar: retirar `com.unity.visualscripting`; decidir sobre `com.unity.pipeline 0.7.0-exp.1` | 0,5 | — |

### Entrada

| ID | Trabajo | Est. | Depende de |
|---|---|---|---|
| SK-020 | Fachada `IPlayerInput` sobre `InputSystem_Actions.inputactions` | 3 | SK-010 |
| SK-021 | Sustituir todas las lecturas directas de teclado/mando en la lógica de gameplay | 2 | SK-020 |
| SK-022 | Remapeo de controles con persistencia | 3 | SK-021 |
| SK-023 | Detección de dispositivo e iconos de botón según mando conectado | 2 | SK-021 |

### Datos

| ID | Trabajo | Est. | Depende de |
|---|---|---|---|
| SK-030 | `MovementProfile`, `CombatProfile`, `VitalsProfile` como ScriptableObjects | 3 | SK-013 |
| SK-031 | `EnemyArchetype` + `AttackDefinition`; migrar el guardia imperial a datos | 3 | SK-030 |

### Flujo y pulido

| ID | Trabajo | Est. | Depende de |
|---|---|---|---|
| SK-040 | `Bootstrap` y flujo de escenas mínimo (arranque → juego) | 3 | SK-010 |
| SK-041 | Ajuste de movimiento y combate a partir de pruebas en mano | 5 | SK-003 |
| SK-042 | Revisión de legibilidad: separar el carmesí del jugador del carmesí del entorno | 2 | SK-003 |

**Total Fase 1 ≈ 41 días-persona (≈ 6 semanas a jornada completa).**

---

## P2 — Fase 2: Vertical slice

| ID | Trabajo | Est. |
|---|---|---|
| SK-100 | Arquitectura de datos de nivel: `RegionDefinition`, layout separado del dressing | 8 |
| SK-101 | Herramienta de editor para construir e inspeccionar layouts | 7 |
| SK-102 | Adaptar el generador procedural para aplicarse **sobre** un layout | 5 |
| SK-103 | Grafo de conectividad en `Core` + validación automática de progresión | 5 |
| SK-110 | Sistema de guardado versionado con escritura atómica y migradores | 8 |
| SK-111 | Santuarios Kurohane: descanso, guardado, punto de reaparición | 4 |
| SK-120 | Decisión uGUI contra UI Toolkit, y esqueleto de UI | 2 |
| SK-121 | HUD de producción: vida, postura, medidores de habilidad | 6 |
| SK-122 | Menú principal con la identidad visual definida | 5 |
| SK-123 | Menú de pausa, opciones, pantalla de muerte | 4 |
| SK-130 | Diseño del primer jefe (primer duelo con Ren) + arena | 6 |
| SK-131 | Implementación del primer jefe con fases y ventanas de castigo | 9 |
| SK-140 | Fachada de audio + primer pase de SFX | 5 |
| SK-141 | Primera pista musical integrada y reactiva | 4 |
| SK-150 | Primera cinemática con Timeline (El despertar de Akio) | 7 |
| SK-160 | Diseño de la primera región completa | 15 |
| SK-170 | Akio: concept art de producción aprobado | 5 |
| SK-171 | Akio: modelo 3D y rig en Blender | 20 |
| SK-172 | Akio: set de animación (24 estados) | 40 |
| SK-180 | Medición de rendimiento sobre hardware de referencia | 2 |
| SK-181 | Build de validación en Windows + prueba en máquina limpia | 3 |

**Total Fase 2 ≈ 170 días-persona (≈ 31 semanas).** De ellos, **65 son arte y animación de
Akio** (SK-170 a SK-172): la partida candidata a externalizarse.

---

## P3 — Fases 3 a 9

Se detallarán cuando la Fase 2 esté cerrada, para no planificar sobre supuestos que la
Fase 2 va a cambiar. Resumen de alcance y estimación en [`ROADMAP.md`](ROADMAP.md) §6.

| Bloque | Est. (semanas-persona) |
|---|---|
| Fase 3 · Sistemas completos | 18 |
| Fase 4 · Producción del mundo | 34 (20 en Opción B) |
| Fase 5 · Campaña narrativa | 86 (40 en Opción B) |
| Fase 6 · Alpha | 6 |
| Fase 7 · Beta | 12 |
| Fase 8 · Release candidate | 5 |
| Fase 9 · Publicación | 4 |

---

## Decisiones del director pendientes

Bloquean planificación, no solo trabajo.

| # | Decisión | Bloquea |
|---|---|---|
| D1 | Autorizar la fusión a `main` | Todo |
| D2 | Alcance objetivo: A, B o C | Fases 4 y 5 |
| D3 | Mantener el rig procedural durante las fases 1–3 | Prioridad de SK-170/171/172 |
| D4 | uGUI o UI Toolkit | SK-120 |
| D5 | Las salvaguardas de Isamu: qué son y cómo se activan | Diseño del Acto V |
| D6 | Reparto de posturas de los siete clanes ante 2474 | Tono de cada región, diseño de jefes |
| D7 | Si Ren sobrevive y participa en el Acto V | Acto IV y V |
| D8 | Externalizar o no el arte de personajes | Presupuesto y calendario de la Fase 2 |

Las decisiones narrativas abiertas están listadas al completo en
[`NARRATIVE_BIBLE.md`](NARRATIVE_BIBLE.md) §10.

---

## Registro de trabajo completado

| Fecha | ID | Trabajo | Verificación |
|---|---|---|---|
| 2026-11-10 | — | Auditoría integral del repositorio | [`AUDIT.md`](AUDIT.md) |
| 2026-11-10 | — | Añadidos 10 archivos `.meta` de Unity ausentes | 13 `.meta` para 13 archivos |
| 2026-11-10 | — | Escena del prototipo añadida a Build Settings como escena 0 | `EditorBuildSettings.asset` |
| 2026-11-10 | — | Plan maestro, arquitectura objetivo, biblia narrativa y de personajes | `docs/` |
| 2026-11-03 | — | Mejora del prototipo: rig procedural, feel de combate, IA enemiga, entorno ampliado | Commit `2b6062d`; **sin verificar en Unity** |
