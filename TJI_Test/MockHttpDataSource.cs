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
using TJI.Communication;

namespace TJI_Test
{
    internal class MockHttpDataSource : IHttpDataSource
    {
        private readonly Dictionary<string, IHttpResponse> _fakeResponses = new Dictionary<string, IHttpResponse>();
        private readonly Dictionary<string, WebException> _fakeExceptions = new Dictionary<string, WebException>();

        public IHttpResponse GetResponse(HttpWebRequest request)
        {
            string key = $"{request.RequestUri.AbsoluteUri}##{request.Method}";

            if (_fakeResponses.ContainsKey(key))
            {
                return _fakeResponses[key];
            }
            if (_fakeExceptions.ContainsKey(key))
            {
                throw _fakeExceptions[key];
            }

            return null;
        }

        public void WriteRequestData(HttpWebRequest request, string data)
        {
            
        }

        public void SetResponse(string url, string method, IHttpResponse response)
        {
            string key = $"{url}##{method}";
            if (_fakeResponses.ContainsKey(key))
            {
                _fakeResponses.Remove(key);
            }

            _fakeResponses.Add(key, response);
        }

        public void SetException(string url, string method, WebException exception)
        {
            string key = $"{url}##{method}";
            if (_fakeExceptions.ContainsKey(key))
            {
                _fakeExceptions.Remove(key);
            }

            _fakeExceptions.Add(key, exception);
        }
    }
}