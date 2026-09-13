using System;
using System.Web.UI.WebControls;

namespace DigitalTransparencySystem.Helpers
{
    public static class UiNotice
    {
        public const string StickySuccess = "dtas-notice dtas-notice-success dtas-notice-sticky";
        public const string StickyDanger = "dtas-notice dtas-notice-danger dtas-notice-sticky";
        public const string StickyWarning = "dtas-notice dtas-notice-warning dtas-notice-sticky";
        public const string StickyInfo = "dtas-notice dtas-notice-info dtas-notice-sticky";
        public const string ToastSuccess = "dtas-notice dtas-notice-success dtas-toast";
        public const string ToastDanger = "dtas-notice dtas-notice-danger dtas-toast";

        public static void Bind(Label label, string error, string ok)
        {
            if (label == null)
                return;

            if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(ok))
            {
                label.Text = "";
                label.CssClass = "dtas-notice";
                return;
            }

            if (!string.IsNullOrEmpty(error))
            {
                label.CssClass = StickyDanger;
                label.Text = error;
                return;
            }

            label.CssClass = ToastSuccess;
            label.Text = ok;
        }

        public static void Bind(Label label, string text, bool ok)
        {
            if (ok)
                Bind(label, null, text);
            else
                Bind(label, text, null);
        }

        public static void BindPanel(Panel panel, Label label, string error, string ok)
        {
            if (panel == null)
            {
                Bind(label, error, ok);
                return;
            }

            if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(ok))
            {
                panel.Visible = false;
                if (label != null)
                    label.Text = "";
                return;
            }

            panel.Visible = true;
            if (!string.IsNullOrEmpty(error))
            {
                panel.CssClass = StickyDanger;
                if (label != null)
                {
                    label.CssClass = "";
                    label.Text = error;
                }
                return;
            }

            panel.CssClass = ToastSuccess;
            if (label != null)
            {
                label.CssClass = "";
                label.Text = ok;
            }
        }

        public static void BindPanel(Panel panel, Label label, string text, bool ok)
        {
            if (ok)
                BindPanel(panel, label, null, text);
            else
                BindPanel(panel, label, text, null);
        }

        public static bool HasError(Label label)
        {
            return label != null
                && !string.IsNullOrEmpty(label.CssClass)
                && label.CssClass.IndexOf("dtas-notice-danger", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
