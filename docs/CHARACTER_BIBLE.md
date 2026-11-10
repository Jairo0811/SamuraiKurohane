# SAMURAI KUROHANE — Biblia de personajes

**Versión:** 1.0 · **Fecha:** 2026-11-10
**Director creativo:** Jairo Matías · **Desarrollador:** JM Game Studios

Etiquetas: **`[CANON]`** aprobado, no modificable sin autorización · **`[PROPUESTA]`**
derivado, pendiente de revisión · **`[ABIERTO]`** decisión no tomada.

---

## 0. Identidad visual común

### 0.1 Paleta del proyecto

**`[CANON]`** Negro · carbón · carmesí · acero · blanco · naranja controlado.

**`[PROPUESTA]`** Reparto funcional de la paleta, para que el color signifique algo y no
sea decoración:

| Color | Función | Dónde |
|---|---|---|
| Negro / carbón | Base de todo lo Kurohane y de la silueta del jugador | Armadura de Akio, metal ennegrecido, madera oscura |
| **Carmesí** | Energía, legado Kurohane, peligro del jugador | Filo de Kirisame, emblema, neón del mundo, luna |
| Acero | Autoridad imperial, tecnología del Shogunato | Armaduras imperiales, Shirogane, arquitectura oficial |
| Blanco | Daigo y el orden absoluto | Armadura ceremonial de Daigo |
| **Naranja controlado** | Luz habitable: linternas, ventanas, fuego | Faroles, shoji, brasas |
| Oro discreto | Jerarquía, solo en los más altos cargos | Detalles de Daigo y Raizen |

**`[PROPUESTA]`** Regla de legibilidad: **el carmesí del jugador y el carmesí del entorno no
deben competir.** En arenas de combate el entorno baja a naranja y acero, y el carmesí queda
reservado a Akio, a su katana y a los telegraphs enemigos. Esto ya es un problema real en el
prototipo actual, donde el neón carmesí del entorno es muy presente.

### 0.2 Emblema del Clan Kurohane

**`[CANON]`** Representa **alas o plumas negras**, con estética heráldica japonesa
futurista. **Prohibido el emblema triangular** de referencias conceptuales antiguas.

**`[PROPUESTA]`** Especificación para que el emblema sea reproducible de forma consistente
por cualquiera que trabaje en el proyecto:

- Una sola pluma estilizada, o un par de alas simétricas, inscrita en un **mon** circular
  (el formato heráldico japonés tradicional).
- Silueta legible en negativo a 32 × 32 px: debe funcionar como icono de UI, grabado en
  armadura, estandarte y marca de agua.
- Carmesí sobre negro, o negro sobre carmesí. Sin degradados en la versión canónica.
- Sin texto integrado.

**`[ABIERTO]`** Elección entre pluma única y par de alas. Decisión del director creativo.
Hasta que se tome, el prototipo usa una pluma de tres trazos como provisional, no como
canon.

---

## 1. AKIO KUROHANE — protagonista jugable

### Canon narrativo

**`[CANON]`** Último superviviente conocido del Clan Kurohane. Heredero del clan. Samurái
disciplinado, reservado y experto en combate con katana. Su cuerpo posee reconstrucciones
tecnológicas realizadas tras los acontecimientos de 2474. Fue derrotado por Ren Takemori
durante la Noche de las Plumas Negras y declarado muerto.

**`[CANON]`** Arco: comienza buscando venganza y termina comprendiendo que la justicia no
consiste en repetir la violencia que destruyó a su familia.

**`[CANON]`** **No puede ser sustituido como protagonista.**

### Diseño visual obligatorio

**`[CANON]`** Todos estos elementos son obligatorios y **no se modifican sin autorización**:

- Kasa japonés amplio y oscuro
- Armadura samurái negra de tecnología avanzada
- Placas de acero ennegrecido
- Ropa negra y gris carbón
- Capa larga deteriorada
- Detalles luminosos carmesí
- Katana **Kirisame**
- Emblema alado del Clan Kurohane
- Silueta reconocible a distancia
- Anatomía humana realista

