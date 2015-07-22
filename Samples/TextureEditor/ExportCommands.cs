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
		public TextureExporter( MainForm mainForm, SchemaLoader schemaLoader )
		{
			m_mainForm = mainForm;
			m_schemaLoader = schemaLoader;
		}

		public void ExportOne( Uri fileUri )
		{
			//List<Uri> fileList = new List<Uri>();
			//fileList.Add( fileUri );
			//Export( fileList );
			ProgressOutputWindow powin = new ProgressOutputWindow();
			powin.VisibleChanged += ( sender, msg ) =>
			{
				if ( m_bgThread == null )
					m_bgThread = new BackgroundThread( powin, m_schemaLoader, fileUri );
			};

			powin.CancelClicked += ( sender, msg ) =>
			{
				if ( m_bgThread != null )
					m_bgThread.Stop();
			};

			powin.ShowDialog();
			if ( m_bgThread != null )
				m_bgThread.Wait();
		}

		public void ExportAll()
		{
			ProgressOutputWindow powin = new ProgressOutputWindow();
			powin.VisibleChanged += ( sender, msg ) =>
			{
				if ( m_bgThread == null )
					m_bgThread = new BackgroundThread( powin, m_schemaLoader, null );
			};

			powin.CancelClicked += ( sender, msg ) =>
			{
				if ( m_bgThread != null )
					m_bgThread.Stop();
			};

			powin.ShowDialog();
			if ( m_bgThread != null )
				m_bgThread.Wait();
		}

		void Export( List<Uri> fileList )
		{
			////ProgressOutputWindow powin = new ProgressOutputWindow();
			//m_bgThread = new BackgroundThread( m_mainForm );
			////powin.ShowDialog( owner );

			//int nDone = 0;
			//foreach( Uri uri in fileList )
			//{
			//	float progress = (float)nDone / (float)fileList.Count;
			//	m_bgThread.SetProgress( progress );
			//	ExportUri( uri );
			//	nDone += 1;
			//}

			//m_bgThread.SetProgress( 1.0f );
			////m_bgThread.Stop();
			//m_bgThread.Done();
			//m_bgThread.Wait();
		}

		private class BackgroundThread
		{
			//private IWin32Window m_owner;
			//private MainForm m_mainForm;
			private ProgressOutputWindow m_progressWindow;
			private SchemaLoader m_schemaLoader;
			private Uri m_fileToExport;

			private Thread m_thread;
			private bool m_alreadyStopped;
			//private int m_nWarnings;
			private int m_nErrors;

			//public BackgroundThread( MainForm mainForm )
			public BackgroundThread( ProgressOutputWindow progressWindow, SchemaLoader schemaLoader, Uri fileToExport )
			{
				//m_owner = owner;
				//m_mainForm = mainForm;
				m_progressWindow = progressWindow;
				m_schemaLoader = schemaLoader;
				m_fileToExport = fileToExport;
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
					//if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
					//	m_progressWindow.BeginInvoke( new MethodInvoker( m_progressWindow.Close ) );
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
					//lock ( this )
					//{
					//	if ( !m_alreadyStopped )
					//	{
					//		m_progressWindow = new ProgressOutputWindow();
					//	}
					//}

					//if ( !m_alreadyStopped )
					//{
					//	//m_progressWindow.ShowDialog( m_owner );
					//	m_progressWindow.Visible = true;
					//	Application.Run( m_progressWindow );
					//	//m_progressWindow.Show();
					//	//m_mainForm.BeginInvoke( new MethodInvoker( () => m_progressWindow.ShowDialog(m_mainForm) ) );
					//}

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

					List<Uri> fileList = new List<Uri>();

					if ( m_fileToExport != null )
					{
						fileList.Add( m_fileToExport );
					}
					else
					{
						string[] files = System.IO.Directory.GetFiles( PICO_DEMO, "*.metadata", SearchOption.AllDirectories );
						foreach ( string s in files )
						{
							Uri uri = new Uri( s );
							fileList.Add( uri );
						}
					}

					AddInfo( "Found " + fileList.Count + " files\n\n" );

					//System.Threading.Thread.Sleep( 5000 );

					int nDone = 0;
					foreach ( Uri uri in fileList )
					{
						lock ( this )
						{
							if ( m_alreadyStopped )
							{
								break;
							}
						}

						float progress = (float)nDone / (float)fileList.Count;
						SetProgress( progress );
						ExportUri( uri );
						nDone += 1;
					}

					if ( nDone == fileList.Count )
					{
						SetProgress( 1.0f );
						AddInfo( "Finished\n" );
					}
					else
					{
						AddError( "Cancelled by user. Finished\n " + nDone + "/" + fileList.Count );
					}

					//if ( m_nWarnings > 0 )
					//{
					//	AddInfo( "" + m_nWarnings + " warnings!\n" );
					//}

					if ( m_nErrors > 0 )
					{
						AddError( "" + m_nErrors + " errors!\n" );
					}

					Done();
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

			public int ExportUri( Uri metadataUri )
			{
				string metadataFilePath = Path.GetFullPath( metadataUri.LocalPath );
				AddInfo( "Exporting file: " + metadataFilePath + "\n" );

				string inputFile = metadataFilePath.Substring( 0, metadataFilePath.Length - ".metadata.".Length + 1 );
				string dir_data = PICO_DEMO + "data\\";
				string dir_dataWin = PICO_DEMO + "dataWin\\";
				string outputFileWin_tmp = inputFile.Replace( dir_data, dir_dataWin );
				string outputFileWin = outputFileWin_tmp + ".dds";

				string dir_dataPS4 = PICO_DEMO + "dataPS4\\";
				string outputFilePS4_tmp = inputFile.Replace( dir_data, dir_dataPS4 );
				string outputFilePS4 = outputFilePS4_tmp + ".gnf";

				bool exportRequired = false;

				if ( ! File.Exists(inputFile) )
				{
					AddInfo( "File " + inputFile + " doesn't exist!\n" );
					return 1;
				}

				DateTime inputFileLwt = File.GetLastWriteTime( inputFile );
				//DateTime inputFileCt = File.GetCreationTime( inputFile );
				//DateTime inputFileDT = inputFileLwt >= inputFileCt ? inputFileLwt : inputFileCt;
				DateTime inputFileDT = inputFileLwt;

				DateTime metadataFileLwt = File.GetLastWriteTime( metadataFilePath );
				//DateTime metadataFileCt = File.GetCreationTime( metadataFilePath );
				//DateTime metadataFileDT = metadataFileLwt >= metadataFileCt ? metadataFileLwt : metadataFileCt;
				DateTime metadataFileDT = metadataFileLwt;

				DateTime newerDT = inputFileDT >= metadataFileDT ? inputFileDT : metadataFileDT;

				if ( File.Exists( outputFileWin ) )
				{
					DateTime dt = File.GetLastWriteTime( outputFileWin );
					//DateTime ct = File.GetCreationTime( outputFileWin );
					//if ( newerDT > dt || newerDT > ct )
					if ( newerDT > dt )
					{
						exportRequired = true;
					}
				}
				else
				{
					exportRequired = true;
				}

				if ( !exportRequired && File.Exists(outputFilePS4) )
				{
					DateTime dt = File.GetLastWriteTime( outputFilePS4 );
					if ( newerDT > dt )
					{
						exportRequired = true;
					}
				}
				else
				{
					exportRequired = true;
				}

				if ( ! exportRequired )
				{
					AddInfo( "File is up-to-date " + inputFile + "\n" );
					return 0;
				}

				if ( File.Exists( metadataFilePath ) )
				{
					// read existing metadata
					using ( FileStream stream = File.OpenRead( metadataFilePath ) )
					{
						var reader = new DomXmlReader( m_schemaLoader );
						Sce.Atf.Dom.DomNode rootNode = reader.Read( stream, metadataUri );
						rootNode.InitializeExtensions();

						TextureMetadata tm = rootNode.As<TextureMetadata>();

						if ( tm.CopySourceFile )
						{
							AddInfo( "Copying " + inputFile + " to " + outputFileWin + "\n" );
							//System.IO.File.Delete( outputFileWin );
							System.IO.File.Copy( inputFile, outputFileWin, true );
							System.IO.File.SetLastWriteTime( outputFileWin, DateTime.Now );
						}
						else
						{
							string cmd = " -if CUBIC";
							if ( tm.Width > 0 )
								cmd += " -w " + tm.Width;
							if ( tm.Height > 0 )
								cmd += " -h " + tm.Height;

							if ( !tm.GenMipMaps )
								cmd += " -m 1";

							if ( tm.ForceSourceSrgb )
								cmd += " -srgbi";

							if ( tm.FlipY )
								cmd += " -vflip";

							Format format = Format.Unknown;

							//if ( tm.ExtendedFormat != SharpDX.DXGI.Format.Unknown )
							//	//cmd += " -f " + tm.ExtendedFormat.ToString();
							//	format = tm.ExtendedFormat;
							//else if ( tm.Format != SharpDX.DXGI.Format.Unknown )
								//cmd += " -f " + tm.Format.ToString();
								format = tm.Format;

							string preset = tm.Preset;
							if ( preset == TextureMetadata.TEXTURE_PRESET_CUSTOM_FORMAT )
								format = tm.Format;

							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_BC1_SRGB )
								format = SharpDX.DXGI.Format.BC1_UNorm_SRgb;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_BC3_SRGB )
								format = SharpDX.DXGI.Format.BC3_UNorm_SRgb;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_BC7_SRGB )
								format = SharpDX.DXGI.Format.BC7_UNorm_SRgb;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_SRGB )
								format = SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_BC6H_HDR_UNORM )
								format = SharpDX.DXGI.Format.BC6H_Uf16;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_COLOR_HDR_UNORM )
								format = SharpDX.DXGI.Format.R16G16B16A16_Float;

							else if ( preset == TextureMetadata.TEXTURE_PRESET_NORMALMAP_BC5 )
								format = SharpDX.DXGI.Format.BC5_SNorm;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_NORMALMAP_RG8 )
								format = SharpDX.DXGI.Format.R8G8_SNorm;

							else if ( preset == TextureMetadata.TEXTURE_PRESET_AMBIENT_BC4 )
								format = SharpDX.DXGI.Format.BC4_UNorm;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_AMBIENT_R8 )
								format = SharpDX.DXGI.Format.R8_UNorm;

							else if ( preset == TextureMetadata.TEXTURE_PRESET_SPECULARMAP_BC1 )
								format = SharpDX.DXGI.Format.BC1_UNorm;
							else if ( preset == TextureMetadata.TEXTURE_PRESET_SPECULARMAP_UNORM )
								format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

							else
							{
								AddError( "Invalid format: " + metadataFilePath + "\n" );
								m_nErrors += 1;
								return 1;
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

							string dirWin = System.IO.Path.GetDirectoryName( outputFileWin );
							System.IO.Directory.CreateDirectory( dirWin );

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

								//System.IO.File.SetLastWriteTime( outputFileWin, DateTime.Now );
							}
						}

						{
							string dirPS4 = System.IO.Path.GetDirectoryName( outputFilePS4 );
							System.IO.Directory.CreateDirectory( dirPS4 );

							string arg = "-f Auto ";
							arg += " -i \"" + outputFileWin + "\" ";
							arg += " -o \"" + outputFilePS4 + "\" ";

							int ires = RunCommand_orbis_image2gnf( arg );
							if ( ires != 0 )
							{
								m_nErrors += 1;
								return ires;
							}

							//System.IO.File.SetLastWriteTime( outputFilePS4, DateTime.Now );
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
					AddError( "Couldn't open file: " + metadataFilePath + "\n" );
					m_nErrors += 1;
					return 1;
				}
			}

			int RunCommand_texconv( string arg )
			{
				AddInfo( "Command: texconv.exe " + arg + "\n" );

				System.Diagnostics.Process process = new System.Diagnostics.Process();
				System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
				startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				startInfo.FileName = texconv_exe;
				startInfo.Arguments = arg;
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				process.StartInfo = startInfo;
				process.OutputDataReceived += ( sender, args ) => AddInfo( "texconv output: " + args.Data + "\n" );
				process.ErrorDataReceived  += ( sender, args ) => AddError( "texconv output: " + args.Data + "\n" );
				process.Start();
				process.BeginOutputReadLine();
				////
				//// Read in all the text from the process with the StreamReader.
				////
				//using ( StreamReader reader = process.StandardOutput )
				//{
				//	string result = reader.ReadToEnd();
				//	AddInfo( "texconv output: " + result + "\n" );
				//}
				process.WaitForExit();

				if ( process.ExitCode != 0 )
				{
					AddError( "FAILED!\n" );
				}
				else
				{
					AddInfo( "Done!\n" );
				}

				return process.ExitCode;
			}
			int RunCommand_orbis_image2gnf( string arg )
			{
				AddInfo( "Command: orbis-image2gnf " + arg + "\n" );

				System.Diagnostics.Process process = new System.Diagnostics.Process();
				System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
				startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				startInfo.FileName = "orbis-image2gnf.exe";
				startInfo.Arguments = arg;
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				process.StartInfo = startInfo;
				process.OutputDataReceived += ( sender, args ) => AddInfo( "orbis-image2gnf.exe output: " + args.Data + "\n" );
				process.ErrorDataReceived  += ( sender, args ) => AddError( "texconv output: " + args.Data + "\n" );
				process.Start();
				process.BeginOutputReadLine();
				process.WaitForExit();

				if ( process.ExitCode != 0 )
				{
					AddError( "FAILED!\n" );
				}
				else
				{
					AddInfo( "Done!\n" );
				}

				return process.ExitCode;
			}
		}

		private static readonly string PICO_ROOT = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_ROOT" ) + "\\" );
		private static readonly string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
		private static readonly string texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
 
		//private IWin32Window m_owner;
		private MainForm m_mainForm;
		private SchemaLoader m_schemaLoader;
		private BackgroundThread m_bgThread;
	}
}
