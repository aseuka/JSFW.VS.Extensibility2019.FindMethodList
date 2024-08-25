using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace JSFW.VS.Extensibility.FindMethodList
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
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "15a082a4-4679-4da7-bfd5-f588c9fe70f0";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members


        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private System.IServiceProvider ServiceProvider
        {
            get
            {
                return this;
            }
        }


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Cmds.FindingMethodList.Initialize(this); 
            ResetKeyBinding_QuickLunch();
        }

        private void ResetKeyBinding_QuickLunch()
        {
            /*
              ## 2017에서 빠른실행 (Ctrl + Q )가 포커스를 땡겨가서 포맷터가 단축키로 활용이 안되어 재설정.
             
              # 제거 
                 Window.ActivateQuickLaunch 
            */

            // VSTHRD010 메인 스레드에서 단일 스레드 유형 호출
            // 커맨드... 변경 곳에서 안보이던 경고메세지들이 눈에 띈다. 
            // 2022로 소스를 열었더니.... 
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                /*UI code here*/
                var _applicationObject = ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                foreach (EnvDTE.Command cmd in _applicationObject.Commands)
                {
                    switch (cmd.Name)
                    {
                        default: break;

                        case "Window.ActivateQuickLaunch": // 빠른실행
                            cmd.Bindings = new object[] { }; // 제거
                            break;

                        case "EditorContextMenus.CodeWindow.메소드찾아가기":
                            cmd.Bindings = new object[] { "전역::CTRL+1" };
                            break;
                    }
                }

            }); 
            
        }
        #endregion
    }
}
