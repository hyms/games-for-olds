# Plan de Trabajo Global — Clásicos Mente Activa 2D

> **Documento SDD global del producto.**
> Checklist de alto nivel: qué se construye, en qué orden y cuándo se entrega.
> El detalle de cada juego vive en su propio `plan.md`.

## Leyenda

- `[x]` Hecho y verificado en Godot
- `[ ]` Pendiente
- 🔜 En curso (siguiente tarea a atacar)
- **P1** Bloqueante para la fase · **P2** Importante · **P3** Mejora

---

## Fase 0 — Base técnica del proyecto

- [x] Estructura de carpetas (autoloads, core, ui, minigames, resources, docs, tests).
- [x] Proyecto configurado en vertical 360×640, estirado `canvas_items`, orientación móvil.
- [x] Emulación de toque → ratón para probar en escritorio.
- [x] Autoload único `SceneManager` con navegación por rutas y señales tipadas.
- [x] Tema accesible compartido: tipografía del sistema, tamaños grandes, estilos de botón y panel de alto contraste.
- [x] Componente `BigButton` reutilizable (tamaño mínimo táctil 56×56, foco, grupo de accesibilidad).
- [x] Menú principal con los tres juegos y botón Salir.
- [x] Documentación SDD: documentos globales y documentos por minijuego.
- [x] Skill del proyecto (`godot4-mastery`) con guías y catálogo de errores.

---

## Fase 1 — Sopa de Letras (primer minijuego)

- [x] Especificación, plan y pruebas del minijuego en `minigames/word_search/`.
- [x] Recurso de temática editable y cinco temáticas con palabras.
- [x] Normalización de palabras y filtros de longitud.
- [x] Generador procedural de tablero con diagonales e invertidas.
- [x] Celda con estados y selección por toque inicial + final.
- [x] Validación de palabras en cualquier sentido.
- [x] Contador, botones Volver / Reiniciar / Pista y paso de temática.
- 🔜 **Tablero visual 10×10** (fase actual; detalle en [plan del juego](../minigames/word_search/plan.md)).

---

## Fase 2 — Sopa de Letras: cierre de jugabilidad

- [ ] Tablero visible y completo en todos los tamaños de pantalla.
- [ ] Lista de palabras pendientes visible y actualizada.
- [ ] Pista funcional.
- [ ] Feedback de acierto, error y victoria.
- [ ] Selección por arrastre con tolerancia (opcional).
- [ ] Reinicio y navegación sin arrastrar estado viejo.
- [ ] Pruebas del minijuego completas en editor y Android.

---

## Fase 3 — Tangram / Puzles de encaje

- [ ] Crear documentación SDD del minijuego (`spec`, `plan`, `test`).
- [ ] Escena del juego y acceso desde el menú.
- [ ] Siete piezas clásicas con estilo pixel art.
- [ ] Arrastrar, soltar, rotar por pasos y snap con tolerancia.
- [ ] Niveles como datos editables y progresión guardada.
- [ ] Pista, reiniciar y deshacer.
- [ ] Accesibilidad: piezas grandes, sin gestos finos, sin tiempo.

---

## Fase 4 — Lógica de Bloques (estilo Dr. Mario)

- [ ] Crear documentación SDD del minijuego (`spec`, `plan`, `test`).
- [ ] Escena del juego y acceso desde el menú.
- [ ] Tablero, cápsulas de dos colores, movimiento, giro y caída.
- [ ] Detección de líneas de cuatro o más, eliminación y cadenas.
- [ ] Condiciones de nivel completado y fin de partida.
- [ ] Modo relajado por defecto y modo normal.
- [ ] Pausa accesible y colores distinguibles por forma/símbolo.

---

## Fase 5 — Transversal de la app

- [ ] Pantalla de ajustes accesible desde el menú principal.
- [ ] Control de sonido y música.
- [ ] Tamaño de texto configurable.
- [ ] Contraste alto opcional.
- [ ] Velocidad de Bloques configurable.
- [ ] Guardado local de preferencias.
- [ ] Guardado local de progreso por juego.
- [ ] Tutorial breve en el primer acceso a cada juego.
- [ ] Botón de ayuda dentro de cada juego.
- [ ] Centralización de textos para futuras traducciones.
- [ ] Transiciones suaves entre escenas (fade).

