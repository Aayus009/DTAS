# Black-and-white crow's-foot ERDs from the live DigitalTransparencyDB schema.
from __future__ import print_function
import html
import os

from PIL import Image, ImageDraw, ImageFont

OUT = os.path.dirname(os.path.abspath(__file__))
SCALE = 2
FONT_REG = r"C:\Windows\Fonts\times.ttf"
FONT_BD = r"C:\Windows\Fonts\timesbd.ttf"


def font(size, bold=False):
    path = FONT_BD if bold else FONT_REG
    try:
        return ImageFont.truetype(path, size)
    except Exception:
        return ImageFont.load_default()


class Ent(object):
    def __init__(self, key, name, attrs, x, y, w=220):
        self.key = key
        self.name = name
        self.attrs = attrs  # list of (text, kind) kind=pk|fk|attr
        self.x = x
        self.y = y
        self.w = w
        self.draw_id = None

    @property
    def h(self):
        return 28 + 16 * max(1, len(self.attrs))

    @property
    def cx(self):
        return self.x + self.w / 2.0

    @property
    def cy(self):
        return self.y + self.h / 2.0

    def side(self, which):
        if which == "t":
            return self.cx, self.y
        if which == "b":
            return self.cx, self.y + self.h
        if which == "l":
            return self.x, self.cy
        return self.x + self.w, self.cy


class Rel(object):
    def __init__(self, a, b, a_card, b_card, a_side, b_side, label=""):
        self.a = a
        self.b = b
        self.a_card = a_card  # "1", "0..1", "*"
        self.b_card = b_card
        self.a_side = a_side
        self.b_side = b_side
        self.label = label


class Diagram(object):
    def __init__(self, code, title, subtitle, w, h):
        self.code = code
        self.title = title
        self.subtitle = subtitle
        self.w = w
        self.h = h
        self.ents = {}
        self.rels = []
        self.notes = []

    def add(self, key, name, attrs, x, y, w=220):
        self.ents[key] = Ent(key, name, attrs, x, y, w)
        return key

    def rel(self, a, b, a_card, b_card, a_side, b_side, label=""):
        self.rels.append(Rel(a, b, a_card, b_card, a_side, b_side, label))

    def note(self, text, x, y, w=360, h=48):
        self.notes.append((text, x, y, w, h))


def route(p1, p2, s1, s2):
    x1, y1 = p1
    x2, y2 = p2
    if s1 == "r" and s2 == "l":
        mid = (x1 + x2) / 2.0
        return [(x1, y1), (mid, y1), (mid, y2), (x2, y2)]
    if s1 == "l" and s2 == "r":
        mid = (x1 + x2) / 2.0
        return [(x1, y1), (mid, y1), (mid, y2), (x2, y2)]
    if s1 == "b" and s2 == "t":
        mid = (y1 + y2) / 2.0
        return [(x1, y1), (x1, mid), (x2, mid), (x2, y2)]
    if s1 == "t" and s2 == "b":
        mid = (y1 + y2) / 2.0
        return [(x1, y1), (x1, mid), (x2, mid), (x2, y2)]
    if s1 in ("r", "l") and s2 in ("t", "b"):
        return [(x1, y1), (x2, y1), (x2, y2)]
    if s1 in ("t", "b") and s2 in ("r", "l"):
        return [(x1, y1), (x1, y2), (x2, y2)]
    return [(x1, y1), (x2, y2)]


def svg_escape(t):
    return html.escape(t)


def crow_svg(x, y, inward_x, inward_y, many):
    dx, dy = inward_x - x, inward_y - y
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    px, py = -uy, ux
    parts = []
    if many:
        for k in (-1, 0, 1):
            parts.append(
                '<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.2"/>'.format(
                    x + 10 * ux + 6 * k * px,
                    y + 10 * uy + 6 * k * py,
                    x,
                    y,
                )
            )
    else:
        parts.append(
            '<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.3"/>'.format(
                x + 5 * px, y + 5 * py, x - 5 * px, y - 5 * py
            )
        )
    return parts


