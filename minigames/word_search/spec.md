# Especificación — Sopa de Letras

> **Documento SDD** del minijuego *Sopa de Letras* de **Clásicos Mente Activa 2D**.
> Especifica únicamente este juego. Tangram y Lógica de Bloques tienen su propia
> especificación en sus respectivas carpetas.
>
> **Versión:** 0.2 · **Fecha:** 26/10/2026 · **Estado:** MVP en desarrollo
> **Documentación relacionada:** [Spec global](../../docs/spec.md) ·
> [Plan global](../../docs/plan.md) · [Pruebas globales](../../docs/test.md) ·
> [Plan del juego](plan.md) · [Pruebas del juego](test.md)
>
> Ubicación del minijuego: `minigames/word_search/`

---

## 1. Contexto y visión

**Clásicos Mente Activa 2D** es una colección de minijuegos clásicos de agilidad
mental para adultos de 40 años en adelante, con especial cuidado en personas de
60+ y en quienes tienen poca familiaridad con la tecnología.

La **Sopa de Letras** es el primer minijuego de la colección. El jugador elige
una temática y busca palabras escondidas en un tablero de letras. La experiencia
debe ser relajante: sin cronómetro, sin anuncios, sin registro y sin castigos por
equivocarse.

La propuesta de valor del minijuego es: **juego conocido, fácil de entender,
legible y sin frustración**.

---

## 2. Objetivos

### 2.1 Objetivo general

Ofrecer una sopa de letras accesible y agradable que una persona mayor pueda
jugar de forma autónoma, en sesiones cortas, sin sentirse examinada.

### 2.2 Objetivos específicos y medibles

| # | Objetivo | Criterio de logro |
|---|----------|-------------------|
| O1 | Autonomía desde el primer uso | El jugador entiende la mecánica y encuentra su primera palabra sin ayuda |
| O2 | Sesiones cortas | Una partida completa dura entre 3 y 10 minutos |
| O3 | Accesibilidad real | Letras legibles, alto contraste, estados no solo por color y selección sin gestos finos |
| O4 | Sin presión de tiempo | No existe cronómetro ni derrota |
| O5 | Claridad | El jugador ve siempre qué palabras busca, cuántas lleva y cómo salir |
| O6 | Rendimiento | 60 fps estables en Android de gama baja |

### 2.3 Métricas de éxito del minijuego

- ≥ 80 % de los usuarios de prueba encuentran al menos una palabra sin ayuda.
- ≤ 10 % abandona la partida en los primeros 60 segundos.
- El jugador identifica sin explicación qué debe hacer al ver la pantalla.
- Comentarios cualitativos: "se entiende", "se ve bien", "no me apura".

---

## 3. Público objetivo

| Perfil | Características | Necesidad principal |
|--------|-----------------|---------------------|
| Adulto 40–59 | Juega ocasionalmente en el móvil | Rapidez y claridad |
| Adulto 60+ (prioritario) | Vista cansada, motricidad fina reducida, poca familiaridad con gestos | Letra grande, toques simples, tolerancia al error |
| Cuidador o familiar | Configura la app para otra persona | Sin anuncios, sin compras, sin registros |

Necesidades transversales: ver bien, tocar fácil, entender sin jerga y recordar
pocas opciones por pantalla.

---

## 4. Principios de accesibilidad del minijuego

1. **Letras grandes y legibles** en todas las celdas del tablero.
2. **Alto contraste** entre letras, celdas y fondo.
3. **Nunca solo color:** los estados seleccionada, encontrada, error y pista se
   comunican también con borde, forma, icono o texto.
4. **Feedback inmediato:** toda acción produce respuesta visual en menos de 0,2 s.
5. **Sin cronómetro** ni presión de tiempo.
6. **Tolerancia al error:** selección por toque inicial y final; una selección
   inválida se deshace sin penalización.
7. **Navegación predecible:** botón Volver siempre visible; máximo dos toques
   hasta el menú principal.
8. **Sin distracciones:** sin anuncios, sin notificaciones agresivas y sin sonidos
   estridentes.

