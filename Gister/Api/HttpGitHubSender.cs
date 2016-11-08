using System;
using System.IO;
using System.Net;
using EchelonTouchInc.Gister.Api.Credentials;
using FluentHttp;
using Newtonsoft.Json.Linq;

namespace EchelonTouchInc.Gister.Api
{
    public class HttpGitHubSender : GitHubSender
    {

        public string SendGist(string fileName, string content, string description, bool isPublic, GitHubCredentials credentials)
        {
            string gistAsJson = new CreatesGistMessages().CreateMessage(fileName, content, description, isPublic);

            using (MemoryStream stream = new MemoryStream())
            {
                FluentHttpRequest request = new FluentHttpRequest()
                    .BaseUrl("https://api.github.com")
                    .ResourcePath("/gists")
                    .Method("POST")
                    .Headers(h => h.Add("User-Agent", "Gister"))
                    .Headers(h => h.Add("Content-Type", "application/json"))
                    .Body(x => x.Append(gistAsJson))
                    .OnResponseHeadersReceived((o, e) => e.SaveResponseIn(stream));

                AppliesGitHubCredentialsToFluentHttpRequest.ApplyCredentials(credentials, request);

                FluentHttpAsyncResult response = request
                    .Execute();

                if (response.Response.HttpWebResponse.StatusCode != HttpStatusCode.Created)
                    throw new GitHubUnauthorizedException(response.Response.HttpWebResponse.StatusDescription);

                return PeelOutGistHtmlUrl(response);
            }
        }


        private static string PeelOutGistHtmlUrl(FluentHttpAsyncResult response)
        {
            response.Response.SaveStream.Seek(0, SeekOrigin.Begin);
            string gistJson = FluentHttpRequest.ToString(response.Response.SaveStream);

            dynamic gist = JObject.Parse(gistJson);

            return (string)gist.html_url;
        }
    }

    [Serializable]
    public class GitHubUnauthorizedException : Exception
    {
        public GitHubUnauthorizedException(string statusDescription)
            : base(statusDescription)
        {
        }
    }
}