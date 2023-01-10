// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISettingsService.cs" company="none">
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

using EPiServer.Core;
using EPiServer.Web;
using System;
using System.Collections.Generic;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Interface ISettingsService
/// </summary>
public interface ISettingsService
{
    /// <summary>
    ///     Gets the global settings.
    /// </summary>
    /// <value>The global settings.</value>
    Dictionary<Type, ContentReference> GlobalSettings { get; }

    /// <summary>
    ///     Gets or sets the global settings root.
    /// </summary>
    /// <value>The global settings root.</value>
    ContentReference GlobalSettingsRoot { get; set; }

    /// <summary>
    ///     Gets or sets the settings root.
    /// </summary>
    /// <value>The settings root.</value>
    ContentReference SettingsRoot { get; set; }

    /// <summary>
    ///     Gets the settings roots.
    /// </summary>
    /// <value>The settings roots.</value>
    IEnumerable<ContentReference> SettingsRoots { get; }

    /// <summary>
    ///     Gets the first matching setting by traversing the content tree, starting the search for it from the current content context.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    T GetSetting<T>() where T : SettingsBase;

    /// <summary>
    ///     Gets the first matching setting by traversing the content tree, starting the search from the provided content reference.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <param name="contentLink">The content link.</param>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    T GetSetting<T>(ContentReference contentLink) where T : SettingsBase;

    /// <summary>
    ///     Gets the first matching setting by traversing the content tree, starting the search from the provided content reference.
    /// </summary>
    /// <param name="settingsType">The settings type</param>
    /// <param name="contentLink">The content link.</param>
    /// <returns>An instance of <typeparamref name="SettingsBase" /></returns>
    SettingsBase GetSetting(Type settingsType, ContentReference contentLink);
    
    /// <summary>
    ///     Gets the first matching setting by traversing the content tree, starting the search from the provided content.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <param name="content">The content.</param>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    T GetSetting<T>(IContent content) where T : SettingsBase;
    
    /// <summary>
    ///     Gets the first matching setting by traversing the content tree, starting the search from the provided content.
    /// </summary>
    /// <param name="settingsType">The settings type</param>
    /// <param name="content">The content where to start the search.</param>
    /// <returns><typeparamref name="SettingsBase" /></returns>
    SettingsBase GetSetting(Type settingsType, IContent content);
    
    /// <summary>
    ///     Gets the setting implementing the specified type from the global settings repository.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    T GetGlobalSetting<T>() where T : SettingsBase;
    
    /// <summary>
    ///     Gets the setting implementing the specified type from the global settings repository.
    /// </summary>
    /// <param name="settingsType">The settings type</param>
    /// <returns>An instance of <typeparamref name="SettingsBase" /> </returns>
    /// <exception cref="ArgumentException">It type does not inherit from <typeparamref name="SettingsBase" /></exception>
    SettingsBase GetGlobalSetting(Type settingsType);

    /// <summary>
    ///     Gets all settings found traversing the content tree starting from the provided content link.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    IEnumerable<T> GetSettingsRecursive<T>(ContentReference contentLink) where T : SettingsBase;
    
    /// <summary>
    ///     Gets all settings found traversing the content tree starting from the provided content.
    /// </summary>
    /// <typeparam name="T">The settings type</typeparam>
    /// <returns>An instance of <typeparamref name="T" /></returns>
    IEnumerable<T> GetSettingsRecursive<T>(IContent content) where T : SettingsBase;

    /// <summary>
    ///     Initializes the settings.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException">If the root name is already registered with another contentRootId.</exception>
    void InitSettings();

    /// <summary>
    ///     Updates the settings.
    /// </summary>
    /// <param name="content">The content.</param>
    void UpdateSettings(IContent content);

    /// <summary>
    ///     Updates the settings root folder for a site.
    /// </summary>
    /// <param name="siteDefinition">The site definition.</param>
    ContentReference ValidateOrCreateSiteSettingsRoot(SiteDefinition siteDefinition);
}
