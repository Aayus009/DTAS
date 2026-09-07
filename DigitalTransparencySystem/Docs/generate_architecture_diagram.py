"""Generate the DTAS system architecture diagram as SVG and PNG."""
from __future__ import annotations

import os
from xml.sax.saxutils import escape

from PIL import Image, ImageDraw, ImageFont

OUT_DIR = os.path.dirname(os.path.abspath(__file__))
W, H = 1600, 2050
SCALE = 3

BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
BAND = (245, 245, 245)

TIMES = r"C:\Windows\Fonts\times.ttf"
TIMES_BD = r"C:\Windows\Fonts\timesbd.ttf"


def font(size: int, bold: bool = False):
    path = TIMES_BD if bold else TIMES
    return ImageFont.truetype(path, size)


class Box:
    def __init__(self, x, y, w, h, lines, bold=False):
        self.x = x
        self.y = y
        self.w = w
        self.h = h
        self.lines = lines if isinstance(lines, (list, tuple)) else [lines]
        self.bold = bold

    @property
    def cx(self):
        return self.x + self.w / 2.0

    @property
    def cy(self):
        return self.y + self.h / 2.0

    @property
    def bottom(self):
        return self.y + self.h

    @property
    def top(self):
        return self.y


def row_boxes(x0, y, total_w, h, groups, gap=22, bold=False):
    n = len(groups)
    bw = (total_w - gap * (n - 1)) / float(n)
    boxes = []
    x = x0
    for lines in groups:
        boxes.append(Box(x, y, bw, h, lines, bold=bold))
        x += bw + gap
    return boxes


def vline(edges, x, y1, y2):
    edges.append(("v", x, y1, y2))


def hline(edges, x1, x2, y):
    edges.append(("h", y, x1, x2))


