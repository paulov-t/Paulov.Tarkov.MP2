using Newtonsoft.Json;
using System;

namespace Paulov.Tarkov.MP2
{
    public class BackendUrlAndTokenRetriever
    {
        public string BackendUrl { get; }
        public string Version { get; }

        public string PHPSESSID { get; private set; }

        public BackendUrlAndTokenRetriever(string backendUrl, string version)
        {
            BackendUrl = backendUrl;
            Version = version;
        }

        private static BackendUrlAndTokenRetriever CreateBackendConnectionFromEnvVars()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args == null)
                return null;

            var beUrl = string.Empty;
            var php = string.Empty;

            // Get backend url
            foreach (string arg in args)
            {
                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    var item = JsonConvert.DeserializeObject<BackendUrlAndTokenRetriever>(json);
                    beUrl = item.BackendUrl;
                }
                if (arg.Contains("-token="))
                {
                    php = arg.Replace("-token=", string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(php) && !string.IsNullOrEmpty(beUrl))
            {
                return new BackendUrlAndTokenRetriever(beUrl, php);
            }
            return null;
        }

        public static string GetUserId()
        {
            return CreateBackendConnectionFromEnvVars().PHPSESSID;
        }

        public static BackendUrlAndTokenRetriever GetBackendConnection()
        {
            return CreateBackendConnectionFromEnvVars();
        }

        public static string GetBackendUrlAsHttps()
        {
            var backendConnection = CreateBackendConnectionFromEnvVars();
            if (backendConnection != null)
            {
                if (!backendConnection.BackendUrl.Contains("https://"))
                    return $"https://{backendConnection.BackendUrl}";
                else
                    return backendConnection.BackendUrl;
            }
            return null;
        }

        public static string GetBackendUrlAsWS()
        {
            var backendConnection = CreateBackendConnectionFromEnvVars();
            if (backendConnection != null)
            {
                if (!backendConnection.BackendUrl.Contains("ws://"))
                    return $"ws://{backendConnection.BackendUrl}";
                else
                    return backendConnection.BackendUrl;
            }
            return null;
        }

    }
}
