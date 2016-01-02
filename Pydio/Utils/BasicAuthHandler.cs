using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pydio.Utils
{
    public class BasicAuthHandler : DelegatingHandler
    {
        private readonly string username;
        private readonly string password;

        public BasicAuthHandler(string username, string password)
            : this(username, password, new HttpClientHandler())
        {
        }

        public BasicAuthHandler(string username, string password, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.username = username;
            this.password = password;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = CreateBasicHeader();

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        public AuthenticationHeaderValue CreateBasicHeader()
        {
            var byteArray = System.Text.Encoding.UTF8.GetBytes(username + ":" + password);
            var base64String = Convert.ToBase64String(byteArray);
            return new AuthenticationHeaderValue("Basic", base64String);
        }
    }
}
