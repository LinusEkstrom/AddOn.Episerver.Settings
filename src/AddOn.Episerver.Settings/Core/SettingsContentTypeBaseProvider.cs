// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContentTypeBaseProvider.cs" company="none">
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

using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Provides the content type base used by settings content.
/// </summary>
[ServiceConfiguration(typeof(IContentTypeBaseProvider), Lifecycle = ServiceInstanceScope.Singleton)]
public class SettingsContentTypeBaseProvider : IContentTypeBaseProvider
{
    private static readonly ContentTypeBase SettingContentType = new("Setting");

    /// <summary>
    ///     Gets the content type bases handled by this provider.
    /// </summary>
    public IEnumerable<ContentTypeBase> ContentTypeBases => new[] { SettingContentType };

    /// <summary>
    ///     Resolves the CLR type for a content type base.
    /// </summary>
    /// <param name="contentTypeBase">The content type base.</param>
    /// <returns>The CLR type for the content type base.</returns>
    public Type Resolve(ContentTypeBase contentTypeBase)
    {
        return typeof(SettingsBase);
    }
}
