//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets; 

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

using picoTimelineEditor.DomNodeAdapters;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Controls.SyntaxEditorControl;

using pico.Hub;

namespace picoTimelineEditor
{
    /// <summary>
    /// Part of editor that handles interaction with picoHub
	/// </summary>
    public partial class TimelineEditor
    {
		public static readonly string TIMELINEEDITOR_TAG = "timeline";

		private void hubService_receiveMessages( object sender, pico.Hub.MessagesReceivedEventArgs args )
		{
			// callback on main thread
			//
			foreach ( HubMessageIn msg in args.Messages )
			{
				if ( msg.payloadSize_ <= TIMELINEEDITOR_TAG.Length )
					continue;

				string tag = msg.UnpackString( TIMELINEEDITOR_TAG.Length );
				if ( tag != TIMELINEEDITOR_TAG )
					return;

				string cmd = msg.UnpackString();
				if ( cmd == "levelChanged" )
				{
					foreach ( IDocument document in m_documentRegistry.Documents )
					{
						if ( !document.Dirty )
							continue;

						TimelineHubCommunication hubComm = document.Cast<TimelineHubCommunication>();
						hubComm.sendReloadTimeline( false );
					}

					//m_animListEditor.RemoveAllItems();
					hubService_sendSelectTimeline();
					hubService_sendPlayPause();
					//hubService_sendScrubberPosition();
				}
				else if ( cmd == "animList" )
				{
					//string category = msg.UnpackString();
					//int nAnims = msg.UnpackInt();
					//for ( int ianim = 0; ianim < nAnims; ++ianim )
					//{
					//	string userName = msg.UnpackString();
					//	string fileName = msg.UnpackString();

					//	picoAnimListEditorElement ale = new picoAnimListEditorElement( category, userName, fileName );
					//	ale.updateIcon();
					//	m_animListEditor.AddItem( ale, category, this );
					//}
				}
				else if ( cmd == "requestPlaybackInfo" )
				{
					//string editMode = m_editMode.ToString();
					//changeEditMode( editMode );
				}
				else if ( cmd == "scrubberPosPico" )
				{
					TimelineDocument document = ActiveDocument;
					if ( document == null )
						continue;

					string filename = msg.UnpackString();
					string absFilename = pico.Paths.LocalPathToPicoDataAbsolutePath( filename );
					if ( document.Uri.LocalPath != absFilename )
					{
						Outputs.WriteLine( OutputMessageType.Warning, "{0} doesn't match active document", filename );
						continue;
					}

					float scrubberPos = msg.UnpackFloat();
					m_receivingScrubberPos = true;
					document.ScrubberManipulator.Position = scrubberPos;
					m_receivingScrubberPos = false;
				}
			}
		}

		public void hubService_sendSelectTimeline()
		{
			string docUri = null;

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context != null )
			{
				TimelineDocument document = context.As<TimelineDocument>();
				if ( document != null )
				{
					docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
				}
			}

			if ( string.IsNullOrEmpty( docUri ) )
				docUri = ".*";

			HubMessage hubMsg = new HubMessage( TimelineHubCommunication.TIMELINE_TAG );
			hubMsg.appendString( "selectTimeline" ); // command
			hubMsg.appendString( docUri ); // what timeline
			m_hubService.send( hubMsg );

			if ( docUri != ".*" )
				hubService_sendScrubberPosition();
		}

		//public void changeEditMode( EditMode editMode )
		//{
		//	m_editMode = editMode;

		//	HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
		//	hubMsg.appendString( "editMode" ); // command

		//	string docUri = null;
		//	float scrubberPosition = 0;
		//	IDocument document = m_documentRegistry.ActiveDocument;
		//	if ( document != null )
		//	{
		//		docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
		//		TimelineDocument timelineDocument = document.As<TimelineDocument>();
		//		scrubberPosition = timelineDocument.ScrubberManipulator.Position;
		//	}

		//	if ( string.IsNullOrEmpty( docUri ) )
		//		docUri = "*";

		//	hubMsg.appendString( docUri );
		//	hubMsg.appendString( editMode.ToString() ); // what mode
		//	hubMsg.appendFloat( scrubberPosition );

		//	m_hubService.sendAlways( hubMsg );
		//}

		public void hubService_sendPlayPause()
		{
			//HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			//hubMsg.appendString( "editMode" ); // command

			//string docUri = null;
			//float scrubberPosition = 0;
			//IDocument document = m_documentRegistry.ActiveDocument;
			//if ( document != null )
			//{
			//	docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
			//	TimelineDocument timelineDocument = document.As<TimelineDocument>();
			//	scrubberPosition = timelineDocument.ScrubberManipulator.Position;
			//}

			//if ( string.IsNullOrEmpty( docUri ) )
			//	docUri = "*";

			//hubMsg.appendString( docUri );
			//hubMsg.appendString( play ? "Preview" : "Editing" ); // what mode
			//hubMsg.appendFloat( scrubberPosition );

			//m_hubService.send( hubMsg );

			HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			hubMsg.appendString( "playPause" ); // command
			hubMsg.appendInt( Playing ? 1 : 0 );

			float scrubberPosition = 0;
			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context != null )
			{
				TimelineDocument document = context.As<TimelineDocument>();
				scrubberPosition = document.ScrubberManipulator.Position;
			}
			hubMsg.appendFloat( scrubberPosition );

			m_hubService.send( hubMsg );
		}

		//private bool hubService_validate( out string docUri )
		//{
		//	docUri = "";

		//	if ( m_hubService == null )
		//		return false;

		//	if ( m_hubService.BlockOutboundTraffic )
		//		return false;

		//	IDocument document = m_documentRegistry.ActiveDocument;
		//	if ( document == null )
		//		return false;

		//	docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
		//	if ( docUri.Length == 0 )
		//	{
		//		Outputs.WriteLine( OutputMessageType.Error, "Timeline document {0} is not located within PICO_DEMO\\data folder!", document.Uri.LocalPath );
		//		return false;
		//	}

		//	return true;
		//}

		public void hubService_sendScrubberPosition()
		{
			//if ( m_editMode != EditMode.Editing || m_receivingScrubberPos )
			if ( m_receivingScrubberPos )
				return;

			//string docFilename;
			//if ( ! hubService_validate(out docFilename) )
			//	return;

			HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			hubMsg.appendString( "scrubberPos" );

			//hubMsg.appendString( docFilename );
			string docUri = null;
			float scrubberPosition = 0;

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context != null )
			{
				TimelineDocument document = context.As<TimelineDocument>();
				if ( document != null )
				{
					docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
					scrubberPosition = document.ScrubberManipulator.Position;
				}
			}

			if ( string.IsNullOrEmpty( docUri ) )
				docUri = ".*";

			hubMsg.appendString( docUri );
			hubMsg.appendFloat( scrubberPosition );

			m_hubService.send( hubMsg );
		}

		private bool m_receivingScrubberPos;
	}
}
