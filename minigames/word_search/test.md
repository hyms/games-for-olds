# Plan de Pruebas — Sopa de Letras (v0.2)

> **Documento SDD** del minijuego *Sopa de Letras*.
> Qué se prueba, cómo y con qué resultado esperado.
>
> **Versión:** 0.2 · **Fecha:** 26/10/2026 · **Estado:** Fase 2 en curso
> **Documentación relacionada:** [Spec global](../../docs/spec.md) ·
> [Plan global](../../docs/plan.md) · [Pruebas globales](../../docs/test.md) ·
> [Spec del juego](spec.md) · [Plan del juego](plan.md)

---

## 1. Estrategia

### 1.1 Niveles de prueba

| Nivel | Cuándo | Dónde |
|-------|--------|-------|
| Humo | Tras cada cambio | Editor de Godot |
| Unitario | Al cerrar lógica de generación, selección o puntaje | GdUnit4 headless |
| Funcional | Al cerrar cada fase | Editor + Android |
| Regresión | Antes de cada entrega | Editor + Android |
| Accesibilidad | Al cerrar cada pantalla | Android real |
| Usabilidad | Antes de publicar | Usuario 40+ |

### 1.2 Enfoque

- **Caja negra** para la experiencia del jugador; **caja blanca** para generación,
  selección y puntaje.
- **Accesibilidad como requisito funcional:** un fallo de accesibilidad es un
  defecto.
- **Casos límite:** selecciones incompletas, palabras invertidas, diagonales,
  reinicios, texto grande, cambio de nivel y volver a entrar.
- **Repetición:** la generación es aleatoria; los casos de tablero se repiten al
  menos 10 veces.
- **Estado actual:** la generación y los datos existen pero no están verificados.
  La selección, el tablero visible, la lista, las pistas, los niveles y las
  estadísticas se implementan entre las fases 2 y 4 del [plan](plan.md).

---

## 2. Entornos y configuraciones

| ID | Entorno | Uso |
|----|---------|-----|
| E1 | Editor Godot, 360×640 | Desarrollo diario |
| E2 | Editor Godot, 720×1280 | Escalado intermedio |
| E3 | Android gama baja (2–3 GB RAM) | Rendimiento mínimo |
| E4 | Android gama media (4–6 GB RAM) | Referencia |
| E5 | Pantalla 20:9 | Sin recortes |
| E6 | Tablet vertical | Escalado y centrado |
| E7 | Modo avión | Uso 100 % offline |

Configuraciones:

- Texto Normal (20 px), Grande (24 px) y Muy grande (28 px).
- Niveles Fácil (8×8), Medio (10×10) y Difícil (10×10 con invertidas).
- Sonido activado y desactivado.
- Primera ejecución y ejecución con estadísticas previas.
- Orientación vertical únicamente.

---

## 3. Navegación y flujo

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| NAV-01 | Arranque | Abrir la app | Menú principal en < 3 s, sin errores | P1 |
| NAV-02 | Entrar a Sopa | Menú → Sopa de Letras | Aparece el selector de temática y nivel | P1 |
| NAV-03 | Iniciar partida | Selector → temática → Jugar | Tablero construido y jugable | P1 |
| NAV-04 | Volver al menú | Partida → Volver | Menú principal sin bloqueos | P1 |
| NAV-05 | Volver desde el selector | Selector → Volver | Menú principal | P1 |
| NAV-06 | Ida y vuelta | Entrar, salir y volver a entrar | Partida nueva sin estado viejo | P1 |
| NAV-07 | Cambio de temática | Animales → Volver → Profesiones | La partida usa la temática elegida | P1 |
| NAV-08 | Cambio de nivel | Fácil → Volver → Difícil | Se aplican tablero y reglas del nivel | P1 |
| NAV-09 | Salir de la app | Menú → Salir | Cierre correcto | P2 |

---