def render_svg(d):
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<svg xmlns="http://www.w3.org/2000/svg" width="{0}" height="{1}" viewBox="0 0 {0} {1}">'.format(d.w, d.h),
        "<style>.t{font-family:'Times New Roman',Times,serif;fill:#000}.title{font-size:20px;font-weight:bold}"
        ".sub{font-size:12px}.en{font-size:13px;font-weight:bold}.at{font-size:11px}.pk{font-size:11px;font-weight:bold}"
        ".nt{font-size:11px}</style>",
        '<rect width="{0}" height="{1}" fill="#fff"/>'.format(d.w, d.h),
        '<text class="t title" x="{0}" y="28" text-anchor="middle">{1}</text>'.format(d.w / 2, svg_escape(d.title)),
        '<text class="t sub" x="{0}" y="50" text-anchor="middle">{1}</text>'.format(d.w / 2, svg_escape(d.subtitle)),
    ]

    for rel in d.rels:
        a, b = d.ents[rel.a], d.ents[rel.b]
        p1, p2 = a.side(rel.a_side), b.side(rel.b_side)
        pts = route(p1, p2, rel.a_side, rel.b_side)
        poly = " ".join("{0:.1f},{1:.1f}".format(x, y) for x, y in pts)
        parts.append(
            '<polyline points="{0}" fill="none" stroke="#000" stroke-width="1.15"/>'.format(poly)
        )
        if len(pts) >= 2:
            parts += crow_svg(pts[0][0], pts[0][1], pts[1][0], pts[1][1], rel.a_card == "*")
            parts += crow_svg(pts[-1][0], pts[-1][1], pts[-2][0], pts[-2][1], rel.b_card == "*")
        if rel.label:
            mx = (pts[0][0] + pts[-1][0]) / 2.0
            my = (pts[0][1] + pts[-1][1]) / 2.0 - 6
            parts.append(
                '<text class="t nt" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                    mx, my, svg_escape(rel.label)
                )
            )

    for e in d.ents.values():
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                e.x, e.y, e.w, e.h
            )
        )
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="26" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                e.x, e.y, e.w
            )
        )
        parts.append(
            '<text class="t en" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                e.cx, e.y + 18, svg_escape(e.name)
            )
        )
        for i, (text, kind) in enumerate(e.attrs):
            cls = "pk" if kind == "pk" else "at"
            prefix = "" if kind == "attr" else ("PK  " if kind == "pk" else "FK  ")
            deco = " text-decoration:underline;" if kind == "pk" else ""
            parts.append(
                '<text class="t {0}" x="{1}" y="{2}" style="{3}">{4}{5}</text>'.format(
                    cls, e.x + 8, e.y + 42 + i * 16, deco, prefix, svg_escape(text)
                )
            )

    for text, x, y, w, h in d.notes:
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1"/>'.format(
                x, y, w, h
            )
        )
        lines = text.split("\n")
        for i, line in enumerate(lines):
            parts.append(
                '<text class="t nt" x="{0}" y="{1}">{2}</text>'.format(x + 8, y + 16 + i * 14, svg_escape(line))
            )
    parts.append("</svg>")
    return "\n".join(parts)


def dashed_line(dr, a, b, s):
    x1, y1 = a
    x2, y2 = b
    dx, dy = x2 - x1, y2 - y1
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    pos, on, step = 0, True, 7 * s
    while pos < length:
        nxt = min(pos + step, length)
        if on:
            dr.line(
                [(x1 + ux * pos, y1 + uy * pos), (x1 + ux * nxt, y1 + uy * nxt)],
                fill=(0, 0, 0),
                width=max(1, s),
            )
        on = not on
        pos = nxt


def crow_png(dr, x, y, ix, iy, many, s):
    dx, dy = ix - x, iy - y
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    px, py = -uy, ux
    if many:
        for k in (-1, 0, 1):
            dr.line(
                [
                    ((x + 10 * ux + 6 * k * px) * s, (y + 10 * uy + 6 * k * py) * s),
                    (x * s, y * s),
                ],
                fill=(0, 0, 0),
                width=2 * s,
            )
    else:
        dr.line(
            [((x + 6 * px) * s, (y + 6 * py) * s), ((x - 6 * px) * s, (y - 6 * py) * s)],
            fill=(0, 0, 0),
            width=2 * s,
        )


def render_png(d):
    s = SCALE
    img = Image.new("RGB", (d.w * s, d.h * s), (255, 255, 255))
    dr = ImageDraw.Draw(img)
    f_title, f_sub = font(22 * s, True), font(14 * s)
    f_en, f_pk, f_at, f_nt = font(14 * s, True), font(12 * s, True), font(12 * s), font(12 * s)

    def midtext(text, x, y, fnt):
        tw = dr.textlength(text, font=fnt)
        dr.text((x - tw / 2, y), text, fill=(0, 0, 0), font=fnt)

    midtext(d.title, (d.w * s) / 2, 8 * s, f_title)
    midtext(d.subtitle, (d.w * s) / 2, 36 * s, f_sub)

    for rel in d.rels:
        a, b = d.ents[rel.a], d.ents[rel.b]
        pts = route(a.side(rel.a_side), b.side(rel.b_side), rel.a_side, rel.b_side)
        for i in range(len(pts) - 1):
            dr.line(
                [(pts[i][0] * s, pts[i][1] * s), (pts[i + 1][0] * s, pts[i + 1][1] * s)],
                fill=(0, 0, 0),
                width=2 * s,
            )
        if len(pts) >= 2:
            crow_png(dr, pts[0][0], pts[0][1], pts[1][0], pts[1][1], rel.a_card == "*", s)
            crow_png(dr, pts[-1][0], pts[-1][1], pts[-2][0], pts[-2][1], rel.b_card == "*", s)
        if rel.label:
            mx = (pts[0][0] + pts[-1][0]) / 2.0
            my = (pts[0][1] + pts[-1][1]) / 2.0
            midtext(rel.label, mx * s, (my - 12) * s, f_nt)

    for e in d.ents.values():
        dr.rectangle(
            [e.x * s, e.y * s, (e.x + e.w) * s, (e.y + e.h) * s],
            outline=(0, 0, 0),
            width=2 * s,
            fill=(255, 255, 255),
        )
        dr.rectangle(
            [e.x * s, e.y * s, (e.x + e.w) * s, (e.y + 26) * s],
            outline=(0, 0, 0),
            width=2 * s,
            fill=(255, 255, 255),
        )
        midtext(e.name, e.cx * s, (e.y + 6) * s, f_en)
        for i, (text, kind) in enumerate(e.attrs):
            prefix = "PK  " if kind == "pk" else ("FK  " if kind == "fk" else "")
            fnt = f_pk if kind == "pk" else f_at
            dr.text(((e.x + 8) * s, (e.y + 30 + i * 16) * s), prefix + text, fill=(0, 0, 0), font=fnt)

    for text, x, y, w, h in d.notes:
        dr.rectangle([x * s, y * s, (x + w) * s, (y + h) * s], outline=(0, 0, 0), width=s, fill=(255, 255, 255))
        for i, line in enumerate(text.split("\n")):
            dr.text(((x + 8) * s, (y + 6 + i * 14) * s), line, fill=(0, 0, 0), font=f_nt)
    return img


