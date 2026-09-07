# Generates black-and-white draw.io (.drawio) use case diagrams for DTAS.
from __future__ import print_function
import os
import html

OUT = os.path.dirname(os.path.abspath(__file__))

BW = "fillColor=#ffffff;strokeColor=#000000;fontColor=#000000;fontFamily=Times New Roman;"
ACTOR = (
    "shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;"
    + BW + "fontSize=12;fontStyle=1;"
)
USECASE = "ellipse;whiteSpace=wrap;html=1;" + BW + "fontSize=11;align=center;"
SYSTEM = (
    "rounded=0;whiteSpace=wrap;html=1;verticalAlign=top;fillColor=none;strokeColor=#000000;"
    "fontColor=#000000;fontFamily=Times New Roman;fontSize=13;fontStyle=1;spacingTop=6;"
)
NOTE = (
    "shape=note;whiteSpace=wrap;html=1;size=16;align=left;spacingLeft=8;spacingTop=4;"
    + BW + "fontSize=11;"
)
TITLE = (
    "text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;"
    "whiteSpace=wrap;fontFamily=Times New Roman;fontSize=18;fontStyle=1;fontColor=#000000;"
)
SUB = (
    "text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;"
    "whiteSpace=wrap;fontFamily=Times New Roman;fontSize=12;fontColor=#000000;"
)
ASSOC = "endArrow=none;html=1;strokeColor=#000000;strokeWidth=1;exitX=1;exitY=0.35;exitDx=0;exitDy=0;"
ASSOC_L = "endArrow=none;html=1;strokeColor=#000000;strokeWidth=1;exitX=0;exitY=0.35;exitDx=0;exitDy=0;"
INCLUDE = (
    "endArrow=open;html=1;dashed=1;dashPattern=8 8;strokeColor=#000000;fontColor=#000000;"
    "fontFamily=Times New Roman;fontSize=10;fontStyle=2;"
)
GEN = "endArrow=block;endFill=0;html=1;strokeColor=#000000;strokeWidth=1.2;"


class Diagram(object):
    def __init__(self, name, page_w=1650, page_h=1050):
        self.name = name
        self.page_w = page_w
        self.page_h = page_h
        self.cells = []
        self.n = 2

    def _id(self):
        i = "c{0}".format(self.n)
        self.n += 1
        return i

    def add(self, style, value, x, y, w, h):
        i = self._id()
        self.cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" vertex="1" parent="1">'
            '<mxGeometry x="{3}" y="{4}" width="{5}" height="{6}" as="geometry"/></mxCell>'.format(
                i, html.escape(value), style, x, y, w, h
            )
        )
        return i

    def title(self, text, y=20):
        return self.add(TITLE, text, 40, y, self.page_w - 80, 30)

    def subtitle(self, text, y=50):
        return self.add(SUB, text, 40, y, self.page_w - 80, 24)

    def system(self, label, x, y, w, h):
        return self.add(SYSTEM, label, x, y, w, h)

    def actor(self, label, x, y):
        return self.add(ACTOR, label, x, y, 40, 70)

    def usecase(self, label, x, y, w=230, h=56):
        return self.add(USECASE, label, x, y, w, h)

    def note(self, text, x, y, w=320, h=90):
        return self.add(NOTE, text, x, y, w, h)

    def edge(self, src, tgt, style=ASSOC, value=""):
        i = self._id()
        self.cells.append(
            '        <mxCell id="{0}" value="{1}" style="{2}" edge="1" parent="1" source="{3}" target="{4}">'
            '<mxGeometry relative="1" as="geometry"/></mxCell>'.format(
                i, html.escape(value), style, src, tgt
            )
        )
        return i

    def assoc(self, actor_id, uc_id, from_right=False):
        return self.edge(actor_id, uc_id, ASSOC_L if from_right else ASSOC)

    def include(self, src, tgt):
        return self.edge(src, tgt, INCLUDE, "<<include>>")

    def generalize(self, child, parent):
        return self.edge(child, parent, GEN)

    def xml(self, diagram_id):
        body = "\n".join(self.cells)
        return (
            '  <diagram id="{0}" name="{1}">\n'
            '    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" '
            'connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="{2}" pageHeight="{3}" '
            'math="0" shadow="0">\n'
            '      <root>\n'
            '        <mxCell id="0"/>\n'
            '        <mxCell id="1" parent="0"/>\n'
            '{4}\n'
            '      </root>\n'
            '    </mxGraphModel>\n'
            '  </diagram>'
        ).format(diagram_id, html.escape(self.name), self.page_w, self.page_h, body)


