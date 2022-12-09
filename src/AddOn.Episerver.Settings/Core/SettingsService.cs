// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsService.cs" company="none">
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
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Framework.Cache;
using EPiServer.Framework.TypeScanner;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Class SettingsService.
///     Implements the <see cref="ISettingsService" />
/// </summary>
/// <seealso cref="ISettingsService" />
public class SettingsService : ISettingsService
{
    /// <summary>
    ///     The global settings root name
    /// </summary>
    public const string GlobalSettingsRootName = "Global Settings Root";

    /// <summary>
    ///     The settings root name
    /// </summary>
    public const string SettingsRootName = "Settings Root";

    /// <summary>
    ///     The site settings root name
    /// </summary>
    public const string SiteSettingsRootName = "Site Settings Root";

    /// <summary>
    ///     The ancestor references loader
    /// </summary>
    private readonly AncestorReferencesLoader ancestorReferencesLoader;

    /// <summary>
    ///     The synchronized object instance cache
    /// </summary>
    private readonly ISynchronizedObjectInstanceCache cache;

    /// <summary>
    ///     The content repository
    /// </summary>
    private readonly IContentRepository contentRepository;

    /// <summary>
    ///     The content root service
    /// </summary>
    private readonly ContentRootService contentRootService;

    /// <summary>
    ///     The content type repository
    /// </summary>
    private readonly IContentTypeRepository contentTypeRepository;

    /// <summary>
    ///     The key used for storing global settings in the synchronized cache
    /// </summary>
    private readonly string globalSettingsCacheKey = "addon.episerver.settings.globalsettings";

    /// <summary>
    ///     The logger instance
    /// </summary>
    private readonly ILogger log = LogManager.GetLogger();

    /// <summary>
    ///     The site definition repository
    /// </summary>
    private readonly ISiteDefinitionRepository siteDefinitionRepository;

    private readonly ConcurrentDictionary<Guid, ContentReference> siteSettingsRoots = new ConcurrentDictionary<Guid, ContentReference>();

    /// <summary>
    ///     The type scanner lookup
    /// </summary>
    private readonly ITypeScannerLookup typeScannerLookup;

        private readonly ISettingsResolver[] settingsResolvers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsService" /> class.
    /// </summary>
    /// <param name="contentRepository">The content repository.</param>
    /// <param name="siteDefinitionRepository">The site definition reposi</param>
    /// <param name="contentRootService">The content root service.</param>
    /// <param name="typeScannerLookup">The type scanner lookup.</param>
    /// <param name="contentTypeRepository">The content type repository.</param>
    /// <param name="ancestorReferencesLoader">The ancestor references loader.</param>
    /// <param name="synchronizedObjectInstanceCache"></param>
        /// <param name="settingsResolvers"></param>
    public SettingsService(
        IContentRepository contentRepository,
        ISiteDefinitionRepository siteDefinitionRepository,
        ContentRootService contentRootService,
        ITypeScannerLookup typeScannerLookup,
        IContentTypeRepository contentTypeRepository,
        AncestorReferencesLoader ancestorReferencesLoader,
        ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache, 
        IEnumerable<ISettingsResolver> settingsResolvers)
    {
        this.contentRepository = contentRepository;
        this.siteDefinitionRepository = siteDefinitionRepository;
        this.contentRootService = contentRootService;
        this.typeScannerLookup = typeScannerLookup;
        this.contentTypeRepository = contentTypeRepository;
        this.ancestorReferencesLoader = ancestorReferencesLoader;
        cache = synchronizedObjectInstanceCache;
        this.settingsResolvers = settingsResolvers.OrderBy(x => x.SortOrder).ToArray();
    }

    /// <summary>
    ///     Gets the global settings root unique identifier
    /// </summary>
    private Guid GlobalSettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61a");

    /// <summary>
    ///     Gets the settings root unique identifier
    /// </summary>
    private Guid SettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61c");

    /// <summary>
    ///     Gets the global settings.
    /// </summary>
    /// <value>The global settings.</value>
    public Dictionary<Type, ContentReference> GlobalSettings => GetGlobalSettings();

    /// <summary>
    ///     Gets or sets the global settings root.
    /// </summary>
    /// <value>The global settings root.</value>
    public ContentReference GlobalSettingsRoot { get; set; }

    /// <summary>
    ///     Gets or sets the settings root.
    /// </summary>
    /// <value>The settings root.</value>
    public ContentReference SettingsRoot { get; set; }

