(function () {
    var root = document.getElementById("txRoot");
    if (!root) return;

    var url = root.getAttribute("data-sync");
    if (!url) return;

    function text(id, value) {
        var el = document.getElementById(id);
        if (el) el.textContent = value == null ? "0" : String(value);
    }

    function bar(id, pct) {
        var el = document.getElementById(id);
        if (el) el.style.width = (pct || 0) + "%";
    }

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function renderDecisions(rows) {
        var box = document.getElementById("txDecisionList");
        if (!box) return;
        if (!rows || !rows.length) {
            box.innerHTML = "<div class=\"tx-empty\">No decisions recorded yet.</div>";
            return;
        }
        box.innerHTML = rows.map(function (row) {
            return "<div class=\"tx-row\"><div><p>" + escapeHtml(row.Title) + "</p><span>Recorded "
                + escapeHtml(row.Recorded) + " · " + escapeHtml(row.Responsible)
                + "</span></div><div class=\"tx-chips\"><span class=\"tx-chip\">"
                + escapeHtml(row.Priority) + "</span><span class=\"tx-chip\">"
                + escapeHtml(row.Status) + "</span><span class=\"tx-chip\">Due "
                + escapeHtml(row.Due) + "</span></div></div>";
        }).join("");
    }

    function renderEvents(rows) {
        var box = document.getElementById("txEventList");
        if (!box) return;
        if (!rows || !rows.length) {
            box.innerHTML = "<div class=\"tx-empty\">No public campus events are live yet.</div>";
            return;
        }
        box.innerHTML = rows.map(function (row) {
            return "<div class=\"tx-event\"><div class=\"tx-event-top\"><div><p class=\"font-semibold m-0\">"
                + escapeHtml(row.Name) + "</p><span class=\"tx-chip\">" + escapeHtml(row.Status)
                + "</span></div><strong>" + escapeHtml(row.Percent)
                + "%</strong></div><div class=\"tx-bar\"><span style=\"width:" + (row.Percent || 0)
                + "%\"></span></div><p class=\"tx-hint\">" + escapeHtml(row.Label) + "</p></div>";
        }).join("");
    }

    function renderMeetings(rows) {
        var box = document.getElementById("txMeetingList");
        if (!box) return;
        if (!rows || !rows.length) {
            box.innerHTML = "<div class=\"tx-empty\">No meetings have been scheduled yet.</div>";
            return;
        }
        box.innerHTML = rows.map(function (row) {
            var chip = row.HasMinutes ? "tx-chip tx-chip-ok" : "tx-chip tx-chip-wait";
            return "<div class=\"tx-row\"><div><p>" + escapeHtml(row.Title) + "</p><span>"
                + escapeHtml(row.When) + "</span></div><span class=\"" + chip + "\">"
                + escapeHtml(row.MinutesLabel) + "</span></div>";
        }).join("");
    }

    function apply(data) {
        if (!data || !data.Ok) return;
        text("txSyncedAt", data.SyncedAt);
        text("txOpenDecisions", data.OpenDecisions);
        text("txImplemented", data.ImplementedDecisions);
        text("txImplementedPct", data.ImplementedPercent);
        text("txPublicEvents", data.PublicEvents);
        text("txEventPct", data.EventProgressPercent);
        text("txMinutes", data.MeetingsWithMinutes);
        text("txMeetings", data.Meetings);
        text("txPolls", data.OpenPolls);
        text("txGapMinutes", data.MeetingsWithMinutes + " / " + data.Meetings);
        text("txGapDecisions", data.ImplementedDecisions + " / " + data.TotalDecisions);
        text("txGapEvents", data.EventProgressPercent + "%");
        bar("txImplementedBar", data.ImplementedPercent);
        bar("txEventBar", data.EventProgressPercent);
        bar("txMinutesBar", data.MinutesPercent);
        bar("txGapMinutesBar", data.MinutesPercent);
        bar("txGapDecisionsBar", data.ImplementedPercent);
        bar("txGapEventsBar", data.EventProgressPercent);
        renderDecisions(data.Decisions);
        renderEvents(data.Events);
        renderMeetings(data.MeetingsList);
    }

    function poll() {
        var req = new XMLHttpRequest();
        req.open("GET", url, true);
        req.onreadystatechange = function () {
            if (req.readyState !== 4) return;
            if (req.status !== 200) return;
            try {
                apply(JSON.parse(req.responseText));
            } catch (e) { }
        };
        req.send();
    }

    setInterval(poll, 5000);
})();
