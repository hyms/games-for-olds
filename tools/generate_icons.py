#!/usr/bin/env python3
"""Genera los iconos pixel-art de la interfaz (assets/icons/*.png).

Rejilla base 16x16 -> PNG final 32x32 (escala x2 con vecino más cercano,
para que Godot lo muestre nítido con `default_texture_filter=0`).

Estilo: relleno blanco con contorno tinta de 1 px (visible sobre cualquier
color de familia: púrpura, amarillo, turquesa, cian o crema).

Uso:  python3 tools/generate_icons.py
Salida: assets/icons/{star,wand,magnifier,refresh,lock,pause,menu,restart,back,check}.png
"""

from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw

SIZE = 16
SCALE = 2
FILL = (255, 255, 255, 255)
OUTLINE = (16, 16, 16, 255)

OUT_DIR = Path(__file__).resolve().parent.parent / "assets" / "icons"


def _mask(draw_fn) -> Image.Image:
    """Dibuja la silueta en blanco sobre transparente y devuelve la máscara."""
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(img))
    return img


def _outline(img: Image.Image) -> Image.Image:
    """Añade contorno de 1 px alrededor de la silueta existente."""
    px = img.load()
    cells = {(x, y) for y in range(SIZE) for x in range(SIZE) if px[x, y][3] > 0}
    ring = set()
    for x, y in cells:
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                n = (x + dx, y + dy)
                if 0 <= n[0] < SIZE and 0 <= n[1] < SIZE and n not in cells:
                    ring.add(n)
    for x, y in ring:
        px[x, y] = OUTLINE
    return img


def _line(d: ImageDraw.ImageDraw, p0, p1, width: int = 2) -> None:
    d.line([p0, p1], fill=FILL, width=width)


# --- Iconos ------------------------------------------------------------------

def star() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        cx, cy, r = 8.0, 8.0, 7.0
        pts = []
        for i in range(10):
            ang = -math.pi / 2 + i * math.pi / 5
            rad = r if i % 2 == 0 else r * 0.42
            pts.append((cx + rad * math.cos(ang), cy + rad * math.sin(ang)))
        d.polygon(pts, fill=FILL)

    return _mask(draw)


def wand() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        _line(d, (3, 12), (10, 5), 2)  # asta
        # destello en la punta (cruz)
        _line(d, (11, 2), (13, 4), 1)
        _line(d, (13, 2), (11, 4), 1)
        d.point((12, 6), fill=FILL)
        d.point((10, 3), fill=FILL)

    return _mask(draw)


def magnifier() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        d.ellipse([2, 2, 10, 10], outline=FILL, width=2)
        _line(d, (10, 10), (13, 13), 2)

    return _mask(draw)


def _arc_with_head(box, start: float, end: float, width: int = 2):
    """Devuelve una función que dibuja un arco con punta de flecha al final."""

    def draw(d: ImageDraw.ImageDraw) -> None:
        d.arc(box, start=start, end=end, fill=FILL, width=width)
        cx = (box[0] + box[2]) / 2.0
        cy = (box[1] + box[3]) / 2.0
        r = (box[2] - box[0]) / 2.0 - width / 2.0
        ang = math.radians(end)
        px, py = cx + r * math.cos(ang), cy + r * math.sin(ang)
        tx, ty = -math.sin(ang), math.cos(ang)  # tangente en sentido horario
        nx, ny = math.cos(ang), math.sin(ang)   # normal hacia fuera
        s = 3.2
        tip = (px + tx * s * 1.3, py + ty * s * 1.3)
        left = (px - tx * s * 0.4 + nx * s, py - ty * s * 0.4 + ny * s)
        right = (px - tx * s * 0.4 - nx * s, py - ty * s * 0.4 - ny * s)
        d.polygon([tip, left, right], fill=FILL)

    return draw


def refresh() -> Image.Image:
    # Casi círculo completo con pequeño hueco a la izquierda; gira en sentido horario.
    return _mask(_arc_with_head([2, 2, 13, 13], start=200, end=180))


def lock() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        d.arc([4, 2, 11, 9], start=180, end=360, fill=FILL, width=2)  # ojal
        d.rectangle([3, 8, 12, 14], fill=FILL)  # cuerpo
        d.point((7, 10), fill=OUTLINE)  # cerradura (se rellena luego con contorno)

    return _mask(draw)


def pause() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        d.rectangle([4, 3, 6, 12], fill=FILL)
        d.rectangle([9, 3, 11, 12], fill=FILL)

    return _mask(draw)


def menu() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        for y in (3, 7, 11):
            d.rectangle([2, y, 13, y + 1], fill=FILL)

    return _mask(draw)


def restart() -> Image.Image:
    # Círculo con hueco arriba y punta apuntando a la derecha (reintentar).
    return _mask(_arc_with_head([2, 2, 13, 13], start=350, end=280))


def back() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        _line(d, (4, 8), (13, 8), 2)
        _line(d, (4, 8), (9, 3), 2)
        _line(d, (4, 8), (9, 13), 2)

    return _mask(draw)


def check() -> Image.Image:
    def draw(d: ImageDraw.ImageDraw) -> None:
        _line(d, (2, 8), (6, 12), 2)
        _line(d, (6, 12), (13, 4), 2)

    return _mask(draw)


ICONS = {
    "star": star,
    "wand": wand,
    "magnifier": magnifier,
    "refresh": refresh,
    "lock": lock,
    "pause": pause,
    "menu": menu,
    "restart": restart,
    "back": back,
    "check": check,
}


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, fn in ICONS.items():
        img = _outline(fn())
        big = img.resize((SIZE * SCALE, SIZE * SCALE), Image.NEAREST)
        path = OUT_DIR / f"{name}.png"
        big.save(path)
        print(f"{path.relative_to(OUT_DIR.parent.parent)} {big.size[0]}x{big.size[1]}")


if __name__ == "__main__":
    main()
