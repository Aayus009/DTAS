# Black-and-white UML activity diagrams: SVG, PNG, and draw.io.
from __future__ import print_function
import html
import os

from PIL import Image, ImageDraw, ImageFont

OUT = os.path.dirname(os.path.abspath(__file__))
LANE_W = 300
TOP = 130
ROW_H = 92
LEFT = 48
NODE_W = 214
NODE_H = 50
DEC_W = 148
DEC_H = 74
SCALE = 2

FONT_REG = r"C:\Windows\Fonts\times.ttf"
FONT_BD = r"C:\Windows\Fonts\timesbd.ttf"


def font(size, bold=False):
    path = FONT_BD if bold else FONT_REG
    try:
        return ImageFont.truetype(path, size)
    except Exception:
        return ImageFont.load_default()


class Node(object):
    def __init__(self, key, kind, label, lane, row):
        self.key = key
        self.kind = kind
        self.label = label
        self.lane = lane
        self.row = row
        self.draw_id = None

    def size(self):
        if self.kind in ("start", "end"):
            return 28, 28
        if self.kind == "decision":
            return DEC_W, DEC_H
        return NODE_W, NODE_H

    def box(self):
        w, h = self.size()
        x = LEFT + self.lane * LANE_W + (LANE_W - w) / 2.0
        y = TOP + self.row * ROW_H + (ROW_H - h) / 2.0
        return x, y, w, h

    def cx(self):
        x, y, w, h = self.box()
        return x + w / 2.0

    def cy(self):
        x, y, w, h = self.box()
        return y + h / 2.0

    def left(self):
        x, y, w, h = self.box()
        return x, self.cy()

    def right(self):
        x, y, w, h = self.box()
        return x + w, self.cy()

    def bottom(self):
        x, y, w, h = self.box()
        return self.cx(), y + h

    def top(self):
        x, y, w, h = self.box()
        return self.cx(), y

    def lines(self):
        return [ln for ln in self.label.replace("\\n", "\n").split("\n") if ln != ""]


class Edge(object):
    def __init__(self, src, tgt, label=""):
        self.src = src
        self.tgt = tgt
        self.label = label
        self.points = []


class Activity(object):
    def __init__(self, code, title, subtitle, lanes, page_w=None, page_h=None):
        self.code = code
        self.title = title
        self.subtitle = subtitle
        self.lanes = lanes
        self.nodes = {}
        self.edges = []
        self.page_w = page_w or (LEFT * 2 + len(lanes) * LANE_W)
        self.page_h = page_h or 1100

    def add(self, key, kind, label, lane, row):
        self.nodes[key] = Node(key, kind, label, lane, row)
        return key

    def link(self, src, tgt, label=""):
        self.edges.append(Edge(src, tgt, label))

    def max_row(self):
        return max(n.row for n in self.nodes.values())

    def height(self):
        return max(self.page_h, TOP + (self.max_row() + 2) * ROW_H + 70)

    def occupied_between(self, src, tgt):
        lo, hi = min(src.row, tgt.row), max(src.row, tgt.row)
        for n in self.nodes.values():
            if n is src or n is tgt:
                continue
            if n.lane == src.lane == tgt.lane and lo < n.row < hi:
                return True
        return False

    def route_all(self):
        used_mids = {}
        for i, e in enumerate(self.edges):
            e.points = self.route(e, i, used_mids)

    def exit_pt(self, node, other, label):
        lab = (label or "").lower()
        if node.kind != "decision":
            return node.bottom()
        if other.row < node.row:
            return node.left() if other.lane <= node.lane else node.right()
        if lab in ("no", "join", "dismiss", "suspend"):
            return node.left()
        if other.lane < node.lane:
            return node.left()
        if other.lane > node.lane:
            return node.right()
        return node.bottom()

    def enter_pt(self, node, other):
        x, y, w, h = node.box()
        if other.row > node.row:
            return node.bottom()
        if other.row == node.row:
            return node.left() if other.lane < node.lane else node.right()
        if other.lane < node.lane:
            return node.left()
        if other.lane > node.lane:
            return node.right()
        return node.top()

    def route(self, e, index, used_mids):
        a = self.nodes[e.src]
        b = self.nodes[e.tgt]
        ax, ay, aw, ah = a.box()
        bx, by, bw, bh = b.box()
        start = self.exit_pt(a, b, e.label)
        end = self.enter_pt(b, a)

        if b.row < a.row:
            if start[0] <= a.cx():
                rail = min(ax, bx) - 22
            else:
                rail = max(ax + aw, bx + bw) + 22
            return [start, (rail, start[1]), (rail, end[1]), end]

        if b.row == a.row:
            mid_x = (start[0] + end[0]) / 2.0
            if abs(start[1] - end[1]) < 4:
                return [start, end]
            return [start, (mid_x, start[1]), (mid_x, end[1]), end]

        blocked = self.occupied_between(a, b) or (a.lane == b.lane and b.row - a.row > 1)
        if blocked:
            if start[0] <= a.cx():
                rail = LEFT + min(a.lane, b.lane) * LANE_W + 14
            else:
                rail = LEFT + (max(a.lane, b.lane) + 1) * LANE_W - 14
            return [start, (rail, start[1]), (rail, end[1]), end]

        if abs(start[0] - end[0]) < 8:
            return [start, end]

        key = (round((start[1] + end[1]) / 2, 0), min(a.lane, b.lane), max(a.lane, b.lane))
        bump = used_mids.get(key, 0)
        used_mids[key] = bump + 1
        mid_y = (start[1] + end[1]) / 2.0 + bump * 14
        return [start, (start[0], mid_y), (end[0], mid_y), end]


