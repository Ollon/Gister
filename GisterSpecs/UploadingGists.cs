using EchelonTouchInc.Gister.Api;
using NUnit.Framework;
using Should.Fluent;

namespace GisterSpecs
{
    public class UploadingGists
    {
        [Test]
        public void UploadTheGistToGitHub()
        {
            var gitHubSender = new MockGitHubSender();
            var gistApi = new GistApi { GitHubSender = gitHubSender };

            gistApi.Create(new GitHubCredentials("get", "real"), "file3.cs", "My oh my");

            gitHubSender.SentAGist.Should().Be.True();
        }
        [Test]
        public void GistUrlWillBeAvaible()
        {
            string actualUrl = null;

            var gitHubSender = new MockGitHubSender() { ResultingUrl = "http://gist.github.com/123" };
            var gistApi = new GistApi { GitHubSender = gitHubSender, UrlAvailable = url=>actualUrl = url };

            gistApi.Create(new GitHubCredentials("get", "real"), "file3.cs", "My oh my");

            actualUrl.Should().Equal("http://gist.github.com/123");

        }
    }
}