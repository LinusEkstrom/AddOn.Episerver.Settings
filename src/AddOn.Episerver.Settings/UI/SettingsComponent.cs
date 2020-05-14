// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsComponent.cs" company="none">
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

namespace AddOn.Episerver.Settings.UI
{
    using EPiServer.Shell;
    using EPiServer.Shell.ViewComposition;

    /// <summary>
    /// Component that provides a tree based navigation for CMS pages.
    /// Implements the <see cref="ComponentDefinitionBase" />
    /// </summary>
    /// <seealso cref="ComponentDefinitionBase" />
    [Component]
    public sealed class SettingsComponent : ComponentDefinitionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsComponent"/> class.
        /// </summary>
        public SettingsComponent()
            : base("epi-cms/component/SharedBlocks")
        {
            this.LanguagePath = "/episerver/cms/components/settings";
            this.Title = "Settings";
            this.SortOrder = 200;
            this.Categories = new string[]
                                  {
                                      "cms"
                                  };
            this.PlugInAreas = new[] { PlugInArea.AssetsDefaultGroup };
            this.Settings.Add(new Setting("repositoryKey", value: SettingsRepositoryDescriptor.RepositoryKey));
        }
    }
}