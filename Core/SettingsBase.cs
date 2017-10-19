using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Core;

namespace EpiserverExtensions.Settings.Core
{
    public class SettingsBase : BasicContent, IVersionable
    {
        //TODO: Add content type translations for base class

        public bool IsPendingPublish { get; set; }

        public DateTime? StartPublish { get; set; }

        public VersionStatus Status { get; set; }

        public DateTime? StopPublish { get; set; }
    }
}