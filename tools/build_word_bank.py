#!/usr/bin/env python3
"""Genera el banco de palabras de la Sopa de Letras (10 categorías, ≥600 palabras).

Escribe `resources/themes/themes/*.tres` (recurso `WordSearchTheme`) con las 10
categorías de la spec §5.1, en su orden. Reglas validadas antes de escribir
(decisión C9 y spec §5):

- 10 categorías con `category_id` único (= nombre del archivo).
- Palabras normalizadas: mayúsculas A–Z, sin tildes, diéresis ni Ñ (ESPAÑA → ESPANA).
- Longitud 4–15 letras (tablero mínimo 8×8, máximo 15×15).
- Sin duplicados dentro de la categoría NI entre categorías (evita que dos
  grupos combinados en la misma partida repitan palabra).
- ≥60 palabras válidas por categoría y ≥600 en total.

Uso:  python3 tools/build_word_bank.py
AVISO: es una herramienta de generación puntual. Si más adelante editas los
`.tres` a mano (desde el editor), NO la re-ejecutes: sobrescribiría los cambios.
"""

from __future__ import annotations

import unicodedata
from pathlib import Path

MIN_WORDS = 60
MIN_LEN = 4
MAX_LEN = 15
MIN_TOTAL = 600

ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / "resources" / "themes" / "themes"
SCRIPT_PATH = "res://resources/themes/WordSearchTheme.gd"

# uid conservados de los 5 recursos originales (Godot ya los conocía).
EXISTING_UIDS = {
    "animales.tres": "uid://d2xvkwm1yq5bp",
    "frutas_verduras.tres": "uid://c7ypmxn2kq8vh",
    "objetos.tres": "uid://xjv4p8k5m3qh",
    "paises_ciudades.tres": "uid://b8qkm3xp4w7th",
    "profesiones.tres": "uid://h6k9n3xv2p8t",
}

