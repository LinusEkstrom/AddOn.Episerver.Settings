using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EpiserverExtensions.Settings.Core;

namespace EpiserverExtensions.Settings.UI
{
    [ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
    public class SettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
    {
        public static string RepositoryKey
        { get { return "dynamiccontent"; } }//TODO: Change into static string.

        public override string Key
        {
            get { return RepositoryKey; }
        }

        public override string Name
        {
            get { return LocalizationService.Current.GetString("/contentrepositories/pages/name"); }//TODO: Change
        }

        public override IEnumerable<ContentReference> Roots
        {
            get { return new ContentReference[] { SettingsService.GlobalSettingsRoot, SettingsService.SettingsRoot }; }
        }

        public override string SearchArea
        {
            get { return "cms/pages"; }//TODO: Investigate or remove.
        }

        public override string CustomNavigationWidget
        {
            get { return "epi-cms/component/PageNavigationTree"; }
            //TODO: Can we use a generic content tree instead?
        }

        public override IEnumerable<Type> MainNavigationTypes
        {
            get { return new Type[] { typeof(ContentFolder) }; }
        }

        public override IEnumerable<Type> ContainedTypes
        {
            get { return new Type[] { typeof(ContentFolder), typeof(SettingsBase) }; }
        }

        public override IEnumerable<Type> CreatableTypes
        {
            get { return new Type[] { typeof(SettingsBase) }; }
        }

        public override IEnumerable<string> MainViews { get { return new[] { EPiServer.Cms.Shell.UI.CompositeViews.Internal.HomeView.ViewName }; } }

        public override int SortOrder
        {
            get { return 100; }
        }

        public override string CustomSelectTitle
        {
            get { return LocalizationService.Current.GetString("/contentrepositories/pages/customselecttitle"); }
        }
    }
}