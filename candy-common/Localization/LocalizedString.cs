using System.Collections.Generic;
using System.Linq;

namespace Candy.Common
{
    /// <summary>
    /// Represents a localized string with translations for different languages.
    /// </summary>
    public class LocalizedString : Dictionary<string, string>
    {
        /// <summary>
        /// Default language code to fall back to when a translation is missing.
        /// </summary>
        public const string DefaultLanguage = "en";

        /// <summary>
        /// Gets or sets the translation for the specified language code.
        /// If no translation exists for the requested language, returns a fallback text.
        /// Fallback order: requested language → default language → any available translation → "???".
        /// </summary>
        public new string this[string languageCode]
        {
            get => GetTranslation(languageCode);
            set => base[languageCode] = value;
        }

        /// <summary>
        /// Gets the translation for the specified language code with fallback logic.
        /// Fallback order: requested language → default language → any available translation → "???".
        /// </summary>
        /// <param name="languageCode">The language code to get translation for.</param>
        /// <returns>The translated text or a fallback.</returns>
        public string GetTranslation(string languageCode)
        {
            // Try requested language
            if (TryGetValue(languageCode, out var text))
                return text;

            // Try default language
            if (TryGetValue(DefaultLanguage, out var defaultText))
                return defaultText;

            // Try any language
            return Values.FirstOrDefault() ?? "???";
        }
        
        /// <summary>
        /// Gets the translation for the specified language code and formats it with the provided arguments.
        /// </summary>
        /// <param name="languageCode">The language code to get translation for.</param>
        /// <param name="args">Arguments to format the string with.</param>
        /// <returns>The formatted translated string.</returns>
        public string Format(string languageCode, params object[] args)
        {
            string translation = GetTranslation(languageCode);
            return string.Format(translation, args);
        }

        /// <summary>
        /// Checks if a translation exists for the specified language code.
        /// </summary>
        /// <param name="languageCode">The language code to check.</param>
        /// <returns>True if a translation exists for the specified language code.</returns>
        public bool HasTranslation(string languageCode)
        {
            return ContainsKey(languageCode);
        }
    }
} 