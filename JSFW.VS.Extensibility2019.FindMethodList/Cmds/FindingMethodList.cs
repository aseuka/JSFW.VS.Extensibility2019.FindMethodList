using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Linq;
using JSFW.VS.Extensibility.Cmds.Controls;
using EnvDTE;

namespace JSFW.VS.Extensibility.Cmds
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FindingMethodList
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9d0daf78-a05a-4197-8a43-fc0dd7c61c2d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindingMethodListCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FindingMethodList(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FindingMethodList Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new FindingMethodList(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "FindingMethodListCommand";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);


            try
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    EnvDTE80.DTE2 _applicationObject = ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

                    /////*
                    //// activedocumentchanged, documentsaved 이벤트에다가만 걸어버리면..
                    //// */
                    ////if (_applicationObject.ActiveDocument != null)
                    ////{
                    ////    //Get active document 
                    ////    EnvDTE.TextDocument textDoc = (EnvDTE.TextDocument)_applicationObject.ActiveDocument.Object("");
                    ////    if (textDoc != null)
                    ////    {
                    ////        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    ////        string keys = "";
                    ////        foreach (EnvDTE.Command cmd in _applicationObject.Commands)
                    ////        {
                    ////            keys = "";
                    ////            if (cmd.Bindings != null && cmd.Bindings is Array && 0 < ((Array)cmd.Bindings).Length)
                    ////            {
                    ////                foreach (var bindItem in ((Array)cmd.Bindings))
                    ////                {
                    ////                    keys += bindItem + " || ";
                    ////                }
                    ////            }
                    ////            sb.AppendFormat(@"Name={0}, ID={1}, Guid={2}, Key={3}", cmd.Name, cmd.ID, cmd.Guid, keys);
                    ////            sb.AppendLine();
                    ////        }
                    ////        textDoc.StartPoint.CreateEditPoint();
                    ////        textDoc.Selection.Insert("" + sb, (int)EnvDTE.vsInsertFlags.vsInsertFlagsContainNewText);
                    ////        textDoc = null;
                    ////    }
                    ////} 

                    //Check active document

                    if (_applicationObject.ActiveDocument != null)
                    { 
                        var codeElems = FindMthListEx.Descendants(_applicationObject.ActiveDocument.ProjectItem.FileCodeModel, EnvDTE.vsCMElement.vsCMElementFunction)                                                        
                                                     .Cast<EnvDTE.CodeFunction>()
                                                     .Select(o =>  new MethodList.MethodCodeFunctionObject
                                                         {
                                                             FunctionKind = o.FunctionKind,
                                                             FullName = o.FullName,
                                                             Name = o.Name,
                                                             Line = o.StartPoint.Line,
                                                             Element = o,
                                                             Comment = o.Comment,
                                                             DocComment = o.DocComment,
                                                         }).OrderBy(o => o.Name).ToList();
                        EnvDTE.TextDocument objTextDocument = (EnvDTE.TextDocument)_applicationObject.ActiveDocument.Object("");
                        EnvDTE.TextSelection objTextSelection = objTextDocument.Selection;
                        using (MethodList mm = new MethodList())
                        {
                            string prjName = "";
                            Array _projects = _applicationObject.ActiveSolutionProjects as Array;
                            if (_projects.Length != 0 && _projects != null)
                            {
                                Project _selectedProject = _projects.GetValue(0) as Project;
                                prjName = _selectedProject.Name;
                            }
                            mm.SetProjectName(prjName);
                            mm.SetMethodList(codeElems, objTextSelection);
                            mm.ShowDialog();
                        }
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
    }

    static class FindMthListEx
    {
        public static IEnumerable<EnvDTE.CodeElement> Descendants(this EnvDTE.FileCodeModel fileCodeModel, EnvDTE.vsCMElement? childrenType = null)
        {
            var classes = new System.Collections.Generic.List<EnvDTE.CodeElement>();

            foreach (EnvDTE.CodeElement elem in fileCodeModel.CodeElements)
            {
                if (elem.Kind == childrenType && (childrenType == EnvDTE.vsCMElement.vsCMElementNamespace || childrenType == EnvDTE.vsCMElement.vsCMElementImportStmt))
                {
                    yield return elem;
                }
                else
                {
                    switch (elem.Kind)
                    {
                        case EnvDTE.vsCMElement.vsCMElementNamespace:
                        case EnvDTE.vsCMElement.vsCMElementClass:
                            classes.AddRange(Descendants(elem, EnvDTE.vsCMElement.vsCMElementClass, elem.Kind == EnvDTE.vsCMElement.vsCMElementClass));
                            break;
                    }
                }
            }

            foreach (EnvDTE.CodeElement elem in classes)
            {
                if (childrenType != null && childrenType == EnvDTE.vsCMElement.vsCMElementClass)
                {
                    yield return elem;
                }
                else
                {
                    if (childrenType == null)
                    {
                        yield return elem;
                    }

                    foreach (EnvDTE.CodeElement c in elem.Children)
                    {
                        if (c.Kind != EnvDTE.vsCMElement.vsCMElementClass)
                        {
                            if (childrenType == null || c.Kind == childrenType)
                            {
                                yield return c;
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<EnvDTE.CodeElement> Descendants(this EnvDTE.CodeElement source, EnvDTE.vsCMElement? childrenType = null, bool includeSelf = false)
        {
            var children = new List<EnvDTE.CodeElement>();

            if (includeSelf && childrenType != null && source.Kind == childrenType.Value)
            {
                children.Add(source);
            }

            foreach (EnvDTE.CodeElement elem in source.Children)
            {
                if (childrenType == null || elem.Kind == childrenType.Value)
                {
                    children.Add(elem);
                }

                if (elem.Children != null && elem.Children.Count > 0)
                {
                    children.AddRange(Descendants(elem, childrenType, false));
                }
            }

            return children;
        }



    }
}