    /// <summary>
    ///     Gets the settings roots. In case site-specific assets are enabled this will contain
    ///     both the shared and site specific roots, otherwise only the shared root.
    /// </summary>
    /// <value>The settings root.</value>
    public IEnumerable<ContentReference> SettingsRoots
    {
        get
        {
            var settings = new List<ContentReference> { SettingsRoot };
            if (!SiteDefinition.Current.GlobalAssetsRoot.CompareToIgnoreWorkID(SiteDefinition.Current.SiteAssetsRoot))
            {
                if (siteSettingsRoots.TryGetValue(SiteDefinition.Current.Id, out var settingsRef))
                {
                    settings.Add(settingsRef);
                }
            }

            return settings;
        }
    }

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <returns>An instance of <typeparamref name="T" /> </returns>
    public T GetSettings<T>() where T : SettingsBase
    {
        try
        {
            var type = typeof(T);
            var globalSettings = GetGlobalSettings();

            if (globalSettings.ContainsKey(type))
            {
                return contentRepository.Get<T>(globalSettings[type]);
            }
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            log.Error($"[Settings] {keyNotFoundException.Message}", keyNotFoundException);
        }
        catch (ArgumentNullException argumentNullException)
        {
            log.Error($"[Settings] {argumentNullException.Message}", argumentNullException);
        }

        return default;
    }

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>An instance of <typeparamref name="T" /> </returns>
    /// <exception cref="T:System.Threading.SynchronizationLockException">
    ///     The current thread has not entered the lock in read
    ///     mode.
    /// </exception>
    /// <exception cref="T:System.Threading.LockRecursionException">
    ///     The current thread cannot acquire the write lock when it
    ///     holds the read lock.-or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is
    ///     <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire
    ///     the read lock when it already holds the read lock. -or-The
    ///     <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is
    ///     <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire
    ///     the read lock when it already holds the write lock. -or-The recursion number would exceed the capacity of the
    ///     counter. This limit is so large that applications should never encounter this exception.
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    ///     The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object
    ///     has been disposed.
    /// </exception>
    public T GetSettings<T>(IContent content)
            where T : SettingsBase
    {
        if (content == null)
        {
            return default;
        }

        var settingsFromContent = TryGetSettingsFromContent<T>(content);

        if (settingsFromContent != null)
        {
            return settingsFromContent;
        }

        var ancestors =
            ancestorReferencesLoader.GetAncestors(content.ContentLink);

        foreach (var parentReference in ancestors)
        {
            IContent parentContent;

            if (!contentRepository.TryGet(parentReference, out parentContent))
            {
                continue;
            }

            settingsFromContent = TryGetSettingsFromContent<T>(parentContent);

            if (settingsFromContent != null)
            {
                return settingsFromContent;
            }
        }

        return GetSettings<T>();
    }

    /// <summary>
    ///     Initializes the settings.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException">If the rootname is already registered with another contentRootId.</exception>
    public void InitSettings()
    {
        try
        {
            contentRootService.Register<ContentFolder>(
            GlobalSettingsRootName,
            GlobalSettingsRootGuid,
            SiteDefinition.Current.GlobalAssetsRoot);

            GlobalSettingsRoot = contentRootService.Get(GlobalSettingsRootName);
        }
        catch (NotSupportedException notSupportedException)
        {
            log.Error($"[Settings] {notSupportedException.Message}", notSupportedException);
            throw;
        }

        try
        {
            contentRootService.Register<ContentFolder>(
            SettingsRootName,
            SettingsRootGuid,
            SiteDefinition.Current.SiteAssetsRoot);

            SettingsRoot = contentRootService.Get(SettingsRootName);
        }
        catch (NotSupportedException notSupportedException)
        {
            log.Error($"[Settings] {notSupportedException.Message}", notSupportedException);
            throw;
        }

        foreach (var siteDefinition in siteDefinitionRepository.List())
        {
            ValidateOrCreateSiteSettingsRoot(siteDefinition);
        }

        InitializeContentInstances();
    }

    /// <summary>
    ///     Updates the settings.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <exception cref="T:System.Threading.LockRecursionException">
    ///     The
    ///     <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is
    ///     <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the
    ///     lock in any mode. -or-The current thread has entered read mode, so trying to enter the lock in write mode would
    ///     create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit
    ///     is so large that applications should never encounter it.
    /// </exception>
    /// <exception cref="T:System.ObjectDisposedException">
    ///     The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object
    ///     has been disposed.
    /// </exception>
    /// <exception cref="T:System.Threading.SynchronizationLockException">
    ///     The current thread has not entered the lock in write
    ///     mode.
    /// </exception>
    public void UpdateSettings(IContent content)
    {
        try
        {
            if (content is SettingsBase && content.ParentLink == GlobalSettingsRoot)
            {
                ClearCache();
            }
        }
        catch (EPiServerException ePiServerException)
        {
            log.Error($"[Settings] {ePiServerException.Message}", ePiServerException);
        }
    }

    /// <summary>
    ///     Creates a settings folder for a site and adds it to the settings lookup
    /// </summary>
    public ContentReference ValidateOrCreateSiteSettingsRoot(SiteDefinition siteDefinition)
    {
        if (siteDefinition.SiteAssetsRoot.CompareToIgnoreWorkID(siteDefinition.GlobalAssetsRoot))
        {
            return ContentReference.EmptyReference;
        }

        try
        {
            return siteSettingsRoots.GetOrAdd(siteDefinition.Id, id =>
            {
                var contentRootId = DeterministicGuid.Create(siteDefinition.Id, SettingsRootName);
                if (!contentRepository.TryGet<ContentFolder>(contentRootId, out var root))
                {
                    root = contentRepository.GetDefault<ContentFolder>(siteDefinition.SiteAssetsRoot);
                    root.ContentGuid = contentRootId;
                    root.Name = SiteSettingsRootName;

                    contentRepository.Save(root, SaveAction.Publish | SaveAction.SkipValidation, AccessLevel.NoAccess);
                }

                return root.ContentLink;
            });
        }
        catch (NotSupportedException notSupportedException)
        {
            log.Error($"[Settings] {notSupportedException.Message}", notSupportedException);
            throw;
        }
    }

