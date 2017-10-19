using System;
using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;

namespace EpiserverExtensions.Settings.Core
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class SettingsInit : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var settingsService = ServiceLocator.Current.GetInstance<SettingsService>();

            settingsService.InitSettings();
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}