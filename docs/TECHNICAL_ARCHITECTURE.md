# SAMURAI KUROHANE — Arquitectura técnica objetivo

**Versión:** 1.0 · **Fecha:** 2026-11-10
**Estado actual:** ver [`AUDIT.md`](AUDIT.md) · **Plan:** ver [`ROADMAP.md`](ROADMAP.md)

---

## 1. Principio rector

El prototipo actual funciona. **Esta arquitectura es un destino, no una reescritura.**

Regla de migración: ningún sistema existente se reescribe desde cero. Se extrae su lógica,
se le pone una prueba, y se mueve. Si un refactor no puede demostrarse con una prueba que
pase antes y después, no se hace.

Lo que hoy existe y debe sobrevivir intacto en su comportamiento:

- El *feel* del movimiento de Akio: buffer de entrada, coyote time, salto variable, dash.
- El contrato de combate: telegraph → respuesta → castigo, con postura y parry.
- El feedback de impacto: hit-stop, sacudida, chispas, arcos de corte.
- La vestidura procedural del entorno: materiales, atmósfera, parallax, iluminación.

---

## 2. Problema actual, en una frase

Toda la lógica vive en `MonoBehaviour` dentro de un único ensamblado implícito, sin capa de
datos, sin capa de entrada y sin frontera testeable. Eso es correcto para un prototipo y es
el techo del proyecto a partir de aquí.

| Síntoma medido en la auditoría | Causa |
|---|---|
| Cero pruebas automatizadas posibles | Sin `.asmdef`, el Test Framework no puede aislar un ensamblado |
| Remapeo de controles imposible | Teclas escritas a mano en la lógica de gameplay |
| Balance no ajustable sin tocar la escena | ~60 valores como `[SerializeField]` en componentes |
| `AkioPrototypeController` con 976 líneas | Movimiento, combate, defensa, daño y estado en una clase |

---

## 3. Ensamblados

```
Assets/SamuraiKurohane/
├── Core/          JMGameStudios.SamuraiKurohane.Core        (C# puro, sin UnityEngine)
├── Runtime/       JMGameStudios.SamuraiKurohane.Runtime     (gameplay, MonoBehaviour)
├── Editor/        JMGameStudios.SamuraiKurohane.Editor      (herramientas; solo Editor)
└── Tests/
    ├── EditMode/  JMGameStudios.SamuraiKurohane.Tests.EditMode
    └── PlayMode/  JMGameStudios.SamuraiKurohane.Tests.PlayMode
```

**`Core` es la decisión más importante de este documento.** Es un ensamblado de C# puro, sin
referencia a `UnityEngine`, que contiene las máquinas de estado y las reglas que se pueden
probar sin abrir el Editor:

- Resolución de postura y guard break
- Ventanas de parry, contraataque y cancelación
- Progresión del combo y temporizadores
- Buffer de entrada y coyote time
- Cálculo de daño con multiplicadores
- Grafo de conectividad del mundo y validación de progresión
- Migración de versiones de guardado

Ventaja concreta: esas pruebas se ejecutan en segundos desde la línea de comandos, sin
licencia de Unity ni Editor abierto. Es lo que hace alcanzable la sección 23 del prompt
maestro.

`Runtime` conecta `Core` a Unity: lee física, mueve `Rigidbody`, dispara VFX, habla con el
rig. No contiene reglas; las consulta.

---

## 4. Módulos y responsabilidades

| Módulo | Responsabilidad | Hoy |
|---|---|---|
| `Bootstrap` | Arranque, orden de inicialización, carga de la primera escena | No existe |
| `Input` | Traduce `InputSystem_Actions` a intenciones de juego; remapeo; esquemas de control | **No existe** (teclas a mano) |
| `Player.Movement` | Locomoción, salto, dash | En `AkioPrototypeController` |
| `Player.Combat` | Combos, ataques, hitboxes | En `AkioPrototypeController` |
| `Player.Defense` | Guardia, parry, contraataque | En `AkioPrototypeController` |
| `Vitals` | Vida y postura, compartido entre jugador y enemigos | Duplicado en jugador y enemigo |
| `Abilities` | Técnicas Kirisame, Karmic Threads, desbloqueos | No existe |
| `Enemy.AI` | Comportamiento, telegraphs, percepción | En `PrototypeCombatTarget` |
| `Boss.AI` | Fases, patrones, mecánica distintiva | No existe |
| `Animation` | Puente a rig procedural y, después, a Animator | `SamuraiVisualRig` |
| `Camera` | Seguimiento, look-ahead, shake, zoom | `PrototypeCameraRig` |
| `World` | Datos de nivel, layout, regiones, grafo de conectividad | **No existe** |
| `Scenes` | Transiciones, carga, puntos de entrada | No existe |
| `Progression` | Habilidades, mejoras, estado del mundo | No existe |
| `Save` | Serialización versionada, migración | No existe |
| `Quests` / `Dialogue` | Misiones y conversaciones | No existe |
| `Cinematics` | Timeline, control de cámara cinematográfica | No existe |
| `Audio` | Mezclador, música reactiva, SFX | No existe |
| `Localization` | Tablas de cadenas ES/EN | No existe |
| `UI` | HUD, menús, mapa, inventario | Solo `OnGUI` de depuración |
| `Settings` | Opciones, accesibilidad, persistencia | No existe |

