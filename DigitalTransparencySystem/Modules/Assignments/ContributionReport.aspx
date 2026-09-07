<%@ Page Title="Contribution Report | DTAS" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ContributionReport.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Assignments.ContributionReport" %>
<%@ Register TagPrefix="uc" TagName="AdminSidebar" Src="~/MasterPages/AdminSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="AdminTopbar" Src="~/MasterPages/AdminTopbar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserSidebar" Src="~/MasterPages/UserSidebar.ascx" %>
<%@ Register TagPrefix="uc" TagName="UserTopbar" Src="~/MasterPages/UserTopbar.ascx" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Assets/css/dashboard.css") %>" />
    <style>
        #reportDownloadRoot { width: 100%; max-width: none; }
        .report-chart-wrap { height: 280px; position: relative; }
        .report-chart-wrap canvas { display: block; }
        .pdf-keep { page-break-inside: avoid; break-inside: avoid; }
        .no-print { }
        @media print {
            .no-print { display: none !important; }
            .pdf-keep { page-break-inside: avoid; break-inside: avoid; }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AdminTopbar ID="adminTop" runat="server" Visible="false" />
    <uc:UserTopbar ID="userTop" runat="server" Visible="false" />

    <div class="flex min-h-screen">
        <uc:AdminSidebar ID="adminSide" ActivePage="Assignments" runat="server" Visible="false" />
        <uc:UserSidebar ID="userSide" ActivePage="Assignments" runat="server" Visible="false" />

        <main class="dashboard-main print-report flex-1 ml-64 p-8">
            <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="standard-card rounded-xl p-8">
                <a href="<%= ResolveUrl("~/Modules/Assignments/MyAssignments.aspx") %>" class="inline-flex items-center gap-1 text-primary font-bold text-sm mb-3">
                    <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                    Back
                </a>
                <p class="text-on-surface-variant">You do not have access to this contribution report.</p>
            </asp:Panel>

            <asp:Panel ID="pnlReport" runat="server">
                <div class="flex justify-end gap-2 mb-4 no-print">
                    <asp:HyperLink ID="lnkWorkspace" runat="server" CssClass="inline-flex items-center gap-1 px-4 py-2 border border-outline rounded-xl font-label-md">
                        <span class="material-symbols-outlined text-[20px]">arrow_back</span>
                        Back
                    </asp:HyperLink>
                    <button type="button" id="btnDownloadReport" class="btn-primary inline-flex items-center gap-1">
                        <span class="material-symbols-outlined text-[20px]">download</span>
                        Download PDF
                    </button>
                </div>
                <asp:HiddenField ID="hidChartJson" runat="server" />
                <asp:HiddenField ID="hidDownloadName" runat="server" />

                <div id="reportDownloadRoot">
                <header class="mb-8">
                    <p class="font-badge-cap text-badge-cap uppercase text-outline mb-2">DTAS contribution report</p>
                    <h1 class="font-headline-lg text-headline-lg text-primary mb-2"><asp:Literal ID="litAssignment" runat="server"></asp:Literal></h1>
                    <p class="text-on-surface-variant"><asp:Literal ID="litMeta" runat="server"></asp:Literal></p>
                    <p class="text-xs text-outline mt-2">Generated <asp:Literal ID="litGenerated" runat="server"></asp:Literal></p>
                </header>

                <section class="standard-card rounded-xl p-6 mb-6">
                    <h3 class="font-title-lg text-title-lg text-primary mb-2">How the score is calculated</h3>
                    <p class="text-on-surface-variant mb-3">Scores come from <code>sp_GetMemberContributionReport</code>. They are not estimated and not random.</p>
                    <ul class="text-sm text-on-surface-variant list-disc pl-5 space-y-1">
                        <li>50% completion - completed assigned tasks / assigned tasks</li>
                        <li>20% timeliness - lower when assigned tasks are overdue</li>
                        <li>20% activity - progress updates recorded in task history</li>
                        <li>10% recency - last recorded activity within 14 days</li>
                    </ul>
                    <p class="text-xs text-outline mt-3">If there is no last update, the report says "No recorded activity". The system does not claim someone was offline.</p>
                    <p class="mt-3 font-semibold">Group progress: <asp:Literal ID="litGroupProgress" runat="server"></asp:Literal></p>
                </section>

                <section class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                    <div class="standard-card rounded-xl p-6 pdf-keep">
                        <h3 class="font-title-lg text-title-lg text-primary mb-3">Contribution scores</h3>
                        <div class="report-chart-wrap"><canvas id="chartScores"></canvas></div>
                    </div>
                    <div class="standard-card rounded-xl p-6 pdf-keep">
                        <h3 class="font-title-lg text-title-lg text-primary mb-2">Score components</h3>
                        <p class="text-xs text-on-surface-variant mb-3">Same formula as the database: 50 completion - 20 timeliness - 20 activity - 10 recency.</p>
                        <div class="report-chart-wrap"><canvas id="chartComponents"></canvas></div>
                    </div>
                    <div class="standard-card rounded-xl p-6 pdf-keep">
                        <h3 class="font-title-lg text-title-lg text-primary mb-3">Tasks completed vs assigned</h3>
                        <div class="report-chart-wrap"><canvas id="chartTasks"></canvas></div>
                    </div>
                    <div class="standard-card rounded-xl p-6 pdf-keep">
                        <h3 class="font-title-lg text-title-lg text-primary mb-3">Task status</h3>
                        <div class="report-chart-wrap"><canvas id="chartStatus"></canvas></div>
                    </div>
                </section>

                <section class="standard-card rounded-xl overflow-hidden mb-6">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Score breakdown</h3>
                    </div>
                    <asp:Repeater ID="rptBreakdown" runat="server">
                        <HeaderTemplate>
                            <table id="tblBreakdown" class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant">
                                        <th class="px-6 py-3">Member</th>
                                        <th class="px-6 py-3">Completion / 50</th>
                                        <th class="px-6 py-3">Timeliness / 20</th>
                                        <th class="px-6 py-3">Activity / 20</th>
                                        <th class="px-6 py-3">Recency / 10</th>
                                        <th class="px-6 py-3">Total</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="px-6 py-3 font-semibold"><%# Eval("FullName") %></td>
                                <td class="px-6 py-3"><%# Eval("CompletionPts") %></td>
                                <td class="px-6 py-3"><%# Eval("TimelinessPts") %></td>
                                <td class="px-6 py-3"><%# Eval("ActivityPts") %></td>
                                <td class="px-6 py-3"><%# Eval("RecencyPts") %></td>
                                <td class="px-6 py-3 font-bold text-primary"><%# DigitalTransparencySystem.Helpers.AssignmentService.FormatPercent(Eval("ContributionScore")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </section>

                <section class="standard-card rounded-xl overflow-hidden mb-6">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Member contribution</h3>
                    </div>
                    <asp:Repeater ID="rptMembers" runat="server">
                        <HeaderTemplate>
                            <table id="tblMembers" class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant">
                                        <th class="px-6 py-3">Member</th>
                                        <th class="px-6 py-3">Assigned</th>
                                        <th class="px-6 py-3">Completed</th>
                                        <th class="px-6 py-3">Pending</th>
                                        <th class="px-6 py-3">Overdue</th>
                                        <th class="px-6 py-3">Updates</th>
                                        <th class="px-6 py-3">Last activity</th>
                                        <th class="px-6 py-3">Score</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="px-6 py-3 font-semibold"><%# Eval("FullName") %>
                                    <span class="block text-xs font-normal text-outline"><%# Eval("Responsibility") %></span></td>
                                <td class="px-6 py-3"><%# Eval("AssignedTasks") %></td>
                                <td class="px-6 py-3"><%# Eval("CompletedTasks") %></td>
                                <td class="px-6 py-3"><%# Eval("PendingTasks") %></td>
                                <td class="px-6 py-3"><%# Eval("OverdueTasks") %></td>
                                <td class="px-6 py-3"><%# Eval("ProgressUpdates") %></td>
                                <td class="px-6 py-3"><%# ActivityLabel(Eval("LastActivity")) %></td>
                                <td class="px-6 py-3 font-bold text-primary"><%# DigitalTransparencySystem.Helpers.AssignmentService.FormatPercent(Eval("ContributionScore")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </section>

                <section class="standard-card rounded-xl overflow-hidden">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Tasks</h3>
                    </div>
                    <asp:Repeater ID="rptTasks" runat="server">
                        <HeaderTemplate>
                            <table id="tblTasks" class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant">
                                        <th class="px-6 py-3">Task</th>
                                        <th class="px-6 py-3">Owner</th>
                                        <th class="px-6 py-3">Status</th>
                                        <th class="px-6 py-3">Due</th>
                                        <th class="px-6 py-3">Last update</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="px-6 py-3"><%# Eval("Title") %></td>
                                <td class="px-6 py-3"><%# Eval("OwnerName") %></td>
                                <td class="px-6 py-3"><%# Eval("Status") %></td>
                                <td class="px-6 py-3"><%# Eval("DueDate") == DBNull.Value ? "-" : Convert.ToDateTime(Eval("DueDate")).ToString("MMM dd, yyyy") %></td>
                                <td class="px-6 py-3"><%# ActivityLabel(Eval("UpdatedAt")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoTasks" runat="server" Visible="false" CssClass="px-6 py-8 text-on-surface-variant">No tasks recorded for this group.</asp:Panel>
                </section>

                <section class="standard-card rounded-xl overflow-hidden mb-6 mt-6">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Submitted files</h3>
                    </div>
                    <asp:Repeater ID="rptFiles" runat="server">
                        <HeaderTemplate>
                            <table id="tblFiles" class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant">
                                        <th class="px-6 py-3">File</th>
                                        <th class="px-6 py-3">Task</th>
                                        <th class="px-6 py-3">Uploaded by</th>
                                        <th class="px-6 py-3">When</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="px-6 py-3"><%# Eval("FileName") %></td>
                                <td class="px-6 py-3"><%# Eval("TaskTitle") %></td>
                                <td class="px-6 py-3"><%# Eval("UploadedByName") %></td>
                                <td class="px-6 py-3"><%# Convert.ToDateTime(Eval("UploadedAt")).ToString("MMM dd, yyyy HH:mm") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoFiles" runat="server" Visible="false" CssClass="px-6 py-8 text-on-surface-variant">No files uploaded yet.</asp:Panel>
                </section>

                <section class="standard-card rounded-xl overflow-hidden mt-6">
                    <div class="px-6 py-4 border-b border-surface-container-high">
                        <h3 class="font-title-lg text-title-lg text-primary">Submitted links</h3>
                    </div>
                    <asp:Repeater ID="rptLinks" runat="server">
                        <HeaderTemplate>
                            <table id="tblLinks" class="w-full text-left font-body-md">
                                <thead>
                                    <tr class="bg-surface-container-low text-on-surface-variant">
                                        <th class="px-6 py-3">Link</th>
                                        <th class="px-6 py-3">Task</th>
                                        <th class="px-6 py-3">Added by</th>
                                        <th class="px-6 py-3">When</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-surface-container-high">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="px-6 py-3"><%# Eval("DisplayLabel") %></td>
                                <td class="px-6 py-3"><%# Eval("TaskTitle") %></td>
                                <td class="px-6 py-3"><%# Eval("AddedByName") %></td>
                                <td class="px-6 py-3"><%# Convert.ToDateTime(Eval("AddedAt")).ToString("MMM dd, yyyy HH:mm") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoLinks" runat="server" Visible="false" CssClass="px-6 py-8 text-on-surface-variant">No links submitted yet.</asp:Panel>
                </section>
                </div>
            </asp:Panel>
        </main>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/jspdf@2.5.2/dist/jspdf.umd.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/jspdf-autotable@3.8.4/dist/jspdf.plugin.autotable.min.js"></script>
    <script type="text/javascript">
        (function () {
            var field = document.getElementById('<%= hidChartJson.ClientID %>');
            var nameField = document.getElementById('<%= hidDownloadName.ClientID %>');
            if (!field || !field.value) return;
            var data = JSON.parse(field.value);

            function themeColor(name, fallback) {
                return (window.dtCss ? window.dtCss(name) : '') || fallback || '';
            }

            var primary = themeColor('--dt-primary', '#001142');
            var tertiary = themeColor('--dt-tertiary', '#005137');
            var secondary = themeColor('--dt-secondary-container', '#d4e3ff');
            var error = themeColor('--dt-error', '#ba1a1a');
            var outline = themeColor('--dt-outline', '#757682');
            var tick = themeColor('--dt-on-surface-variant', '#444651');
            var charts = [];
            var common = {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                plugins: { legend: { labels: { color: tick, boxWidth: 10, font: { size: 10 } } } },
                scales: {
                    x: { ticks: { color: tick, font: { size: 10 } }, grid: { color: 'rgba(0,0,0,0.06)' } },
                    y: { ticks: { color: tick, font: { size: 10 } }, grid: { color: 'rgba(0,0,0,0.06)' } }
                }
            };

            var scoreCtx = document.getElementById('chartScores');
            if (scoreCtx) charts.push(new Chart(scoreCtx, {
                type: 'bar',
                data: { labels: data.names, datasets: [{ label: 'Contribution score', data: data.scores, backgroundColor: primary }] },
                options: Object.assign({ indexAxis: 'y' }, common)
            }));

            var compCtx = document.getElementById('chartComponents');
            if (compCtx) charts.push(new Chart(compCtx, {
                type: 'bar',
                data: {
                    labels: data.names,
                    datasets: [
                        { label: 'Completion', data: data.completion, backgroundColor: primary },
                        { label: 'Timeliness', data: data.timeliness, backgroundColor: tertiary },
                        { label: 'Activity', data: data.activity, backgroundColor: outline },
                        { label: 'Recency', data: data.recency, backgroundColor: error }
                    ]
                },
                options: common
            }));

            var taskCtx = document.getElementById('chartTasks');
            if (taskCtx) charts.push(new Chart(taskCtx, {
                type: 'bar',
                data: {
                    labels: data.names,
                    datasets: [
                        { label: 'Assigned', data: data.assigned, backgroundColor: secondary },
                        { label: 'Completed', data: data.completed, backgroundColor: tertiary }
                    ]
                },
                options: common
            }));

            var statusCtx = document.getElementById('chartStatus');
            if (statusCtx) charts.push(new Chart(statusCtx, {
                type: 'doughnut',
                data: {
                    labels: data.statusLabels,
                    datasets: [{ data: data.statusCounts, backgroundColor: [outline, primary, secondary, tertiary, error] }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: false,
                    plugins: { legend: { position: 'bottom', labels: { color: tick, boxWidth: 10, font: { size: 10 } } } }
                }
            }));

            var btn = document.getElementById('btnDownloadReport');
            if (btn) btn.addEventListener('click', function () {
                var JsPDF = window.jspdf && window.jspdf.jsPDF;
                if (!JsPDF) return;
                btn.disabled = true;
                btn.textContent = 'Preparing PDF...';
                try {
                    buildContributionPdf(JsPDF, nameField).save((nameField && nameField.value ? nameField.value : 'DTAS-Contribution-Report') + '.pdf');
                } finally {
                    btn.disabled = false;
                    btn.innerHTML = '<span class="material-symbols-outlined text-[20px]">download</span> Download PDF';
                }
            });

            function pdfText(value) {
                return String(value == null ? '' : value)
                    .replace(/\u2014|\u2013|\u2212/g, '-')
                    .replace(/\u2018|\u2019|\u201A|\u2032/g, "'")
                    .replace(/\u201C|\u201D|\u201E|\u2033/g, '"')
                    .replace(/\u2022|\u00B7|\u2027|\u2219|\u22C5/g, '-')
                    .replace(/\u00A0/g, ' ')
                    .replace(/\u2026/g, '...');
            }

            function textOf(selector) {
                var el = document.querySelector(selector);
                return pdfText(el ? String(el.textContent || '').replace(/\s+/g, ' ').trim() : '');
            }

            function readTable(id) {
                var table = document.getElementById(id);
                var head = [];
                var body = [];
                if (!table) return { head: head, body: body };
                var ths = table.querySelectorAll('thead th');
                for (var i = 0; i < ths.length; i++)
                    head.push(pdfText((ths[i].textContent || '').replace(/\s+/g, ' ').trim()));
                var rows = table.querySelectorAll('tbody tr');
                for (var r = 0; r < rows.length; r++) {
                    var cells = rows[r].cells;
                    var row = [];
                    for (var c = 0; c < cells.length; c++)
                        row.push(pdfText((cells[c].innerText || '').replace(/\s+\n/g, '\n').trim()));
                    if (row.join('').length)
                        body.push(row);
                }
                return { head: head, body: body };
            }

            function chartPng(id) {
                var canvas = document.getElementById(id);
                if (!canvas) return null;
                try {
                    return canvas.toDataURL('image/png', 1);
                } catch (e) {
                    return null;
                }
            }

            function buildContributionPdf(JsPDF) {
                var doc = new JsPDF({ unit: 'mm', format: 'a4', orientation: 'portrait' });
                var pageW = 210;
                var pageH = 297;
                var margin = 14;
                var contentW = pageW - margin * 2;
                var y = 0;
                var primary = [0, 17, 66];
                var ink = [28, 30, 38];
                var muted = [90, 92, 102];
                var line = [220, 222, 228];
                var footerTop = 286;

                function ensure(h) {
                    if (y + h > footerTop - 4) {
                        doc.addPage();
                        y = 16;
                    }
                }

                function sectionTitle(label) {
                    ensure(12);
                    doc.setFont('helvetica', 'bold');
                    doc.setFontSize(11);
                    doc.setTextColor(primary[0], primary[1], primary[2]);
                    doc.text(label, margin, y);
                    y += 2;
                    doc.setDrawColor(line[0], line[1], line[2]);
                    doc.setLineWidth(0.3);
                    doc.line(margin, y, pageW - margin, y);
                    y += 6;
                }

                function addTable(id, emptyText) {
                    var data = readTable(id);
                    if (!data.body.length) {
                        ensure(8);
                        doc.setFont('helvetica', 'normal');
                        doc.setFontSize(9);
                        doc.setTextColor(muted[0], muted[1], muted[2]);
                        doc.text(emptyText, margin, y);
                        y += 8;
                        return;
                    }
                    ensure(20);
                    doc.autoTable({
                        startY: y,
                        head: [data.head],
                        body: data.body,
                        theme: 'grid',
                        styles: {
                            font: 'helvetica',
                            fontSize: 8,
                            cellPadding: 2.1,
                            overflow: 'linebreak',
                            valign: 'middle',
                            textColor: ink,
                            lineColor: line,
                            lineWidth: 0.15
                        },
                        headStyles: {
                            fillColor: primary,
                            textColor: 255,
                            fontStyle: 'bold',
                            fontSize: 7.5
                        },
                        alternateRowStyles: { fillColor: [246, 247, 250] },
                        margin: { left: margin, right: margin, bottom: 16 },
                        tableWidth: contentW
                    });
                    y = doc.lastAutoTable.finalY + 8;
                }

                doc.setFillColor(primary[0], primary[1], primary[2]);
                doc.rect(0, 0, pageW, 28, 'F');
                doc.setTextColor(255, 255, 255);
                doc.setFont('helvetica', 'normal');
                doc.setFontSize(8.5);
                doc.text('DTAS  -  DIGITAL TRANSPARENCY AND ACCOUNTABILITY SYSTEM', margin, 10);
                doc.setFont('helvetica', 'bold');
                doc.setFontSize(16);
                doc.text('Contribution report', margin, 20);

                y = 38;
                var title = textOf('#reportDownloadRoot h1') || 'Contribution report';
                var meta = textOf('#reportDownloadRoot header p.text-on-surface-variant');
                var generated = textOf('#reportDownloadRoot header p.text-xs');
                var progress = textOf('#reportDownloadRoot .font-semibold') || textOf('#<%= litGroupProgress.ClientID %>');

                doc.setTextColor(primary[0], primary[1], primary[2]);
                doc.setFont('helvetica', 'bold');
                doc.setFontSize(14);
                var titleLines = doc.splitTextToSize(title, contentW);
                doc.text(titleLines, margin, y);
                y += titleLines.length * 6 + 2;

                doc.setFont('helvetica', 'normal');
                doc.setFontSize(9.5);
                doc.setTextColor(ink[0], ink[1], ink[2]);
                if (meta) {
                    var metaLines = doc.splitTextToSize(meta, contentW);
                    doc.text(metaLines, margin, y);
                    y += metaLines.length * 4.6 + 2;
                }
                if (generated) {
                    doc.setTextColor(muted[0], muted[1], muted[2]);
                    doc.setFontSize(8.5);
                    doc.text(generated, margin, y);
                    y += 7;
                }
                if (progress) {
                    doc.setFont('helvetica', 'bold');
                    doc.setFontSize(10);
                    doc.setTextColor(primary[0], primary[1], primary[2]);
                    doc.text(progress.indexOf('Group progress') === 0 ? progress : ('Group progress: ' + progress), margin, y);
                    y += 8;
                }

                sectionTitle('How the score is calculated');
                doc.setFont('helvetica', 'normal');
                doc.setFontSize(9);
                doc.setTextColor(ink[0], ink[1], ink[2]);
                var formula = [
                    'Scores come from the database procedure. They are not estimated and not random.',
                    '50% completion - completed assigned tasks divided by assigned tasks.',
                    '20% timeliness - lower when assigned tasks are overdue.',
                    '20% activity - progress updates recorded in task history.',
                    '10% recency - last recorded activity within 14 days.',
                    'If there is no last update, the report says "No recorded activity".'
                ];
                for (var f = 0; f < formula.length; f++) {
                    var flines = doc.splitTextToSize(formula[f], contentW);
                    ensure(flines.length * 4.4 + 1);
                    doc.text(flines, margin, y);
                    y += flines.length * 4.4 + 1.2;
                }
                y += 4;

                sectionTitle('Charts');
                var chartItems = [
                    { id: 'chartScores', title: 'Contribution scores' },
                    { id: 'chartComponents', title: 'Score components (50 / 20 / 20 / 10)' },
                    { id: 'chartTasks', title: 'Tasks completed vs assigned' },
                    { id: 'chartStatus', title: 'Task status' }
                ];
                var gap = 4;
                var colW = (contentW - gap) / 2;
                var imgH = 58;
                var blockH = 8 + imgH;
                for (var i = 0; i < chartItems.length; i += 2) {
                    ensure(blockH + 4);
                    for (var col = 0; col < 2; col++) {
                        var item = chartItems[i + col];
                        if (!item) continue;
                        var x = margin + col * (colW + gap);
                        doc.setFont('helvetica', 'bold');
                        doc.setFontSize(8.5);
                        doc.setTextColor(ink[0], ink[1], ink[2]);
                        doc.text(item.title, x, y);
                        var png = chartPng(item.id);
                        if (png)
                            doc.addImage(png, 'PNG', x, y + 2.5, colW, imgH, undefined, 'FAST');
                        else {
                            doc.setFont('helvetica', 'normal');
                            doc.setTextColor(muted[0], muted[1], muted[2]);
                            doc.text('Chart unavailable', x, y + 12);
                        }
                    }
                    y += blockH + 6;
                }

                sectionTitle('Score breakdown');
                addTable('tblBreakdown', 'No member scores recorded.');
                sectionTitle('Member contribution');
                addTable('tblMembers', 'No members recorded.');
                sectionTitle('Tasks');
                addTable('tblTasks', 'No tasks recorded for this group.');
                sectionTitle('Submitted files');
                addTable('tblFiles', 'No files uploaded yet.');
                sectionTitle('Submitted links');
                addTable('tblLinks', 'No links submitted yet.');

                var pageCount = doc.getNumberOfPages();
                for (var p = 1; p <= pageCount; p++) {
                    doc.setPage(p);
                    doc.setDrawColor(line[0], line[1], line[2]);
                    doc.setLineWidth(0.25);
                    doc.line(margin, footerTop, pageW - margin, footerTop);
                    doc.setFont('helvetica', 'normal');
                    doc.setFontSize(8);
                    doc.setTextColor(muted[0], muted[1], muted[2]);
                    doc.text('DTAS contribution report', margin, footerTop + 5);
                    doc.text('Page ' + p + ' of ' + pageCount, pageW - margin, footerTop + 5, { align: 'right' });
                }
                return doc;
            }
        })();
    </script>
</asp:Content>
