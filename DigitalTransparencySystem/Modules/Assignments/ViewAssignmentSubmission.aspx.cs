using System;
using System.IO;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class ViewAssignmentSubmission : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            AssignmentTaskFileRecord file = ResolveFile();
            if (file == null || string.IsNullOrWhiteSpace(file.FilePath))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            AssignmentAccess access = AssignmentService.GetAccess(
                file.GroupID,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string);
            if (!access.CanView)
            {
                Response.StatusCode = 403;
                Response.End();
                return;
            }

            string physical = AssignmentService.SubmissionPhysicalPath(file.FilePath, Server);
            if (string.IsNullOrEmpty(physical) || !File.Exists(physical))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            string ext = (Path.GetExtension(file.FileName ?? physical) ?? "").ToLowerInvariant();
            string contentType = "application/octet-stream";
            switch (ext)
            {
                case ".pdf": contentType = "application/pdf"; break;
                case ".png": contentType = "image/png"; break;
                case ".jpg":
                case ".jpeg": contentType = "image/jpeg"; break;
                case ".txt": contentType = "text/plain"; break;
                case ".zip": contentType = "application/zip"; break;
                case ".doc": contentType = "application/msword"; break;
                case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                case ".xls": contentType = "application/vnd.ms-excel"; break;
                case ".xlsx": contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                case ".ppt": contentType = "application/vnd.ms-powerpoint"; break;
                case ".pptx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; break;
            }

            string downloadName = string.IsNullOrWhiteSpace(file.FileName) ? "submission" + ext : Path.GetFileName(file.FileName);
            downloadName = downloadName.Replace("\"", "").Replace("\r", "").Replace("\n", "");
            if (string.IsNullOrWhiteSpace(downloadName))
                downloadName = "submission" + ext;

            bool forceDownload = string.Equals(Request.QueryString["download"], "1", StringComparison.OrdinalIgnoreCase);
            string disposition = forceDownload || !CanPreviewInline(ext) ? "attachment" : "inline";

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", disposition + "; filename=\"" + downloadName + "\"");
            Response.AddHeader("Content-Length", new FileInfo(physical).Length.ToString());
            Response.TransmitFile(physical);
            Response.End();
        }

        private AssignmentTaskFileRecord ResolveFile()
        {
            int fileId;
            if (int.TryParse(Request.QueryString["FileID"], out fileId))
                return AssignmentService.GetTaskFile(fileId);

            int taskId;
            if (int.TryParse(Request.QueryString["TaskID"], out taskId))
                return AssignmentService.GetFirstTaskFile(taskId);

            return null;
        }

        private static bool CanPreviewInline(string ext)
        {
            return ext == ".pdf" || ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".txt";
        }
    }
}
