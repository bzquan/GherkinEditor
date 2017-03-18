using System;
using System.Collections.Generic;
using System.Configuration;

namespace Gherkin.Util
{
    public static class ConfigReader
    {
        public static T GetValue<T>(string key, T defaultValue)
        {
            try
            {
                return (T)s_Reader.GetValue(key, typeof(T));
            }
            catch (InvalidOperationException)
            {
                return defaultValue;
            }
        }

        public static List<string> GetLisValue(string key, string defaultValue)
        {
            string value = GetValue(key, defaultValue);
            List<string> values = new List<string>(value.Split(new char[] { '|' }));
            List<string> values_trimmed = new List<string>();
            foreach (string v in values)
            {
                values_trimmed.Add(v.Trim());
            }
            return values_trimmed;
        }

        static AppSettingsReader s_Reader = new AppSettingsReader();
    }
}
