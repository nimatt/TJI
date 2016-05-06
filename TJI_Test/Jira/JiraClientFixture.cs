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

using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using TJI.Communication;
using TJI.Jira;
using TJI.Toggl;

namespace TJI_Test.Jira
{
    [TestFixture]
    public class JiraClientFixture
    {
        private const string FakeUsername = "user";
        private const string FakePassword = "pwd";
        private const string FakeServerUrl = "https://jira.local:8443";
        private static readonly string SessionUrl = $"{FakeServerUrl}/rest/auth/1/session";
        private const string CookieName = "JSESSIONID";
        private const string ValidFakeCookie = CookieName + "=secret_value;Domain=jira.atlassian.com;Path=/;";

        [Test]
        public void Login_NoAuthCookieCallsLoginFail()
        {
            JiraClientStatusKeeper statusKeeper = LoginWithHeaders(new Dictionary<string, string>());

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_EmptySetCookieHeaderLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", string.Empty}};
            JiraClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_InvalidSetCookieHeaderLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {CookieName, "This Is not a valid header"}
            };
            JiraClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_EmptyAuthCookieLoginFail()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Set-Cookie", CookieName + "=;Path=/;"}
            };
            JiraClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsTrue(statusKeeper.LogonFailed);
        }

        [Test]
        public void Login_ValidCookieHeaderLoginSuccess()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Set-Cookie", CookieName + "=secret_value;Path=/;"}
            };
            JiraClientStatusKeeper statusKeeper = LoginWithHeaders(headers);

            Assert.IsFalse(statusKeeper.LogonFailed);
            Assert.IsTrue(statusKeeper.IsLoggedIn);
            Assert.IsTrue(statusKeeper.LogonSucceeded);
        }

        [Test]
        public void Logout_LogoutWhenNotLoggedInDoesNothing()
        {
            JiraClient client = new JiraClient(FakeUsername, FakePassword, FakeServerUrl);
            JiraClientStatusKeeper statusKeeper = new JiraClientStatusKeeper(client);
            client.LogOut();

            Assert.IsFalse(statusKeeper.IsLoggedIn);
            Assert.IsFalse(statusKeeper.LogoutFailed);
            Assert.IsFalse(statusKeeper.LogoutSucceeded);
        }

        [Test]
        public void Logout_ForbiddenResponseLogoutFails()
        {
            JiraClientStatusKeeper statusKeeper = LogInLogOutWithStatus(HttpStatusCode.Forbidden);

            Assert.True(statusKeeper.IsLoggedIn);
            Assert.IsFalse(statusKeeper.LogoutSucceeded);
            Assert.IsTrue(statusKeeper.LogoutFailed);
            Assert.IsTrue(statusKeeper.EncounteredError);
        }

        [Test]
        public void Logout_OKResponseLogoutSucceed()
        {
            JiraClientStatusKeeper statusKeeper = LogInLogOutWithStatus(HttpStatusCode.OK);

            Assert.IsFalse(statusKeeper.IsLoggedIn);
            Assert.IsTrue(statusKeeper.LogoutSucceeded);
            Assert.IsFalse(statusKeeper.LogoutFailed);
        }
        

        private static MockHttpDataSource GetLoggedInClient(out JiraClient client, out JiraClientStatusKeeper statusKeeper)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            client = new JiraClient(FakeUsername, FakePassword, FakeServerUrl, source);
            statusKeeper = new JiraClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            });

            client.LogIn();

            Assert.IsTrue(statusKeeper.IsLoggedIn);
            return source;
        }
        
        private static JiraClientStatusKeeper LogInLogOutWithStatus(HttpStatusCode statusCode)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> {{"Set-Cookie", ValidFakeCookie}};
            MockHttpDataSource source = new MockHttpDataSource();
            JiraClient client = new JiraClient(FakeUsername, FakePassword, FakeServerUrl, source);
            JiraClientStatusKeeper statusKeeper = new JiraClientStatusKeeper(client);

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

        private static JiraClientStatusKeeper LoginWithHeaders(Dictionary<string, string> headers)
        {
            IHttpResponse response = new MockHttpResponse<TogglEntry[]>()
            {
                FakeHeaders = headers
            };
            MockHttpDataSource source = new MockHttpDataSource();
            JiraClient client = new JiraClient(FakeUsername, FakePassword, FakeServerUrl, source);
            JiraClientStatusKeeper statusKeeper = new JiraClientStatusKeeper(client);

            source.SetResponse(SessionUrl, "POST", response);
            client.LogIn();
            return statusKeeper;
        }

        private class JiraClientStatusKeeper
        {
            private readonly JiraClient _jiraClient;

            public bool LogonFailed { get; private set; }
            public bool LogonSucceeded { get; private set; }
            public bool LogoutFailed { get; private set; }
            public bool LogoutSucceeded { get; private set; }

            public bool IsLoggedIn => _jiraClient.IsLoggedIn;

            public bool EncounteredError => _jiraClient.EncounteredError;

            public JiraClientStatusKeeper(JiraClient client)
            {
                _jiraClient = client;

                client.LogonFailed += delegate { LogonFailed = true; };
                client.LogonSucceeded += delegate { LogonSucceeded = true; };
                client.LogoutFailed += delegate { LogoutFailed = true; };
                client.LogoutSucceeded += delegate { LogoutSucceeded = true; };
            }
        }
    }
}