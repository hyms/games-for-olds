# Dirección de Arte — Clásicos Mente Activa 2D (v0.1)

> **Documento SDD global** de estilo visual.
> Extraído de la plantilla aprobada por el usuario. Se aplica a todos los
> minijuegos y pantallas, empezando por la Sopa de Letras.
>
> **Versión:** 0.1 · **Fecha:** 27/10/2026 · **Estado:** Plantilla aprobada
> **Documentación relacionada:** [Spec global](spec.md) · [Plan global](plan.md) ·
> [Plan de la Sopa](../minigames/word_search/plan.md) ·
> [Manifiesto de assets](../assets/asset-manifest.json)
>
> **Plantilla de referencia:** `assets/reference/wathersoup.jpg` (copia de
> `tmp/wathersoup.jpg`, aportada por el usuario el 27/10/2026).

---

## 1. Marco técnico

- **Motor:** Godot 4.7, 2D, renderizado `mobile`.
- **Viewport:** 360×640 lógicos, estirado `canvas_items`, orientación vertical.
- **Plataforma:** Android (gama media/baja), escritorio para desarrollo.
- **Escala de la plantilla:** la imagen mide 574×1024 px y equivale a nuestro
  viewport 360×640. **Factor de conversión: ÷ 1,594** (1 px de plantilla =
  0,627 px lógicos).
- **Filtrado:** vecino más cercano (nearest), sin mipmaps. El proyecto ya fija
  `default_texture_filter=0`.

---

## 2. Sistema visual

### 2.1 Lenguaje de formas

- Esquinas redondeadas pequeñas y consistentes (radio ≈ 5 px lógicos).
- Bordes de 2–3 px lógicos con dos tonos: claro arriba/izquierda, oscuro
  abajo/derecha (bisel sutil tipo “pixel art limpio”).
- Sombra dura y pequeña (2–3 px lógicos) sin difuminado.
- Contornos oscuros y contundentes; nunca sombras largas ni degradados.

### 2.2 Paleta

| Rol | Hex | Uso |
|-----|-----|-----|
| Fondo arena | `#b5ad96` | Fondo general de pantallas |
| Crema celda | `#fbf3da` | Relleno de celdas del tablero |
| Crema placa | `#ead9bb` | Interior de placas y modales |
| Tinta | `#101010` | Letras y texto sobre claros |
| Gris borde | `#838a94` / `#686f75` / `#535456` | Bordes de placas y celdas |
| Púrpura primario | `#9944c6` | Botón principal, píldora de categoría |
| Amarillo acento | `#fdc109` | Acciones mágicas (pista) |
| Turquesa acento | `#099c92` | Confirmación, estado encontrado |
| Cian acento | `#08d0df` | Acciones de sistema (refrescar, salir) |
| Azul trazo | `#6095fb` | Selección en curso (línea de arrastre) |
| Naranja pista | `#f9881e` | Palabra resaltada por pista |

### 2.3 Contraste verificado (WCAG 2.1)

| Par | Ratio | ¿Cumple? |
|-----|-------|----------|
| Tinta `#101010` sobre celda `#fbf3da` | 17,2:1 | Sí |
| Tinta sobre placa `#ead9bb` | 13,7:1 | Sí |
| Tinta sobre púrpura `#9944c6` | 5,3:1 | Sí (texto blanco: 5,3:1) |
| Tinta sobre amarillo `#fdc109` | 11,6:1 | Sí |
| **Negro** sobre turquesa `#099c92` | 5,6:1 | Sí (**blanco: 3,4:1, prohibido**) |
| **Negro** sobre cian `#08d0df` | 10,1:1 | Sí (**blanco: 1,9:1, prohibido**) |
| **Negro** sobre azul trazo `#6095fb` | 6,5:1 | Sí (**blanco: 2,9:1, prohibido**) |
| Negro sobre naranja `#f9881e` | 7,8:1 | Sí |
| Borde placa claro `#838a94` sobre fondo | 1,6:1 | No → usar `#535456` (3,4:1) |

**Reglas derivadas:**

- Texto sobre púrpura: blanco. Sobre turquesa, cian, azul y amarillo: negro.
- El borde visible de placas y celdas usa siempre el tono oscuro `#535456`.
- La sombra `#b7b199` sobre el fondo arena es casi invisible (1,0:1): es
  decorativa; la delimitación real la hace el borde oscuro.

---

## 3. Geometría (plantilla → lógico)

| Elemento | Plantilla (px) | Lógico (px) | Notas |
|----------|----------------|-------------|-------|
| Celda | 65 × 65 | 41 × 41 | En 8×8; en 15×15 baja a ≈ 20 y requiere zoom |
| Separación entre celdas | 10 | 6 | Proporción celda:separación ≈ 6,5:1 |
| Paso de celda | 75 | 47 | Tamaño celda + separación |
| Margen lateral | 30 | 19 | El tablero queda centrado |
| Radio de esquina | 8 | 5 | Igual en celdas, placas y botones |
| Placa de encabezado | 460 × 222 | 288 × 139 | Cabecera superior |
| Botón de navegación | 56 × 56 (plantilla) | **56 × 56** | Ver nota de accesibilidad |