**`[CANON]`** Complementos documentados en el README del proyecto, coherentes con lo
anterior: cuero y telas envejecidas, iconografía del clan, katana con energía roja
controlada, silueta clara para gameplay lateral.

### Especificación de silueta para gameplay 2.5D

**`[PROPUESTA]`** La vista lateral impone requisitos que un turnaround de concept art no
captura. Para que Akio sea legible en movimiento:

| Elemento | Requisito |
|---|---|
| Kasa | Ala ancha y asimétrica de perfil: es el rasgo que lo identifica a un vistazo |
| Capa | Debe moverse. Es el principal indicador de velocidad y de dirección del dash |
| Katana | Visible envainada en reposo y claramente trazada en ataque; el filo carmesí marca el arco del golpe |
| Hombros | Placas que no oculten la cabeza de perfil |
| Contraste | El personaje debe leerse como **masa oscura con acentos carmesí** sobre fondos oscuros: la luz de contorno es obligatoria, no decorativa |

**`[PROPUESTA]`** Lo que ya existe en el rig procedural (`SamuraiVisualRig.cs`) y debe
conservarse al pasar a arte de producción: proporciones, altura de cámara, posición del
kasa, longitud de la capa, punto de origen del arco de la katana. Son valores ya ajustados
a la cámara del juego.

### Estado de producción

| Activo | Estado |
|---|---|
| Rig procedural jugable | **Existe** (`SamuraiVisualRig.cs`, arquetipo `Akio`) |
| Concept art de producción aprobado | **No existe** |
| Modelo 3D | No existe |
| Rig y skinning | No existe |
| Set de animación (24 estados) | No existe |

**`[ABIERTO]`** Edad de Akio en 2474 y 2486; si envejeció en suspensión.
**`[ABIERTO]`** Alcance visible de las reconstrucciones tecnológicas: ¿se ven? ¿tienen coste
narrativo o jugable?
**`[ABIERTO]`** Si lleva el rostro cubierto, parcialmente visible o descubierto bajo el kasa.

---

## 2. KIRISAME — la katana

**`[CANON]`** Arma principal y única de Akio. **No se sustituye por un arsenal de armas
inconexas.** Puede recibir mejoras y desbloquear técnicas. Tiene funciones originales que se
revelan en el Acto III.

**`[CANON]`** Las **Karmic Threads** son conexiones tecnológicas, **no magia**. Kirisame
puede detectar e interrumpir conexiones entre sistemas energéticos y mecanismos: abrir rutas,
interrumpir defensas, desactivar maquinaria, activar plataformas, alterar mecanismos, crear
oportunidades en combates de jefe, descubrir accesos secretos.

**`[PROPUESTA]`** Lectura visual: el filo tiene energía carmesí **controlada**, no un aura
constante. Se intensifica en el impacto, el parry exitoso y las técnicas. Un brillo
permanente arruinaría el contraste y el valor del momento de golpe.

**`[PROPUESTA]`** Por qué Kirisame puede interactuar con Tenshō: Isamu participó en la
arquitectura de la red y construyó en la katana del clan la llave de sus salvaguardas. Esto
une arma, padre, legado y clímax en una sola idea, y justifica que el arma sea única e
insustituible.

**`[ABIERTO]`** Confirmar esa conexión entre Kirisame y las salvaguardas de Isamu. Es la
decisión narrativa pendiente más importante (ver `NARRATIVE_BIBLE.md` §10.1).

### Técnicas canónicas

**`[CANON]`**

| Técnica | Función |
|---|---|
| **Kirisame** | Conjunto de técnicas ofensivas de espada |
| **Tsukiyomi** | Movilidad avanzada, reposicionamiento, desplazamientos rápidos |
| **Kurenai** | Ataques carmesí de alto impacto |
| **Crimson Counter** | Contraataque tras defensa precisa |
| **Falling Wing** | Corte aéreo descendente |
| **Silent Draw** | Desenvainado veloz |
| **Moon Sever** | Técnica avanzada de ruptura |
| **Void Cut** | Atravesar distancias y determinadas barreras |

