
namespace LanguageTranslator.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enum representing language.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Language
    {
        /// <summary>
        /// English.
        /// </summary>
        English,

        /// <summary>
        /// Italian.
        /// </summary>
        Italian,

        /// <summary>
        /// French.
        /// </summary>
        French,

        /// <summary>
        /// Spanish.
        /// </summary>
        Spanish,

        /// <summary>
        /// Portuguese.
        /// </summary>
        Portuguese,

        /// <summary>
        /// Chinese.
        /// </summary>
        Chinese,

        /// <summary>
        /// Russian
        /// </summary>
        Russian
    }
}
