using System;
using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace DigitalTransparencySystem.Helpers
{
    public static class AccountabilityExcelReport
    {
        private static readonly Color Navy = Color.FromArgb(14, 34, 56);
        private static readonly Color Brass = Color.FromArgb(176, 137, 72);
        private static readonly Color Cream = Color.FromArgb(243, 234, 216);

        public static byte[] Build(DataTable staffRows, int taskCompletionPct, int decisionPct, int liveEvents, string preparedBy)
        {
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet summary = package.Workbook.Worksheets.Add("Summary");
                ExcelWorksheet staff = package.Workbook.Worksheets.Add("By staff");
                ExcelWorksheet charts = package.Workbook.Worksheets.Add("Charts");

                WriteSummary(summary, staffRows, taskCompletionPct, decisionPct, liveEvents, preparedBy);
                int lastStaffRow = WriteStaffSheet(staff, staffRows);
                WriteCharts(charts, staff, lastStaffRow);

                summary.PrinterSettings.FitToPage = true;
                staff.PrinterSettings.Orientation = eOrientation.Landscape;
                charts.PrinterSettings.Orientation = eOrientation.Landscape;

                return package.GetAsByteArray();
            }
        }

        private static void WriteSummary(ExcelWorksheet ws, DataTable staffRows, int taskCompletionPct, int decisionPct, int liveEvents, string preparedBy)
        {
            ws.Cells["A1"].Value = "DTAS Accountability Report";
            ws.Cells["A1:B1"].Merge = true;
            StyleTitle(ws.Cells["A1"]);

            ws.Cells["A2"].Value = "Generated";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
            ws.Cells["A3"].Value = "Prepared by";
            ws.Cells["B3"].Value = string.IsNullOrWhiteSpace(preparedBy) ? "Administrator" : preparedBy.Trim();

            ws.Cells["A4"].Value = "Metric";
            ws.Cells["B4"].Value = "Value";
            StyleHeader(ws.Cells["A4:B4"]);

            ws.Cells["A5"].Value = "Task completion %";
            ws.Cells["B5"].Value = taskCompletionPct;
            ws.Cells["A6"].Value = "Decision progress %";
            ws.Cells["B6"].Value = decisionPct;
            ws.Cells["A7"].Value = "Live events";
            ws.Cells["B7"].Value = liveEvents;

            int completed = 0, pending = 0, inProgress = 0, overdue = 0, people = 0;
            foreach (DataRow row in staffRows.Rows)
            {
                people++;
                completed += ToInt(row["Completed"]);
                pending += ToInt(row["Pending"]);
                inProgress += ToInt(row["InProgress"]);
                overdue += ToInt(row["Overdue"]);
            }

            ws.Cells["A9"].Value = "Staff with assigned tasks";
            ws.Cells["B9"].Value = people;
            ws.Cells["A10"].Value = "Completed (assigned)";
            ws.Cells["B10"].Value = completed;
            ws.Cells["A11"].Value = "Pending";
            ws.Cells["B11"].Value = pending;
            ws.Cells["A12"].Value = "In progress";
            ws.Cells["B12"].Value = inProgress;
            ws.Cells["A13"].Value = "Overdue";
            ws.Cells["B13"].Value = overdue;

            ws.Cells["A15"].Value = "Status mix";
            ws.Cells["B15"].Value = "Count";
            StyleHeader(ws.Cells["A15:B15"]);
            ws.Cells["A16"].Value = "Completed";
            ws.Cells["B16"].Value = completed;
            ws.Cells["A17"].Value = "Pending";
            ws.Cells["B17"].Value = pending;
            ws.Cells["A18"].Value = "In progress";
            ws.Cells["B18"].Value = inProgress;
            ws.Cells["A19"].Value = "Overdue";
            ws.Cells["B19"].Value = overdue;

            ws.Column(1).Width = 32;
            ws.Column(2).Width = 16;

            var metricChart = ws.Drawings.AddChart("InstitutionMetrics", eChartType.ColumnClustered);
            metricChart.Title.Text = "Institution metrics";
            metricChart.Series.Add(ws.Cells["B5:B6"], ws.Cells["A5:A6"]).Header = "Percent";
            metricChart.YAxis.MaxValue = 100;
            metricChart.YAxis.MinValue = 0;
            metricChart.Legend.Remove();
            metricChart.SetPosition(3, 0, 3, 0);
            metricChart.SetSize(480, 260);

            var pie = ws.Drawings.AddChart("StatusPie", eChartType.Pie) as ExcelPieChart;
            if (pie != null)
            {
                pie.Title.Text = "Assigned work mix";
                pie.Series.Add(ws.Cells["B16:B19"], ws.Cells["A16:A19"]);
                pie.DataLabel.ShowPercent = true;
                pie.DataLabel.ShowCategory = true;
                pie.DataLabel.ShowValue = false;
                pie.SetPosition(18, 0, 3, 0);
                pie.SetSize(480, 300);
            }
        }

        private static int WriteStaffSheet(ExcelWorksheet ws, DataTable staffRows)
        {
            string[] headers = { "Full name", "Username", "Role", "Total tasks", "Completed", "Pending", "In progress", "Overdue", "Completion %" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cells[1, i + 1].Value = headers[i];
            StyleHeader(ws.Cells[1, 1, 1, headers.Length]);

            int rowIndex = 2;
            foreach (DataRow row in staffRows.Rows)
            {
                int total = ToInt(row["TotalTasks"]);
                int completed = ToInt(row["Completed"]);
                ws.Cells[rowIndex, 1].Value = PersonName(row);
                ws.Cells[rowIndex, 2].Value = CellText(row, "Username");
                ws.Cells[rowIndex, 3].Value = CellText(row, "Role");
                ws.Cells[rowIndex, 4].Value = total;
                ws.Cells[rowIndex, 5].Value = completed;
                ws.Cells[rowIndex, 6].Value = ToInt(row["Pending"]);
                ws.Cells[rowIndex, 7].Value = ToInt(row["InProgress"]);
                ws.Cells[rowIndex, 8].Value = ToInt(row["Overdue"]);
                ws.Cells[rowIndex, 9].Value = total <= 0 ? 0 : Math.Round(completed * 100.0 / total, 1);
                rowIndex++;
            }

            int lastRow = Math.Max(rowIndex - 1, 1);
            if (lastRow >= 2)
                ws.Cells[2, 9, lastRow, 9].Style.Numberformat.Format = "0.0";

            ws.View.FreezePanes(2, 1);
            for (int col = 1; col <= 9; col++)
                ws.Column(col).AutoFit();
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 24);

            return lastRow;
        }

        private static void WriteCharts(ExcelWorksheet charts, ExcelWorksheet staff, int lastStaffRow)
        {
            charts.Cells["A1"].Value = "Accountability charts";
            charts.Cells["A1:D1"].Merge = true;
            StyleTitle(charts.Cells["A1"]);
            charts.Cells["A2"].Value = "Charts below use the Summary and By staff sheets. Open this file in Excel to view them.";
            charts.Cells["A2:D2"].Merge = true;

            if (lastStaffRow >= 2)
            {
                var stacked = charts.Drawings.AddChart("StaffStatus", eChartType.ColumnStacked);
                stacked.Title.Text = "Tasks by person";
                stacked.Series.Add(staff.Cells[2, 5, lastStaffRow, 5], staff.Cells[2, 1, lastStaffRow, 1]).Header = "Completed";
                stacked.Series.Add(staff.Cells[2, 6, lastStaffRow, 6], staff.Cells[2, 1, lastStaffRow, 1]).Header = "Pending";
                stacked.Series.Add(staff.Cells[2, 7, lastStaffRow, 7], staff.Cells[2, 1, lastStaffRow, 1]).Header = "In progress";
                stacked.Series.Add(staff.Cells[2, 8, lastStaffRow, 8], staff.Cells[2, 1, lastStaffRow, 1]).Header = "Overdue";
                stacked.SetPosition(3, 0, 0, 0);
                stacked.SetSize(920, 380);
                stacked.Legend.Position = eLegendPosition.Bottom;

                var pctChart = charts.Drawings.AddChart("StaffPct", eChartType.BarClustered);
                pctChart.Title.Text = "Completion % by person";
                pctChart.Series.Add(staff.Cells[2, 9, lastStaffRow, 9], staff.Cells[2, 1, lastStaffRow, 1]).Header = "Completion %";
                pctChart.XAxis.MaxValue = 100;
                pctChart.Legend.Remove();
                pctChart.SetPosition(24, 0, 0, 0);
                pctChart.SetSize(920, 380);
            }
            else
            {
                charts.Cells["A4"].Value = "No assigned-task rows were found, so staff charts were not added.";
            }
        }

        private static void StyleTitle(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 16;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Navy);
        }

        private static void StyleHeader(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Color.SetColor(Navy);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Cream);
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Color.SetColor(Brass);
        }

        private static string PersonName(DataRow row)
        {
            string fullName = CellText(row, "FullName");
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;
            string username = CellText(row, "Username");
            if (!string.IsNullOrWhiteSpace(username))
                return username;
            return "User " + CellText(row, "UserID");
        }

        private static string CellText(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return "";
            return Convert.ToString(row[column]) ?? "";
        }

        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToInt32(value);
        }
    }
}
