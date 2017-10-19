using EPiServer.Shell.ViewComposition;

namespace EpiserverExtensions.Settings.UI
{
    /// <summary>
    /// Component that provides a tree based navigation for CMS pages.
    /// </summary>
    [Component]
    public class GlobalSettingsComponent : ComponentDefinitionBase
    {
        public GlobalSettingsComponent()
            : base("epi-cms.component.MainNavigationComponent")
        {
            //LanguagePath = "/episerver/cms/components/pagetree";
            SortOrder = 100;
            Settings.Add(new Setting("repositoryKey", GlobalSettingsRepositoryDescriptor.RepositoryKey));
        }
    }
}