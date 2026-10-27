# Plan de Pruebas Global — Clásicos Mente Activa 2D

> **Documento SDD global del producto.**
> Define la estrategia, los entornos y las pruebas que aplican a toda la app.
> El detalle de cada juego vive en su propio `test.md`.
>
> Planes por minijuego:
> - [Sopa de Letras](../minigames/word_search/test.md) (disponible)
> - Tangram / Puzles de encaje (pendiente)
> - Lógica de Bloques (pendiente)

---

## 1. Estrategia

### 1.1 Niveles de prueba

| Nivel | Cuándo | Dónde | Responsable |
|-------|--------|-------|-------------|
| Humo | Tras cada cambio | Editor de Godot | Desarrollo |
| Funcional del juego | Al cerrar cada fase | Editor + Android | Desarrollo |
| Regresión | Antes de cada entrega | Editor + Android | Desarrollo |
| Accesibilidad | Al cerrar cada juego | Android real | Desarrollo + revisión |
| Usabilidad | Antes de publicar | Con usuario 40+ | Producto |

### 1.2 Enfoque

- **Caja negra:** se prueba lo que ve y hace el jugador.
- **Accesibilidad como requisito funcional:** un fallo de accesibilidad es un
  defecto, no una mejora.
- **Casos límite siempre:** selecciones incompletas, reinicios, volver y entrar,
  pantallas extremas y primera ejecución.
- **Repetición:** los contenidos aleatorios se prueban al menos 10 veces.
- **Trazabilidad:** cada caso tiene ID y cada defecto referencia su caso.

### 1.3 Pirámide de pruebas del producto

1. **Humo global:** la app arranca, muestra el menú y permite entrar y salir de
   cada juego.
2. **Funcional por juego:** cubierto en el `test.md` de cada minijuego.
3. **Transversal:** navegación, accesibilidad, rendimiento, estabilidad,
   persistencia, audio y offline.
4. **Regresión y entrega:** checklist completo antes de publicar.
5. **Usabilidad:** validación con el perfil objetivo.

---

## 2. Entornos y matriz de dispositivos

### 2.1 Dispositivos / configuraciones

| ID | Entorno | Uso |
|----|---------|-----|
| E1 | Editor Godot, ventana 360×640 | Desarrollo diario |
| E2 | Editor Godot, ventana 720×1280 | Escalado intermedio |
| E3 | Android gama baja (2–3 GB RAM) | Rendimiento mínimo aceptable |
| E4 | Android gama media (4–6 GB RAM) | Experiencia de referencia |
| E5 | Pantalla 20:9 (1080×2400 o similar) | Comprobar que no hay recortes |
| E6 | Tablet vertical | Escalado y centrado |
| E7 | Modo avión | Uso 100 % offline |

### 2.2 Configuraciones de prueba

- Texto normal y texto grande (cuando exista el ajuste).
- Sonido activado y desactivado.
- Orientación vertical únicamente.
- Primera ejecución (sin datos) y ejecución con progreso previo.
- Instalación limpia y actualización sobre una instalación anterior.

---

## 3. Pruebas transversales

### 3.1 Instalación y arranque

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| APP-01 | Instalación limpia | Instalar el APK en dispositivo sin datos previos | Instala y abre sin errores | P1 |
| APP-02 | Actualización | Instalar una versión nueva sobre la anterior | Conserva preferencias y progreso | P1 |
| APP-03 | Arranque en frío | Abrir la app cerrada | Menú principal en < 3 s | P1 |
| APP-04 | Arranque sin conexión | Abrir en modo avión | Funciona igual que con conexión | P1 |
| APP-05 | Permisos | Revisar permisos solicitados | Solo los mínimos necesarios; ninguno innecesario | P1 |
| APP-06 | Cierre correcto | Pulsar Salir | La app se cierra sin errores | P2 |

### 3.2 Navegación global

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| NAV-01 | Menú principal | Abrir la app | Se ven los tres juegos y Salir, claramente | P1 |
| NAV-02 | Entrar a cada juego | Menú → cada juego | Cada entrada lleva a su pantalla correspondiente | P1 |
| NAV-03 | Volver al menú | Desde cada juego, volver | Se llega al menú en un máximo de dos toques | P1 |
| NAV-04 | Ida y vuelta repetida | Entrar y salir 20 veces alternando juegos | Sin bloqueos, sin estado viejo, sin errores | P1 |
| NAV-05 | Juego no disponible | Entrar a un juego aún no implementado | No hay pantalla vacía ni bloqueo; se informa claramente | P2 |
| NAV-06 | Botón atrás del sistema | Pulsar atrás en menú y en juego | Comportamiento predecible; no cierra la app de forma accidental | P2 |
| NAV-07 | Rotación del dispositivo | Girar el móvil | La app permanece en vertical | P2 |