### Regla de dependencias

```
Core  ←  Runtime  ←  Editor
                  ←  Tests
```

`Core` no depende de nada. `Runtime` depende de `Core`. **Nada depende de `UI`.** Sin
dependencias circulares: cualquier ciclo detectado es un error de arquitectura, no un
detalle.

---

## 5. Datos: ScriptableObjects

Hoy no existe ninguno. Objetivo:

| Asset | Contenido | Sustituye a |
|---|---|---|
| `MovementProfile` | Velocidad, aceleración, gravedades, salto, coyote, buffer | 13 `[SerializeField]` del controlador |
| `CombatProfile` | Rangos, daños, ventanas, multiplicador de contraataque | ~20 campos |
| `VitalsProfile` | Vida, postura, regeneración, retardos | 4 campos ×2 entidades |
| `EnemyArchetype` | Perfil completo de una familia de enemigos | Campos de `PrototypeCombatTarget` |
| `AttackDefinition` | Telegraph, duración, daño, bloqueable, daño a postura | Estructuras internas |
| `AbilityDefinition` | Técnica, coste, efecto en combate, efecto en exploración | No existe |
| `RegionDefinition` | Layout, conexiones, puertas por habilidad | No existe |

Beneficio inmediato: el ajuste de balance deja de requerir abrir una escena, y una familia de
enemigos nueva es un asset, no una clase.

---

## 6. Capa de entrada

Hoy: `Keyboard.current.jKey.wasPressedThisFrame` dentro de la lógica de combate.

Objetivo: `InputSystem_Actions.inputactions` (que ya existe en el proyecto y nadie usa) como
única fuente, detrás de una fachada que expone **intenciones**:

```
IPlayerInput
  float  MoveAxis
  bool   JumpPressed / JumpHeld
  bool   DashPressed
  bool   LightPressed / HeavyPressed
  bool   ParryPressed
  bool   GuardHeld
```

Esto habilita de golpe: remapeo persistente (secciones 18 y 27), iconos según el mando
conectado, entrada simulada en pruebas de PlayMode, y una sola puerta de entrada para
menús, diálogos y cinemáticas.

El buffer de entrada y el coyote time **se mueven a `Core`** y se prueban: son lógica pura
sobre el tiempo.

---

## 7. Datos de nivel: layout contra dressing

El hallazgo más importante de la auditoría para el futuro del proyecto.

**Hoy:** el nivel es código. `new Ledge(-26.5f, 5.0f, 0.8f, 0.6f)`. No es editable
visualmente, no es inspeccionable y no se puede validar.

**Objetivo:**

| Capa | Qué contiene | Quién la toca |
|---|---|---|
| **Layout** | Colisión, plataformas, puertas, transiciones, puntos de spawn, puntos de habilidad. Datos autorizados y versionados | Level designer, en el Editor |
| **Dressing** | Materiales, atmósfera, linternas, parallax, props, partículas. **El generador actual** | Generado, con semilla |

El generador procedural existente se conserva íntegro y pasa a aplicarse **sobre** un layout
en lugar de definirlo. Se gana lo que falta —niveles diseñables y validables— sin perder lo
que ya funciona: que vestir una región cueste minutos y no semanas.

Sobre el layout se construye el **grafo de conectividad**, que vive en `Core` y permite
responder automáticamente, en cada build: *¿es alcanzable toda la progresión obligatoria con
las habilidades disponibles en ese punto?* Esa prueba es la defensa del proyecto contra los
bloqueos de progresión, que en un metroidvania son el bug más caro de encontrar a mano.

