# Especificación General — Clásicos Mente Activa 2D

> **Documento SDD global del producto.**
> Describe la visión, los objetivos generales y los requisitos transversales.
> Cada minijuego tiene su propia especificación detallada en su carpeta.
>
> Documentos por minijuego:
> - [Sopa de Letras](../minigames/word_search/spec.md) (disponible)
> - Tangram / Puzles de encaje (pendiente)
> - Lógica de Bloques (pendiente)

---

## 1. Visión del producto

**Clásicos Mente Activa 2D** es una colección de minijuegos clásicos de agilidad
mental pensada para adultos de 40 años en adelante, con especial cuidado en
personas de 60+ y en quienes tienen poca familiaridad con la tecnología.

La propuesta de valor es simple: **juegos conocidos, fáciles de entender, sin
frustración y sin castigos por el tiempo**. El jugador debe poder abrir la app,
elegir un juego y empezar a jugar en menos de 30 segundos, sin registro, sin
anuncios y sin conexión a internet.

### Principios del producto

1. **Accesibilidad primero:** todo se diseña pensando en vista, oído y motricidad
   reducidos.
2. **Sin presión:** los juegos cognitivos no castigan con tiempo ni con derrotas
   bruscas.
3. **Sin distracciones:** sin anuncios, sin compras, sin notificaciones agresivas.
4. **Simple por fuera:** pocas opciones por pantalla y navegación predecible.
5. **Contenido como dato:** juegos, temáticas y niveles editables sin reescribir
   la lógica.

---

## 2. Objetivos generales

### 2.1 Objetivo general

Ofrecer una experiencia de entretenimiento cognitivo diario, accesible y
agradable, que las personas mayores puedan disfrutar de forma autónoma y sin
sensación de examen.

### 2.2 Objetivos específicos y medibles

| # | Objetivo | Criterio de logro |
|---|----------|-------------------|
| O1 | Autonomía desde el primer uso | Un adulto 40+ completa una partida sin ayuda tras una explicación de ≤ 30 s |
| O2 | Sesiones cortas y cómodas | Partidas de 3 a 15 minutos |
| O3 | Accesibilidad real | Objetivos táctiles ≥ 56×56 px en menús; contraste alto; información nunca solo por color; texto ≥ 20 px |
| O4 | Sin frustración por tiempo | Sopa de Letras y Tangram sin cronómetro; Bloques con modo relajado |
| O5 | Uso sin conexión | 100 % funcional en modo avión |
| O6 | Rendimiento | 60 fps estables en Android de gama baja; arranque < 3 s |
| O7 | Claridad de navegación | Acción de volver siempre visible; máximo 2 niveles hasta iniciar una partida |

### 2.3 Métricas de éxito del producto

- ≥ 80 % de los usuarios de prueba terminan una partida completa en la primera
  sesión.
- ≤ 10 % abandona en los primeros 60 segundos.
- Comprensión inmediata de cada mecánica sin leer instrucciones largas.
- Comentarios cualitativos: "se ve bien", "se entiende", "no me apura".

---

## 3. Público objetivo

| Perfil | Características | Necesidad principal |
|--------|-----------------|---------------------|
| Adulto 40–59 | Juega ocasionalmente en el móvil | Rapidez y claridad |
| Adulto 60+ (prioritario) | Vista cansada, motricidad fina reducida, poca familiaridad con gestos | Letra grande, toques simples, tolerancia al error |
| Cuidador o familiar | Instala y configura la app para otra persona | Sin anuncios, sin compras, sin registros |

Necesidades transversales: ver bien, tocar fácil, entender sin jerga y recordar
pocas opciones por pantalla.

---

## 4. Alcance del producto

### 4.1 Primera versión (MVP)

| Juego | Estado | Especificación |
|-------|--------|----------------|
| Sopa de Letras | Base funcional completa; en pulido visual | [Ver spec](../minigames/word_search/spec.md) |
| Tangram / Puzles de encaje | Pendiente de implementación | Pendiente |
| Lógica de Bloques (estilo *Dr. Mario*) | Pendiente de implementación | Pendiente |

### 4.2 Fuera de alcance del MVP

- Multijugador, ranking online o cuentas de usuario.
- Compras, anuncios o suscripciones.
- iOS (se evalúa en una fase posterior).
- Reconocimiento de voz o gestos complejos.
- Traducciones a otros idiomas (el MVP es solo en español).
- Nuevos juegos fuera de los tres definidos.

---

## 5. Principios de accesibilidad transversales

Aplican a **todos** los juegos y pantallas:

1. **Grande y claro:** tipografía mínima de 20 px; títulos destacados; botones de
   menú de al menos 56×56 px.
2. **Contraste alto:** texto claro sobre fondo oscuro (o viceversa); bordes
   visibles; evitar grises sobre grises.
3. **Nunca solo color:** los estados (seleccionado, encontrado, error, pista) se
   comunican también con forma, borde, icono o texto.
4. **Feedback inmediato:** toda acción del jugador produce una respuesta visual
   clara en menos de 0,2 s.
5. **Sin castigos por tiempo:** los juegos cognitivos no presionan con cuentas
   atrás. La velocidad en Bloques es configurable.
6. **Tolerancia al error:** selección de palabras por toque inicial y final;
   encaje de piezas con margen; deshacer donde aplique.
7. **Navegación predecible:** botón "Volver" siempre presente; sin menús ocultos
   ni gestos imprescindibles.