BW = "fillColor=#ffffff;strokeColor=#000000;fontColor=#000000;fontFamily=Times New Roman;"


def render_drawio(d):
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
                i, value, style, int(x), int(y), int(w), int(h)
            )
        )
        return i

    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=18;fontStyle=1;fontColor=#000000;",
        html.escape(d.title), 20, 8, d.w - 40, 26,
    )
    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=12;fontColor=#000000;",
        html.escape(d.subtitle), 20, 36, d.w - 40, 20,
    )

    for e in d.ents.values():
        rows = []
        for text, kind in e.attrs:
            mark = "<u>%s</u>" % html.escape(text) if kind == "pk" else html.escape(text)
            tag = "PK " if kind == "pk" else ("FK " if kind == "fk" else "")
            rows.append(tag + mark)
        value = "<b>%s</b><br>%s" % (html.escape(e.name), "<br>".join(rows))
        e.draw_id = cell(
            "whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacingLeft=8;spacingTop=4;"
            + BW + "fontSize=11;",
            value, e.x, e.y, e.w, e.h,
        )

    for text, x, y, w, h in d.notes:
        cell(
            "shape=note;whiteSpace=wrap;html=1;size=14;align=left;spacingLeft=6;" + BW + "fontSize=11;",
            html.escape(text).replace("\n", "&lt;br&gt;"), x, y, w, h,
        )

    style = (
        "endArrow=ERmany;startArrow=ERmandOne;html=1;strokeColor=#000000;fontColor=#000000;"
        "fontFamily=Times New Roman;fontSize=10;edgeStyle=orthogonalEdgeStyle;"
    )
    style_mm = (
        "endArrow=ERmany;startArrow=ERmany;html=1;strokeColor=#000000;fontColor=#000000;"
        "fontFamily=Times New Roman;fontSize=10;edgeStyle=orthogonalEdgeStyle;"
    )
    style_11 = (
        "endArrow=ERmandOne;startArrow=ERmandOne;html=1;strokeColor=#000000;fontColor=#000000;"
        "fontFamily=Times New Roman;fontSize=10;edgeStyle=orthogonalEdgeStyle;"
    )
    for rel in d.rels:
        i = nid()
        st = style
        if rel.a_card == "*" and rel.b_card == "*":
            st = style_mm
        elif rel.a_card != "*" and rel.b_card != "*":
            st = style_11
        elif rel.a_card == "*" and rel.b_card != "*":
            st = (
                "endArrow=ERmandOne;startArrow=ERmany;html=1;strokeColor=#000000;fontColor=#000000;"
                "fontFamily=Times New Roman;fontSize=10;edgeStyle=orthogonalEdgeStyle;"
            )
        cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" edge="1" parent="1" source="{3}" target="{4}">'
            '<mxGeometry relative="1" as="geometry"/></mxCell>'.format(
                i, html.escape(rel.label), st, d.ents[rel.a].draw_id, d.ents[rel.b].draw_id
            )
        )

    return (
        '  <diagram id="{0}" name="{1}">\n'
        '    <mxGraphModel dx="1400" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" '
        'arrows="1" fold="1" page="1" pageScale="1" pageWidth="{2}" pageHeight="{3}" math="0" shadow="0">\n'
        "      <root>\n        <mxCell id=\"0\"/>\n        <mxCell id=\"1\" parent=\"0\"/>\n"
        "{4}\n      </root>\n    </mxGraphModel>\n  </diagram>"
    ).format(d.code.lower(), html.escape(d.code), d.w, d.h, "\n".join(cells))


def mxfile(xmls):
    return (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<mxfile host="app.diagrams.net" agent="DTAS" version="22.1.0" type="device">\n'
        + "\n".join(xmls)
        + "\n</mxfile>\n"
    )


