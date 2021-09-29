// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsSearchProvider.cs" company="none">
//      Copyright © 2020 Linus Ekström, Jeroen Stemerdink.
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//      SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AddOn.Episerver.Settings.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AddOn.Episerver.Settings.Core;

    using EPiServer;
    using EPiServer.Cms.Shell.Search;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.Framework.Localization;
    using EPiServer.Globalization;
    using EPiServer.ServiceLocation;
    using EPiServer.Shell;
    using EPiServer.Shell.Search;
    using EPiServer.Web;
    using EPiServer.Web.Routing;

    /// <summary>
    /// Class SettingsSearchProvider.
    /// </summary>
    [SearchProvider]
    public class SettingsSearchProvider : ContentSearchProviderBase<SettingsBase, ContentType>
    {
        internal const string SearchArea = "Settings/settings";

        private readonly IContentLoader contentLoader;

        private readonly LocalizationService localizationService;

        private readonly ISettingsService settingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsSearchProvider" /> class.
        /// </summary>
        /// <param name="localizationService">The localization service.</param>
        /// <param name="siteDefinitionResolver">The site definition resolver.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="editUrlResolver">The edit URL resolver.</param>
        /// <param name="currentSiteDefinition">The current site definition.</param>
        /// <param name="languageResolver">The language resolver.</param>
        /// <param name="urlResolver">The URL resolver.</param>
        /// <param name="templateResolver">The template resolver.</param>
        /// <param name="uiDescriptorRegistry">The UI descriptor registry.</param>
        /// <param name="contentLoader">The content loader.</param>
        /// <param name="settingsService">The settings service.</param>
        public SettingsSearchProvider(
            LocalizationService localizationService,
            ISiteDefinitionResolver siteDefinitionResolver,
            IContentTypeRepository<ContentType> contentTypeRepository,
            EditUrlResolver editUrlResolver,
            ServiceAccessor<SiteDefinition> currentSiteDefinition,
#if NET461
            LanguageResolver languageResolver,
#else
            IContentLanguageAccessor languageResolver,
#endif
            UrlResolver urlResolver,
            TemplateResolver templateResolver,
            UIDescriptorRegistry uiDescriptorRegistry,
            IContentLoader contentLoader,
            ISettingsService settingsService)
            : base(
                localizationService: localizationService,
                siteDefinitionResolver: siteDefinitionResolver,
                contentTypeRepository: contentTypeRepository,
                editUrlResolver: editUrlResolver,
                currentSiteDefinition: currentSiteDefinition,
                languageResolver: languageResolver,
                urlResolver: urlResolver,
                templateResolver: templateResolver,
                uiDescriptorRegistry: uiDescriptorRegistry)
        {
            this.contentLoader = contentLoader;
            this.settingsService = settingsService;
            this.localizationService = localizationService;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <value>The area.</value>
        public override string Area => SearchArea;

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        public override string Category => this.localizationService.GetString("/episerver/cms/components/settings/title");

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>The icon CSS class.</value>
        protected override string IconCssClass => "epi-iconSettings";

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="SearchResult"/>.</returns>
        public override IEnumerable<SearchResult> Search(Query query)
        {
            if (string.IsNullOrWhiteSpace(value: query?.SearchQuery) || query.SearchQuery.Trim().Length < 2)
            {
                return Enumerable.Empty<SearchResult>();
            }

            List<SearchResult> searchResultList = new List<SearchResult>();
            string str = query.SearchQuery.Trim();

            IEnumerable<ContentReference> settings =
                this.contentLoader.GetDescendents(contentLink: this.settingsService.SettingsRoot);

            foreach (ContentReference contentReference in settings)
            {
                SettingsBase setting;

                if (!this.contentLoader.TryGet(contentLink: contentReference, content: out setting))
                {
                    continue;
                }

                if (setting.Name.IndexOf(value: str, comparisonType: StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                searchResultList.Add(this.CreateSearchResult(contentData: setting));

                if (searchResultList.Count == query.MaxResults)
                {
                    break;
                }
            }

            return searchResultList;
        }

        /// <summary>
        /// Creates the preview text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The preview text.</returns>
        protected override string CreatePreviewText(IContentData content)
        {
            return content == null
                       ? string.Empty
                       : $"{((SettingsBase)content).Name} {this.localizationService.GetString("/contentrepositories/settings/customselecttitle").ToLower()}";
        }
    }
}