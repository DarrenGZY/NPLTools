using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;
using NPLTools.Language;
using NPLTools.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using EnvDTE;
using Microsoft.VisualStudioTools.Project;

namespace NPLTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.NPLPackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideLanguageService(typeof(NPLLanguageInfo), NPLConstants.NPLExtension, 106, RequestStockColors = true, ShowSmartIndent = true, ShowCompletion = true, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = true, EnableAdvancedMembersOption = true, ShowDropDownOptions = true)]
    //[ProvideLanguageExtension(typeof(NPLLanguageInfo), ".npl")]
    //[ProvideEditorExtension2(typeof(NPLEditorFactory), NPLConstants.NPLFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = LuaConstants.ProjectFactoryGuid, NameResourceID = 3004, DefaultName = "module")]
    //[ProvideEditorExtension2(typeof(NPLEditorFactory), NPLConstants.LuaFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = LuaConstants.ProjectFactoryGuid, NameResourceID = 3004, DefaultName = "module")]
    //[ProvideLanguageExtension(typeof(NPLEditorFactory), NPLConstants.NPLFileExtension)]
    //[ProvideLanguageExtension(typeof(NPLEditorFactory), NPLConstants.LuaFileExtension)]
    //[ProvideAutoLoad(UIContextGuids80.NoSolution)]
    //[ProvideBraceCompletion(NPLConstants.ContentType)]
    public sealed class NPLToolsPackage : CommonPackage
    {
        public static NPLToolsPackage Instance;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NPLPackage"/> class.
        /// </summary>
        public NPLToolsPackage()
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
        }

        private void NPLPackage_OnIdle(object sender, ComponentManagerEventArgs e)
        {
            Debug.WriteLine("On Idle -----------------------------------------------");
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(String);
        }

        internal override LibraryManager CreateLibraryManager(CommonPackage package)
        {
            throw new NotImplementedException();
        }

        public override bool IsRecognizedFile(string filename)
        {
            throw new NotImplementedException();
        }


        internal static void NavigateTo(System.IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int line, int col)
        {
            VsUtilities.NavigateTo(serviceProvider, filename, docViewGuidType, line, col);
        }

        internal static void NavigateTo(System.IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int pos)
        {
            VsUtilities.NavigateTo(serviceProvider, filename, docViewGuidType, pos);
        }

        #endregion
    }
}
