# Digital Transparency and Accountability Status System for Educational Community Decision-Making

**Final Year Project — Full System Specification (v2)**
**Platform:** ASP.NET Web Forms (.NET Framework), C#, SQL Server

---

## 1. Project Overview

This system digitizes how a college manages **events/projects, meetings, decisions, and task accountability**, while giving the wider student community a **transparency view** into what has been decided and how work is progressing — without exposing internal, confidential details.

There is no external "university approval" layer — **Admin is the top authority** in the system. Everything originates from Admin (or Faculty, where permitted) and flows down to task-level execution, with visibility carefully split between:

- **Internal working view** — Admin + Faculty/Staff + assigned Student Representatives see full task detail, meeting minutes, and decision history.
- **Public transparency view** — General Students see only approved decisions, event progress, and completed activities.

A **Decision** is the core unit of accountability in this system (not a "proposal"): decisions come out of meetings, get assigned as tasks, get tracked to completion, and their status is what drives the transparency dashboard.

---

## 2. User Roles & Permissions

| Role | Access Level | Key Permissions |
|---|---|---|
| **Admin** | Full system control | Create events/projects, create meetings, create/manage decisions, assign responsibilities/tasks, manage users & roles, view all reports, moderate feedback, manage polls, view full accountability data |
| **Faculty/Staff** | Operational / execution level | View assigned tasks, update task progress, submit reports, participate in meeting discussions, view decisions relevant to them, upload evidence for completed work |
| **Student Representative** | Semi-internal / representative level | View decisions (approved + relevant in-progress), participate in polls, submit feedback on behalf of students, track transparency reports in more detail than general students |
| **General Student** | Public / read-only + feedback | View public decisions, view event progress status, submit suggestions/complaints, vote in open polls (if eligible) |

**Visibility rule (accountability vs. transparency):**

| Data | Admin | Faculty/Staff | Student Rep | General Student |
|---|---|---|---|---|
| Task list, deadlines, assignees | Yes | Yes (own + team) | No | No |
| Task update history / evidence files | Yes | Yes (own + team) | No | No |
| Full meeting minutes | Yes | Yes (attended meetings) | Summary only | No |
| Decision history (Created→Reviewed→Approved→Implemented→Completed) | Yes | Yes | Approved decisions only | Final status only |
| Accountability score / department performance | Yes | Yes (own department) | No | No |
| Approved decisions & event progress | Yes | Yes | Yes | Yes (Public Transparency Page) |
| Polls | Create/manage | View | Vote | Vote |
| Feedback/suggestions submitted | Yes (all) | No | Yes (own) | Yes (own) |

---

## 3. System Flow

```
Login
  |
Dashboard (role-specific)
  |
Events/Projects  --------------+
  |                             |
Meetings (linked to Event)      |
  |                             |
Decisions (created in/after     |
a meeting, linked to Event)     |
  |                             |
Task Assignment (Decision       |
broken down into tasks)         |
  |                             |
Progress Updates (Faculty/Staff |
update status + upload proof)   |
  |                             |
Reports (generated from         |
task + decision data)           |
  |                             |
Transparency Dashboard ---------+
  (aggregated, filtered view for
   Student Reps + Public)
```

**Connectivity logic (how modules link together):**
- An **Event** is the container (e.g., "Convocation 2026").
- A **Meeting** is scheduled under an Event, has an Agenda and Participants.
- A **Decision** is created from a Meeting (`MeetingReference` field), inheriting context (e.g., "Venue Selected" decided in "Venue Selection" meeting).
- A **Decision** can trigger a **Poll** when multiple options exist (e.g., "Convocation Venue Selection" poll feeds into the final Decision).
- Once a Decision is approved, it is broken into one or more **Tasks**, each assigned to Faculty/Staff/Student Rep via **TaskAssignments**.
- Assignees post **TaskUpdates** (status + evidence), which roll up into the Decision's progress and the Event's overall completion %.
- All of this aggregates into the **Transparency Dashboard** (internal, full charts) and the **Public Transparency Page** (filtered, approved-only).
- **Feedback** runs in parallel — students submit suggestions/complaints tied optionally to an Event or Decision; Admin reviews and resolves them independent of the task pipeline.
- **Notifications** fire on every state change across all modules (task assigned, decision approved, meeting scheduled, poll open, feedback resolved).
- **Reports** pull from Events, Decisions, Tasks, and Accountability data to generate exportable summaries.

