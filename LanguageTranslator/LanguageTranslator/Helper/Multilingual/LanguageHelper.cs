//---------------------------------------------------------------------
// <copyright file="LanguageHelper.cs" owner="MobinAli">
// </copyright>
//---------------------------------------------------------------------

namespace LanguageTranslator.Helper.Multilingual
{
    using LanguageTranslator.Models;

    /// <summary>
    /// Helper class for <see cref="Language"/>.
    /// </summary>
    public static class LanguageHelper
    {
        /// <summary>
        /// Gets language name.
        /// </summary>
        /// <param name="language">Language.</param>
        /// <returns>Two letter ISO language name.</returns>
        public static string GetLanguageName(Language language)
        {
            switch (language)
            {
                case Language.Italian:
                    return "it";

                case Language.French:
                    return "fr";

                case Language.Spanish:
                    return "es";

                case Language.Portuguese:
                    return "pt";

                case Language.Chinese:
                    return "zh-Hans";

                case Language.Russian:
                    return "ru";

                case Language.English:
                default:
                    return "en";
            }
        }

        /// <summary>
        /// Gets language.
        /// </summary>
        /// <param name="languageName">Two letter ISO language name.</param>
        /// <returns>Language.</returns>
        public static Language GetLanguage(string languageName)
        {
            switch (languageName.Trim().ToLower())
            {
                // Italian
                case "it":
                // Italian - Italy
                case "it-it":
                // Italian - Switzerland
                case "it-ch":
                    return Language.Italian;

                // French
                case "fr":
                // French - Belgium
                case "fr-be":
                // French - Canada
                case "fr-ca":
                // French - France
                case "fr-fr":
                // French - Luxembourg
                case "fr-lu":
                // French - Monaco
                case "fr-mc":
                // French - Switzerland
                case "fr-ch":
                    return Language.French;

                // Spanish
                case "es":
                // Spanish - Argentina
                case "es-ar":
                // Spanish - Bolivia
                case "es-bo":
                // Spanish - Chile
                case "es-cl":
                // Spanish - Colombia
                case "es-co":
                // Spanish - Costa Rica
                case "es-cr":
                // Spanish - Dominican Republic
                case "es-do":
                // Spanish - Ecuador
                case "es-ec":
                // Spanish - El Salvador
                case "es-sv":
                // Spanish - Guatemala
                case "es-gt":
                // Spanish - Honduras
                case "es-hn":
                // Spanish - Mexico
                case "es-mx":
                // Spanish - Nicaragua
                case "es-ni":
                // Spanish - Panama
                case "es-pa":
                // Spanish - Paraguay
                case "es-py":
                // Spanish - Peru
                case "es-pe":
                // Spanish - Puerto Rico
                case "es-pr":
                // Spanish - Spain
                case "es-es":
                // Spanish - Uruguay
                case "es-uy":
                // Spanish - Venezuela
                case "es-ve":
                    return Language.Spanish;

                // Portuguese
                case "pt":
                // Portuguese - Brazil
                case "pt-br":
                // Portuguese - Portugal
                case "pt-pt":
                    return Language.Portuguese;

                // Chinese - Hong Kong SAR
                case "zh-hk":
                // Chinese - Macau SAR
                case "zh-mo":
                // Chinese - China
                case "zh-cn":
                // Chinese (Simplified)
                case "zh-hans":
                // Chinese - Singapore
                case "zh-sg":
                // Chinese - Taiwan
                case "zh-tw":
                // Chinese (Traditional)
                case "zh-hant":
                    return Language.Chinese;

                // Russian
                case "ru":
                // Russian - Russia
                case "ru-ru":
                    return Language.Russian;

                // English
                case "en":
                // English - Australia
                case "en-au":
                // English - Belize
                case "en-bz":
                // English - Canada
                case "en-ca":
                // English - Caribbean
                case "en-cb":
                // English - Ireland
                case "en-ie":
                // English - Jamaica
                case "en-jm":
                // English - New Zealand
                case "en-nz":
                // English - Philippines
                case "en-ph":
                // English - South Africa
                case "en-za":
                // English - Trinidad and Tobago
                case "en-it":
                // English - United Kingdom
                case "en-gb":
                // English - United States
                case "en-us":
                // English - Zimbabwe
                case "en-zw":
                default:
                    return Language.English;
            }
        }
    }
}