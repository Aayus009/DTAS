(function () {
    var tried = {};
    var ready = false;
    var noteCount = 0;
    var chatSent = false;
    var proposeSent = false;
    var pipeline = { todo: 'progress', progress: 'review' };
    var advanceLabel = { todo: 'Start', progress: 'Send to review', review: 'Done is locked' };

    var events = {
        forum: {
            title: 'Campus Sustainability Forum',
            badge: 'Public · Planned',
            meta: ['18 Sep 2026', 'Main Auditorium', '24 members (sample)'],
            copy: 'Visitors can read the public summary of campus events. Joining and editing still need a signed-in role.',
            purpose: 'Collect student and faculty input on the 2026 campus waste-reduction plan.',
            lead: 'Faculty sample lead · Environmental Studies'
        },
        council: {
            title: 'Student Council open hour',
            badge: 'Public · In Progress',
            meta: ['22 Sep 2026', 'Student Union Hall', '18 members (sample)'],
            copy: 'A public drop-in for hostel and cafeteria issues. Task work is visible as a gist only in this demo.',
            purpose: 'Log student concerns and show which items moved to the event board.',
            lead: 'Student council chair · sample'
        },
        board: {
            title: 'Facilities board briefing',
            badge: 'Public gist · Private workspace',
            meta: ['25 Sep 2026', 'Admin Conference Room', 'Invite-only team'],
            copy: 'The public sees the purpose and date. Member lists, Connect, and Zoom stay behind sign-in.',
            purpose: 'Brief faculty on auditorium booking and budget notes for the forum.',
            lead: 'Event admin · Facilities'
        }
    };

    var records = [
        { id: 'r1', type: 'decision', title: 'Waste-reduction plan adopted', date: '12 Aug 2026', detail: 'Board accepted the three-bin campus plan. Implementation tasks sit on the sample event board.' },
        { id: 'r2', type: 'minutes', title: 'Board briefing minutes (gist)', date: '12 Aug 2026', detail: 'Attendance, agenda, and two action items. Full minutes PDF is locked in guest demo.' },
        { id: 'r3', type: 'budget', title: 'Q1 facilities allocation gist', date: '04 Jul 2026', detail: 'Public summary only: auditorium hire and recycling bins. Line-item downloads need an account.' },
        { id: 'r4', type: 'decision', title: 'Forum date confirmed', date: '01 Sep 2026', detail: 'Public event date set for 18 Sep 2026. Private workspace remains invite-only.' }
    ];

    function $(id) {
        return document.getElementById(id);
    }

    function markTried(key) {
        if (!ready || tried[key]) return;
        tried[key] = true;
        var item = document.querySelector('#demoTryList [data-try="' + key + '"]');
        if (item) item.classList.add('is-tried');
        var n = Object.keys(tried).length;
        var el = $('demoProgress');
        if (el) el.textContent = 'Tried ' + n + ' of 6 guest actions';
    }

    function setText(id, text) {
        var el = $(id);
        if (el) el.textContent = text || '';
    }

    function renderEvent(key) {
        var data = events[key] || events.forum;
        var card = $('demo-event');
        var heading = card ? card.querySelector('h2') : null;
        if (heading) heading.textContent = data.title;
        setText('demoEventBadge', data.badge);
        setText('demoEventCopy', data.copy);
        setText('demoEventPurpose', data.purpose);
        setText('demoEventLead', data.lead);
        var meta = $('demoEventMeta');
        if (meta) {
            var icons = ['calendar_today', 'location_on', 'group'];
            meta.innerHTML = data.meta.map(function (item, i) {
                return '<span><span class="material-symbols-outlined">' + icons[i] + '</span> ' + item + '</span>';
            }).join('');
        }
        document.querySelectorAll('[data-event]').forEach(function (btn) {
            var on = btn.getAttribute('data-event') === key;
            btn.classList.toggle('is-on', on);
            btn.setAttribute('aria-selected', on ? 'true' : 'false');
        });
        markTried('browse');
    }

    function addNote() {
        var input = $('demoNoteInput');
        var list = $('demoNoteList');
        if (!input || !list) return;
        var text = (input.value || '').replace(/^\s+|\s+$/g, '');
        if (!text) {
            setText('demoNoteHint', 'Write a short note first.');
            return;
        }
        if (noteCount >= 2) {
            setText('demoNoteHint', 'Guest demo allows 2 notes. Sign in to use the live workspace.');
            return;
        }
        noteCount += 1;
        var item = document.createElement('div');
        item.className = 'demo-note';
        item.innerHTML = '<strong>Guest visitor</strong><p></p>';
        item.querySelector('p').textContent = text;
        list.appendChild(item);
        input.value = '';
        setText('demoNoteHint', noteCount === 1 ? 'One more sample note allowed.' : 'Note limit reached. Nothing was saved.');
        markTried('note');
    }

    function colBody(name) {
        return $('col-' + name);
    }

    function refreshCounts() {
        ['todo', 'progress', 'review', 'done'].forEach(function (name) {
            var body = colBody(name);
            var count = body ? body.querySelectorAll('.demo-task').length : 0;
            var badge = document.querySelector('.demo-count[data-count="' + name + '"]');
            if (badge) badge.textContent = String(count);
        });
    }

    function paintTaskButton(task) {
        var status = task.getAttribute('data-status');
        var btn = task.querySelector('[data-advance]');
        if (!btn) return;
        if (status === 'review') {
            btn.textContent = 'Done is locked';
            btn.classList.add('demo-task-btn-locked');
            btn.removeAttribute('data-advance');
            btn.setAttribute('type', 'button');
            btn.onclick = function () { if (typeof openLogin === 'function') openLogin(); };
            return;
        }
        btn.textContent = advanceLabel[status] || 'Move';
        btn.classList.remove('demo-task-btn-locked');
    }

    function advanceTask(task) {
        var from = task.getAttribute('data-status');
        var to = pipeline[from];
        if (!to) {
            setText('demoBoardHint', 'Guest demo stops at In Review. Sign in to mark work Done.');
            return;
        }
        var dest = colBody(to);
        if (!dest) return;
        task.setAttribute('data-status', to);
        task.classList.add('is-moved');
        paintTaskButton(task);
        dest.appendChild(task);
        refreshCounts();
        setText('demoBoardHint', task.querySelector('.demo-key').textContent + ' moved to ' + (to === 'progress' ? 'In Progress' : 'In Review') + '. Not saved.');
        markTried('board');
    }

    function resetBoard() {
        var home = {
            'demo-1': 'todo',
            'demo-2': 'progress',
            'demo-4': 'todo',
            'demo-3': 'done'
        };
        Object.keys(home).forEach(function (id) {
            var task = document.querySelector('[data-task="' + id + '"]');
            var dest = colBody(home[id]);
            if (!task || !dest) return;
            task.setAttribute('data-status', home[id]);
            task.classList.remove('is-moved');
            dest.appendChild(task);
            var oldBtn = task.querySelector('button');
            if (task.getAttribute('data-task') === 'demo-3' || task.getAttribute('data-task') === 'demo-4') return;
            if (oldBtn && !oldBtn.getAttribute('data-advance')) {
                oldBtn.onclick = null;
                oldBtn.setAttribute('data-advance', '1');
                oldBtn.classList.remove('demo-task-btn-locked');
            }
            paintTaskButton(task);
        });
        refreshCounts();
        setText('demoBoardHint', 'Board reset to the sample starting state.');
    }

    function renderRecords(filter) {
        var list = $('demoRecordList');
        if (!list) return;
        var rows = records.filter(function (r) { return filter === 'all' || r.type === filter; });
        list.innerHTML = rows.map(function (r) {
            return '<li><button type="button" class="demo-record" data-record="' + r.id + '">' +
                '<span class="demo-key">' + r.type + '</span>' +
                '<strong>' + r.title + '</strong>' +
                '<em>' + r.date + '</em></button></li>';
        }).join('');
        document.querySelectorAll('[data-record-filter]').forEach(function (btn) {
            btn.classList.toggle('is-on', btn.getAttribute('data-record-filter') === filter);
        });
        markTried('records');
    }

    function showRecord(id) {
        var rec = records.filter(function (r) { return r.id === id; })[0];
        var box = $('demoRecordDetail');
        if (!rec || !box) return;
        box.hidden = false;
        box.innerHTML = '<h3>' + rec.title + '</h3><p>' + rec.detail + '</p>';
        markTried('records');
    }

    function sendChat() {
        if (chatSent) {
            setText('demoChatHint', 'One guest message already sent. Creating rooms is locked.');
            return;
        }
        var input = $('demoChatInput');
        var chat = $('demoChat');
        if (!input || !chat) return;
        var text = (input.value || '').replace(/^\s+|\s+$/g, '');
        if (!text) {
            setText('demoChatHint', 'Write a short reply first.');
            return;
        }
        var msg = document.createElement('div');
        msg.className = 'demo-chat-msg is-guest';
        msg.innerHTML = '<strong>Guest visitor</strong><p></p>';
        msg.querySelector('p').textContent = text;
        chat.appendChild(msg);
        input.value = '';
        input.disabled = true;
        chatSent = true;
        var send = $('demoChatSend');
        if (send) send.disabled = true;
        setText('demoChatHint', 'Sample reply posted. It will vanish on refresh.');
        markTried('connect');
    }

    function previewProposal() {
        if (proposeSent) return;
        var title = (($('demoProposeTitle') || {}).value || '').replace(/^\s+|\s+$/g, '');
        var gist = (($('demoProposeGist') || {}).value || '').replace(/^\s+|\s+$/g, '');
        var typeEl = $('demoProposeType');
        var type = typeEl ? typeEl.value : 'Student activity';
        var box = $('demoProposeResult');
        if (!box) return;
        if (!title || !gist) {
            box.hidden = false;
            box.innerHTML = '<p>Add a title and gist to preview the sample proposal.</p>';
            return;
        }
        proposeSent = true;
        box.hidden = false;
        box.innerHTML = '<span class="demo-badge demo-badge-public">Queued · sample only</span>' +
            '<h3></h3><p class="demo-copy"></p>' +
            '<p class="demo-hint">Faculty approval and event creation are locked in guest demo.</p>';
        box.querySelector('h3').textContent = title + ' · ' + type;
        box.querySelector('.demo-copy').textContent = gist;
        var btn = $('demoProposeSend');
        if (btn) btn.disabled = true;
    }

    document.addEventListener('click', function (e) {
        var eventBtn = e.target.closest('[data-event]');
        if (eventBtn) {
            e.preventDefault();
            renderEvent(eventBtn.getAttribute('data-event'));
            return;
        }
        var advance = e.target.closest('[data-advance]');
        if (advance) {
            e.preventDefault();
            var task = advance.closest('.demo-task');
            if (task) advanceTask(task);
            return;
        }
        var rsvp = e.target.closest('[data-rsvp]');
        if (rsvp) {
            e.preventDefault();
            setText('demoRsvpBadge', 'RSVP · ' + rsvp.getAttribute('data-rsvp'));
            setText('demoMeetingHint', 'Sample RSVP saved in this page only. Zoom stays locked.');
            markTried('meeting');
            return;
        }
        var recFilter = e.target.closest('[data-record-filter]');
        if (recFilter) {
            e.preventDefault();
            renderRecords(recFilter.getAttribute('data-record-filter'));
            return;
        }
        var rec = e.target.closest('[data-record]');
        if (rec) {
            e.preventDefault();
            showRecord(rec.getAttribute('data-record'));
        }
    });

    document.addEventListener('change', function (e) {
        if (e.target && e.target.getAttribute('data-agenda')) markTried('meeting');
    });

    var noteSend = $('demoNoteSend');
    if (noteSend) noteSend.addEventListener('click', addNote);
    var noteInput = $('demoNoteInput');
    if (noteInput) noteInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); addNote(); }
    });

    var reset = $('demoResetBoard');
    if (reset) reset.addEventListener('click', resetBoard);

    var chatSend = $('demoChatSend');
    if (chatSend) chatSend.addEventListener('click', sendChat);
    var chatInput = $('demoChatInput');
    if (chatInput) chatInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); sendChat(); }
    });

    var propose = $('demoProposeSend');
    if (propose) propose.addEventListener('click', previewProposal);

    document.querySelectorAll('.demo-task').forEach(function (task) {
        paintTaskButton(task);
    });
    refreshCounts();
    renderEvent('forum');
    renderRecords('all');
    ready = true;
})();