# ---------- diagrams from live DigitalTransparencyDB ----------
def erd1():
    d = Diagram(
        "ERD-1",
        "Figure ERD-1. DTAS logical data model (overview)",
        "Crow's foot. Live DigitalTransparencyDB. Many tables reference Users; only ownership and membership links are drawn here.",
        1680, 980,
    )
    d.add("roles", "Roles", [("RoleID", "pk"), ("RoleName", "attr")], 40, 80, 160)
    d.add("dept", "Departments", [("DepartmentID", "pk"), ("DepartmentName", "attr")], 40, 200, 180)
    d.add("users", "Users", [("UserID", "pk"), ("RoleID", "fk"), ("DepartmentID", "fk"), ("Email", "attr"), ("AccountStatus", "attr")], 280, 120, 210)
    d.add("idoc", "IdentityDocuments", [("DocumentID", "pk"), ("UserID", "fk"), ("VerificationStatus", "attr")], 40, 360, 200)
    d.add("events", "Events", [("EventID", "pk"), ("ClubID", "fk"), ("Status", "attr"), ("Visibility", "attr")], 560, 80, 200)
    d.add("em", "EventMembers", [("EventMemberID", "pk"), ("EventID", "fk"), ("UserID", "fk"), ("InviteStatus", "attr")], 560, 250, 210)
    d.add("meet", "Meetings", [("MeetingID", "pk"), ("EventID", "fk"), ("ZoomMeetingId", "attr"), ("Status", "attr")], 560, 430, 210)
    d.add("mp", "MeetingParticipants", [("ParticipantID", "pk"), ("MeetingID", "fk"), ("UserID", "fk")], 560, 610, 210)
    d.add("clubs", "Clubs", [("ClubID", "pk"), ("LeadUserID", "fk"), ("InviteCode", "attr"), ("IsRestricted", "attr")], 840, 80, 200)
    d.add("cm", "ClubMembers", [("MembershipID", "pk"), ("ClubID", "fk"), ("UserID", "fk"), ("InviteStatus", "attr")], 840, 250, 210)
    d.add("cg", "ConnectGroups", [("GroupID", "pk"), ("EventID", "fk"), ("ClubID", "fk"), ("InviteCode", "attr")], 840, 430, 210)
    d.add("cmem", "ConnectMembers", [("MemberID", "pk"), ("GroupID", "fk"), ("UserID", "fk")], 840, 610, 210)
    d.add("asg", "Assignments", [("AssignmentID", "pk"), ("CreatedBy", "fk"), ("Deadline", "attr"), ("AssignmentCode", "attr")], 1120, 80, 220)
    d.add("ag", "AssignmentGroups", [("GroupID", "pk"), ("AssignmentID", "fk"), ("LeaderID", "fk")], 1120, 250, 220)
    d.add("am", "AssignmentMembers", [("MemberID", "pk"), ("GroupID", "fk"), ("UserID", "fk")], 1120, 400, 220)
    d.add("at", "AssignmentTasks", [("TaskID", "pk"), ("GroupID", "fk"), ("AssignedUserID", "fk")], 1120, 540, 220)
    d.add("cr", "ContributionRecords", [("RecordID", "pk"), ("GroupID", "fk"), ("UserID", "fk"), ("ContributionScore", "attr")], 1120, 700, 230)
    d.add("tasks", "Tasks", [("TaskID", "pk"), ("EventID", "fk"), ("Status", "attr")], 1410, 80, 200)
    d.add("dec", "Decisions", [("DecisionID", "pk"), ("EventID", "fk"), ("Status", "attr")], 1410, 230, 200)
    d.add("polls", "Polls", [("PollID", "pk"), ("EventID", "fk"), ("IsRestricted", "attr")], 1410, 380, 200)
    d.add("rep", "ContentReports", [("ReportID", "pk"), ("ReporterID", "fk"), ("TargetType", "attr")], 1410, 530, 210)
    d.add("note", "Notifications", [("NotificationID", "pk"), ("UserID", "fk")], 1410, 680, 200)

    d.rel("roles", "users", "1", "*", "r", "l")
    d.rel("dept", "users", "1", "*", "r", "l")
    d.rel("users", "idoc", "1", "*", "b", "t")
    d.rel("users", "events", "1", "*", "r", "l", "creates / proposes")
    d.rel("events", "em", "1", "*", "b", "t")
    d.rel("users", "em", "1", "*", "r", "l")
    d.rel("meet", "mp", "1", "*", "b", "t")
    d.rel("clubs", "events", "1", "*", "l", "r", "optional ClubID")
    d.rel("clubs", "cm", "1", "*", "b", "t")
    d.rel("cg", "cmem", "1", "*", "b", "t")
    d.rel("asg", "ag", "1", "*", "b", "t")
    d.rel("ag", "am", "1", "*", "b", "t")
    d.note("DigitalTransparencyDB on SQL Server Express. ContributionScore is computed. It is not typed in.", 40, 880, 520, 40)
    return d


