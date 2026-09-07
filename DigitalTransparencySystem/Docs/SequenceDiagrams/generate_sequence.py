# Black-and-white UML sequence diagrams: SVG, PNG, and draw.io.
from __future__ import print_function
import html
import os

from PIL import Image, ImageDraw, ImageFont

OUT = os.path.dirname(os.path.abspath(__file__))
LEFT = 80
TOP = 108
HEAD_W = 168
HEAD_H = 42
GAP = 250
STEP = 48
SCALE = 2
TAIL = 56

FONT_REG = r"C:\Windows\Fonts\times.ttf"
FONT_BD = r"C:\Windows\Fonts\timesbd.ttf"


def font(size, bold=False):
    path = FONT_BD if bold else FONT_REG
    try:
        return ImageFont.truetype(path, size)
    except Exception:
        return ImageFont.load_default()


class Message(object):
    def __init__(self, src, tgt, text, dashed=False):
        self.src = src
        self.tgt = tgt
        self.text = text
        self.dashed = dashed
        self.y = 0


class Fragment(object):
    def __init__(self, kind, start, end, guard="", mid=None, guard2="", extras=None):
        self.kind = kind
        self.start = start
        self.end = end
        self.guard = guard
        self.mid = mid
        self.guard2 = guard2
        self.extras = extras or []  # list of (msg_index, guard)


class Sequence(object):
    def __init__(self, code, title, subtitle, actors):
        self.code = code
        self.title = title
        self.subtitle = subtitle
        self.actors = actors
        self.messages = []
        self.fragments = []

    def call(self, src, tgt, text):
        self.messages.append(Message(src, tgt, text, False))

    def ret(self, src, tgt, text):
        self.messages.append(Message(src, tgt, text, True))

    def alt(self, start, end, guard, mid, guard2):
        self.fragments.append(Fragment("alt", start, end, guard, mid, guard2))

    def opt(self, start, end, guard):
        self.fragments.append(Fragment("opt", start, end, guard))

    def loop(self, start, end, guard):
        self.fragments.append(Fragment("loop", start, end, guard))

    def alt3(self, start, end, g1, mid1, g2, mid2, g3):
        self.fragments.append(Fragment("alt", start, end, g1, mid1, g2, [(mid2, g3)]))

    def n(self):
        return len(self.actors)

    def cx(self, i):
        return LEFT + i * GAP + HEAD_W / 2.0

    def page_w(self):
        return LEFT * 2 + (self.n() - 1) * GAP + HEAD_W

    def layout(self):
        y = TOP + HEAD_H + 36
        for i, m in enumerate(self.messages):
            extra = 10 if any(
                f.start == i or (f.mid is not None and f.mid == i) or any(ex[0] == i for ex in f.extras)
                for f in self.fragments
            ) else 0
            y += extra
            m.y = y
            y += STEP
        return y + TAIL

    def page_h(self):
        return self.layout()


def wrap_text(draw, text, max_w, fnt):
    words = text.split()
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


def svg_escape(t):
    return html.escape(t)


