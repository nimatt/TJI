/*
 * This file is part of TJI.
 * 
 * TJI is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * TJI is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with TJI.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace TJI.Communication
{
    public abstract class SessionClient
    {
        private static readonly Log Logger = Log.GetLogger(typeof(SessionClient));

        private Regex CookieContents => new Regex(CookieName + @"=(?<contents>[^;]+);.*Path=(?<path>[^;]+);.*", RegexOptions.Compiled);

        public abstract string SessionUrl { get; }

        public event Action LogonSucceeded;
        public event Action LogonFailed;

        public event Action LogoutSucceeded;
        public event Action LogoutFailed;

        protected abstract IHttpDataSource DataSource { get; }

        public abstract string CookieName { get; }
        protected abstract string ClientName { get; }

        protected Cookie AuthCookie { get; private set; }


        // TODO: Add expiration
        public bool IsLoggedIn => AuthCookie != null;

        protected abstract void AddAuthenticationData(HttpWebRequest request);
        
        public void LogIn()
        {
            Logger.Debug($"Logger in to {ClientName} started");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SessionUrl);
            AddAuthenticationData(request);
            request.Method = "POST";

            try
            {
                using (IHttpResponse response = DataSource.GetResponse(request))
                {
                    var headers = response.Headers;
                    if (headers.Keys.Any(k => k.Equals("Set-Cookie")))
                    {
                        Match cookieMatch = CookieContents.Match(headers["Set-Cookie"]);
                        if (cookieMatch.Success)
                        {
                            AuthCookie = new Cookie(CookieName, cookieMatch.Groups["contents"].Value, cookieMatch.Groups["path"].Value);
                            Logger.Debug($"Logged in to {ClientName}");
                            LogonSucceeded?.Invoke();
                        }
                        else
                        {
                            Logger.WarnFormat("Response to log in from {0} didn't contain a Set-Cookie in the format {1}.{2}Instead we got: {3}",
                                ClientName, CookieContents, Environment.NewLine, headers["Set-Cookie"]);
                            LogonFailed?.Invoke();
                        }
                    }
                    else
                    {
                        Logger.WarnFormat("Response to log in from {0} didn't contain a Set-Cookie header. Status received is {1}", ClientName, response.StatusCode);
                        LogonFailed?.Invoke();
                    }
                }
            }
            catch (WebException we)
            {
                Logger.Error($"Error during log in to {ClientName}", we);
                LogonFailed?.Invoke();
            }
        }

        public void LogOut()
        {
            if (AuthCookie == null)
                return;

            Logger.Debug($"Logging out from {ClientName}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SessionUrl);
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(request.RequestUri, AuthCookie);
            request.CookieContainer = cookieContainer;
            request.Method = "DELETE";

            try
            {
                using (IHttpResponse response = DataSource.GetResponse(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        AuthCookie = null;
                        Logger.Debug($"Logged out from {ClientName}");
                        LogoutSucceeded?.Invoke();
                    }
                    else
                    {
                        Logger.Debug($"Failed to log out from {ClientName}");
                        LogoutFailed?.Invoke();
                    }
                }
            }
            catch (WebException we)
            {
                Logger.Error($"Error during log out from {ClientName}", we);
                LogoutFailed?.Invoke();
            }
        }
    }
}