def wrap_text(draw, text, max_w, fnt):
    words = text.replace("\n", " ").split()
    lines, cur = [], ""
    for w in words:
        trial = (cur + " " + w).strip()
        if draw.textlength(trial, font=fnt) <= max_w:
            cur = trial
        else:
            if cur:
                lines.append(cur)
            cur = w
    if cur:
        lines.append(cur)
    return lines or [text]


def svg_escape(text):
    return html.escape(text)


def svg_poly(pts):
    return " ".join("{0:.1f},{1:.1f}".format(x, y) for x, y in pts)


def label_anchor(pts):
    if len(pts) < 2:
        return pts[0][0] + 10, pts[0][1] - 6
    x1, y1 = pts[0]
    x2, y2 = pts[1]
    mx, my = (x1 + x2) / 2.0, (y1 + y2) / 2.0
    if abs(x2 - x1) >= abs(y2 - y1):
        return mx, my - 10
    return mx + 10, my + 4


def render_svg(act):
    act.route_all()
    w = act.page_w
    h = act.height()
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<svg xmlns="http://www.w3.org/2000/svg" width="{0}" height="{1}" viewBox="0 0 {0} {1}">'.format(w, h),
        "<style>.t{font-family:'Times New Roman',Times,serif;fill:#000}.title{font-size:20px;font-weight:bold}"
        ".sub{font-size:12px}.lane{font-size:13px;font-weight:bold}.lab{font-size:12px}.el{font-size:11px}</style>",
        '<defs><marker id="arr" markerWidth="8" markerHeight="8" refX="7" refY="3" orient="auto">'
        '<path d="M0,0 L8,3 L0,6 Z" fill="#000"/></marker></defs>',
        '<rect width="{0}" height="{1}" fill="#fff"/>'.format(w, h),
        '<text class="t title" x="{0}" y="32" text-anchor="middle">{1}</text>'.format(w / 2, svg_escape(act.title)),
        '<text class="t sub" x="{0}" y="54" text-anchor="middle">{1}</text>'.format(w / 2, svg_escape(act.subtitle)),
    ]
    lane_top = 72
    lane_h = h - lane_top - 16
    for i, name in enumerate(act.lanes):
        x = LEFT + i * LANE_W
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                x, lane_top, LANE_W, lane_h
            )
        )
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="28" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                x, lane_top, LANE_W
            )
        )
        parts.append(
            '<text class="t lane" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                x + LANE_W / 2, lane_top + 19, svg_escape(name)
            )
        )

    for e in act.edges:
        parts.append(
            '<polyline points="{0}" fill="none" stroke="#000" stroke-width="1.25" marker-end="url(#arr)"/>'.format(
                svg_poly(e.points)
            )
        )
        if e.label:
            lx, ly = label_anchor(e.points)
            parts.append(
                '<text class="t el" x="{0}" y="{1}">{2}</text>'.format(lx, ly, svg_escape(e.label))
            )

    for n in act.nodes.values():
        x, y, nw, nh = n.box()
        if n.kind == "start":
            parts.append('<circle cx="{0}" cy="{1}" r="12" fill="#000"/>'.format(n.cx(), n.cy()))
        elif n.kind == "end":
            parts.append(
                '<circle cx="{0}" cy="{1}" r="13" fill="#fff" stroke="#000" stroke-width="2"/>'.format(n.cx(), n.cy())
            )
            parts.append('<circle cx="{0}" cy="{1}" r="7" fill="#000"/>'.format(n.cx(), n.cy()))
        elif n.kind == "decision":
            cx, cy = n.cx(), n.cy()
            parts.append(
                '<polygon points="{0},{1} {2},{3} {4},{5} {6},{7}" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                    cx, y, x + nw, cy, cx, y + nh, x, cy
                )
            )
            lines = n.lines()
            start = cy - (len(lines) - 1) * 7
            for i, line in enumerate(lines):
                parts.append(
                    '<text class="t lab" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                        cx, start + i * 14, svg_escape(line)
                    )
                )
        else:
            parts.append(
                '<rect x="{0}" y="{1}" width="{2}" height="{3}" rx="14" ry="14" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                    x, y, nw, nh
                )
            )
            lines = n.lines()
            start = n.cy() - (len(lines) - 1) * 7
            for i, line in enumerate(lines):
                parts.append(
                    '<text class="t lab" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                        n.cx(), start + i * 14, svg_escape(line)
                    )
                )
    parts.append("</svg>")
    return "\n".join(parts)