def erd2():
    d = Diagram(
        "ERD-2",
        "Figure ERD-2. Users, roles, and identity",
        "Campus modules stay locked until email and institutional ID are approved.",
        1500, 820,
    )
    d.add("roles", "Roles", [("RoleID", "pk"), ("RoleName", "attr"), ("Description", "attr")], 40, 90, 200)
    d.add("dept", "Departments", [("DepartmentID", "pk"), ("DepartmentName", "attr"), ("IsActive", "attr")], 40, 250, 200)
    d.add("users", "Users", [
        ("UserID", "pk"), ("RoleID", "fk"), ("DepartmentID", "fk"),
        ("FullName", "attr"), ("Email", "attr"), ("Username", "attr"),
        ("EmailVerified", "attr"), ("IdentityVerified", "attr"),
        ("VerificationStatus", "attr"), ("AccountStatus", "attr"),
        ("InstitutionalID", "attr"), ("VerifiedBy", "fk"),
    ], 320, 90, 240)
    d.add("otp", "EmailVerification", [
        ("VerificationID", "pk"), ("UserID", "fk"), ("Email", "attr"),
        ("OTP", "attr"), ("Expiry", "attr"), ("Purpose", "attr"), ("IsUsed", "attr"),
    ], 640, 90, 230)
    d.add("idoc", "IdentityDocuments", [
        ("DocumentID", "pk"), ("UserID", "fk"), ("FilePath", "attr"),
        ("FileType", "attr"), ("VerificationStatus", "attr"), ("IsCurrent", "attr"),
    ], 640, 320, 240)
    d.add("ivh", "IdentityVerificationHistory", [
        ("HistoryID", "pk"), ("UserID", "fk"), ("DocumentID", "fk"),
        ("OldStatus", "attr"), ("NewStatus", "attr"), ("AdminID", "fk"),
    ], 960, 320, 250)
    d.add("sus", "Suspensions", [
        ("SuspensionID", "pk"), ("UserID", "fk"), ("Reason", "attr"),
        ("StartDate", "attr"), ("EndDate", "attr"), ("CreatedBy", "fk"),
    ], 960, 90, 230)
    d.add("ban", "Bans", [
        ("BanID", "pk"), ("UserID", "fk"), ("Email", "attr"),
        ("InstitutionalID", "attr"), ("BannedBy", "fk"), ("IsActive", "attr"),
    ], 1240, 90, 220)
    d.add("login", "LoginLogs", [
        ("LogID", "pk"), ("UserID", "fk"), ("LoginTime", "attr"), ("IPAddress", "attr"),
    ], 320, 520, 240)
    d.rel("roles", "users", "1", "*", "r", "l")
    d.rel("dept", "users", "1", "*", "r", "l")
    d.rel("users", "otp", "1", "*", "r", "l")
    d.rel("users", "idoc", "1", "*", "b", "l")
    d.rel("idoc", "ivh", "1", "*", "r", "l")
    d.rel("users", "login", "1", "*", "b", "t")
    return d


def erd3():
    d = Diagram(
        "ERD-3",
        "Figure ERD-3. Campus events and Zoom meetings",
        "Public join is a request. Invite code joins immediately. The Zoom room closes when start plus duration ends.",
        1500, 860,
    )
    d.add("users", "Users", [("UserID", "pk"), ("FullName", "attr")], 40, 90, 170)
    d.add("clubs", "Clubs", [("ClubID", "pk"), ("ClubName", "attr")], 40, 250, 170)
    d.add("events", "Events", [
        ("EventID", "pk"), ("ClubID", "fk"), ("CreatedBy", "fk"), ("ProposedBy", "fk"),
        ("EventName", "attr"), ("Status", "attr"), ("Visibility", "attr"),
        ("InviteCode", "attr"), ("IsDisabled", "attr"),
    ], 300, 90, 230)
    d.add("em", "EventMembers", [
        ("EventMemberID", "pk"), ("EventID", "fk"), ("UserID", "fk"),
        ("RoleInEvent", "attr"), ("InviteStatus", "attr"), ("IsActive", "attr"),
    ], 620, 90, 230)
    d.add("ei", "EventInvitations", [
        ("InvitationID", "pk"), ("EventID", "fk"), ("Email", "attr"),
        ("InvitedUserID", "fk"), ("InviteRole", "attr"), ("Status", "attr"),
    ], 300, 380, 240)
    d.add("en", "EventNotes", [
        ("NoteID", "pk"), ("EventID", "fk"), ("UserID", "fk"),
        ("NoteText", "attr"), ("IsPinned", "attr"),
    ], 620, 380, 230)
    d.add("meet", "Meetings", [
        ("MeetingID", "pk"), ("EventID", "fk"), ("CreatedBy", "fk"),
        ("ScheduledDate", "attr"), ("Duration", "attr"),
        ("ZoomMeetingId", "attr"), ("ZoomJoinUrl", "attr"), ("Status", "attr"),
    ], 940, 90, 240)
    d.add("mp", "MeetingParticipants", [
        ("ParticipantID", "pk"), ("MeetingID", "fk"), ("UserID", "fk"),
        ("Role", "attr"), ("AttendanceStatus", "attr"),
    ], 940, 380, 250)
    d.add("note", "Notifications", [
        ("NotificationID", "pk"), ("UserID", "fk"), ("RelatedType", "attr"), ("RelatedID", "attr"),
    ], 1240, 90, 220)
    d.rel("clubs", "events", "1", "*", "r", "l", "optional")
    d.rel("users", "events", "1", "*", "r", "l")
    d.rel("events", "em", "1", "*", "r", "l")
    d.rel("users", "em", "1", "*", "b", "l")
    d.rel("events", "ei", "1", "*", "b", "t")
    d.rel("events", "en", "1", "*", "b", "t")
    d.rel("events", "meet", "1", "*", "r", "l")
    d.rel("meet", "mp", "1", "*", "b", "t")
    return d


