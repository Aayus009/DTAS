using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public sealed class MailDetail
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public MailDetail(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    public sealed class MailContent
    {
        public string Plain { get; set; }
        public string Html { get; set; }
    }

    public static class MailComposer
    {
        public static string FirstName(string fullName)
        {
            string value = (fullName ?? "").Trim();
            if (value.Length == 0)
                return "there";
            int space = value.IndexOf(' ');
            return space > 0 ? value.Substring(0, space) : value;
        }

        public static MailContent WrapPlain(string heading, string body)
        {
            return Build(null, heading, body, null, null, null, null, null);
        }

        public static MailContent Build(
            string heading,
            string intro,
            string body = null,
            IList<MailDetail> details = null,
            string ctaLabel = null,
            string ctaUrl = null,
            string greetingName = null,
            string code = null)
        {
            string greeting = string.IsNullOrWhiteSpace(greetingName)
                ? null
                : "Hi " + greetingName.Trim() + ",";

            if (string.IsNullOrWhiteSpace(intro) && !string.IsNullOrWhiteSpace(heading))
                intro = heading.Trim();

            var plain = new StringBuilder();
            if (!string.IsNullOrEmpty(greeting))
                plain.AppendLine(greeting).AppendLine();
            if (!string.IsNullOrWhiteSpace(intro))
                plain.AppendLine(intro.Trim()).AppendLine();
            if (details != null)
            {
                foreach (MailDetail row in details)
                {
                    if (row == null || string.IsNullOrWhiteSpace(row.Label))
                        continue;
                    plain.Append(row.Label.Trim()).Append(": ").AppendLine((row.Value ?? "").Trim());
                }
                if (details.Count > 0)
                    plain.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                plain.AppendLine("Your code is " + code.Trim() + ".");
                plain.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(body))
                plain.AppendLine(body.Trim()).AppendLine();
            if (!string.IsNullOrWhiteSpace(ctaUrl))
            {
                if (!string.IsNullOrWhiteSpace(ctaLabel))
                    plain.AppendLine(ctaLabel.Trim() + ":");
                plain.AppendLine(ctaUrl.Trim());
                plain.AppendLine();
            }
            plain.Append("DTAS");

            return new MailContent
            {
                Plain = plain.ToString(),
                Html = ToHtml(greeting, intro, details, body, ctaLabel, ctaUrl, code)
            };
        }

        private static string ToHtml(
            string greeting,
            string intro,
            IList<MailDetail> details,
            string body,
            string ctaLabel,
            string ctaUrl,
            string code)
        {
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\" /></head>");
            html.Append("<body style=\"margin:0;padding:16px;background:#ffffff;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:1.5;color:#222222;\">");

            if (!string.IsNullOrEmpty(greeting))
                html.Append("<p>").Append(H(greeting)).Append("</p>");
            if (!string.IsNullOrWhiteSpace(intro))
                html.Append("<p>").Append(H(intro.Trim())).Append("</p>");

            if (details != null)
            {
                var lines = new StringBuilder();
                foreach (MailDetail row in details)
                {
                    if (row == null || string.IsNullOrWhiteSpace(row.Label))
                        continue;
                    if (lines.Length > 0)
                        lines.Append("<br />");
                    lines.Append(H(row.Label.Trim())).Append(": ").Append(H((row.Value ?? "").Trim()));
                }
                if (lines.Length > 0)
                    html.Append("<p>").Append(lines).Append("</p>");
            }

            if (!string.IsNullOrWhiteSpace(code))
                html.Append("<p>Your code is <b>").Append(H(code.Trim())).Append("</b>.</p>");

            if (!string.IsNullOrWhiteSpace(body))
                html.Append("<p>").Append(H(body.Trim()).Replace("\n", "<br />")).Append("</p>");

            if (!string.IsNullOrWhiteSpace(ctaUrl))
            {
                string label = string.IsNullOrWhiteSpace(ctaLabel) ? ctaUrl.Trim() : ctaLabel.Trim();
                html.Append("<p><a href=\"").Append(H(ctaUrl.Trim())).Append("\">")
                    .Append(H(label)).Append("</a></p>");
            }

            html.Append("<p style=\"margin-top:24px;color:#666666;font-size:12px;\">DTAS</p>");
            html.Append("</body></html>");
            return html.ToString();
        }

        private static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? "");
        }
    }
}
