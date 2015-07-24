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
using SharpDX.DXGI;
using System.Threading;
using System.Collections.Generic;

namespace TextureEditor
{   
    /// <summary>
    /// A MEF component for providing user commands related to the RenderView component</summary>
    //[Export(typeof(IInitializable))]
    //[Export(typeof(TextureViewCommands))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class TextureViewCommands : ICommandClient
    {
		public TextureViewCommands( ICommandService commandService, TexturePreviewWindowSharpDX panel3D, MainForm mainForm, SchemaLoader schemaLoader )
        {
            m_previewWindow = panel3D;
			m_mainForm = mainForm;
			m_schemaLoader = schemaLoader;

            commandService.RegisterCommand(
               Command.FitInWindow,
               StandardMenu.View,
               CommandGroup,
               "Fit In Window",
               "Fits texture to cover whole window",
               Keys.F,
               //null,
			   Sce.Atf.Resources.FitToSizeImage,
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
			   Sce.Atf.Resources.PinGreyImage,
			   CommandVisibility.Default,
			   this);

			commandService.RegisterCommand(
				Command.ShowSource,
				StandardMenu.View,
				CommandGroup,
				"Show source",
				"Displays source texture",
				Keys.None,
				Resources.SourceImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowExported,
				StandardMenu.View,
				CommandGroup,
				"Show exported",
				"Displays exported texture",
				Keys.None,
				Resources.ExportedImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowDiff,
				StandardMenu.View,
				CommandGroup,
				"Show difference",
				"Displays difference between source and exported image",
				Keys.None,
				Resources.DifferenceImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
			   Command.ExportOne,
			   StandardMenu.File,
			   CommandGroup,
			   "Export One",
			   "Exports currently selected texture",
			   Keys.F,
				//null,
			   Sce.Atf.Resources.ComponentImage,
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
			   Sce.Atf.Resources.ComponentsImage,
			   CommandVisibility.Default,
			   this );
		}

        /// <summary>
        /// Rendering modes</summary>
        protected enum Command
        {
            FitInWindow,
			FitSize,
			ShowSource,
			ShowExported,
			ShowDiff,
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
				case Command.ShowSource:
					return true;
				case Command.ShowExported:
				case Command.ShowDiff:
					{
						if ( m_previewWindow.SelectedTexture != null )
							return m_previewWindow.SelectedTexture.ExportedTexture != null;
						return false;
					}
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
					case Command.ShowSource:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Source;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowExported:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Exported;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowDiff:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Difference;
						m_previewWindow.Invalidate();
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

			if ( commandTag is Command )
			{
				switch ( (Command)commandTag )
				{
					//case Command.RenderSmooth:
					//	state.Check = ( activeControl.RenderState.RenderMode & RenderMode.Smooth ) != 0;
					//	break;
					case Command.ShowSource:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Source;
						break;
					case Command.ShowExported:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Exported;
						break;
					case Command.ShowDiff:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Difference;
						break;
				}
			}
        }

        #endregion

		void ExportOne()
		{
			TextureProperties tp = m_previewWindow.SelectedTexture;
			//ExportUri( tp.FileUri );
			TextureExporter te = new TextureExporter( m_mainForm, m_schemaLoader );
			te.ExportOne( new Uri(tp.FileUri.LocalPath + ".metadata") );
			//ProgressOutputWindow powin = new ProgressOutputWindow();
			//BackgroundThread bgThread = new BackgroundThread( powin, tp.FileUri );
			//powin.ShowDialog( m_owner );
			//bgThread.Wait();
		}

		void ExportAll()
		{
			TextureExporter te = new TextureExporter( m_mainForm, m_schemaLoader );
			te.ExportAll();
		}

		//void DoExport( List<string> fileList )
		//{
		//}

		//int ExportUri( Uri resourceUri )
		//{
		//	string metadataFilePath = resourceUri.LocalPath + ".metadata";
		//	Uri metadataUri = new Uri( metadataFilePath );

		//	if ( File.Exists( metadataFilePath ) )
		//	{
		//		// read existing metadata
		//		using ( FileStream stream = File.OpenRead( metadataFilePath ) )
		//		{
		//			var reader = new DomXmlReader( m_schemaLoader );
		//			Sce.Atf.Dom.DomNode rootNode = reader.Read( stream, metadataUri );
		//			rootNode.InitializeExtensions();

		//			string inputFile = Path.GetFullPath( resourceUri.AbsolutePath );
		//			string dir_data = PICO_DEMO + "data\\";
		//			string dir_dataWin = PICO_DEMO + "dataWin\\";
		//			string outputFileWin_tmp = inputFile.Replace( dir_data, dir_dataWin );
		//			string outputFileWin = outputFileWin_tmp + ".dds";

		//			TextureMetadata tm = rootNode.As<TextureMetadata>();

		//			if ( tm.CopySourceFile )
		//			{
		//				System.IO.File.Copy( inputFile, outputFileWin, true );
		//			}
		//			else
		//			{
		//				string cmd = " -if CUBIC";
		//				if ( tm.Width > 0 )
		//					cmd += " -w " + tm.Width;
		//				if ( tm.Height > 0 )
		//					cmd += " -h " + tm.Height;

		//				if ( !tm.GenMipMaps )
		//					cmd += " -miplevels 0";

		//				if ( tm.ForceSourceSrgb )
		//					cmd += " -srgbi";

		//				Format format = Format.Unknown;

		//				if ( tm.ExtendedFormat != SharpDX.DXGI.Format.Unknown )
		//					//cmd += " -f " + tm.ExtendedFormat.ToString();
		//					format = tm.ExtendedFormat;
		//				else if ( tm.Format != SharpDX.DXGI.Format.Unknown )
		//					//cmd += " -f " + tm.Format.ToString();
		//					format = tm.Format;
		//				else
		//				{
		//					throw new Exception( "Invalid format" );
		//				}

		//				bool bcSrgbFormat = false;
		//				// texconv incorrectly generates mipmaps for compressed formats
		//				// so first resize/genmips to uncompressed file, and then compress
		//				//
		//				if ( tm.GenMipMaps &&
		//					(
		//					   format == Format.BC1_UNorm_SRgb
		//					|| format == Format.BC2_UNorm_SRgb
		//					|| format == Format.BC3_UNorm_SRgb
		//					|| format == Format.BC7_UNorm_SRgb
		//					)
		//					)
		//				{
		//					bcSrgbFormat = true;
		//					cmd += " -f " + Format.R8G8B8A8_UNorm_SRgb.ToString();
		//				}
		//				else
		//				{
		//					cmd += " -f " + format.ToString();
		//				}

		//				cmd += " -of " + outputFileWin;
		//				cmd += " " + inputFile;

		//				int ires = RunCommand_texconv( cmd );
		//				if ( ires != 0 )
		//				{
		//					return ires;
		//				}

		//				if ( bcSrgbFormat )
		//				{
		//					string cmd2 = " -f " + format.ToString();
		//					cmd2 += " -of " + outputFileWin;
		//					cmd2 += " " + outputFileWin;

		//					ires = RunCommand_texconv( cmd2 );
		//					if ( ires != 0 )
		//					{
		//						return ires;
		//					}
		//				}
		//			}

		//			{
		//				string dir_dataPS4 = PICO_DEMO + "dataPS4\\";
		//				string outputFilePS4_tmp = inputFile.Replace( dir_data, dir_dataWin );
		//				string outputFilePS4 = outputFileWin_tmp + ".gnf";
		//				string arg = "-f Auto ";
		//				arg += " -i \"" + outputFileWin + "\" ";
		//				arg += " -o \"" + outputFilePS4 + "\" ";

		//				RunCommand_orbis_image2gnf( arg );
		//			}
		//		}

		//		return 0;
		//	}
		//	else
		//	{
		//		//throw new Exception( "File not found" );
		//		//m_errorDialogService.Write( OutputMessageType.Error, "Please configure texture's metadata first!" );
		//		//ErrorDialog errDialog = new ErrorDialog();
		//		//errDialog.StartPosition = FormStartPosition.CenterScreen;
		//		//errDialog.Text = "Error!".Localize();

		//		//string message = "Please configure texture's metadata first!";
		//		//errDialog.MessageId = message;
		//		//errDialog.Message = message;
		//		//errDialog.Visible = false; //Just in case a second error message comes through, because...
		//		//errDialog.Show( m_owner ); //if Visible is true, Show() crashes.

		//		//MessageBox.Show( m_owner, "Please configure texture's metadata first!", "Error", MessageBoxButtons.OK );
		//		return 1;
		//	}
		//}

		//int RunCommand_texconv( string arg )
		//{
		//	System.Diagnostics.Process process = new System.Diagnostics.Process();
		//	System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		//	//startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		//	startInfo.FileName = texconv_exe;
		//	startInfo.Arguments = arg;
		//	process.StartInfo = startInfo;
		//	process.Start();
		//	process.WaitForExit();

		//	return process.ExitCode;
		//}
		//int RunCommand_orbis_image2gnf( string arg )
		//{
		//	System.Diagnostics.Process process = new System.Diagnostics.Process();
		//	System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		//	//startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		//	startInfo.FileName = "orbis-image2gnf.exe";
		//	startInfo.Arguments = arg;
		//	process.StartInfo = startInfo;
		//	process.Start();
		//	process.WaitForExit();

		//	return process.ExitCode;
		//}


		//private class BackgroundThread
		//{
		//	private readonly ProgressOutputWindow m_progressWindow;
		//	private readonly Thread m_thread;
		//	private bool m_alreadyStopped;
		//	private Uri m_fileToExport;

		//	public BackgroundThread( ProgressOutputWindow parent, Uri fileToExport )
		//	{
		//		m_progressWindow = parent;
		//		m_fileToExport = fileToExport;
		//		m_thread = new Thread( Run );
		//		m_thread.Name = "progress dialog";
		//		m_thread.IsBackground = true; //so that the thread can be killed if app dies.
		//		//m_thread.SetApartmentState( ApartmentState.STA );
		//		m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
		//		m_thread.Start();
		//	}

		//	public void Stop()
		//	{
		//		lock ( this )
		//		{
		//			m_alreadyStopped = true;
		//		}
		//	}

		//	public void Wait()
		//	{
		//		m_thread.Join();
		//	}

		//	//public void UpdateLabel()
		//	//{
		//	//	lock ( this )
		//	//	{
		//	//		if ( m_dialog != null && m_dialog.IsHandleCreated )
		//	//			m_dialog.BeginInvoke( new MethodInvoker( ThreadUnsafeUpdate ) );
		//	//	}
		//	//}

		//	//public void UpdateLocation()
		//	//{
		//	//	lock ( this )
		//	//	{
		//	//		if ( m_dialog != null && m_dialog.IsHandleCreated )
		//	//			m_dialog.BeginInvoke( new MethodInvoker( ThreadUnsafeUpdateLocation ) );
		//	//	}
		//	//}

		//	private void Run()
		//	{
		//		try
		//		{
		//			//lock ( this )
		//			//{
		//			//	if ( !m_alreadyStopped )
		//			//	{
		//			//		m_progressWindow.Close();
		//			//		return;
		//			//	}
		//			//}

		//			if ( m_fileToExport != null )
		//			{

		//			}

		//			AddInfo( "Hej!\n" );
		//			SetProgress( 0.1f );
		//			System.Threading.Thread.Sleep( 1000 );
		//			SetProgress( 0.5f );
		//			AddError( "Ho!\n" );
		//			System.Threading.Thread.Sleep( 1000 );
		//			SetProgress( 1.0f );
		//			Done();
		//		}
		//		finally
		//		{
		//			//lock ( this )
		//			//{
		//			//}
		//		}
		//	}

		//	void AddInfo( string str )
		//	{
		//		lock(m_progressWindow)
		//		{
		//			//m_progressWindow.AddInfo( str );
		//			//m_progressWindow.BeginInvoke( new MethodInvoker( AddInfoThreadUnsafe ), new[] { str } );
		//			m_progressWindow.BeginInvoke( new MethodInvoker( () => this.AddInfoThreadUnsafe(str) ) );
		//		}
		//	}

		//	void AddError( string str )
		//	{
		//		lock ( m_progressWindow )
		//		{
		//			//m_progressWindow.AddError( str );
		//			m_progressWindow.BeginInvoke( new MethodInvoker( () => this.AddErrorThreadUnsafe( str ) ) );
		//		}
		//	}

		//	void Done()
		//	{
		//		lock ( m_progressWindow )
		//		{
		//			m_progressWindow.BeginInvoke( new MethodInvoker(DoneThreadUnsafe) );
		//		}
		//	}

		//	void SetProgress( float progress )
		//	{
		//		lock ( m_progressWindow )
		//		{
		//			m_progressWindow.BeginInvoke( new MethodInvoker( () => this.SetProgressThreadUnsafe( progress ) ) );
		//		}
		//	}

		//	void AddInfoThreadUnsafe( string str )
		//	{
		//		m_progressWindow.AddInfo( str );
		//	}

		//	void AddErrorThreadUnsafe( string str )
		//	{
		//		m_progressWindow.AddError( str );
		//	}

		//	void DoneThreadUnsafe()
		//	{
		//		m_progressWindow.EnableUserClose();
		//	}

		//	void SetProgressThreadUnsafe( float progress )
		//	{
		//		m_progressWindow.SetProgress( progress );
		//	}
		//}

		//[Import( AllowDefault = true )]
		//private IWin32Window m_owner;
		private MainForm m_mainForm;

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
		private static readonly string PICO_ROOT = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_ROOT" ) + "\\" );
		private static readonly string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
		private static readonly string texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
    }
}
