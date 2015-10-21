using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace pico
{
    /// <summary>
    /// Used for checking if required pico services are running and launches missing ones.
	/// </summary>
    public static class ScreamInterop
    {
		[DllImport( "kernel32", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern IntPtr LoadLibrary( string fileName );

		//[ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
		[DllImport( "kernel32", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool FreeLibrary( IntPtr hModule );

		[DllImportAttribute( "picoScreamInteropNative.dll", EntryPoint = "picoScreamInteropNative_Initialize", CallingConvention = CallingConvention.StdCall )]
		private static extern int NativeStartUp();

		[DllImportAttribute( "picoScreamInteropNative.dll", EntryPoint = "picoScreamInteropNative_Shutdown", CallingConvention = CallingConvention.StdCall )]
		private static extern int NativeShutDown();

		[DllImportAttribute( "picoScreamInteropNative.dll", EntryPoint = "picoScreamInteropNative_RefreshBank", CallingConvention = CallingConvention.StdCall )]
		private static extern void NativeRefreshBank( string bankName );

		[DllImportAttribute( "picoScreamInteropNative.dll", EntryPoint = "picoScreamInteropNative_RefreshAllBanks", CallingConvention = CallingConvention.StdCall )]
		private static extern void NativeRefreshAllBanks();

		[DllImportAttribute( "picoScreamInteropNative.dll", EntryPoint = "picoScreamInteropNative_GetBankSounds", CallingConvention = CallingConvention.StdCall )]
		private static extern void NativeGetBankSounds( string bankName, out IntPtr unmanagedStringArray, out int iStringCount );


		public static void RefreshBank( string bankName )
		{
			NativeRefreshBank( bankName );
		}

		public static void RefreshAllBanks()
		{
			NativeRefreshAllBanks();
		}

		public static string[] GetBankSounds( string bankName )
		{
			IntPtr unmanagedStringArray = IntPtr.Zero;
			int StringCount = 0;
			NativeGetBankSounds( bankName, out unmanagedStringArray, out StringCount );

			if ( StringCount == 0 )
				return null;

			IntPtr[] pIntPtrArray = new IntPtr[StringCount];
			string[] managedStringArray = new string[StringCount];

			Marshal.Copy( unmanagedStringArray, pIntPtrArray, 0, StringCount );

			for ( int i = 0; i < StringCount; i++ )
			{
				managedStringArray[i] = Marshal.PtrToStringAnsi( pIntPtrArray[i] );
			}

			return managedStringArray;
		}


		public static void StartUp()
		{
			s_libHandle = LoadLibrary( "picoScreamInteropNative.dll" );

			NativeStartUp();

			//string[] sounds = GetBankSounds( "sounds/beach.bnk" );
		}

		public static void ShutDown()
		{
			NativeShutDown();

			FreeLibrary( s_libHandle );
		}

		private static IntPtr s_libHandle;
	}
}
