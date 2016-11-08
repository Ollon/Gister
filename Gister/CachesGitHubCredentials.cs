using System;
using System.IO;
using System.Text;
using EchelonTouchInc.Gister.Api.Credentials;

namespace EchelonTouchInc.Gister
{
    public class CachesGitHubCredentials : IRetrievesCredentials
    {
        private const string CredentialsFileName = ".githubcred";

        public CachesGitHubCredentials()
        {
            Encrypt = input =>
                          {
                              byte[] byt = Encoding.UTF8.GetBytes(input);
                              return Convert.ToBase64String(byt);
                          };
            Decrypt = input =>
                          {

                              byte[] byt = Convert.FromBase64String(input);
                              return Encoding.UTF8.GetString(byt);
                          };
        }

        public string TestPathToCredentials { get; set; }

        public Func<string, string> Encrypt { get; set; }

        public Func<string, string> Decrypt { get; set; }

        public bool IsAvailable()
        {
            string path = GetPathToCredentialsFile();

            return File.Exists(path);
        }

        public void AssureNotCached()
        {
            PurgeAnyCache();
        }

        public void Cache(GitHubCredentials credentials)
        {
            GitHubUserCredentials userCredentials = credentials as GitHubUserCredentials;
            if (userCredentials == null) return;

            string path = GetPathToCredentialsFile();

            PurgeAnyCache();

            File.WriteAllLines(path, new[] { Encrypt(userCredentials.Username), Encrypt(userCredentials.Password) });
        }

        public GitHubCredentials Retrieve()
        {
            string path = GetPathToCredentialsFile();

            string[] lines = File.ReadAllLines(path);

            string username = Decrypt(lines[0]);
            string password = Decrypt(lines[1]);

            return new GitHubUserCredentials(username, password);
        }

        private string GetPathToCredentialsFile()
        {
            return IsTestPathProvided() ? TestPathToCredentials : VsProfileCredentials();
        }


        private static string VsProfileCredentials()
        {
            string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return Path.Combine(profilePath, CredentialsFileName);
        }

        private bool IsTestPathProvided()
        {
            return !string.IsNullOrEmpty(TestPathToCredentials);
        }

        private void PurgeAnyCache()
        {
            string path = GetPathToCredentialsFile();

            if (File.Exists(path))
                File.Delete(path);
        }
    }
}