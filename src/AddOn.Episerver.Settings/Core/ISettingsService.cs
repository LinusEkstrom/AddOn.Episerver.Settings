// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISettingsService.cs" company="none">
//      Copyright © 2019 Linus Ekström, Jeroen Stemerdink.
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

namespace AddOn.Episerver.Settings.Core
{
    using System;
    using System.Collections.Generic;

    using EPiServer.Core;

    /// <summary>
    /// Interface ISettingsService
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets the global settings.
        /// </summary>
        /// <value>The global settings.</value>
        Dictionary<Type, object> GlobalSettings { get; }

        /// <summary>
        /// Gets or sets the global settings root.
        /// </summary>
        /// <value>The global settings root.</value>
        ContentReference GlobalSettingsRoot { get; set; }

        /// <summary>
        /// Gets or sets the settings root.
        /// </summary>
        /// <value>The settings root.</value>
        ContentReference SettingsRoot { get; set; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <returns>An instance of <typeparamref name="T" /></returns>
        T GetSettings<T>();

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <typeparam name="T">The settings type</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>An instance of <typeparamref name="T" /></returns>
        T GetSettings<T>(IContent content)
            where T : IContent;

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">If the rootname is already registered with another contentRootId.</exception>
        void InitSettings();

        /// <summary>
        /// Updates the settings.
        /// </summary>
        /// <param name="content">The content.</param>
        void UpdateSettings(IContent content);
    }
}