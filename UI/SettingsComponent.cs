using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace EpiserverExtensions.Settings.UI
{
    /// <summary>
    /// Component that provides a tree based navigation for CMS pages.
    /// </summary>
    [Component]
    public class SettingsComponent : ComponentDefinitionBase
    {
        public SettingsComponent()
            : base("epi-cms/widget/HierarchicalList")
        {
            //TODO: Add translations
            //LanguagePath = "/episerver/cms/components/pagetree";
            Title = "Settings";
            SortOrder = 200;
            PlugInAreas = new[] { PlugInArea.NavigationDefaultGroup };
            Settings.Add(new Setting("repositoryKey", SettingsRepositoryDescriptor.RepositoryKey));
        }
    }
}