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

namespace AddOn.Episerver.Settings.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using EPiServer;
    using EPiServer.Core;
    using EPiServer.DataAbstraction;
    using EPiServer.Framework.Cache;
    using EPiServer.Framework.TypeScanner;
    using EPiServer.Logging;
    using EPiServer.Web;
    using EPiServer.Web.Routing;

    /// <summary>
    /// Class SettingsService.
    /// Implements the <see cref="ISettingsService" />
    /// </summary>
    /// <seealso cref="ISettingsService" />
    public class SettingsService : ISettingsService, IDisposable
    {
        /// <summary>
        /// The global settings root name
        /// </summary>
        public const string GlobalSettingsRootName = "Global Settings Root";

        /// <summary>
        /// The settings root name
        /// </summary>
        public const string SettingsRootName = "Settings Root";

        /// <summary>
        /// The ancestor references loader
        /// </summary>
        private readonly AncestorReferencesLoader ancestorReferencesLoader;

        /// <summary>
        /// The content repository
        /// </summary>
        private readonly IContentRepository contentRepository;

        /// <summary>
        /// The content root service
        /// </summary>
        private readonly ContentRootService contentRootService;

        /// <summary>
        /// The content type repository
        /// </summary>
        private readonly IContentTypeRepository contentTypeRepository;

        /// <summary>
        /// The logger instance
        /// </summary>
        private readonly ILogger log = LogManager.GetLogger();

        /// <summary>
        /// The cache lock
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The type scanner lookup
        /// </summary>
        private readonly ITypeScannerLookup typeScannerLookup;

        /// <summary>
        /// The synchronized object instance cache
        /// </summary>
        private readonly ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache;

        /// <summary><c>true</c> when this instance is already disposed off; <c>false</c> to when not.</summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="contentRepository">The content repository.</param>
        /// <param name="contentRootService">The content root service.</param>
        /// <param name="typeScannerLookup">The type scanner lookup.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="ancestorReferencesLoader">The ancestor references loader.</param>
        public SettingsService(
            IContentRepository contentRepository,
            ContentRootService contentRootService,
            ITypeScannerLookup typeScannerLookup,
            IContentTypeRepository contentTypeRepository,
            AncestorReferencesLoader ancestorReferencesLoader,
            ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache)
        {
            this.contentRepository = contentRepository;
            this.contentRootService = contentRootService;
            this.typeScannerLookup = typeScannerLookup;
            this.contentTypeRepository = contentTypeRepository;
            this.ancestorReferencesLoader = ancestorReferencesLoader;

            this.GlobalSettings = new Dictionary<Type, object>();
            this.synchronizedObjectInstanceCache = synchronizedObjectInstanceCache;
        }

        /// <summary>
        /// Gets the global settings.
        /// </summary>
        /// <value>The global settings.</value>
        public Dictionary<Type, object> GlobalSettings { get; }

        /// <summary>
        /// Gets or sets the global settings root.
        /// </summary>
        /// <value>The global settings root.</value>
        public ContentReference GlobalSettingsRoot { get; set; }

        /// <summary>
        /// Gets or sets the settings root.
        /// </summary>
        /// <value>The settings root.</value>
        public ContentReference SettingsRoot { get; set; }

        /// <summary>
        /// Gets the global settings root unique identifier
        /// </summary>
        private Guid GlobalSettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61a");

        /// <summary>
        /// Gets the settings root unique identifier
        /// </summary>
        private Guid SettingsRootGuid { get; } = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61c");

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.Dispose(true);
        }

        /// <summary>
        /// Create cache key
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string CreateCacheKey(Type type)
        {
            return $"Settings_{type}";
        }

        /// <summary>
        /// Add cache item
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        private void AddCacheItem(string cacheKey, object value)
        {
            this.synchronizedObjectInstanceCache.Insert(cacheKey, value, new CacheEvictionPolicy(null, new[] { "Epinova:MasterSettings" }));
        }

        /// <summary>
        /// Get cache item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private T GetCacheItem<T>(string cacheKey)
        {
            var value = this.synchronizedObjectInstanceCache.Get(cacheKey);

            if (value != null)
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// Update cache item
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        private void UpdateCacheItem(string cacheKey, object value)
        {
            this.synchronizedObjectInstanceCache.Remove(cacheKey);
            this.AddCacheItem(cacheKey, value);
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        /// <exception cref="T:System.Threading.LockRecursionException">The current thread cannot acquire the write lock when it holds the read lock.-or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the read lock. -or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the write lock. -or-The recursion number would exceed the capacity of the counter. This limit is so large that applications should never encounter this exception.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in read mode.</exception>
        public T GetSettings<T>()
        {
            this.readerWriterLock.EnterReadLock();

            try
            {
                var type = typeof(T);
                var cacheKey = CreateCacheKey(type);
                var cachedSettings = this.GetCacheItem<T>(cacheKey);
                
                if (cachedSettings != null)
                {
                    return cachedSettings;
                }

                if (this.GlobalSettings.ContainsKey(type))
                {
                    return (T)this.GlobalSettings[type];
                }
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                this.log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
            }
            catch (ArgumentNullException argumentNullException)
            {
                this.log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
            }
            finally
            {
                this.readerWriterLock.ExitReadLock();
            }

            return default;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in read mode.</exception>
        /// <exception cref="T:System.Threading.LockRecursionException">The current thread cannot acquire the write lock when it holds the read lock.-or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the read lock. -or-The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" />, and the current thread has attempted to acquire the read lock when it already holds the write lock. -or-The recursion number would exceed the capacity of the counter. This limit is so large that applications should never encounter this exception.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        public T GetSettings<T>(IContent content)
            where T : IContent
        {
            if (content == null)
            {
                return default;
            }

            T settingsFromContent = this.TryGetSettingsFromContent<T>(content: content);

            if (settingsFromContent != null)
            {
                return settingsFromContent;
            }

            IEnumerable<ContentReference> ancestors =
                this.ancestorReferencesLoader.GetAncestors(contentLink: content.ContentLink);

            foreach (ContentReference parentReference in ancestors)
            {
                IContent parentContent;

                if (!this.contentRepository.TryGet(contentLink: parentReference, content: out parentContent))
                {
                    continue;
                }

                settingsFromContent = this.TryGetSettingsFromContent<T>(content: parentContent);

                if (settingsFromContent != null)
                {
                    return settingsFromContent;
                }
            }

            return this.GetSettings<T>();
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">If the rootname is already registered with another contentRootId.</exception>
        public void InitSettings()
        {
            try
            {
                this.contentRootService.Register<ContentFolder>(
                    rootName: GlobalSettingsRootName,
                    contentRootId: this.GlobalSettingsRootGuid,
                    parent: SiteDefinition.Current.GlobalAssetsRoot);
                this.GlobalSettingsRoot = this.contentRootService.Get(rootName: GlobalSettingsRootName);
            }
            catch (NotSupportedException notSupportedException)
            {
                this.log.Error($"[Settings] {notSupportedException.Message}", exception: notSupportedException);
                throw;
            }

            try
            {
                this.contentRootService.Register<ContentFolder>(
                    rootName: SettingsRootName,
                    contentRootId: this.SettingsRootGuid,
                    parent: SiteDefinition.Current.SiteAssetsRoot);
                this.SettingsRoot = this.contentRootService.Get(rootName: SettingsRootName);
            }
            catch (NotSupportedException notSupportedException)
            {
                this.log.Error($"[Settings] {notSupportedException.Message}", exception: notSupportedException);
                throw;
            }

            this.InitializeContentInstances();
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <exception cref="T:System.Threading.LockRecursionException">The <see cref="P:System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is <see cref="F:System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has already entered the lock in any mode. -or-The current thread has entered read mode, so trying to enter the lock in write mode would create the possibility of a deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.ReaderWriterLockSlim" /> object has been disposed.</exception>
        /// <exception cref="T:System.Threading.SynchronizationLockException">The current thread has not entered the lock in write mode.</exception>
        public void UpdateSettings(IContent content)
        {
            Type contentType = content.GetOriginalType();

            this.readerWriterLock.EnterWriteLock();

            try
            {
                if (!this.GlobalSettings.ContainsKey(key: contentType))
                {
                    return;
                }

                this.GlobalSettings[key: contentType] = content;

                var cacheKey = CreateCacheKey(contentType);

                this.UpdateCacheItem(cacheKey, content);
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                this.log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
            }
            catch (ArgumentNullException argumentNullException)
            {
                this.log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (this.readerWriterLock != null)
            {
                this.readerWriterLock.Dispose();
            }

            this.disposed = true;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Initializes the content instances.
        /// </summary>
        /// <exception cref="T:EPiServer.Core.EPiServerException">Could not create instance of a content type since it has an invalid .NET class associated. This happens if a class have been deleted, try remove the content type in Admin or from database."</exception>
        private void InitializeContentInstances()
        {
            List<IContent> existingItems = new List<IContent>();
            Type type = typeof(SettingsContentTypeAttribute);
            IEnumerable<Type> settingsModelTypes = this.typeScannerLookup.AllTypes.Where(
                t => t.GetCustomAttributes(type, false).Length > 0);

            try
            {
                existingItems = this.contentRepository
                    .GetChildren<IContent>(contentLink: this.GlobalSettingsRoot).ToList();
            }
            catch (EPiServerException ePiServerException)
            {
                this.log.Error($"[Settings] {ePiServerException.Message}", exception: ePiServerException);
            }
            
            foreach (Type settingsType in settingsModelTypes)
            {
                SettingsContentTypeAttribute attribute =
                    settingsType.GetCustomAttributes(attributeType: type, false).FirstOrDefault() as
                        SettingsContentTypeAttribute;

                if (attribute == null)
                {
                    continue;
                }

                Guid attributeGuid = new Guid(g: attribute.SettingsInstanceGuid);
                IContent existingItem = null;
                
                if(existingItems.Any())
                {
                    existingItem = existingItems.FirstOrDefault(i => i.ContentGuid == attributeGuid);
                }
                if(existingItem == null)
                {
                    contentRepository.TryGet<IContent>(attributeGuid, out existingItem);
                }

                if (existingItem == null)
                {
                    ContentType contentType = this.contentTypeRepository.Load(modelType: settingsType);

                    IContent newSettings = this.contentRepository.GetDefault<IContent>(
                        parentLink: this.GlobalSettingsRoot,
                        contentTypeID: contentType.ID);

                    newSettings.Name = attribute.SettingsName;
                    newSettings.ContentGuid = new Guid(g: attribute.SettingsInstanceGuid);

                    this.contentRepository.Save(
                        content: newSettings,
                        action: EPiServer.DataAccess.SaveAction.Publish,
                        access: EPiServer.Security.AccessLevel.NoAccess);

                    existingItem = newSettings;
                }

                var originalType = existingItem.GetOriginalType();
                var cacheKey = CreateCacheKey(originalType);

                this.GlobalSettings.Add(originalType, value: existingItem);
                this.AddCacheItem(cacheKey, existingItem);
            }
        }

        /// <summary>
        /// Tries to get the settings from content.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>An instance of <typeparamref name="T"/> </returns>
        private T TryGetSettingsFromContent<T>(IContent content)
            where T : IContent
        {
            PropertyData property = content.Property[name: typeof(T).Name];

            if (property == null || property.IsNull)
            {
                return default;
            }

            ContentReference reference = property.Value as ContentReference;

            if (reference == null)
            {
                return default;
            }

            T settingsObject;

            this.contentRepository.TryGet(contentLink: reference, content: out settingsObject);

            return settingsObject != null ? settingsObject : default;
        }
    }
}