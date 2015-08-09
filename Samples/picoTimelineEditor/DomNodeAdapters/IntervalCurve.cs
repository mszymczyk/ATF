//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalCurve : Interval, ICurveSet, ITimelineObjectCreator
    {

		/// <summary>
		/// Gets and sets the event's name</summary>
		public override string Name
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalCurveType.nameAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCurveType.nameAttribute, value ); }
		}

		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			DomNode.AttributeChanged += DomNode_AttributeChanged;

			m_curves = GetChildList<ICurve>( Schema.intervalCurveType.curveChild );
			if (m_curves.Count > 0)
				return;

			// add few sample curves.

			// add x channel
			Curve curve = (new DomNode( Schema.curveType.Type )).As<Curve>();
			curve.Name = "curve_" + Name;
			curve.DisplayName = Name;
			curve.MinX = Start;
			curve.MaxX = Start + Length;
			curve.MinY = float.MinValue;
			curve.MaxY = float.MaxValue;
			//curve.CurveColor = Color.Red;
			curve.CurveColor = Color;
			curve.PreInfinity = CurveLoopTypes.Cycle;
			curve.PostInfinity = CurveLoopTypes.Cycle;
			curve.XLabel = "x-Time";
			curve.YLabel = "x-Pos";

			IControlPoint cp = curve.CreateControlPoint();
			cp.X = Start;
			cp.Y = 0;
			curve.AddControlPoint( cp );
			cp = curve.CreateControlPoint();
			cp.X = Start + Length;
			cp.Y = 1;
			curve.AddControlPoint( cp );
			CurveUtils.ComputeTangent( curve );
			m_curves.Add( curve );
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			//if ( e.AttributeInfo.Equivalent(Schema.intervalCurveType.startAttribute) )
			//{
				foreach( Curve curve in m_curves )
				{
					curve.MinX = Start;
					curve.MaxX = Start + Length;
					curve.Name = "curve_" + Name;
					curve.DisplayName = Name;
					curve.CurveColor = Color;
				}
			//}
		}

        #region ICurveSet Members

        /// <summary>
        /// Gets list of the curves associated with an animation.</summary>
        public IList<ICurve> Curves
        {
            get { return m_curves; }
        }

        private IList<ICurve> m_curves;
        #endregion

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNode dn = new DomNode( Schema.intervalCurveType.Type );
			IntervalCurve i = dn.As<IntervalCurve>();
			return i;
		}
		#endregion
	}
}





