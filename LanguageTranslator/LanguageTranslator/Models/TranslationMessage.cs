namespace LanguageTranslator.Models
{
    using LanguageTranslator.Helper.Multilingual;
    using System.Collections.Generic;
 
    /// <summary>
    /// Represents the request and response message of the Translation Service.
    /// </summary>
    public class TranslationMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMessage"/> class.
        /// </summary>
        public TranslationMessage()
        {
            this.To = LanguageHelper.GetLanguageName(Language.English);
            this.Translations = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the language to translate from.
        /// </summary>
        public string From
        {
            get
            {
                return LanguageHelper.GetLanguageName(Language.English);
            }
        }

        /// <summary>
        /// Gets or sets the language to translate to.
        /// </summary>
        public string To
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of key value pairs where key represents text to translate and value represents its translation.
        /// </summary>
        public Dictionary<string, string> Translations
        {
            get;
            set;
        }
    }
}
