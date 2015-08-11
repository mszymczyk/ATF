//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;

namespace picoTimelineEditor
{
	/// <summary>
	/// Timeline document, as a DOM hierarchy and identified by an URI. Each TimelineControl has one
	/// or more TimelineDocuments.</summary>
	public class picoTimelineControl : D2dTimelineControl
	{
		public picoTimelineControl(
			ITimelineDocument timelineDocument,
			D2dTimelineRenderer timelineRenderer,
			TimelineConstraints timelineConstraints,
			bool createDefaultManipulators )
			: base(timelineDocument, timelineRenderer, timelineConstraints, createDefaultManipulators)
		{
		}

		/// <summary>
		/// Constrains one world coordinate of a timeline object that might be moved or resized</summary>
		/// <param name="offset">Timeline world coordinate</param>
		/// <returns>Constrained frame offset</returns>
		/// <remarks>Default constrains offsets to integral values, forcing all move and resize
		/// operations to maintain integral start and length properties.</remarks>
		public override float ConstrainFrameOffset( float offset )
		{
			//return (float)Math.Round(offset);
			return (float)MathUtil.Snap( offset, 10.0f );
		}
	}
}