def uc1():
    d = Diagram("UC-1 High Level", 1650, 1000)
    d.title("Figure UC-1. DTAS high-level use case diagram")
    d.subtitle("Black and white UML. Student, Faculty, and Staff specialise Campus user.")
    d.system("Digital Transparency and Accountability System (DTAS)", 360, 90, 820, 760)
    guest = d.actor("Guest", 80, 130)
    campus = d.actor("Campus user", 80, 360)
    student = d.actor("Student", 30, 620)
    faculty = d.actor("Faculty", 120, 620)
    staff = d.actor("Staff", 210, 620)
    admin = d.actor("System Admin", 1520, 380)
    d.generalize(student, campus)
    d.generalize(faculty, campus)
    d.generalize(staff, campus)

    u = [
        d.usecase("Register and sign in", 430, 130),
        d.usecase("Verify institutional identity", 430, 210),
        d.usecase("Create or propose campus event", 430, 290),
        d.usecase("Create and manage clubs", 430, 370),
        d.usecase("Schedule and join meetings", 430, 450),
        d.usecase("Complete tasks and assignments", 430, 530),
        d.usecase("Vote and view transparency", 430, 610),
        d.usecase("Submit feedback or flag content", 430, 700),
    ]
    a = [
        d.usecase("Review identity documents", 880, 170),
        d.usecase("Approve event proposals", 880, 270),
        d.usecase("Restrict or restore records", 880, 370),
        d.usecase("Manage users and flags", 880, 470),
        d.usecase("Publish decisions and polls", 880, 570),
        d.usecase("View live transparency dashboard", 880, 670),
    ]
    d.assoc(guest, u[0])
    for x in u:
        d.assoc(campus, x)
    for x in a:
        d.assoc(admin, x, True)
    d.assoc(admin, u[6], True)
    d.note(
        "Faculty and staff create events.\nStudents propose events.\nAdmin does not create events or clubs.",
        20, 820, 320, 100,
    )
    return d


def uc2():
    d = Diagram("UC-2 Authentication", 1550, 920)
    d.title("Figure UC-2. Authentication and identity")
    d.subtitle("Email verification and institutional ID gate access to campus modules.")
    d.system("DTAS Authentication", 320, 90, 860, 680)
    guest = d.actor("Guest", 80, 180)
    user = d.actor("Campus user", 80, 480)
    admin = d.actor("System Admin", 1400, 340)

    reg = d.usecase("Register account", 380, 140)
    sign = d.usecase("Sign in", 380, 230)
    google = d.usecase("Sign in with Google", 380, 320)
    reset = d.usecase("Reset password", 380, 410)
    email = d.usecase("Verify email (OTP)", 700, 180)
    upload = d.usecase("Upload institutional ID", 700, 360)
    profile = d.usecase("Update profile", 700, 470)
    logout = d.usecase("Sign out", 700, 580)
    review = d.usecase("Review identity queue", 1000, 340)

    d.assoc(guest, reg)
    d.assoc(guest, sign)
    d.assoc(guest, google)
    d.assoc(guest, reset)
    d.assoc(user, sign)
    d.assoc(user, upload)
    d.assoc(user, profile)
    d.assoc(user, logout)
    d.assoc(admin, review, True)
    d.include(sign, email)
    d.include(upload, email)
    d.note(
        "Non-admin users must verify email, then upload ID, before events, tasks, polls, meetings, and clubs.",
        20, 760, 340, 90,
    )
    return d


