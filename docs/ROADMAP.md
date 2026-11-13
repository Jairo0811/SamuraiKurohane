# SAMURAI KUROHANE — Plan maestro de producción

**Versión:** 1.0 · **Fecha:** 2026-11-10
**Estado del proyecto al redactarlo:** PROTOTIPO (ver [`AUDIT.md`](AUDIT.md))
**Decisión creativa final:** Jairo Matías, JM Game Studios

---

## 1. Supuestos del plan

Todas las cifras de este documento dependen de estos supuestos. **Si alguno cambia, las
estimaciones cambian.** Se declaran explícitamente para que puedan discutirse.

| # | Supuesto | Si cambia |
|---|---|---|
| S1 | **Un desarrollador** (Jairo) con asistencia de IA para código, documentación, andamiaje y revisión | Con un segundo programador, las fases 1–3 se acortan ~35% |
| S2 | **No hay artista 3D, animador, compositor ni equipo de QA dedicados** | Es el factor que más cambia el plan: ver §5 |
| S3 | Ritmo sostenido de **20 h/semana** (media jornada real, no picos) | A 40 h/semana, todos los plazos se reducen a la mitad |
| S4 | La IA asiste pero **no sustituye** decisiones creativas ni validación en el Editor | — |
| S5 | Plataforma única: **Windows x64** | Cada plataforma adicional añade 4–8 semanas-persona |
| S6 | Sin multijugador, sin servicios online, sin microtransacciones | — |
| S7 | Audio y arte **originales o con licencia**, sin assets de terceros protegidos | Usar paquetes con licencia reduce el arte de entorno ~30% |
| S8 | Estimaciones en **semanas-persona a jornada completa** (40 h) | Para calendario a 20 h/semana: multiplicar por 2 |

**Estas estimaciones no son un compromiso de fecha.** No se fija ninguna fecha de
lanzamiento en este documento, por regla explícita del proyecto.

---

## 2. El problema de alcance, dicho claramente

El alcance descrito en el prompt maestro —9 regiones, 7 clanes con jefe propio, 14
personajes con nombre modelados y animados, 5 actos, ~10 cinemáticas, sistemas completos de
progresión, mapa, misiones, diálogos, guardado, localización y audio— es el alcance de un
**estudio pequeño de 4 a 8 personas durante 2 a 3 años**.

Estimación del alcance completo tal y como está escrito:

| Fase | Semanas-persona |
|---|---|
| 1 · Fundamentos jugables | 6 |
| 2 · Vertical slice | 31 |
| 3 · Sistemas completos | 18 |
| 4 · Producción del mundo | 34 |
| 5 · Campaña narrativa | 86 |
| 6 · Alpha | 6 |
| 7 · Beta | 12 |
| 8 · Release candidate | 5 |
| 9 · Publicación | 4 |
| **Total** | **≈ 202** |

**202 semanas-persona ≈ 4 años a jornada completa para una sola persona**, o alrededor de
8 años al ritmo del supuesto S3.

El 60% de ese coste no es programación: es **arte de personajes y animación**. Solo el set
de animaciones de Akio que pide la sección 15 (24 estados, con transiciones, prioridades y
sincronización de hitboxes) son 8–12 semanas para alguien que no es animador especialista.
Los 13 personajes con nombre restantes son otras ~39 semanas.

Tu propia sección 26 dice: *"Cuando una característica comprometa el proyecto, propone una
alternativa de menor costo sin destruir su identidad"* y *"Nunca recortes la historia o
elimines personajes canónicos silenciosamente"*. Las tres opciones siguientes respetan
ambas reglas: **no se elimina ni un personaje, ni un clan, ni un acto de la historia.** Lo
que cambia es el coste de producción de cada uno.

---

## 3. Tres alcances propuestos

### Opción A — Alcance completo literal

Todo lo descrito, a la máxima fidelidad: 9 regiones completas, 14 personajes con modelo y
rig propios, 11 encuentros de jefe únicos, 10 cinemáticas completas.

- **Coste:** ≈ 202 semanas-persona
- **Viabilidad en solitario:** baja. Requiere contratación o un equipo.
- **Riesgo principal:** abandono por agotamiento antes de llegar a la Fase 5.

### Opción B — Full Game v1 (**recomendada como objetivo del juego completo**)

Misma historia, mismos personajes canónicos, mismos 7 clanes, mismo final. Se reduce el
**coste de producción**, no el contenido narrativo:

