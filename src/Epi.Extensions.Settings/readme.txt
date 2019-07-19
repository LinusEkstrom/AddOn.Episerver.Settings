1. Add a settings item

[ContentType(GUID = "a5506171-c6b2-4a7a-9c44-bb870f695956")]
public class MenuSettings : SettingsBase
{
    public virtual ContentReference MenuRoot { get; set; }
}


2. Add a property to e.g the start page

[UIHint("dynamicsettings")]
[AllowedTypes(new Type[] { typeof(MenuSettings) })]
[Display(GroupName = "SiteSettings")]
public virtual ContentReference MenuSettings { get; set; }


3. Getting the settings

ISettingsService settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
settingsService.GetSettings<MenuSettings>(currentPage);


The convention here is that we will traverse the structure and look for properties with the same name as the settings type, in this case "MenuSettings".


3. Add a global settings item (will be created at initialization)

[SettingsContentType(AvailableInEditMode = false, GUID = "15506171-c6b2-4a7a-9c44-bb870f695911", SettingsInstanceGUID = "d8701e64-8206-4e24-bd3f-cb02b875d6c6", SettingsName = "Google Analytics")]
public class GoogleAnalyticsSettings : SettingsBase
{
    public virtual string UserName { get; set; }
}


4. Getting the global settings

ISettingsService settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
settingsService.GetSettings<GoogleAnalyticsSettings>();


5. If you go into the editorial interface, there should be a new tab under assets: "Settings". You can create new instances here. 
   In the main menu, under CMS, there should be an item "Global settings", where you can find the global settings items.

6. You can even combine local settings with global settings where a local setting will override a global setting. If you want to try this, you can do the following:
* Go into the adminstrative interface and change the GoogleAnalyticsSettings content type to allow creation in edit mode.
* Go into the settings gadget in the editorial view and create a GoogleAnalyticsSettings setting.
* Add this property to a page type:

[UIHint("dynamicsettings")]
[AllowedTypes(new Type[] { typeof(GoogleAnalyticsSettings) })]
[Display(GroupName = "SiteSettings)]
public virtual ContentReference GoogleAnalyticsSettings { get; set; }

* Assign the value of the property to the setting you created in the local settings.
*After you have done this, content under the node that has assigned the local setting should get these settings, while content outside of this structure should get the global settings.
