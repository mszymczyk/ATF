//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
	/// <summary>
	/// Adapts DomNode to a special purpose group of camera tracks</summary>
	public class GroupCamera : Group
	{
		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			IList<ITrack> tracks = Tracks;
			if ( tracks.Count > 0 )
				return;

			// add default track for camera animation
			//
			TrackCameraAnim trackCameraAnim = ( new DomNode( Schema.trackCameraAnimType.Type ) ).As<TrackCameraAnim>();
			trackCameraAnim.Name = "CameraAnimTrack";
			tracks.Add( trackCameraAnim );
		}

		/// <summary>
		/// Gets and sets group blend in duration</summary>
		public float BlendInDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCameraType.blendInDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCameraType.blendInDurationAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets group blend out duration</summary>
		public float BlendOutDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCameraType.blendOutDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCameraType.blendOutDurationAttribute, value ); }
		}

		public string PreCutsceneCamera
		{
			get { return (string)DomNode.GetAttribute( Schema.groupCameraType.preCutsceneCameraAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCameraType.preCutsceneCameraAttribute, value ); }
		}

		public string PostCutsceneCamera
		{
			get { return (string)DomNode.GetAttribute( Schema.groupCameraType.postCutsceneCameraAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCameraType.postCutsceneCameraAttribute, value ); }
		}

		#region IGroup Members

		/// <summary>
		/// Gets and sets the group name</summary>
		public override string Name
		{
			get { return (string)DomNode.GetAttribute( Schema.groupCameraType.nameAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCameraType.nameAttribute, value ); }
		}

		#endregion
	}
}




