﻿using System;
using System.IO;
using System.Text;

namespace pico
{
	public class Paths
	{
		static Paths()
		{
			string sdkDirEnvVar = Environment.GetEnvironmentVariable( "SCE_ORBIS_SDK_DIR" );
			if ( sdkDirEnvVar != null && sdkDirEnvVar.Length > 0 )
			{
				PICO_DEMO_dataPS4 = PICO_DEMO + "dataPS4\\";

				SCE_ORBIS_SDK_DIR = Path.GetFullPath( sdkDirEnvVar + "\\" );
				orbis_image2gnf_exe = SCE_ORBIS_SDK_DIR + "host_tools\\bin\\orbis-image2gnf.exe";

				HAS_PS4_SDK_INSTALLED = true;
			}
		}

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

		public static string CanonicalizePathSimple( string srcPath )
		{
			StringBuilder dst = new StringBuilder();

			char lastC = '\\'; // this will remove leading '\' or '/'
			for ( int i = 0; i < srcPath.Length; ++i )
			{
				char c = srcPath[i];
				if ( c == '/' )
					c = '\\';

				if ( lastC == '\\' && c == '\\' )
				{
					// skip
				}
				else
				{
					lastC = c;
					dst.Append( c );
				}
			}


			string str = dst.ToString();
			return str;
		}

		public static readonly string PICO_ROOT = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_ROOT" ) + "\\" );
		public static readonly string PICO_DEMO = Path.GetFullPath( Environment.GetEnvironmentVariable( "PICO_DEMO" ) + "\\" );
		public static readonly string PICO_DEMO_data = PICO_DEMO + "data\\";
		public static readonly string PICO_DEMO_dataWin = PICO_DEMO + "dataWin\\";
		public static readonly string texconv_exe = PICO_ROOT + "bin64\\texconv.exe";
		public static readonly string nvcompress_exe = PICO_ROOT + "bin64\\nvcompress.exe";

		// ps4 stuff
		//
		public static readonly bool HAS_PS4_SDK_INSTALLED;

		public static readonly string PICO_DEMO_dataPS4;
		public static readonly string SCE_ORBIS_SDK_DIR;
		public static readonly string orbis_image2gnf_exe;
	}
}
