using System;
using System.Collections.Generic;
using System.Net;
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

            foreach (Models.Repository repo in repositories)
            {
                System.Diagnostics.Debug.WriteLine("Repo:" + repo.Label);
            }

            return repositories;


        }

        public async Task<List<Models.File>> browse(List<string> path)
        {

            string Workspace = "";
            string Path = "";

            for (int i = 0; i < path.Count; i++)
            {
                string item = path[i];
                if (i == 0)
                {
                    Workspace = item;
                }
                else {
                    string encodedPath = "/";
                    foreach (String pathItem in item.Split('/'))
                    {
                        encodedPath += WebUtility.UrlEncode(pathItem) + "/";
                    }
                    Path += encodedPath;
                }
            }


            return await Ls(Workspace, Path);
        }

        private string EncodePath(string path)
        {
            string encodedPath = "/";
            foreach (String pathItem in path.Split('/'))
            {
                if (!pathItem.Equals(""))
                {
                    encodedPath += WebUtility.UrlEncode(pathItem) + "/";
                }
            }

            return encodedPath.Remove(encodedPath.Length - 1);
        }

        public async Task<List<Models.File>> Ls(string WorkSpace, string Path)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(WorkSpace) + "/ls" + EncodePath(Path);

            string response = await PostRequest(URL);
            try
            {
                return parseFolder(response, WorkSpace);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Source);
                throw e;
            }
        }


        public async Task<bool> Move(string WorkSpace, string From, string To)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(WorkSpace) + "/move/?nodes[]=" + EncodePath(From) + "&dest=" + EncodePath(To);

            string response = await PostRequest(URL);


            return ProcessResponseMessage(response);

        }

        public async Task<bool> Delete(string WorkSpace, string Path)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(WorkSpace) + "/delete" + EncodePath(Path);

            string response = await PostRequest(URL);


            return ProcessResponseMessage(response);

        }

        public async Task<bool> Copy(string WorkSpace, string From, string To)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(WorkSpace) + "/copy/?nodes[]=" + EncodePath(From) + "&dest=" + EncodePath(To);

            string response = await PostRequest(URL);


            return ProcessResponseMessage(response);

        }

        public async Task<bool> Rename(Models.File File, string NewName)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(File.WorkSpace) + "/rename/?file=" + EncodePath(File.Path) + "&filename_new=" + WebUtility.UrlEncode(NewName);

            string response = await PostRequest(URL);


            return ProcessResponseMessage(response);


        }

        public async Task<bool> MkDir(string WorkSpace,string Path, string Name)
        {

            string URL = Server.Url + "/api/" + WebUtility.UrlEncode(WorkSpace) + "/mkdir/?dir=" + EncodePath(Path) + "&dirname=" + WebUtility.UrlEncode(Name);

            string response = await PostRequest(URL);
            //http://files.ftpix.com/api/media/mkdir/PydioTests?dirname=new-folder-newnew&dir=/PydioTests

            // return ProcessResponseMessage(response);
            //Pydio always return an error even if the file has been created properly...
            return true;


        }

        private bool ProcessResponseMessage(string xml)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNodeList children = xmlDoc.GetElementsByTagName("tree").Item(0).ChildNodes;
            foreach (XmlNode child in children)
            {
                if (child.Name.Equals("message"))
                {
                    foreach (XmlAttribute attribute in child.Attributes)
                    {
                        if (attribute.Name.Equals("type"))
                        {
                            if (attribute.Value.Equals("SUCCESS"))
                            {
                                return true;
                            }
                            else {

                                throw new Exceptions.PydioException(child.InnerText);
                            }
                        }
                    }
                }
            }

            return false;

        }

        private List<Models.File> parseFolder(string xml, string WorkSpace)
        {
            List<Models.File> files = new List<Models.File>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNodeList children = xmlDoc.GetElementsByTagName("tree").Item(0).ChildNodes;
            foreach (XmlNode child in children)
            {
                Models.File file = new Models.File();
                file.WorkSpace = WorkSpace;
                System.Diagnostics.Debug.WriteLine("child:" + child.Name);

                //ignoring the pagination child
                if (child.Name.Equals("tree"))
                {

                    foreach (XmlAttribute attribute in child.Attributes)
                    {
                        switch (attribute.Name)
                        {
                            case "text":
                                file.Label = attribute.Value;
                                break;
                            case "is_file":
                                file.Folder = attribute.Value.Equals("false");
                                break;
                            case "filename":
                                file.Id = attribute.Value;
                                break;
                            case "mimestring":
                                file.Mime = attribute.Value;
                                System.Diagnostics.Debug.WriteLine("MIME:" + file.Mime);
                                break;
                        }
                    }

                    files.Add(file);

                }
            }

            return files;
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
                    System.Diagnostics.Debug.WriteLine("child:" + child.Name);
                }

                //Getting the label
                if (add)
                {
                    //System.Diagnostics.Debug.WriteLine("Label:" + child.FirstChild.InnerXml);

                    repository.Label = child.FirstChild.InnerXml;
                    repositories.Add(repository);
                }


            }
            return repositories;

        }
    }
}
