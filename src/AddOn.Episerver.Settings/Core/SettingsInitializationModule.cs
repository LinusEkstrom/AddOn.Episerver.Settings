// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsInitializationModule.cs" company="none">
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

using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Configuration;
using EPiServer.Shell.Modules;
using EPiServer.Web;
using System;
using System.Linq;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace AddOn.Episerver.Settings.Core;
#if NET48
#else
    using Microsoft.Extensions.DependencyInjection;
#endif

/// <summary>
///     Class SettingsInit.
/// </summary>
/// <seealso cref="IInitializableModule" />
/// <author>Jeroen Stemerdink</author>
[InitializableModule]
[ModuleDependency(typeof(InitializationModule))]
public class SettingsInitializationModule : IConfigurableModule
{
    private static IContentEvents contentEvents;

    private static ISiteDefinitionEvents siteDefinitionEvents;

    private static bool initialized;

    private static LocalizationService localizationService;

    private static ISettingsService settingsService;

    /// <summary>Configure the IoC container before initialization.</summary>
    /// <param name="context">The context on which the container can be accessed.</param>
    public void ConfigureContainer(ServiceConfigurationContext context)
    {
        if (context == null)
        {
            return;
        }

#if NET48
        var services = context.Services;
        services.AddSingleton<ISettingsService, SettingsService>();
#else
            context.Services.AddSingleton<ISettingsService, SettingsService>();
            context.Services.Configure<ProtectedModuleOptions>(pm => pm.Items.Add(new ModuleDetails() { Name = "AddOn.Episerver.Settings" }));
#endif
    }

    /// <summary>
    ///     Initializes this instance.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <remarks>
    ///     Gets called as part of the EPiServer Framework initialization sequence. Note that it will be called
    ///     only once per AppDomain, unless the method throws an exception. If an exception is thrown, the initialization
    ///     method will be called repeatedly for each request reaching the site until the method succeeds.
    /// </remarks>
    /// <exception cref="T:EPiServer.ServiceLocation.ActivationException">
    ///     if there is are errors resolving the service
    ///     instance.
    /// </exception>
    public void Initialize(InitializationEngine context)
    {
        if (context == null || context.HostType != HostType.WebApplication)
        {
            return;
        }

        if (initialized)
        {
            return;
        }

        context.InitComplete += (sender, args) =>
        {
            var moduleTable = context.Locate.Advanced.GetInstance<ModuleTable>();
            var modules = moduleTable.GetModules().ToList();
            var settings = modules.FirstOrDefault(m => string.Equals(m.Name, "AddOn.Episerver.Settings", StringComparison.OrdinalIgnoreCase));
            var commerce = modules.FirstOrDefault(m => string.Equals(m.Name, "EPiServer.Commerce.Shell", StringComparison.OrdinalIgnoreCase));

            if (settings != null && commerce != null)
            {
                settings.Manifest.ClientModule.ModuleDependencies.Add(new ModuleDependency
                    { Dependency = "EPiServer.Commerce.Shell", DependencyType = ModuleDependencyTypes.Require | ModuleDependencyTypes.RunAfter });
            }
        };

        settingsService = context.Locate.Advanced.GetInstance<ISettingsService>();
        contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
        siteDefinitionEvents = context.Locate.Advanced.GetInstance<ISiteDefinitionEvents>();
        localizationService = context.Locate.Advanced.GetInstance<LocalizationService>();

        context.InitComplete += InitCompleteHandler;

        contentEvents.PublishedContent += PublishedContent;
        contentEvents.MovingContent += MovingContent;
        contentEvents.DeletingContent += DeletingContent;

        siteDefinitionEvents.SiteCreated += SiteChanged;
        siteDefinitionEvents.SiteUpdated += SiteChanged;

        initialized = true;
    }

    /// <summary>
    ///     Resets the module into an uninitialized state.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <remarks>
    ///     <para>
    ///         This method is usually not called when running under a web application since the web app may be shut down very
    ///         abruptly, but your module should still implement it properly since it will make integration and unit testing
    ///         much simpler.
    ///     </para>
    ///     <para>
    ///         Any work done by
    ///         <see
    ///             cref="M:EPiServer.Framework.IInitializableModule.Initialize(EPiServer.Framework.Initialization.InitializationEngine)" />
    ///         as well as any code executing on
    ///         <see cref="E:EPiServer.Framework.Initialization.InitializationEngine.InitComplete" /> should be reversed.
    ///     </para>
    /// </remarks>
    public void Uninitialize(InitializationEngine context)
    {
        if (context == null)
        {
            return;
        }

        if (!initialized)
        {
            return;
        }

        contentEvents.PublishedContent -= PublishedContent;
        contentEvents.MovingContent -= MovingContent;
        contentEvents.DeletingContent -= DeletingContent;

        siteDefinitionEvents.SiteCreated -= SiteChanged;
        siteDefinitionEvents.SiteUpdated -= SiteChanged;

        initialized = false;
    }

    /// <summary>
    ///     Executed when the content is being deleted.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="DeleteContentEventArgs" /> instance containing the event data.</param>
    private static void DeletingContent(object sender, DeleteContentEventArgs e)
    {
        if (e == null || e.Content == null)
        {
            return;
        }

        // if the content item is an instance of SettingsBase, it has been instantiated with the fallback base class
        // since the ContentType class no longer exists and should be possible to delete
        if (e.Content.GetOriginalType() == typeof(SettingsBase) || !(e.Content is SettingsBase) || e.Content.ParentLink != settingsService.GlobalSettingsRoot)
        {
            return;
        }

        e.CancelAction = true;
        e.CancelReason = localizationService.GetString("/edit/deletesetting/deletenotsupported");
    }

    /// <summary>
    ///     Initializes the complete handler.
    /// </summary>
    /// <param name="sender"> The sender. </param>
    /// <param name="e"> The <see cref="EventArgs" /> instance containing the event data. </param>
    /// <exception cref="T:System.NotSupportedException">If the rootname is already registered with another contentRootId.</exception>
    private static void InitCompleteHandler(object sender, EventArgs e)
    {
        settingsService.InitSettings();
    }

    /// <summary>
    ///     Executed when the content is being moved.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ContentEventArgs" /> instance containing the event data.</param>
    private static void MovingContent(object sender, ContentEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        // if the content item is an instance of SettingsBase, it has been instantiated with the fallback base class
        // since the ContentType class no longer exists and should be possible to move to the waste basket
        if (e.Content.GetOriginalType() == typeof(SettingsBase) ||
            !(e.Content is SettingsBase) ||
            e.Content.ParentLink != settingsService.GlobalSettingsRoot ||
            e.TargetLink.ID != 2)
        {
            return;
        }

        e.CancelAction = true;
        e.CancelReason = localizationService.GetString("/edit/deletesetting/deletenotsupported");
    }

    /// <summary>
    ///     Executed when the content is published.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ContentEventArgs" /> instance containing the event data.</param>
    private static void PublishedContent(object sender, ContentEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        settingsService.UpdateSettings(e.Content);
    }

    public static void SiteChanged(object sender, SiteDefinitionEventArgs e)
    {
        if (e.Site == null)
        {
            return;
        }

        settingsService.ValidateOrCreateSiteSettingsRoot(e.Site);
    }
}
