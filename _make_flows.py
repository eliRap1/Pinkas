"""Generates wpf-flow.png, web-flow.png, arch-flow.png using PIL only.

Soft Structuralism palette: paper #FAFAF7, ink #0B0B0F, accent #3B5BFF,
mint #10B981, amber #F59E0B, rose #EF4444. Plus Jakarta Sans falls back
to system sans-serif on Pillow's default."""

import os
from PIL import Image, ImageDraw, ImageFont

OUT = r"D:\yudb"
W, H = 1600, 900
PAPER = (250, 250, 247)
INK = (11, 11, 15)
INK_SOFT = (110, 110, 130)
ACCENT = (59, 91, 255)
MINT = (16, 185, 129)
AMBER = (245, 158, 11)
ROSE = (239, 68, 68)
GREY = (180, 180, 180)


def font(size, bold=False):
    paths = [
        r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf",
        r"C:\Windows\Fonts\segoeuib.ttf" if bold else r"C:\Windows\Fonts\segoeui.ttf",
    ]
    for p in paths:
        if os.path.exists(p):
            try: return ImageFont.truetype(p, size)
            except Exception: pass
    return ImageFont.load_default()


def card(draw, x, y, w, h, label, color=ACCENT, sub=""):
    r = 18
    draw.rounded_rectangle((x, y, x + w, y + h), r, fill=PAPER, outline=color, width=2)
    draw.rounded_rectangle((x + 4, y + 4, x + w - 4, y + h - 4), r - 4, outline=(230, 230, 230), width=1)
    f1 = font(20, bold=True)
    f2 = font(13)
    draw.text((x + 16, y + 14), label, font=f1, fill=INK)
    if sub:
        draw.text((x + 16, y + 42), sub, font=f2, fill=INK_SOFT)


def arrow(draw, x1, y1, x2, y2, color=ACCENT, label=""):
    draw.line((x1, y1, x2, y2), fill=color, width=2)
    # arrowhead
    import math
    ang = math.atan2(y2 - y1, x2 - x1)
    L = 10
    p1 = (x2 - L * math.cos(ang - math.pi / 7), y2 - L * math.sin(ang - math.pi / 7))
    p2 = (x2 - L * math.cos(ang + math.pi / 7), y2 - L * math.sin(ang + math.pi / 7))
    draw.polygon((x2, y2, p1[0], p1[1], p2[0], p2[1]), fill=color)
    if label:
        mx, my = (x1 + x2) // 2, (y1 + y2) // 2 - 8
        f = font(11)
        bbox = draw.textbbox((mx, my), label, font=f)
        draw.rectangle((bbox[0] - 4, bbox[1] - 2, bbox[2] + 4, bbox[3] + 2), fill=PAPER)
        draw.text((mx, my), label, font=f, fill=INK_SOFT)


def title(draw, t, sub):
    f1 = font(36, bold=True)
    f2 = font(16)
    draw.text((40, 30), t, font=f1, fill=INK)
    draw.text((40, 76), sub, font=f2, fill=INK_SOFT)


# ============================ ARCH ===========================
img = Image.new("RGB", (W, H), PAPER)
d = ImageDraw.Draw(img)
title(d, "B-Managed — Architecture", "WCF service · Access (.accdb) · 2 clients · multi-currency")
card(d, 600, 200, 400, 100, "WCF Service1",       ACCENT, "BasicHttpBinding · localhost:8744")
card(d, 100, 200, 380, 100, "BManagedClient (WPF)", MINT, "owner / employee / client roles")
card(d, 1120, 200, 380, 100, "BManagedWeb (Razor)", MINT, "Razor Pages · Tailwind · agency UI")
card(d, 600, 420, 400, 100, "ViewDB layer", ACCENT, "BaseDB · parameterized OleDb · INNER JOIN reports")
card(d, 600, 580, 400, 100, "BusinessLogic", AMBER, "VAT calc · invoice numberer · PDF builder")
card(d, 600, 740, 400, 100, "BManaged.accdb", ROSE, "10 tables · seeded admin · ILS↔USD rates")

arrow(d, 290, 250, 600, 250, MINT, "WCF SOAP")
arrow(d, 1300, 250, 1000, 250, MINT, "WCF SOAP")
arrow(d, 800, 300, 800, 420, ACCENT, "Service1 → ViewDB")
arrow(d, 800, 520, 800, 580, AMBER, "report ops")
arrow(d, 800, 680, 800, 740, ROSE, "OleDb")
img.save(os.path.join(OUT, "arch-flow.png"))

# ============================ WPF ===========================
img = Image.new("RGB", (W, H), PAPER)
d = ImageDraw.Draw(img)
title(d, "B-Managed WPF — Page navigation", "frame-based; role guard at every page")
card(d, 700, 130, 240, 70, "LogIn", ACCENT, "PBKDF2 + role detect")

card(d, 220, 280, 300, 70, "OwnerHome", MINT, "DispatcherTimer 15s")
card(d, 660, 280, 300, 70, "EmployeeHome", AMBER, "assigned projects")
card(d, 1080, 280, 300, 70, "ClientHome", ROSE, "minimal landing")

# Owner spokes
spokes = [
    (40,  430, "Customers"),    (260, 430, "Projects"),
    (480, 430, "Invoices"),     (700, 430, "Expenses"),
    (920, 430, "Reports"),      (1140, 430, "Settings"),
    (1360, 430, "Notifications"),
]
for x, y, lbl in spokes:
    card(d, x, y, 200, 60, lbl, ACCENT)
    arrow(d, 370, 350, x + 100, y, ACCENT)

arrow(d, 820, 200, 370, 280, MINT, "Owner")
arrow(d, 820, 200, 810, 280, AMBER, "Employee")
arrow(d, 820, 200, 1230, 280, ROSE, "Client")
img.save(os.path.join(OUT, "wpf-flow.png"))

# ============================ WEB ===========================
img = Image.new("RGB", (W, H), PAPER)
d = ImageDraw.Draw(img)
title(d, "B-Managed Web — Razor Pages", "session-based role guard · setInterval polling")
card(d, 700, 130, 240, 70, "/Login", ACCENT, "session role")

card(d, 220, 280, 300, 70, "Owner/Home", MINT, "asymmetrical bento")
card(d, 660, 280, 300, 70, "Employee/Home", AMBER, "")
card(d, 1080, 280, 300, 70, "Client/Portal", ROSE, "")

own = [(40, 430, "Customers"), (260, 430, "Projects"), (480, 430, "Invoices"),
       (700, 430, "Expenses"), (920, 430, "Reports")]
for x, y, lbl in own:
    card(d, x, y, 200, 60, "Owner/" + lbl, ACCENT)
    arrow(d, 370, 350, x + 100, y, ACCENT)

card(d, 1140, 430, 220, 60, "Employee/Projects", AMBER)
card(d, 1140, 510, 220, 60, "Employee/Expenses", AMBER)
arrow(d, 810, 350, 1250, 450, AMBER)
arrow(d, 810, 350, 1250, 530, AMBER)

card(d, 1140, 620, 220, 60, "Client/InvoiceView", ROSE)
arrow(d, 1230, 350, 1250, 620, ROSE)

arrow(d, 820, 200, 370, 280, MINT, "Owner")
arrow(d, 820, 200, 810, 280, AMBER, "Employee")
arrow(d, 820, 200, 1230, 280, ROSE, "Client")
img.save(os.path.join(OUT, "web-flow.png"))

print("done")
