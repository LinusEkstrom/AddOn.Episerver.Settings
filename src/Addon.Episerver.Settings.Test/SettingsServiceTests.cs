using AddOn.Episerver.Settings.Core;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Web.Routing;
using Moq;

namespace Addon.Episerver.Settings.Test;

public class SettingsServiceTests
{
    [Fact]
    public void GetSettingFromLinkShouldResolveAnExistingSetting()
    {
        var page1 = new Mock<TestPage>();
        var pageLink = new PageReference(100);
        var settingsLink = new ContentReference(200);
        page1.Setup(page => page.ContentLink).Returns(pageLink);
        page1.Setup(page => page.TestSetting).Returns(settingsLink);
        var setting1 = new Mock<TestSetting>();
        setting1.Setup(setting => setting.ContentLink).Returns(settingsLink);

        var settingsService = SetupSettingsService( new PageData[] { page1.Object }, setting1.Object);

        var result = settingsService.GetSetting(typeof(TestSetting), pageLink);
        
        Assert.Equal(setting1.Object, result);
    }
    
    [Fact]
    public void GetSettingFromContentShouldResolveAnExistingSetting()
    {
        var page1 = new Mock<TestPage>();
        var pageLink = new PageReference(100);
        var settingsLink = new ContentReference(200);
        page1.Setup(page => page.ContentLink).Returns(pageLink);
        page1.Setup(page => page.TestSetting).Returns(settingsLink);
        var setting1 = new Mock<TestSetting>();
        setting1.Setup(setting => setting.ContentLink).Returns(settingsLink);

        var settingsService = SetupSettingsService( new PageData[] { page1.Object }, setting1.Object);

        var result = settingsService.GetSetting(typeof(TestSetting), page1.Object);
        
        Assert.Equal(setting1.Object, result);
    }
    
    [Fact]
    public void GetSettingShouldReturnNullIfNoSettingExists()
    {
        var page1 = new Mock<TestPage>();
        var pageLink = new PageReference(100);
        page1.Setup(page => page.ContentLink).Returns(pageLink);
        
        var settingsService = SetupSettingsService( new PageData[] { page1.Object }, null);

        var result = settingsService.GetSetting(typeof(TestSetting), page1.Object);
        
        Assert.Null(result);
    }
    
    [Fact]
    public void GetSettingShouldTraverseThePageTreeToResolveTheSetting()
    {
        var settingsLink = new ContentReference(200);
        var setting1 = new Mock<TestSetting>();
        setting1.Setup(setting => setting.ContentLink).Returns(settingsLink);
        
        var page1 = new Mock<TestPage>();
        var pageLink1 = new PageReference(100);
        var page2 = new Mock<TestPage>();
        var pageLink2 = new PageReference(101);
        page1.Setup(page => page.ContentLink).Returns(pageLink1);
        page1.Setup(page => page.ParentLink).Returns(pageLink2);
        page2.Setup(page => page.ContentLink).Returns(pageLink2);
        page2.Setup(page => page.TestSetting).Returns(settingsLink);
        
        var settingsService = SetupSettingsService( new PageData[] { page1.Object, page2.Object }, setting1.Object);

        var result = settingsService.GetSetting(typeof(TestSetting), page1.Object);
        
        Assert.Equal(setting1.Object, result);
    }

    private SettingsService SetupSettingsService(PageData[] pages, SettingsBase? result)
    {
        var contentRepository = new Mock<IContentRepository>();
        var ancestorReferencesLoader = new Mock<AncestorReferencesLoader>();
        var settingsResolver = new Mock<ISettingsResolver>();
        var globalSettingsCache = new Mock<ISynchronizedObjectInstanceCache>();

        IContent? content = null;
        contentRepository.Setup(repository => repository.TryGet(It.IsAny<ContentReference>(), out content))
            .Returns((ContentReference link, out IContent? c) =>
            {
                c = pages.FirstOrDefault(x => link.CompareToIgnoreWorkID(x.ContentLink));
                return c != null;
            });
        
        SettingsBase? setting = null;
        settingsResolver.Setup(resolver => resolver.TryResolveSettingFromContent(It.IsAny<IContent>(), out setting))
            .Returns((IContent c, out SettingsBase? s) =>
            {
                s = result;
                return s != null;
            });

        ancestorReferencesLoader.Setup(loader => loader.GetAncestors(It.IsAny<ContentReference>()))
            .Returns((ContentReference link) =>
            {
                return pages.SkipWhile(p => !p.ContentLink.CompareToIgnoreWorkID(link)).Skip(1)
                    .Select(p => p.ContentLink).ToList();
            });

        
        var settingsService = new SettingsService(
            contentRepository.Object,
            null,
            null,
            null,
            null,
            ancestorReferencesLoader.Object,
            globalSettingsCache.Object,
            new[] { settingsResolver.Object },
            null
        );

        return settingsService;
    }
}