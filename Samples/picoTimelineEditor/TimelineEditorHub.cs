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
	}
}
