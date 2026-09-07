(function () {
    var feed = document.getElementById('connectFeed');
    var input = document.getElementById('connectInput');
    var status = document.getElementById('connectStatus');
    var imageInput = document.getElementById('connectImage');
    var imageName = document.getElementById('connectImageName');
    var emojiBar = document.getElementById('connectEmojiBar');
    var typeMenu = document.getElementById('connectTypeMenu');
    var drawer = document.getElementById('connectDrawer');
    var backdrop = document.getElementById('connectMembersBackdrop');
    var menu = document.getElementById('connectMenu');
    var editOverlay = document.getElementById('pnlEditGroup');
    var emojis = ['😀', '😂', '😊', '😍', '😎', '🤔', '😢', '😡', '👍', '👎', '🎉', '🔥', '✅', '⚠️', '📢', '❤️'];
    var type = 'Message';
    var lastId = 0;
    var firstId = 0;
    var firstLoad = true;
    var loadingOlder = false;
    var hasOlder = true;

    function hide(el) { if (el) el.classList.add('hidden'); }
    function show(el) { if (el) el.classList.remove('hidden'); }
    function toggle(el) { if (el) el.classList.toggle('hidden'); }

    function inviteCode() {
        var node = document.getElementById('litCodeText');
        return node ? (node.textContent || '').trim() : '';
    }

    function copyCode() {
        var code = inviteCode();
        if (!code || !navigator.clipboard) return;
        navigator.clipboard.writeText(code);
    }

    function openMembers() {
        hide(menu);
        show(drawer);
        show(backdrop);
    }

    function closeMembers() {
        hide(drawer);
        hide(backdrop);
    }

    function closeEdit() {
        if (editOverlay) editOverlay.classList.add('hidden');
    }

    document.getElementById('btnToggleMembers') && document.getElementById('btnToggleMembers').addEventListener('click', function (e) {
        e.stopPropagation();
        if (drawer && drawer.classList.contains('hidden')) openMembers();
        else closeMembers();
    });
    document.getElementById('btnCloseMembers') && document.getElementById('btnCloseMembers').addEventListener('click', closeMembers);
    backdrop && backdrop.addEventListener('click', closeMembers);

    document.getElementById('btnToggleMenu') && document.getElementById('btnToggleMenu').addEventListener('click', function (e) {
        e.stopPropagation();
        hide(typeMenu);
        hide(emojiBar);
        closeMembers();
        toggle(menu);
    });
    document.getElementById('btnCopyCode') && document.getElementById('btnCopyCode').addEventListener('click', function () {
        copyCode();
        hide(menu);
    });
    document.getElementById('btnCopyCode2') && document.getElementById('btnCopyCode2').addEventListener('click', copyCode);

    document.getElementById('btnShowEdit') && document.getElementById('btnShowEdit').addEventListener('click', function () {
        hide(menu);
        if (editOverlay) editOverlay.classList.remove('hidden');
    });
    document.getElementById('btnCloseEdit') && document.getElementById('btnCloseEdit').addEventListener('click', closeEdit);
    editOverlay && editOverlay.addEventListener('click', function (e) {
        if (e.target === editOverlay) closeEdit();
    });

    document.getElementById('btnTypeToggle') && document.getElementById('btnTypeToggle').addEventListener('click', function (e) {
        e.stopPropagation();
        hide(menu);
        hide(emojiBar);
        toggle(typeMenu);
    });
    document.querySelectorAll('.connect-type-opt').forEach(function (btn) {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.connect-type-opt').forEach(function (b) { b.classList.remove('is-active'); });
            btn.classList.add('is-active');
            type = btn.getAttribute('data-type') || 'Message';
            hide(typeMenu);
        });
    });

    if (emojiBar) {
        emojis.forEach(function (emo) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'connect-emoji';
            b.textContent = emo;
            b.addEventListener('click', function () {
                if (!input) return;
                input.value += emo;
                input.focus();
            });
            emojiBar.appendChild(b);
        });
    }
    document.getElementById('btnEmojiToggle') && document.getElementById('btnEmojiToggle').addEventListener('click', function (e) {
        e.stopPropagation();
        hide(menu);
        hide(typeMenu);
        toggle(emojiBar);
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('#connectMenu') && !e.target.closest('#btnToggleMenu'))
            hide(menu);
        if (!e.target.closest('.connect-composer')) {
            hide(typeMenu);
            hide(emojiBar);
        }
        if (!e.target.closest('#connectDrawer') && !e.target.closest('#btnToggleMembers'))
            closeMembers();
    });

    if (imageInput && imageName) {
        imageInput.addEventListener('change', function () {
            imageName.textContent = imageInput.files && imageInput.files[0] ? imageInput.files[0].name : '';
        });
    }

    if (input) {
        input.addEventListener('input', function () {
            input.style.height = 'auto';
            input.style.height = Math.min(input.scrollHeight, 96) + 'px';
        });
    }

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function initial(name) {
        var value = String(name || '?').trim();
        return value ? value.charAt(0).toUpperCase() : '?';
    }

    function buildRow(msg) {
        var mine = parseInt(msg.senderId, 10) === parseInt(feed.getAttribute('data-user-id') || '0', 10);
        var row = document.createElement('div');
        row.className = 'connect-row' + (mine ? ' is-mine' : '');
        var kind = String(msg.type || 'Message').toLowerCase();
        var badge = msg.type && msg.type !== 'Message' ? '<span class="connect-badge">' + escapeHtml(msg.type) + '</span>' : '';
        var img = msg.image ? '<img class="connect-img" src="' + escapeHtml(msg.image) + '" alt="" />' : '';
        var avatar = mine ? '' : '<span class="connect-avatar sm">' + escapeHtml(initial(msg.name)) + '</span>';
        row.innerHTML =
            avatar +
            '<div class="connect-bubble connect-msg-' + kind + '">' +
            (mine ? '' : '<div class="connect-msg-name">' + escapeHtml(msg.name) + ' ' + badge + '</div>') +
            (mine && badge ? badge : '') +
            (msg.content ? '<p>' + escapeHtml(msg.content) + '</p>' : '') +
            img +
            '<time>' + escapeHtml(msg.at) + '</time>' +
            '</div>';
        return row;
    }

    function ensureSpacer() {
        if (!feed || feed.querySelector('.connect-feed-spacer')) return;
        var spacer = document.createElement('div');
        spacer.className = 'connect-feed-spacer';
        feed.insertBefore(spacer, feed.firstChild);
    }

    function trackIds(id) {
        lastId = Math.max(lastId, id);
        firstId = firstId === 0 ? id : Math.min(firstId, id);
    }

    function poll() {
        if (!feed) return;
        var api = feed.getAttribute('data-api');
        var groupId = feed.getAttribute('data-group-id');
        fetch(api + '?action=messages&groupId=' + encodeURIComponent(groupId) + '&afterId=' + lastId, { credentials: 'same-origin' })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data || !data.ok || !data.messages) return;
                var stick = firstLoad || (feed.scrollHeight - feed.scrollTop - feed.clientHeight < 90);
                ensureSpacer();
                data.messages.forEach(function (msg) {
                    var id = parseInt(msg.id, 10) || 0;
                    trackIds(id);
                    feed.appendChild(buildRow(msg));
                });
                if (stick) feed.scrollTop = feed.scrollHeight;
                firstLoad = false;
            })
            .catch(function () { });
    }

    function loadOlder() {
        if (!feed || loadingOlder || !hasOlder || firstId <= 0) return;
        loadingOlder = true;
        var api = feed.getAttribute('data-api');
        var groupId = feed.getAttribute('data-group-id');
        var prevHeight = feed.scrollHeight;
        fetch(api + '?action=messages&groupId=' + encodeURIComponent(groupId) + '&beforeId=' + firstId, { credentials: 'same-origin' })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                loadingOlder = false;
                if (!data || !data.ok || !data.messages) return;
                if (data.messages.length < 40) hasOlder = false;
                if (!data.messages.length) return;
                var anchor = feed.querySelector('.connect-row');
                data.messages.forEach(function (msg) {
                    var id = parseInt(msg.id, 10) || 0;
                    trackIds(id);
                    var row = buildRow(msg);
                    if (anchor) feed.insertBefore(row, anchor);
                    else feed.appendChild(row);
                });
                feed.scrollTop = feed.scrollHeight - prevHeight;
            })
            .catch(function () { loadingOlder = false; });
    }

    if (feed) {
        feed.addEventListener('scroll', function () {
            if (feed.scrollTop < 48) loadOlder();
        });
    }

    function send() {
        if (!feed || !input) return;
        var text = input.value.trim();
        var file = imageInput && imageInput.files ? imageInput.files[0] : null;
        if (!text && !file) return;
        var form = new FormData();
        form.append('action', 'send');
        form.append('groupId', feed.getAttribute('data-group-id'));
        form.append('type', type);
        form.append('content', text);
        if (file) form.append('image', file);
        if (status) status.textContent = '';
        fetch(feed.getAttribute('data-api'), { method: 'POST', body: form, credentials: 'same-origin' })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data || !data.ok) {
                    if (status) status.textContent = (data && data.error) || 'Could not send.';
                    return;
                }
                input.value = '';
                input.style.height = 'auto';
                if (imageInput) imageInput.value = '';
                if (imageName) imageName.textContent = '';
                hide(emojiBar);
                poll();
            })
            .catch(function () {
                if (status) status.textContent = 'Could not send.';
            });
    }

    var sendBtn = document.getElementById('btnConnectSend');
    if (sendBtn) sendBtn.addEventListener('click', send);
    if (input) {
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                send();
            }
        });
    }

    poll();
    setInterval(poll, 2500);
})();
