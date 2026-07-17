#!/usr/bin/env python3
"""Genera el documento Word del Examen Final para Zoapex.

Parte del documento pristino de la PA4 (PA3 + PA4), elimina todos los
bloques de código (identificados por fuente Consolas) dejando solo las
indicaciones de captura, y agrega la sección "Evaluación Final" con el
mismo criterio: sin código pegado, solo explicación + qué capturar.
"""

from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor

BASE = Path(__file__).resolve().parent
PA4_SRC = BASE.parent / "pa4" / "send" / "Zoapex-PA4.docx"
OUT_DIR = BASE / "send"
OUT_FILE = OUT_DIR / "Zoapex-ExamenFinal.docx"

COLOR_TITLE = RGBColor(0x55, 0x55, 0x55)
COLOR_HEADING = RGBColor(0x2F, 0x5D, 0x8A)
COLOR_BODY = RGBColor(0x22, 0x22, 0x22)
FONT = "Arial"


def set_run(run, *, size=11, bold=False, color=COLOR_BODY, italic=False):
    run.font.name = FONT
    run._element.rPr.rFonts.set(qn("w:eastAsia"), FONT)
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = color


def add_paragraph(doc, text="", *, size=11, bold=False, color=COLOR_BODY, space_after=6, align=None):
    p = doc.add_paragraph()
    if align:
        p.alignment = align
    p.paragraph_format.space_after = Pt(space_after)
    if text:
        run = p.add_run(text)
        set_run(run, size=size, bold=bold, color=color)
    return p


def add_heading(doc, text, level=1):
    sizes = {1: 15, 2: 13, 3: 12}
    return add_paragraph(doc, text, size=sizes.get(level, 12), bold=True, color=COLOR_HEADING, space_after=8)


def add_body(doc, text, space_after=6):
    return add_paragraph(doc, text, size=11, color=COLOR_BODY, space_after=space_after)