def build():
    x0, content_w = 190, 1360
    boxes = []
    bands = []
    labels = []
    edges = []
    captions = []

    def add_band(title, y, h):
        bands.append((40, y, 1520, h, title))

    # Layer 1: Clients
    y = 118
    add_band("1. Clients", y, 148)
    labels.append((52, y + 24, "Web browser"))
    roles = row_boxes(x0, y + 52, content_w, 62, [
        ["Guest"],
        ["Student"],
        ["Faculty"],
        ["Staff"],
        ["Admin"],
    ], bold=True)
    boxes.extend(roles)
    captions.append((x0, y + 124, content_w, "Signed-out guest uses public pages. Signed-in roles use a portal."))

    # Layer 2: Presentation
    y = 300
    add_band("2. Presentation", y, 236)
    labels.append((52, y + 20, "IIS host"))
    host = Box(x0, y + 44, content_w, 44, ["IIS / IIS Express hosts ASP.NET Web Forms 4.7.2 and Site.Master"], bold=True)
    boxes.append(host)
    portals = row_boxes(x0, y + 110, content_w, 86, [
        ["Public site", "Home, FAQ, sign-in popup"],
        ["User portal", "Dashboard, workspaces,", "assignments, meetings"],
        ["Admin portal", "Users, identity review,", "poll results"],
        ["Transparency", "Public gist of events,", "decisions, progress"],
    ])
    boxes.extend(portals)
    captions.append((x0, y + 206, content_w, "Site.Master shows public chrome only when the user is not signed in."))

    # Layer 3: Access
    y = 570
    add_band("3. Access control", y, 126)
    labels.append((52, y + 20, "Every page"))
    access = row_boxes(x0, y + 42, content_w, 58, [
        ["AccessGuard", "Session and page gate"],
        ["Identity check", "ID required for modules"],
        ["RoleAccess", "Admin, Faculty, Staff, Student"],
    ], bold=True)
    boxes.extend(access)

    # Layer 4: Application
    y = 730
    add_band("4. Application services", y, 214)
    labels.append((52, y + 20, "Helpers"))
    svc_top = row_boxes(x0, y + 44, content_w, 66, [
        ["AuthService", "Password, Google, OTP"],
        ["EventService", "Create and propose events"],
        ["AssignmentService", "Faculty assignments, groups"],
        ["MeetingService", "Zoom meetings, recordings"],
    ])
    svc_bot = row_boxes(x0, y + 126, content_w, 66, [
        ["Task workspaces", "Works, teams, updates"],
        ["ConnectService", "Group messaging"],
        ["NotificationService", "In-app notices"],
        ["Identity / moderation", "Verify, restrict, restore"],
    ])
    boxes.extend(svc_top)
    boxes.extend(svc_bot)

    # Layer 5: Data
    y = 978
    add_band("5. Data store", y, 168)
    labels.append((52, y + 20, "SQL Server"))
    db = Box(x0, y + 42, content_w, 50, ["DigitalTransparencyDB on SQL Server Express (parameterized SqlClient)"], bold=True)
    boxes.append(db)
    tables = row_boxes(x0, y + 108, content_w, 40, [
        ["Users, Roles"],
        ["Events, Meetings"],
        ["Decisions, Tasks"],
        ["Assignments, Polls"],
        ["Connect, Audit"],
    ])
    boxes.extend(tables)

    # Layer 6: External
    y = 1180
    add_band("6. External systems", y, 120)
    labels.append((52, y + 20, "Outbound"))
    ext = row_boxes(x0, y + 42, content_w, 56, [
        ["Google OAuth", "Sign-in code flow"],
        ["Zoom API", "Online meeting, recordings"],
        ["Gmail SMTP", "OTP and email notices"],
    ], bold=True)
    boxes.extend(ext)

    # Notes (no em dash)
    notes_y = 1336
    captions.append((x0, notes_y, content_w,
                     "Request path: Browser to IIS to Web Forms page to AccessGuard to a helper service to SQL Server."))
    captions.append((x0, notes_y + 28, content_w,
                     "Outbound path: AuthService calls Google. MeetingService calls Zoom. Notification and OTP call SMTP."))
    captions.append((x0, notes_y + 56, content_w,
                     "Governance flow: Event to Meeting to Decision to workspace Task to public Transparency gist."))
    captions.append((x0, notes_y + 84, content_w,
                     "Academic assignments sit beside that path. System admin does not create assignments or events."))

    # One vertical spine down the centre. No side branches, so lines never cross.
    cx = x0 + content_w / 2.0
    pairs = [
        (roles[2].bottom, host.top),
        (host.bottom, portals[1].top),
        (portals[1].bottom + 28, access[1].top),
        (access[1].bottom, svc_top[1].top),
        (svc_bot[1].bottom + 22, db.top),
    ]
    for y1, y2 in pairs:
        if y2 > y1 + 8:
            vline(edges, cx, y1, y2)

    return {
        "boxes": boxes,
        "bands": bands,
        "labels": labels,
        "edges": edges,
        "captions": captions,
        "title": "DTAS System Architecture",
        "subtitle": "Digital Transparency and Accountability System. ASP.NET Web Forms 4.7.2, C#, Microsoft SQL Server.",
    }


def write_svg(model, path):
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<svg xmlns="http://www.w3.org/2000/svg" width="%d" height="%d" viewBox="0 0 %d %d">' % (W, H, W, H),
        "<style>",
        "  .t { font-family: 'Times New Roman', Times, serif; fill: #000000; }",
        "  .title { font-size: 28px; font-weight: bold; }",
        "  .sub { font-size: 14px; }",
        "  .lab { font-size: 13px; font-weight: bold; }",
        "  .cap { font-size: 13px; }",
        "  .box { font-size: 14px; }",
        "  .boxb { font-size: 14px; font-weight: bold; }",
        "</style>",
        '<rect x="0" y="0" width="%d" height="%d" fill="#ffffff"/>' % (W, H),
        '<text class="t title" x="%d" y="48" text-anchor="middle">%s</text>' % (W / 2, escape(model["title"])),
        '<text class="t sub" x="%d" y="76" text-anchor="middle">%s</text>' % (W / 2, escape(model["subtitle"])),
    ]

    for x, y, w, h, title in model["bands"]:
        parts.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" fill="#f5f5f5" stroke="#000000" stroke-width="1"/>' % (x, y, w, h))
        parts.append('<text class="t lab" x="%.1f" y="%.1f">%s</text>' % (x + 12, y + 18, escape(title)))

    for kind, *rest in model["edges"]:
        if kind == "v":
            x, y1, y2 = rest
            parts.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="#000000" stroke-width="1.4"/>' % (x, y1, x, y2))
            parts.append('<polygon points="%.1f,%.1f %.1f,%.1f %.1f,%.1f" fill="#000000"/>' % (x, y2, x - 5, y2 - 9, x + 5, y2 - 9))
        else:
            y, x1, x2 = rest
            if x2 < x1:
                x1, x2 = x2, x1
            parts.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="#000000" stroke-width="1.4"/>' % (x1, y, x2, y))

    for b in model["boxes"]:
        parts.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" fill="#ffffff" stroke="#000000" stroke-width="1.4"/>' % (b.x, b.y, b.w, b.h))
        cls = "t boxb" if b.bold else "t box"
        n = len(b.lines)
        start = b.cy - (n - 1) * 8
        for i, line in enumerate(b.lines):
            parts.append('<text class="%s" x="%.1f" y="%.1f" text-anchor="middle">%s</text>' % (cls, b.cx, start + i * 16, escape(line)))

    for x, y, text in model["labels"]:
        parts.append('<text class="t cap" x="%.1f" y="%.1f">%s</text>' % (x, y + 36, escape(text)))

    for x, y, w, text in model["captions"]:
        parts.append('<text class="t cap" x="%.1f" y="%.1f">%s</text>' % (x, y, escape(text)))

    parts.append("</svg>")
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(parts))


