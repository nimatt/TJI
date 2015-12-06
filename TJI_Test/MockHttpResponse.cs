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
    internal class MockHttpResponse<TR> : IHttpResponse
    {
        internal IDictionary<string, string> FakeHeaders { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Headers => FakeHeaders;

        internal HttpStatusCode FakeStatusCode { get; set; }
        public HttpStatusCode StatusCode => FakeStatusCode;

        public TR FakeResponseObject { get; set; }
        public T GetResponseData<T>() where T : class => FakeResponseObject as T;

        public void Dispose()
        {
            
        }
    }
}