| Decisión | Qué conserva | Qué ahorra |
|---|---|---|
| **5 regiones completas + 4 fortalezas de clan** como subregiones | Los 7 clanes siguen teniendo territorio reconocible (tu sección 10 ya lo permite: *"regiones, subregiones o fortalezas"*) | ≈ 14 sem |
| **Malla base samurái modular** reskinada para los 7 Daimyo, Ren, Raizen y Daigo; silueta, armadura, color y arma únicos por personaje | Los 14 personajes existen, son distinguibles y tienen identidad visual | ≈ 22 sem |
| **Set de animación compartido** + 6–10 movimientos únicos por jefe | Cada jefe se siente distinto donde importa: su moveset | ≈ 18 sem |
| **3 duelos con Ren** como un encuentro con 3 fases/variantes evolutivas | Los tres enfrentamientos de la historia se mantienen | ≈ 5 sem |
| **6 cinemáticas completas + 4 escenas dialogadas en motor** | Los 10 momentos narrativos se cuentan todos | ≈ 5 sem |
| Audio: 8–10 pistas con variaciones en lugar de banda sonora completa | Identidad sonora íntegra | ≈ 6 sem |

- **Coste:** ≈ **115–125 semanas-persona** (≈ 2,5 años a jornada completa)
- **Viabilidad en solitario:** media. Alta si se externaliza el arte de personajes.

### Opción C — MVP: «Acto I completo» (**recomendada como primer objetivo real**)

Un juego pequeño pero **terminado y publicable**: el Acto I íntegro, de principio a fin.

Contenido: Kurohane Sanctuary + Imperial District (KAGE) completos · Akio con set de
animación de producción · Hana como NPC · un jefe (primer duelo con Ren) · santuarios,
guardado, mapa, UI final, audio, 2 cinemáticas (La Noche de las Plumas Negras y El
despertar) · 3 familias de enemigos · las habilidades Kirisame y Tsukiyomi · 2–3 horas de
juego.

- **Coste:** ≈ **45–50 semanas-persona**
- **Por qué es el objetivo correcto primero:** produce algo que se puede **enseñar,
  publicar como demo o Early Access, y usar para validar o financiar el resto**. Y todo lo
  que se construye en él —sistemas, pipeline, herramientas— se reutiliza íntegro en la
  Opción B. No es trabajo desechado.

### Recomendación

**Objetivo inmediato: Opción C. Objetivo declarado del proyecto: Opción B.**

Esto satisface tu requisito de no conformarse con una demo técnica —la Opción C es un juego
terminado, no una demo— mientras protege la viabilidad. La decisión es tuya.

---

## 4. Presupuesto orientativo

**Advertencia:** cifras orientativas basadas en rangos de mercado generales para
externalización de desarrollo independiente. **No son cotizaciones de ningún proveedor
real** y no se ha consultado a ninguno. Sirven para decidir qué conviene externalizar, no
para comprometer gasto.

Escenario: Opción C (MVP), con el código y el diseño en casa.

| Partida | Rango orientativo (USD) | Nota |
|---|---|---|
| Modelado + rig de Akio (calidad de producción) | 1.500 – 4.000 | La partida de mayor impacto por dólar |
| Set de animación de Akio (24 estados) | 2.000 – 5.000 | Alternativa: librería de animaciones + ajuste propio, 300 – 800 |
| Kit de arte de entorno (2 regiones) | 1.500 – 4.000 | Reducible usando el generador procedural actual como base |
| Música (4–6 pistas originales) | 800 – 2.500 | |
| Diseño de sonido (biblioteca + implementación) | 300 – 1.200 | Bibliotecas con licencia comercial |
| Licencia Unity | 0 | Unity Personal es gratuito bajo el umbral de ingresos vigente; **verificar el umbral actual antes de publicar** |
| Tasa de publicación en tienda PC | ~100 por producto | Verificar importe vigente |
| QA externo (pase de compatibilidad) | 0 – 1.500 | Opcional; sustituible por pruebas propias |
| **Total MVP** | **≈ 6.200 – 18.300** | |

Si todo se hace en casa, el coste monetario del MVP tiende a **~100 USD** (tasa de tienda)
y el coste real es **tiempo**: las ~45–50 semanas-persona de §3.

---

## 5. El riesgo número uno, y qué hacer con él

**Arte y animación de personajes es el 60% del coste y el 100% del cuello de botella.**

Opciones, de menor a mayor coste:

1. **Mantener el rig procedural actual más tiempo del previsto.** Ya existe, ya funciona y
   da una silueta legible. Permite terminar *todo* el diseño de combate, niveles y jefes
   antes de gastar un euro en arte. **Recomendado para las fases 1–3.**
2. **Malla base modular propia en Blender**, reskinada por personaje. Coste de aprendizaje
   alto una vez, coste marginal bajo después.
3. **Externalizar Akio y la malla base**, hacer el resto en casa. Mejor relación
   coste/impacto si hay presupuesto.
