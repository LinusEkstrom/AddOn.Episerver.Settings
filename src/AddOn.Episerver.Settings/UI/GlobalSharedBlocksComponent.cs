using EPiServer.Cms.Shell.UI.UIDescriptors;
using EPiServer.Shell.ViewComposition;

namespace AddOn.Episerver.Settings.UI
{
    internal class GlobalSharedBlocksComponent : ComponentDefinitionBase
    {
        public GlobalSharedBlocksComponent()
            : base("epi-cms/component/SharedBlocks")
        {
            LanguagePath = "/episerver/cms/components/sharedblocks";
            Title = "Blocks";
            SortOrder = 90;
            Settings.Add(new Setting("repositoryKey", BlockRepositoryDescriptor.RepositoryKey));
        }
    }
}