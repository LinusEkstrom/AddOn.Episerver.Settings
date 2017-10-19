using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.TypeScanner;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EpiserverExtensions.Settings.Core
{
    [ServiceConfiguration(typeof(SettingsService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class SettingsService
    {
        private readonly IContentRepository _contentRepository;
        private readonly ContentRootService _contentRootService;
        private readonly ITypeScannerLookup _typeScannerLookup;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly AncestorReferencesLoader _ancestorReferencesLoader;
        private readonly IContentEvents _contentEvents;

        private Dictionary<Type, object> _settings;

        #region Global Settings Root

        public const string GlobalSettingsRootName = "Global Settings Root";

        public static Guid GlobalSettingsRootGuid = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61a");

        public static ContentReference GlobalSettingsRoot;

        #endregion

        #region Settings Root

        public const string SettingsRootName = "Settings Root";

        public static Guid SettingsRootGuid = new Guid("98ed413d-d7b5-4fbf-92a6-120d850fe61c");

        public static ContentReference SettingsRoot;

        #endregion

        public SettingsService(IContentRepository contentRepository, ContentRootService contentRootService, ITypeScannerLookup typeScannerLookup, IContentTypeRepository contentTypeRepository, AncestorReferencesLoader ancestorReferencesLoader, IContentEvents contentEvents)
        {
            _contentRepository = contentRepository;
            _contentRootService = contentRootService;
            _typeScannerLookup = typeScannerLookup;
            _contentTypeRepository = contentTypeRepository;
            _ancestorReferencesLoader = ancestorReferencesLoader;
            _contentEvents = contentEvents;

            _settings = new Dictionary<Type, object>();
        }

        public void InitSettings()
        {
            _contentRootService.Register<ContentFolder>(GlobalSettingsRootName, GlobalSettingsRootGuid, ContentReference.RootPage);
            GlobalSettingsRoot = _contentRootService.Get(GlobalSettingsRootName);

            _contentRootService.Register<ContentFolder>(SettingsRootName, SettingsRootGuid, ContentReference.RootPage);
            SettingsRoot = _contentRootService.Get(SettingsRootName);

            InitializeGlobalSettingsInstances();

            //After initial set up we listen to changes for global settings to refresh the settings dictionary.
            _contentEvents.PublishedContent += ContentEvents_PublishedContent;
        }

        void ContentEvents_PublishedContent(object sender, ContentEventArgs e)
        {
            if (e.Content.ParentLink == GlobalSettingsRoot)
            {
                InitializeGlobalSettingsInstances();
            }
        }

        public T GetSettings<T>()
        {
            if (_settings.ContainsKey(typeof(T)))
            {
                return (T)_settings[typeof(T)];
            }
            return default(T);
        }

        public T GetSettings<T>(IContent content) where T : IContent
        {
            var settings = TryGetSettingsFromContent<T>(content);

            if(settings != null)
            {
                return settings;
            }

            var ancestors = _ancestorReferencesLoader.GetAncestors(content.ContentLink);

            foreach (ContentReference parentReference in ancestors)
            {
                var parentContent = _contentRepository.Get<IContent>(parentReference);

                settings = TryGetSettingsFromContent<T>(parentContent);

                if (settings != null)
                {
                    return settings;
                }
            }

            return GetSettings<T>();
        }

        private T TryGetSettingsFromContent<T>(IContent content) where T : IContent
        {
            var property = content.Property[typeof(T).Name];
            if (property != null && !property.IsNull)
            {
                var reference = property.Value as ContentReference;

                if (reference != null)
                {
                    var settingsObject = _contentRepository.Get<T>(reference);

                    if (settingsObject != null)
                    {
                        return settingsObject;
                    }
                }
            }

            return default(T);
        }

        private void InitializeGlobalSettingsInstances()
        {
            _settings.Clear();

            var settingsModelTypes = _typeScannerLookup.AllTypes.Where(t => t.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false).Length > 0);

            var existingItems = _contentRepository.GetChildren<IContent>(GlobalSettingsRoot);

            //TODO: Investigate if we can use content root service to the singleton registration.

            foreach (Type settingsType in settingsModelTypes)
            {
                Type type = typeof(SettingsContentTypeAttribute);
                var attribute = settingsType.GetCustomAttributes(type, false).FirstOrDefault() as SettingsContentTypeAttribute;

                if (attribute != null)
                {
                    var existingItem = existingItems.FirstOrDefault(i => i.ContentGuid == new Guid(attribute.SettingsInstanceGUID));

                    if (existingItem == null)
                    {
                        var contentType = _contentTypeRepository.Load(settingsType);
                        var newSettings = _contentRepository.GetDefault<IContent>(GlobalSettingsRoot, contentType.ID);
                        newSettings.Name = attribute.SettingsName;
                        newSettings.ContentGuid = new Guid(attribute.SettingsInstanceGUID);

                        _contentRepository.Save(newSettings, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

                        existingItem = newSettings;
                    }

                    _settings.Add(existingItem.GetOriginalType(), existingItem);
                }
            }
        }
    }
}