//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;

namespace SettingsEditor
{
    /// <summary>
    /// Command component that defines Timeline-specific commands. All of these are
    /// popup commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(Commands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Commands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
		public Commands( ICommandService commandService, IContextRegistry contextRegistry, IDocumentService documentService )
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
			m_documentService = documentService;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering timeline commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.ReloadDocument,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Reload Document",
                "Reloads Document",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return DoCommand(commandTag, false);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            DoCommand(commandTag, true);
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
			//Document document = m_contextRegistry.GetActiveContext<Document>();
			//if (document == null)
			//	return;

			//if (commandTag is Command)
			//{
			//	switch ((Command)commandTag)
			//	{
			//		case Command.ReloadDocument:
			//			commandState.Check = document.SplitManipulator != null ? document.SplitManipulator.Active : false;
			//			break;
			//	}
			//}
        }

        #endregion

        private bool DoCommand(object commandTag, bool doing)
        {
			DocumentEditingContext context = m_contextRegistry.GetActiveContext<DocumentEditingContext>();
            if (context == null)
                return false;

			Document document = context.As<Document>();
            if (document == null)
                return false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.ReloadDocument:

                        if (doing)
                        {
							m_editor.Reload( document );
                        }
                        return true;
                }
            }

            return false;
        }

        private enum Command
        {
            ReloadDocument
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
		private IDocumentService m_documentService;

		[Import( AllowDefault = false )]
		private Editor m_editor = null;
	}
}