## 4. Accesibilidad

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| ACC-01 | Objetivos táctiles | Medir botones del menú y selector | ≥ 56×56 px en los tres tamaños de texto | P1 |
| ACC-02 | Texto Normal | Revisar etiquetas y botones | ≥ 20 px, legible | P1 |
| ACC-03 | Texto Grande | Activar Grande | ≥ 24 px; nada se corta | P1 |
| ACC-04 | Texto Muy grande | Activar Muy grande | ≥ 28 px; botones y tablero siguen usables | P1 |
| ACC-05 | Contraste | Revisar texto, celdas y bordes | ≥ 4,5:1 en texto y ≥ 3:1 en bordes | P1 |
| ACC-06 | Estados no solo por color | Seleccionar y encontrar una palabra | Se distingue por borde/forma/icono | P1 |
| ACC-07 | Legibilidad del tablero | Observar a distancia normal | Letras completas, centradas, ≥ 20 px | P1 |
| ACC-08 | Feedback inmediato | Tocar celda o botón | Respuesta visual en < 0,2 s | P1 |
| ACC-09 | Foco | Recorrer menú y selector con teclado/mando | Foco visible y orden lógico | P2 |
| ACC-10 | Sin gestos finos | Jugar una partida completa | Solo toques simples | P1 |
| ACC-11 | Sin cronómetro | Observar la partida | No hay cuenta atrás ni derrota por tiempo | P1 |
| ACC-12 | Sin desbordes | Probar 20/24/28 px en E1/E5/E6 | Todo el texto se lee; nada se solapa | P1 |
| ACC-13 | Movimiento reducido | Activar la opción | El fondo se detiene; la partida sigue | P2 |

---

## 5. Tablero y layout

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| TAB-01 | Tablero Fácil | Jugar nivel Fácil | Cuadrícula 8×8 completa | P1 |
| TAB-02 | Tablero Medio | Jugar nivel Medio | Cuadrícula 10×10 completa | P1 |
| TAB-03 | Tablero Difícil | Jugar nivel Difícil | Cuadrícula 10×10 completa | P1 |
| TAB-04 | Tablero al abrir la escena | Ejecutar la escena directamente | Se usa la temática y el nivel por defecto | P1 |
| TAB-05 | 360×640 | Probar en E1 | Tablero completo, sin recortes | P1 |
| TAB-06 | 20:9 | Probar en E5 | Tablero completo y centrado | P1 |
| TAB-07 | Tablet | Probar en E6 | Tablero completo, ni diminuto ni desbordado | P2 |
| TAB-08 | Panel del tablero | Observar el área de juego | Marco/fondo que distingue el tablero | P1 |
| TAB-09 | Celdas cuadradas | Observar la cuadrícula | Alineadas, contiguas, sin solapamientos | P1 |
| TAB-10 | Celda mínima | Medir celdas en 10×10 | ≥ 30×30 px lógicos | P1 |
| TAB-11 | Redimensionado | Cambiar la ventana del editor | El tablero se recalcula sin romperse | P2 |
| TAB-12 | Legibilidad | Recorrer todas las celdas | Ninguna letra cortada ni fuera | P1 |
| TAB-13 | Encabezado y footer | Observar la pantalla | No se pisan con el tablero | P1 |
| TAB-14 | Sin scroll | Intentar desplazar | La partida no requiere desplazamiento | P2 |

---

## 6. Generación

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| GEN-01 | Palabras íntegras | Repetir 10 partidas por temática | Cada palabra aparece completa en línea recta | P1 |
| GEN-02 | Contador coherente | Comparar contador y palabras generadas | Total = palabras colocadas | P1 |
| GEN-03 | Fácil | Jugar Fácil | 6 palabras, solo horizontal/vertical, sin invertidas | P1 |
| GEN-04 | Medio | Jugar Medio | 8 palabras, con diagonales, sin invertidas | P1 |
| GEN-05 | Difícil | Jugar Difícil | 10 palabras, con diagonales e invertidas | P1 |
| GEN-06 | Longitud | Revisar palabras | Entre 4 y 10 letras; en Fácil ≤ 8 | P1 |
| GEN-07 | Normalización | Revisar Países | ESPAÑA → ESPANA; sin Ñ ni tildes | P1 |
| GEN-08 | Solapamiento | Repetir partidas | Solo comparte celdas con la misma letra | P1 |
| GEN-09 | Tablero lleno | Observar | Sin celdas vacías | P1 |
| GEN-10 | Variedad | Reiniciar varias veces | La disposición cambia | P2 |
| GEN-11 | Mínimo de palabras | Forzar una temática difícil | Reintenta y coloca al menos 3; si no, avisa sin bloquear | P1 |
| GEN-12 | Temática vacía | Forzar una temática sin palabras válidas | Mensaje controlado; sin tablero vacío ni bloqueo | P2 |
| GEN-13 | Profesiones | Revisar la temática | `MECANICO` reemplaza a `FARMACEUTICO`; todas caben | P1 |

---

