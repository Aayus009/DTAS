# Generates black-and-white UML use case SVGs for DTAS.
from __future__ import print_function
import os

OUT = os.path.dirname(os.path.abspath(__file__))

STYLE = """
  .t { font-family: 'Times New Roman', Times, serif; fill: #000; }
  .title { font-size: 22px; font-weight: bold; }
  .sub { font-size: 13px; }
  .sys { font-size: 14px; font-weight: bold; }
  .uc { font-size: 12px; }
  .act { font-size: 13px; font-weight: bold; }
  .rel { font-size: 11px; font-style: italic; }
  .note { font-size: 11px; }
"""


def svg_open(w, h, title, subtitle):
    return [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<svg xmlns="http://www.w3.org/2000/svg" width="{0}" height="{1}" viewBox="0 0 {0} {1}">'.format(w, h),
        '<style>{0}</style>'.format(STYLE),
        '<rect x="0" y="0" width="{0}" height="{1}" fill="#fff"/>'.format(w, h),
        '<text class="t title" x="{0}" y="36" text-anchor="middle">{1}</text>'.format(w / 2, title),
        '<text class="t sub" x="{0}" y="58" text-anchor="middle">{1}</text>'.format(w / 2, subtitle),
    ]


def svg_close():
    return ['</svg>']


def actor(x, y, label):
    # stick figure; (x,y) is the head center
    return [
        '<circle cx="{0}" cy="{1}" r="10" fill="#fff" stroke="#000" stroke-width="1.6"/>'.format(x, y),
        '<line x1="{0}" y1="{1}" x2="{0}" y2="{2}" stroke="#000" stroke-width="1.6"/>'.format(x, y + 10, y + 38),
        '<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.6"/>'.format(x - 16, y + 22, x + 16, y + 22),
        '<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.6"/>'.format(x, y + 38, x - 14, y + 58),
        '<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.6"/>'.format(x, y + 38, x + 14, y + 58),
        '<text class="t act" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(x, y + 76, label),
    ]


def system_box(x, y, w, h, label):
    return [
        '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1.8"/>'.format(x, y, w, h),
        '<text class="t sys" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(x + w / 2.0, y + 22, label),
    ]


def usecase(cx, cy, rx, ry, lines):
    parts = [
        '<ellipse cx="{0}" cy="{1}" rx="{2}" ry="{3}" fill="#fff" stroke="#000" stroke-width="1.5"/>'.format(cx, cy, rx, ry)
    ]
    if isinstance(lines, str):
        lines = [lines]
    start = cy - (len(lines) - 1) * 7
    for i, line in enumerate(lines):
        parts.append(
            '<text class="t uc" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(cx, start + i * 14, line)
        )
    return parts


def line(x1, y1, x2, y2, dashed=False):
    dash = ' stroke-dasharray="6 4"' if dashed else ''
    return ['<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="#000" stroke-width="1.15"{4}/>'.format(x1, y1, x2, y2, dash)]


def assoc(ax, ay, ux, uy):
    # actor chest (~ y+22) to oval edge
    return line(ax + 18, ay + 22, ux - 118, uy)


def assoc_right(ax, ay, ux, uy):
    return line(ax - 18, ay + 22, ux + 118, uy)


def include_arrow(x1, y1, x2, y2, label='&lt;&lt;include&gt;&gt;'):
    parts = line(x1, y1, x2, y2, dashed=True)
    mx, my = (x1 + x2) / 2.0, (y1 + y2) / 2.0 - 8
    parts.append('<text class="t rel" x="{0}" y="{1}" text-anchor="middle">{2}</text>'.format(mx, my, label))
    return parts


def note(x, y, w, h, lines):
    parts = [
        '<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="#fff" stroke="#000" stroke-width="1"/>'.format(x, y, w, h)
    ]
    for i, text in enumerate(lines):
        parts.append('<text class="t note" x="{0}" y="{1}">{2}</text>'.format(x + 8, y + 18 + i * 14, text))
    return parts


def write(name, parts):
    path = os.path.join(OUT, name)
    with open(path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(parts))
    print(path)


def gen_line(x1, y1, x2, y2):
    return line(x1, y1, x2, y2)


def triangle(x, y):
    return ['<polygon points="{0},{1} {2},{3} {4},{5}" fill="#fff" stroke="#000" stroke-width="1.3"/>'.format(
        x, y, x - 8, y + 14, x + 8, y + 14)]


