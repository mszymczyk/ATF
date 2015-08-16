using System;
using System.IO;

namespace pico
{
	public class Paths
	{
		public static string UriToPicoDemoPath( Uri uri )
		{
			string path = uri.LocalPath;
			return PathToPicoDemoPath( path );
		}

		public static string PathToPicoDemoPath( string path )
		{
			string fullPath = Path.GetFullPath( path );
			int index = fullPath.IndexOf( PICO_DEMO_data );
			if ( index == -1 )
				return "";

			string localPath = fullPath.Substring( PICO_DEMO_data.Length );
			return localPath;
		}

		public static string LocalPathToPicoDataAbsolutePath( string localPath )
		{
			string absPath = PICO_DEMO_data + localPath;
			return absPath;
		}

		private static readonly string PICO_ROOT = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_ROOT" ) + "\\" );
		private static readonly string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
		private static readonly string PICO_DEMO_data = PICO_DEMO + "data\\";
		private static readonly string PICO_DEMO_dataWin = PICO_DEMO + "dataWin\\";
		private static readonly string PICO_DEMO_dataPS4 = PICO_DEMO + "dataPS4\\";
		private static readonly string texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
		private static readonly string nvcompress_exe = PICO_ROOT + "bin64\\nvcompress.exe";
	}
}