> **Nota de accesibilidad:** en la plantilla los botones de navegación miden
> 56 px de un ancho de 574 px, es decir ≈ 35 px lógicos. El requisito del
> producto es **56×56 px lógicos** (≈ 90 px de la plantilla). Los botones se
> construirán más grandes de lo que aparecen en el template; el resto de la
> proporción se mantiene.

---

## 4. Componentes

### 4.1 Celda del tablero

- Relleno crema `#fbf3da`, radio 5 px, borde inferior/derecho `#535456`,
  brillo superior de 1 px `#ffffff` a baja opacidad, sombra dura de 2 px.
- Letra centrada en tinta `#101010`, alto contraste.

### 4.2 Estados de la celda

| Estado | Señal visual | Complemento no cromático |
|--------|--------------|--------------------------|
| Reposo | Crema con borde oscuro | — |
| Seleccionada (trazo en curso) | Relleno azul trazo `#6095fb` | Trazo continuo de 4 px sobre la celda |
| Encontrada | Relleno crema + borde grueso turquesa `#099c92` | Marca “✔” y subrayado permanente |
| Pista | Borde naranja `#f9881e` con parpadeo 3 s | Animación de pulso |
| Selección inválida | Parpadeo breve y desvanecido (0,5 s) | Sin color rojo y sin penalización |

### 4.3 Placa de encabezado

- Marco gris de 3 tonos con interior `#ead9bb` y sombra dura.
- Uso en la partida: nombre de la categoría, nivel y contador de palabras;
  el tiempo transcurre en una línea pequeña, nunca como cuenta atrás.

### 4.4 Botón cuadrado (navegación)

- 56×56 px lógicos, esquinas redondeadas, borde `#535456`, bisel claro arriba
  y sombra dura abajo.
- Icono blanco centrado de 28 px.
- Colores por familia: púrpura (acciones principales), amarillo (pista),
  turquesa (búsqueda/confirmación), cian (sistema).

### 4.5 Píldora de categoría

- Fondo púrpura `#9944c6`, borde oscuro, texto blanco centrado.

### 4.6 Traza de selección y pista

- Traza de arrastre: barra azul `#6095fb` redondeada de 4 px sobre la celda.
- Pista: línea naranja `#f9881e` con el mismo grosor.

---

## 5. Tipografía

- **Letras del tablero:** fuente pixel (pendiente de elegir; debe cubrir
  A–Z mayúsculas; el juego normaliza sin tildes ni Ñ).
- **Interfaz:** sans-serif legible (Atkinson Hyperlegible, recomendada en
  `assets/fonts/README.md`).
- Tamaños lógicos: Normal 20 px, Grande 24 px, Muy grande 28 px.

---

## 6. Producción de assets

- **No se recorta la plantilla JPEG.** Es un mockup con artefactos de compresión
  y chrome embebido; se usa solo como referencia de estilo.
- Celdas, placas y botones se construyen con `StyleBoxFlat` o NinePatch limpios
  usando la paleta de §2.2.
- Iconos: PNG con alfa, trazo blanco de 2 px, generados o de un set libre.
- Todos los assets quedan listados en
  [`assets/asset-manifest.json`](../assets/asset-manifest.json) con estado,
  tamaño, origen y licencia.

---

## 7. Configuración de importación

| Ajuste | Valor |
|--------|-------|
| Filtro de textura | `Nearest` |
| Mipmaps | Desactivados |
| Compresión | Sin pérdida (PNG) |
| Color space | sRGB |
| Alpha | Sí en iconos; no en fondos |

---

## 8. Validación

1. Comparar la pantalla de partida con `assets/reference/wathersoup.jpg` a
   resolución nativa.
2. Verificar que el tablero completo se ve en 360×640, 20:9 y tablet.
3. Verificar ≥ 56×56 px lógicos en todos los botones.
4. Medir contrastes con la tabla de §2.3 en los tres tamaños de texto.
5. Confirmar que ningún estado depende solo del color.

---

## 9. Procedencia

| Elemento | Origen | Licencia | Notas |
|----------|--------|----------|-------|
| Plantilla `assets/reference/wathersoup.jpg` | Aportada por el usuario (27/10/2026) | Uso interno como referencia | No se incluye en la exportación |
| Paleta y geometría | Medidas del template con Pillow | — | Valores exactos en §2 y §3 |
| Componentes implementados | Autoría propia (StyleBox/PNG) | Proyecto | Se registran en el manifiesto |
