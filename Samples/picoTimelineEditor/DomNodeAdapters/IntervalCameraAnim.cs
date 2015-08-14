//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalCameraAnim : Interval, ITimelineObjectCreator
    {

		/// <summary>
		/// Gets and sets the event's name</summary>
		public override string Name
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalCameraAnimType.nameAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.nameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the event's length (duration)</summary>
		public override float Length
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.lengthAttribute ); }
			set
			{
				float constrained = Math.Max( value, 1 );                 // >= 1
				constrained = (float)MathUtil.Snap( constrained, 1.0 );   // snapped to nearest integral frame number
				DomNode.SetAttribute( Schema.intervalCameraAnimType.lengthAttribute, constrained );
			}
		}

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.intervalCameraAnimType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.colorAttribute, value.ToArgb() ); }
		}

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNode dn = new DomNode( Schema.intervalCameraAnimType.Type );
			IntervalCameraAnim i = dn.As<IntervalCameraAnim>();
			return i;
		}
		#endregion
	}
}