def write_png(model, path):
    img = Image.new("RGB", (W * SCALE, H * SCALE), WHITE)
    d = ImageDraw.Draw(img)
    f_title = font(28 * SCALE, True)
    f_sub = font(14 * SCALE)
    f_lab = font(13 * SCALE, True)
    f_cap = font(13 * SCALE)
    f_box = font(14 * SCALE)
    f_boxb = font(14 * SCALE, True)

    def S(v):
        return int(round(v * SCALE))

    d.text((S(W / 2), S(28)), model["title"], fill=BLACK, font=f_title, anchor="mt")
    d.text((S(W / 2), S(64)), model["subtitle"], fill=BLACK, font=f_sub, anchor="mt")

    for x, y, w, h, title in model["bands"]:
        d.rectangle([S(x), S(y), S(x + w), S(y + h)], fill=BAND, outline=BLACK, width=SCALE)
        d.text((S(x + 12), S(y + 6)), title, fill=BLACK, font=f_lab)

    for kind, *rest in model["edges"]:
        if kind == "v":
            x, y1, y2 = rest
            d.line([(S(x), S(y1)), (S(x), S(y2))], fill=BLACK, width=max(2, SCALE))
            tip = S(y2)
            d.polygon([(S(x), tip), (S(x) - 5 * SCALE, tip - 9 * SCALE), (S(x) + 5 * SCALE, tip - 9 * SCALE)], fill=BLACK)
        else:
            y, x1, x2 = rest
            d.line([(S(x1), S(y)), (S(x2), S(y))], fill=BLACK, width=max(2, SCALE))

    for b in model["boxes"]:
        d.rectangle([S(b.x), S(b.y), S(b.x + b.w), S(b.y + b.h)], fill=WHITE, outline=BLACK, width=max(2, SCALE))
        f = f_boxb if b.bold else f_box
        n = len(b.lines)
        gap = 18 * SCALE
        start = S(b.cy) - int((n - 1) * gap / 2)
        for i, line in enumerate(b.lines):
            d.text((S(b.cx), start + i * gap), line, fill=BLACK, font=f, anchor="mm")

    for x, y, text in model["labels"]:
        d.text((S(x), S(y + 24)), text, fill=BLACK, font=f_cap)

    for x, y, w, text in model["captions"]:
        d.text((S(x), S(y)), text, fill=BLACK, font=f_cap)

    img.save(path, "PNG", dpi=(300, 300))


def xml_text(value):
    return escape(value).replace("\n", "&#xa;")


def html_lines(lines, bold=False):
    parts = [xml_text(line) for line in lines]
    if bold and parts:
        parts[0] = "&lt;b&gt;%s&lt;/b&gt;" % parts[0]
    return "&lt;br&gt;".join(parts)