def overview():
    w, h = 1480, 880
    p = svg_open(w, h, 'Figure UC-1. DTAS high-level use case diagram',
                 'Black and white UML. Student, Faculty, and Staff specialise Campus user.')
    p += system_box(340, 88, 800, 720, 'Digital Transparency and Accountability System (DTAS)')
    p += actor(110, 140, 'Guest')
    p += actor(110, 360, 'Campus user')
    p += actor(60, 560, 'Student')
    p += actor(160, 560, 'Faculty')
    p += actor(260, 560, 'Staff')
    p += actor(1370, 380, 'System Admin')

    # generalization to Campus user
    p += line(60, 560, 110, 448)
    p += line(160, 560, 110, 448)
    p += line(260, 560, 110, 448)
    p += triangle(110, 434)

    cases = [
        (740, 150, ['Register and sign in']),
        (740, 230, ['Verify institutional identity']),
        (740, 310, ['Create or propose campus event']),
        (740, 390, ['Create and manage clubs']),
        (740, 470, ['Schedule and join meetings']),
        (740, 550, ['Complete tasks and assignments']),
        (740, 630, ['Vote and view transparency']),
        (740, 720, ['Submit feedback or flag content']),
    ]
    for cx, cy, lines in cases:
        p += usecase(cx, cy, 155, 28, lines)

    admin_cases = [
        (1080, 230, ['Review identity documents']),
        (1080, 330, ['Approve event proposals']),
        (1080, 430, ['Restrict or restore records']),
        (1080, 530, ['Manage users and flags']),
        (1080, 630, ['Publish decisions and polls']),
        (1080, 730, ['View live transparency dashboard']),
    ]
    for cx, cy, lines in admin_cases:
        p += usecase(cx, cy, 150, 30, lines)

    p += line(128, 162, 585, 150)
    for uy in (150, 230, 310, 390, 470, 550, 630, 720):
        p += line(128, 382, 585, uy)
    for uy in (230, 330, 430, 530, 630, 730):
        p += line(1352, 402, 1230, uy)
    p += line(1352, 402, 895, 630)

    p += note(20, 750, 300, 90, [
        'Faculty and staff create events.',
        'Students propose events.',
        'Admin does not create events or clubs.',
        'See UC-2 to UC-7 for detail.',
    ])
    p += svg_close()
    write('UC1-HighLevel.svg', p)


def auth():
    w, h = 1400, 820
    p = svg_open(w, h, 'Figure UC-2. Authentication and identity',
                 'Email verification and institutional ID gate access to campus modules.')
    p += system_box(300, 90, 800, 660, 'DTAS Authentication')
    p += actor(110, 200, 'Guest')
    p += actor(110, 480, 'Campus user')
    p += actor(1290, 360, 'System Admin')

    p += usecase(520, 170, 130, 28, ['Register account'])
    p += usecase(520, 250, 130, 28, ['Sign in'])
    p += usecase(520, 330, 130, 28, ['Sign in with Google'])
    p += usecase(520, 410, 130, 28, ['Reset password'])
    p += usecase(780, 210, 140, 30, ['Verify email (OTP)'])
    p += usecase(780, 370, 140, 30, ['Upload institutional ID'])
    p += usecase(780, 490, 140, 30, ['Update profile'])
    p += usecase(780, 600, 140, 30, ['Sign out'])
    p += usecase(1080, 360, 145, 32, ['Review identity queue'])

    p += line(128, 222, 390, 170)
    p += line(128, 222, 390, 250)
    p += line(128, 222, 390, 330)
    p += line(128, 222, 390, 410)
    p += line(128, 502, 390, 250)
    p += line(128, 502, 640, 370)
    p += line(128, 502, 640, 490)
    p += line(128, 502, 640, 600)
    p += line(1272, 382, 1225, 360)

    p += include_arrow(650, 250, 640, 210)
    p += include_arrow(650, 370, 640, 210)

    p += note(40, 700, 320, 80, [
        'Non-admin users must verify email,',
        'then upload ID, before events, tasks,',
        'polls, meetings, and clubs.',
    ])
    p += svg_close()
    write('UC2-Authentication.svg', p)