# Orden = spec §5.1. category_id = nombre del archivo.
CATEGORIES: list[dict] = [
    {
        "id": "animales",
        "name": "Animales y Naturaleza",
        "desc": "Animales domésticos y salvajes, bosques, mares y montañas",
        "words": [
            # Animales
            "PERRO", "GATO", "VACA", "CABALLO", "OVEJA", "CERDO", "PATO", "GALLINA",
            "CONEJO", "TORTUGA", "POLLITO", "BURRO", "ELEFANTE", "DELFIN", "TIGRE",
            "LEON", "MONO", "ARDILLA", "LOBO", "ZORRO", "RATA", "RATON", "CIERVO",
            "JIRAFA", "CEBRA", "HIENA", "COYOTE", "PAVO", "PALOMA", "GORRION",
            "CUERVO", "AGUILA", "HALCON", "PINGUINO", "SERPIENTE", "LAGARTO", "RANA",
            "SAPO", "IGUANA", "CAMALEON", "ARANA", "ABEJA", "MOSCA", "PULPO",
            "CALAMAR", "MEDUSA", "CABRA", "BALLENA", "ORCA", "TIBURON", "FOCO",
            "PAJARO", "MARIPOSA",
            # Naturaleza
            "BOSQUE", "SELVA", "LAGO", "PLAYA", "DESIERTO", "MONTANA", "VOLCAN",
            "ISLA", "CUEVA", "NUBES", "ORILLA", "DUNA", "CORRIENTE",
        ],
    },
    {
        "id": "plantas",
        "name": "Plantas y Flores",
        "desc": "Flores, árboles, hierbas aromáticas y jardines",
        "words": [
            # Flores
            "ROSA", "GIRASOL", "ORQUIDEA", "TULIPAN", "CLAVEL", "VIOLETA", "LIRIO",
            "AZUCENA", "JAZMIN", "DAHLIA", "PETUNIA", "BEGONIA", "MARGARITA",
            "ANEMONA", "VERBENA", "LAVANDA",
            # Árboles y plantas
            "PINO", "CACTUS", "EUCALIPTO", "OLIVO", "CIPRES", "PALMERA", "ABETO",
            "ACEBO", "ARBUSTO", "LAUREL", "HELECHO", "MUSGO", "LIQUEN", "CEREZO",
            "MANZANO", "PERAL", "LIMONERO", "NARANJERO", "CEDRO", "HIGUERA", "SAUCO",
            "SABINA", "TEJO", "ALAMO", "SAUCE", "TILLO",
            # Hierbas y partes
            "ROMERO", "TOMILLO", "ORIGANO", "ALBAHACA", "MENTA", "PEREJIL", "SALVIA",
            "FLOR", "FLORES", "PLANTA", "HOJA", "HOJAS", "RAIZ", "TALLO", "BULBO",
            "PETALO", "POLLEN", "NECTAR", "ARBOL", "ARBOLEDA", "RAMA", "TRONCO",
            "SEMILLA", "PASTO",
        ],
    },
    {
        "id": "frutas_verduras",
        "name": "Frutas y Verduras",
        "desc": "Frutas de temporada y verduras de la huerta",
        "words": [
            # Frutas
            "MANZANA", "PLATANO", "MANGO", "PERA", "CEREZA", "FRESA", "NARANJA",
            "LIMON", "BANANA", "SANDIA", "MELON", "PINA", "KIWI", "PAPAYA", "GUAYABA",
            "GRANADA", "MELOCOTON", "CIRUELA", "DAMASCO", "DATIL", "COCO", "ARANDANO",
            "FRAMBUESA", "MORA", "MARACUYA", "ACEITUNA", "ALBARICOQUE", "NISPERO",
            "CAQUI", "CHIRIMOYA", "POMELO",
            # Verduras
            "ZANAHORIA", "TOMATE", "PEPINO", "CALABAZA", "CEBOLLA", "PATATA",
            "BROCOLI", "ESPINACA", "LECHUGA", "REPOLLO", "PIMENTON", "CHAMPINON",
            "JUDIA", "GUISANTE", "HABA", "ARVEJA", "CALABACIN", "ALCACHOFA",
            "BERENJENA", "ESPARRAGO", "EJOTE", "ELOTE", "YUCA", "CAMOTE", "BATATA",
            "NABO", "RABANO", "ACEDERA", "ESCAROLA", "APIO", "PUERRO", "COLIFLOR",
            "CACAHUATE", "CHAYOTE",
        ],
    },
    {
        "id": "objetos",
        "name": "Cosas del Hogar",
        "desc": "Muebles, utensilios y enseres de casa",
        "words": [
            # Muebles
            "MESA", "SILLA", "MUEBLE", "SILLON", "BUTACA", "ARMARIO", "ESTANTE",
            "CAJON", "DESPENSA", "VITRINA", "CABECERO", "CAMA",
            # Utensilios de cocina
            "CUCHARA", "CUCHARON", "CUCHILLO", "TENEDOR", "PLATO", "VASO", "TAZA",
            "OLLA", "SARTEN", "JARRA", "BANDEJA", "TAPADERA", "ESPUMADERA",
            "SERVILLETA", "MANTEL",
            # Limpieza y baño
            "CEPILLO", "TOALLA", "ESCOBA", "TRAPEADOR", "BANERA", "DUCHA", "GRIFO",
            "LAVADORA", "JABONERA", "REJILLA", "TENDEDERO",
            # Textiles y decoración
            "SABANA", "COBERTOR", "ALMOHADA", "ALMOHADON", "MANTA", "COLCHA", "FUNDA",
            "ESPEJO", "ALFOMBRA", "CORTINA", "PERSIANA", "TAPETE",
            # Enseres y detalles
            "LAMPARA", "RELOJ", "CANDADO", "CERRADURA", "LLAVE", "LINTERNA", "VELA",
            "CENICERO", "PAPELERA", "REFRIGERADOR", "TELEVISION", "MICROONDAS",
            "HORNO", "ESTUFA", "PLANCHA", "SECADOR", "CESTA", "CANASTO", "TABLA",
            "FLORERO", "JARRON", "ALARMA", "PEINE",
        ],
    },
    {
        "id": "profesiones",
        "name": "Profesiones y Oficios",
        "desc": "Trabajos, oficios y profesiones de toda la vida",
        "words": [
            # Sanidad, enseñanza y derecho
            "MEDICO", "ENFERMERA", "FARMACEUTICO", "DENTISTA", "VETERINARIO",
            "MASAJISTA", "MAESTRO", "PROFESOR", "ABOGADO", "NOTARIO", "FISCAL",
            # Oficios manuales
            "CARPINTERO", "MECANICO", "ELECTRICISTA", "PLOMERO", "PINTOR", "ESCULTOR",
            "CERRAJERO", "CERAMISTA", "SASTRE", "COSTURERA", "ZAPATERO", "PELUQUERO",
            "BARBERO", "ALBANIL", "OBRERO", "MINERO",
            # Comercio y servicio
            "COCINERO", "PANADERO", "CARNICERO", "FRUTERO", "QUESERO", "CAMARERO",
            "CAJERO", "VENDEDOR", "REPARTIDOR", "CHOFER", "CONDUCTOR", "COBRADOR",
            "RECEPCIONISTA", "CONTADOR", "ADMINISTRADOR", "ECONOMISTA", "TECNICO",
            "TRADUCTOR", "FOTOGRAFO", "LIBRERO",
            # Campo y naturaleza
            "GRANJERO", "GANADERO", "PESCADOR", "JARDINERO", "AGRONOMO", "APICULTOR",
            "BOTICARIO",
            # Otros
            "ARQUITECTO", "INGENIERO", "BOMBERO", "POLICIA", "SOLDADO", "GUARDIA",
            "PILOTO", "MUSICO", "BAILARIN", "ESCRITOR", "PERIODISTA", "MONJE",
            "OBISPO", "MISIONERO", "ATLETA", "DEPORTISTA", "MAQUINISTA",
        ],
    },
    {
        "id": "paises_ciudades",
        "name": "Países y Geografía",
        "desc": "Países, capitales y lugares del mundo",
        "words": [
            # Hispanoamérica
            "MEXICO", "ARGENTINA", "CHILE", "PERU", "COLOMBIA", "VENEZUELA",
            "ECUADOR", "BOLIVIA", "PARAGUAY", "URUGUAY", "GUATEMALA", "HONDURAS",
            "SALVADOR", "NICARAGUA", "PANAMA", "CUBA", "JAMAICA", "HAITI",
            "DOMINICANA",
            # Europa y resto
            "ESPANA", "BRAZIL", "CANADA", "ALEMANIA", "FRANCIA", "ITALIA",
            "INGLATERRA", "PORTUGAL", "BELGICA", "HOLANDA", "SUIZA", "RUSIA",
            "POLONIA", "GRECIA", "TURQUIA", "CHINA", "JAPON", "COREA", "INDIA",
            "EGIPTO", "MARRUECOS",
            # Ciudades
            "LIMA", "BARCELONA", "MADRID", "SEVILLA", "VALENCIA", "BILBAO",
            "MONTEVIDEO", "MEDELLIN", "BOGOTA", "QUITO", "SANTIAGO", "CARACAS",
            "HABANA", "LISBOA", "LONDRES", "PARIS", "ROMA", "BERLIN", "MOSCU",
            "VIENA", "ATENAS", "OTTAWA",
            # Geografía
            "OCEANO", "COSTA", "VALLE", "LLANURA", "CASCADA", "GLACIAR", "LAGUNA",
        ],
    },
    {
        "id": "comidas",
        "name": "Comidas y Postres",
        "desc": "Platos cotidianos, guisos, mariscos y postres",
        "words": [
            # Platos principales
            "PASTEL", "PAELLA", "CHOCOLATE", "EMPANADA", "GALLETA", "ARROZ",
            "ENSALADA", "SANDWICH", "HAMBURGUESA", "PIZZA", "TACOS", "BURRITO",
            "AREPA", "TAMALES", "RAMEN", "SUSHI", "PASTA", "LASANA", "GNOCCHI",
            "TORTILLA", "BOCADILLO", "PICADILLO",
            # Guisos y legumbres
            "LENTEJAS", "GARBANZOS", "FIDEO", "SOPA", "CALDO", "GUISO", "ESTOFADO",
            "MINESTRA", "GAZPACHO", "MOJETE", "CAZUELA", "MIGAS", "TAPAS", "FIDEUA",
            "FABADA", "PUCHERO", "COCIDO", "SALMOREJO",
            # Carnes, pescados y lácteos
            "JAMON", "QUESO", "CARNE", "POLLO", "PESCADO", "CAMARON", "LANGOSTINO",
            "ATUN", "SALMON", "MERLUZA", "SARDINA", "ANCHOVA", "CALAMARES", "CHULETA",
            "MARISCO", "GAMBAS", "MEJILLONES", "ALMEJAS", "BOQUERON",
            # Postres y bebidas
            "NATILLAS", "CREMA", "TIRAMISU", "HELADO", "SORBETE", "TARTA", "MILHOJA",
            "CHURROS", "BUNUELO", "TORRIJA", "BIZCOCHO", "POLVORON", "TRUFAS",
            "CARLOTA", "SUSPIRO", "MAZAPAN", "BROWNIE", "CAFE", "LECHE", "MIEL",
        ],
    },
    {
        "id": "instrumentos",
        "name": "Instrumentos Musicales",
        "desc": "Instrumentos de orquesta, folk y percusión",
        "words": [
            # Cuerda
            "PIANO", "GUITARRA", "VIOLIN", "VIOLONCHELO", "VIOLA", "CONTRABAJO",
            "ARPA", "LAUD", "BANDURRIA", "CHARANGO", "VIHUELA", "MANDOLINA", "BANJO",
            "UKULELE", "SALTERIO", "CUERDA",
            # Viento
            "FLAUTA", "FLAUTIN", "TROMPETA", "TROMBON", "TROMPA", "TUBA", "EUFONIO",
            "SAXOFON", "CLARINETE", "OBOE", "FAGOT", "ARMONICA", "SILBATO",
            # Teclado y folk
            "ORGANO", "CLAVECIN", "ACORDEON", "BANDONEON", "MELODICA", "PIANOLA",
            "ORGANILLO", "ZANFONA", "DULZAINA", "GAITA", "REBEC", "CHIRIMIA",
            # Percusión
            "KALIMBA", "MARIMBA", "XILOFONO", "VIBRAFONO", "BONGO", "CONGA", "TIMBAL",
            "TAMBOR", "TAMBORIL", "PANDERO", "PANDERETA", "MARACAS", "CASTANUELAS",
            "CLAVES", "GUIRO", "TRIANGULO", "PLATILLOS", "BOMBO", "CAJA", "REDOBLANTE",
            "TAROLA", "GONG", "CAMPANAS", "CAMPANILLA", "CASCABELES", "ATABAL",
            "ZAMBOMBA", "BATERIA",
        ],
    },
    {
        "id": "herramientas",
        "name": "Herramientas y Bricolaje",
        "desc": "Herramientas de mano, taller y bricolaje",
        "words": [
            # Herramientas básicas
            "MARTILLO", "ALICATE", "TALADRO", "SIERRA", "CLAVO", "DESTORNILLADOR",
            "CINCEL", "GRAPA", "NIVEL", "CINTA", "REGLA", "ESCUADRA", "LISTON", "LIJA",
            "TORNILLO", "TUERCA", "ARANDELA", "BROCA", "BROCHA", "PINCEL", "RODILLO",
            "BALDE", "PERNO", "METRO", "ALAMBRE", "GANCHO", "TIJERA",
            # Taller
            "ESMERIL", "ESMERILADORA", "YUNQUE", "BIGORNIA", "LIMADORA", "TRINCHETA",
            "CIZALLA", "SOPLETE", "SOLDADOR", "CUCHILLA", "BISTURI", "MOSQUETON",
            "CALIBRADOR", "FORMON", "ESCOPLO", "PUNZON", "GUBIA", "MAZA",
            # Bricolaje y jardín
            "PEGAMENTO", "SILICONA", "ESPATULA", "HERRAMIENTA", "BARRENA", "BISEL",
            "ATORNILLADOR", "GRAPADORA", "MOLDE", "PLANTILLA", "CARRETILLA",
            "ESCALERA", "ANDAMIO", "ABRAZADERA", "TENAZA", "PINZA", "HACHA", "PICO",
            "PALA", "AZADON", "AZADA", "ESPOSA",
        ],
    },
    {
        "id": "ropa",
        "name": "Ropa y Costura",
        "desc": "Ropa, calzado y materiales de costura",
        "words": [
            # Ropa
            "CAMISA", "PANTALON", "CAMISETA", "FALDA", "BLUSA", "VESTIDO", "BUFANDA",
            "CHAQUETA", "ABRIGO", "SACO", "TRAJE", "CORBATA", "SOMBRERO", "BOINA",
            "PIJAMA", "BATA", "OVEROL", "TUNICA", "ESTOLA", "DELANTAL",
            # Calzado y accesorios
            "ZAPATO", "ZAPATILLA", "BOTAS", "SANDALIA", "BOTIN", "ESCARPIN",
            "PANTUFLA", "CHANCLETA", "ZUECOS", "ALPARGATA", "SUELA", "TACON",
            "CINTURON", "GUANTE", "GORRA", "GORRO", "PANUELO", "VELO", "MANTON",
            "REBOZO", "CHAL", "VISERA",
            # Mediciones y calzas
            "CALCETIN", "MEDIAS", "ROPA",
            # Costura
            "HILO", "AGUJA", "PUNTADA", "TELAR", "MAQUINA", "CIERRE", "BOTON",
            "COSTURA", "BORDADO", "LANA", "ALGODON", "SEDA", "TERCIOPELO", "SATEN",
            "DEDAL", "HEBILLA", "CORCHETE", "CREMALLERA", "OJAL", "AROS", "COLLAR",
            "PULSERA", "CADENA",
        ],
    },
]