4. **Externalizar todo el arte de personajes.** Solo con financiación.

### Otros riesgos

| Riesgo | Impacto | Mitigación |
|---|---|---|
| El prototipo no compila o se ve mal en Unity (nunca se ha verificado) | Alto | **Cerrar en el paso 0.** Es el primer trabajo pendiente |
| `main` sin el código del juego, historias no relacionadas | Alto | Fusión autorizada (AUDIT §2.1) |
| Generador procedural no escala a 9 regiones | Alto | Separar layout de dressing en Fase 2 |
| `com.unity.pipeline 0.7.0-exp.1` es experimental | Medio | Decidir uso consciente o retirarlo antes de depender de él |
| Sin pruebas automatizadas, cada cambio puede romper combate | Medio | Ensamblados + pruebas en Fase 1 |
| Blender MCP no operativo | Bajo | Abrir Blender con el addon; mientras, trabajo en Unity |
| Alcance creciente durante la producción | Alto | Este documento. Toda ampliación pasa por revisión de alcance |
| Agotamiento del desarrollador único | Alto | Opción C: un objetivo alcanzable y publicable |

---

## 6. Fases, entregables y criterios de aceptación

Ninguna fase se declara completa sin cumplir **todos** sus criterios. Los criterios están
redactados para ser verificables, no interpretables.

### Paso 0 — Verificación del prototipo (prerrequisito, ~1 semana)

No es una fase del plan: es la deuda de no haber abierto nunca el proyecto en el Editor.

**Criterios de aceptación**
- [ ] El proyecto compila en Unity 6000.6.1f1 sin errores
- [ ] `ImperialDistrict_Prototype` entra en Play Mode sin excepciones en consola
- [ ] Existe al menos una captura y una grabación corta del juego en marcha
- [ ] Se ha confirmado o corregido el post-procesado URP, los materiales de partículas y la lluvia
- [ ] Las plataformas son alcanzables con el salto actual
- [ ] Los ángulos del rig procedural leen correctamente de perfil
- [ ] `main` contiene el código del juego y el README

### Fase 1 — Fundamentos jugables (6 sem)

**Entregables**
- Definiciones de ensamblado (`JMGameStudios.SamuraiKurohane.Runtime`, `.Editor`, `.Tests`)
- Ensamblado de pruebas con cobertura real de la lógica de combate y postura
- Capa de entrada sobre `InputSystem_Actions.inputactions`, con remapeo
- Datos de balance de combate y movimiento en ScriptableObjects
- Bootstrap de juego y flujo de escenas mínimo
- Limpieza de restos de plantilla de Unity
- Ajuste de movimiento y combate a partir de pruebas en mano

**Criterios de aceptación**
- [ ] `dotnet` / Unity Test Runner ejecuta la suite y **pasa**, con el resultado adjuntado
- [ ] Ninguna tecla está escrita a mano en la lógica de gameplay
- [ ] El remapeo de controles funciona y persiste
- [ ] Un valor de balance se puede cambiar sin abrir la escena
- [ ] El prototipo sigue siendo jugable tras el refactor (verificado en Play Mode)
- [ ] `Assets/TutorialInfo`, `Readme.asset`, `SampleScene` y `HubForceResolve` eliminados

### Fase 2 — Vertical slice (31 sem)

**Entregables**
- Arquitectura de datos de nivel: **layout editable** separado del **dressing procedural**
- Herramienta de editor para construir e inspeccionar layouts
- Una región pequeña y pulida, diseñada (no generada por coordenadas literales)
- Akio con presentación visual de producción
- Primer jefe completo, con arena propia
- Santuarios Kurohane con guardado funcional
- UI real: HUD, pausa, menú principal
- Primer pase de audio
- Primera cinemática
- Build de validación en Windows

**Criterios de aceptación**
- [ ] Un jugador ajeno al proyecto completa la región sin instrucciones verbales
- [ ] Guardar, cerrar el juego, reabrir y continuar restaura el estado correctamente
- [ ] El jefe es derrotable y su derrota tiene consecuencia persistente
- [ ] El `.exe` se instala y ejecuta en una máquina Windows limpia
- [ ] Rendimiento medido y documentado sobre hardware de referencia definido
- [ ] Ningún placeholder crítico en la región del slice

### Fase 3 — Sistemas completos (18 sem)

**Entregables:** progresión y habilidades · Karmic Threads con función jugable demostrable
en combate y exploración · mapa persistente · diálogos · misiones · inventario · guardado
completo y versionado · framework de enemigos escalable con 3 familias nuevas ·
localización ES/EN operativa.