---

## 4. Site Map / Folder Structure

```
/ (Public site - no login required)
 |-- Default.aspx            -> Landing page
 |-- About.aspx               -> About the system / institution
 |-- Features.aspx            -> What the platform offers
 |-- Events.aspx               -> Public list of events (approved/ongoing/completed)
 |-- EventDetails.aspx         -> Public event detail (objectives, timeline, progress)
 |-- Transparency.aspx         -> Public Transparency Page (approved decisions, progress)
 |-- Contact.aspx
 |-- FAQ.aspx
 |-- Login.aspx
 `-- Register.aspx             -> Admin-only creation flow (see note in 5.A.2)

/Student/                      (Student Representative + General Student, authenticated)
 |-- Dashboard.aspx
 |-- Decisions.aspx            -> Decisions visible to students
 |-- Polls.aspx                -> Vote on open polls
 `-- Feedback.aspx             -> Submit suggestions/complaints

/Faculty/                      (Faculty/Staff)
 |-- Dashboard.aspx
 |-- Meetings.aspx             -> Meetings they're part of
 |-- Tasks.aspx                -> Assigned tasks + updates
 `-- Reports.aspx              -> Submit/view their reports

/Admin/                        (Admin only)
 |-- Dashboard.aspx
 |-- Users.aspx                -> User & role management
 |-- Events.aspx                -> Full event CRUD
 |-- Meetings.aspx              -> Full meeting CRUD
 |-- Decisions.aspx             -> Full decision CRUD + history
 |-- Tasks.aspx                 -> Assign & monitor all tasks
 `-- Reports.aspx               -> Generate/export all reports
```

> Note: `EventDetails.aspx`, `Decisions.aspx`, and `Tasks.aspx` exist in more than one area — the **page is shared/templated**, but the **data returned and actions available** are filtered server-side by role (e.g., Admin's Tasks.aspx shows *all* tasks with edit rights; Faculty's Tasks.aspx shows only *their* tasks with update-only rights).

---

## 5. Page-by-Page Functional Detail

### A. Authentication Module

**1. Login.aspx**
- Fields: Email, Password
- Functions: Authenticate then redirect to role-specific Dashboard; "Forgot Password" (email reset link with expiring token)
- Failed-attempt lockout after N tries

**2. Register.aspx**
- **Admin-only** (no public self-registration for Faculty/Student Rep roles — keeps role assignment controlled)
- Fields: Name, Email, Role (dropdown: Faculty, Student Representative, Student), Department
- General Students *may* optionally self-register with a restricted default role, pending Admin activation — configurable

### B. Dashboard Module

**3. Dashboard.aspx** (role-specific rendering)
- Admin sees: Total Events, Active Tasks, Pending Decisions, Completed Tasks, Upcoming Meetings, system-wide charts
- Faculty sees: Their active tasks, their upcoming meetings, their submitted reports status
- Student Rep/Student sees: Public decision counts, poll participation, feedback status
- Charts (Admin/Faculty): Completion Percentage, Department Performance, Decision Status (via a chart control, e.g. Chart.js or the ASP.NET Chart control)

### C. Event / Project Management

**4. Events.aspx (list)**
- Columns: Event Name, Status (Planned/Ongoing/Completed/Archived), Timeline
- Actions (Admin): View, Edit, Archive
- Public version (root `/Events.aspx`) shows only Ongoing/Completed events with progress %, no Edit/Archive controls

**5. Admin/Events.aspx -> Create Event (modal or sub-page)**
- Fields: Event Name, Description, Start Date, End Date, Budget, Organizer
- On save: creates Event record, status = "Planned"

**6. EventDetails.aspx**
- Internal view (Admin/Faculty): Objectives, Budget, Timeline, Team Members, Assigned Tasks, Decisions, full progress breakdown
- Public view: Objectives, Timeline, overall Progress %, list of completed milestones only — **no budget, no team/assignee names**

### D. Meeting Management

**7. Meetings.aspx (list)**
- Columns: Meeting Title, Date, Status (Scheduled/Completed/Cancelled)
- Faculty view: only meetings they're invited to
- Admin view: all meetings, with Create/Edit/Cancel

**8. Admin/Meetings.aspx -> Schedule Meeting**
- Fields: Meeting Title, Date, Time, Venue (or online link), Agenda, Event reference, Participants (multi-select from Users)

