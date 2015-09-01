//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

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
	public class IntervalAnimController : Interval, ITimelineObjectCreator
    {

		///// <summary>
		///// Gets and sets the event's name</summary>
		//public override string Name
		//{
		//	get { return (string) DomNode.GetAttribute( Schema.intervalCameraAnimType.nameAttribute ); }
		//	set { DomNode.SetAttribute( Schema.intervalCameraAnimType.nameAttribute, value ); }
		//}

		///// <summary>
		///// Gets and sets the event's length (duration)</summary>
		//public override float Length
		//{
		//	get { return (float)DomNode.GetAttribute( Schema.intervalCameraAnimType.lengthAttribute ); }
		//	set
		//	{
		//		float constrained = Math.Max( value, 1 );                 // >= 1
		//		constrained = (float)MathUtil.Snap( constrained, 1.0 );   // snapped to nearest integral frame number
		//		DomNode.SetAttribute( Schema.intervalCameraAnimType.lengthAttribute, constrained );
		//	}
		//}

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.intervalCameraAnimType.colorAttribute ) ); }
		//	set { DomNode.SetAttribute( Schema.intervalCameraAnimType.colorAttribute, value.ToArgb() ); }
		//}

		/// <summary>
		/// Gets and sets the intervals's animation file</summary>
		public string AnimFile
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalAnimControllerType.animFileAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalAnimControllerType.animFileAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the intervals's Anim Offset</summary>
		public float AnimOffset
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalAnimControllerType.animOffsetAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalAnimControllerType.animOffsetAttribute, value ); }
		}

		/// <summary>
		/// Gets animation's duration in milliseconds
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimDuration", "AnimFileInfo", "Animation duration in milliseconds" )]
		public float AnimDuration
		{
			get { return ( m_afh != null ) ? m_afh.durationMilliseconds : 0; }
		}

		/// <summary>
		/// Gets animation's number of frames
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimNumFrames", "AnimFileInfo", "Animation frame count" )]
		public int AnimNumFrames
		{
			get { return ( m_afh != null ) ? m_afh.nFrames : 0; }
		}

		/// <summary>
		/// Gets animation's framerate (frames per second)
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimFramerate", "AnimFileInfo", "Animation frames per second" )]
		public float AnimFramerate
		{
			get { return ( m_afh != null ) ? m_afh.framerate : 0; }
		}

		/// <summary>
		/// Performs initialization when the adapter is connected to the DomNode.
		/// </summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			DomNode.AttributeChanged += DomNode_AttributeChanged;

			if ( AnimFile != null && AnimFile.Length > 0 )
				m_afh = pico.Anim.AnimFileHeader.ReadFromFile2( AnimFile );
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent( Schema.intervalAnimControllerType.animFileAttribute ) )
			{
				if ( AnimFile != null && AnimFile.Length > 0 )
					m_afh = pico.Anim.AnimFileHeader.ReadFromFile2( AnimFile );
			}
		}

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNode dn = new DomNode( Schema.intervalAnimControllerType.Type );
			IntervalAnimController i = dn.As<IntervalAnimController>();
			return i;
		}
		#endregion

		public override bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public override bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( !parent.Is<TrackAnimController>() )
				return false;

			return true;
		}

		private pico.Anim.AnimFileHeader m_afh;
	}
}