**`[CANON]`** Cada habilidad debe tener utilidad en combate **y** en exploración cuando sea
razonable. **No se implementan habilidades sin función jugable demostrable.**

**`[PROPUESTA]`** Doble función de cada técnica, para cumplir la regla anterior y la
estructura metroidvania:

| Técnica | En combate | En exploración |
|---|---|---|
| Tsukiyomi | Reposicionamiento, esquiva extendida | Salvar huecos amplios; acceso horizontal |
| Kurenai | Daño alto a postura | Romper barreras frágiles marcadas |
| Crimson Counter | Castigo tras parry | — (puramente de combate) |
| Falling Wing | Apertura aérea | Descenso controlado; romper suelos |
| Silent Draw | Apertura instantánea desde envainado | Cortar mecanismos a distancia |
| Moon Sever | Ruptura de guardia de jefe | Abrir puertas selladas |
| Void Cut | Atravesar al enemigo | **Atravesar barreras**: la habilidad de progresión clave |

**`[PROPUESTA]`** Orden de desbloqueo propuesto: Kirisame (inicio) → Tsukiyomi (Acto I) →
Crimson Counter (Acto I) → Kurenai (Acto II) → Falling Wing (Acto II) → Silent Draw (Acto
III) → Moon Sever (Acto IV) → Void Cut (Acto IV). **`[ABIERTO]`** validar.

---

## 3. DAIGO SUMERAGI — antagonista principal

**`[CANON]`** Canciller Supremo del Neo-Shogunato. Antiguo colaborador de Isamu Kurohane.
Responsable intelectual de la conspiración y del exterminio del clan. Estratega, político y
espadachín extraordinario. Desea eliminar la autonomía militar de los clanes y controlar
Japón mediante Tenshō. Arma: **Shirogane**.

**`[CANON]`** Diseño: armadura ceremonial **negra, blanca y detalles dorados discretos**,
cabello gris, apariencia impecable.

**`[CANON]`** Antagonista **humano y complejo**, no una caricatura malvada. **No se
convierte en monstruo ni en IA.** No se sustituye sin autorización.

**`[CANON]`** Jefe final. Su combate debe expresar el conflicto entre orden absoluto y
libertad.

**`[PROPUESTA]`** Su lógica interna, para que no sea un villano por maldad: Daigo vivió las
guerras entre clanes y concluyó que la autonomía militar de siete ejércitos es la causa
estructural de la violencia. Centralizar el mando bajo una sola autoridad es, para él, el
único final posible de ese ciclo. Los Kurohane no eran sus enemigos: eran el único clano
capaz de impedirlo, e Isamu —su colaborador y amigo— construyó deliberadamente las
salvaguardas que lo bloquean. Su crimen no nace del odio, sino de la convicción de que el
coste era aceptable.

**`[PROPUESTA]`** Contraste visual con Akio, deliberado y total: blanco contra negro,
impecable contra deteriorado, Shirogane (plata) contra Kirisame (lluvia), orden contra
legado. El combate final debe leerse como esa oposición antes de que empiece el diálogo.

**`[ABIERTO]`** Qué le ocurre tras ser entregado a la justicia: juicio público, consecuencias
políticas, destino final.
**`[ABIERTO]`** Si llegó a apreciar a Isamu de verdad, y si lo admite en el duelo final.

---

## 4. REN TAKEMORI — rival recurrente

**`[CANON]`** Hermano menor del Shogun. Comandante imperial. Derrotó a Akio durante la Noche
de las Plumas Negras. Su historia explora el conflicto entre lealtad familiar y justicia.

**`[CANON]`** **Tres enfrentamientos importantes** con Akio, con evolución de movimientos,
técnicas, diálogos y motivaciones.

**`[PROPUESTA]`** Progresión de los tres duelos: ver `NARRATIVE_BIBLE.md` §7.

**`[PROPUESTA]`** Diseño: armadura samurái imperial de élite en acero y blanco roto, sin la
pompa ceremonial de Daigo. Es un soldado, no un político. Silueta más pesada que Akio:
donde Akio es velocidad, Ren es presencia.

