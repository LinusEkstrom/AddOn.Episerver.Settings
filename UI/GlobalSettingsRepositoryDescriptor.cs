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
    public class GlobalSettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
    {
        public static string RepositoryKey
        { get { return "globalsettings"; } }

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
            get { return new ContentReference[] { SettingsService.GlobalSettingsRoot }; }
        }

        public override string SearchArea
        {
            get { return string.Empty; }
        }

        public override string CustomNavigationWidget
        {
            get { return "epi-cms/component/ContentNavigationTree"; }
        }

        public override IEnumerable<Type> ContainedTypes
        {
            get { return new Type[] { typeof(SettingsBase) }; }
        }

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