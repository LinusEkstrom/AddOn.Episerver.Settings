// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsMenuProvider.cs" company="none">
//      Copyright © 2020 Linus Ekström, Jeroen Stemerdink.
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//      SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using EPiServer.Shell.Navigation;
using System.Collections.Generic;

namespace AddOn.Episerver.Settings.UI;

#if NET48
using EPiServer.Security;

#else
    using EPiServer.Authorization;
#endif

/// <summary>
///     Provides menu items for the settings component.
/// </summary>
[MenuProvider]
public class SettingsMenuProvider : IMenuProvider
{
    /// <summary>
    ///     Provides the CMS menu section and the CMS settings section.
    /// </summary>
    /// <returns>
    ///     A list of <see cref="MenuItem" />s that the provider exposes.
    /// </returns>
    public IEnumerable<MenuItem> GetMenuItems()
    {
        var cmsGlobalSettings = new UrlMenuItem(
        "Global settings",
        "/global/cms/settings",
        "/episerver/AddOn.Episerver.Settings/settings")
        {
#if NET48
            IsAvailable = request => PrincipalInfo.HasAdminAccess
#else
                IsAvailable = request => request.User.IsInRole(Roles.CmsAdmins)
#endif
        };

        return new MenuItem[] { cmsGlobalSettings };
    }
}
