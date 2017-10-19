using EPiServer.Core;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using EpiserverExtensions.Settings.Core;

namespace EpiserverExtensions.Settings.UI
{
    /// <summary>
    /// Editor descriptor that will create a block selector.
    /// </summary>
    [EditorDescriptorRegistration(TargetType = typeof(ContentReference), UIHint = "dynamicsettings")]
    public class SettingsReferenceEditorDescriptor : ContentReferenceEditorDescriptor<LocalizableSettingsBase>
    {
        public override string RepositoryKey
        {
            get
            {
                return SettingsRepositoryDescriptor.RepositoryKey;
            }
        }
    }
}