# Plan de Trabajo — Sopa de Letras (v0.2)

> **Documento SDD** del minijuego *Sopa de Letras*.
> Checklist de construcción y estado real de cada tarea.
>
> **Versión:** 0.2 · **Fecha:** 26/10/2026 · **Estado:** Fase 2 en curso
> **Documentación relacionada:** [Spec global](../../docs/spec.md) ·
> [Plan global](../../docs/plan.md) · [Pruebas globales](../../docs/test.md) ·
> [Spec del juego](spec.md) · [Pruebas del juego](test.md)

## Leyenda

- `[x]` Hecho y verificado en Godot
- `[ ]` Pendiente
- 🟡 Parcial o pendiente de verificación
- 🔜 En curso
- **P1** Bloqueante · **P2** Importante · **P3** Mejora

> **Estado real:** la selección de palabras **no funciona** para palabras de
> varias letras. La ruta solo contiene la primera celda y `_end_selection()`
> ignora la última. La validación existe, pero no puede acertar. Se corrige en la
> Fase 2.

---

## Fase 0 — Base técnica compartida

- [x] Estructura actual del proyecto (`autoloads/`, `core/`, `ui/`, `minigames/`, `resources/`, `tests/`).
- [x] Proyecto en vertical 360×640, estirado `canvas_items`.
- [x] Emulación de toque → ratón para escritorio.
- [x] Autoload único `SceneManager`.
- [x] Tema accesible compartido y `BigButton` (56×56).
- [x] Entrada desde el menú principal.
- [x] Documentación SDD del minijuego.

---

## Fase 1 — Datos y generación

- [x] Recurso de temática editable (`WordSearchTheme`).
- [x] Cinco temáticas: Animales, Frutas y Verduras, Países y Ciudades, Objetos del Hogar, Profesiones.
- [x] Normalización a A–Z, sin tildes ni Ñ.
- [x] Filtro por longitud según el tablero.
- [x] Generador procedural con 8 direcciones configurables.
- [x] Solapamiento solo con letra coincidente.
- [x] Relleno de huecos con letras aleatorias.
- [x] Celda con estados normal / seleccionada / encontrada.
- [x] Contador “Palabras encontradas: X/Y”.
- [x] Botones Volver, Reiniciar y Pista.
- [x] Tipado estricto en los scripts actuales.
- [ ] Política de generación parcial: reintentar hasta 5 veces y avisar si no hay mínimo de 3 palabras. *(se cierra en 2.3)*
- [x] Sustituir `FARMACEUTICO` por `MECANICO` en Profesiones.

---

## Fase 2 — Selección y tablero (P1) 🔜

### 2.1 Corrección de selección

- [ ] Al tocar la segunda celda, construir todas las celdas en línea recta entre extremos.
- [ ] Rechazar si las celdas no están alineadas (horizontal, vertical o diagonal).
- [ ] Rechazar selección de una sola celda o doble toque en la misma celda.
- [ ] Comparar la secuencia completa contra las palabras pendientes, en ambos sentidos.
- [ ] Validar también por posiciones para evitar ambigüedades.
- [ ] Limpiar la selección tras acierto o error.
- [ ] Feedback visual suave de 0,5 s en selección inválida.
- [ ] Impedir iniciar selección en celdas ya encontradas.

### 2.2 Tablero visual

- [ ] Temática por defecto si la escena se abre directamente.
- [ ] Handoff del selector con la señal oficial de cambio de escena.
- [ ] Panel con marco del tablero.
- [ ] Celda adaptable al espacio disponible (sin 48 px fijos).
- [ ] Estilo “WordCell” (márgenes pequeños, letra centrada, borde de cuadrícula).
- [ ] Tablero 8×8 para Fácil y 10×10 para Medio/Difícil.
- [ ] Tablero completo en 360×640, 20:9 y tablet.
- [ ] Sin recortes ni scroll.

### 2.3 Política de generación (P1)

- [ ] Calcular el tablero hasta 5 veces si no alcanza 3 palabras.
- [ ] Detectar la temática sin palabras válidas para el nivel.
- [ ] Mostrar “No se pudo preparar el tablero” con opción de reintentar o cambiar de nivel.
- [ ] No dejar nunca un tablero vacío ni una pantalla sin salida.

### 2.4 Verificación de la fase

- [ ] Probar las cinco temáticas en los tres niveles.
- [ ] Probar en editor y Android.
- [ ] Consola sin errores ni advertencias nuevas.
- [ ] Registrar errores nuevos en el catálogo del skill.

---

## Fase 3 — Jugabilidad visible y pulido

### 3.1 Lista de palabras (P1)

- [ ] Mostrar todas las palabras bajo el encabezado.
- [ ] Tachar o marcar las encontradas.
- [ ] Actualizar al instante.
- [ ] Verificar que la lista no reduce el tablero por debajo de 30 px por celda.

### 3.2 Pistas (P1)