---

## 8. Guardado

Requisitos del prompt maestro: versionado, preparado para migraciones, sin pérdida ni
corrupción, resistente a cierre inesperado, con definición explícita de qué se conserva al
morir.

| Decisión | Propuesta |
|---|---|
| Formato | JSON con número de versión de esquema en la raíz |
| Escritura | A archivo temporal y renombrado atómico: un cierre inesperado nunca deja un guardado a medias |
| Copias | El guardado anterior se conserva como respaldo |
| Migración | Cadena de migradores `v1→v2→v3`, en `Core`, con prueba por cada salto |
| Momento de guardado | Santuarios Kurohane, y autoguardado al cambiar de región |
| Al morir | Se conservan: progreso narrativo, jefes derrotados, habilidades, mejoras, atajos abiertos, mapa descubierto. **`[ABIERTO]`** si se pierde algún recurso consumible |

---

## 9. Rendimiento

El prompt pide métricas realistas, medidas, no estimadas. **Hoy no hay ninguna medición.**

**`[PROPUESTA]`** Hardware de referencia y objetivos a fijar en la Fase 2, cuando exista una
región de producción que medir:

| Perfil | Objetivo propuesto |
|---|---|
| Mínimo | 1080p / 60 fps estables |
| Recomendado | 1440p / 60 fps estables |

Puntos de atención ya identificados en el código actual: el generador crea muchos
`GameObject` sueltos (candidatos a *batching* estático), las luces puntuales de legibilidad
se colocan cada 8 unidades a lo largo de 76 (coste acumulable), y el HUD usa `OnGUI`, que
asigna memoria cada frame y debe desaparecer con la UI real.

Ninguna optimización antes de medir.

---

## 10. Orden de migración

Cada paso deja el juego funcionando. Ninguno es un «gran refactor».

| # | Paso | Entregable verificable |
|---|---|---|
| 1 | Crear los 5 ensamblados, mover los scripts actuales a `Runtime` sin cambiarlos | El proyecto compila; el prototipo sigue jugable |
| 2 | Crear `Core` y mover buffer de entrada y coyote time, con pruebas | Pruebas que pasan, ejecutadas desde consola |
| 3 | Extraer postura y vida a `Vitals` en `Core`, compartido jugador/enemigo | Elimina duplicación; pruebas de guard break |
| 4 | Extraer ventanas de parry y contraataque a `Core` | Pruebas de la ventana de parry |
| 5 | Capa de entrada sobre `InputSystem_Actions` | Remapeo funcionando y persistente |
| 6 | Balance a ScriptableObjects | Ajustar un valor sin abrir la escena |
| 7 | `Bootstrap` y flujo de escenas | Menú → juego → menú |
| 8 | Arquitectura de layout + grafo de conectividad | Una región cargada desde datos, con validación automática |

Pasos 1 a 7: Fase 1 del roadmap. Paso 8: inicio de la Fase 2.

---

## 11. Decisiones técnicas pendientes

- **`[ABIERTO]`** Mantener o retirar `com.unity.pipeline 0.7.0-exp.1` (paquete experimental).
- **`[ABIERTO]`** Retirar `com.unity.visualscripting` si el proyecto es solo C#.
- **`[ABIERTO]`** UI con **uGUI** (ya instalado, más material de referencia) o **UI Toolkit**
  (más moderno, mejor para menús complejos). Decisión necesaria antes de la Fase 2.
- **`[ABIERTO]`** Animación: mantener el rig procedural como sistema definitivo para enemigos
  secundarios, o pasar todo a Animator con clips de Blender.
- **`[ABIERTO]`** Middleware de audio: FMOD, Wwise o el mezclador nativo de Unity. El prompt
  pide una arquitectura que permita integrarlos después; eso implica una fachada de audio
  propia desde el principio.
- **`[ABIERTO]`** CI: GitHub Actions puede ejecutar las pruebas de `Core` con `dotnet` sin
  licencia de Unity. Los builds de Unity requieren licencia y runner propio; **no se asume
  que funcionen** sin verificarlo.

---

## 12. Lo que esta arquitectura NO hace

Para evitar sobreingeniería, se descartan explícitamente, salvo que un problema real los
justifique: inyección de dependencias por framework, ECS/DOTS, patrón de comandos para la
entrada, event bus global, y cualquier librería externa sin justificación de necesidad y
licencia. El prompt maestro lo pide así y es la decisión correcta para un equipo de una
persona.