---

## 5. Plataforma y requisitos técnicos (nivel producto)

- **Plataforma principal:** Android móvil, orientación vertical.
- **Resolución de referencia:** 360×640 lógicos, escalable a pantallas 20:9 y tablets.
- **Estilo visual:** pixel art limpio, fondo oscuro y elementos claros.
- **Funcionamiento offline:** las palabras y temáticas están incluidas en la app.
- **Motor:** Godot 4.x (GDScript), exportación a APK.
- **Rendimiento:** 60 fps en gama baja; arranque de la partida sin esperas visibles.

---

## 6. Flujo funcional

1. **Menú principal:** el jugador toca "Sopa de Letras".
2. **Selector de partida:** una sola pantalla con las cinco temáticas, los tres
   niveles de dificultad y el botón **Jugar**. También incluye Volver.
3. **Partida:** encabezado con temática, nivel y contador; tablero; footer con
   Volver, Reiniciar y Pista.
4. **Victoria:** mensaje claro al encontrar todas las palabras, con el tiempo, el
   puntaje y las opciones **Repetir** y **Volver**.

Reglas de navegación:

- Desde la partida se vuelve al menú en un toque.
- Reiniciar no abandona la temática ni el nivel actual.
- Ninguna pantalla queda bloqueada sin salida visible.

---

## 7. Reglas del juego

### 7.1 Tablero

- Tamaño según el nivel: **8×8** en Fácil y **10×10** en Medio/Difícil.
- Todas las celdas muestran una letra; no hay huecos.
- El tablero debe verse completo en pantalla, sin recortes ni desplazamiento.
- Las celdas son cuadradas, contiguas y alineadas; el conjunto se percibe como un
  tablero con marco propio.
- Las letras están centradas y son legibles en todas las celdas.

### 7.1.1 Niveles de dificultad

| Nivel | Nombre | Tablero | Palabras | Diagonales | Invertidas | Pistas |
|-------|--------|---------|----------|------------|------------|--------|
| 1 | Fácil | 8×8 | 6 | No | No | 3 |
| 2 | Medio | 10×10 | 8 | Sí | No | 3 |
| 3 | Difícil | 10×10 | 10 | Sí | Sí | 3 |

- El nivel se elige en el selector y se conserva al reiniciar.
- Las dificultades son datos de la Sopa; el nivel global no las impone.
- El nivel actual se guarda en las estadísticas del juego.

### 7.2 Temáticas disponibles (MVP)

| Temática | Ejemplo de palabras | Palabras candidatas |
|----------|---------------------|---------------------|
| Animales | PERRO, GATO, CABALLO, TORTUGA | 12 |
| Frutas y Verduras | MANZANA, PLATANO, ZANAHORIA, SANDIA | 14 |
| Países y Ciudades | MEXICO, ARGENTINA, BARCELONA, LIMA | 13 |
| Objetos del Hogar | MESA, SILLA, CUCHARA, TELEVISOR | 13 |
| Profesiones | MEDICO, MAESTRO, CARPINTERO, MECANICO | 12 |

> Todas las palabras candidatas tienen entre 4 y 10 letras. En Fácil se descartan
> las de más de 8 letras porque el tablero es 8×8. Si se añade una palabra más
> larga que el tablero, el juego la descarta y lo indica en el Inspector del
> recurso; no se permite que una temática quede vacía.

Las temáticas son datos editables: añadir una nueva no debe requerir cambios en
la lógica del juego.

### 7.3 Generación del tablero

- Se eligen las palabras del nivel: **6** en Fácil, **8** en Medio y **10** en
  Difícil, al azar entre las candidatas.
- Solo se aceptan palabras de **4 a 10 letras** (mínimo 4; máximo impuesto por el
  tablero).
- Direcciones permitidas según el nivel: horizontal y vertical en Fácil;
  horizontal, vertical y diagonal en Medio y Difícil.
- Se permiten palabras invertidas solo en Difícil (de derecha a izquierda o de
  abajo arriba).