**`[ABIERTO]`** Si sobrevive al juego y participa en el Acto V.
**`[ABIERTO]`** Su arma y su nombre, si lo tiene.
**`[ABIERTO]`** Qué sabía exactamente en 2474.

---

## 5. RAIZEN TAKEMORI — Shogun de Yamato

**`[CANON]`** Shogun. Autorizó la operación militar contra los Kurohane usando pruebas
manipuladas. Aunque fue engañado, **continúa siendo responsable de sus decisiones**.
Representa el conflicto entre autoridad, deber y responsabilidad.

**`[PROPUESTA]`** El juego no lo absuelve ni lo demoniza. En el Acto IV, al conocer la
verdad, su reacción no es negarla: es asumir que firmó la orden. Esa aceptación —no su
inocencia— es lo que permite la alianza provisional contra Daigo.

**`[ABIERTO]`** Si hay combate contra Raizen o solo confrontación narrativa.
**`[ABIERTO]`** Su destino en el epílogo: abdicación, juicio, permanencia en el cargo.

---

## 6. HANA TSUKISHIRO

**`[CANON]`** Historiadora tecnológica e ingeniera. Investiga la destrucción del Clan
Kurohane y los documentos eliminados de los archivos oficiales. **Encuentra y reactiva el
santuario donde Akio permanece en suspensión.** Ayuda a Akio a reconstruir su pasado y a
descubrir el verdadero funcionamiento de Tenshō.

**`[CANON]`** Tiene **personalidad, motivaciones, decisiones y desarrollo propios**. No es
una función de apoyo.

**`[PROPUESTA]`** Para cumplir esa regla, Hana necesita tres cosas que todavía no existen:
un motivo personal propio (no «ayudar al protagonista»), al menos una decisión en la que
discrepe de Akio, y algo que pueda perder. Los tres están abiertos.

**`[PROPUESTA]`** Rol jugable: no acompañante de combate. Es la voz que interpreta los
archivos que Akio recupera, y el motivo por el que explorar tiene recompensa narrativa.
Opera desde los santuarios.

**`[ABIERTO]`** Su motivación personal. ¿Por qué investiga a los Kurohane y qué arriesga?
**`[ABIERTO]`** Si tiene relación previa con el clan o con alguien de 2474.
**`[ABIERTO]`** En qué discrepa de Akio, y en qué acto ocurre.
**`[ABIERTO]`** Su diseño visual. No hay ninguna especificación canónica todavía.

---

## 7. ISAMU KUROHANE — padre de Akio

**`[CANON]`** Antiguo líder del Clan Kurohane. Participó en la arquitectura inicial de
Tenshō y **desarrolló mecanismos para impedir su utilización como herramienta de dominación
absoluta**. Murió durante la destrucción del clan. Su historia se descubre mediante
recuerdos, documentos y acontecimientos del pasado.

**`[CANON]`** Autor del mensaje: *«No vengues nuestro nombre. Descubre por qué quisieron
borrarlo»*.

**`[PROPUESTA]`** Nunca aparece en presente. Solo en recuerdos, grabaciones y documentos, y
su voz es la única autoridad moral del juego que Akio no puede discutir. Esa ausencia es
deliberada: el arco de Akio consiste en obedecer esa frase en lugar de vengarlo.

**`[ABIERTO]`** Si existe un recuerdo jugable de 2474 o antes.
**`[ABIERTO]`** Naturaleza exacta de los mecanismos que creó (ver `NARRATIVE_BIBLE.md` §10.1).

---

## 8. NAO KUROHANE — madre de Akio

**`[CANON]`** Figura importante de la infancia de Akio y del legado moral del clan. **Murió
protegiendo la evacuación de civiles durante la masacre.** Su influencia debe estar presente
en el desarrollo emocional del protagonista.

**`[PROPUESTA]`** Si Isamu representa el deber y la estrategia, Nao representa por qué el
deber importa: eligió salvar a desconocidos antes que a su clan. Es el precedente directo de
la decisión final de Akio sobre Daigo. Esa simetría debe quedar explícita en el epílogo.

**`[ABIERTO]`** Cómo se transmite su presencia: santuarios, objetos, recuerdos.

---

