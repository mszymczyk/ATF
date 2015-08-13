using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Sce.Atf;

namespace pico.Hub
{
	/// <summary>
	/// Component for communication with picoHub
	/// </summary>
	[Export( typeof( IInitializable ) )]
	[Export( typeof( HubService ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public class HubService : IInitializable
    {
		#region IInitializable Members

		/// <summary>
		/// Finishes initializing component by connecting to picoHub</summary>
		void IInitializable.Initialize()
		{
			IPHostEntry hostInfo = Dns.GetHostEntry( PICO_HUB_IP );
			IPAddress serverAddr = hostInfo.AddressList[1];
			var clientEndPoint = new IPEndPoint( serverAddr, PICO_HUB_PORT_OUTBOUND );

			// Create a client socket and connect it to the endpoint 
			m_picoHubClientSocketOutbound = new System.Net.Sockets.Socket( System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp );
			m_picoHubClientSocketOutbound.Connect( clientEndPoint );
		}

		#endregion

		public void send( HubMessage msg )
		{
			if ( m_picoHubClientSocketOutbound == null )
				return;

			byte[] bytes = msg.getFinalByteStream();
			m_picoHubClientSocketOutbound.Send( bytes );
		}

		// picoHub
		//
		static string PICO_HUB_IP = "localhost";
		static int PICO_HUB_PORT_OUTBOUND = 6666; // for sending data to picoHub
		//static int PICO_HUB_PORT_INBOUND = 6667; // for receiving data from picoHub

		// Client socket stuff 
		System.Net.Sockets.Socket m_picoHubClientSocketOutbound;
	}

	public class HubMessage
	{
		public HubMessage( string msgTag )
		{
			m_memStream = new MemoryStream();
			byte[] fakeMsg = new byte[4];
			m_memStream.Write( fakeMsg, 0, 4 );
			writeBytes( m_memStream, toBytes(msgTag) );
		}

		public void appendString( string str )
		{
			writeBytes( m_memStream, toBytes( str.Length ) );
			writeBytes( m_memStream, toBytes( str ) );
		}

		public void appendInt( int val )
		{
			writeBytes( m_memStream, toBytes( val ) );
		}

		public void appendFloat( float val )
		{
			writeBytes( m_memStream, toBytes( val ) );
		}

		public void appendBytes( byte[] bytes )
		{
			writeBytes( m_memStream, bytes );
		}

		public byte[] getFinalByteStream()
		{
			byte[] msgBytes = m_memStream.ToArray();
			byte[] msgSizeBytes = toBytes( msgBytes.Length );
			System.Buffer.BlockCopy( msgSizeBytes, 0, msgBytes, 0, 4 );
			return msgBytes;
		}

		private byte[] toBytes( string str )
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes( str );
			return bytes;
		}

		private byte[] toBytes( int ival )
		{
			byte[] bytes = BitConverter.GetBytes( ival );
			return bytes;
		}

		private byte[] toBytes( float fval )
		{
			byte[] bytes = BitConverter.GetBytes( fval );
			return bytes;
		}

		private void writeBytes( MemoryStream ms, byte[] bytes )
		{
			ms.Write( bytes, 0, bytes.Length );
		}

		MemoryStream m_memStream;
	}
}
