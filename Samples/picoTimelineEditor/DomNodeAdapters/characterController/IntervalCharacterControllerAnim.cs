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
	public class IntervalCharacterControllerAnim : Interval, ITimelineObjectCreator
    {

		///// <summary>
		///// Gets and sets the event's name</summary>
		//public override string Name
		//{
		//	get { return (string) DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.nameAttribute ); }
		//	set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.nameAttribute, value ); }
		//}

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.colorAttribute ) ); }
		//	set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.colorAttribute, value.ToArgb() ); }
		//}

		/// <summary>
		/// Gets and sets the camera's animation file</summary>
		public string AnimFile
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.animFileAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.animFileAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's Anim Offset</summary>
		public float AnimOffset
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.animOffsetAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.animOffsetAttribute, value ); }
		}

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNode dn = new DomNode( Schema.intervalCharacterControllerAnimType.Type );
			IntervalCharacterControllerAnim i = dn.As<IntervalCharacterControllerAnim>();
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
			if ( !parent.Is<TrackCharacterControllerAnim>() )
				return false;

			return true;
		}
	}
}