def add_bullet(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(4)
    p.paragraph_format.left_indent = Inches(0.25)
    run = p.add_run("• " + text)
    set_run(run, size=11, color=COLOR_BODY)
    return p


def add_placeholder(doc, text):
    return add_paragraph(doc, text, size=10, bold=True, color=RGBColor(0xE7, 0x6F, 0x51), space_after=12)


# ---------------------------------------------------------------------
# Limpieza: elimina bloques de código (fuente Consolas) del documento
# base de la PA4, dejando intactas las captions y los placeholders de
# captura que ya indican qué pantalla/archivo capturar.
# ---------------------------------------------------------------------

def _is_code_paragraph(p):
    return len(p.runs) > 0 and p.runs[0].font.name == "Consolas"


def _is_visually_empty(p):
    has_drawing = bool(p._element.findall(".//" + qn("w:drawing")))
    return not has_drawing and not p.text.strip()


def strip_code_blocks(doc: Document):
    paragraphs = doc.paragraphs
    to_remove = []
    i, n = 0, len(paragraphs)
    while i < n:
        if _is_code_paragraph(paragraphs[i]):
            j = i
            while j < n and _is_code_paragraph(paragraphs[j]):
                to_remove.append(paragraphs[j])
                j += 1
            # el separador en blanco que add_code_block agregaba al final del bloque
            if j < n and len(paragraphs[j].runs) == 0:
                to_remove.append(paragraphs[j])
                j += 1
            i = j
        else:
            i += 1
    for p in to_remove:
        p._element.getparent().remove(p._element)
    return len(to_remove)


# Puntos donde el código ya fue eliminado (por el alumno o por strip_code_blocks)
# y no quedó ninguna captura real insertada: se completa con UNA indicación clara.
KNOWN_GAPS = [
    ("Archivo: Zoapex.Web/Data/ZoapexDbContext.cs",
     "[CAPTURA: pantalla del archivo ZoapexDbContext.cs en Visual Studio / Cursor]"),
    ("Archivo: Zoapex.Web/Data/CatalogRepository.cs — filtro del catálogo",
     "[CAPTURA: método GetCatalogAsync en CatalogRepository.cs]"),
    ("Archivo: Zoapex.Web/Data/OrderRepository.cs — historial del cliente",
     "[CAPTURA: método GetCustomerOrdersAsync en OrderRepository.cs]"),
    ("2) Invocación desde el proyecto web (EF Core + SqlQuery)",
     "[CAPTURA: método RegisterOrderAsync en OrderRepository.cs]"),
]


def fill_known_gaps(doc: Document):
    filled = 0
    for caption_text, placeholder_text in KNOWN_GAPS:
        paragraphs = doc.paragraphs
        n = len(paragraphs)
        for i, p in enumerate(paragraphs):
            if p.text.strip() != caption_text:
                continue
            j = i + 1
            first_blank, extra = None, []
            while j < n and _is_visually_empty(paragraphs[j]):
                if first_blank is None:
                    first_blank = paragraphs[j]
                else:
                    extra.append(paragraphs[j])
                j += 1
            if first_blank is not None:
                run = first_blank.add_run(placeholder_text)
                set_run(run, size=10, bold=True, color=RGBColor(0xE7, 0x6F, 0x51))
                for p_rm in extra:
                    p_rm._element.getparent().remove(p_rm._element)
                filled += 1
            break
    return filled


def collapse_blank_runs(doc: Document):
    """Elimina corridas de 2+ párrafos vacíos consecutivos (ya sin código ni
    captura) para que no queden huecos en blanco. Deja intactos los blancos
    sueltos (separadores originales entre secciones) y cualquier párrafo con
    imagen."""
    paragraphs = doc.paragraphs
    to_remove = []
    i, n = 0, len(paragraphs)
    while i < n:
        if _is_visually_empty(paragraphs[i]):
            j = i
            while j < n and _is_visually_empty(paragraphs[j]):
                j += 1
            if j - i >= 2:
                to_remove.extend(paragraphs[i:j])
            i = j
        else:
            i += 1
    for p in to_remove:
        p._element.getparent().remove(p._element)
    return len(to_remove)


def build_examen_final(doc: Document):
    doc.add_page_break()

    # Portada de la sección (nombre, proyecto y fecha)
    add_paragraph(doc, "EVALUACIÓN FINAL", size=10, bold=True, color=COLOR_TITLE, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "Cierra tu proyecto: Transaccionalidad Web + Reporte a Excel",
                  size=18, bold=True, color=COLOR_HEADING, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "Desarrollo de Aplicaciones Básico", size=11, color=COLOR_BODY, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "Alumno:  Stephano Camarena Villa", size=11, color=COLOR_BODY, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "Proyecto:  Zoapex — Tienda de mascotas", size=11, color=COLOR_BODY, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "Fecha de entrega:  16 de julio de 2026", size=11, color=COLOR_BODY, align=WD_ALIGN_PARAGRAPH.CENTER)
    add_paragraph(doc, "", space_after=10)

    add_body(
        doc,
        "Esta sección cierra el ciclo del dato del proyecto Zoapex aplicando el flujo guardar → "
        "mostrar → exportar sobre la misma solución de la PA3/PA4. Se implementa una operación "
        "transaccional maestro-detalle en la web, un reporte de ventas con análisis y la descarga de "
        "ese reporte a Excel con EPPlus. Se respeta la regla de oro: la vista nunca consulta la base "
        "de datos directamente, siempre pasa por la capa de datos (repositorios y el servicio de "
        "exportación).",
    )

    # ---- Parte A ----
    add_heading(doc, "A. Escenario transaccional en la web (maestro-detalle, 'todo o nada')", level=2)
    add_body(
        doc,
        "Qué se guarda: una Venta / Pedido con estructura cabecera-detalle: la tabla order (cabecera) "
        "y order_detail (varias líneas). La operación se graba 'todo o nada': si una línea falla, no "
        "se inserta nada. La cabecera, todos los detalles y el descuento de stock se ejecutan dentro "
        "de la misma transacción del procedimiento almacenado en PostgreSQL (fn_register_order). La "
        "vista (Pages/Cart.cshtml.cs) nunca toca la base de datos: llama a "
        "OrderRepository.RegisterOrderAsync, y ese método invoca la función fn_register_order.",
    )
    add_placeholder(doc, "[CAPTURA A-1: código de OrderRepository.cs — método RegisterOrderAsync completo]")
    add_placeholder(doc, "[CAPTURA A-2: función fn_register_order en el Editor SQL de Supabase o en el archivo zoapex-db/sql/zoapex_database.sql]")
    add_placeholder(doc, "[CAPTURA A-3: carrito confirmando la compra y mensaje '¡Pedido registrado correctamente!' en Mis pedidos]")
    add_placeholder(doc, "[CAPTURA A-4: registros resultantes en las tablas order y order_detail, con el stock del producto ya descontado]")

    # ---- Parte B ----
    add_heading(doc, "B. Reporte y descarga a Excel con EPPlus", level=2)

    add_heading(doc, "B.1. El reporte en la web (mostrar)", level=3)
    add_body(
        doc,
        "Creé una página nueva /SalesReport ('Reporte de ventas') que presenta el dato como análisis, "
        "tabla y gráfico (no como texto crudo). Muestra cuatro indicadores (KPIs): número de pedidos, "
        "unidades vendidas, ticket promedio y ventas totales; un gráfico de barras con el Top 5 de "
        "productos por ingreso; una tabla de ventas por fecha; y una tabla con el detalle de pedidos.",
    )
    add_placeholder(doc, "[CAPTURA B-1: página /SalesReport con los KPIs, el gráfico de barras y las tablas]")

    add_heading(doc, "B.2. Cómo llegué al reporte (explicado con mis palabras)", level=3)
    add_body(
        doc,
        "Partí del dato que ya guardo en la Parte A: los pedidos (order) con sus líneas (order_detail). "
        "Me pregunté qué necesita un administrador para entender el negocio y decidí no mostrar los "
        "pedidos 'en crudo', sino resumirlos y analizarlos. Primero armé una sola consulta LINQ "
        "(GetSalesReportAsync) que trae los pedidos activos; a partir de esa información calculé los "
        "cuatro KPIs, siendo el ticket promedio las ventas totales divididas entre el número de "
        "pedidos. Luego quise responder '¿qué se vende más?', así que agrupé las líneas por producto y "
        "sumé sus ingresos para obtener el Top 5, que muestro como gráfico de barras (cada barra es "
        "proporcional al ingreso del producto respecto al mayor). Para ver la evolución en el tiempo, "
        "agrupé los mismos pedidos por fecha y sumé sus totales. Finalmente dejé una tabla con el "
        "detalle de pedidos para revisar caso por caso. Así el reporte pasa de 'texto crudo' a un "
        "tablero con análisis, gráfico y tablas, y ese mismo dato es el que después se descarga en Excel.",
    )

    add_heading(doc, "B.3. La descarga a Excel con EPPlus (exportar)", level=3)
    add_body(
        doc,
        "El botón 'Descargar Excel' apunta al handler OnGetExportAsync, que genera y descarga un "
        "archivo .xlsx real. La respuesta usa el content-type de Office y un nombre de archivo con "
        "fecha y hora. El armado del libro (dos hojas: 'Resumen' con KPIs y gráfico, y 'Detalle de "
        "pedidos' con autofiltro y fila de totales) vive en Data/SalesExcelExporter.cs, y la licencia "
        "de uso académico se configura una sola vez en Program.cs.",
    )
    add_placeholder(doc, "[CAPTURA B-2: código de SalesReport.cshtml.cs — método OnGetExportAsync]")
    add_placeholder(doc, "[CAPTURA B-3: código de SalesExcelExporter.cs — método Build (armado de hojas y gráfico con EPPlus)]")
    add_placeholder(doc, "[CAPTURA B-4: Excel descargado y abierto — hojas 'Resumen' y 'Detalle de pedidos']")

    add_heading(doc, "B.4. La query adjunta (LINQ que alimenta el reporte)", level=3)
    add_body(
        doc,
        "Consulta LINQ de agregación en la capa de datos (Data/OrderRepository.cs, método "
        "GetSalesReportAsync). Trae los pedidos con EF Core y calcula los KPIs, el top de productos "
        "(agrupando por producto y sumando cantidad e ingreso) y las ventas por fecha.",
    )
    add_placeholder(doc, "[CAPTURA B-5: código de GetSalesReportAsync (LINQ) en OrderRepository.cs — la consulta que alimenta el reporte]")

    # ---- Parte C ----
    add_heading(doc, "C. Seguridad (opcional — puntos extra)", level=2)
    add_bullet(doc, "Roles Admin / Cliente: columna 'role' en customer; el rol se guarda en la sesión al iniciar sesión.")
    add_bullet(doc, "Reglas de acceso: la página y el handler de exportación solo los ve el Administrador (guardia CustomerSession.IsAdmin); el enlace 'Reporte' del menú solo aparece para Admin.")
    add_bullet(doc, "Formulario de Login (/Account/Login) con validación de credenciales y hash de contraseña (PasswordHelper).")
    add_bullet(doc, "Caducidad de sesión (timeout): IdleTimeout de 30 minutos configurado en Program.cs.")
    add_body(doc, "Cuentas demo — Admin: admin@zoapex.com / zoapex123 · Cliente: cliente@zoapex.com / zoapex123.", space_after=8)
    add_placeholder(doc, "[CAPTURA C-1: menú 'Reporte' visible con la cuenta Admin, y acceso denegado / redirección a Login con la cuenta Cliente]")

    # ---- Verificación ----
    add_heading(doc, "Verificación realizada (de punta a punta)", level=2)
    add_bullet(doc, "dotnet build → compila sin errores.")
    add_bullet(doc, "Login como admin@zoapex.com → /SalesReport responde HTTP 200 con KPIs, gráfico y tablas.")
    add_bullet(doc, "Botón Descargar Excel → descarga un .xlsx real (hojas 'Resumen' y 'Detalle de pedidos', con gráfico y autofiltro).")
    add_bullet(doc, "Acceso sin sesión a /SalesReport y al handler Export → HTTP 302 hacia el Login (regla de acceso por rol funcionando).")

    # ---- Entregable ----
    add_heading(doc, "Entregable", level=2)
    add_bullet(doc, "Proyecto actualizado en .zip (carpeta zoapex-web/), o video corto: guardar carrito → ver reporte → descargar Excel.")
    add_bullet(doc, "Este documento (con capturas) adjunto al de la PA3/PA4.")
    add_bullet(doc, "Excel de muestra generado por el propio sistema: reporte-ventas-ejemplo.xlsx.")


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    if PA4_SRC.exists():
        doc = Document(str(PA4_SRC))
    else:
        doc = Document()
        add_paragraph(doc, "Zoapex — Documento del proyecto", size=20, bold=True,
                      color=COLOR_HEADING, align=WD_ALIGN_PARAGRAPH.CENTER)

    removed = strip_code_blocks(doc)
    print(f"Párrafos de código eliminados de la sección PA4: {removed}")

    filled = fill_known_gaps(doc)
    print(f"Huecos completados con indicación de captura: {filled}")

    collapsed = collapse_blank_runs(doc)
    print(f"Párrafos en blanco sobrantes eliminados: {collapsed}")

    build_examen_final(doc)
    doc.save(str(OUT_FILE))
    print(f"Generado: {OUT_FILE}")


if __name__ == "__main__":
    main()
