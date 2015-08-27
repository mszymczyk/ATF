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

		/// <summary>
		/// Gets and sets the camera's Field of View</summary>
		public float FieldOfView
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.fovAttribute); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.fovAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's Near Clip Plane</summary>
		public float NearClipPlane
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.nearClipPlaneAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.nearClipPlaneAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's Far Clip Plane</summary>
		public float FarClipPlane
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.farClipPlaneAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.farClipPlaneAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's camera view</summary>
		public string CameraView
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalCameraAnimType.cameraViewAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.cameraViewAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's animation file</summary>
		public string AnimFile
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalCameraAnimType.animFileAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.animFileAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's Anim Offset</summary>
		public float AnimOffset
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.animOffsetAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCameraAnimType.animOffsetAttribute, value ); }
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





