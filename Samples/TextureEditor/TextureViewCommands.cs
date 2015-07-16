//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using System;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System.IO;
using Sce.Atf.Controls;

namespace TextureEditor
{   
    /// <summary>
    /// A MEF component for providing user commands related to the RenderView component</summary>
    //[Export(typeof(IInitializable))]
    //[Export(typeof(TextureViewCommands))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class TextureViewCommands : ICommandClient
    {
		public TextureViewCommands( ICommandService commandService, TexturePreviewWindowSharpDX panel3D, IWin32Window owner, SchemaLoader schemaLoader )
        {
            m_previewWindow = panel3D;
			m_owner = owner;
			m_schemaLoader = schemaLoader;

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

			commandService.RegisterCommand(
			   Command.ExportOne,
			   StandardMenu.File,
			   CommandGroup,
			   "Export One",
			   "Exports currently selected texture",
			   Keys.F,
				//null,
			   Resources.ComponentImage,
			   CommandVisibility.Default,
			   this );

			commandService.RegisterCommand(
			   Command.ExpartAll,
			   StandardMenu.File,
			   CommandGroup,
			   "Export All",
			   "Exports all textures",
			   Keys.F,
				//null,
			   Resources.ComponentsImage,
			   CommandVisibility.Default,
			   this );
		}

        /// <summary>
        /// Rendering modes</summary>
        protected enum Command
        {
            FitInWindow,
			FitSize,
			ExportOne,
			ExpartAll
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

            if (m_previewWindow == null)
                return false;
            
            switch ((Command)commandTag)
            {
                case Command.FitInWindow:
                    return true;
				case Command.FitSize:
					return true;
				case Command.ExportOne:
					return m_previewWindow.SelectedTexture != null;
				case Command.ExpartAll:
					return true;
			}

            return false;
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (m_previewWindow == null)
                return; 
            
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.FitInWindow:
                        m_previewWindow.fitInWindow();
                        break;
					case Command.FitSize:
						m_previewWindow.fitSize();
						break;
					case Command.ExportOne:
						ExportOne();
						break;
					case Command.ExpartAll:
						ExportAll();
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
            if (m_previewWindow == null)
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

		void ExportOne()
		{
			TextureProperties tp = m_previewWindow.SelectedTexture;
			ExportUri( tp.FileUri );
		}
		void ExportAll()
		{
		}

		void ExportUri( Uri resourceUri )
		{
			string metadataFilePath = resourceUri.LocalPath + ".metadata";
			Uri metadataUri = new Uri( metadataFilePath );

			if ( File.Exists( metadataFilePath ) )
			{
				// read existing metadata
				using ( FileStream stream = File.OpenRead( metadataFilePath ) )
				{
					var reader = new DomXmlReader( m_schemaLoader );
					Sce.Atf.Dom.DomNode rootNode = reader.Read( stream, metadataUri );
					rootNode.InitializeExtensions();

					TextureMetadata tm = rootNode.As<TextureMetadata>();

					string cmd = " -if CUBIC";
					if ( tm.Width > 0 )
						cmd += " -w " + tm.Width;
					if ( tm.Height > 0 )
						cmd += " -h " + tm.Height;

					if ( !tm.GenMipMaps )
						cmd += " -miplevels 0";

					if ( tm.ForceSourceSrgb )
						cmd += " -srgbi";

					if ( tm.ExtendedFormat != SharpDX.DXGI.Format.Unknown )
						cmd += " -f " + tm.ExtendedFormat.ToString();
					else if ( tm.Format != SharpDX.DXGI.Format.Unknown )
						cmd += " -f " + tm.Format.ToString();
					else
					{
						throw new Exception("Invalid format");
					}

					string file_metadata = Path.GetFullPath( resourceUri.AbsolutePath );
					string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
					string dir_data = PICO_DEMO + "data\\";
					string dir_dataWin = PICO_DEMO + "dataWin\\";

					string file = file_metadata.Replace( dir_data, dir_dataWin );

					cmd += " -of " + file;
					cmd += " " + resourceUri.AbsolutePath;

					System.Diagnostics.Process process = new System.Diagnostics.Process();
					System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
					//startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
					string PICO_ROOT = Environment.GetEnvironmentVariable( "PICO_ROOT" );
					startInfo.FileName = PICO_ROOT + "bin64\\texconv.exe";
					startInfo.Arguments = cmd;
					process.StartInfo = startInfo;
					process.Start();
					process.WaitForExit();
				}
			}
			else
			{
				//throw new Exception( "File not found" );
				//m_errorDialogService.Write( OutputMessageType.Error, "Please configure texture's metadata first!" );
				//ErrorDialog errDialog = new ErrorDialog();
				//errDialog.StartPosition = FormStartPosition.CenterScreen;
				//errDialog.Text = "Error!".Localize();

				//string message = "Please configure texture's metadata first!";
				//errDialog.MessageId = message;
				//errDialog.Message = message;
				//errDialog.Visible = false; //Just in case a second error message comes through, because...
				//errDialog.Show( m_owner ); //if Visible is true, Show() crashes.

				MessageBox.Show( m_owner, "Please configure texture's metadata first!", "Error", MessageBoxButtons.OK );
			}
		}

		//[Import( AllowDefault = true )]
		private IWin32Window m_owner;

		//[Import( AllowDefault = false )]
		private SchemaLoader m_schemaLoader = null;

		//[Import( AllowDefault = false )]
		//private ErrorDialogService m_errorDialogService = null;

        //[Import(AllowDefault=false)]
        //private ICommandService m_commandService;

        //[Import(AllowDefault=false)]
        //private RenderView m_renderView;
        private TexturePreviewWindowSharpDX m_previewWindow;

        private static string CommandGroup = "TexturePreviewCommands";
    }
}