## 9. Los siete Daimyo

**`[CANON]`** Cada uno es jefe principal de su clan. **No todos son malvados ni mueren al
ser derrotados.** Cada jefe debe representar un conflicto narrativo y una prueba de las
capacidades del jugador.

| Daimyo | Clan | Especialidad `[CANON]` | Postura ante 2474 `[PROPUESTA]` |
|---|---|---|---|
| **Ayame Mizuchi** | Mizuchi | Agua, navegación, hidráulica, naginata | Arrepentida |
| **Masanori Tetsuda** | Tetsuryū | Industria pesada, exoesqueletos | Cómplice |
| **Sayaka Tsukikage** | Tsukikage | Espionaje, infiltración, camuflaje, precisión | Arrepentida |
| **Kenshin Hoshin** | Hoshin | Aeroespacial, fortalezas suspendidas | Cómplice |
| **Goro Arakawa** | Raijin | Armamento eléctrico, velocidad, combate frontal | Convencido |
| **Jubei Enma** | Enma | Fuerzas de asalto, armamento avanzado | Convencido |
| **Ren Takemori** | Takemori | Combate samurái imperial de élite | Atrapado |

**`[ABIERTO]`** Validar el reparto de posturas. Define quién muere, quién se rinde y quién
se alía.
**`[ABIERTO]`** Diseño visual, arma, moveset y mecánica distintiva de cada uno. Siete fichas
pendientes.
**`[ABIERTO]`** Qué secreto guarda cada uno sobre 2474.

**`[PROPUESTA]`** Para contener el coste de producción (ver `ROADMAP.md` §3, Opción B): los
siete comparten una **malla base samurái modular**, y se diferencian por silueta de armadura,
color de clan, arma y moveset. Lo que los hace memorables es cómo pelean, no cuántos
polígonos únicos tienen.

---

## 10. Familias de enemigos

**`[CANON]`** Diseñar familias relacionadas con cada clan: guardias imperiales, samuráis de
élite, shinobi tecnológicos, soldados con exoesqueleto, arqueros de energía, lanceros, drones
defensivos, unidades de patrulla, especialistas en ruptura de postura, guardianes de
fortalezas.

**`[CANON]`** Cada enemigo debe documentar: rol, silueta, ataques, telegraphs, vida, postura,
movilidad, rango, debilidades, estados, animaciones, recompensas, comportamientos y
condiciones de derrota.

### Estado de producción

| Enemigo | Estado |
|---|---|
| Guardia imperial | **Existe** (`PrototypeCombatTarget.cs` + rig `ImperialGuard`): 3 ataques telegrafiados, postura, ruptura, respawn |
| Todas las demás familias | No existen |

**`[PROPUESTA]`** El guardia imperial actual es la plantilla correcta: su patrón
telegraph → respuesta → castigo es el contrato de combate del juego. Las demás familias
deben variar **una** dimensión de ese contrato cada una (rango, velocidad, verticalidad,
presión de postura), no añadir sistemas nuevos.

**`[ABIERTO]`** Ficha completa de cada familia. Se redactarán en `BOSS_DESIGN.md` y en el
documento de enemigos cuando la Fase 3 las requiera.

---

## 11. Estado global de producción de personajes

| Personaje | Canon narrativo | Especificación visual | Activo jugable |
|---|---|---|---|
| Akio Kurohane | Completo | **Completo y obligatorio** | Rig procedural |
| Kirisame | Completo | Parcial | Rig procedural |
| Daigo Sumeragi | Completo | Completo | No existe |
| Ren Takemori | Completo | Abierto | No existe |
| Raizen Takemori | Completo | Abierto | No existe |
| Hana Tsukishiro | Completo | **Abierto** | No existe |
| Isamu Kurohane | Completo | Abierto | No existe |
| Nao Kurohane | Completo | Abierto | No existe |
| 7 Daimyo | Nombre + especialidad | **Abierto** | No existe |
| Guardia imperial | — | Parcial | **Existe** |

**Prioridad de producción, según `ROADMAP.md`:** Akio primero y solo Akio hasta terminar la
Fase 2. Todo lo demás espera.
