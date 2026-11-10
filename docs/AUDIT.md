# SAMURAI KUROHANE — Auditoría integral del proyecto

**Fecha:** 2026-11-10
**Commit auditado:** `2b6062d` (rama `feature/imperial-district-prototype`)
**Comparado con:** `fa97ba9` (rama `main`)
**Auditor:** asistencia técnica (Claude) bajo dirección de Jairo Matías

> **Cómo leer este documento.** Todo lo marcado como *verificado* se comprobó leyendo
> los archivos del repositorio o ejecutando una herramienta, y el método queda indicado.
> Todo lo marcado como *no verificado* no se pudo comprobar en este entorno y no debe
> darse por bueno. **El proyecto no se ha abierto ni ejecutado en el Editor de Unity
> durante esta auditoría.**

---

## 1. Resumen ejecutivo

El proyecto está en **prototipo técnico temprano**, no en preproducción avanzada.

Lo que existe es sólido en una sola dirección: un prototipo de movimiento y combate
razonablemente profundo, con un generador de entorno procedural, todo en ~5.300 líneas
de C#. Lo que no existe es casi toda la infraestructura que convierte un prototipo en un
videojuego: no hay sistema de guardado, ni flujo de escenas, ni interfaz de usuario real,
ni audio, ni localización, ni una sola prueba automatizada, ni definiciones de ensamblado.

Hay además **tres defectos de repositorio que deben resolverse antes de seguir
produciendo contenido**, porque cada día que pasa aumentan su coste:

1. `main` y `feature/imperial-district-prototype` **no comparten historia**. Son dos
   árboles independientes. Todo el código del juego vive fuera de `main`.
2. Faltaban 10 archivos `.meta` de Unity en el control de versiones. (Corregido en este
   bloque de trabajo.)
3. La escena del prototipo no estaba en Build Settings. (Corregido en este bloque.)

**Clasificación honesta del estado actual: PROTOTIPO.** No es un vertical slice, no es
alpha. La distinción importa porque la planificación posterior depende de ella.

---

## 2. Estado del repositorio

### 2.1 Topología de ramas — hallazgo crítico

*Verificado con `git merge-base`, `git log` y `git diff --name-status`.*

```
main      fab0edb ── 5b529e7 ── 7c58000 ── fa97ba9    (README + plantilla Unity vacía)

feature/  (raíz propia) ── f6691f2 ── 2b6062d          (TODO el código del juego)
imperial-district-prototype
```

`git merge-base HEAD origin/main` **no devuelve nada**: las dos ramas no tienen ancestro
común. Consecuencias concretas:

| Hecho verificado | Consecuencia |
|---|---|
| `main` contiene 76 archivos y **ningún script de gameplay** | La rama principal del proyecto no contiene el juego |
| La rama de trabajo contiene 99 archivos pero **no contiene `README.md`** | Un merge normal a `main` **borraría el README** (`git diff` lo marca como `D README.md`) |
| No hay ancestro común | `git merge` falla sin `--allow-unrelated-histories`; un Pull Request en GitHub mostrará el 100% de los archivos como cambiados |

**Origen probable:** el proyecto Unity se subió en un commit inicial independiente
(`Registro inicial`) y después se creó el README sobre `main`, mientras el prototipo se
desarrolló en una rama creada desde un árbol distinto (probablemente un `git init` nuevo
o un push forzado temprano).

**Resolución propuesta (requiere tu autorización, no ejecutada):**

```bash
git checkout main
git merge feature/imperial-district-prototype --allow-unrelated-histories
# resolver el único conflicto real: conservar README.md de main
git checkout --ours README.md && git add README.md
git commit
```

Tras esto, `main` contiene el juego y el README, y todas las ramas futuras parten de una
historia única. **No se ha tocado `main`.** Según tus reglas de trabajo, la fusión a `main`
necesita tu aprobación explícita.

### 2.2 Git LFS

*Verificado leyendo `.gitattributes` y el estado de trabajo.*

`.gitattributes` está correctamente configurado para LFS (modelos, audio, vídeo, imágenes,
fuentes, ejecutables). Esto es adecuado para lo que viene.

