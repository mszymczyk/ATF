//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;

namespace TextureEditor
{   
    /// <summary>
    /// A MEF component for providing user commands related to the RenderView component</summary>
    //[Export(typeof(IInitializable))]
    //[Export(typeof(TextureViewCommands))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class TextureViewCommands : ICommandClient//, IInitializable
    {
        public TextureViewCommands(ICommandService commandService, TexturePreviewWindowSharpDX panel3D)
        {
            m_panel3D = panel3D;

            commandService.RegisterCommand(
               Command.FitInWindow,
               StandardMenu.View,
               CommandGroup,
               "Fit In Window",
               "Fits texture to cover whole window",
               Keys.F,
               //null,
               Resources.FitToSizeImage,
               CommandVisibility.Default,
               this);
		
			commandService.RegisterCommand(
			   Command.FitSize,
			   StandardMenu.View,
			   CommandGroup,
			   "Fit Size",
			   "Resizes texture to it's original size",
			   Keys.F,
				//null,
			   Resources.PinGreyImage,
			   CommandVisibility.Default,
			   this);
		}

        /// <summary>
        /// Rendering modes</summary>
        protected enum Command
        {
            FitInWindow,
			FitSize
        }

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            if (m_panel3D == null)
                return false;
            
            switch ((Command)commandTag)
            {
                case Command.FitInWindow:
                    return true;
				case Command.FitSize:
					return true;
			}

            return false;
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (m_panel3D == null)
                return; 
            
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.FitInWindow:
                        m_panel3D.fitInWindow();
                        break;
					case Command.FitSize:
						m_panel3D.fitSize();
						break;
				}
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
            if (m_panel3D == null)
                return;

            //if (commandTag is Command)
            //{
            //    switch ((Command)commandTag)
            //    {
            //        case Command.RenderSmooth:
            //            state.Check = (activeControl.RenderState.RenderMode & RenderMode.Smooth) != 0;
            //            break;
            //    }
            //}
        }

        #endregion

        //[Import(AllowDefault=false)]
        //private ICommandService m_commandService;

        //[Import(AllowDefault=false)]
        //private RenderView m_renderView;
        private TexturePreviewWindowSharpDX m_panel3D;

        private static string CommandGroup = "TexturePreviewCommands";
    }
}