def arrow_head(dr, pts, s):
    if len(pts) < 2:
        return
    x1, y1 = pts[-2]
    x2, y2 = pts[-1]
    dx, dy = x2 - x1, y2 - y1
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    px, py = -uy, ux
    p1 = (x2, y2)
    p2 = (x2 - 9 * s * ux + 5 * s * px, y2 - 9 * s * uy + 5 * s * py)
    p3 = (x2 - 9 * s * ux - 5 * s * px, y2 - 9 * s * uy - 5 * s * py)
    dr.polygon([p1, p2, p3], fill=(0, 0, 0))


def render_png(act):
    act.route_all()
    w, h = int(act.page_w), int(act.height())
    s = SCALE
    img = Image.new("RGB", (w * s, h * s), (255, 255, 255))
    dr = ImageDraw.Draw(img)
    f_title, f_sub = font(22 * s, True), font(14 * s)
    f_lane, f_lab, f_el = font(15 * s, True), font(13 * s), font(12 * s)

    def T(text, x, y, fnt, anchor="lt"):
        if anchor == "mid":
            tw = dr.textlength(text, font=fnt)
            dr.text((x - tw / 2, y), text, fill=(0, 0, 0), font=fnt)
        else:
            dr.text((x, y), text, fill=(0, 0, 0), font=fnt)

    T(act.title, (w * s) / 2, 10 * s, f_title, "mid")
    T(act.subtitle, (w * s) / 2, 38 * s, f_sub, "mid")

    lane_top = 72
    lane_h = h - lane_top - 16
    for i, name in enumerate(act.lanes):
        x = LEFT + i * LANE_W
        dr.rectangle(
            [x * s, lane_top * s, (x + LANE_W) * s, (lane_top + lane_h) * s],
            outline=(0, 0, 0),
            width=2 * s,
        )
        dr.rectangle(
            [x * s, lane_top * s, (x + LANE_W) * s, (lane_top + 28) * s],
            outline=(0, 0, 0),
            width=2 * s,
        )
        T(name, (x + LANE_W / 2) * s, (lane_top + 5) * s, f_lane, "mid")

    for e in act.edges:
        scaled = [(p[0] * s, p[1] * s) for p in e.points]
        dr.line(scaled, fill=(0, 0, 0), width=2 * s)
        arrow_head(dr, scaled, s)
        if e.label:
            lx, ly = label_anchor(e.points)
            T(e.label, lx * s, (ly - 6) * s, f_el)

    for n in act.nodes.values():
        x, y, nw, nh = n.box()
        if n.kind == "start":
            dr.ellipse(
                [(n.cx() - 12) * s, (n.cy() - 12) * s, (n.cx() + 12) * s, (n.cy() + 12) * s],
                fill=(0, 0, 0),
            )
        elif n.kind == "end":
            dr.ellipse(
                [(n.cx() - 13) * s, (n.cy() - 13) * s, (n.cx() + 13) * s, (n.cy() + 13) * s],
                outline=(0, 0, 0),
                width=2 * s,
            )
            dr.ellipse(
                [(n.cx() - 7) * s, (n.cy() - 7) * s, (n.cx() + 7) * s, (n.cy() + 7) * s],
                fill=(0, 0, 0),
            )
        elif n.kind == "decision":
            cx, cy = n.cx(), n.cy()
            dr.polygon(
                [(cx * s, y * s), ((x + nw) * s, cy * s), (cx * s, (y + nh) * s), (x * s, cy * s)],
                outline=(0, 0, 0),
                fill=(255, 255, 255),
            )
            lines = n.lines()
            total = len(lines) * 15
            ty = cy - total / 2.0
            for i, line in enumerate(lines):
                T(line, cx * s, (ty + i * 15) * s, f_lab, "mid")
        else:
            dr.rounded_rectangle(
                [x * s, y * s, (x + nw) * s, (y + nh) * s],
                radius=14 * s,
                outline=(0, 0, 0),
                width=2 * s,
                fill=(255, 255, 255),
            )
            lines = wrap_text(dr, n.label.replace("\n", " "), (nw - 16) * s, f_lab)
            if not lines:
                lines = n.lines()
            total = len(lines) * 16
            ty = n.cy() - total / 2.0
            for i, line in enumerate(lines):
                T(line, n.cx() * s, (ty + i * 16) * s, f_lab, "mid")
    return img


