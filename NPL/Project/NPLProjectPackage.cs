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
using Microsoft.VisualStudioTools;

namespace NPLTools.Project
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideProjectFactory(typeof(NPLProjectFactory), null,
    "NPL Project Files (*.nplproj);*.nplproj", "nplproj", "nplproj",
    @"Templates\Projects\WebApplication", LanguageVsTemplate = "NPL")]
    [ProvideProjectItem(typeof(NPLProjectFactory), "NPL Items", ".\\NullPath", 500)]
    [Guid(NPLProjectPackageGuidString)]
    [ProvideObject(typeof(NPLPropertyPage))]
    [DeveloperActivity("NPL", typeof(NPLProjectPackage))]
    public sealed class NPLProjectPackage : CommonProjectPackage
    {
        /// <summary>
        /// NPLProjectPackage GUID string.
        /// </summary>
        public const string NPLProjectPackageGuidString = "341ba1ac-a1ce-4ab3-b281-ae5dc002f09a";

        public string nplExePath;

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

        //public override string ProductUserContext
        //{
        //    get { return ""; }
        //}

        public override CommonEditorFactory CreateEditorFactory()
        {
            return null;
        }

        public override ProjectFactory CreateProjectFactory()
        {
            return new NPLProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactoryPromptForEncoding()
        {
            return null;
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
            return "npl";
        }

        public override string GetProductName()
        {
            return "npl";
        }

        public override string GetProductVersion()
        {
            return "1.0";
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new NPLProjectFactory(this));
        }

        public EnvDTE.DTE DTE
        {
            get { return GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE; }
        }

        #endregion
    }
}