def render_svg(seq):
    h = seq.page_h()
    w = seq.page_w()
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<svg xmlns="http://www.w3.org/2000/svg" width="{0}" height="{1}" viewBox="0 0 {0} {1}">'.format(w, h),
        "<style>.t{font-family:'Times New Roman',Times,serif;fill:#000}.title{font-size:20px;font-weight:bold}"
        ".sub{font-size:12px}.act{font-size:13px;font-weight:bold}.lab{font-size:12px}"
        ".fr{font-size:11px;font-style:italic}.kind{font-size:11px;font-weight:bold}</style>",
        '<defs><marker id="arr" markerWidth="8" markerHeight="8" refX="7" refY="3" orient="auto">'
        '<path d="M0,0 L8,3 L0,6 Z" fill="#000"/></marker></defs>',
        '<rect width="{0}" height="{1}" fill="#fff"/>'.format(w, h),
        '<text class="t title" x="{0}" y="28" text-anchor="middle">{1}</text>'.format(w / 2, svg_escape(seq.title)),
        '<text class="t sub" x="{0}" y="50" text-anchor="middle">{1}</text>'.format(w / 2, svg_escape(seq.subtitle)),
    ]

    life_top = TOP
    life_bot = h - 24
    for i, name in enumerate(seq.actors):
        x = LEFT + i * GAP
        cx = seq.cx(i)
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1.4"/>'.format(
                x, life_top, HEAD_W, HEAD_H
            )
        )
        parts.append(
            '<text class="t act" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                cx, life_top + 26, svg_escape(name)
            )
        )
        parts.append(
            '<line x1="{0}" y1="{1}" x2="{0}" y2="{2}" stroke="#000" stroke-width="1" stroke-dasharray="5 4"/>'.format(
                cx, life_top + HEAD_H, life_bot
            )
        )

    for fr in seq.fragments:
        y1 = seq.messages[fr.start].y - 22
        y2 = seq.messages[fr.end].y + 18
        x1 = LEFT - 18
        x2 = LEFT + (seq.n() - 1) * GAP + HEAD_W + 18
        parts.append(
            '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="none" stroke="#000" stroke-width="1.15"/>'.format(
                x1, y1, x2 - x1, y2 - y1
            )
        )
        tab = 44 if fr.kind != "loop" else 48
        parts.append(
            '<polygon points="{0},{1} {2},{1} {3},{4} {0},{4}" fill="#fff" stroke="#000" stroke-width="1.15"/>'.format(
                x1, y1, x1 + tab, x1 + tab - 8, y1 + 16
            )
        )
        parts.append(
            '<text class="t kind" x="{0}" y="{1}">{2}</text>'.format(x1 + 6, y1 + 13, fr.kind)
        )
        if fr.guard:
            parts.append(
                '<text class="t fr" x="{0}" y="{1}">[{2}]</text>'.format(x1 + 56, y1 + 14, svg_escape(fr.guard))
            )
        if fr.mid is not None:
            my = seq.messages[fr.mid].y - 22
            parts.append(
                '<line x1="{0}" y1="{1}" x2="{2}" y2="{1}" stroke="#000" stroke-width="1" stroke-dasharray="6 4"/>'.format(
                    x1, my, x2
                )
            )
            if fr.guard2:
                parts.append(
                    '<text class="t fr" x="{0}" y="{1}">[{2}]</text>'.format(x1 + 56, my + 14, svg_escape(fr.guard2))
                )
        for mid_i, g in fr.extras:
            my = seq.messages[mid_i].y - 22
            parts.append(
                '<line x1="{0}" y1="{1}" x2="{2}" y2="{1}" stroke="#000" stroke-width="1" stroke-dasharray="6 4"/>'.format(
                    x1, my, x2
                )
            )
            if g:
                parts.append(
                    '<text class="t fr" x="{0}" y="{1}">[{2}]</text>'.format(x1 + 56, my + 14, svg_escape(g))
                )

    for m in seq.messages:
        x1, x2 = seq.cx(m.src), seq.cx(m.tgt)
        y = m.y
        dash = ' stroke-dasharray="6 4"' if m.dashed else ""
        if m.src == m.tgt:
            x = x1 + 8
            parts.append(
                '<polyline points="{0},{1} {2},{1} {2},{3} {0},{3}" fill="none" stroke="#000" stroke-width="1.2"{4} marker-end="url(#arr)"/>'.format(
                    x, y, x + 36, y + 18, dash
                )
            )
            parts.append(
                '<text class="t lab" x="{0}" y="{1}">{2}</text>'.format(x + 42, y + 8, svg_escape(m.text))
            )
        else:
            parts.append(
                '<line x1="{0}" y1="{1}" x2="{2}" y2="{1}" stroke="#000" stroke-width="1.2"{3} marker-end="url(#arr)"/>'.format(
                    x1, y, x2, dash
                )
            )
            mx = (x1 + x2) / 2.0
            parts.append(
                '<text class="t lab" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(
                    mx, y - 7, svg_escape(m.text)
                )
            )
    parts.append("</svg>")
    return "\n".join(parts)