### 3.3 Accesibilidad

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| ACC-01 | Objetivos táctiles | Medir botones de menús y selectores | ≥ 56×56 px lógicos | P1 |
| ACC-02 | Contraste | Revisar textos y elementos sobre fondo | Contraste alto y legible | P1 |
| ACC-03 | Información no solo por color | Revisar estados en todos los juegos | Siempre hay forma, borde, icono o texto adicional | P1 |
| ACC-04 | Tipografía | Revisar tamaños de texto | ≥ 20 px en textos principales | P1 |
| ACC-05 | Feedback inmediato | Tocar botones y celdas | Respuesta visual en < 0,2 s | P1 |
| ACC-06 | Navegación por foco | Recorrer menús con teclado o mando | Foco visible y orden lógico | P2 |
| ACC-07 | Sin gestos finos obligatorios | Jugar cada juego solo con toques | Toda acción tiene alternativa táctil simple | P1 |
| ACC-08 | Sin presión de tiempo | Observar Sopa y Tangram | No hay cronómetros ni derrotas por tiempo | P1 |
| ACC-09 | Textos grandes sin desbordar | Activar texto grande (cuando exista) | Nada se corta ni se sale de pantalla | P2 |

### 3.4 Rendimiento y estabilidad

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| PERF-01 | FPS en gama baja | Usar la app 5 minutos en E3 | 60 fps estables | P1 |
| PERF-02 | Memoria | Sesión de 15 minutos | Sin crecimiento anómalo | P2 |
| PERF-03 | Cambios de escena | Entrar y salir 20 veces | Sin fugas visibles ni errores | P1 |
| PERF-04 | Segundo plano | Minimizar y volver a la app | Se recupera sin errores ni pérdida de estado | P2 |
| PERF-05 | Sesión larga | 20–30 minutos de uso | Sin degradación, sin cierres | P2 |

### 3.5 Consola y errores

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| LOG-01 | Arranque limpio | Abrir la app y recorrer el menú | Sin errores ni advertencias | P1 |
| LOG-02 | Carga de contenidos | Entrar a cada juego y selector | Todos los recursos cargan sin avisos | P1 |
| LOG-03 | Reinicios | Reiniciar partidas y volver a entrar | Sin errores de nodos liberados | P1 |

### 3.6 Persistencia y ajustes (cuando existan)

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| PER-01 | Guardado de preferencias | Cambiar sonido o tamaño de texto y reabrir | La preferencia se conserva | P1 |
| PER-02 | Guardado de progreso | Completar contenido y reabrir | El progreso se conserva | P1 |
| PER-03 | Restablecer progreso | Usar la opción de restablecer | Vuelve al estado inicial con confirmación | P2 |
| PER-04 | Partida en curso | Salir de la app a mitad de partida y volver | Estado manejado sin errores (conservado o reiniciado de forma clara) | P2 |

### 3.7 Audio

| ID | Caso | Pasos | Resultado esperado | Prioridad |
|----|------|-------|--------------------|-----------|
| AUD-01 | Efectos | Realizar acciones en cada juego | Sonidos suaves, sin saturación | P2 |
| AUD-02 | Silencio total | Activar silencio | No suena nada hasta reactivarlo | P2 |
| AUD-03 | Mezcla | Música y efectos a la vez | Los efectos se distinguen de la música | P3 |
| AUD-04 | Segundo plano | Minimizar durante el juego | El audio se pausa correctamente | P3 |

---

## 4. Pruebas por juego

El detalle completo está en el plan de pruebas de cada minijuego. Aquí se define
el **humo mínimo** que todo juego debe pasar antes de su prueba funcional.

### 4.1 Humo mínimo por juego

