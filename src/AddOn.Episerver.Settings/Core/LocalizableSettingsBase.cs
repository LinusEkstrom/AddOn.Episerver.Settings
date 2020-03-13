﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalizableSettingsBase.cs" company="none">
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
    using System.Collections.Generic;
    using System.Globalization;

    using EPiServer.Core;

    /// <summary>
    /// Class LocalizableSettingsBase.
    /// Implements the <see cref="SettingsBase" />
    /// Implements the <see cref="EPiServer.Core.ILocalizable" />
    /// </summary>
    /// <seealso cref="SettingsBase" />
    /// <seealso cref="EPiServer.Core.ILocalizable" />
    public class LocalizableSettingsBase : SettingsBase, ILocalizable
    {
        /// <summary>
        /// Gets or sets the existing languages for the <see cref="T:EPiServer.Core.ContentData" />
        /// </summary>
        /// <value>The existing languages.</value>
        public IEnumerable<CultureInfo> ExistingLanguages { get; set; }

        /// <summary>
        /// Gets or sets the language for this instance (typically a <see cref="T:EPiServer.Core.ContentData" /> instance).
        /// </summary>
        /// <value>The language.</value>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the master language for this <see cref="T:EPiServer.Core.ContentData" /> instance.
        /// </summary>
        /// <value>The master language.</value>
        public CultureInfo MasterLanguage { get; set; }
    }
}