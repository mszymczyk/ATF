using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;

using pico.Hub;

namespace pico.Controls
{	/// <summary>
	/// Component for communication with picoHub
	/// </summary>
	[Export( typeof( IInitializable ) )]
	[Export( typeof( TouchPad ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public partial class TouchPad : AdaptableControl, IInitializable//, IControlHostClient
	{
		public TouchPad()
		{
			InitializeComponent();
		}


		#region IInitializable

		/// <summary>
		/// Finishes initializing component by setting up scripting service, subscribing to document
		/// events, and creating PropertyDescriptors for settings</summary>
		void IInitializable.Initialize()
		{
			ControlInfo controlInfo = new ControlInfo( "TouchPad", "Touch Pad", StandardControlGroup.Bottom );
			m_controlHostService.RegisterControl(
				this,
				controlInfo,
				null );

			MouseDown += TouchPad_MouseDown;
			MouseUp += TouchPad_MouseUp;
			MouseMove += TouchPad_MouseMove;
			GotFocus += TouchPad_GotFocus;
			LostFocus += TouchPad_LostFocus;
			KeyDown += TouchPad_KeyDown;
			KeyUp += TouchPad_KeyUp;
		}

		void TouchPad_MouseDown( object sender, MouseEventArgs e )
		{
			if ( e.Button == System.Windows.Forms.MouseButtons.Left )
			{
				m_mouseDown = true;
				//m_prevMousePositionValid = true;
				//m_prevMousePosition = e.Location;
				m_mousePosition = e.Location;

				SendMouseDown();
			}
		}

		void TouchPad_MouseUp( object sender, MouseEventArgs e )
		{
			m_mouseDown = false;
			SendMouseUp();
		}

		void TouchPad_MouseMove( object sender, MouseEventArgs e )
		{
			if ( m_mouseDown )
			{
				//if ( ! m_prevMousePositionValid )
				//{
				//	m_prevMousePosition = e.Location;
				//}

				//m_prevMousePositionValid = true;
				m_mousePosition = e.Location;

				SendMouseMove();
			}
		}

		void TouchPad_GotFocus( object sender, EventArgs e )
		{
			HasKeyboardFocus = true;
		}

		void TouchPad_LostFocus( object sender, EventArgs e )
		{
			m_mouseDown = false;
			//m_prevMousePositionValid = false;
			HasKeyboardFocus = false;
		}

		void TouchPad_KeyDown( object sender, KeyEventArgs e )
		{
			int keyVal = e.KeyValue;
			//if ( keyVal < m_keysDown.Length )
			//{
			//	m_keysDown[keyVal] = true;
			//	SendMouseMove();
			//}
			if ( keyVal < 256 )
			{
				HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
				hubMsg.appendString( "touchPad" );
				hubMsg.appendString( "key" );
				hubMsg.appendInt( keyVal );
				hubMsg.appendInt( 1 );
				m_hubService.send( hubMsg );
			}
		}

		void TouchPad_KeyUp( object sender, KeyEventArgs e )
		{
			int keyVal = e.KeyValue;
			//if ( keyVal < m_keysDown.Length )
			//{
			//	m_keysDown[keyVal] = false;
			//	SendMouseMove();
			//}
			if ( keyVal < 256 )
			{
				HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
				hubMsg.appendString( "touchPad" );
				hubMsg.appendString( "key" );
				hubMsg.appendInt( keyVal );
				hubMsg.appendInt( 0 );
				m_hubService.send( hubMsg );
			}
		}

		private void SendMouseDown()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseDown" );
			FillButtons( hubMsg );
			m_hubService.send( hubMsg );
		}

		private void SendMouseUp()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseUp" );
			m_hubService.send( hubMsg );
		}

		private void SendMouseMove()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseMove" );
			FillButtons( hubMsg );
			m_hubService.send( hubMsg );
		}

		//private void SendKeys()
		//{
		//	HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
		//	hubMsg.appendString( "touchPad" );
		//	hubMsg.appendString( "mouseMove" );
		//	FillButtons( hubMsg );
		//	m_hubService.send( hubMsg );
		//}

		private void FillButtons( HubMessage msg )
		{
			msg.appendInt( m_mousePosition.X );
			msg.appendInt( m_mousePosition.Y );

			//int nKeysDown = 0;
			//for ( int i = 0; i < m_keysDown.Length; ++i )
			//{
			//	if ( m_keysDown[i] )
			//		nKeysDown += 1;
			//}

			//msg.appendInt( nKeysDown );

			//if ( nKeysDown > 0 )
			//{
			//	for ( int i = 0; i < m_keysDown.Length; ++i )
			//	{
			//		if ( m_keysDown[i] )
			//			msg.appendByte( (byte) i );
			//	}
			//}
		}

		#endregion

		//#region IControlHostClient Members

		///// <summary>
		///// Notifies the client that its Control has been activated. Activation occurs when
		///// the Control gets focus, or a parent "host" Control gets focus.</summary>
		///// <param name="control">Client Control that was activated</param>
		///// <remarks>This method is only called by IControlHostService if the Control was previously
		///// registered for this IControlHostClient.</remarks>
		//public void Activate( Control control )
		//{
		//	m_contextRegistry.ActiveContext = this;
		//}

		///// <summary>
		///// Notifies the client that its Control has been deactivated. Deactivation occurs when
		///// another Control or "host" Control gets focus.</summary>
		///// <param name="control">Client Control that was deactivated</param>
		///// <remarks>This method is only called by IControlHostService if the Control was previously
		///// registered for this IControlHostClient.</remarks>
		//public void Deactivate( Control control )
		//{
		//}

		///// <summary>
		///// Requests permission to close the client's Control</summary>
		///// <param name="control">Client Control to be closed</param>
		///// <returns>True if the Control can close, or false to cancel</returns>
		///// <remarks>
		///// 1. This method is only called by IControlHostService if the Control was previously
		///// registered for this IControlHostClient.
		///// 2. If true is returned, the IControlHostService calls its own
		///// UnregisterControl. The IControlHostClient has to call RegisterControl again
		///// if it wants to re-register this Control.</remarks>
		//public bool Close( Control control )
		//{
		//	return true;
		//}

		//#endregion

		[Import( AllowDefault = true )]
		private HubService m_hubService = null;

		[Import( AllowDefault = true )]
		private IControlHostService m_controlHostService = null;

		[Import( AllowDefault = true )]
		private IContextRegistry m_contextRegistry = null;

		private bool m_mouseDown;
		//private bool m_prevMousePositionValid;
		//private Point m_prevMousePosition;
		private Point m_mousePosition;

		public static readonly string APPPARAM_TAG = "appparam";

		private bool[] m_keysDown = new bool[127];
	}
}
