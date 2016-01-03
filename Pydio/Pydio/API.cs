using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Pydio.Pydio
{
    class API
    {
        public Models.Server Server;
        HttpClient httpClient;

        public API(Models.Server Server)
        {
            this.Server = Server;
            httpClient = new HttpClient(new Utils.BasicAuthHandler(Server.Username, Server.Password));
        }

        private async Task<string> PostRequest(string URL)
        {
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
                return httpResponseBody;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Models.Node>> ListWorkspaces()
        {
            string URL = Server.Url + "/api/pydio/state/user/repositories";
            string response = await PostRequest(URL);

            List<Models.Node> repositories = parseRepositories(response);

            foreach (Models.Repository repo in repositories) {
                System.Diagnostics.Debug.WriteLine("Repo:" + repo.Label);
            }

            return repositories;


        }

        public async void Ls(string WorkSpace, string Path)
        {
            WorkSpace = "/api/media";
            string URL = Server.Url + WorkSpace + "/ls/" + Path;

            string response = await PostRequest(URL);

        }

        private List<Models.Node> parseRepositories(string xml)
        {
            List<Models.Node> repositories = new List<Models.Node>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNodeList children = xmlDoc.GetElementsByTagName("repositories").Item(0).ChildNodes;
            foreach (XmlNode child in children)
            {
                Models.Repository repository = new Models.Repository();
                bool add = false;
                foreach (XmlAttribute attribute in child.Attributes)
                {
                    if (attribute.Name.Equals("access_type") && attribute.Value.Equals("fs"))
                    {
                        add = true;
                    }

                    if (attribute.Name.Equals("repositorySlug"))
                    {
                        repository.Id = attribute.Value;
                    }
                    //System.Diagnostics.Debug.WriteLine("child:" + child.Name);
                }

                //Getting the label
                if (add) {
                    //System.Diagnostics.Debug.WriteLine("Label:" + child.FirstChild.InnerXml);

                    repository.Label = child.FirstChild.InnerXml;
                    repositories.Add(repository);
                }


            }
            return repositories;

        }
    }
}
