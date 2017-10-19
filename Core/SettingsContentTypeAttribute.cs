using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.DataAnnotations;

namespace EpiserverExtensions.Settings.Core
{
    public class SettingsContentTypeAttribute : ContentTypeAttribute
    {
        public string SettingsInstanceGUID { get; set; }

        public string SettingsName { get; set; }
    }
}