def uc3():
    d = Diagram("UC-3 Events", 1650, 1000)
    d.title("Figure UC-3. Campus events")
    d.subtitle("Faculty and staff create events. Students propose. Admin approves proposals and may restrict records.")
    d.system("DTAS Events", 330, 90, 960, 760)
    fs = d.actor("Faculty / Staff", 70, 150)
    st = d.actor("Student", 70, 400)
    ea = d.actor("Event Admin", 70, 680)
    admin = d.actor("System Admin", 1520, 400)

    create = d.usecase("Create event", 390, 130)
    propose = d.usecase("Propose event", 390, 220)
    browse = d.usecase("Browse campus events", 390, 310)
    request = d.usecase("Request to join event", 390, 400)
    code = d.usecase("Accept invitation code", 390, 490)
    workspace = d.usecase("Open event workspace", 700, 160)
    accept = d.usecase("Accept or decline join requests", 700, 270)
    invite = d.usecase("Invite members", 700, 380)
    roles = d.usecase("Assign event roles", 700, 490)
    club = d.usecase("Link club to event", 700, 600)
    approve = d.usecase("Approve or reject proposal", 1020, 220)
    restrict = d.usecase("Restrict or restore event", 1020, 380)
    viewall = d.usecase("View all events", 1020, 540)

    d.assoc(fs, create)
    d.assoc(fs, browse)
    d.assoc(fs, workspace)
    d.assoc(fs, accept)
    d.assoc(fs, invite)
    d.assoc(st, propose)
    d.assoc(st, browse)
    d.assoc(st, request)
    d.assoc(st, code)
    d.assoc(st, workspace)
    d.assoc(ea, workspace)
    d.assoc(ea, accept)
    d.assoc(ea, invite)
    d.assoc(ea, roles)
    d.assoc(ea, club)
    d.assoc(admin, approve, True)
    d.assoc(admin, restrict, True)
    d.assoc(admin, viewall, True)
    d.include(create, workspace)
    d.note(
        "Admin cannot create events.\nPublic join is a request. Event Admin or Event Manager accepts members.",
        20, 850, 360, 90,
    )
    return d


def uc4():
    d = Diagram("UC-4 Meetings", 1550, 920)
    d.title("Figure UC-4. Meetings and Zoom rooms")
    d.subtitle("A meeting is hosted for an event. DTAS creates the Zoom room and closes it when duration ends.")
    d.system("DTAS Meetings", 330, 90, 860, 680)
    host = d.actor("Host", 80, 230)
    member = d.actor("Event member", 80, 540)
    zoom = d.actor("Zoom API", 1400, 380)

    schedule = d.usecase("Schedule meeting for an event", 390, 140)
    start = d.usecase("Start as host", 390, 250)
    minutes = d.usecase("Write minutes", 390, 360)
    resend = d.usecase("Resend join link", 390, 470)
    createz = d.usecase("Create Zoom room", 740, 140)
    send = d.usecase("Send join link to event members", 740, 250)
    join = d.usecase("Join Zoom room", 740, 400)
    close = d.usecase("Close room after scheduled duration", 740, 540)

    d.assoc(host, schedule)
    d.assoc(host, start)
    d.assoc(host, minutes)
    d.assoc(host, resend)
    d.assoc(member, join)
    d.assoc(zoom, createz, True)
    d.assoc(zoom, close, True)
    d.include(schedule, createz)
    d.include(schedule, send)
    d.include(close, join)
    d.note(
        "Only the host writes minutes.\nWhen start time + duration ends, Start and Join are removed and the Zoom room is ended and deleted.",
        20, 760, 360, 100,
    )
    return d


def uc5():
    d = Diagram("UC-5 Clubs and Connect", 1550, 920)
    d.title("Figure UC-5. Clubs and Connect")
    d.subtitle("Campus users create clubs. Admin may restrict a club but cannot create one.")
    d.system("DTAS Clubs and Connect", 330, 90, 860, 680)
    user = d.actor("Campus user", 80, 200)
    lead = d.actor("Club lead", 80, 520)
    admin = d.actor("System Admin", 1400, 360)

    create = d.usecase("Create club", 390, 130)
    req = d.usecase("Request to join public club", 390, 230)
    code = d.usecase("Join with invite code", 390, 330)
    leave = d.usecase("Leave club", 390, 430)
    connect = d.usecase("Open Connect group", 390, 530)
    approve = d.usecase("Approve or decline join requests", 720, 180)
    invite = d.usecase("Invite members", 720, 300)
    transfer = d.usecase("Transfer leadership", 720, 420)
    work = d.usecase("Use club workspace", 720, 540)
    restrict = d.usecase("Restrict or restore club", 1030, 340)

    d.assoc(user, create)
    d.assoc(user, req)
    d.assoc(user, code)
    d.assoc(user, leave)
    d.assoc(user, connect)
    d.assoc(user, work)
    d.assoc(lead, approve)
    d.assoc(lead, invite)
    d.assoc(lead, transfer)
    d.assoc(lead, work)
    d.assoc(admin, restrict, True)
    d.note(
        "Restricted clubs cannot message, invite, join, or link to events.\nMembers may still view and leave.",
        20, 760, 340, 90,
    )
    return d


