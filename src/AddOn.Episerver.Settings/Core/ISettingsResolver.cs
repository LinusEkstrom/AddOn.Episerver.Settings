using EPiServer.Core;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Implement this interface if you wish to customize how settings are
///     resolved from the content tree.
/// </summary>
public interface ISettingsResolver
{
    /// <summary>
    ///     Used to order the registered resolvers. Use any value below int32.MaxValue
    ///     to be called before the default implementation.
    /// </summary>
    public int SortOrder { get; }
    
    /// <summary>
    ///     Tries to locate a settings instance based on the content item provided.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="setting"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>A boolean indicating if a setting was found</returns>
    public bool TryResolveSettingFromContent<T>(IContent content, out T setting)  where T : SettingsBase;
}