**9. Meeting Details page**
- Shows: Agenda, Participants list + attendance marking, Minutes of Meeting (free text + structured decision/action log)
- Buttons: **Record Decision** (opens Create Decision pre-filled with `MeetingReference`), **Assign Task** (opens Task Assignment pre-filled with context)

### E. Decision Management (Core Module)

**10. Decisions.aspx (list)**
- Columns: Decision Title, Status
- Example rows: "Venue Selected — Completed", "Catering Approved — Pending"
- Admin/Faculty view: all decisions with full history access
- Student view: only decisions marked **Approved/Implemented/Completed** (i.e., public-safe statuses)

**11. Admin/Decisions.aspx -> Create Decision**
- Fields: Decision Title, Description, Meeting Reference (dropdown of meetings), Priority (Low/Medium/High)
- If multiple options need community input, Admin can attach a **Poll** to this decision before finalizing it

**12. Decision Details page**
- Shows: Decision History timeline, Responsible Person, Approval Status, Progress %
- Status pipeline (visualized like a Jira-style timeline):
```
Created -> Reviewed -> Approved -> Implemented -> Completed
```
- Each transition is written to **DecisionHistory** (who changed it, when, from-status to to-status) — this is the accountability trail for decisions specifically.

### F. Accountability Module (Task Assignment & Tracking)

**13. Tasks.aspx (Admin) / Responsibility Assignment**
- Admin assigns: Faculty, Staff, or Student Representative
- Fields: Task (linked to a Decision or Event), Assignee, Deadline, Priority
- Writes to **TaskAssignments** (who assigned, who to, when)

**14. Task Details page**
- Shows: Description, Deadline, Status, Attachments, full update history (every status change + comment, timestamped)

**15. Faculty/Tasks.aspx -> Task Update**
- Status options: Not Started, In Progress, Completed, Delayed
- Evidence upload: PDF, Images, Documents (stored via **Attachments** table, linked to TaskUpdate)
- Every update writes a new row to **TaskUpdates** (append-only — never overwritten) — this is the core accountability log: *who did what, when, with what proof.*

### G. Transparency Module

**16. Transparency Dashboard** (internal — Admin/Faculty, more detail than public)
- Shows: Total Decisions, Completed Decisions, Pending Decisions, Delayed Tasks
- Charts: Accountability Score (per department/person), Completion Rate, Department-wise Progress

**17. Transparency.aspx** (Public Transparency Page)
- Students can view: Approved Decisions, Event Progress, Completed Activities
- Explicitly **excludes**: assignee names tied to delays, internal meeting minutes, budget figures, feedback contents, decisions still in "Created/Reviewed" (unapproved) status
- This page is the system's core public-trust deliverable — it should load without authentication.

### H. Feedback Module

**18. Student/Feedback.aspx -> Suggestion Page**
- Fields: Type (Suggestion/Complaint/Recommendation), Related Event (optional), Description, Attachment (optional)

**19. Feedback Management page** (Admin)
- Status pipeline: New -> Under Review -> Resolved
- Admin can respond; response optionally visible to the submitter only (not public)

### I. Voting / Polling Module

**20. Student/Polls.aspx -> Poll List**
- Shows open + recently closed polls (e.g., "Convocation Venue Selection", "Theme Selection")
- Each poll optionally linked to a pending Decision

**21. Vote page**
- One vote per user per poll (enforced via unique constraint on `PollId + UserId` in **Votes** table)
- Results hidden until poll closes (configurable), then shown as aggregated counts — never shows *who* voted for what publicly (though stored internally for integrity/audit)

### J. Reporting Module

**22. Reports.aspx**
- Report types: Event Report, Decision Report, Accountability Report
- Faculty can generate reports scoped to their own tasks/department; Admin can generate system-wide

**23. Report Details / Export**
- Export formats: PDF, Excel
- Accountability Report specifically aggregates **TaskUpdates + TaskAssignments** into "on-time %", "completed vs. assigned", per person/department — internal use only, never public

### K. User Management

**24. Admin/Users.aspx**
- Manage Faculty, Students, Staff — create, deactivate, edit department/role

**25. Role Management**
- Roles: Admin, Faculty, Staff, Student Representative, Student
- Controls which nav items / pages a logged-in user can reach (server-side check on every page load, not just hidden menu items)