def events():
    w, h = 1500, 900
    p = svg_open(w, h, 'Figure UC-3. Campus events',
                 'Faculty and staff create events. Students propose. Admin approves proposals and may restrict records.')
    p += system_box(300, 90, 900, 740, 'DTAS Events')
    p += actor(110, 180, 'Faculty / Staff')
    p += actor(110, 420, 'Student')
    p += actor(110, 680, 'Event Admin')
    p += actor(1390, 420, 'System Admin')

    p += usecase(520, 170, 140, 28, ['Create event'])
    p += usecase(520, 270, 140, 28, ['Propose event'])
    p += usecase(520, 370, 140, 28, ['Browse campus events'])
    p += usecase(520, 470, 140, 28, ['Request to join event'])
    p += usecase(520, 570, 140, 28, ['Accept invitation code'])
    p += usecase(850, 200, 150, 30, ['Open event workspace'])
    p += usecase(850, 320, 150, 30, ['Accept or decline', 'join requests'])
    p += usecase(850, 450, 150, 30, ['Invite members'])
    p += usecase(850, 570, 150, 30, ['Assign event roles'])
    p += usecase(850, 690, 150, 30, ['Link club to event'])
    p += usecase(1150, 280, 145, 30, ['Approve or reject', 'proposal'])
    p += usecase(1150, 430, 145, 30, ['Restrict or restore', 'event'])
    p += usecase(1150, 580, 145, 30, ['View all events'])

    p += line(128, 202, 380, 170)
    p += line(128, 202, 380, 370)
    p += line(128, 202, 700, 200)
    p += line(128, 202, 700, 320)
    p += line(128, 202, 700, 450)
    p += line(128, 442, 380, 270)
    p += line(128, 442, 380, 370)
    p += line(128, 442, 380, 470)
    p += line(128, 442, 380, 570)
    p += line(128, 442, 700, 200)
    p += line(128, 702, 700, 200)
    p += line(128, 702, 700, 320)
    p += line(128, 702, 700, 450)
    p += line(128, 702, 700, 570)
    p += line(128, 702, 700, 690)
    p += line(1372, 442, 1295, 280)
    p += line(1372, 442, 1295, 430)
    p += line(1372, 442, 1295, 580)

    p += include_arrow(660, 170, 700, 200)
    p += note(40, 800, 360, 70, [
        'Admin cannot create events.',
        'Public join is a request. Event Admin',
        'or Event Manager accepts members.',
    ])
    p += svg_close()
    write('UC3-Events.svg', p)


def meetings():
    w, h = 1400, 820
    p = svg_open(w, h, 'Figure UC-4. Meetings and Zoom rooms',
                 'A meeting is hosted for an event. DTAS creates the Zoom room and closes it when duration ends.')
    p += system_box(300, 90, 800, 660, 'DTAS Meetings')
    p += actor(110, 260, 'Host')
    p += actor(110, 540, 'Event member')
    p += actor(1290, 400, 'Zoom API')

    p += usecase(520, 170, 145, 30, ['Schedule meeting', 'for an event'])
    p += usecase(520, 300, 145, 28, ['Start as host'])
    p += usecase(520, 410, 145, 28, ['Write minutes'])
    p += usecase(520, 530, 145, 28, ['Resend join link'])
    p += usecase(800, 170, 145, 30, ['Create Zoom room'])
    p += usecase(800, 300, 145, 30, ['Send join link to', 'event members'])
    p += usecase(800, 450, 145, 28, ['Join Zoom room'])
    p += usecase(800, 580, 145, 30, ['Close room after', 'scheduled duration'])

    p += line(128, 282, 375, 170)
    p += line(128, 282, 375, 300)
    p += line(128, 282, 375, 410)
    p += line(128, 282, 375, 530)
    p += line(128, 562, 655, 450)
    p += line(1272, 422, 945, 170)
    p += line(1272, 422, 945, 580)

    p += include_arrow(665, 170, 655, 170)
    p += include_arrow(665, 185, 655, 285)
    p += include_arrow(945, 580, 945, 470)

    p += note(40, 700, 340, 80, [
        'Only the host writes minutes.',
        'When start time + duration ends,',
        'Start and Join are removed and the',
        'Zoom room is ended and deleted.',
    ])
    p += svg_close()
    write('UC4-Meetings.svg', p)


def clubs():
    w, h = 1400, 820
    p = svg_open(w, h, 'Figure UC-5. Clubs and Connect',
                 'Campus users create clubs. Admin may restrict a club but cannot create one.')
    p += system_box(300, 90, 800, 660, 'DTAS Clubs and Connect')
    p += actor(110, 220, 'Campus user')
    p += actor(110, 520, 'Club lead')
    p += actor(1290, 360, 'System Admin')

    p += usecase(520, 160, 140, 28, ['Create club'])
    p += usecase(520, 250, 140, 28, ['Request to join', 'public club'])
    p += usecase(520, 360, 140, 28, ['Join with invite code'])
    p += usecase(520, 460, 140, 28, ['Leave club'])
    p += usecase(520, 560, 140, 28, ['Open Connect group'])
    p += usecase(800, 220, 145, 30, ['Approve or decline', 'join requests'])
    p += usecase(800, 360, 145, 28, ['Invite members'])
    p += usecase(800, 480, 145, 28, ['Transfer leadership'])
    p += usecase(800, 600, 145, 28, ['Use club workspace'])
    p += usecase(1100, 360, 145, 30, ['Restrict or restore', 'club'])

    p += line(128, 242, 380, 160)
    p += line(128, 242, 380, 250)
    p += line(128, 242, 380, 360)
    p += line(128, 242, 380, 460)
    p += line(128, 242, 380, 560)
    p += line(128, 242, 655, 600)
    p += line(128, 542, 655, 220)
    p += line(128, 542, 655, 360)
    p += line(128, 542, 655, 480)
    p += line(128, 542, 655, 600)
    p += line(1272, 382, 1245, 360)

    p += note(40, 700, 320, 80, [
        'Restricted clubs cannot message,',
        'invite, join, or link to events.',
        'Members may still view and leave.',
    ])
    p += svg_close()
    write('UC5-ClubsConnect.svg', p)


