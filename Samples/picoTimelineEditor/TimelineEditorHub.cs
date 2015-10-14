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
		public static readonly string ANIMCLIPEDITOR_TAG = "timeline";

		public void changeEditMode( EditMode editMode )
		{
			m_editMode = editMode;

			HubMessage hubMsg = new HubMessage( ANIMCLIPEDITOR_TAG );
			hubMsg.appendString( "editMode" ); // command
			hubMsg.appendString( editMode.ToString() ); // what mode

			float scrubberPosition = 0;
			IDocument document = m_documentRegistry.ActiveDocument;
			TimelineDocument timelineDocument = document.As<TimelineDocument>();
			if ( timelineDocument != null )
				scrubberPosition = timelineDocument.ScrubberManipulator.Position;
			hubMsg.appendFloat( scrubberPosition );

			m_hubService.sendAlways( hubMsg );
		}

		public void hubService_sendReloadTimeline( TimelineDocument document )
		{
			if (m_isWriting)
				return;

			m_isWriting = true;

			MemoryStream stream = new MemoryStream();
			var writer = new TimelineXmlWriter( s_schemaLoader.TypeCollection );
			//writer.PersistDefaultAttributes = true;

			//writer.Write( DomNode, stream, m_timelineDocument.Uri );

			//HubMessage hubMessage = new HubMessage( TIMELINE_TAG );
			//hubMessage.appendString( "reloadTimeline" );
			//hubMessage.appendString( docUri );
			//hubMessage.appendInt( (int) stream.Length );
			//hubMessage.appendBytes( stream.ToArray() );
			//TimelineDocument document = this.As<TimelineDocument>();
			//hubMessage.appendFloat( document.ScrubberManipulator.Position );

			//m_hubService.send( hubMessage );

			m_isWriting = false;
		}

		private bool m_isWriting; // to prevent endless recursion while serializing DOM with TimelineXmlWriter
	}
}
