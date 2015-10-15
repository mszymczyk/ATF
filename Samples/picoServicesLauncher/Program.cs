using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace picoServicesLauncher
{
	class Program
	{
		static void Main( string[] args )
		{
			//Program p = new Program();
			//p.Run();

			pico.ServicesLauncher.LaunchServices();
		}


		//void Run()
		//{
		//	string PICO_ROOT = Environment.GetEnvironmentVariable( "PICO_ROOT" );
		//	if ( string.IsNullOrEmpty(PICO_ROOT) )
		//	{
		//		System.Console.WriteLine( "PICO_ROOT environment variable not set! pico services won't be launched" );
		//		System.Console.ReadLine();
		//		return;
		//	}

		//	string bin_path = Path.GetFullPath( PICO_ROOT + "\\bin\\" );
		//	string bin64_path = Path.GetFullPath( PICO_ROOT + "\\bin64\\" );

		//	Process[] localAll = Process.GetProcesses();
		//	var picoHubList = localAll.Where( p => p.ProcessName.Contains( "picoHub" ) ).ToList();
		//	if ( picoHubList.Count == 0 )
		//	{
		//		// launch picoHub.exe
		//		//
		//		string picoHub_exe = bin64_path + "picoHub.exe";
		//		//Process.Start( picoHub_exe );
		//		StartAndWaitForExit( picoHub_exe );
		//	}

		//	var picoLogOutputList = localAll.Where( p => p.ProcessName.Contains( "picoLogOutput" ) ).ToList();
		//	if ( picoLogOutputList.Count == 0 )
		//	{
		//		// launch picoLogOutput.exe
		//		//
		//		string picoLogOutput_exe = bin_path + "picoLogOutput.exe";
		//		//Process.Start( picoLogOutput_exe );
		//		StartAndWaitForExit( picoLogOutput_exe );
		//	}
		//}

		//void StartAndWaitForExit( string fileName )
		//{
		//	System.Diagnostics.Process process = new System.Diagnostics.Process();
		//	System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		//	startInfo.FileName = fileName;
		//	process.StartInfo = startInfo;
		//	process.Start();
		//	process.WaitForExit();
		//}
	}
}
