using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.VisualStudioTools.Project;
using NPLTools.Project;

namespace NPLTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(NPLProjectPackage.PackageGuidString)]
    [ProvideProjectFactory(typeof(NPLProjectFactory), null,
    "NPL Project Files (*.nplproj);*.nplproj", "nplproj", "nplproj",
    ".\\NullPath", LanguageVsTemplate = "NPL")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class NPLProjectPackage : CommonProjectPackage
    {
        /// <summary>
        /// NPLProjectPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6ca69e75-b036-43b4-8786-9107c8ae2ca9";

        /// <summary>
        /// Initializes a new instance of the <see cref="NPLProjectPackage"/> class.
        /// </summary>
        public NPLProjectPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new NPLProjectFactory(this));
        }

        #endregion

        #region CommonProjectPackage Abstracts Methods
        public override CommonEditorFactory CreateEditorFactory()
        {
            return null;
        }

        public override ProjectFactory CreateProjectFactory()
        {
            return new NPLProjectFactory(this);
        }

        public override uint GetIconIdForAboutBox()
        {
            return 400;
        }

        public override uint GetIconIdForSplashScreen()
        {
            return 300;
        }

        public override string GetProductDescription()
        {
            return "NPL Tools";
        }

        public override string GetProductName()
        {
            return "NPL Tools";
        }

        public override string GetProductVersion()
        {
            return "1.0";
        }
        #endregion


    }
}
