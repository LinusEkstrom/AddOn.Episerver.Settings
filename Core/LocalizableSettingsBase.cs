using System.Collections.Generic;
using EPiServer.Core;

namespace EpiserverExtensions.Settings.Core
{
    public class LocalizableSettingsBase : SettingsBase, ILocalizable
    {
        public IEnumerable<System.Globalization.CultureInfo> ExistingLanguages { get; set; }

        public System.Globalization.CultureInfo MasterLanguage { get; set; }

        public System.Globalization.CultureInfo Language { get; set; }
    }
}