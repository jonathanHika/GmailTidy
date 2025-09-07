using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailTidy.Core
{
    public static class GmailClient
    {
        private static readonly string[] Scopes = {
            GmailService.Scope.GmailModify,
            GmailService.Scope.GmailSettingsBasic,
            GmailService.Scope.GmailLabels
        };

        public static GmailService CreateService(string appName = "GmailTidy")
        {
            using var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            var credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GmailTidy", "token");
            Directory.CreateDirectory(credPath);

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = appName
            });
        }
    }
}
