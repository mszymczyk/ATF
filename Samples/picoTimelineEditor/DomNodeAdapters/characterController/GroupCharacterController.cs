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
	public class GroupCharacterController : Group
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
			TrackCharacterControllerAnim track = ( new DomNode( Schema.trackCharacterControllerAnimType.Type ) ).As<TrackCharacterControllerAnim>();
			track.Name = "TrackCharacterController";
			tracks.Add( track );
		}

		/// <summary>
		/// Gets and sets the camera's animation file</summary>
		public string NodeName
		{
			get { return (string)DomNode.GetAttribute( Schema.groupCharacterControllerType.nodeNameAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.nodeNameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets group blend in duration</summary>
		public float BlendInDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCharacterControllerType.blendInDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.blendInDurationAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets group blend out duration</summary>
		public float BlendOutDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCharacterControllerType.blendOutDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.blendOutDurationAttribute, value ); }
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




