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
	/// Utility for exporting textures based on metadata
    /// </summary>
    public class TextureExporter
    {
		public TextureExporter( MainForm mainForm, SchemaLoader schemaLoader )
		{
			m_mainForm = mainForm;
			m_schemaLoader = schemaLoader;
		}

		public void ExportOne( Uri fileUri )
		{
			ProgressOutputWindow powin = new ProgressOutputWindow();
			powin.VisibleChanged += ( sender, msg ) =>
			{
				if ( m_bgThread == null )
					m_bgThread = new BackgroundThread( powin, m_schemaLoader, fileUri, false );
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

		public void ExportAll( bool batchExport )
		{
			ProgressOutputWindow powin = new ProgressOutputWindow();
			powin.VisibleChanged += ( sender, msg ) =>
			{
				if ( m_bgThread == null )
					m_bgThread = new BackgroundThread( powin, m_schemaLoader, null, batchExport );
			};

			powin.CancelClicked += ( sender, msg ) =>
			{
				if ( m_bgThread != null )
					m_bgThread.Stop();
			};

			powin.ShowDialog();
			if ( m_bgThread != null )
				m_bgThread.Wait();

			if ( batchExport )
			{
				m_mainForm.Close();
			}
		}

		private class BackgroundThread
		{
			private ProgressOutputWindow m_progressWindow;
			private SchemaLoader m_schemaLoader;
			private Uri m_fileToExport;
			private bool m_batchExport;

			private Thread m_thread;
			private bool m_alreadyStopped;
			private int m_nErrors;

			public BackgroundThread( ProgressOutputWindow progressWindow, SchemaLoader schemaLoader, Uri fileToExport, bool batchExport )
			{
				m_progressWindow = progressWindow;
				m_schemaLoader = schemaLoader;
				m_fileToExport = fileToExport;
				m_batchExport = batchExport;
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
					List<Uri> fileList = new List<Uri>();

					if ( m_fileToExport != null )
					{
						fileList.Add( m_fileToExport );
					}
					else
					{
						string[] files = System.IO.Directory.GetFiles( pico.Paths.PICO_DEMO_data, "*.metadata", SearchOption.AllDirectories );
						foreach ( string s in files )
						{
							Uri uri = new Uri( s );
							fileList.Add( uri );
						}
					}

					AddInfo( "Found " + fileList.Count + " files\n\n" );

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

						SetProgress( nDone, fileList.Count );
						ExportUri( uri );
						nDone += 1;
					}

					if ( nDone == fileList.Count )
					{
						SetProgress( nDone, fileList.Count );
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

					if ( m_batchExport )
						DoneBatchExport();
					else
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
			public void DoneBatchExport()
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( DoneBatchExportThreadUnsafe ) );
				}
			}

			public void SetProgress( int nDone, int nTotal )
			{
				lock ( this )
				{
					if ( m_progressWindow != null && m_progressWindow.IsHandleCreated )
						m_progressWindow.BeginInvoke( new MethodInvoker( () => this.SetProgressThreadUnsafe( nDone, nTotal ) ) );
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

			private void DoneBatchExportThreadUnsafe()
			{
				m_progressWindow.Close();
			}

			private void SetProgressThreadUnsafe( int nDone, int nTotal )
			{
				m_progressWindow.SetProgress( nDone, nTotal );
			}

			public int ExportUri( Uri metadataUri )
			{
				string metadataFilePath = Path.GetFullPath( metadataUri.LocalPath );
				AddInfo( "Exporting file: " + metadataFilePath + "\n" );

				string inputFile = metadataFilePath.Substring( 0, metadataFilePath.Length - ".metadata.".Length + 1 );
				string outputFileWin_tmp = inputFile.Replace( pico.Paths.PICO_DEMO_data, pico.Paths.PICO_DEMO_dataWin );
				string outputFileWin = outputFileWin_tmp + ".dds";

				string outputFilePS4 = null;
				if ( pico.Paths.HAS_PS4_SDK_INSTALLED )
				{
					string outputFilePS4_tmp = inputFile.Replace( pico.Paths.PICO_DEMO_data, pico.Paths.PICO_DEMO_dataPS4 );
					outputFilePS4 = outputFilePS4_tmp + ".gnf";
				}

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
							string dirWin = System.IO.Path.GetDirectoryName( outputFileWin );
							System.IO.Directory.CreateDirectory( dirWin );

							AddInfo( "Copying " + inputFile + " to " + outputFileWin + "\n" );
							//System.IO.File.Delete( outputFileWin );
							System.IO.File.Copy( inputFile, outputFileWin, true );
							System.IO.File.SetLastWriteTime( outputFileWin, DateTime.Now );
						}
						else
						{
							string cmd = "";
							//cmd += " -if CUBIC";
							//cmd += " -if POINT";
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

							string preset = tm.Preset;
							if ( preset == TextureMetadata.TEXTURE_PRESET_CUSTOM_FORMAT )
								format = tm.Format;

							else if ( Enum.TryParse<SharpDX.DXGI.Format>(tm.Preset, out format) )
							{

							}

							else
							{
								AddError( "Invalid format: " + metadataFilePath + "\n" );
								m_nErrors += 1;
								return 1;
							}

							//bool isBc123 = false;
							//if (
							//	   format == Format.BC1_UNorm
							//	|| format == Format.BC1_UNorm_SRgb
							//	|| format == Format.BC2_UNorm
							//	|| format == Format.BC2_UNorm_SRgb
							//	|| format == Format.BC3_UNorm
							//	|| format == Format.BC3_UNorm_SRgb
							//	)
							//{
							//	isBc123 = true;
							//}

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
								//if ( isBc123 )
								//	cmd += " -bcdither "; // improves compression quality of smooth gradients
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
								if ( !tm.GenMipMaps )
									cmd2 += " -m 1";
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

						if ( tm.ExportToGnf && outputFilePS4 != null )
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
						else
						{
							if ( System.IO.File.Exists( outputFilePS4 ) )
								System.IO.File.Delete( outputFilePS4 );
						}
					}

					return 0;
				}
				else
				{
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
				startInfo.FileName = pico.Paths.texconv_exe;
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
				startInfo.FileName = pico.Paths.orbis_image2gnf_exe;
				startInfo.Arguments = arg;
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				process.StartInfo = startInfo;
				process.OutputDataReceived += ( sender, args ) => AddInfo( "orbis-image2gnf.exe output: " + args.Data + "\n" );
				process.ErrorDataReceived  += ( sender, args ) => AddError( "orbis-image2gnf.exe output: " + args.Data + "\n" );
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

		public static string GetDataWinTexture( string dataTexture )
		{
			string outputFileWin_tmp = dataTexture.Replace( pico.Paths.PICO_DEMO_data, pico.Paths.PICO_DEMO_dataWin );
			string outputFileWin = outputFileWin_tmp + ".dds";
			return outputFileWin;
		}
 
		private MainForm m_mainForm;
		private SchemaLoader m_schemaLoader;
		private BackgroundThread m_bgThread;
	}
}
