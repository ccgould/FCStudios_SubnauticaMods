using System.Collections;
using System.Collections.Generic;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.DataCollectors
{
    internal static class PatreonCollector
    {
        public const string accessToken = "a-MV8cgqaGxL7StA8jmRRCHtc1TrjqnKmw9BkPIZ_yY";
        private static List<string> users = new List<string>();
        [SerializeField] private static bool _isRunning;
        private static bool _dataHasBeenCollected;

        public static void GetData()
        {
            if (_isRunning || _dataHasBeenCollected) return;
            users.Clear();
            CoroutineHost.StartCoroutine( GetDataAsync("https://www.patreon.com/api/oauth2/api/campaigns/5265416/pledges?include=patron.null"));
        }

        private static IEnumerator GetDataAsync(string request)
        {
            _isRunning = true;

            var headers = new Dictionary<string, string>
            {
                {"authorization", "Bearer " + accessToken}
            };

            //QuickLogger.Debug("1");

            WWW patreonRequest = new WWW(request, null, headers);

            while (!patreonRequest.isDone)
            {
                yield return null;
            }

            //QuickLogger.Debug("2");

            if(string.IsNullOrWhiteSpace(patreonRequest.text)) yield break;
            //QuickLogger.Debug("3");
            PatreonObj account = JsonConvert.DeserializeObject<PatreonObj>(patreonRequest.text);
            //QuickLogger.Debug("4");
            if (account == null || account.included == null)
            {
                _isRunning = false;
                PrintResults();
                yield break;
            }
            //QuickLogger.Debug("5");
            foreach (Patron patron in account.included)
            {
                if (patron != null && !string.IsNullOrWhiteSpace(patron.attributes.full_name))
                {
                    users.Add(patron.attributes.full_name);
                }
            }
            //QuickLogger.Debug("6");
            if (account.links.ContainsKey("next"))
            {
                if (!string.IsNullOrWhiteSpace(account.links["next"]))
                {
                    yield return new WaitForSeconds(1.0f);
                    QuickLogger.Debug("Attempting to add more names");
                    yield return GetDataAsync(account.links["next"]);
                }
            }
            else
            {
                _isRunning = false;
                _dataHasBeenCollected = true;
                PrintResults();
            }

            QuickLogger.Debug("---- End Data Collection --- ");
            yield break;
        }

        private static void PrintResults()
        {
            QuickLogger.Debug("------------------------------------");
            foreach (var user in users)
            {
                QuickLogger.Debug(user);
            }
            QuickLogger.Debug("------------------------------------");
        }

        public static string GetPatreon(int index)
        {
            return users[index];
        }

        public static bool GetIsRunning()
        {
            return _isRunning;
        }

        public static int GetTotalPatrons()
        {
            return users.Count;
        }
        private class PatreonObj
        {
            public Dictionary<string, string> links { get; set; }
            public List<Patron> included = new List<Patron>();
        }

        private class Patron
        {
            public PatronInfo attributes { get; set; }
        }

        private class PatronInfo
        {
            public string full_name { get; set; }
        }
    }
}