def uc6():
    d = Diagram("UC-6 Work and governance", 1650, 1000)
    d.title("Figure UC-6. Tasks, assignments, decisions, and polls")
    d.subtitle("Contribution percentage is computed by the system. Admin does not invent scores.")
    d.system("DTAS Work and governance", 330, 90, 960, 760)
    em = d.actor("Event Manager", 70, 150)
    fac = d.actor("Faculty", 70, 400)
    stu = d.actor("Student", 70, 650)
    admin = d.actor("System Admin", 1520, 400)

    t1 = d.usecase("Create event task", 390, 130)
    t2 = d.usecase("Assign task members", 390, 220)
    t3 = d.usecase("Work in task workspace", 390, 310)
    t4 = d.usecase("Create assignment", 390, 410)
    t5 = d.usecase("Complete assignment group work", 390, 510)
    t6 = d.usecase("Vote on open poll", 390, 610)
    r1 = d.usecase("View contribution report", 700, 220)
    r2 = d.usecase("View published decision", 700, 370)
    r3 = d.usecase("Extend task deadline", 700, 520)
    a1 = d.usecase("Create or edit decision", 1020, 180)
    a2 = d.usecase("Create and close poll", 1020, 300)
    a3 = d.usecase("Restrict task or decision", 1020, 420)
    a4 = d.usecase("Oversee task list", 1020, 540)

    d.assoc(em, t1)
    d.assoc(em, t2)
    d.assoc(em, t3)
    d.assoc(em, r3)
    d.assoc(fac, t4)
    d.assoc(fac, r1)
    d.assoc(fac, r3)
    d.assoc(stu, t3)
    d.assoc(stu, t5)
    d.assoc(stu, t6)
    d.assoc(stu, r1)
    d.assoc(stu, r2)
    d.assoc(admin, a1, True)
    d.assoc(admin, a2, True)
    d.assoc(admin, a3, True)
    d.assoc(admin, a4, True)
    d.note(
        "Faculty create assignments. Staff do not.\nContribution uses stored formula (completion, timeliness, activity, recency).",
        20, 850, 380, 90,
    )
    return d


def uc7():
    d = Diagram("UC-7 Transparency and admin", 1550, 920)
    d.title("Figure UC-7. Transparency, feedback, and administration")
    d.subtitle("The community portal is the public gist. The admin dashboard is the live operations view.")
    d.system("DTAS Transparency and moderation", 330, 90, 860, 680)
    user = d.actor("Campus user", 80, 230)
    admin = d.actor("System Admin", 1400, 380)

    p1 = d.usecase("View community transparency portal", 390, 130)
    p2 = d.usecase("Submit feedback", 390, 230)
    p3 = d.usecase("Flag content", 390, 330)
    p4 = d.usecase("View own reports", 390, 430)
    p5 = d.usecase("Receive notifications", 390, 530)
    a1 = d.usecase("View live transparency dashboard", 740, 160)
    a2 = d.usecase("Respond to feedback", 740, 270)
    a3 = d.usecase("Review content flags", 740, 380)
    a4 = d.usecase("Suspend or ban user", 740, 490)
    a5 = d.usecase("Export reports / audit", 740, 600)

    d.assoc(user, p1)
    d.assoc(user, p2)
    d.assoc(user, p3)
    d.assoc(user, p4)
    d.assoc(user, p5)
    d.assoc(admin, a1, True)
    d.assoc(admin, a2, True)
    d.assoc(admin, a3, True)
    d.assoc(admin, a4, True)
    d.assoc(admin, a5, True)
    d.assoc(admin, p1, True)
    d.note(
        "Admin may open the community portal to see the same gist users see.\nPrivate files stay off Transparency.",
        20, 760, 360, 90,
    )
    return d


def mxfile(diagrams):
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<mxfile host="app.diagrams.net" agent="DTAS" version="22.1.0" type="device">',
    ]
    for i, d in enumerate(diagrams, 1):
        parts.append(d.xml("uc{0}".format(i)))
    parts.append("</mxfile>")
    return "\n".join(parts) + "\n"


def write(name, text):
    path = os.path.join(OUT, name)
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print(path)


if __name__ == "__main__":
    diagrams = [uc1(), uc2(), uc3(), uc4(), uc5(), uc6(), uc7()]
    write("DTAS-Use-Cases.drawio", mxfile(diagrams))
    names = [
        "UC1-HighLevel.drawio",
        "UC2-Authentication.drawio",
        "UC3-Events.drawio",
        "UC4-Meetings.drawio",
        "UC5-ClubsConnect.drawio",
        "UC6-WorkGovernance.drawio",
        "UC7-TransparencyAdmin.drawio",
    ]
    for name, diagram in zip(names, diagrams):
        write(name, mxfile([diagram]))
