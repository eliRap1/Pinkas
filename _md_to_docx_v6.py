"""Build the Hebrew project book .docx from the v6 markdown.
Pure python-docx, no pandoc. Handles headings, tables, lists, code, paragraphs."""
import re
from docx import Document
from docx.shared import Pt, RGBColor, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

SRC = r"D:\yudb\ספר_פרויקט_B_Managed_FINAL_v6.md"
OUT = r"D:\yudb\ספר_פרויקט_B_Managed_FINAL_v6.docx"

doc = Document()

# Set document defaults: RTL Hebrew, A4
section = doc.sections[0]
section.page_height = Inches(11.69)
section.page_width  = Inches(8.27)
section.left_margin = section.right_margin = Inches(0.8)
section.top_margin  = section.bottom_margin = Inches(0.8)

style = doc.styles["Normal"]
style.font.name = "Calibri"
style.font.size = Pt(11)
# Hebrew shaping support
rPr = style.element.get_or_add_rPr()
rFonts = rPr.find(qn("w:rFonts"))
if rFonts is None:
    rFonts = OxmlElement("w:rFonts")
    rPr.append(rFonts)
rFonts.set(qn("w:cs"), "David")
rFonts.set(qn("w:ascii"), "Calibri")
rFonts.set(qn("w:hAnsi"), "Calibri")

def set_rtl(p):
    pPr = p._p.get_or_add_pPr()
    bidi = OxmlElement("w:bidi")
    pPr.append(bidi)

def add_heading(text, level=1):
    p = doc.add_heading(text, level=level)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    set_rtl(p)
    return p

def add_para(text, bold=False, color=None):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    set_rtl(p)
    r = p.add_run(text)
    r.bold = bold
    if color:
        r.font.color.rgb = color
    return p

def add_code(text):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    pPr = p._p.get_or_add_pPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear"); shd.set(qn("w:color"), "auto"); shd.set(qn("w:fill"), "F2F2EE")
    pPr.append(shd)
    r = p.add_run(text)
    r.font.name = "Consolas"
    r.font.size = Pt(9)
    r.font.color.rgb = RGBColor(0x0B, 0x0B, 0x0F)
    return p

def add_table(rows):
    """rows = list of list of cells. First row is header."""
    if not rows: return
    t = doc.add_table(rows=len(rows), cols=len(rows[0]))
    t.style = "Light Grid Accent 1"
    for i, r in enumerate(rows):
        for j, c in enumerate(r):
            cell = t.rows[i].cells[j]
            cell.text = str(c)
            for p in cell.paragraphs:
                p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
                set_rtl(p)
                if i == 0:
                    for run in p.runs:
                        run.bold = True
    return t

# ---------------- Parse markdown ----------------
with open(SRC, "r", encoding="utf-8") as f:
    md = f.read()

lines = md.split("\n")
i = 0
in_code = False
code_buf = []
table_buf = []
in_table = False

def flush_code():
    global code_buf
    if code_buf:
        add_code("\n".join(code_buf))
        code_buf = []

def flush_table():
    global table_buf, in_table
    # parse markdown table (skip separator row of dashes)
    parsed = []
    for row in table_buf:
        cells = [c.strip() for c in row.strip("|").split("|")]
        if all(re.match(r"^-+:?$|^:?-+:?$", c.replace(" ", "")) for c in cells if c):
            continue
        parsed.append(cells)
    if parsed:
        add_table(parsed)
    table_buf = []
    in_table = False

while i < len(lines):
    line = lines[i]

    if line.strip().startswith("```"):
        if not in_code:
            flush_table()
            in_code = True
        else:
            in_code = False
            flush_code()
        i += 1
        continue
    if in_code:
        code_buf.append(line)
        i += 1
        continue

    if line.lstrip().startswith("|") and "|" in line[2:]:
        flush_code()
        table_buf.append(line)
        in_table = True
        i += 1
        continue
    elif in_table:
        flush_table()

    m = re.match(r"^(#+)\s+(.+)$", line.strip())
    if m:
        flush_code(); flush_table()
        level = min(len(m.group(1)), 4)
        add_heading(m.group(2).strip(), level=level)
        i += 1
        continue

    if line.strip().startswith("---"):
        i += 1
        continue

    if line.strip().startswith("* ") or line.strip().startswith("- "):
        flush_code()
        text = re.sub(r"^[\s]*[\*\-]\s+", "", line)
        p = doc.add_paragraph(text, style="List Bullet")
        p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        set_rtl(p)
        i += 1
        continue

    if re.match(r"^\d+\.\s", line.strip()):
        flush_code()
        text = re.sub(r"^\d+\.\s+", "", line.strip())
        p = doc.add_paragraph(text, style="List Number")
        p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        set_rtl(p)
        i += 1
        continue

    if line.strip() == "":
        i += 1
        continue

    # plain paragraph — strip inline backticks markers but keep text
    text = line.strip()
    text = re.sub(r"`([^`]+)`", r"\1", text)
    text = re.sub(r"\*\*([^*]+)\*\*", r"\1", text)
    text = re.sub(r"\*([^*]+)\*", r"\1", text)
    add_para(text)
    i += 1

flush_code()
flush_table()

doc.save(OUT)
print("wrote", OUT)