BW = "fillColor=#ffffff;strokeColor=#000000;fontColor=#000000;fontFamily=Times New Roman;"


def render_drawio(act):
    act.route_all()
    cells = []
    n = [2]

    def nid():
        i = "n{0}".format(n[0])
        n[0] += 1
        return i

    def cell(style, value, x, y, w, h):
        i = nid()
        cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" vertex="1" parent="1">'
            '<mxGeometry x="{3}" y="{4}" width="{5}" height="{6}" as="geometry"/></mxCell>'.format(
                i, html.escape(value), style, int(x), int(y), int(w), int(h)
            )
        )
        return i

    w, h = act.page_w, act.height()
    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=18;fontStyle=1;fontColor=#000000;",
        act.title, 20, 12, w - 40, 28,
    )
    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=12;fontColor=#000000;",
        act.subtitle, 20, 42, w - 40, 22,
    )
    lane_top = 72
    lane_h = h - lane_top - 16
    for i, name in enumerate(act.lanes):
        x = LEFT + i * LANE_W
        cell(
            "swimlane;startSize=28;horizontal=1;whiteSpace=wrap;html=1;" + BW + "fontSize=13;fontStyle=1;",
            name, x, lane_top, LANE_W, lane_h,
        )

    for node in act.nodes.values():
        x, y, nw, nh = node.box()
        if node.kind == "start":
            node.draw_id = cell(
                "ellipse;html=1;fillColor=#000000;strokeColor=#000000;fontColor=#000000;",
                "", x, y, nw, nh,
            )
        elif node.kind == "end":
            node.draw_id = cell(
                "ellipse;html=1;shape=endState;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;",
                "", x, y, nw, nh,
            )
        elif node.kind == "decision":
            node.draw_id = cell(
                "rhombus;whiteSpace=wrap;html=1;" + BW + "fontSize=11;",
                node.label.replace("\n", " "), x, y, nw, nh,
            )
        else:
            node.draw_id = cell(
                "rounded=1;whiteSpace=wrap;html=1;arcSize=30;" + BW + "fontSize=11;",
                node.label.replace("\n", " "), x, y, nw, nh,
            )

    for e in act.edges:
        i = nid()
        style = (
            "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;"
            "endArrow=block;endFill=1;html=1;strokeColor=#000000;fontColor=#000000;"
            "fontFamily=Times New Roman;fontSize=11;"
        )
        pts = e.points[1:-1] if len(e.points) > 2 else []
        if pts:
            array = "<Array as=\"points\">" + "".join(
                '<mxPoint x="{0}" y="{1}"/>'.format(int(p[0]), int(p[1])) for p in pts
            ) + "</Array>"
        else:
            array = ""
        cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" edge="1" parent="1" source="{3}" target="{4}">'
            '<mxGeometry relative="1" as="geometry">{5}</mxGeometry></mxCell>'.format(
                i, html.escape(e.label), style, act.nodes[e.src].draw_id, act.nodes[e.tgt].draw_id, array
            )
        )

    return (
        '  <diagram id="{0}" name="{1}">\n'
        '    <mxGraphModel dx="1400" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" '
        'arrows="1" fold="1" page="1" pageScale="1" pageWidth="{2}" pageHeight="{3}" math="0" shadow="0">\n'
        "      <root>\n        <mxCell id=\"0\"/>\n        <mxCell id=\"1\" parent=\"0\"/>\n"
        "{4}\n      </root>\n    </mxGraphModel>\n  </diagram>"
    ).format(act.code.lower(), html.escape(act.code), int(w), int(h), "\n".join(cells))