| ID | Caso | Resultado esperado |
|----|------|--------------------|
| SMK-01 | Entrar al juego desde el menú | Se abre la pantalla correcta |
| SMK-02 | Iniciar una partida | El contenido se genera/carga sin errores |
| SMK-03 | Realizar una acción principal | Hay feedback visual inmediato |
| SMK-04 | Salir del juego | Se vuelve al menú sin bloqueos |
| SMK-05 | Volver a entrar | El juego arranca limpio |
| SMK-06 | Consola | Sin errores ni advertencias |

### 4.2 Sopa de Letras

Ver [minigames/word_search/test.md](../minigames/word_search/test.md):
tablero y layout (TAB), generación (GEN), selección (SEL), progreso (PRG),
pista (HIN) y pantalla/controles (UI).

### 4.3 Tangram / Puzles de encaje

Plan de pruebas pendiente de crear junto con el minijuego. Debe cubrir al menos:
arrastre, rotación por botones, snap con tolerancia, encaje incorrecto,
completar nivel, reiniciar, pista, progreso guardado y ausencia de tiempo límite.

### 4.4 Lógica de Bloques

Plan de pruebas pendiente de crear junto con el minijuego. Debe cubrir al menos:
caída, movimiento, giro, aterrizaje, eliminación de cuatro o más, cadenas,
límites del tablero, fin de nivel, fin de partida, pausa, modo relajado y
distinción de colores por forma.

---

## 5. Regresión global

Se ejecuta antes de cada entrega:

1. Instalación limpia y arranque en frío.
2. Recorrido completo del menú y de los tres juegos.
3. Humo mínimo de cada juego (SMK-01 a SMK-06).
4. Accesibilidad básica (ACC-01 a ACC-05, ACC-07 y ACC-08).
5. Cambios de escena repetidos (20 veces).
6. Consola sin errores ni advertencias nuevas.
7. Funcionamiento en modo avión.
8. Actualización sobre instalación anterior.
9. Una partida completa del juego que se modificó en la entrega.

---

## 6. Checklist de publicación

- [ ] Build firmado generado e instalado en dispositivo real.
- [ ] Regresión global completa y sin P0 ni P1.
- [ ] Los planes de prueba de los tres juegos ejecutados.
- [ ] Accesibilidad verificada en dispositivo.
- [ ] Rendimiento medido en gama baja.
- [ ] Ficha de producto, icono y capturas listos.
- [ ] Política de privacidad (sin datos recogidos) publicada.
- [ ] Versión y notas de la entrega registradas.
- [ ] Copia del build archivada.

---

## 7. Criterios de salida (exit criteria)

No se publica una entrega si:

- Existe un defecto **P0**: cierre de la app, bloqueo, pérdida de datos o
  contenido imposible de completar.
- Existe un defecto **P1** abierto en navegación, accesibilidad básica o en el
  flujo principal de un juego.
- La consola muestra errores en el flujo principal.
- El contenido crítico se recorta en E5 o E6.
- Algún objetivo táctil principal queda por debajo del mínimo sin alternativa.

Se considera lista cuando:

- No quedan P0 ni P1 abiertos.
- Toda la regresión global pasa.
- Se completó al menos una prueba de usabilidad con el perfil objetivo.

---

## 8. Severidades de defectos

| Severidad | Descripción | Ejemplo |
|-----------|-------------|---------|
| P0 | Bloqueante total | La app se cierra al entrar a un juego |
| P1 | Impide jugar o incumple accesibilidad básica | Un tablero no se ve o no se puede completar |
| P2 | Molesto pero con alternativa | Un botón secundario no responde |
| P3 | Cosmético | Un texto descentrado o un color poco fino |

### Plantilla de reporte

- **ID:**
- **Título:**
- **Severidad:** P0 / P1 / P2 / P3
- **Juego / pantalla:**
- **Entorno:** editor o dispositivo, resolución, versión
- **Pasos para reproducir:**
- **Resultado esperado:**
- **Resultado obtenido:**
- **Evidencia:** captura o video
- **Frecuencia:** siempre / a veces / una vez
- **Caso de prueba relacionado:**
- **Notas:**

---

## 9. Gestión de la calidad SDD

- Cada juego no se cierra sin su `test.md` ejecutado y sin P0/P1.
- Todo cambio de reglas actualiza primero el `spec` y luego las pruebas.
- Los defectos encontrados durante el desarrollo se registran con su severidad y
  se enlazan al caso que los detectó.
- Las pruebas manuales actuales son la base; la automatización se evaluará
  cuando el contenido se estabilice.
