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
    /// Adapts DomNode to a group of tracks</summary>
    public class GroupCamera : DomNodeAdapter, IGroup, ICloneable
    {
		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			//DomNode.ChildInserted += DomNode_ChildInserted;
			//DomNode.ChildRemoved += DomNode_ChildRemoved;

			//m_tracks = new List<ITrack>();

			//ITrack trackFader = GetChild<ITrack>( Schema.groupCameraType.trackFaderChild );
			//if ( trackFader != null )
			//{
			//	m_tracks.Add( trackFader );
			//}
		}

		//private void DomNode_ChildInserted( object sender, ChildEventArgs e )
		//{
		//	TrackFader trackFader = e.Child.As<TrackFader>();
		//	if ( trackFader != null )
		//	{
		//		m_tracks.Add( trackFader );
		//	}
		//}

		//private void DomNode_ChildRemoved( object sender, ChildEventArgs e )
		//{
		//	TrackFader trackFader = e.Child.As<TrackFader>();
		//	if ( trackFader != null )
		//	{
		//		m_tracks.Remove( trackFader );
		//	}
		//}

		#region IGroup Members

        /// <summary>
        /// Gets and sets the group name</summary>
        public string Name
        {
			//get { return (string)DomNode.GetAttribute(Schema.groupType.nameAttribute); }
			//set { DomNode.SetAttribute(Schema.groupType.nameAttribute, value); }
			get { return "GroupCamera"; }
			set { }
        }

        /// <summary>
        /// Gets and sets whether the group is expanded (i.e., are the tracks it contains
        /// visible?)</summary>
        public bool Expanded
        {
            get { return (bool)DomNode.GetAttribute(Schema.groupCameraType.expandedAttribute); }
			set { DomNode.SetAttribute( Schema.groupCameraType.expandedAttribute, value ); }
        }

        /// <summary>
        /// Gets the timeline that contains the group</summary>
        public ITimeline Timeline
        {
            get { return GetParentAs<Timeline>(); }
        }

        /// <summary>
        /// Creates a new track. Try to use TimelineControl.Create(ITrack) if there is a "source" ITrack.
        /// Does not add the track to this group.</summary>
        /// <returns>New unparented track</returns>
        public ITrack CreateTrack()
        {
			//return new DomNode(Schema.trackFaderType.Type).As<ITrack>();
			return null;
        }

        /// <summary>
        /// Gets the list of all tracks in the group</summary>
        public IList<ITrack> Tracks
        {
            get { return GetChildList<ITrack>(Schema.groupCameraType.trackGroupCameraChild); }
			//get { return m_tracks; }
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Copies this timeline object, returning a new timeline object that is not in any timeline-related
        /// container. If the copy can't be done, null is returned.</summary>
        /// <returns>A copy of this timeline object or null if copy fails</returns>
        public virtual object Clone()
        {
            DomNode domCopy = DomNode.Copy(new DomNode[] { DomNode })[0];
            return domCopy.As<ITimelineObject>();
        }

        #endregion

		//private List<ITrack> m_tracks;
	}
}