def mxfile(diagram_xmls):
    return (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<mxfile host="app.diagrams.net" agent="DTAS" version="22.1.0" type="device">\n'
        + "\n".join(diagram_xmls)
        + "\n</mxfile>\n"
    )


def ad1():
    a = Activity(
        "AD-1",
        "Figure AD-1. Register, verify email, and verify identity",
        "Campus modules stay locked until email and institutional ID are approved.",
        ["User", "System", "System Admin"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("reg", "action", "Register or sign in", 0, 1)
    a.add("otp", "action", "Send email OTP", 1, 2)
    a.add("vemail", "decision", "Email\nverified?", 0, 3)
    a.add("id", "action", "Upload institutional ID", 0, 4)
    a.add("queue", "action", "Place document in identity queue", 1, 5)
    a.add("rev", "action", "Review identity document", 2, 6)
    a.add("ok", "decision", "Approved?", 2, 7)
    a.add("open", "action", "Allow campus modules", 1, 8)
    a.add("deny", "action", "Keep account gated / ask again", 0, 8)
    a.add("e1", "end", "", 1, 9)
    a.add("e2", "end", "", 0, 9)
    a.link("s", "reg")
    a.link("reg", "otp")
    a.link("otp", "vemail")
    a.link("vemail", "id", "Yes")
    a.link("vemail", "otp", "No")
    a.link("id", "queue")
    a.link("queue", "rev")
    a.link("rev", "ok")
    a.link("ok", "open", "Yes")
    a.link("ok", "deny", "No")
    a.link("open", "e1")
    a.link("deny", "e2")
    return a


def ad2():
    a = Activity(
        "AD-2",
        "Figure AD-2. Student proposes an event; admin approves or rejects",
        "Faculty and staff create events directly. Students submit Status = Proposed.",
        ["Student", "System", "System Admin"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("form", "action", "Fill propose-event form\n(lead must be faculty/staff)", 0, 1)
    a.add("save", "action", "Save event as Proposed", 1, 2)
    a.add("note", "action", "Notify admin of proposal", 1, 3)
    a.add("rev", "action", "Review proposal", 2, 4)
    a.add("d", "decision", "Approve?", 2, 5)
    a.add("plan", "action", "Set status Planned\nand add event lead", 1, 6)
    a.add("rej", "action", "Set status Rejected\nwith reason", 2, 6)
    a.add("n1", "action", "Notify proposer", 1, 7)
    a.add("n2", "action", "Notify proposer", 2, 7)
    a.add("e1", "end", "", 1, 8)
    a.add("e2", "end", "", 2, 8)
    a.link("s", "form")
    a.link("form", "save")
    a.link("save", "note")
    a.link("note", "rev")
    a.link("rev", "d")
    a.link("d", "plan", "Yes")
    a.link("d", "rej", "No")
    a.link("plan", "n1")
    a.link("rej", "n2")
    a.link("n1", "e1")
    a.link("n2", "e2")
    return a


def ad3():
    a = Activity(
        "AD-3",
        "Figure AD-3. Join a public campus event",
        "Public join is a request. Invite code joins immediately.",
        ["Campus user", "System", "Event Admin / Manager"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("open", "action", "Open campus event", 0, 1)
    a.add("how", "decision", "Has invite\ncode?", 0, 2)
    a.add("code", "action", "Enter invitation code", 0, 3)
    a.add("imm", "action", "Add as Accepted member", 1, 4)
    a.add("req", "action", "Submit join request", 0, 5)
    a.add("pend", "action", "Store InviteStatus = Requested", 1, 6)
    a.add("rev", "action", "Accept or decline request", 2, 7)
    a.add("ok", "decision", "Accept?", 2, 8)
    a.add("acc", "action", "Set Accepted and notify", 1, 9)
    a.add("dec", "action", "Set Declined and notify", 2, 9)
    a.add("e1", "end", "", 1, 10)
    a.add("e2", "end", "", 2, 10)
    a.link("s", "open")
    a.link("open", "how")
    a.link("how", "code", "Yes")
    a.link("how", "req", "No")
    a.link("code", "imm")
    a.link("req", "pend")
    a.link("imm", "e1")
    a.link("pend", "rev")
    a.link("rev", "ok")
    a.link("ok", "acc", "Yes")
    a.link("ok", "dec", "No")
    a.link("acc", "e1")
    a.link("dec", "e2")
    return a


def ad4():
    a = Activity(
        "AD-4",
        "Figure AD-4. Schedule a Zoom meeting for an event",
        "DTAS creates the Zoom room, sends the join link, and closes the room when duration ends.",
        ["Host", "System", "Zoom API", "Event members"],
        page_w=LEFT * 2 + 4 * LANE_W,
    )
    a.add("s", "start", "", 0, 0)
    a.add("pick", "action", "Choose event, title,\ntime, duration", 0, 1)
    a.add("api", "action", "Create scheduled Zoom room", 2, 2)
    a.add("ok", "decision", "Room\ncreated?", 1, 3)
    a.add("err", "action", "Show Zoom error and stop", 0, 4)
    a.add("save", "action", "Save meeting and add accepted members", 1, 4)
    a.add("e2", "end", "", 0, 5)
    a.add("send", "action", "Notify members with join link", 3, 5)
    a.add("host", "action", "Start as host / members join", 0, 6)
    a.add("wait", "action", "Wait until start + duration", 1, 7)
    a.add("kill", "action", "End and delete Zoom room", 2, 8)
    a.add("close", "action", "Mark Ended, clear Start/Join", 1, 9)
    a.add("e1", "end", "", 1, 10)
    a.link("s", "pick")
    a.link("pick", "api")
    a.link("api", "ok")
    a.link("ok", "save", "Yes")
    a.link("ok", "err", "No")
    a.link("err", "e2")
    a.link("save", "send")
    a.link("send", "host")
    a.link("host", "wait")
    a.link("wait", "kill")
    a.link("kill", "close")
    a.link("close", "e1")
    return a


def ad5():
    a = Activity(
        "AD-5",
        "Figure AD-5. Create a club or request to join",
        "Admin cannot create clubs. Public join is a request. Invite code joins immediately.",
        ["Campus user", "System", "Club lead"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("ch", "decision", "Create or\njoin?", 0, 1)
    a.add("cr", "action", "Enter name and visibility", 0, 2)
    a.add("sv", "action", "Create club; user becomes lead", 1, 3)
    a.add("e2", "end", "", 1, 4)
    a.add("codeq", "decision", "Has invite\ncode?", 0, 4)
    a.add("code", "action", "Enter invitation code", 0, 5)
    a.add("imm", "action", "Add as active member", 1, 6)
    a.add("req", "action", "Request to join public club", 0, 7)
    a.add("pend", "action", "Store pending request", 1, 8)
    a.add("rev", "action", "Accept or decline", 2, 9)
    a.add("ok", "decision", "Accept?", 2, 10)
    a.add("acc", "action", "Activate membership", 1, 11)
    a.add("e1", "end", "", 1, 12)
    a.link("s", "ch")
    a.link("ch", "cr", "Create")
    a.link("ch", "codeq", "Join")
    a.link("cr", "sv")
    a.link("sv", "e2")
    a.link("codeq", "code", "Yes")
    a.link("codeq", "req", "No")
    a.link("code", "imm")
    a.link("req", "pend")
    a.link("imm", "e1")
    a.link("pend", "rev")
    a.link("rev", "ok")
    a.link("ok", "acc", "Yes")
    a.link("ok", "e1", "No")
    a.link("acc", "e1")
    return a


def ad6():
    a = Activity(
        "AD-6",
        "Figure AD-6. Faculty assignment and computed contribution",
        "Contribution is calculated (completion, timeliness, activity, recency). It is not typed in.",
        ["Faculty", "System", "Student"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("cr", "action", "Create assignment and groups", 0, 1)
    a.add("inv", "action", "Invite or place students in groups", 1, 2)
    a.add("work", "action", "Complete group tasks and submit", 2, 3)
    a.add("rec", "action", "Record completion, dates, activity", 1, 4)
    a.add("due", "decision", "Deadline\npassed?", 1, 5)
    a.add("calc", "action", "Run contribution formula", 1, 6)
    a.add("rep", "action", "Open contribution report", 0, 7)
    a.add("view", "action", "View own contribution", 2, 7)
    a.add("e", "end", "", 1, 8)
    a.link("s", "cr")
    a.link("cr", "inv")
    a.link("inv", "work")
    a.link("work", "rec")
    a.link("rec", "due")
    a.link("due", "calc", "Yes")
    a.link("due", "rec", "No")
    a.link("calc", "rep")
    a.link("calc", "view")
    a.link("rep", "e")
    a.link("view", "e")
    return a


def ad7():
    a = Activity(
        "AD-7",
        "Figure AD-7. Flag content and admin moderation",
        "Users flag content. Admin reviews the queue. Admin cannot moderate another admin.",
        ["Campus user", "System", "System Admin"],
    )
    a.add("s", "start", "", 0, 0)
    a.add("flag", "action", "Submit content flag", 0, 1)
    a.add("q", "action", "Store in content-report queue", 1, 2)
    a.add("rev", "action", "Review flag", 2, 3)
    a.add("d", "decision", "Action?", 2, 4)
    a.add("ban", "action", "Suspend or ban reported user", 0, 5)
    a.add("ok", "action", "Dismiss flag", 1, 5)
    a.add("hide", "action", "Restrict event, club, task, or decision", 2, 5)
    a.add("log", "action", "Write audit log", 1, 6)
    a.add("e", "end", "", 1, 7)
    a.link("s", "flag")
    a.link("flag", "q")
    a.link("q", "rev")
    a.link("rev", "d")
    a.link("d", "ok", "Dismiss")
    a.link("d", "hide", "Restrict")
    a.link("d", "ban", "Suspend")
    a.link("ok", "log")
    a.link("hide", "log")
    a.link("ban", "log")
    a.link("log", "e")
    return a


NAMES = {
    "AD-1": "AD1-Identity",
    "AD-2": "AD2-ProposeEvent",
    "AD-3": "AD3-JoinEvent",
    "AD-4": "AD4-MeetingZoom",
    "AD-5": "AD5-Clubs",
    "AD-6": "AD6-Assignment",
    "AD-7": "AD7-Moderation",
}

CAPTIONS = {
    "AD-1": "Figure AD-1. Register, email OTP, and institutional ID review.",
    "AD-2": "Figure AD-2. Student proposes an event. Admin approves or rejects.",
    "AD-3": "Figure AD-3. Join a public event by request or invitation code.",
    "AD-4": "Figure AD-4. Schedule a Zoom meeting, send the join link, close the room after duration.",
    "AD-5": "Figure AD-5. Create a club or request to join. Admin cannot create clubs.",
    "AD-6": "Figure AD-6. Faculty assignment. Contribution is computed, not typed in.",
    "AD-7": "Figure AD-7. Flag content and admin moderation.",
}


def write(name, data, mode="w"):
    path = os.path.join(OUT, name)
    if mode == "wb":
        with open(path, "wb") as f:
            f.write(data)
    else:
        with open(path, "w", encoding="utf-8") as f:
            f.write(data)
    print(path)
    return path


def check_overlap(act):
    seen = {}
    for n in act.nodes.values():
        key = (n.lane, n.row)
        if key in seen:
            print("OVERLAP {0}: {1} and {2} at lane {3} row {4}".format(
                act.code, seen[key], n.key, n.lane, n.row
            ))
        seen[key] = n.key


def main():
    diagrams = [ad1(), ad2(), ad3(), ad4(), ad5(), ad6(), ad7()]
    xmls = []
    files = []
    for act in diagrams:
        check_overlap(act)
        names = NAMES[act.code]
        write(names + ".svg", render_svg(act))
        png_path = os.path.join(OUT, names + ".png")
        render_png(act).save(png_path, dpi=(150, 150))
        print(png_path)
        xml = render_drawio(act)
        xmls.append(xml)
        write(names + ".drawio", mxfile([xml]))
        files.append((names, CAPTIONS[act.code]))

    write("DTAS-Activity-Diagrams.drawio", mxfile(xmls))

    figures = "".join(
        "<figure><img src='{0}.svg' alt='{1}'/><figcaption>{1}</figcaption></figure>\n".format(n, cap)
        for n, cap in files
    )
    write(
        "DTAS-Activity-Diagrams.html",
        """<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>DTAS Activity Diagrams (black and white)</title>
  <style>
    body { font-family: "Times New Roman", Times, serif; color: #000; background: #fff; margin: 24px; }
    h1 { font-size: 22px; }
    p, li { font-size: 13px; line-height: 1.45; }
    figure { page-break-inside: avoid; margin: 0 0 28px; }
    img { width: 100%; max-width: 1100px; border: 1px solid #000; }
    figcaption { font-size: 12px; margin-top: 6px; }
    @media print { body { margin: 12mm; } a { color: #000; text-decoration: none; } }
  </style>
</head>
<body>
  <h1>DTAS activity diagrams</h1>
  <p>Black-and-white UML activity diagrams for the Digital Transparency and Accountability System. PNG, SVG, and draw.io files are in this folder. Open this file in a browser and print to PDF if you need a report copy.</p>
  <ul>
    <li>Faculty and staff create events. Students propose events. System Admin does not create events or clubs.</li>
    <li>Public club and event join is a request. An invite code joins immediately.</li>
    <li>The Zoom room closes when start time plus duration ends.</li>
    <li>Contribution percentage is computed. It is not entered by hand.</li>
  </ul>
"""
        + figures
        + "</body></html>",
    )


if __name__ == "__main__":
    main()
