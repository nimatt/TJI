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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;

namespace TJI.Communication
{
    public class HttpResponse : IHttpResponse
    {
        private readonly HttpWebResponse _webResponse;

        public HttpResponse() { }

        public HttpResponse(HttpWebResponse webResponse)
        {
            _webResponse = webResponse;
        }

        public virtual IDictionary<string, string> Headers
        {
            get
            {
                // TODO: We should handle the case with multiple headers with the same key since it's allowed
                return _webResponse.Headers.AllKeys.ToDictionary(key => key, key => _webResponse.Headers[key]);
            }
        }

        public virtual HttpStatusCode StatusCode => _webResponse.StatusCode;

        public T GetResponseData<T>() where T : class
        {
            T entries = default(T);
            using (Stream stream = _webResponse.GetResponseStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                if (stream != null)
                {
                    entries = serializer.ReadObject(stream) as T;
                }
            }

            return entries;
        } 

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webResponse?.Dispose();
            }
        }
    }
}