def write_drawio(model, path):
    cells = ['        <mxCell id="0"/>', '        <mxCell id="1" parent="0"/>']
    n = 2

    def add_vertex(value, x, y, w, h, style):
        nonlocal n
        cid = "c%d" % n
        n += 1
        cells.append(
            '        <mxCell id="%s" value="%s" style="%s" vertex="1" parent="1">'
            % (cid, value.replace('"', "&quot;"), style)
        )
        cells.append(
            '          <mxGeometry x="%.1f" y="%.1f" width="%.1f" height="%.1f" as="geometry"/>' % (x, y, w, h)
        )
        cells.append("        </mxCell>")
        return cid

    box_style = (
        "rounded=0;whiteSpace=wrap;html=1;fillColor=#FFFFFF;strokeColor=#000000;"
        "fontFamily=Times New Roman;fontColor=#000000;fontSize=14;align=center;verticalAlign=middle;"
    )
    box_bold = box_style + "fontStyle=1;"
    band_style = (
        "rounded=0;whiteSpace=wrap;html=1;fillColor=#F5F5F5;strokeColor=#000000;"
        "fontFamily=Times New Roman;fontColor=#000000;fontSize=13;fontStyle=1;"
        "align=left;verticalAlign=top;spacingLeft=12;spacingTop=4;"
    )
    title_style = (
        "text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;"
        "fontFamily=Times New Roman;fontColor=#000000;fontSize=28;fontStyle=1;"
    )
    sub_style = (
        "text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;"
        "fontFamily=Times New Roman;fontColor=#000000;fontSize=14;"
    )
    cap_style = (
        "text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;"
        "fontFamily=Times New Roman;fontColor=#000000;fontSize=13;"
    )
    edge_style = (
        "endArrow=classic;html=1;rounded=0;strokeColor=#000000;strokeWidth=1.4;"
        "fontFamily=Times New Roman;fontColor=#000000;"
    )

    add_vertex(xml_text(model["title"]), 200, 16, 1200, 40, title_style)
    add_vertex(xml_text(model["subtitle"]), 80, 56, 1440, 28, sub_style)

    for x, y, w, h, title in model["bands"]:
        add_vertex(xml_text(title), x, y, w, h, band_style)

    for b in model["boxes"]:
        value = html_lines(b.lines, bold=b.bold)
        add_vertex(value, b.x, b.y, b.w, b.h, box_bold if b.bold else box_style)

    for x, y, text in model["labels"]:
        add_vertex(xml_text(text), x, y + 20, 130, 20, cap_style)

    for x, y, w, text in model["captions"]:
        add_vertex(xml_text(text), x, y - 12, w, 22, cap_style)

    for kind, *rest in model["edges"]:
        if kind != "v":
            continue
        x, y1, y2 = rest
        cid = "c%d" % n
        n += 1
        cells.append(
            '        <mxCell id="%s" value="" style="%s" edge="1" parent="1">' % (cid, edge_style)
        )
        cells.append('          <mxGeometry relative="1" as="geometry">')
        cells.append('            <mxPoint x="%.1f" y="%.1f" as="sourcePoint"/>' % (x, y1))
        cells.append('            <mxPoint x="%.1f" y="%.1f" as="targetPoint"/>' % (x, y2))
        cells.append("          </mxGeometry>")
        cells.append("        </mxCell>")

    xml = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<mxfile host="app.diagrams.net" type="device">',
        '  <diagram id="dtas-arch" name="DTAS System Architecture">',
        '    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="%d" pageHeight="%d" math="0" shadow="0">'
        % (W, H),
        "      <root>",
    ]
    xml.extend(cells)
    xml.extend(["      </root>", "    </mxGraphModel>", "  </diagram>", "</mxfile>"])
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(xml))


def main():
    model = build()
    svg_path = os.path.join(OUT_DIR, "DTAS-System-Architecture.svg")
    png_path = os.path.join(OUT_DIR, "DTAS-System-Architecture.png")
    drawio_path = os.path.join(OUT_DIR, "DTAS-System-Architecture.drawio")
    write_drawio(model, drawio_path)
    print(drawio_path)
    try:
        write_svg(model, svg_path)
        print(svg_path)
    except OSError as err:
        print("SVG skipped:", err)
    try:
        write_png(model, png_path)
        print(png_path)
    except OSError as err:
        print("PNG skipped:", err)


if __name__ == "__main__":
    main()