def dashed_line(dr, a, b, s, dash=7):
    x1, y1 = a
    x2, y2 = b
    dx, dy = x2 - x1, y2 - y1
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    pos = 0
    on = True
    step = dash * s
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


def arrow_head(dr, x1, y1, x2, y2, s):
    dx, dy = x2 - x1, y2 - y1
    length = (dx * dx + dy * dy) ** 0.5 or 1
    ux, uy = dx / length, dy / length
    px, py = -uy, ux
    p1 = (x2, y2)
    p2 = (x2 - 9 * s * ux + 4.5 * s * px, y2 - 9 * s * uy + 4.5 * s * py)
    p3 = (x2 - 9 * s * ux - 4.5 * s * px, y2 - 9 * s * uy - 4.5 * s * py)
    dr.polygon([p1, p2, p3], fill=(0, 0, 0))


def render_png(seq):
    w, h = int(seq.page_w()), int(seq.page_h())
    s = SCALE
    img = Image.new("RGB", (w * s, h * s), (255, 255, 255))
    dr = ImageDraw.Draw(img)
    f_title, f_sub = font(22 * s, True), font(14 * s)
    f_act, f_lab, f_fr, f_kind = font(14 * s, True), font(13 * s), font(12 * s), font(12 * s, True)

    def midtext(text, x, y, fnt):
        tw = dr.textlength(text, font=fnt)
        dr.text((x - tw / 2, y), text, fill=(0, 0, 0), font=fnt)

    midtext(seq.title, (w * s) / 2, 8 * s, f_title)
    midtext(seq.subtitle, (w * s) / 2, 36 * s, f_sub)

    life_top = TOP
    life_bot = h - 24
    for i, name in enumerate(seq.actors):
        x = LEFT + i * GAP
        cx = seq.cx(i)
        dr.rectangle(
            [x * s, life_top * s, (x + HEAD_W) * s, (life_top + HEAD_H) * s],
            outline=(0, 0, 0),
            width=2 * s,
            fill=(255, 255, 255),
        )
        midtext(name, cx * s, (life_top + 12) * s, f_act)
        dashed_line(dr, (cx * s, (life_top + HEAD_H) * s), (cx * s, life_bot * s), s)

    for fr in seq.fragments:
        y1 = seq.messages[fr.start].y - 22
        y2 = seq.messages[fr.end].y + 18
        x1 = LEFT - 18
        x2 = LEFT + (seq.n() - 1) * GAP + HEAD_W + 18
        dr.rectangle([x1 * s, y1 * s, x2 * s, y2 * s], outline=(0, 0, 0), width=2 * s)
        tab = 44
        dr.polygon(
            [(x1 * s, y1 * s), ((x1 + tab) * s, y1 * s), ((x1 + tab - 8) * s, (y1 + 16) * s), (x1 * s, (y1 + 16) * s)],
            outline=(0, 0, 0),
            fill=(255, 255, 255),
        )
        dr.text(((x1 + 6) * s, (y1 + 2) * s), fr.kind, fill=(0, 0, 0), font=f_kind)
        if fr.guard:
            dr.text(((x1 + 56) * s, (y1 + 2) * s), "[" + fr.guard + "]", fill=(0, 0, 0), font=f_fr)
        if fr.mid is not None:
            my = seq.messages[fr.mid].y - 22
            dashed_line(dr, (x1 * s, my * s), (x2 * s, my * s), s)
            if fr.guard2:
                dr.text(((x1 + 56) * s, (my + 2) * s), "[" + fr.guard2 + "]", fill=(0, 0, 0), font=f_fr)
        for mid_i, g in fr.extras:
            my = seq.messages[mid_i].y - 22
            dashed_line(dr, (x1 * s, my * s), (x2 * s, my * s), s)
            if g:
                dr.text(((x1 + 56) * s, (my + 2) * s), "[" + g + "]", fill=(0, 0, 0), font=f_fr)

    for m in seq.messages:
        x1, x2 = seq.cx(m.src), seq.cx(m.tgt)
        y = m.y
        if m.src == m.tgt:
            pts = [(x1 + 8) * s, y * s, (x1 + 44) * s, y * s, (x1 + 44) * s, (y + 18) * s, (x1 + 8) * s, (y + 18) * s]
            dr.line([(pts[0], pts[1]), (pts[2], pts[3])], fill=(0, 0, 0), width=2 * s)
            dr.line([(pts[2], pts[3]), (pts[4], pts[5])], fill=(0, 0, 0), width=2 * s)
            dr.line([(pts[4], pts[5]), (pts[6], pts[7])], fill=(0, 0, 0), width=2 * s)
            arrow_head(dr, pts[4], pts[5], pts[6], pts[7], s)
            dr.text(((x1 + 50) * s, (y + 2) * s), m.text, fill=(0, 0, 0), font=f_lab)
        else:
            if m.dashed:
                dashed_line(dr, (x1 * s, y * s), (x2 * s, y * s), s)
            else:
                dr.line([(x1 * s, y * s), (x2 * s, y * s)], fill=(0, 0, 0), width=2 * s)
            arrow_head(dr, x1 * s, y * s, x2 * s, y * s, s)
            mx = (x1 + x2) / 2.0
            max_w = abs(x2 - x1) * s - 16
            lines = wrap_text(dr, m.text, max(max_w, 80 * s), f_lab)
            total = len(lines) * 15 * s
            ty = y * s - 8 * s - total
            for i, line in enumerate(lines):
                midtext(line, mx * s, ty + i * 15 * s, f_lab)
    return img


