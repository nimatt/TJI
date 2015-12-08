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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using NUnit.Framework;
using TJI.Communication;
using TJI.Toggl;

namespace TJI_Test.Toggl
{
    [TestFixture]
    public class TogglClientFixture
    {
        private const string FakeToken = "fake token";
        private const string CookieName = "toggl_api_session_new";
        private const string SessionUrl = "https://www.toggl.com/api/v8/sessions";
        private const string ValidFakeCookie = CookieName + "=secret_value;Domain=toggl.com;Path=/;";

        [Test]
        public void Login_NoAuthCookieCallsLoginFail()
        {
            ToggleClientStatusKeeper statusKeeper = LoginWithHeaders(new Dictionary<string, string>());

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_EmptySetCookieHeaderLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", string.Empty}};
            ToggleClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_InvalidSetCookieHeaderLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {CookieName, "This Is not a valid header"}
            };
            ToggleClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_EmptyAuthCookieLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Set-Cookie", CookieName + "=;Path=/;"}
            };
            ToggleClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_ValidCookieHeaderLoginSuccess()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Set-Cookie", CookieName + "=secret_value;Path=/;"}
            };
            ToggleClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsFalse(statusKeeper.LogonFailed);
            Assert.IsTrue(statusKeeper.IsLoggedIn);
            Assert.IsTrue(statusKeeper.LogonSucceeded);
        }

        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TogglClient_CreationWithEmptyAuthThrows()
        {
            Assert.Throws(typeof (ArgumentException), () => new TogglClient(string.Empty));
        }

        [Test]
        public void Logout_LogoutWhenNotLoggedInDoesNothing()
        {
            TogglClient client = new TogglClient(FakeToken);
            ToggleClientStatusKeeper statusKeeper = new ToggleClientStatusKeeper(client);
            client.LogOut();

            Assert.IsFalse(statusKeeper.IsLoggedIn);
            Assert.IsFalse(statusKeeper.LogoutFailed);
            Assert.IsFalse(statusKeeper.LogoutSucceeded);
        }

        [Test]
        public void Logout_ForbiddenResponseLogoutFails()
        {
            ToggleClientStatusKeeper statusKeeper = LogInLogOutWithStatus(HttpStatusCode.Forbidden);

            Assert.True(statusKeeper.IsLoggedIn);
            Assert.IsFalse(statusKeeper.LogoutSucceeded);
            Assert.IsTrue(statusKeeper.LogoutFailed);
            Assert.IsTrue(statusKeeper.EncounteredError);
        }

        [Test]
        public void Logout_OKResponseLogoutSucceed()
        {
            ToggleClientStatusKeeper statusKeeper = LogInLogOutWithStatus(HttpStatusCode.OK);

            Assert.IsFalse(statusKeeper.IsLoggedIn);
            Assert.IsTrue(statusKeeper.LogoutSucceeded);
            Assert.IsFalse(statusKeeper.LogoutFailed);
        }

        [Test]
        public void Logout_DataSourceTimeoutFailsLogout()
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            TogglClient client = new TogglClient(FakeToken, source);
            ToggleClientStatusKeeper statusKeeper = new ToggleClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            });
            source.SetException(SessionUrl, "DELETE", new WebException("Timeout", WebExceptionStatus.Timeout));

            client.LogIn();
            client.LogOut();

            Assert.True(statusKeeper.IsLoggedIn);
            Assert.IsFalse(statusKeeper.LogoutSucceeded);
            Assert.IsTrue(statusKeeper.LogoutFailed);
            Assert.IsTrue(statusKeeper.EncounteredError);
        }

        [Test]
        public void GetEntries_ValidEntryReturned()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglEntry[] entries = {
                CreateTogglEntry(new DateTime(2015,12,5,12,0,0), 50)
            };
            IEnumerable<TogglEntry> returnedEntries = GetEntries(entries, from, to);

            Assert.AreEqual(1, returnedEntries.Count());
        }

        [Test]
        public void GetEntries_ShortEntriesRemoved()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglEntry[] entries = {
                CreateTogglEntry(new DateTime(2015,12,5,12,0,0), 5),
                CreateTogglEntry(new DateTime(2015,12,5,12,0,0), 50),
                CreateTogglEntry(new DateTime(2015,12,5,12,0,0), 14)
            };
            IEnumerable<TogglEntry> returnedEntries = GetEntries(entries, from, to);

            Assert.AreEqual(1, returnedEntries.Count());
        }

        [Test]
        public void GetEntries_NoEntriesReturnsEmptyCollection()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglEntry[] entries = {};
            IEnumerable<TogglEntry> returnedEntries = GetEntries(entries, from, to);

            Assert.AreEqual(0, returnedEntries.Count());
        }

        [Test]
        public void GetEntries_NullReturnsNull()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            IEnumerable<TogglEntry> returnedEntries = GetEntries(null, from, to);

            Assert.IsNull(returnedEntries);
        }

        [Test]
        public void GetEntries_NullInvokesFail()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglClient client;
            ToggleClientStatusKeeper statusKeeper;
            var source = GetLoggedInClient(out client, out statusKeeper);

            source.SetResponse(client.GetFormattedEntriesUrl(from, to), "GET", new MockHttpResponse<TogglEntry[]>()
            {
                FakeStatusCode = HttpStatusCode.OK,
                FakeResponseObject = null
            });

            client.GetEntries(from, to);

            Assert.IsTrue(statusKeeper.GetEntriesFailed);
        }

        [Test]
        public void GetEntries_HttpErrorStatusCausesFail()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglClient client;
            ToggleClientStatusKeeper statusKeeper;
            var source = GetLoggedInClient(out client, out statusKeeper);

            source.SetResponse(client.GetFormattedEntriesUrl(from, to), "GET", new MockHttpResponse<TogglEntry[]>()
            {
                FakeStatusCode = HttpStatusCode.BadRequest,
                FakeResponseObject = new[]
                {
                    CreateTogglEntry(from.AddDays(1), 60)
                }
            });

            client.GetEntries(from, to);

            Assert.IsTrue(statusKeeper.GetEntriesFailed);
        }

        [Test]
        public void GetEntries_TimeoutCausesFail()
        {
            DateTime from = new DateTime(2015, 12, 4);
            DateTime to = new DateTime(2015, 12, 6);
            TogglClient client;
            ToggleClientStatusKeeper statusKeeper;
            var source = GetLoggedInClient(out client, out statusKeeper);

            source.SetException(client.GetFormattedEntriesUrl(from, to), "GET", new WebException("Web exception", WebExceptionStatus.Timeout));

            var entries = client.GetEntries(from, to);

            Assert.IsNull(entries);
            Assert.IsTrue(statusKeeper.GetEntriesFailed);
        }

        private static MockHttpDataSource GetLoggedInClient(out TogglClient client, out ToggleClientStatusKeeper statusKeeper)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            client = new TogglClient(FakeToken, source);
            statusKeeper = new ToggleClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            });

            client.LogIn();

            Assert.IsTrue(statusKeeper.IsLoggedIn);
            return source;
        }

        private static TogglEntry CreateTogglEntry(DateTime time, int duration, string desc = "test")
        {
            return new TogglEntry()
            {
                At = time.ToString(CultureInfo.CurrentCulture),
                Start = time.ToString(CultureInfo.CurrentCulture),
                Stop = time.AddSeconds(duration).ToString(CultureInfo.CurrentCulture),
                Duration = duration,
                Description = desc
            };
        }

        private static IEnumerable<TogglEntry> GetEntries(TogglEntry[] inputEntries, DateTime from, DateTime to)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            TogglClient client = new TogglClient(FakeToken, source);
            ToggleClientStatusKeeper statusKeeper = new ToggleClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            });

            client.LogIn();

            Assert.IsTrue(statusKeeper.IsLoggedIn);

            source.SetResponse(client.GetFormattedEntriesUrl(from, to), "GET", new MockHttpResponse<TogglEntry[]>()
            {
                FakeStatusCode = HttpStatusCode.OK,
                FakeResponseObject = inputEntries
            });

            return client.GetEntries(from, to);
        } 

        private static ToggleClientStatusKeeper LogInLogOutWithStatus(HttpStatusCode statusCode)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            TogglClient client = new TogglClient(FakeToken, source);
            ToggleClientStatusKeeper statusKeeper = new ToggleClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            });
            source.SetResponse(SessionUrl, "DELETE", new MockHttpResponse<TogglEntry[]>()
            {
                FakeStatusCode = statusCode
            });
            client.LogIn();
            client.LogOut();
            return statusKeeper;
        }

        private static ToggleClientStatusKeeper LoginWithHeaders(Dictionary<string, string> headers)
        {
            IHttpResponse response = new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            };
            MockHttpDataSource source = new MockHttpDataSource();
            TogglClient client = new TogglClient(FakeToken, source);
            ToggleClientStatusKeeper statusKeeper = new ToggleClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", response);
            client.LogIn();
            return statusKeeper;
        }

        private class ToggleClientStatusKeeper
        {
            private readonly TogglClient _togglClient;

            public bool LogonFailed { get; private set; }
            public bool LogonSucceeded { get; private set; }
            public bool LogoutFailed { get; private set; }
            public bool LogoutSucceeded { get; private set; }
            public bool GetEntriesFailed { get; private set; }

            public bool IsLoggedIn => _togglClient.IsLoggedIn;

            public bool EncounteredError => _togglClient.EncounteredError;

            public ToggleClientStatusKeeper(TogglClient client)
            {
                _togglClient = client;

                client.LogonFailed += delegate { LogonFailed = true; };
                client.LogonSucceeded += delegate { LogonSucceeded = true; };
                client.LogoutFailed += delegate { LogoutFailed = true; };
                client.LogoutSucceeded += delegate { LogoutSucceeded = true; };
                client.FetchingEntriesFailed += delegate { GetEntriesFailed = true; };
            }
        }
    }
}