---

## 6. Navigation Structure

```
Dashboard
|
|-- Events
|    |-- Event List
|    |-- Create Event        (Admin only)
|    `-- Event Details
|
|-- Meetings
|    |-- Meeting List
|    |-- Schedule Meeting     (Admin only)
|    `-- Meeting Details
|
|-- Decisions
|    |-- Decision List
|    |-- Create Decision      (Admin only)
|    `-- Decision Details
|
|-- Tasks
|    |-- Assignment            (Admin only)
|    |-- Task List
|    `-- Task Updates          (Faculty/Staff)
|
|-- Transparency
|    |-- Dashboard             (internal)
|    `-- Public View
|
|-- Feedback
|    |-- Suggestions           (Student)
|    `-- Complaints Management (Admin)
|
|-- Polls
|    |-- Create Poll           (Admin only)
|    `-- Vote                  (Student)
|
|-- Reports
|
`-- User Management            (Admin only)
```

---

## 7. Database Design (Core Tables)

| Table | Key Columns |
|---|---|
| **Users** | UserId (PK), FullName, Email, PasswordHash, RoleId (FK), DepartmentId (FK), IsActive, CreatedAt |
| **Roles** | RoleId (PK), RoleName (Admin, Faculty, Staff, StudentRep, Student) |
| **Departments** | DepartmentId (PK), DepartmentName |
| **Events** | EventId (PK), Name, Description, StartDate, EndDate, Budget, Organizer, Status, CreatedBy |
| **Meetings** | MeetingId (PK), EventId (FK), Title, Date, Time, Venue, Agenda, Status, CreatedBy |
| **MeetingParticipants** | Id (PK), MeetingId (FK), UserId (FK), AttendanceStatus |
| **Decisions** | DecisionId (PK), EventId (FK), MeetingId (FK, nullable), Title, Description, Priority, Status, ResponsiblePersonId (FK), CreatedAt |
| **DecisionHistory** | HistoryId (PK), DecisionId (FK), OldStatus, NewStatus, ChangedBy (FK Users), ChangedAt |
| **Tasks** | TaskId (PK), DecisionId (FK, nullable), EventId (FK), Title, Description, Deadline, Priority, Status |
| **TaskAssignments** | AssignmentId (PK), TaskId (FK), AssignedTo (FK Users), AssignedBy (FK Users), AssignedAt |
| **TaskUpdates** | UpdateId (PK), TaskId (FK), UpdatedBy (FK Users), Status, Comment, UpdatedAt *(append-only)* |
| **Attachments** | AttachmentId (PK), TaskUpdateId (FK, nullable), FeedbackId (FK, nullable), FilePath, FileType, UploadedBy, UploadedAt |
| **Feedback** | FeedbackId (PK), UserId (FK), EventId (FK, nullable), Type, Description, Status, SubmittedAt |
| **Polls** | PollId (PK), DecisionId (FK, nullable), Title, StartDate, EndDate, IsResultVisibleLive |
| **PollOptions** | PollOptionId (PK), PollId (FK), OptionLabel |
| **Votes** | VoteId (PK), PollId (FK), PollOptionId (FK), UserId (FK), VotedAt *(unique: PollId+UserId)* |
| **Notifications** | NotificationId (PK), UserId (FK), Message, Type, IsRead, CreatedAt |
| **Reports** | ReportId (PK), EventId (FK, nullable), Type, GeneratedBy, GeneratedAt, FilePath |

**Relationships:**
- `Events (1) -> Meetings (N) -> Decisions (N) -> Tasks (N) -> TaskUpdates (N)` — the full accountability chain.
- `Decisions (1) -> DecisionHistory (N)` — status-change audit trail specific to decisions.
- `Decisions (0..1) -> Polls (1) -> PollOptions (N) -> Votes (N)`.
- `Tasks (1) -> TaskAssignments (N)` (supports reassignment history — old assignments kept, not deleted).
- `TaskUpdates (1) -> Attachments (N)` and `Feedback (1) -> Attachments (N)` (shared attachment table via nullable FKs).
- `Users (N) <-> MeetingParticipants <-> Meetings (N)` (many-to-many with attendance status).

---

## 8. System Architecture