- [ ] Implementar 3 pistas por partida.
- [ ] Mostrar contador “Pista (3)”, “Pista (2)”, etc.
- [ ] Resaltar una palabra pendiente completa durante 3 s.
- [ ] Evitar palabras ya encontradas.
- [ ] Feedback no cromático (borde/animación).
- [ ] Al agotar: botón “Sin pistas” inactivo.
- [ ] Restaurar las 3 pistas al reiniciar.
- [ ] No bloquear la partida ni depender de anuncios.

### 3.3 Dificultad (P1)

- [ ] Fácil: 8×8, 6 palabras, sin diagonales, sin invertidas.
- [ ] Medio: 10×10, 8 palabras, con diagonales, sin invertidas.
- [ ] Difícil: 10×10, 10 palabras, con diagonales e invertidas.
- [ ] Selector de nivel en la pantalla de partida.
- [ ] Conservar el nivel al reiniciar.
- [ ] Dificultades como datos, no incrustadas en la lógica.

### 3.4 Victoria, puntaje y estadísticas (P1)

- [ ] Mensaje de victoria con tiempo, puntaje, Repetir y Volver.
- [ ] Fórmula de puntaje:
      `palabras * 100 + max(0, 600 - segundos) - pistas * 50`, mínimo 0.
- [ ] Multiplicador por nivel: Fácil 1.0, Medio 1.25, Difícil 1.5.
- [ ] Registrar nivel, tiempo, puntaje y resultado en `SaveManager`.
- [ ] Actualizar `best_time_seconds` y `best_score` del nivel actual.
- [ ] No mostrar cronómetro durante la partida.

### 3.5 Feedback y robustez (P2)

- [ ] Animación breve al encontrar una palabra.
- [ ] Sonido suave de acierto, error, pista y victoria.
- [ ] Reiniciar limpia estado, contador y pistas.
- [ ] Volver y entrar no arrastra estado viejo.
- [ ] El contador nunca se desincroniza.
- [ ] Probar selección de una celda, dos letras, zigzag y palabras cruzadas.

### 3.6 Selección por arrastre (P3)

- [ ] Detectar arrastre y resaltar celdas.
- [ ] Validar al soltar.
- [ ] Mantener la selección por toques como opción principal.
- [ ] Añadir tolerancia sin conflicto con scroll.

---

## Fase 4 — Transversal del minijuego

- [ ] Sonido de acierto, error, pista y victoria.
- [ ] Música de fondo opcional.
- [ ] Control de audio desde ajustes.
- [ ] Tamaño de texto Normal / Grande / Muy grande sin desbordes.
- [ ] Guardado del estado de la partida en curso (deseable).
- [ ] Tutorial breve la primera vez.
- [ ] Botón de ayuda “?”.
- [ ] Textos centralizados para futuras traducciones.
- [ ] Estadísticas visibles en la pantalla global.

---

## Fase 5 — Calidad y entrega del minijuego

### 5.1 Pruebas (P1)

- [ ] Ejecutar `test.md` en editor.
- [ ] Ejecutar `test.md` en Android real.
- [ ] Probar 16:9, 18:9, 19.5:9 y 20:9.
- [ ] Prueba de usabilidad con el perfil objetivo.
- [ ] Consola sin errores ni advertencias nuevas.

### 5.2 Rendimiento y estabilidad (P1)

- [ ] 60 fps en gama baja.
- [ ] Tiempo hasta tablero jugable < 0,5 s.
- [ ] Memoria estable en sesiones largas.
- [ ] Sin fugas al entrar y salir repetidamente.

### 5.3 Integración en la app (P1)

- [ ] APK instala y ejecuta el juego.
- [ ] Flujo menú → selector → partida → victoria → menú.
- [ ] Modo avión.
- [ ] Estadísticas por juego y nivel correctas.

---

## Backlog específico

- [ ] Nuevas temáticas (refranes, música, cine, naturaleza, comidas).
- [ ] Reto diario opcional.
- [ ] Tableros 12×12 como dificultad avanzada.
- [ ] Narración por voz de la lista de palabras.
- [ ] Temas de alto contraste adicionales.
- [ ] Selección por arrastre como preferencia.

---

## Definición de terminado (por tarea)

1. Funciona en el editor con la temática y el nivel reales.
2. No introduce errores ni advertencias.
3. Cumple la accesibilidad de `spec.md`.
4. Se probó el caso feliz y al menos un caso límite.
5. Se registró en este plan y, si aplica, en `test.md`.

---

## Riesgos y supuestos

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Selección incompleta | Juego injugable | Corregir ruta y validación con tests unitarios |
| Tablero 10×10 con celdas pequeñas | Dificultad táctil | Celdas ≥ 30 px, toques y pruebas con usuarios |
| Texto grande desborda | Mala legibilidad | Probar 20/24/28 px y permitir crecimiento en altura |
| Generación parcial | Partida sin palabras | Reintentos y mínimo de 3 |
| Cambio de escena pierde temática | Tablero vacío | Señal oficial y tema por defecto |
| Puntaje mal calibrado | Estadísticas poco útiles | Fórmula simple y pruebas |
| Rendimiento en gama baja | Mala experiencia | Medir FPS y dibujo simple |
