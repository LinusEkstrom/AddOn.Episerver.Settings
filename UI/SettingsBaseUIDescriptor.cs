using System;
using EPiServer.Core;
using EPiServer.Shell;
using EpiserverExtensions.Settings.Core;

namespace EpiserverExtensions.Settings.UI
{
    [UIDescriptorRegistration]
    public class SettingsBaseUIDescriptor : UIDescriptor<LocalizableSettingsBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockUIDescriptor"/> class.
        /// </summary>
        public SettingsBaseUIDescriptor()
            : base()
        {
            IsPrimaryType = true;
            ContainerTypes = new Type[] { typeof(ContentFolder) }; // list block folder in restore dialog only, since block cannot be moved into another block.
            CommandIconClass = "epi-iconSharedBlock";
        }
    }
}