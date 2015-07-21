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
    public class TextureExporter
    {
		public TextureExporter( IWin32Window owner, MainForm mainForm, SchemaLoader schemaLoader )
		{
			m_owner = owner;
			m_mainForm = mainForm;
			m_schemaLoader = schemaLoader;
		}

		public void ExportOne( Uri fileUri )
		{
			List<Uri> fileList = new List<Uri>();
			fileList.Add( fileUri );
			Export( fileList );
		}

		public void ExportAll()
		{
		}

		void Export( List<Uri> fileList )
		{
			//ProgressOutputWindow powin = new ProgressOutputWindow();
			m_bgThread = new BackgroundThread( m_mainForm );
			//powin.ShowDialog( owner );

			int nDone = 0;
			foreach( Uri uri in fileList )
			{
				float progress = (float)nDone / (float)fileList.Count;
				m_bgThread.SetProgress( progress );
				ExportUri( uri );
				nDone += 1;
			}

			m_bgThread.SetProgress( 1.0f );
			//m_bgThread.Stop();
			m_bgThread.Done();
			m_bgThread.Wait();
		}

		int ExportUri( Uri resourceUri )
		{
			string metadataFilePath = resourceUri.LocalPath + ".metadata";
			Uri metadataUri = new Uri( metadataFilePath );
			m_bgThread.AddInfo( "Exporting file: " + metadataFilePath + "\n" );

			if ( File.Exists( metadataFilePath ) )
			{
				// read existing metadata
				using ( FileStream stream = File.OpenRead( metadataFilePath ) )
				{
					var reader = new DomXmlReader( m_schemaLoader );
					Sce.Atf.Dom.DomNode rootNode = reader.Read( stream, metadataUri );
					rootNode.InitializeExtensions();

					string inputFile = Path.GetFullPath( resourceUri.AbsolutePath );
					string dir_data = PICO_DEMO + "data\\";
					string dir_dataWin = PICO_DEMO + "dataWin\\";
					string outputFileWin_tmp = inputFile.Replace( dir_data, dir_dataWin );
					string outputFileWin = outputFileWin_tmp + ".dds";

					TextureMetadata tm = rootNode.As<TextureMetadata>();

					if ( tm.CopySourceFile )
					{
						m_bgThread.AddInfo( "Copying " + inputFile + " to " + outputFileWin + "\n" );
						System.IO.File.Copy( inputFile, outputFileWin, true );
					}
					else
					{
						string cmd = " -if CUBIC";
						if ( tm.Width > 0 )
							cmd += " -w " + tm.Width;
						if ( tm.Height > 0 )
							cmd += " -h " + tm.Height;

						if ( !tm.GenMipMaps )
							cmd += " -miplevels 0";

						if ( tm.ForceSourceSrgb )
							cmd += " -srgbi";

						Format format = Format.Unknown;

						if ( tm.ExtendedFormat != SharpDX.DXGI.Format.Unknown )
							//cmd += " -f " + tm.ExtendedFormat.ToString();
							format = tm.ExtendedFormat;
						else if ( tm.Format != SharpDX.DXGI.Format.Unknown )
							//cmd += " -f " + tm.Format.ToString();
							format = tm.Format;
						else
						{
							throw new Exception( "Invalid format" );
						}

						bool bcSrgbFormat = false;
						// texconv incorrectly generates mipmaps for compressed formats
						// so first resize/genmips to uncompressed file, and then compress
						//
						if ( tm.GenMipMaps &&
							(
							   format == Format.BC1_UNorm_SRgb
							|| format == Format.BC2_UNorm_SRgb
							|| format == Format.BC3_UNorm_SRgb
							|| format == Format.BC7_UNorm_SRgb
							)
							)
						{
							bcSrgbFormat = true;
							cmd += " -f " + Format.R8G8B8A8_UNorm_SRgb.ToString();
						}
						else
						{
							cmd += " -f " + format.ToString();
						}

						cmd += " -of " + outputFileWin;
						cmd += " " + inputFile;

						int ires = RunCommand_texconv( cmd );
						if ( ires != 0 )
						{
							return ires;
						}

						if ( bcSrgbFormat )
						{
							string cmd2 = " -f " + format.ToString();
							cmd2 += " -of " + outputFileWin;
							cmd2 += " " + outputFileWin;

							ires = RunCommand_texconv( cmd2 );
							if ( ires != 0 )
							{
								m_nErrors += 1;
								return ires;
							}
						}
					}

					{
						string dir_dataPS4 = PICO_DEMO + "dataPS4\\";
						string outputFilePS4_tmp = inputFile.Replace( dir_data, dir_dataWin );
						string outputFilePS4 = outputFileWin_tmp + ".gnf";
						string arg = "-f Auto ";
						arg += " -i \"" + outputFileWin + "\" ";
						arg += " -o \"" + outputFilePS4 + "\" ";

						int ires = RunCommand_orbis_image2gnf( arg );
						if ( ires != 0 )
						{
							m_nErrors += 1;
							return ires;
						}
					}
				}

				return 0;
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

				//MessageBox.Show( m_owner, "Please configure texture's metadata first!", "Error", MessageBoxButtons.OK );
				m_bgThread.AddError( "Couldn't open file: " + metadataFilePath + "\n" );
				m_nErrors += 1;
				return 1;
			}
		}

		int RunCommand_texconv( string arg )
		{
			m_bgThread.AddInfo( "Command: " + arg + " ..." );

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = texconv_exe;
			startInfo.Arguments = arg;
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();

			if ( process.ExitCode != 0 )
			{
				m_bgThread.AddError( "FAILED!\n" );
			}
			else
			{
				m_bgThread.AddInfo( "OK!\n" );
			}

			return process.ExitCode;
		}
		int RunCommand_orbis_image2gnf( string arg )
		{
			m_bgThread.AddInfo( "Command: " + arg + " ..." );

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "orbis-image2gnf.exe";
			startInfo.Arguments = arg;
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();

			if ( process.ExitCode != 0 )
			{
				m_bgThread.AddError( "FAILED!\n" );
			}
			else
			{
				m_bgThread.AddInfo( "OK!\n" );
			}

			return process.ExitCode;
		}


		private class BackgroundThread
		{
			//private IWin32Window m_owner;
			private MainForm m_mainForm;
			private ProgressOutputWindow m_progressWindow;
			private Thread m_thread;
			private bool m_alreadyStopped;

			public BackgroundThread( MainForm mainForm )
			{
				//m_owner = owner;
				m_mainForm = mainForm;
				m_thread = new Thread( Run );
				m_thread.Name = "progress dialog";
				m_thread.IsBackground = true; //so that the thread can be killed if app dies.
				m_thread.SetApartmentState( ApartmentState.STA );
				m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				m_thread.Start();
			}

			public void Stop()
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( m_progressWindow.Close ) );
					m_alreadyStopped = true;
				}
			}

			public void Wait()
			{
				m_thread.Join();
			}

			private void Run()
			{
				try
				{
					lock ( this )
					{
						if ( !m_alreadyStopped )
						{
							m_progressWindow = new ProgressOutputWindow();
						}
					}

					if ( !m_alreadyStopped )
					{
						//m_progressWindow.ShowDialog( m_owner );
						m_progressWindow.Visible = true;
						Application.Run( m_progressWindow );
						//m_progressWindow.Show();
						//m_mainForm.BeginInvoke( new MethodInvoker( () => m_progressWindow.ShowDialog(m_mainForm) ) );
					}

					//if ( m_fileToExport != null )
					//{

					//}

					//AddInfo( "Hej!\n" );
					//SetProgress( 0.1f );
					//System.Threading.Thread.Sleep( 1000 );
					//SetProgress( 0.5f );
					//AddError( "Ho!\n" );
					//System.Threading.Thread.Sleep( 1000 );
					//SetProgress( 1.0f );
					//Done();
				}
				finally
				{
					//lock ( this )
					//{
					//	if ( m_progressWindow != null )
					//	{
					//		// If the dialog was visible when it went away, we need to make the parent visible.  
					//		// We cannot rely on Windows to do it for us, since the parent is owned by a different 
					//		// thread, which means that we cannot tell windows about the parent/child relationship.
					//		//var visibleAtClose = m_progressWindow.Visible;

					//		// Now make the dialog go away
					//		m_progressWindow.Dispose();
					//		m_progressWindow = null;

					//		//if ( visibleAtClose )
					//		//{
					//		//	// Though unlikely, it is possible the parent dialog has already been disposed,
					//		//	// so check for null Owner.
					//		//	if ( m_owner != null )
					//		//		m_owner.BeginInvoke( new MethodInvoker( m_parent.Show ) );
					//		//}
					//	}
					//}
				}
			}

			public void AddInfo( string str )
			{
				lock(this)
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( () => this.AddInfoThreadUnsafe(str) ) );
				}
			}

			public void AddError( string str )
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( () => this.AddErrorThreadUnsafe( str ) ) );
				}
			}

			public void Done()
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( DoneThreadUnsafe ) );
				}
			}

			public void SetProgress( float progress )
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( () => this.SetProgressThreadUnsafe( progress ) ) );
				}
			}

			private void AddInfoThreadUnsafe( string str )
			{
				m_progressWindow.AddInfo( str );
			}

			private void AddErrorThreadUnsafe( string str )
			{
				m_progressWindow.AddError( str );
			}

			private void DoneThreadUnsafe()
			{
				m_progressWindow.EnableUserClose();
			}

			private void SetProgressThreadUnsafe( float progress )
			{
				m_progressWindow.SetProgress( progress );
			}
		}

		private static readonly string PICO_ROOT = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_ROOT" ) + "\\" );
		private static readonly string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
		private static readonly string texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
 
		private IWin32Window m_owner;
		private MainForm m_mainForm;
		private SchemaLoader m_schemaLoader;
		private BackgroundThread m_bgThread;
		private int m_nWarnings;
		private int m_nErrors;
	}
}
