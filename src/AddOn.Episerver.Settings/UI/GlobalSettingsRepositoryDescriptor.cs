// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSettingsRepositoryDescriptor.cs" company="none">
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

using AddOn.Episerver.Settings.Core;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddOn.Episerver.Settings.UI;

/// <summary>
///     Class GlobalSettingsRepositoryDescriptor.
///     Implements the <see cref="ContentRepositoryDescriptorBase" />
/// </summary>
/// <seealso cref="ContentRepositoryDescriptorBase" />
[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class GlobalSettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    /// <summary>
    ///     Gets the repository key.
    /// </summary>
    /// <value>The repository key.</value>
    public static string RepositoryKey => "globalsettings";

    /// <summary>
    ///     Gets the contained types.
    /// </summary>
    /// <value>The contained types.</value>
    public override IEnumerable<Type> ContainedTypes
    {
        get { return new[] { typeof(SettingsBase) }; }
    }

    /// <summary>
    ///     Gets the creatable types.
    /// </summary>
    /// <value>The creatable types.</value>
    /// <remarks>Disable creating items by editors.</remarks>
    public override IEnumerable<Type> CreatableTypes => new Type[0];

    /// <summary>
    ///     Gets the custom navigation widget.
    /// </summary>
    /// <value>The custom navigation widget.</value>
    public override string CustomNavigationWidget => "epi-cms/component/ContentNavigationTree";

    /// <summary>
    ///     Gets the custom select title.
    /// </summary>
    /// <value>The custom select title.</value>
    public override string CustomSelectTitle => LocalizationService.Current.GetString("/contentrepositories/globalsettings/customselecttitle");

    /// <summary>
    ///     Gets the key.
    /// </summary>
    /// <value>The key.</value>
    public override string Key => RepositoryKey;

    /// <summary>
    ///     Gets the main views.
    /// </summary>
    /// <value>The main views.</value>
    public override IEnumerable<string> MainViews
    {
        get { return new string[1] { SettingsView.ViewName }; }
    }

    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public override string Name => LocalizationService.Current.GetString("/contentrepositories/globalsettings/name");

    /// <summary>
    ///     Gets the prevent copying for.
    /// </summary>
    /// <value>The prevent copying for.</value>
    public override IEnumerable<string> PreventCopyingFor
    {
        get
        {
            return Settings.Service.GlobalSettings.Select(
            gs => gs.Value.ToString());
        }
    }

    /// <summary>
    ///     Gets the prevent deletion for.
    /// </summary>
    /// <value>The prevent deletion for.</value>
    public override IEnumerable<string> PreventDeletionFor
    {
        get
        {
            return Settings.Service.GlobalSettings.Select(
            gs => gs.Value.ToString());
        }
    }

    /// <summary>
    ///     Gets the roots.
    /// </summary>
    /// <value>The roots.</value>
    public override IEnumerable<ContentReference> Roots
    {
        get { return new[] { Settings.Service.GlobalSettingsRoot }; }
    }

    /// <summary>
    ///     Gets the search area.
    /// </summary>
    /// <value>The search area.</value>
    public override string SearchArea => GlobalSettingsSearchProvider.SearchArea;

    /// <summary>
    ///     Gets the sort order.
    /// </summary>
    /// <value>The sort order.</value>
    public override int SortOrder => 1000;

    private Injected<ISettingsService> Settings { get; set; }
}
