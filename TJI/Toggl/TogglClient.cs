using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TJI.Communication;

namespace TJI.Toggl
{
    public class TogglClient : SessionClient
    {
        internal const string EntriesUrl = "https://toggl.com/api/v8/time_entries";
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

        private static readonly Log Logger = Log.GetLogger(typeof(TogglClient));

        private readonly string _apiToken;

        public override string SessionUrl => "https://www.toggl.com/api/v8/sessions";

        public event Action<string> FetchingEntriesFailed;

        protected override IHttpDataSource DataSource { get; }
        public override string CookieName => "toggl_api_session_new";
        protected override string ServerUrl => "https://www.toggl.com";

        protected override string ClientName => "Toggl";

        public bool EncounteredError
        {
            get;
            private set;
        }

        protected override void AddAuthenticationData(HttpWebRequest request)
        {
            string userpass = _apiToken + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;

            request.Headers.Add("Authorization", authHeader);
            request.ContentLength = 0;
        }

        public TogglClient(string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentException("Cannot have an empty api token");
                
            DataSource = new HttpDataSource();

            _apiToken = apiToken;

            LogonFailed += delegate { EncounteredError = true; };
            LogonSucceeded += delegate { EncounteredError = false; };
            LogoutFailed += delegate { EncounteredError = true; };
            LogoutSucceeded += delegate { EncounteredError = false; };
        }

        internal TogglClient(string apiToken, IHttpDataSource dataSource)
            : this(apiToken)
        {
            DataSource = dataSource;
        }

        public IReadOnlyCollection<TogglEntry> GetEntries(DateTime from, DateTime to)
        {
            IReadOnlyCollection<TogglEntry> entries = null;
            string errorMessage = string.Empty;
            string completeUrl = GetFormattedEntriesUrl(from, to);

            Logger.DebugFormat("Getting entries from Toggl updated between {0} and {1}", GetFormattedTime(from), GetFormattedTime(to));
            
            try
            {
                using (IHttpResponse response = GetResponse(() => GetEntriesRequest(completeUrl)))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Debug("Got a OK when getting entries from Toggl");
                        try
                        {
                            entries = response.GetResponseData<TogglEntry[]>();
                        }
                        catch (FormatException fe)
                        {
                            errorMessage = "Invalid response from server";
                            Logger.Error("Error while parsing Toggl server response", fe);
                        }
                        if (entries == null)
                        {
                            Logger.Error("Serializing Toggl server response gave null");
                            errorMessage = "Invalid response from server";
                        }
                        else if (entries.Any())
                        {
                            Logger.DebugFormat("Got {0} entries from Toggl", entries.Count);
                        }
                        else
                        {
                            Logger.Debug("Got an empty array of entries from Toggl");
                        }
                    }
                    else
                    {
                        errorMessage = $"Server returned '{response.StatusCode}'";
                        Logger.WarnFormat("Did not get an OK when fetching entries from Toggle {0}", response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                errorMessage = we.Message;
                Logger.Error("Exception while getting Toggl entries", we);
            }

            if (entries != null)
            {
                EncounteredError = false;
                entries = RemoveTooShortEntries(entries);
            }
            else
            {
                EncounteredError = true;
                FetchingEntriesFailed?.Invoke(errorMessage);
            }

            return entries;
        }

        private HttpWebRequest GetEntriesRequest(string url)
        {
            HttpWebRequest request = GetRequest(url, relative: false);
            request.Method = "GET";
            request.ContentType = "application/json";
            return request;
        }

        internal string GetFormattedEntriesUrl(DateTime from, DateTime to)
        {
            return $"{EntriesUrl}?start_date={GetFormattedTime(from)}&end_date={GetFormattedTime(to)}";
        }

        private static IReadOnlyCollection<TogglEntry> RemoveTooShortEntries(IReadOnlyCollection<TogglEntry> entries)
        {
            entries = (from e in entries
                where e.Duration > 30
                select e).ToList();

            if (entries.Any())
            {
                Logger.DebugFormat("{0} entries exceed 30 seconds", entries.Count);
            }
            else
            {
                Logger.Debug("No entries exceeding 30 seconds");
            }
            return entries;
        }

        private string GetFormattedTime(DateTime time)
        {
            return Uri.EscapeDataString(time.ToString(TimeFormat));
        }
    }
}