def erd4():
    d = Diagram(
        "ERD-4",
        "Figure ERD-4. Clubs and Connect",
        "Admin cannot create clubs. Public join is a request. A club or event may have a linked Connect group.",
        1500, 820,
    )
    d.add("users", "Users", [("UserID", "pk"), ("FullName", "attr")], 40, 200, 170)
    d.add("clubs", "Clubs", [
        ("ClubID", "pk"), ("LeadUserID", "fk"), ("CreatedBy", "fk"),
        ("ClubName", "attr"), ("IsPublic", "attr"), ("InviteCode", "attr"),
        ("IsRestricted", "attr"), ("IsDeleted", "attr"),
    ], 280, 80, 230)
    d.add("cm", "ClubMembers", [
        ("MembershipID", "pk"), ("ClubID", "fk"), ("UserID", "fk"),
        ("Role", "attr"), ("InviteStatus", "attr"), ("IsActive", "attr"),
    ], 280, 340, 230)
    d.add("gi", "GroupInvitations", [
        ("InvitationID", "pk"), ("ClubID", "fk"), ("Email", "attr"),
        ("InvitedUserID", "fk"), ("Status", "attr"),
    ], 580, 80, 230)
    d.add("gm", "GroupMessages", [
        ("MessageID", "pk"), ("ClubID", "fk"), ("SenderID", "fk"),
        ("MessageType", "attr"), ("IsDeleted", "attr"),
    ], 580, 300, 230)
    d.add("ga", "GroupAnnouncements", [
        ("AnnouncementID", "pk"), ("ClubID", "fk"), ("SenderID", "fk"),
        ("Title", "attr"), ("IsUrgent", "attr"),
    ], 580, 520, 230)
    d.add("cg", "ConnectGroups", [
        ("GroupID", "pk"), ("CreatedBy", "fk"), ("EventID", "fk"), ("ClubID", "fk"),
        ("GroupName", "attr"), ("InviteCode", "attr"),
    ], 880, 80, 240)
    d.add("cmem", "ConnectMembers", [
        ("MemberID", "pk"), ("GroupID", "fk"), ("UserID", "fk"),
        ("Role", "attr"), ("IsArchived", "attr"),
    ], 880, 320, 240)
    d.add("cmsg", "ConnectMessages", [
        ("MessageID", "pk"), ("GroupID", "fk"), ("SenderID", "fk"),
        ("MessageType", "attr"), ("Content", "attr"),
    ], 1200, 80, 240)
    d.add("events", "Events", [("EventID", "pk"), ("EventName", "attr"), ("ClubID", "fk")], 1200, 320, 220)
    d.rel("users", "clubs", "1", "*", "r", "l")
    d.rel("clubs", "cm", "1", "*", "b", "t")
    d.rel("users", "cm", "1", "*", "r", "l")
    d.rel("clubs", "gi", "1", "*", "r", "l")
    d.rel("clubs", "gm", "1", "*", "r", "l")
    d.rel("clubs", "ga", "1", "*", "r", "l")
    d.rel("clubs", "cg", "0..1", "*", "r", "l")
    d.rel("events", "cg", "0..1", "*", "l", "r")
    d.rel("cg", "cmem", "1", "*", "b", "t")
    d.rel("cg", "cmsg", "1", "*", "r", "l")
    return d


def erd5():
    d = Diagram(
        "ERD-5",
        "Figure ERD-5. Faculty assignments and computed contribution",
        "Contribution is calculated (completion, timeliness, activity, recency). It is not typed in.",
        1500, 860,
    )
    d.add("users", "Users", [("UserID", "pk"), ("FullName", "attr"), ("RoleID", "fk")], 40, 200, 190)
    d.add("asg", "Assignments", [
        ("AssignmentID", "pk"), ("CreatedBy", "fk"), ("AssignmentName", "attr"),
        ("Deadline", "attr"), ("AssignmentCode", "attr"), ("Status", "attr"),
        ("AssignmentType", "attr"),
    ], 300, 80, 240)
    d.add("ag", "AssignmentGroups", [
        ("GroupID", "pk"), ("AssignmentID", "fk"), ("LeaderID", "fk"),
        ("GroupName", "attr"), ("IsFinalized", "attr"), ("IsDeleted", "attr"),
    ], 620, 80, 240)
    d.add("am", "AssignmentMembers", [
        ("MemberID", "pk"), ("GroupID", "fk"), ("UserID", "fk"),
        ("Responsibility", "attr"),
    ], 620, 320, 240)
    d.add("ai", "AssignmentInvitations", [
        ("InvitationID", "pk"), ("AssignmentID", "fk"), ("GroupID", "fk"),
        ("Email", "attr"), ("Status", "attr"),
    ], 300, 380, 250)
    d.add("at", "AssignmentTasks", [
        ("TaskID", "pk"), ("GroupID", "fk"), ("AssignedUserID", "fk"),
        ("ParentTaskID", "fk"), ("Title", "attr"), ("Status", "attr"),
        ("SubmittedBy", "fk"),
    ], 940, 80, 250)
    d.add("files", "AssignmentTaskFiles", [
        ("FileID", "pk"), ("TaskID", "fk"), ("UploadedBy", "fk"),
        ("FilePath", "attr"),
    ], 940, 360, 250)
    d.add("links", "AssignmentTaskLinks", [
        ("LinkID", "pk"), ("TaskID", "fk"), ("AddedBy", "fk"),
        ("Url", "attr"),
    ], 940, 540, 250)
    d.add("cr", "ContributionRecords", [
        ("RecordID", "pk"), ("GroupID", "fk"), ("UserID", "fk"),
        ("CompletionPercentage", "attr"), ("ContributionScore", "attr"),
        ("CalculatedAt", "attr"),
    ], 1240, 80, 240)
    d.add("hist", "AssignmentTaskHistory", [
        ("HistoryID", "pk"), ("TaskID", "fk"), ("OldStatus", "attr"),
        ("NewStatus", "attr"), ("ChangedBy", "fk"),
    ], 1240, 320, 240)
    d.rel("users", "asg", "1", "*", "r", "l")
    d.rel("asg", "ag", "1", "*", "r", "l")
    d.rel("asg", "ai", "1", "*", "b", "t")
    d.rel("ag", "am", "1", "*", "b", "t")
    d.rel("users", "am", "1", "*", "r", "l")
    d.rel("ag", "at", "1", "*", "r", "l")
    d.rel("at", "files", "1", "*", "b", "t")
    d.rel("at", "links", "1", "*", "b", "t")
    d.rel("ag", "cr", "1", "*", "r", "l")
    d.rel("users", "cr", "1", "*", "r", "l")
    d.rel("at", "hist", "1", "*", "r", "l")
    return d