## 7. Selección de palabras

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| SEL-01 | Horizontal | Tocar primera y última letra | Se marca la palabra completa | P1 |
| SEL-02 | Vertical | Tocar primera y última letra | Se marca la palabra completa | P1 |
| SEL-03 | Diagonal | Tocar primera y última letra | Se marca la palabra completa | P1 |
| SEL-04 | Sentido inverso | Tocar última y primera letra | Se reconoce igual | P1 |
| SEL-05 | Celdas intermedias | Seleccionar una palabra de 5+ letras | Se resaltan todas las letras intermedias | P1 |
| SEL-06 | No alineadas | Tocar dos celdas fuera de línea | Se rechaza sin marcar | P1 |
| SEL-07 | Selección inválida | Tocar dos letras que no forman palabra | Aviso suave, sin penalización | P1 |
| SEL-08 | Misma celda | Tocar dos veces la misma celda | La selección se cancela | P2 |
| SEL-09 | Una sola celda | Tocar una celda y no la segunda | No se marca palabra | P2 |
| SEL-10 | Celda encontrada | Tocar una celda ya encontrada | No inicia selección | P1 |
| SEL-11 | Palabras cruzadas | Seleccionar una palabra que comparte letras | Se marca solo la correcta | P1 |
| SEL-12 | Limpieza tras acierto | Encontrar una palabra y seguir | La selección se limpia | P1 |
| SEL-13 | Dos pendientes iguales | Forzar dos palabras con la misma secuencia | Gana la de longitud exacta | P2 |
| SEL-14 | Arrastre | Arrastrar el dedo (cuando exista) | Resalta y valida al soltar | P3 |

---

## 8. Progreso, puntaje y estadísticas

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| PRG-01 | Contador inicial | Iniciar partida | 0/N con N correcto | P1 |
| PRG-02 | Contador al encontrar | Encontrar una palabra | +1 exacto | P1 |
| PRG-03 | Contador máximo | Encontrar todas | N/N, nunca más | P1 |
| PRG-04 | Reiniciar | Pulsar Reiniciar | Contador 0/N, tablero nuevo y 3 pistas | P1 |
| PRG-05 | Victoria | Encontrar la última | Mensaje con tiempo, puntaje, Repetir y Volver | P1 |
| PRG-06 | Tiempo | Completar una partida | Se registra el tiempo sin mostrarlo durante el juego | P1 |
| PRG-07 | Puntaje | Completar una partida | Aplica la fórmula del nivel y se guarda | P1 |
| PRG-08 | Mejor tiempo | Completar el mismo nivel más rápido | `best_time_seconds` baja | P1 |
| PRG-09 | Mejor puntaje | Completar el mismo nivel con más puntaje | `best_score` sube | P1 |
| PRG-10 | Nivel actual | Completar el nivel actual | `current_level` avanza | P1 |
| PRG-11 | Persistencia | Reiniciar la app | Nivel, mejor tiempo y mejor puntaje se conservan | P1 |
| PRG-12 | Abandono | Salir a mitad de partida | No se registra victoria ni mejor tiempo | P1 |
| PRG-13 | Estadísticas globales | Abrir la pantalla de estadísticas | Muestra nivel, mejor tiempo y mejor puntaje de Sopa | P1 |

---

## 9. Pistas

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| HIN-01 | Primera pista | Pulsar Pista | Resalta una palabra pendiente completa 3 s | P1 |
| HIN-02 | Contador | Usar pistas | Muestra 3 → 2 → 1 | P1 |
| HIN-03 | Límite | Usar la cuarta pista | Botón “Sin pistas” inactivo; no hay error | P1 |
| HIN-04 | No repite encontradas | Usar pista varias veces | Solo señala pendientes | P1 |
| HIN-05 | Reinicio | Agotar pistas y reiniciar | Vuelve a 3 | P1 |
| HIN-06 | No bloquea | Usar pista y seguir | La partida continúa | P1 |
| HIN-07 | Feedback no cromático | Observar la pista | Borde/animación además del color | P2 |
| HIN-08 | Sin anuncios | Agotar pistas | No aparece publicidad ni se ofrece anuncio | P1 |
| HIN-09 | Victoria | Usar pista con todo encontrado | No hay error; botón inactivo o mensaje | P2 |

---

## 10. Pantalla y controles

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| UI-01 | Encabezado | Entrar a la partida | Temática, nivel y contador visibles | P1 |
| UI-02 | Footer | Observar | Volver, Reiniciar y Pista visibles y pulsables | P1 |
| UI-03 | Sin solapes | Observar el footer | Los tres botones caben sin cortarse | P1 |
| UI-04 | Lista de palabras | Entrar a la partida | Se ven todas las palabras a buscar | P1 |
| UI-05 | Lista actualizada | Encontrar una palabra | Se marca como encontrada | P1 |
| UI-06 | Textos | Revisar en E1 y E5 | Nada se corta ni se sale | P2 |
| UI-07 | Botón sin pistas | Agotar pistas | “Sin pistas” visible e inactivo | P1 |

