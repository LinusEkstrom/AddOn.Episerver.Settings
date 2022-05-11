using EPiServer.Shell.ViewComposition;

namespace AddOn.Episerver.Settings.UI
{
    internal class GlobalSettingsVersionsComponent : ComponentDefinitionBase
    {
        public GlobalSettingsVersionsComponent() : base("epi-cms/component/VersionsComponent")
        {
            LanguagePath = "/episerver/cms/components/versions";
            Title = "Versions";
            SortOrder = 100;
        }
    }
}