def erd6():
    d = Diagram(
        "ERD-6",
        "Figure ERD-6. Event tasks, decisions, and polls",
        "Event tasks are separate from faculty AssignmentTasks. Admin may restrict a task, decision, or poll.",
        1500, 860,
    )
    d.add("events", "Events", [("EventID", "pk"), ("EventName", "attr"), ("Status", "attr")], 40, 200, 190)
    d.add("users", "Users", [("UserID", "pk"), ("FullName", "attr")], 40, 400, 190)
    d.add("tasks", "Tasks", [
        ("TaskID", "pk"), ("EventID", "fk"), ("CreatedBy", "fk"), ("LeaderID", "fk"),
        ("TaskTitle", "attr"), ("Status", "attr"), ("DueDate", "attr"),
        ("IsRestricted", "attr"),
    ], 300, 80, 240)
    d.add("ta", "TaskAssignments", [
        ("AssignmentID", "pk"), ("TaskID", "fk"), ("UserID", "fk"),
    ], 300, 360, 240)
    d.add("tc", "TaskComments", [
        ("CommentID", "pk"), ("TaskID", "fk"), ("UserID", "fk"),
        ("Comment", "attr"), ("MentionedUserID", "attr"),
    ], 300, 520, 250)
    d.add("tt", "TaskTeams", [
        ("TeamID", "pk"), ("TaskID", "fk"), ("LeaderID", "fk"),
        ("ProgressPercent", "attr"),
    ], 620, 80, 230)
    d.add("ttm", "TaskTeamMembers", [
        ("MemberID", "pk"), ("TeamID", "fk"), ("UserID", "fk"),
        ("Status", "attr"),
    ], 620, 280, 230)
    d.add("dec", "Decisions", [
        ("DecisionID", "pk"), ("EventID", "fk"), ("MeetingID", "fk"),
        ("ResponsibleUserID", "fk"), ("Status", "attr"), ("IsRestricted", "attr"),
    ], 920, 80, 250)
    d.add("dh", "DecisionHistory", [
        ("HistoryID", "pk"), ("DecisionID", "fk"), ("OldStatus", "attr"),
        ("NewStatus", "attr"), ("ChangedBy", "fk"),
    ], 920, 340, 250)
    d.add("polls", "Polls", [
        ("PollID", "pk"), ("EventID", "fk"), ("DecisionID", "fk"),
        ("CreatedBy", "fk"), ("IsRestricted", "attr"),
    ], 1240, 80, 230)
    d.add("po", "PollOptions", [
        ("OptionID", "pk"), ("PollID", "fk"), ("OptionText", "attr"),
        ("VoteCount", "attr"),
    ], 1240, 300, 230)
    d.add("votes", "Votes", [
        ("VoteID", "pk"), ("PollID", "fk"), ("OptionID", "fk"),
        ("UserID", "fk"),
    ], 1240, 500, 230)
    d.rel("events", "tasks", "1", "*", "r", "l")
    d.rel("tasks", "ta", "1", "*", "b", "t")
    d.rel("users", "ta", "1", "*", "r", "l")
    d.rel("tasks", "tc", "1", "*", "b", "t")
    d.rel("tasks", "tt", "1", "1", "r", "l")
    d.rel("tt", "ttm", "1", "*", "b", "t")
    d.rel("dec", "dh", "1", "*", "b", "t")
    d.rel("polls", "po", "1", "*", "b", "t")
    d.rel("polls", "votes", "1", "*", "b", "t")
    d.rel("po", "votes", "1", "*", "b", "t")
    return d


