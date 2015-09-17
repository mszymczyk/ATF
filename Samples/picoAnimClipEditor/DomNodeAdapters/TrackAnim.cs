//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

namespace picoAnimClipEditor.DomNodeAdapters
{
	/// <summary>
	/// Adapts DomNode to a Track</summary>
	public class TrackAnim : DomNodeAdapter, AnimClipElementValidationInterface
	{
		/// <summary>
		/// Returns the Name property. Useful for debugging purposes.</summary>
		/// <returns>Name property</returns>
		public override string ToString()
		{
			Track track = DomNode.Cast<Track>();
			return track.Name;
		}

		public virtual bool CanParentTo( DomNode parent )
		{
			return true;
		}

		public virtual bool Validate( DomNode parent )
		{
			return true;
		}
	}
}



