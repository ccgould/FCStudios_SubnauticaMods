using System;
using System.Collections.Generic;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;

namespace ExStorageDepot.Configuration
{
    public class Config
    {
        [JsonProperty] internal Dictionary<string, string> Configuration { get; set; }

        public string GetValue(string key)
        {
            try
            {
                foreach (KeyValuePair<string, string> valuePair in Configuration)
                {
                    if (valuePair.Key == key)
                    {
                        return valuePair.Value;
                    }
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error<Config>(e.Message);
            }
            return String.Empty;
        }
    }
}