BW = "fillColor=#ffffff;strokeColor=#000000;fontColor=#000000;fontFamily=Times New Roman;"


def render_drawio(seq):
    h = seq.page_h()
    w = seq.page_w()
    cells = []
    n = [2]

    def nid():
        i = "n{0}".format(n[0])
        n[0] += 1
        return i

    def cell(style, value, x, y, ww, hh):
        i = nid()
        cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" vertex="1" parent="1">'
            '<mxGeometry x="{3}" y="{4}" width="{5}" height="{6}" as="geometry"/></mxCell>'.format(
                i, html.escape(value), style, int(x), int(y), int(ww), int(hh)
            )
        )
        return i

    def edge(style, value, src, tgt, y=None):
        i = nid()
        geo = '<mxGeometry relative="1" as="geometry"/>'
        if y is not None:
            geo = (
                '<mxGeometry relative="1" as="geometry"><mxPoint x="0" y="{0}" as="offset"/></mxGeometry>'.format(int(y))
            )
        cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" edge="1" parent="1" source="{3}" target="{4}">'
            "{5}</mxCell>".format(i, html.escape(value), style, src, tgt, geo)
        )
        return i

    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=18;fontStyle=1;fontColor=#000000;",
        seq.title, 20, 10, w - 40, 26,
    )
    cell(
        "text;html=1;strokeColor=none;fillColor=none;align=center;fontFamily=Times New Roman;fontSize=12;fontColor=#000000;",
        seq.subtitle, 20, 38, w - 40, 20,
    )

    life_ids = []
    life_bot = h - 24
    life_h = life_bot - TOP
    for i, name in enumerate(seq.actors):
        x = LEFT + i * GAP
        lid = cell(
            "shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=0;"
            "collapsible=0;recursiveResize=0;outlineConnect=0;size=" + str(HEAD_H) + ";"
            + BW + "fontSize=12;fontStyle=1;verticalAlign=top;spacingTop=10;",
            name, x, TOP, HEAD_W, life_h,
        )
        life_ids.append(lid)

    for fr in seq.fragments:
        y1 = seq.messages[fr.start].y - 22
        y2 = seq.messages[fr.end].y + 18
        x1 = LEFT - 18
        label = fr.kind
        if fr.guard:
            label = fr.kind + " [" + fr.guard + "]"
        if fr.guard2:
            label = label + " / [" + fr.guard2 + "]"
        for _mid, g in fr.extras:
            if g:
                label = label + " / [" + g + "]"
        cell(
            "shape=umlFrame;whiteSpace=wrap;html=1;width=40;height=20;boundedLbl=1;verticalAlign=middle;"
            "align=left;spacingLeft=4;" + BW + "fontSize=11;",
            label, x1, y1, (LEFT + (seq.n() - 1) * GAP + HEAD_W + 18) - x1, y2 - y1,
        )

    sync = (
        "html=1;endArrow=block;endFill=1;startArrow=none;strokeColor=#000000;fontColor=#000000;"
        "fontFamily=Times New Roman;fontSize=11;verticalAlign=bottom;exitX=0.5;exitY=1;entryX=0.5;entryY=1;"
    )
    async_ = (
        "html=1;endArrow=open;endFill=0;dashed=1;dashPattern=6 4;strokeColor=#000000;fontColor=#000000;"
        "fontFamily=Times New Roman;fontSize=11;verticalAlign=bottom;exitX=0.5;exitY=1;entryX=0.5;entryY=1;"
    )
    for m in seq.messages:
        style = async_ if m.dashed else sync
        if m.src == m.tgt:
            style = style + "edgeStyle=orthogonalEdgeStyle;"
        edge(style, m.text, life_ids[m.src], life_ids[m.tgt], m.y - TOP)

    return (
        '  <diagram id="{0}" name="{1}">\n'
        '    <mxGraphModel dx="1400" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" '
        'arrows="1" fold="1" page="1" pageScale="1" pageWidth="{2}" pageHeight="{3}" math="0" shadow="0">\n'
        "      <root>\n        <mxCell id=\"0\"/>\n        <mxCell id=\"1\" parent=\"0\"/>\n"
        "{4}\n      </root>\n    </mxGraphModel>\n  </diagram>"
    ).format(seq.code.lower(), html.escape(seq.code), int(w), int(h), "\n".join(cells))


