// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsBase.cs" company="none">
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

using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Class SettingsBase.
///     Implements the <see cref="BasicContent" />
///     Implements the <see cref="IVersionable" />
/// </summary>
/// <seealso cref="BasicContent" />
/// <seealso cref="IVersionable" />
[ContentType(GUID = "484DAD32-3E16-4943-B7BF-A542C7BDC379", AvailableInEditMode = false)]
public class SettingsBase : BasicContent, IVersionable, IContentSecurable
{
    private readonly Injected<IContentLoader> _contentLoader;
    
    /// <summary>
    ///     Gets or sets a value indicating whether this item is in pending publish state.
    /// </summary>
    /// <value><c>true</c> if this instance is in pending publish state; otherwise, <c>false</c>.</value>
    public bool IsPendingPublish { get; set; }

    /// <summary>
    ///     Gets or sets the start publish date for this item.
    /// </summary>
    /// <value>The start publish.</value>
    public DateTime? StartPublish { get; set; }

    /// <summary>
    ///     Gets or sets the version status of this item.
    /// </summary>
    /// <value>The status.</value>
    public VersionStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the stop publish date for this item.
    /// </summary>
    /// <value>The stop publish.</value>
    public DateTime? StopPublish { get; set; }

    /// <summary>
    ///     Gets the security descriptor for this item.
    /// </summary>
    /// <returns>The security descriptor.</returns>
    public ISecurityDescriptor GetSecurityDescriptor()
    {
        return GetContentSecurityDescriptor();
    }
    
    /// <summary>
    ///     Gets the content security descriptor for this item.
    /// </summary>
    /// <returns>The content security descriptor.</returns>
    public IContentSecurityDescriptor GetContentSecurityDescriptor()
    {
        // return any list of valid ACL, for example from Root or StartPage which those settings belong to
        var contentRef = ContentReference.IsNullOrEmpty(ContentReference.StartPage) ? ContentReference.RootPage : ContentReference.StartPage;
        var accessControlList = (_contentLoader.Service.Get<IContent>(contentRef) as PageData)?.ACL;
        var writableAccessControlList = accessControlList?.CreateWritableClone();
        // If the ACL is inherited the ACL isn't allowed to be modified.
        if (writableAccessControlList is not null && writableAccessControlList.IsInherited) { 
            writableAccessControlList.IsInherited = false;
        }
        return writableAccessControlList as IContentSecurityDescriptor;
    }
}

