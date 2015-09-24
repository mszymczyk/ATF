//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;
using System.Collections.Generic;

using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Timeline</summary>
    public class Timeline : DomNodeAdapter, IHierarchicalTimeline, IHierarchicalTimelineList
    {
        #region ITimeline Members

        /// <summary>
        /// Creates a new group</summary>
        /// <returns>New group</returns>
        public IGroup CreateGroup()
        {
            Group group = new DomNode(Schema.groupType.Type).As<Group>();

			NodeTypePaletteItem paletteItem = Schema.groupType.Type.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = Schema.groupType.Type.IdAttribute;
			if ( paletteItem != null &&
                idAttribute != null )
			{
				group.DomNode.SetAttribute( idAttribute, paletteItem.Name );
			}

			return group;
        }

        /// <summary>
        /// Creates a new marker</summary>
        /// <returns>New marker</returns>
        public IMarker CreateMarker()
        {
            return new DomNode(Schema.markerType.Type).As<IMarker>();
        }

        /// <summary>
        /// Gets the list of all groups in the timeline</summary>
        public IList<IGroup> Groups
        {
            get { return GetChildList<IGroup>(Schema.timelineType.groupChild); }
        }

        /// <summary>
        /// Gets the list of all markers in the timeline</summary>
        public IList<IMarker> Markers
        {
            get { return GetChildList<IMarker>(Schema.timelineType.markerChild); }
        }

        #endregion

        #region IHierarchicalTimeline

        /// <summary>
        /// Gets the references owned by this timeline. This is not a recursive enumeration.</summary>
        IEnumerable<ITimelineReference> IHierarchicalTimeline.References
        {
            get { return GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild); }
        }

        #endregion

        #region IHierarchicalTimelineList

        /// <summary>
        /// Gets an IList that allows for adding, removing, counting, clearing, etc., the list
        /// of ITimelineReferences</summary>
        public IList<ITimelineReference> References
        {
            get { return GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild); }
        }

        #endregion

        // Only client-specific code can create a new ITimelineReference and add it.
        internal void AddReference(ITimelineReference reference)
        {
            GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild).Add(reference);
        }

		/// <summary>
		/// Gets and sets the timelines's animation userName</summary>
		public string AnimFilename
		{
			get { return (string) DomNode.GetAttribute( Schema.timelineType.animFilenameAttribute ); }
			set { DomNode.SetAttribute( Schema.timelineType.animFilenameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the timelines's animation userName</summary>
		public string AnimUserName
		{
			get { return (string) DomNode.GetAttribute( Schema.timelineType.animUserNameAttribute ); }
			set { DomNode.SetAttribute( Schema.timelineType.animUserNameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the timelines's animation category</summary>
		public string AnimCategory
		{
			get { return (string) DomNode.GetAttribute( Schema.timelineType.animCategoryAttribute ); }
			set { DomNode.SetAttribute( Schema.timelineType.animCategoryAttribute, value ); }
		}

		///// <summary>
		///// Performs initialization when the adapter is connected to the DomNode.
		///// Raises the DomNodeAdapter NodeSet event. Creates read only data for animdata
		///// </summary>
		//protected override void OnNodeSet()
		//{
		//	base.OnNodeSet();
		//}

		public void setAnimFile( string animFile, picoAnimListEditorElement ale )
		{
			if ( ! picoAnimClipDomValidator.ParentHasChildOfType<GroupAnim>( DomNode ) )
			{
				if ( ale != null )
				{
					AnimFilename = animFile;
					AnimUserName = ale.UserName;
					AnimCategory = ale.Category;
				}

				IGroup group = CreateGroup( Schema.groupAnimType.Type );
				Groups.Add( group );
				
				GroupAnim groupAnim = group.Cast<GroupAnim>();
				ITrack track = groupAnim.CreateTrack( Schema.trackAnimType.Type );
				group.Tracks.Add( track );
				if ( ale != null )
					track.Name = ale.UserName;
				else
					track.Name = Path.GetFileNameWithoutExtension( animFile );

				IInterval interval = new DomNode( Schema.intervalAnimType.Type ).Cast<IInterval>();
				track.Intervals.Add( interval );
				interval.Length = 1000;
				interval.Name = "Anim";

				// add extra track for sound effects
				//
				IGroup groupUser = CreateGroup( Schema.groupType.Type );
				Groups.Add( groupUser );

				ITrack trackSfx = (new DomNode( Schema.trackType.Type )).Cast<Track>();
				groupUser.Tracks.Add( trackSfx );
				trackSfx.Name = "SfxTrack";
			}

			foreach( IGroup group in Groups )
			{
				GroupAnim groupAnim = group.As<GroupAnim>();
				if ( groupAnim != null )
				{
					foreach( ITrack track in group.Tracks )
					{
						TrackAnim trackAnim = track.As<TrackAnim>();
						if ( trackAnim != null )
						{
							trackAnim.AnimFile = animFile;
							System.Diagnostics.Debug.Assert( track.Intervals.Count == 1 );
							Interval interval = track.Intervals[0].Cast<Interval>();
							if ( interval.Length != trackAnim.AnimDuration )
							{
								interval.Length = trackAnim.AnimDuration;
							}

							break;
						}
					}

					break;
				}
			}
		}

		/// <summary>
		/// Creates a new group of given type</summary>
		/// <returns>New group</returns>
		public IGroup CreateGroup( DomNodeType type )
		{
			Group group = new DomNode( type ).As<Group>();

			NodeTypePaletteItem paletteItem = type.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = type.IdAttribute;
			if ( paletteItem != null &&
                idAttribute != null )
			{
				group.DomNode.SetAttribute( idAttribute, paletteItem.Name );
			}

			return group;
		}
	}
}