**Defecto detectado:** `Assets/TutorialInfo/Icons/URP.png` está registrado como archivo
normal pero su contenido en el repositorio es un **puntero LFS de 130 bytes** en lugar del
PNG de 24 KB. Al clonar sin LFS instalado, el archivo queda corrupto. Es un archivo de la
plantilla de Unity, sin impacto en el juego, pero conviene limpiarlo cuando se eliminen los
restos de plantilla (ver 5.3).

### 2.3 Archivos `.meta` ausentes — corregido

*Verificado comparando cada `.cs` con su `.meta`.*

Faltaban en el control de versiones los `.meta` de: `CombatFeedback.cs`,
`ImperialDistrictPrototype.Architecture.cs`, `.Atmosphere.cs`, `.Characters.cs`,
`PrototypeCameraRig.cs`, `PrototypeHUD.cs`, `PrototypeMaterials.cs`,
`PrototypePostProcessing.cs`, `SamuraiVisualRig.cs` y `README.md`.

Sin ellos, cada máquina que abre el proyecto genera GUIDs distintos, y cualquier
referencia futura a esos scripts desde una escena o un prefab se rompe al cambiar de
equipo. **Creados en este bloque de trabajo con GUIDs nuevos y estables.**

> **Aviso:** si ya habías abierto el proyecto en Unity con estos scripts presentes, Unity
> generó sus propios `.meta` localmente. Al hacer `git pull`, Git avisará de que se
> sobrescribirían archivos sin seguimiento. Borra tus `.meta` locales de esos 10 archivos
> y vuelve a hacer `pull`; los GUIDs del repositorio pasan a ser los oficiales.

---

## 3. Configuración técnica

*Verificado leyendo `ProjectSettings/ProjectVersion.txt` y `Packages/manifest.json`.*

| Elemento | Valor real | Coincide con la documentación |
|---|---|---|
| Unity | `6000.6.1f1` (rev `7efac9f6c10e`) | Sí |
| URP | `17.6.0` | Sí |
| Input System | `1.20.0` | Sí |
| Test Framework | `1.8.0` | Instalado, **sin usar** |
| Timeline | `6.6.0` | Instalado, sin usar |
| AI Navigation | `2.0.14` | Instalado, sin usar |
| uGUI | `2.6.0` | Instalado, sin usar |
| Visual Scripting | `1.9.12` | Instalado, sin usar |
| `com.unity.pipeline` | `0.7.0-exp.1` | **Paquete experimental** |

**Riesgo señalado:** `com.unity.pipeline 0.7.0-exp.1` es experimental. Un paquete `-exp`
puede cambiar su API o desaparecer entre versiones menores de Unity. Hay que decidir si se
usa de forma consciente o si se retira antes de que haya código que dependa de él.