```
+------------------------------------------------+
| Presentation Layer (.aspx + .aspx.cs)           |
|  - Public site (no auth): Default, Events,      |
|    EventDetails, Transparency, FAQ, Contact     |
|  - /Student, /Faculty, /Admin (auth + role      |
|    checked in Page_Load of each page)           |
+---------------------+----------------------------+
                      |
+---------------------v----------------------------+
| Business Logic Layer (BLL)                       |
|  EventManager, MeetingManager, DecisionManager,  |
|  TaskManager, PollManager, FeedbackManager,       |
|  ReportManager, NotificationManager,              |
|  AccountabilityManager (aggregates TaskUpdates    |
|  + DecisionHistory into scores/charts)            |
+---------------------+----------------------------+
                      |
+---------------------v----------------------------+
| Data Access Layer (DAL)                          |
|  ADO.NET / Entity Framework repositories;         |
|  stored procedures for vote casting and           |
|  status-history inserts (integrity-critical)      |
+---------------------+----------------------------+
                      |
+---------------------v----------------------------+
| SQL Server Database                              |
+---------------------------------------------------+
```

**Solution structure:**
```
TransparencySystem.sln
 |-- TransparencySystem.Web
 |    |-- Root (public pages)
 |    |-- Student/
 |    |-- Faculty/
 |    |-- Admin/
 |    `-- Shared/ (Login, Register, MasterPages, Notifications control)
 |-- TransparencySystem.BLL
 |-- TransparencySystem.DAL
 |-- TransparencySystem.Models
 `-- TransparencySystem.Common (Enums, RoleAuthHelper, AuditHelper)
```

**Role enforcement pattern:** every `.aspx.cs` in `/Admin`, `/Faculty`, `/Student` calls a shared `AuthHelper.EnsureRole(Roles.X)` in `Page_Load` before rendering any data — this is what prevents a Student from hitting `Faculty/Tasks.aspx` directly by URL.

---

## 9. Non-Functional Requirements

- **Security:** Hashed passwords (BCrypt/PBKDF2), parameterized SQL, anti-forgery tokens, server-side role checks on every protected page (not just hidden nav links).
- **Auditability:** `DecisionHistory` and `TaskUpdates` are append-only — no UPDATE/DELETE permission on these tables even for Admin, only INSERT.
- **Public performance:** `Default.aspx`, `Events.aspx`, `Transparency.aspx` are unauthenticated and should be lightweight/cacheable since they're the most publicly hit pages.
- **Integrity:** One-vote-per-user enforced at DB level via a unique constraint, not just UI validation.
- **Usability:** Shared Master Pages per role folder (`/Student`, `/Faculty`, `/Admin`) so navigation only shows what that role can access.

---

## 10. Recommended Scope for FYP (10 Core Modules)

To keep the project realistic and demonstrable within a final-year timeline:

1. Login & Role Management
2. Dashboard (role-specific)
3. Event Management
4. Meeting Management
5. Decision Management
6. Task Assignment & Accountability
7. Transparency Dashboard (internal + public)
8. Feedback System
9. Notifications
10. Reporting & Analytics

Polls/Voting can be built as an **11th module or a sub-feature of Decision Management** if time is limited — it plugs cleanly into the Decision pipeline without needing its own full CRUD suite.

---

## 11. Suggested Development Order

1. DB schema + ER diagram finalization.
2. Auth + Role Management + Master Pages per role.
3. Event Management (CRUD).
4. Meeting Management (linked to Events).
5. Decision Management + DecisionHistory (the core accountability trail).
6. Task Assignment + TaskUpdates + Attachments.
7. Transparency Dashboard (internal) then the Public Transparency Page (filtered view of the same data).
8. Polls (linked to Decisions).
9. Feedback module.
10. Notifications + Reports (built last since they consume data from everything above).
11. Role-based access testing — specifically verify a Student account can never reach `/Faculty/*` or `/Admin/*` data even via direct URL, and that the Public Transparency Page never leaks budget/assignee/internal-status data.

---

## 12. Key Point to Emphasize in Your Report

> The system's core contribution is the **Decision -> Task -> Update chain feeding two different views of the same data**: an internal accountability view (full detail, restricted to Admin/Faculty/assigned Student Rep) and a public transparency view (approved/completed-only, no names or budgets). Both views read from the same `DecisionHistory` and `TaskUpdates` tables — transparency isn't a separate manually-maintained page, it's a filtered projection of the accountability data itself. That's the technical and conceptual core worth defending in your viva.