**Criterios de aceptación**
- [ ] Cada habilidad desbloquea acceso a una zona previamente inalcanzable (verificado en el grafo)
- [ ] El grafo de conectividad del mundo se valida automáticamente: sin bloqueos de progresión
- [ ] Cambiar el idioma en opciones cambia todo el texto del juego, sin cadenas rígidas
- [ ] Un guardado de la versión anterior se migra sin pérdida de progreso
- [ ] Una familia de enemigos nueva se añade sin tocar el código del jugador

### Fase 4 — Producción del mundo (34 sem · 20 en Opción B)

**Entregables:** las regiones restantes · level design completo · arte de entorno por clan ·
secretos, atajos, ascensores, viaje rápido · los 7 clanes representados.

**Criterios de aceptación**
- [ ] El mapa completo es recorrible de principio a fin
- [ ] Toda progresión obligatoria es alcanzable (validación automática del grafo)
- [ ] Cada clan tiene territorio visualmente distinguible
- [ ] Rendimiento dentro del objetivo en todas las regiones

### Fase 5 — Campaña narrativa (86 sem · 40 en Opción B)

**Entregables:** los 5 actos implementados · el epílogo · todos los jefes · las cinemáticas ·
los diálogos · las misiones principales.

**Criterios de aceptación**
- [ ] La campaña se completa de principio a fin en una partida
- [ ] El final implementado es el canónico: Akio entrega a Daigo a la justicia
- [ ] Todos los personajes canónicos aparecen con su rol narrativo
- [ ] Ningún hilo narrativo principal queda sin cerrar

### Fase 6 — Alpha (6 sem)

**Criterio único y suficiente:** el juego **se puede terminar de principio a fin**, aunque
haya assets temporales, errores y pulido pendiente.

### Fase 7 — Beta (12 sem)

Arte y animación consolidados · audio · UI final · optimización medida · corrección de bugs
· compatibilidad · accesibilidad · balance.

**Criterios de aceptación**
- [ ] Sin bugs de prioridad crítica o alta abiertos
- [ ] Objetivos de rendimiento alcanzados y medidos, no estimados
- [ ] Opciones de accesibilidad de la sección 27 implementadas o justificadamente descartadas
- [ ] Sin placeholders críticos

### Fase 8 — Release candidate (5 sem)

Validación de contenido · pruebas de regresión · builds · instalación · documentación ·
créditos · **auditoría de licencias de todo asset, fuente, sonido y librería**.

### Fase 9 — Publicación y soporte (4 sem)

Ficha de tienda · material promocional · trailer · capturas · publicación **cuando el
proyecto esté listo, sin fecha comprometida de antemano** · corrección posterior.

---

## 7. Orden de trabajo inmediato

| # | Trabajo | Depende de | Estado |
|---|---|---|---|
| 0.1 | Fusionar la rama a `main` conservando el README | **Tu autorización** | Pendiente |
| 0.2 | Compilar y verificar en Unity, con capturas | 0.1 | Pendiente |
| 0.3 | Corregir lo que 0.2 revele | 0.2 | Pendiente |
| 1.1 | Ensamblados + suite de pruebas | 0.3 | Pendiente |
| 1.2 | Capa de entrada con remapeo | 1.1 | Pendiente |
| 1.3 | Balance a ScriptableObjects | 1.1 | Pendiente |
| 1.4 | Limpieza de plantilla | 0.1 | Pendiente |
| 1.5 | Bootstrap y flujo de escenas | 1.1 | Pendiente |

El backlog detallado vive en [`BACKLOG.md`](BACKLOG.md).

---

## 8. Registro de decisiones canónicas

Toda decisión creativa o técnica aprobada se registra aquí, con fecha, para que ninguna
sesión futura la contradiga.

| Fecha | Decisión | Autor | Estado |
|---|---|---|---|
| 2026-11-10 | El rig procedural de personajes se mantiene como representación oficial durante las fases 1–3; el arte de producción llega en Fase 2 para Akio y en Fase 5 para el resto | Propuesta | **Pendiente de aprobación** |
| 2026-11-10 | Objetivo inmediato Opción C (MVP Acto I); objetivo declarado del juego completo Opción B | Propuesta | **Pendiente de aprobación** |
| 2026-11-10 | Los 7 clanes se representan con 5 regiones completas + 4 fortalezas de clan | Propuesta | **Pendiente de aprobación** |
| 2026-11-10 | Unity 6000.6.1f1, URP 17.6.0, Input System 1.20.0 son las versiones fijadas del proyecto | Verificado en repositorio | Vigente |
| 2026-11-10 | Plataforma inicial única: Windows x64 | Prompt maestro | Vigente |
| 2026-11-10 | Final canónico: Akio entrega a Daigo a la justicia, no lo ejecuta | Prompt maestro | Vigente |