- El solapamiento entre palabras solo se permite cuando comparten la misma letra.
- Los espacios libres se rellenan con letras al azar.
- Se normalizan los textos a A–Z: sin tildes, diéresis ni Ñ (ESPAÑA → ESPANA).
- Todas las palabras deben existir íntegras en el tablero, en línea recta y en una
  sola dirección.
- La disposición cambia al reiniciar o al volver a entrar.
- **Política de fallo:** si una disposición coloca menos de 3 palabras, se
  reintenta hasta 5 veces. Si aun así no se alcanza el mínimo, se muestra un
  mensaje claro ("No se pudo preparar el tablero") y se ofrece reintentar, sin
  dejar al jugador en un tablero vacío o sin salida.

### 7.4 Selección de palabras

- **Versión actual:** el jugador toca la primera letra y luego la última letra de
  la palabra.
- **Ruta de selección:** se construye con **todas las celdas en línea recta**
  entre la primera y la última, extremos incluidos. No se aceptan saltos ni
  celdas intermedias fuera de la línea.
- **Alineación:** la selección debe ser horizontal, vertical o diagonal exacta.
  Si las dos celdas no están alineadas, se rechaza sin marcar letras.
- **Coincidencia:** se compara la secuencia completa de letras con cada palabra
  pendiente, en cualquier sentido. Si dos palabras pendientes comparten la misma
  secuencia, gana la de longitud exacta de la selección.
- **Una sola celda:** tocar dos veces la misma celda cancela la selección; no se
  marca ninguna palabra.
- **Selección inválida:** se deshace con un aviso visual suave de 0,5 s y sin
  penalización.
- Las palabras encontradas quedan marcadas de forma permanente.
- No se puede iniciar una selección desde una celda ya encontrada.
- **Versión futura:** arrastre del dedo con tolerancia, manteniendo también la
  selección por toques.

### 7.5 Estado y progreso

- Contador siempre visible: "Palabras encontradas: X/Y".
- Lista de palabras pendientes siempre visible bajo el encabezado, con las
  encontradas tachadas o marcadas.
- El contador nunca puede superar el total ni quedar desincronizado.
- Al encontrar la última palabra se muestra el mensaje
  **"¡Enhorabuena! Encontraste todas las palabras"** con el tiempo, el puntaje y
  los botones **Repetir** y **Volver**.
- Reiniciar devuelve el contador a 0, restaura las pistas a 3 y genera una
  disposición nueva.

### 7.6 Pista

- **Límite:** 3 pistas por partida. No se acumulan entre partidas.
- El botón Pista muestra las pistas restantes: "Pista (3)", "Pista (2)", etc.
- Cada uso resalta **una palabra pendiente completa** (o su primera celda y
  dirección) durante **3 segundos**, con borde o animación además del color.
- Nunca señala una palabra ya encontrada.
- No penaliza, no bloquea y no interrumpe la partida.
- Al llegar a 0, el botón queda inactivo con el texto **"Sin pistas"** y un
  tooltip o etiqueta que explica que se recuperan al reiniciar.
- Reiniciar la partida restaura las 3 pistas.
- **La Sopa no muestra anuncios** en esta versión; la recarga por anuncio no
  forma parte de su alcance.

### 7.7 Victoria

- Se produce únicamente al encontrar todas las palabras.
- No existe derrota ni fin de partida por tiempo.
- El mensaje permite repetir la partida o volver al selector.

### 7.8 Reglas de negocio

- Encontrar palabras no otorga puntos durante la partida; el valor es el logro
  personal. El puntaje se calcula al terminar y se guarda en estadísticas.
- **Fórmula de puntaje de la Sopa:**
  `puntaje = palabras_encontradas * 100 + max(0, 600 - segundos) - pistas_usadas * 50`,
  con un mínimo de 0. El multiplicador por nivel es 1.0 en Fácil, 1.25 en Medio y
  1.5 en Difícil. El resultado se redondea a entero.
- Repetir partida genera una nueva disposición y restaura las 3 pistas.
- La partida no se pierde por equivocarse.
- No hay compras ni cuentas. **La Sopa no muestra anuncios**; su sistema de pistas
  no depende de publicidad.

