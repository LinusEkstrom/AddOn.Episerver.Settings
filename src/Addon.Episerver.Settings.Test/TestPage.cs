using EPiServer.Core;

namespace Addon.Episerver.Settings.Test;

public class TestPage : PageData
{
    public virtual ContentReference TestSetting { get; set; } = ContentReference.EmptyReference;
}