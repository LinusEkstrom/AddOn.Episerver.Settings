# AddOn.Episerver.Settings

[![Platform](https://img.shields.io/badge/platform-.NET%204.6.1-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx)
[![Platform](https://img.shields.io/badge/platform-.NET%205-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2011.20.7-orange.svg?style=flat)](http://world.episerver.com/cms/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012.0.2-orange.svg?style=flat)](http://world.episerver.com/cms/)
[![GitHub license](https://img.shields.io/badge/license-MIT%20license-blue.svg?style=flat)](LICENSE)


## About
A typed replacement of custom built settings systems built on PageData/BlockData.

### Instructions

1. Add a settings item
> You can use a [Resharper template](templates/SettingsTemplates.DotSettings)
```csharp
[ContentType(GUID = "a5506171-c6b2-4a7a-9c44-bb870f695956")]
public class MenuSettings : SettingsBase
{
    public virtual ContentReference MenuRoot { get; set; }
}
```

2. Add a property to e.g the start page
```csharp
[UIHint("dynamicsettings")]
[AllowedTypes(new Type[] { typeof(MenuSettings) })]
[Display(GroupName = "SiteSettings")]
public virtual ContentReference MenuSettings { get; set; }
```

3. Getting the settings
```csharp
ISettingsService settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
settingsService.GetSettings<MenuSettings>(currentPage);
```

The convention here is that we will traverse the structure and look for properties with the same name as the settings type, in this case "MenuSettings".


3. Add a global settings item (will be created at initialization)
> You can use a [Resharper template](templates/SettingsTemplates.DotSettings)
```csharp
[SettingsContentType(AvailableInEditMode = false, GUID = "15506171-c6b2-4a7a-9c44-bb870f695911", SettingsInstanceGuid = "d8701e64-8206-4e24-bd3f-cb02b875d6c6", SettingsName = "Google Analytics")]
public class GoogleAnalyticsSettings : SettingsBase
{
    public virtual string UserName { get; set; }
}
```

4. Getting the global settings
```csharp
ISettingsService settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
settingsService.GetSettings<GoogleAnalyticsSettings>();
```

5. If you go into the editorial interface, there should be a new tab under assets: "Settings". You can create new instances here. 
   In the main menu, under CMS, there should be an item "Global settings", where you can find the global settings items.

6. You can even combine local settings with global settings where a local setting will override a global setting. If you want to try this, you can do the following:
* Go into the adminstrative interface and change the GoogleAnalyticsSettings content type to allow creation in edit mode.
* Go into the settings gadget in the editorial view and create a GoogleAnalyticsSettings setting.
* Add this property to a page type:
```csharp
[UIHint("dynamicsettings")]
[AllowedTypes(new Type[] { typeof(GoogleAnalyticsSettings) })]
[Display(GroupName = "SiteSettings)]
public virtual ContentReference GoogleAnalyticsSettings { get; set; }
```
* Assign the value of the property to the setting you created in the local settings.
* After you have done this, content under the node that has assigned the local setting should get these settings, while content outside of this structure should get the global settings.

## Removing Settings classes
If upgrading from an earlier version of AddOn.Episerver.Settings, some values must be set in the database first. In table tblContentType each row that contains a Settings-type must have the Base-column updated to the value **Setting**. This ensures that existing settings entities can be loaded by the CMS even if the type class no longer exists.

Then locate and delete all instances that are of the no longer existing settings-types. Remember to remove from the _Global Settings_ **and** to empty the trash can. After this is done the types will be dropped automatically the next time the CMS is restarted.

0. Update tblContentType, set Base column to "Setting" for Settings types.
1. Deploy code with removed obsolete settings classes to the site.
2. Remove obsolete instances of settings, including global settings.
3. Restart site so that the CMS removes the no longer used Content Types.

## Requirements

* Episerver CMS >= 11.20.7
* .Net 4.6.1