---

## 11. Regresión

Antes de cada entrega:

1. Arranque limpio y navegación completa.
2. Una partida completa por cada temática.
3. Una partida completa por cada nivel.
4. Tablero 8×8 y 10×10 en E1, E5 y E6.
5. Reiniciar, volver y cambio de escena repetido 20 veces.
6. Consola sin errores ni advertencias nuevas.
7. Instalación limpia del APK.
8. Estadísticas correctas tras varias partidas.

---

## 12. Matriz de trazabilidad

Cada requisito del [spec del juego](spec.md) debe llegar a una tarea del
[plan](plan.md) y a un caso de este documento.

| Requisito (spec) | Tarea (plan) | Pruebas | Estado |
|------------------|--------------|---------|--------|
| §5 Plataforma y offline | 5.3 | `NAV-01`, `NAV-09`, `GEN-01` | Pendiente |
| §6 Flujo funcional y navegación | 2.2, 3.3 | `NAV-01`…`NAV-09` | Pendiente |
| §7.1 Tablero | 2.2 | `TAB-01`…`TAB-14` | Pendiente |
| §7.1.1 Niveles de dificultad | 3.3 | `GEN-03`…`GEN-06`, `TAB-01`…`TAB-03`, `NAV-08` | Pendiente |
| §7.2 Temáticas | 1 (hecho) | `GEN-07`, `GEN-12`, `GEN-13` | Parcial |
| §7.3 Generación del tablero | 1, 2.3 | `GEN-01`…`GEN-12` | Parcial |
| §7.4 Selección de palabras | 2.1 | `SEL-01`…`SEL-13` (`SEL-14` en 3.6) | Pendiente |
| §7.5 Estado y progreso | 3.1, 3.4 | `PRG-01`…`PRG-06`, `UI-01`, `UI-04`, `UI-05` | Pendiente |
| §7.6 Pista | 3.2 | `HIN-01`…`HIN-09`, `UI-07` | Pendiente |
| §7.7 Victoria | 3.4 | `PRG-05`, `PRG-07` | Pendiente |
| §7.8 Reglas de negocio y puntaje | 3.4 | `PRG-07`…`PRG-12`, `HIN-08` | Pendiente |
| §7.9 Accesibilidad del juego | 2.2, 3.5, 4 | `ACC-01`…`ACC-12`, `TAB-10`, `TAB-12` | Pendiente |
| §8.1 Audio | 4 | Cubierto por `CORE-AUDIO` global | Pendiente |
| §8.2 Persistencia y estadísticas | 3.4, 4 | `PRG-08`…`PRG-13` | Pendiente |
| §8.3 Tutorial y ayuda | 4 | Revisión manual de usabilidad | Pendiente |
| §9 Fuera de alcance | — | `ACC-11`, `HIN-08` | — |
| §10 Criterios de aceptación | 5.1–5.3 | Regresión §11 | Pendiente |

Lectura de la matriz:

- **Parcial** significa que la lógica existe pero no está verificada en Godot.
- **Pendiente** significa que la tarea todavía no está implementada.
- Toda fila en `Pendiente` bloquea el cierre del minijuego.
- `ACC-13` (movimiento reducido) pertenece al fondo global; se prueba en el plan
  global, no en este juego.

---

## 13. Criterios de salida

No se considera terminado si:

- Existe un **P0**: cierre, bloqueo, tablero imposible de completar o pérdida de
  datos.
- Existe un **P1** en navegación, generación, selección, pistas, estadísticas o
  accesibilidad.
- La consola muestra errores en el flujo principal.
- El tablero se recorta en E1, E5 o E6.
- El texto grande desborda o tapa controles.
- Algún objetivo táctil queda por debajo de 56×56 px sin alternativa.

Se considera listo cuando no quedan P0/P1, toda la regresión pasa y se completó
una prueba de usabilidad con el perfil objetivo.

---

## 14. Severidades y reporte

| Severidad | Descripción | Ejemplo |
|-----------|-------------|---------|
| P0 | Bloqueante total | La app se cierra al entrar |
| P1 | Impide jugar o incumple accesibilidad | No se pueden encontrar palabras |
| P2 | Molesto con alternativa | Un botón secundario no responde |
| P3 | Cosmético | Texto descentrado |

Plantilla: ID, título, severidad, entorno, pasos, resultado esperado, resultado
obtenido, evidencia, frecuencia, caso relacionado y notas.