def work():
    w, h = 1500, 900
    p = svg_open(w, h, 'Figure UC-6. Tasks, assignments, decisions, and polls',
                 'Contribution percentage is computed by the system. Admin does not invent scores.')
    p += system_box(300, 90, 900, 740, 'DTAS Work and governance')
    p += actor(110, 180, 'Event Manager')
    p += actor(110, 400, 'Faculty')
    p += actor(110, 620, 'Student')
    p += actor(1390, 400, 'System Admin')

    p += usecase(530, 170, 145, 28, ['Create event task'])
    p += usecase(530, 260, 145, 28, ['Assign task members'])
    p += usecase(530, 350, 145, 28, ['Work in task workspace'])
    p += usecase(530, 450, 145, 28, ['Create assignment'])
    p += usecase(530, 540, 145, 30, ['Complete assignment', 'group work'])
    p += usecase(530, 650, 145, 28, ['Vote on open poll'])
    p += usecase(850, 260, 150, 30, ['View contribution', 'report'])
    p += usecase(850, 400, 150, 28, ['View published decision'])
    p += usecase(850, 540, 150, 28, ['Extend task deadline'])
    p += usecase(1150, 220, 145, 28, ['Create or edit decision'])
    p += usecase(1150, 340, 145, 28, ['Create and close poll'])
    p += usecase(1150, 460, 145, 30, ['Restrict task or', 'decision'])
    p += usecase(1150, 600, 145, 28, ['Oversee task list'])

    p += line(128, 202, 385, 170)
    p += line(128, 202, 385, 260)
    p += line(128, 202, 385, 350)
    p += line(128, 202, 700, 540)
    p += line(128, 422, 385, 450)
    p += line(128, 422, 700, 260)
    p += line(128, 422, 700, 540)
    p += line(128, 642, 385, 350)
    p += line(128, 642, 385, 540)
    p += line(128, 642, 385, 650)
    p += line(128, 642, 700, 260)
    p += line(128, 642, 700, 400)
    p += line(1372, 422, 1295, 220)
    p += line(1372, 422, 1295, 340)
    p += line(1372, 422, 1295, 460)
    p += line(1372, 422, 1295, 600)

    p += note(40, 800, 360, 70, [
        'Faculty create assignments. Staff do not.',
        'Contribution uses stored formula',
        '(completion, timeliness, activity, recency).',
    ])
    p += svg_close()
    write('UC6-WorkGovernance.svg', p)


def transparency():
    w, h = 1400, 820
    p = svg_open(w, h, 'Figure UC-7. Transparency, feedback, and administration',
                 'The community portal is the public gist. The admin dashboard is the live operations view.')
    p += system_box(300, 90, 800, 660, 'DTAS Transparency and moderation')
    p += actor(110, 240, 'Campus user')
    p += actor(1290, 400, 'System Admin')

    p += usecase(520, 170, 145, 30, ['View community', 'transparency portal'])
    p += usecase(520, 290, 145, 28, ['Submit feedback'])
    p += usecase(520, 400, 145, 28, ['Flag content'])
    p += usecase(520, 510, 145, 28, ['View own reports'])
    p += usecase(520, 620, 145, 28, ['Receive notifications'])
    p += usecase(850, 200, 150, 30, ['View live transparency', 'dashboard'])
    p += usecase(850, 330, 150, 28, ['Respond to feedback'])
    p += usecase(850, 440, 150, 28, ['Review content flags'])
    p += usecase(850, 550, 150, 28, ['Suspend or ban user'])
    p += usecase(850, 660, 150, 28, ['Export reports / audit'])

    p += line(128, 262, 375, 170)
    p += line(128, 262, 375, 290)
    p += line(128, 262, 375, 400)
    p += line(128, 262, 375, 510)
    p += line(128, 262, 375, 620)
    p += line(1272, 422, 1000, 200)
    p += line(1272, 422, 1000, 330)
    p += line(1272, 422, 1000, 440)
    p += line(1272, 422, 1000, 550)
    p += line(1272, 422, 1000, 660)
    p += line(1272, 422, 665, 170)

    p += note(40, 720, 340, 70, [
        'Admin may open the community portal',
        'to see the same gist users see.',
        'Private files stay off Transparency.',
    ])
    p += svg_close()
    write('UC7-TransparencyAdmin.svg', p)


if __name__ == '__main__':
    overview()
    auth()
    events()
    meetings()
    clubs()
    work()
    transparency()