def mxfile(xmls):
    return (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<mxfile host="app.diagrams.net" agent="DTAS" version="22.1.0" type="device">\n'
        + "\n".join(xmls)
        + "\n</mxfile>\n"
    )


# ---------- diagrams (message indexes are 0-based) ----------
def sd1():
    s = Sequence(
        "SD-1",
        "Figure SD-1. Register, verify email, and verify identity",
        "Campus modules stay locked until email and institutional ID are approved.",
        ["User", "AuthService", "EmailOtpService", "SQL Server", "System Admin"],
    )
    s.call(0, 1, "Register(name, email, password, role)")
    s.call(1, 3, "CreateSelfRegisteredUser()")
    s.ret(3, 1, "userId")
    s.call(1, 2, "Send(userId, PurposeRegister)")
    s.call(2, 3, "Store OTP hash")
    s.ret(2, 0, "OTP email")
    s.call(0, 2, "Verify(otp)")
    s.call(2, 1, "MarkEmailVerified()")
    s.call(1, 3, "EmailVerified = 1")
    s.call(0, 1, "Upload institutional ID")
    s.call(1, 3, "Insert identity document (Queued)")
    s.call(4, 1, "Review identity document")
    s.call(1, 3, "Set identity Approved")
    s.ret(1, 0, "Campus modules unlocked")
    s.call(1, 3, "Set identity Rejected")
    s.ret(1, 0, "Account stays gated")
    s.alt(12, 15, "Approved", 14, "Rejected")
    return s