8. **Sesiones cortas:** salir y volver no debe confundir ni perder el hilo.
9. **Sin distracciones:** sin anuncios, sin notificaciones agresivas, sin sonidos
   estridentes.
10. **Idioma:** español neutro, con tuteo y vocabulario cotidiano.

---

## 6. Plataforma y requisitos técnicos

- **Plataforma principal:** Android (móvil), orientación vertical.
- **Resolución de referencia:** 360×640 lógicos, escalable a pantallas modernas
  (20:9) y tablets.
- **Modo de estirado:** el área de juego se adapta conservando la legibilidad; el
  contenido crítico nunca debe recortarse.
- **Estilo visual:** pixel art limpio, colores saturados pero no chillones,
  fondos oscuros con elementos claros.
- **Funcionamiento offline:** todos los contenidos están incluidos en la app.
- **Persistencia:** preferencias y progreso se guardan localmente.
- **Motor:** Godot 4.x (GDScript), exportación a APK.
- **Rendimiento objetivo:** 60 fps; memoria moderada; arranque < 3 s.

---

## 7. Arquitectura funcional (nivel producto)

1. **Pantalla de inicio (Menú principal):** título de la app, tres juegos y Salir.
2. **Selección previa a cada juego:** temática, dificultad, nivel o modo.
3. **Partida:** pantalla de juego con estado visible y accesos a volver, pista /
   pausa y reiniciar.
4. **Fin de partida:** mensaje de logro claro con opción de repetir o salir.
5. **Ajustes (fase posterior):** sonido, tamaño de texto, contraste y velocidad.

Reglas de navegación:

- Siempre se puede volver al menú principal en un máximo de dos toques.
- Cambiar de juego no requiere reiniciar la app.
- Ninguna pantalla puede quedar bloqueada sin salida visible.
- Un solo mecanismo de navegación compartido por toda la app.

---

## 8. Los minijuegos (visión general)

### 8.1 Sopa de Letras

Buscar palabras escondidas en un tablero de 10×10, con cinco temáticas, sin
tiempo límite y con selección por toque inicial y final. La especificación
completa vive en [minigames/word_search/spec.md](../minigames/word_search/spec.md).

### 8.2 Tangram / Puzles de encaje

Completar siluetas colocando y rotando siete piezas geométricas clásicas. Sin
tiempo límite, con imán de encaje y rotación por botones. Niveles progresivos y
guardado de progreso. Su especificación se creará junto con su implementación.

### 8.3 Lógica de Bloques (estilo *Dr. Mario*)

Cápsulas de dos colores que caen y se eliminan al alinear cuatro o más del mismo
color. Modo relajado por defecto, pausa accesible y colores distinguibles también
por forma o símbolo. Su especificación se creará junto con su implementación.

---

## 9. Requisitos transversales del producto

### 9.1 Audio

- Música de fondo suave y opcional, con volumen bajo.
- Efectos de sonido para aciertos, errores, pistas y victoria.
- Control de volumen general y silencio total desde ajustes.
- Sin sonidos bruscos ni alarmas.

### 9.2 Ajustes

- Tamaño de texto (normal / grande / muy grande).
- Sonido y música (on/off y volumen).
- Velocidad de Bloques.
- Contraste alto.
- Restablecer progreso.

### 9.3 Persistencia

- Preferencias del jugador.
- Progreso por juego.
- Estado de la partida en curso al salir de la app (deseable).
- Todo local, sin cuentas ni nube.

### 9.4 Tutorial y ayuda

- Explicación breve la primera vez que se entra a cada juego.
- Recordatorio accesible desde un botón de ayuda.
- Sin textos largos: frases cortas y ejemplos visuales.

### 9.5 Localización

- Español como único idioma del MVP.
- Textos centralizados para facilitar futuras traducciones.

---

## 10. Criterios de aceptación del producto (MVP)

1. Los tres juegos son accesibles desde el menú principal y se pueden completar.
2. Cada juego cumple su propia especificación y sus criterios de aceptación.
3. No existen bloqueos, pantallas sin salida ni errores que impidan jugar.
4. Los objetivos táctiles principales cumplen la accesibilidad o tienen
   alternativa equivalente.
5. La app funciona sin conexión y sin permisos innecesarios.
6. El APK se instala y ejecuta en un Android de gama baja con rendimiento
   aceptable.
7. Un usuario de prueba del perfil objetivo completa una partida sin ayuda.

---

## 11. Organización de la documentación (SDD)

| Documento | Ubicación | Contenido |
|-----------|-----------|-----------|
| Spec global | `docs/spec.md` | Visión, objetivos y requisitos transversales |
| Plan global | `docs/plan.md` | Fases, checklist y entrega |
| Test global | `docs/test.md` | Estrategia y pruebas transversales |
| Spec de juego | `minigames/<juego>/spec.md` | Reglas y objetivos del juego |
| Plan de juego | `minigames/<juego>/plan.md` | Checklist del juego |
| Test de juego | `minigames/<juego>/test.md` | Casos de prueba del juego |

Regla: los documentos globales no duplican las reglas de cada juego; las resumen
y enlazan a su documento específico.

---

## 12. Glosario

- **MVP:** versión mínima jugable con los tres juegos.
- **SDD:** desarrollo guiado por especificación (spec → plan → pruebas).
- **Minijuego:** cada uno de los juegos independientes de la colección.
- **Temática:** conjunto de palabras de una partida de Sopa de Letras.
- **Pista:** ayuda que señala una solución pendiente sin penalización.
- **Modo relajado:** configuración sin presión de tiempo ni aceleración agresiva.