**Recomendación:** retirar `com.unity.visualscripting` si no se va a usar (el proyecto es
C#), para reducir tiempo de compilación y superficie de dependencias.

### 3.1 Escenas

*Verificado leyendo las escenas y `EditorBuildSettings.asset`.*

| Escena | Contenido real |
|---|---|
| `ImperialDistrict_Prototype.unity` | **3 GameObjects**: `Main Camera`, `Crimson Directional Light`, `Imperial District Prototype` |
| `SampleScene.unity` | Plantilla de Unity, sin usar |

Los 3 objetos confirman el enfoque *geometry-first*: el nivel entero se construye en
tiempo de ejecución desde código. Esto tiene una ventaja real (iteración rapidísima sin
trabajo de artista) y un coste que llegará pronto: **ningún nivel autorizado por un level
designer puede construirse así**. Ver 5.1.

**Defecto corregido:** Build Settings solo contenía `SampleScene.unity`. Cualquier build
hecho desde `File > Build` habría compilado la escena vacía de la plantilla. El script
`SamuraiKurohaneWindowsBuilder` pasa la escena del prototipo explícitamente, así que el
build por menú sí funcionaba, pero el de Unity por defecto no. Ahora
`ImperialDistrict_Prototype` es la escena 0 y `SampleScene` queda desactivada.

---

## 4. Inventario de sistemas

*Verificado leyendo los 12 scripts del prototipo (5.325 líneas) y buscando referencias
de API en todo `Assets/`.*

### 4.1 Lo que existe y está implementado

| Sistema | Archivo | Líneas | Estado |
|---|---|---|---|
| Controlador del jugador | `AkioPrototypeController.cs` | 976 | Implementado |
| Rig visual procedural | `SamuraiVisualRig.cs` | 1042 | Implementado |
| IA del guardia imperial | `PrototypeCombatTarget.cs` | 571 | Implementado |
| Generador de entorno | `ImperialDistrictPrototype.*` (4 archivos) | 1308 | Implementado |
| Feedback de combate | `CombatFeedback.cs` | 466 | Implementado |
| Materiales y texturas procedurales | `PrototypeMaterials.cs` | 420 | Implementado |
| Cámara y parallax | `PrototypeCameraRig.cs` | 223 | Implementado |
| HUD de depuración | `PrototypeHUD.cs` | 259 | Implementado (`OnGUI`) |
| Post-procesado URP | `PrototypePostProcessing.cs` | 60 | Implementado |
| Build de Windows | `SamuraiKurohaneWindowsBuilder.cs` | — | Implementado |

**Mecánicas de combate presentes en el código:** combo ligero de 3 golpes con avance y
asistencia de apuntado, ataque pesado, guardia con coste de postura, parry con ventana
corta, ventana de contraataque (×1.6), guard break, dash con invulnerabilidad y cancelación
de recuperación, buffer de entrada (0,15 s), coyote time (0,10 s), salto de altura variable,
hit-stun, hit-stop, respawn por caída.

**IA enemiga presente:** tres ataques telegrafiados (corte bloqueable 0,5 s; mandoble
**no bloqueable** 0,85 s que obliga a esquivar; estocada con avance), encadenado de
seguimiento, postura con regeneración, ruptura de postura, respawn, registro estático de
objetivos para la asistencia de apuntado.

Esto es **mucho más de lo que suele tener un prototipo** y es la base real del proyecto.
No debe reescribirse.

### 4.2 Lo que NO existe

*Verificado por búsqueda de API en todo `Assets/`: ninguna referencia encontrada.*

| Sistema requerido por el diseño | Estado |
|---|---|
| Sistema de guardado | **No existe** (sin `PlayerPrefs`, `JsonUtility` ni serialización) |
| Audio | **No existe** (sin `AudioSource` ni `AudioClip` en todo el proyecto) |
| Localización | **No existe** (textos en cadenas rígidas dentro del HUD) |
| Interfaz de usuario real | **No existe** (solo `OnGUI` de depuración; sin Canvas ni UI Toolkit) |
| Menú principal | No existe |
| Gestión de escenas | No existe (una sola escena) |
| Mapa | No existe |
| Progresión y habilidades | No existe |
| Inventario | No existe |
| Diálogos | No existe |
| Misiones | No existe |
| Cinemáticas | No existe (Timeline instalado, sin usar) |
| Santuarios / checkpoints | No existe |
| Viaje rápido | No existe |
| Jefes | No existe |
| Pruebas automatizadas | **No existe ni una** |
| Definiciones de ensamblado (`.asmdef`) | **No existe ninguna** |

### 4.3 Hallazgo: el Input System está instalado pero no se usa como sistema

*Verificado: `AkioPrototypeController` lee `Keyboard.current`, `Mouse.current` y
`Gamepad.current` directamente; `Assets/InputSystem_Actions.inputactions` existe y está
asignado en `EditorBuildSettings` como asset de configuración, pero **ningún script lo
referencia**.*

Las teclas están escritas a mano en el código (`keyboard.aKey`, `Keyboard.current.jKey`,
`Gamepad.current.buttonWest`…). Funciona, pero:

- **El remapeo de controles es imposible** sin reescribir la capa de entrada. Tu sección 18
  lo pide explícitamente, y la 27 lo pide como requisito de accesibilidad.
- No hay esquemas de control, ni detección de dispositivo, ni iconos de botón dependientes
  del mando conectado.

Esto es deuda técnica contenida y barata de pagar **ahora** (una capa de entrada que
traduzca acciones), y cara de pagar más tarde, cuando haya menús, diálogos y cinemáticas
leyendo entrada por su cuenta.

### 4.4 Hallazgo: todo el gameplay vive en `MonoBehaviour` sin capa de datos

No hay un solo `ScriptableObject` en el proyecto. Los ~60 valores de balance (velocidades,
daños, ventanas de parry, duraciones) son campos `[SerializeField]` dentro de los
componentes.

Consecuencia práctica: **no se puede ajustar el balance sin tocar la escena o el prefab**,
no se puede tener variantes de enemigo compartiendo lógica, y el diseñador de combate no
puede trabajar en paralelo al programador. Tu sección 16 pide ScriptableObjects para datos
de configuración; es el momento correcto de introducirlos, antes de que existan 10 familias
de enemigos.

---

## 5. Impedimentos para llegar al juego completo

Ordenados por coste de postergarlos.

### 5.1 El generador procedural no escala a 9 regiones

El nivel actual se construye por código con coordenadas literales
(`new Ledge(-26.5f, 5.0f, 0.8f, 0.6f)`). Para un metroidvania con mapa persistente,
backtracking, puertas por habilidad y un grafo de conectividad verificable, esto no sirve:
no es editable visualmente, no es inspeccionable por un level designer y no se puede
validar automáticamente.

**No hay que tirarlo.** El valor del generador es la *vestidura*: materiales, atmósfera,
linternas, parallax, props. La resolución es separar **layout** (datos de nivel editables,
autorizados, versionables) de **dressing** (el generador actual, aplicado sobre el layout).

### 5.2 Sin ensamblados no hay pruebas

Unity Test Framework está instalado pero sin `.asmdef` no se puede crear un ensamblado de
pruebas aislado. Y sin pruebas, tu sección 23 (QA) es inalcanzable y tu regla "no presentar
pruebas inexistentes como satisfactorias" obliga a decir, en cada entrega, que nada está
probado automáticamente. Es el primer habilitador de calidad del proyecto.

### 5.3 Restos de plantilla de Unity

`Assets/TutorialInfo/` (Readme.cs, ReadmeEditor.cs, hojas de estilo, iconos),
`Assets/Readme.asset`, `Assets/Scenes/SampleScene.unity` y `Assets/Editor/HubForceResolve.cs`
son restos de la plantilla. Ocupan espacio, aparecen en búsquedas y confunden la estructura.
Eliminarlos es barato ahora y molesto después.

### 5.4 El prototipo no se ha verificado en el Editor

**Limitación real de esta auditoría.** Los scripts se comprobaron sintácticamente con el
compilador de Roslyn fuera de Unity (sin errores propios del proyecto; los únicos errores
restantes son por los tipos de `UnityEngine` que no existen en ese entorno). Pero:

- No se ha compilado dentro de Unity.
- No se ha ejecutado Play Mode.
- No existe ninguna captura ni grabación del juego funcionando.
- El comportamiento visual del rig procedural, los materiales de partículas, la lluvia y el
  post-procesado URP **no está confirmado**.

Las API de URP usadas en `PrototypePostProcessing.cs` (`profile.Add<T>(true)`,
`.Override(...)`, `TonemappingMode.ACES`) y la configuración de materiales transparentes de
partículas son los puntos con más probabilidad de necesitar ajuste.

**Esto es el primer riesgo a cerrar**, porque toda la planificación posterior asume que el
prototipo funciona.

### 5.5 Pipeline de Blender no operativo

*Verificado llamando a la herramienta del servidor MCP de Blender.*

El servidor MCP de Blender **está conectado** a esta sesión (estado `announced`), pero la
llamada falla:

```
Cannot connect to Blender at localhost:9876.
Ensure Blender is running with the MCP addon enabled and the server started.
```

Es decir: el puente existe, pero **Blender no está abierto con el addon escuchando**. Para
habilitarlo: abrir Blender, activar el addon MCP en Preferencias, y pulsar el botón de
iniciar servidor (puerto 9876). Mientras no esté, el pipeline de assets 3D no puede
ejecutarse desde aquí y el trabajo se concentra en Unity.

### 5.6 Ubicación del proyecto en tu equipo: desconocida

Este equipo (`desktop-5qbhcjs`, Windows x64) está vinculado a la sesión, pero **no hay
carpetas conectadas**. Se buscó el proyecto en `~/source/repos`, `~/Documentos/GitHub`,
`~/Documents` y `~/OneDrive` sin encontrarlo.

Saber dónde está permitiría trabajar directamente sobre tu copia y, con control del
escritorio, **abrir Unity y verificar Play Mode de verdad** — que es exactamente lo que
falta (5.4).

---

## 6. Discrepancias entre documentación y código

Tu sección 29 pide identificar contradicciones, investigar su origen y proponer resolución.

| # | Documentación | Código real | Resolución propuesta |
|---|---|---|---|
| 1 | El README de `main` describe el prototipo jugable | `main` **no contiene ningún script de gameplay** | Fusionar la rama a `main` (ver 2.1) |
| 2 | README del prototipo: "enemigo de entrenamiento" (singular) | Hay **3 guardias imperiales** con nombres distintos | Documentación ya actualizada en el commit `2b6062d` |
| 3 | Input System 1.20.0 listado como sistema de entrada | Entrada leída por dispositivo, sin acciones | Introducir capa de entrada (ver 4.3) |
| 4 | "HUD de depuración" | Es un HUD de combate bastante completo, pero en `OnGUI` | Reclasificar: HUD de prototipo, a sustituir por UI real |
| 5 | El README menciona `Build-Windows.ps1` y el menú de build | Ambos existen y son correctos | Sin acción |
| 6 | Sección 23 pide cobertura de pruebas | Cero pruebas, cero ensamblados | Habilitar ensamblados y pruebas (ver 5.2) |

---

## 7. Qué es realmente jugable hoy

Afirmación honesta, con la limitación de 5.4 por delante:

- **Una sola arena**, de 76 unidades de ancho, con 6 plataformas y tres zonas decorativas.
- **Un bucle de combate** contra 3 guardias idénticos en comportamiento.
- **Sin objetivo, sin progresión, sin final.** No se puede ganar ni avanzar.
- **Sin guardado**: cada ejecución empieza igual.
- Al morir, respawn en el punto inicial.

Es decir: **un campo de pruebas de combate y movimiento**, que es exactamente lo que debe
ser un prototipo en esta fase, y que responde a la pregunta correcta ("¿se siente bien
moverse y pelear?"). Pero no es un juego, ni un vertical slice.

---

## 8. Correcciones aplicadas en este bloque de trabajo

| Corrección | Archivos | Verificación |
|---|---|---|
| Añadidos 10 `.meta` ausentes | `Assets/SamuraiKurohane/Prototype/*.meta` | Comprobado: 13 `.meta` para 13 archivos |
| Escena del prototipo añadida a Build Settings como escena 0; `SampleScene` desactivada | `ProjectSettings/EditorBuildSettings.asset` | Comprobado leyendo el archivo |
| Documentación de proyecto creada | `docs/*.md` | Este documento y los adjuntos |

**No se ha tocado `main`. No se ha reescrito ningún sistema existente. No se ha eliminado
ningún archivo.**

---

## 9. Siguiente paso recomendado

En este orden, y con una decisión tuya en el paso 1:

1. **Autorizar la fusión a `main`** (2.1). Hasta que ocurra, todo el trabajo se acumula
   fuera de la rama principal.
2. **Verificar el prototipo en Unity** (5.4): abrir el proyecto, compilar, Play Mode,
   capturar lo que se ve. Si quieres, puedo hacerlo yo mismo en tu equipo con control de
   escritorio, o puedes mandarme la consola y una captura.
3. **Fase 1 del roadmap**: ensamblados + primeras pruebas + capa de entrada + datos de
   balance en ScriptableObjects. Es la infraestructura que todo lo demás necesita.

El plan completo está en [`ROADMAP.md`](ROADMAP.md). La arquitectura objetivo, en
[`TECHNICAL_ARCHITECTURE.md`](TECHNICAL_ARCHITECTURE.md).