def sd2():
    s = Sequence(
        "SD-2",
        "Figure SD-2. Student proposes an event; admin decides",
        "Faculty and staff create events directly. Students submit Status = Proposed.",
        ["Student", "EventService", "SQL Server", "System Admin"],
    )
    s.call(0, 1, "Submit propose-event form (faculty/staff lead)")
    s.call(1, 2, "INSERT Events Status = Proposed")
    s.ret(2, 1, "eventId")
    s.ret(1, 3, "Notify proposal")
    s.call(3, 1, "Review proposal")
    s.call(1, 2, "Status = Planned; add event lead")
    s.ret(1, 0, "Notify approved")
    s.call(1, 2, "Status = Rejected with reason")
    s.ret(1, 0, "Notify rejected")
    s.alt(5, 8, "Approve", 7, "Reject")
    return s


def sd3():
    s = Sequence(
        "SD-3",
        "Figure SD-3. Join a public campus event",
        "Public join is a request. Invite code joins immediately.",
        ["Campus user", "EventService", "SQL Server", "Event Admin"],
    )
    s.call(0, 1, "Open public event")
    s.call(0, 1, "JoinByCode(code)")
    s.call(1, 2, "Add member InviteStatus = Accepted")
    s.ret(1, 0, "Joined")
    s.call(0, 1, "JoinPublic(eventId)")
    s.call(1, 2, "InviteStatus = Requested")
    s.ret(1, 3, "Join request")
    s.call(3, 1, "AcceptJoinRequest() or DeclineJoinRequest()")
    s.call(1, 2, "Set Accepted or Declined")
    s.ret(1, 0, "Notify decision")
    s.alt(1, 9, "Has invite code", 4, "No code: request")
    return s


def sd4():
    s = Sequence(
        "SD-4",
        "Figure SD-4. Schedule a Zoom meeting for an event",
        "DTAS creates the Zoom room, sends the join link, and closes the room when duration ends.",
        ["Host", "MeetingService", "Zoom API", "SQL Server", "Event members"],
    )
    s.call(0, 1, "Create(event, title, time, duration)")
    s.call(1, 2, "GetAccessToken()")
    s.ret(2, 1, "bearer token")
    s.call(1, 2, "POST /v2/users/me/meetings")
    s.ret(2, 1, "joinUrl, startUrl")
    s.call(1, 3, "INSERT Meetings; add accepted members")
    s.ret(1, 4, "Notify join link")
    s.ret(1, 0, "Meeting scheduled")
    s.call(1, 1, "CloseExpiredMeetings()")
    s.call(1, 2, "EndAndDeleteMeeting()")
    s.call(1, 3, "Status = Completed; clear Start/Join")
    s.ret(2, 1, "error")
    s.ret(1, 0, "Show Zoom error and stop")
    s.alt(4, 12, "Room created", 11, "Create failed")
    return s


def sd5():
    s = Sequence(
        "SD-5",
        "Figure SD-5. Create a club or request to join",
        "Admin cannot create clubs. Public join is a request. Invite code joins immediately.",
        ["Campus user", "ClubService", "SQL Server", "Club lead"],
    )
    s.call(0, 1, "CreateClub(name, visibility)")
    s.call(1, 2, "INSERT Clubs; creator becomes lead")
    s.ret(1, 0, "Club created")
    s.call(0, 1, "JoinByCode(code)")
    s.call(1, 2, "Add active member")
    s.ret(1, 0, "Joined")
    s.call(0, 1, "JoinPublic(clubId)")
    s.call(1, 2, "Store pending request")
    s.ret(1, 3, "Join request")
    s.call(3, 1, "AcceptJoinRequest() or DeclineJoinRequest()")
    s.call(1, 2, "Activate or decline membership")
    s.ret(1, 0, "Notify decision")
    s.alt3(0, 11, "Create", 3, "Join with code", 6, "Join public (request)")
    return s