def normalize(word: str) -> str:
    """A–Z mayúsculas sin tildes, diéresis ni Ñ (misma regla que el juego)."""
    word = unicodedata.normalize("NFD", word.strip().upper())
    word = "".join(ch for ch in word if not unicodedata.combining(ch))
    return "".join(ch for ch in word if "A" <= ch <= "Z")


def validate() -> None:
    errors: list[str] = []
    if len(CATEGORIES) != 10:
        errors.append(f"se esperaban 10 categorías, hay {len(CATEGORIES)}")
    seen_ids: set[str] = set()
    seen_words: dict[str, str] = {}
    total = 0

    for cat in CATEGORIES:
        cid = cat["id"]
        if cid in seen_ids:
            errors.append(f"{cid}: category_id duplicado")
        seen_ids.add(cid)
        words: list[str] = cat["words"]
        if len(words) < MIN_WORDS:
            errors.append(f"{cid}: {len(words)} palabras (mínimo {MIN_WORDS})")
        total += len(words)
        local: set[str] = set()
        for w in words:
            if w != normalize(w):
                errors.append(f"{cid}: '{w}' no está normalizada (va como {normalize(w)})")
            if not (MIN_LEN <= len(w) <= MAX_LEN):
                errors.append(f"{cid}: '{w}' tiene {len(w)} letras (permitido {MIN_LEN}-{MAX_LEN})")
            if w in local:
                errors.append(f"{cid}: '{w}' duplicada")
            local.add(w)
            if w in seen_words:
                errors.append(f"{cid}: '{w}' ya está en {seen_words[w]} (sin repetir entre categorías)")
            seen_words[w] = cid

    if total < MIN_TOTAL:
        errors.append(f"total {total} palabras (mínimo {MIN_TOTAL})")

    if errors:
        print("ERRORES:")
        for e in errors:
            print(f"  - {e}")
        raise SystemExit(1)


def write_tres(cat: dict) -> None:
    path = OUT_DIR / f"{cat['id']}.tres"
    uid = EXISTING_UIDS.get(path.name)
    header_uid = f' uid="{uid}"' if uid else ""
    words = ", ".join(f'"{w}"' for w in cat["words"])
    lines = [
        f'[gd_resource type="Resource" script_class="WordSearchTheme" load_steps=2 format=3{header_uid}]',
        "",
        f'[ext_resource type="Script" path="{SCRIPT_PATH}" id="1_bank"]',
        "",
        "[resource]",
        'script = ExtResource("1_bank")',
        f'CategoryId = "{cat["id"]}"',
        f'ThemeName = "{cat["name"]}"',
        f'Description = "{cat["desc"]}"',
        f"Words = Array[String]([{words}])",
        "",
    ]
    path.write_text("\n".join(lines), encoding="utf-8")


def main() -> None:
    validate()
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    total = 0
    for cat in CATEGORIES:
        write_tres(cat)
        total += len(cat["words"])
        print(f"  {cat['id']:<18} {len(cat['words']):>3} palabras  {cat['name']}")
    print(f"Total: {total} palabras en {len(CATEGORIES)} categorías → {OUT_DIR}")


if __name__ == "__main__":
    main()
