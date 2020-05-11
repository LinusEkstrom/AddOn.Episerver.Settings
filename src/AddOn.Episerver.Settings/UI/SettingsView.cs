// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsView.cs" company="none">
//      Copyright © 2019 Linus Ekström, Jeroen Stemerdink.
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
    using System.Collections.Generic;

    using AddOn.Episerver.Settings.Core;

    using EPiServer.Core;
    using EPiServer.Framework.Localization;
    using EPiServer.Shell;
    using EPiServer.Shell.ViewComposition;
    using EPiServer.Shell.ViewComposition.Containers;
    using EPiServer.Shell.Web;
    using EPiServer.Web.Routing;

    /// <summary>
    /// Class SettingsView.
    /// Implements the <see cref="ICompositeView" />
    /// Implements the <see cref="IRoutable" />
    /// Implements the <see cref="ICustomGlobalNavigationMenuBehavior" />
    /// Implements the <see cref="IRestrictedComponentCategoryDefinition" />
    /// </summary>
    /// <seealso cref="ICompositeView" />
    /// <seealso cref="IRoutable" />
    /// <seealso cref="ICustomGlobalNavigationMenuBehavior" />
    /// <seealso cref="IRestrictedComponentCategoryDefinition" />
    [CompositeView]
    public class SettingsView : ICompositeView,
                                IRoutable,
                                ICustomGlobalNavigationMenuBehavior,
                                IRestrictedComponentCategoryDefinition
    {
        /// <summary>
        /// The view name
        /// </summary>
        public static readonly string ViewName = "/episerver/cms/settings";

        /// <summary>
        /// The localization service
        /// </summary>
        private readonly LocalizationService localizationService;

        /// <summary>
        /// The settings service
        /// </summary>
        private readonly ISettingsService settingsService;

        /// <summary>
        /// The root container
        /// </summary>
        private IContainer rootContainer;

        /// <summary>
        /// The route segment
        /// </summary>
        private string routeSegment;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView" /> class.
        /// </summary>
        /// <param name="localizationService">The localization service used for getting localized resources.</param>
        /// <param name="settingsService">The settings service.</param>
        public SettingsView(LocalizationService localizationService, ISettingsService settingsService)
        {
            this.localizationService = localizationService;
            this.settingsService = settingsService;
        }

        /// <summary>
        /// Gets the default context. 
        /// </summary>
        /// <value>Returns a context reference to the start page, if defined, otherwise a context reference to the root page is returned.</value>
        public string DefaultContext
        {
            get
            {
                ContentReference defaultContext = this.settingsService.GlobalSettingsRoot;
                return defaultContext.GetUri(false).ToString();
            }
        }

        /// <summary>
        /// Gets the type of the menu.
        /// </summary>
        /// <value>The type of the menu.</value>
        public GlobalNavigationMenuType MenuType
        {
            get
            {
                return GlobalNavigationMenuType.Static;
            }
        }

        /// <summary>
        /// Gets the name of the view. Used or finding views.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return ViewName;
            }
        }

        /// <summary>
        /// Gets the root <see cref="IContainer" /> that contains the different panels for the view.
        /// </summary>
        /// <value>The container.</value>
        public IContainer RootContainer
        {
            get
            {
                if (this.rootContainer != null)
                {
                    return this.rootContainer;
                }

                IContainer navigation = new PinnablePane().Add(
                    new ComponentPaneContainer { ContainerType = ContainerType.System }.Add(
                        new GlobalSettingsComponent().CreateComponent()));

                IContainer<BorderSettingsDictionary> content = new BorderContainer()
                    .Add(
                        new ContentPane { PlugInArea = "/episerver/cms/action" },
                        new BorderSettingsDictionary(region: BorderContainerRegion.Top)).Add(
                        new ContentPane { PlugInArea = "/episerver/cms/maincontent" },
                        new BorderSettingsDictionary(region: BorderContainerRegion.Center));

                this.rootContainer = new BorderContainer().Add(
                    component: navigation,
                    new BorderSettingsDictionary(
                        region: BorderContainerRegion.Leading,
                        new Setting("minSize", 305),
                        new Setting("splitter", "true"),
                        new Setting("liveSplitters", "false"),
                        new Setting("id", "navigation"))).Add(
                    component: content,
                    new BorderSettingsDictionary(region: BorderContainerRegion.Center));

                this.rootContainer.Settings["id"] = this.Name + "_rootContainer";
                return this.rootContainer;
            }
        }

        /// <summary>
        /// Gets or sets the Route segment.
        /// </summary>
        /// <value>The Route segment.</value>
        public string RouteSegment
        {
            get
            {
                return this.routeSegment ?? (this.routeSegment = "settings");
            }

            set
            {
                this.routeSegment = value;
            }
        }

        /// <summary>
        /// Gets a localized title for this view.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get
            {
                return "Settings";
            }
        }

        /// <summary>
        /// Creates a new instance of the view.
        /// </summary>
        /// <returns>A new instance of the view.</returns>
        public ICompositeView CreateView()
        {
            return new SettingsView(
                localizationService: this.localizationService,
                settingsService: this.settingsService);
        }

        /// <summary>
        /// Gets the component categories.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of categories.</returns>
        public IEnumerable<string> GetComponentCategories()
        {
            return new string[] { };
        }
    }
}