def sd6():
    s = Sequence(
        "SD-6",
        "Figure SD-6. Faculty assignment and computed contribution",
        "Contribution is calculated (completion, timeliness, activity, recency). It is not typed in.",
        ["Faculty", "AssignmentService", "SQL Server", "Student"],
    )
    s.call(0, 1, "CreateAssignment(name, deadline)")
    s.call(1, 2, "INSERT Assignments and groups")
    s.ret(1, 3, "Invite or place in group")
    s.call(3, 1, "SubmitWork(task, files, links)")
    s.call(1, 2, "Record completion, dates, activity")
    s.call(3, 1, "Complete group tasks")
    s.call(1, 2, "Record activity")
    s.loop(5, 6, "Until deadline")
    s.call(0, 1, "GetContribution(groupId)")
    s.call(1, 2, "sp_GetMemberContributionReport")
    s.ret(2, 1, "Computed percents")
    s.ret(1, 0, "Contribution report")
    s.ret(1, 3, "View own contribution")
    return s


def sd7():
    s = Sequence(
        "SD-7",
        "Figure SD-7. Flag content and admin moderation",
        "Users flag content. Admin reviews the queue. Admin cannot moderate another admin.",
        ["Campus user", "ModerationService", "SQL Server", "System Admin"],
    )
    s.call(0, 1, "SubmitReport(type, id, reason)")
    s.call(1, 2, "INSERT content report (Pending)")
    s.ret(1, 3, "Flag in queue")
    s.call(3, 1, "Review flag")
    s.call(1, 2, "ResolveReport(Dismissed)")
    s.call(3, 2, "Restrict event, club, task, or decision")
    s.call(3, 1, "Suspend() or Ban()")
    s.call(1, 2, "Update user status")
    s.alt(4, 7, "Dismiss", 5, "Restrict or suspend")
    s.call(1, 2, "Write audit log")
    return s


NAMES = {
    "SD-1": "SD1-Identity",
    "SD-2": "SD2-ProposeEvent",
    "SD-3": "SD3-JoinEvent",
    "SD-4": "SD4-MeetingZoom",
    "SD-5": "SD5-Clubs",
    "SD-6": "SD6-Assignment",
    "SD-7": "SD7-Moderation",
}

CAPTIONS = {
    "SD-1": "Figure SD-1. Register, email OTP, and institutional ID review.",
    "SD-2": "Figure SD-2. Student proposes an event. Admin approves or rejects.",
    "SD-3": "Figure SD-3. Join a public event by request or invitation code.",
    "SD-4": "Figure SD-4. Schedule a Zoom meeting, send the join link, close the room after duration.",
    "SD-5": "Figure SD-5. Create a club or request to join. Admin cannot create clubs.",
    "SD-6": "Figure SD-6. Faculty assignment. Contribution is computed, not typed in.",
    "SD-7": "Figure SD-7. Flag content and admin moderation.",
}


def write(name, data):
    path = os.path.join(OUT, name)
    with open(path, "w", encoding="utf-8") as f:
        f.write(data)
    print(path)
    return path


def main():
    diagrams = [sd1(), sd2(), sd3(), sd4(), sd5(), sd6(), sd7()]
    xmls = []
    files = []
    for seq in diagrams:
        names = NAMES[seq.code]
        write(names + ".svg", render_svg(seq))
        png_path = os.path.join(OUT, names + ".png")
        render_png(seq).save(png_path, dpi=(150, 150))
        print(png_path)
        xml = render_drawio(seq)
        xmls.append(xml)
        write(names + ".drawio", mxfile([xml]))
        files.append((names, CAPTIONS[seq.code]))

    write("DTAS-Sequence-Diagrams.drawio", mxfile(xmls))

    figures = "".join(
        "<figure><img src='{0}.svg' alt='{1}'/><figcaption>{1}</figcaption></figure>\n".format(n, cap)
        for n, cap in files
    )
    write(
        "DTAS-Sequence-Diagrams.html",
        """<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>DTAS Sequence Diagrams (black and white)</title>
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
  <h1>DTAS sequence diagrams</h1>
  <p>Black-and-white UML sequence diagrams for the Digital Transparency and Accountability System. PNG, SVG, and draw.io files are in this folder. Open this file in a browser and print to PDF if you need a report copy.</p>
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
