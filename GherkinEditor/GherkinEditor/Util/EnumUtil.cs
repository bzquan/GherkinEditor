using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Reflection;

namespace Gherkin.Util
{
    public static class EnumUtil
    {
        public static Languages CurrentLanguage { get; set; } = Languages.English;

        /// <summary>
        /// Get description text of an enum value.
        /// Sample definition of an enum type
        /// public enum EnumType
        /// {
        ///    [Description("This is a Feature")]
        ///    Feature,
        ///    
        ///    When supporting multi languages to use Language Description to seperate different languages
        ///    [Description("English:This is a Feature|日本語:これは機能です|中文:这是功能")]
        ///    Feature,
        /// }
        /// public enum Languages
        /// {
        ///    [Description("English")]
        ///    English,
        ///    [Description("日本語")]
        ///    Japanese,
        ///    [Description("中文")]
        ///    Chinese,
        /// }
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return LanguageSpecificDescription(attributes[0].Description);
            }
            else
                return value.ToString();
        }

        private static string LanguageSpecificDescription(string description)
        {
            var desciptions = description.Split('|');
            if (desciptions.Length > 1)
            {
                string currentLang = GetEnumDescription(CurrentLanguage) + ":";
                foreach (var desc in desciptions)
                {
                    string desc_trim = desc.Trim();
                    if (desc_trim.StartsWith(currentLang, StringComparison.Ordinal))
                        return desc_trim.Substring(currentLang.Length);
                }
            }

            return description;
        }

        /// <summary>
        /// Convert description of enum to enum value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decriptionOfEnumValue"></param>
        /// <returns></returns>
        public static T ToEnumValue<T>(string decriptionOfEnumValue)
        {
            IEnumerable<T> enums = EnumToList<T>();
            foreach (T value in enums)
            {
                if (GetEnumDescription(value as Enum) == decriptionOfEnumValue)
                {
                    return value;
                }
            }
            return enums.First();
        }

        /// <summary>
        /// Enumerate all the values of a given enum.
        /// This will allow you to easily create a drop down list based on an enum. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> EnumToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Add enum description strings to a comboBox
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        public static void InitComboBoxByEnum<T>(ComboBox comboBox)
        {
            IEnumerable<T> enums = EnumToList<T>();
            foreach (T value in enums)
            {
                comboBox.Items.Add(GetEnumDescription(value as Enum));
            }
        }
    }
}
