using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigitalTransparencySystem.Helpers
{
    /// <summary>
    /// Thin client for the Zoom REST API (v2) using Server-to-Server OAuth.
    /// Reads Zoom_AccountId / Zoom_ClientId / Zoom_ClientSecret from appSettings.
    /// </summary>
    public class ZoomService
    {
        private const string TokenUrl = "https://zoom.us/oauth/token";
        private const string ApiBase = "https://api.zoom.us/v2";

        private readonly string _accountId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private string _lastError;

        public ZoomService()
        {
            _accountId = ConfigurationManager.AppSettings["Zoom_AccountId"];
            _clientId = ConfigurationManager.AppSettings["Zoom_ClientId"];
            _clientSecret = ConfigurationManager.AppSettings["Zoom_ClientSecret"];
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_accountId)
                    && !string.IsNullOrWhiteSpace(_clientId)
                    && !string.IsNullOrWhiteSpace(_clientSecret);
            }
        }

        /// <summary>Gets a valid S2S OAuth bearer token, or null on failure.</summary>
        public string GetAccessToken()
        {
            if (!IsConfigured) return null;

            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(_clientId) + ":" + HttpUtility.UrlEncode(_clientSecret)));

            var body = new StringBuilder();
            body.Append("grant_type=account_credentials");
            body.Append("&account_id=" + HttpUtility.UrlEncode(_accountId));

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(TokenUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers["Authorization"] = "Basic " + credentials;
                request.Timeout = 15000;

                byte[] data = Encoding.UTF8.GetBytes(body.ToString());
                request.ContentLength = data.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = JObject.Parse(reader.ReadToEnd());
                    return json["access_token"]?.ToString();
                }
            }
            catch (WebException ex)
            {
                _lastError = FormatApiError(ex, "Could not sign in to Zoom. Check Zoom_AccountId, Zoom_ClientId, and Zoom_ClientSecret.");
                return null;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Creates a scheduled Zoom room and returns the join / start links.
        /// Recording is not requested — Zoom cloud recording needs a paid plan and is not used here.
        /// </summary>
        public ZoomMeetingResult CreateMeeting(
            string topic, DateTime startTime, int durationMinutes, string agenda, string timeZone = "UTC")
        {
            return CreateMeetingInternal(topic, startTime, durationMinutes, agenda, timeZone);
        }

        /// <summary>Turns on Zoom cloud auto-recording for an existing meeting. Returns null on success.</summary>
        public string EnableCloudRecording(long zoomMeetingId)
        {
            string token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
                return "Zoom is not configured.";

            var payload = new JObject
            {
                ["settings"] = new JObject
                {
                    ["auto_recording"] = "cloud"
                }
            };

            try
            {
                ExecuteJsonRequest("PATCH", ApiBase + "/meetings/" + zoomMeetingId.ToString(), token, payload.ToString());
                return null;
            }
            catch (WebException ex)
            {
                return FormatApiError(ex,
                    "Could not turn on cloud recording. Enable cloud recording on the Zoom account (this usually needs a paid Zoom plan).");
            }
        }

        private ZoomMeetingResult CreateMeetingInternal(
            string topic, DateTime startTime, int durationMinutes, string agenda, string timeZone)
        {
            string token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
                return Fail(_lastError ?? "Could not sign in to Zoom. Check the Zoom app credentials in Web.config.");

            string host = ResolveHostUser(token);
            if (string.IsNullOrWhiteSpace(host))
                return Fail(_lastError ?? "No Zoom host user was found on this account.");

            var payload = new JObject
            {
                ["topic"] = topic ?? "Meeting",
                ["type"] = 2,
                ["start_time"] = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["duration"] = durationMinutes > 0 ? durationMinutes : 60,
                ["timezone"] = string.IsNullOrWhiteSpace(timeZone) ? "Asia/Kolkata" : timeZone,
                ["agenda"] = agenda ?? "",
                ["settings"] = new JObject
                {
                    ["host_video"] = true,
                    ["participant_video"] = true,
                    ["join_before_host"] = true,
                    ["waiting_room"] = false,
                    ["mute_upon_entry"] = false,
                    ["approval_type"] = 2,
                    ["auto_recording"] = "none"
                }
            };

            try
            {
                string responseBody = ExecuteJsonRequest(
                    "POST",
                    ApiBase + "/users/" + HttpUtility.UrlEncode(host.Trim()) + "/meetings",
                    token,
                    payload.ToString());

                var json = JObject.Parse(responseBody);
                string joinUrl = json["join_url"]?.ToString();
                if (string.IsNullOrWhiteSpace(joinUrl))
                    return Fail("Zoom created a meeting but did not return a join link.");

                return new ZoomMeetingResult
                {
                    MeetingId = json["id"] != null ? json["id"].ToObject<long>() : 0,
                    JoinUrl = joinUrl,
                    StartUrl = json["start_url"]?.ToString(),
                    Passcode = json["password"]?.ToString(),
                    HostId = json["host_id"]?.ToString()
                };
            }
            catch (WebException ex)
            {
                return Fail(FormatApiError(ex, "Zoom could not create the meeting room."));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        private string ResolveHostUser(string token)
        {
            string host = ConfigurationManager.AppSettings["Zoom_HostEmail"];
            if (!string.IsNullOrWhiteSpace(host))
                return host.Trim();

            try
            {
                string body = ExecuteJsonRequest("GET", ApiBase + "/users?status=active&page_size=30", token, null);
                var json = JObject.Parse(body);
                var users = json["users"] as JArray;
                if (users == null || users.Count == 0)
                    return "me";

                foreach (JObject user in users)
                {
                    int type = user["type"] != null ? user["type"].ToObject<int>() : 1;
                    if (type >= 2)
                        return FirstNonEmpty(user["id"]?.ToString(), user["email"]?.ToString());
                }

                return FirstNonEmpty(users[0]["id"]?.ToString(), users[0]["email"]?.ToString());
            }
            catch (WebException)
            {
                return "me";
            }
        }

        private static string FirstNonEmpty(string first, string second)
        {
            if (!string.IsNullOrWhiteSpace(first)) return first.Trim();
            return string.IsNullOrWhiteSpace(second) ? null : second.Trim();
        }

        private static ZoomMeetingResult Fail(string error)
        {
            return new ZoomMeetingResult { Error = error };
        }

        /// <summary>
        /// Fetches cloud recordings for a specific meeting (by join URL or past meeting id).
        /// Returns a list of recording links; empty list when none found.
        /// </summary>
        public List<ZoomRecording> GetMeetingRecordings(string hostIdOrUserId, long? meetingId, DateTime? from, DateTime? to)
        {
            var recordings = new List<ZoomRecording>();

            string token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return recordings;

            string user = string.IsNullOrWhiteSpace(hostIdOrUserId) ? "me" : HttpUtility.UrlEncode(hostIdOrUserId);
            string url = ApiBase + $"/users/{user}/recordings?page_size=300";

            if (meetingId.HasValue)
                url += "&meeting_id=" + meetingId.Value.ToString();

            if (from.HasValue)
                url += "&from=" + from.Value.ToString("yyyy-MM-dd");

            if (to.HasValue)
                url += "&to=" + to.Value.ToString("yyyy-MM-dd");

            try
            {
                string responseBody = ExecuteJsonRequest("GET", url, token, null);
                var json = JObject.Parse(responseBody);
                var meetingList = json["meetings"] as JArray;
                if (meetingList == null) return recordings;

                foreach (JObject meeting in meetingList)
                {
                    var files = meeting["recording_files"] as JArray;
                    if (files == null) continue;

                    foreach (JObject file in files)
                    {
                        string playUrl = file["play_url"]?.ToString();
                        string downloadUrl = file["download_url"]?.ToString();
                        if (string.IsNullOrEmpty(playUrl) && string.IsNullOrEmpty(downloadUrl)) continue;

                        recordings.Add(new ZoomRecording
                        {
                            MeetingId = meeting["id"] != null ? meeting["id"].ToObject<long>() : 0,
                            RecordingType = file["recording_type"]?.ToString() ?? "Zoom",
                            Title = meeting["topic"]?.ToString(),
                            PlayUrl = playUrl,
                            DownloadUrl = downloadUrl,
                            FileSize = file["file_size"] != null ? file["file_size"].ToObject<long?>() : null,
                            DurationSeconds = file["recording_start"] != null && file["recording_end"] != null
                                ? (long?)(DateTime.Parse(file["recording_end"].ToString()) - DateTime.Parse(file["recording_start"].ToString())).TotalSeconds
                                : null,
                            RecordedAt = file["recording_start"] != null
                                ? (DateTime?)DateTime.Parse(file["recording_start"].ToString())
                                : null
                        });
                    }
                }
            }
            catch
            {
                // ignore; caller treats empty list as "no recordings"
            }

            return recordings;
        }

        /// <summary>Ends a live Zoom meeting if it is running, then deletes it so join/start links stop working.</summary>
        public void EndAndDeleteMeeting(long meetingId)
        {
            string token = GetAccessToken();
            if (string.IsNullOrEmpty(token) || meetingId <= 0)
                return;

            try
            {
                ExecuteJsonRequest("PUT", ApiBase + "/meetings/" + meetingId.ToString() + "/status", token,
                    "{\"action\":\"end\"}");
            }
            catch
            {
            }

            try
            {
                ExecuteJsonRequest("DELETE", ApiBase + "/meetings/" + meetingId.ToString(), token, null);
            }
            catch
            {
            }
        }

        /// <summary>Deletes a Zoom meeting by id. Returns true on success.</summary>
        public bool DeleteMeeting(long meetingId)
        {
            string token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                ExecuteJsonRequest("DELETE", ApiBase + "/meetings/" + meetingId.ToString(), token, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ExecuteJsonRequest(string method, string url, string token, string jsonBody)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["Authorization"] = "Bearer " + token;
            request.Timeout = 20000;

            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonBody);
                request.ContentLength = data.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string FormatApiError(WebException ex, string fallback)
        {
            string detail = ReadWebExceptionBody(ex) ?? "";
            if (detail.IndexOf("Cloud recording is not available for host", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "This Zoom host cannot cloud-record. Cloud recording needs a paid Zoom plan (Pro or higher) and Recording turned on for that host. In Zoom go to Admin → Account Management → Recording and enable cloud recording, then Admin → User Management → Users and give this host a Pro license. Free/Basic Zoom users cannot record to the cloud. After that, click Enable cloud recording again. Meetings already created stay on this host until you schedule a new one.";
            }
            if (detail.IndexOf("does not contain scopes", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("meeting:write", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("meeting:update:meeting", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "The Zoom app is missing meeting scopes. In Zoom Marketplace open the Server-to-Server OAuth app → Scopes → add meeting:write:meeting, meeting:write:meeting:admin, user:read:user, and user:read:list_users → Save → Activate.";
            }
            if (!string.IsNullOrWhiteSpace(detail))
                return detail;
            return fallback;
        }

        private static string ReadWebExceptionBody(WebException ex)
        {
            try
            {
                if (ex.Response == null)
                    return null;
                using (Stream stream = ex.Response.GetResponseStream())
                {
                    if (stream == null)
                        return null;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string body = reader.ReadToEnd();
                        if (string.IsNullOrWhiteSpace(body))
                            return null;
                        try
                        {
                            var json = JObject.Parse(body);
                            string message = json["message"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(message))
                                return message;
                        }
                        catch
                        {
                        }
                        return body.Length > 240 ? body.Substring(0, 240) : body;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class ZoomMeetingResult
    {
        public long MeetingId { get; set; }
        public string JoinUrl { get; set; }
        public string StartUrl { get; set; }
        public string Passcode { get; set; }
        public string HostId { get; set; }
        public string Error { get; set; }
    }

    public class ZoomRecording
    {
        public long MeetingId { get; set; }
        public string RecordingType { get; set; }
        public string Title { get; set; }
        public string PlayUrl { get; set; }
        public string DownloadUrl { get; set; }
        public long? FileSize { get; set; }
        public long? DurationSeconds { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}