    /// <summary>
    ///     Initializes the content instances.
    /// </summary>
    /// <exception cref="T:EPiServer.Core.EPiServerException">
    ///     Could not create instance of a content type since it has an
    ///     invalid .NET class associated. This happens if a class have been deleted, try remove the content type in Admin or
    ///     from database."
    /// </exception>
    private void InitializeContentInstances()
    {
        var existingItems = new List<IContent>();
        var type = typeof(SettingsContentTypeAttribute);
        var settingsModelTypes = typeScannerLookup.AllTypes.Where(
        t => t.GetCustomAttributes(type, false).Length > 0);

        try
        {
            existingItems = LoadGlobalSettings().ToList();
        }
        catch (EPiServerException ePiServerException)
        {
            log.Error($"[Settings] {ePiServerException.Message}", ePiServerException);
        }

        foreach (var settingsType in settingsModelTypes)
        {
            var attribute =
                settingsType.GetCustomAttributes(type, false).FirstOrDefault() as
                    SettingsContentTypeAttribute;

            if (attribute == null)
            {
                continue;
            }

            if (attribute.SettingsInstanceGuid == null)
            {
                var errorMessage =
                    $"The SettingsInstanceGuid property must be set on the SettingsContentTypeAttribute of the Settings type {settingsType.Name}";

                log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            var attributeGuid = new Guid(attribute.SettingsInstanceGuid);
            IContent existingItem = null;

            if (existingItems.Any())
            {
                existingItem = existingItems.FirstOrDefault(i => i.ContentGuid == attributeGuid);
            }

            if (existingItem == null)
            {
                contentRepository.TryGet(attributeGuid, out existingItem);
            }

            if (existingItem == null)
            {
                var contentType = contentTypeRepository.Load(settingsType);

                var newSettings = contentRepository.GetDefault<IContent>(
                GlobalSettingsRoot,
                contentType.ID);

                newSettings.Name = attribute.SettingsName;
                newSettings.ContentGuid = new Guid(attribute.SettingsInstanceGuid);

                contentRepository.Save(
                newSettings,
                SaveAction.Publish,
                AccessLevel.NoAccess);
            }
        }

        // In case any reads of global settings has occured while populating the settings
        ClearCache();
    }

    /// <summary>
    ///     Tries to get the settings from content.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>An instance of <typeparamref name="T" /> </returns>
    private T TryGetSettingsFromContent<T>(IContent content)
        where T : SettingsBase
        {
            foreach (var settingsResolver in settingsResolvers)
            {
                if (settingsResolver.TryResolveSettingFromContent<T>(content, out var setting))
                {
                    return setting;
                }
            }

            return default;
        }

    /// <summary>Loads the global settings instances from below the Global settings root.</summary>
    /// <returns>
    ///     All loadable children
    /// </returns>
    private IEnumerable<IContent> LoadGlobalSettings()
    {
        var existingItemRefs = contentRepository.GetDescendents(GlobalSettingsRoot);

        foreach (var itemRef in existingItemRefs)
        {
            IContent item = null;

            try
            {
                item = contentRepository.Get<IContent>(itemRef);
            }
            catch (EPiServerException ex)
            {
                log.Error($"[Settings] {ex.Message}", ex);
            }

            // If the item is an instance of SettingsBase has been loaded as a fallback because the ContentType class no longer exists
            // or if it's no setting at all..
            if (item != null && item.GetOriginalType() != typeof(SettingsBase) && item is SettingsBase)
            {
                yield return item;
            }
        }
    }

    /// <summary>Gets the global settings from the cache if they exist, otherwise an empty Dictionary.</summary>
    /// <returns>
    ///     A dictionary of all existing global settings instances from the cache
    /// </returns>
    private Dictionary<Type, ContentReference> GetGlobalSettings()
    {
        return cache.ReadThrough(
        globalSettingsCacheKey,
        () => LoadGlobalSettings().Distinct(new ContentTypeComparer()).ToDictionary(item => item.GetOriginalType(), item => item.ContentLink),
        ReadStrategy.Wait);
    }

    /// <summary>Removes the global settings entry from the cache.</summary>
    private void ClearCache()
    {
        cache.Remove(globalSettingsCacheKey);
    }

    private class ContentTypeComparer : IEqualityComparer<IContent>
    {
        public bool Equals(IContent x, IContent y)
        {
            return x.GetOriginalType() == y.GetOriginalType();
        }

        public int GetHashCode(IContent obj)
        {
            return obj.GetOriginalType().GetHashCode();
        }
    }
}
