using EPiServer;
using EPiServer.Core;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Resolves settings based on the class name of the settings type.
/// </summary>
public class PropertyNameSettingsResolver : ISettingsResolver
{
    private readonly IContentRepository contentRepository;

    /// <summary>
    ///     Gets a value used for ordering registered resolvers
    /// </summary>
    public int SortOrder => int.MaxValue;
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyNameSettingsResolver" /> class.
    /// </summary>
    /// <param name="contentRepository"></param>
    public  PropertyNameSettingsResolver(IContentRepository contentRepository)
    {
        this.contentRepository = contentRepository;
    }

    /// <summary>
    ///     Tries to resolve a settings instance based on the class name of the settings type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="setting"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>A boolean indicating if a setting was found</returns>
    public bool TryResolveSettingFromContent<T>(IContent content, out T setting) where T : SettingsBase
    {
        setting = null;
        PropertyData property = content.Property[name: typeof(T).Name];

        if (property == null || property.IsNull)
        {
            return false;
        }

        ContentReference reference = property.Value as ContentReference;

        if (reference == null)
        {
            return false;
        }
            
        contentRepository.TryGet(contentLink: reference, content: out setting);

        return setting != null;
    }
}