### 7.9 Accesibilidad específica del juego

- Celdas de al menos **30×30 px lógicos** en 360×640; letras de al menos
  **20 px** en tamaño normal.
- La selección por toque inicial/final compensa el tamaño reducido de celda.
- Contraste mínimo **4,5:1** para letras y **3:1** para bordes y estados.
- Estado encontrado distinguible por color **y** por borde/estilo.
- Sin dependencia de arrastre ni de gestos de precisión en la versión actual.
- El tamaño de texto global (Normal / Grande / Muy grande) solo cambia la
  tipografía; no reduce el tablero ni los objetivos táctiles.

---

## 8. Requisitos transversales del minijuego

### 8.1 Audio

- Efectos suaves para acierto, selección inválida, pista y victoria.
- Música de fondo opcional y de volumen bajo.
- Silencio total accesible desde ajustes (fase posterior).

### 8.2 Persistencia

- Preferencias de sonido y tamaño de texto (fase posterior).
- **Estadísticas del juego:** nivel actual, mejor tiempo y mejor puntaje del
  nivel actual. El esquema global vive en `SaveManager`.
- Estado de la partida en curso al salir de la app (deseable).
- Todo local, sin cuentas ni nube.

### 8.2.1 Estadísticas y puntaje

- Al terminar una partida se registra: nivel, tiempo total en segundos, puntaje
  y si se completó.
- `best_time_seconds` guarda el menor tiempo para completar el nivel actual.
- `best_score` guarda el mayor puntaje obtenido en el nivel actual.
- La pantalla global de estadísticas muestra, por juego: nivel actual, mejor
  tiempo del nivel y mejor puntaje.
- Reiniciar estadísticas exige confirmación.

### 8.3 Tutorial y ayuda

- Explicación breve de la mecánica la primera vez que se abre el juego.
- Recordatorio accesible desde un botón de ayuda.
- Sin textos largos: frases cortas y ejemplos visuales.

### 8.4 Localización

- Español como único idioma del MVP.
- Textos centralizados para facilitar futuras traducciones.

---

## 9. Fuera de alcance de este minijuego

- Tangram y Lógica de Bloques (tienen su propio documento).
- Multijugador, ranking online y cuentas.
- Compras o suscripciones.
- Anuncios dentro de la Sopa (decisión propia del juego).
- Nuevos tamaños de tablero fuera de los tres niveles definidos.
- Reconocimiento de voz y gestos complejos.

---

## 10. Criterios de aceptación

1. El tablero se ve completo y legible en 360×640, en 20:9 y en tablet, en los
   tres niveles.
2. Todas las palabras del contador existen íntegras en el tablero.
3. La selección funciona en horizontal, vertical, diagonal y en sentido inverso
   según el nivel.
4. Una selección inválida no penaliza ni bloquea.
5. El contador y la lista de palabras se mantienen sincronizados.
6. La pista señala siempre una palabra pendiente y se agota a las 3, sin errores
   ni bloqueos; se restaura al reiniciar.
7. Se puede terminar una partida sin usar gestos de precisión.
8. Al terminar se registran nivel, tiempo y puntaje en las estadísticas.
9. No hay errores ni advertencias en consola durante el flujo completo.
10. Un usuario del perfil objetivo completa una partida sin ayuda.

---

## 11. Glosario

- **Tablero:** cuadrícula de 8×8 o 10×10 celdas donde se esconden las palabras.
- **Temática:** conjunto de palabras asociado a un tema.
- **Palabra pendiente:** palabra de la lista que aún no se ha encontrado.
- **Pista:** ayuda que señala una solución pendiente; la Sopa ofrece 3 por
  partida.
- **Nivel:** dificultad de la Sopa (Fácil, Medio o Difícil) con tablero y reglas
  propias.
- **Puntaje:** valor calculado al terminar la partida según palabras, tiempo y
  pistas usadas.
- **Selección:** acción de marcar la primera y la última letra de una palabra.