---

## Fase 6 — Calidad y entrega

### 6.1 Pruebas globales (P1)

- [ ] Ejecutar los planes de prueba de los tres juegos.
- [ ] Ejecutar el plan global de pruebas en editor y Android.
- [ ] Probar en pantallas 16:9, 18:9, 19.5:9 y 20:9.
- [ ] Prueba de usabilidad con usuarios del perfil objetivo.
- [ ] Revisión de consola: cero errores y cero advertencias nuevas.

### 6.2 Rendimiento y estabilidad (P1)

- [ ] Medir FPS en gama baja; objetivo 60 fps estables.
- [ ] Medir tiempo de arranque; objetivo < 3 s.
- [ ] Comprobar consumo de memoria en sesiones largas.
- [ ] Verificar ausencia de fugas al cambiar de escena repetidamente.

### 6.3 Exportación Android (P1)

- [ ] Instalar plantillas de exportación.
- [ ] Configurar preset Android (paquete, icono, versión, orientación vertical).
- [ ] Generar APK de prueba e instalar en dispositivo.
- [ ] Verificar permisos mínimos y funcionamiento offline.
- [ ] Preparar build firmado para distribución.

### 6.4 Publicación (P2)

- [ ] Ficha del producto (nombre, descripción, capturas, icono).
- [ ] Política de privacidad mínima (sin datos recogidos).
- [ ] Canal de distribución elegido y primer build publicado.
- [ ] Plan de actualizaciones (nuevas temáticas, niveles y juegos).

---

## Backlog global

- [ ] Nuevos juegos clásicos (dominó, memori, crucigramas sencillos, sudoku fácil).
- [ ] Nuevas temáticas para Sopa de Letras.
- [ ] Más niveles para Tangram y Bloques.
- [ ] Reto diario opcional sin presión de tiempo.
- [ ] Modo multijugador local por turnos.
- [ ] Narración de instrucciones por voz.
- [ ] Temas de alto contraste y ajustes de daltonismo.
- [ ] Estadísticas personales simples.
- [ ] Versión para tablets con áreas de juego mayores.
- [ ] iOS.

---

## Definición de terminado (global)

Un juego se considera terminado cuando:

1. Su `spec.md` está implementado y verificado.
2. Su `test.md` se ejecutó completo en editor y Android.
3. No quedan defectos P0 ni P1 abiertos.
4. Cumple los principios de accesibilidad globales.
5. Está integrado en el menú y se puede jugar de principio a fin.
6. Está reflejado en este plan y en su plan específico.

---

## Riesgos y supuestos

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Tablero 10×10 con celdas pequeñas para dedos | Dificultad táctil | Selección por toques; celdas contiguas; validar con usuarios |
| El cambio de escena pierde datos entre pantallas | Flujo roto | Señal oficial de cambio de escena y valores por defecto |
| Crecimiento del alcance (más juegos, más ajustes) | Retrasos | Congelar el MVP en tres juegos y priorizar accesibilidad |
| Tipado estricto de GDScript ralentiza el avance | Retrasos | Seguir el skill del proyecto y su catálogo de errores |
| Rendimiento en gama baja | Mala experiencia | Medir FPS, dibujo simple, evitar efectos costosos |
| El editor regenera UIDs al importar | Ruido en git | Commitear los UID generados una vez estabilizados |

---

## Gobernanza SDD

- Cada minijuego nace con `spec.md`, `plan.md` y `test.md` **antes** de escribir
  lógica.
- Los documentos globales resumen y enlazan; no duplican reglas de juego.
- Toda tarea completada se marca en el plan correspondiente.
- Todo defecto encontrado se registra con su severidad y su caso de prueba.
- Un cambio de reglas se refleja primero en el `spec` y luego en el código.