def erd7():
    d = Diagram(
        "ERD-7",
        "Figure ERD-7. Moderation, audit, feedback, and notifications",
        "Users flag content. Admin reviews the queue. Admin cannot moderate another admin.",
        1480, 780,
    )
    d.add("users", "Users", [
        ("UserID", "pk"), ("AccountStatus", "attr"), ("Email", "attr"),
    ], 40, 200, 200)
    d.add("rep", "ContentReports", [
        ("ReportID", "pk"), ("ReporterID", "fk"), ("AssignedAdminID", "fk"),
        ("TargetType", "attr"), ("TargetID", "attr"), ("Reason", "attr"),
        ("Status", "attr"), ("Resolution", "attr"),
    ], 320, 80, 260)
    d.add("sus", "Suspensions", [
        ("SuspensionID", "pk"), ("UserID", "fk"), ("CreatedBy", "fk"),
        ("StartDate", "attr"), ("EndDate", "attr"), ("IsActive", "attr"),
    ], 320, 380, 260)
    d.add("ban", "Bans", [
        ("BanID", "pk"), ("UserID", "fk"), ("BannedBy", "fk"),
        ("Email", "attr"), ("InstitutionalID", "attr"), ("IsActive", "attr"),
    ], 660, 380, 250)
    d.add("audit", "AuditLogs", [
        ("LogID", "pk"), ("UserID", "fk"), ("Action", "attr"),
        ("EntityType", "attr"), ("EntityID", "attr"), ("Timestamp", "attr"),
    ], 660, 80, 250)
    d.add("fb", "Feedback", [
        ("FeedbackID", "pk"), ("UserID", "fk"), ("Category", "attr"),
        ("Title", "attr"), ("Rating", "attr"), ("Status", "attr"),
    ], 990, 80, 230)
    d.add("note", "Notifications", [
        ("NotificationID", "pk"), ("UserID", "fk"), ("Title", "attr"),
        ("NotificationType", "attr"), ("RelatedType", "attr"), ("RelatedID", "attr"),
        ("IsRead", "attr"),
    ], 990, 320, 250)
    d.add("att", "Attachments", [
        ("AttachmentID", "pk"), ("UploadedBy", "fk"), ("FileName", "attr"),
        ("RelatedType", "attr"), ("RelatedID", "attr"),
    ], 1290, 80, 170)
    d.rel("users", "rep", "1", "*", "r", "l")
    d.rel("users", "sus", "1", "*", "r", "l")
    return d


NAMES = {
    "ERD-1": "ERD1-Overview",
    "ERD-2": "ERD2-Identity",
    "ERD-3": "ERD3-EventsMeetings",
    "ERD-4": "ERD4-ClubsConnect",
    "ERD-5": "ERD5-Assignments",
    "ERD-6": "ERD6-WorkGovernance",
    "ERD-7": "ERD7-Moderation",
}

CAPTIONS = {
    "ERD-1": "Figure ERD-1. Logical data model overview. Live DigitalTransparencyDB.",
    "ERD-2": "Figure ERD-2. Users, roles, email OTP, and institutional ID.",
    "ERD-3": "Figure ERD-3. Campus events, members, and Zoom meetings.",
    "ERD-4": "Figure ERD-4. Clubs and Connect groups.",
    "ERD-5": "Figure ERD-5. Faculty assignments and computed contribution.",
    "ERD-6": "Figure ERD-6. Event tasks, decisions, and polls.",
    "ERD-7": "Figure ERD-7. Moderation, audit, feedback, and notifications.",
}


def write(name, data):
    path = os.path.join(OUT, name)
    with open(path, "w", encoding="utf-8") as f:
        f.write(data)
    print(path)
    return path


def main():
    diagrams = [erd1(), erd2(), erd3(), erd4(), erd5(), erd6(), erd7()]
    xmls = []
    files = []
    for d in diagrams:
        names = NAMES[d.code]
        write(names + ".svg", render_svg(d))
        png_path = os.path.join(OUT, names + ".png")
        render_png(d).save(png_path, dpi=(150, 150))
        print(png_path)
        xml = render_drawio(d)
        xmls.append(xml)
        write(names + ".drawio", mxfile([xml]))
        files.append((names, CAPTIONS[d.code]))

    write("DTAS-ERD.drawio", mxfile(xmls))
    figures = "".join(
        "<figure><img src='{0}.svg' alt='{1}'/><figcaption>{1}</figcaption></figure>\n".format(n, cap)
        for n, cap in files
    )
    write(
        "DTAS-ERD.html",
        """<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>DTAS Entity Relationship Diagrams (black and white)</title>
  <style>
    body { font-family: "Times New Roman", Times, serif; color: #000; background: #fff; margin: 24px; }
    h1 { font-size: 22px; }
    p, li { font-size: 13px; line-height: 1.45; }
    figure { page-break-inside: avoid; margin: 0 0 28px; }
    img { width: 100%; max-width: 1200px; border: 1px solid #000; }
    figcaption { font-size: 12px; margin-top: 6px; }
    @media print { body { margin: 12mm; } a { color: #000; text-decoration: none; } }
  </style>
</head>
<body>
  <h1>DTAS entity relationship diagrams</h1>
  <p>Black-and-white crow's-foot ERDs from the live DigitalTransparencyDB schema. PNG, SVG, and draw.io files are in this folder.</p>
  <ul>
    <li>Faculty and staff create events. Students propose events. System Admin does not create events or clubs.</li>
    <li>Public club and event join is a request (InviteStatus). An invite code joins immediately.</li>
    <li>Meetings store ZoomMeetingId and join URL. The room is cleared when status becomes Completed.</li>
    <li>ContributionScore is computed by sp_GetMemberContributionReport. It is not typed in.</li>
  </ul>
"""
        + figures
        + "</body></html>",
    )


if __name__ == "__main__":
    main()
