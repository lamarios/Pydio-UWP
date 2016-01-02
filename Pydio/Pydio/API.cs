using System;
using System.Net.Http;

namespace Pydio.Pydio
{
    class API
    {
        public Models.Server Server;
        HttpClient httpClient;

        public API(Models.Server Server) {
            this.Server = Server;
            httpClient =  new HttpClient(new Utils.BasicAuthHandler(Server.Username, Server.Password));
        }

        public async void Ls(string WorkSpace, string Path) {
            WorkSpace = "/api/media";
            string URL = Server.Url + WorkSpace + "/ls/" + Path;

            System.Diagnostics.Debug.WriteLine("URL:" + URL);

            Uri requestUri = new Uri(URL);

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

          


            try
            {
                //Send the GET request
                httpResponse = await httpClient.PostAsync(requestUri, null);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("Response:" + httpResponseBody);

            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
        }

    }
}
