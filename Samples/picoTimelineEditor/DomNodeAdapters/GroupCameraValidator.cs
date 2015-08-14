using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Validator that checks for special rules related to GroupCamera
	/// Only certain type of tracks and intervals can be added to this group
	/// </summary>
    public class GroupCameraValidator : Validator
    {
        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
        }

        /// <summary>
        /// Performs custom actions after validation finished</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnEnding(object sender, System.EventArgs e)
        {
			// this functionality must be handled in OnEnding, not OnEnded in order for history (undo/redo) to work correctly
			// (history context adds command in OnEnded even if transaction was cancelled)
			//
            foreach (Pair<Pair<DomNode, DomNode>, ChildInfo> pair in m_childChanges)
            {
                DomNode parent = pair.First.First;
                DomNode child = pair.First.Second;
				GroupCamera childGroupCamera = child.As<GroupCamera>();
				if ( childGroupCamera != null )
				{
					foreach( DomNode existingChild in parent.Children )
					{
						GroupCamera otherGroupCamera = existingChild.As<GroupCamera>();
						if ( otherGroupCamera != null && !childGroupCamera.Equals(otherGroupCamera) )
							throw new InvalidTransactionException( "Only one CameraGroup is allowed!" );
					}
				}

				TrackCameraAnim childTrackCameraAnim = child.As<TrackCameraAnim>();
				if ( childTrackCameraAnim != null )
				{
					GroupCamera parentGroupCamera = parent.As<GroupCamera>();
					if ( parentGroupCamera != null )
					{
						foreach ( DomNode existingChild in parent.Children )
						{
							TrackCameraAnim otherTrackCameraAnim = existingChild.As<TrackCameraAnim>();
							if ( otherTrackCameraAnim != null && !childTrackCameraAnim.Equals( otherTrackCameraAnim ) )
								throw new InvalidTransactionException( "Only one TrackCameraAnim is allowed!" );
						}
					}
					else
					{
						throw new InvalidTransactionException( "TrackCameraAnim can be parented only to GroupCamera" );
					}
				}

				IntervalCameraAnim childIntervalCameraAnim = child.As<IntervalCameraAnim>();
				if ( childIntervalCameraAnim != null )
				{
					TrackCameraAnim parentTrackCameraAnim = parent.As<TrackCameraAnim>();
					if ( parentTrackCameraAnim != null )
					{

					}
					else
					{
						throw new InvalidTransactionException( "CameraAnim can be parented only to TrackCameraAnim" );
					}
				}
            }
            m_childChanges.Clear();
        }

        /// <summary>
        /// Performs custom actions when validation cancelled</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnCancelled(object sender, System.EventArgs e)
        {
            m_childChanges.Clear();
        }

        //pairs of parent and child; todo: use the Tuple in .Net 4.0
        private HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>> m_childChanges =
            new HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>>();
    }
}
