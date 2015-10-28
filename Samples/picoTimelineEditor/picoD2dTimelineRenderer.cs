//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

using picoTimelineEditor.DomNodeAdapters;

namespace picoTimelineEditor
{
    /// <summary>
    /// Default timeline renderer. This class is designed to be instantiated
    /// once per TimelineControl.</summary>
	public class picoD2dTimelineRenderer : Sce.Atf.Controls.Timelines.Direct2D.D2dDefaultTimelineRenderer
    {

		/// <summary>
		/// Initializes class with graphics object</summary>
		/// <param name="graphics">Graphics object for drawing</param>
		public override void Init( D2dGraphics graphics )
		{
			base.Init( graphics );

			HeaderWidth = 180;

			m_picoTextBrush = graphics.CreateSolidBrush( SystemColors.WindowText );

			D2dGradientStop[] gradstops = 
            { 
                new D2dGradientStop(Color.Gray, 0),
                new D2dGradientStop(Color.Black, 1.0f),
            };
			m_fillLinearGradientBrush = graphics.CreateLinearGradientBrush( gradstops );
		}


		public IContextRegistry ContextRegistry
		{
			get;
			set;
		}

		/// <summary>
		/// Disposes unmanaged resources</summary>
		/// <param name="disposing">Whether or not Dispose invoked this method</param>
		protected override void Dispose( bool disposing )
		{
			if ( !m_picoDisposed )
			{
				if ( disposing )
				{
					m_picoTextBrush.Dispose();
					m_fillLinearGradientBrush.Dispose();
				}

				// dispose any unmanaged resources.

				m_picoDisposed = true;
			}
			// always call base regardles.
			base.Dispose( disposing );
		}

		/// <summary>
		/// Draws an interval</summary>
		/// <param name="interval">Interval</param>
		/// <param name="bounds">Bounding rectangle, in screen space</param>
		/// <param name="drawMode">Drawing mode</param>
		/// <param name="c">Drawing context</param>
		protected override void Draw( IInterval interval, RectangleF bounds, DrawMode drawMode, Context c )
		{
			Color color = interval.Color;
			switch ( drawMode & DrawMode.States )
			{
				case DrawMode.Normal:
					RectangleF realPart = new RectangleF(
						bounds.X,
						bounds.Y,
						GdiUtil.TransformVector( c.Transform, interval.Length ),
						bounds.Height );
					bool hasTail = realPart.Width < MinimumDrawnIntervalLength;

					float h = color.GetHue();
					float s = color.GetSaturation();
					float b = color.GetBrightness();
					Color endColor = ColorUtil.FromAhsb( color.A, h, s * 0.3f, b );
					c.Graphics.FillRectangle(
						realPart,
						new PointF( 0, realPart.Top ), new PointF( 0, realPart.Bottom ),
						color, endColor );

					if ( hasTail )
					{
						endColor = ColorUtil.FromAhsb( 64, h, s * 0.3f, b );
						RectangleF tailPart = new RectangleF(
							realPart.Right,
							bounds.Y,
							bounds.Width - realPart.Width,
							bounds.Height );
						c.Graphics.FillRectangle( tailPart, endColor );
					}

					if (interval.Is<ICurveSet>())
						DrawCurves( interval.As<ICurveSet>(), bounds, c );

					// pico
					// add line at the left border of interval
					// this helps to notice where intervals begin/end when they are tightly packed next to each other
					//
					Color lineColor = ColorUtil.FromAhsb( endColor.A, 360.0f - h, 1.0f, 0.5f );
					c.Graphics.DrawLine( new PointF( realPart.Left, realPart.Top ), new PointF( realPart.Left, realPart.Bottom ), lineColor, 2 );

					if ( color.R + color.G + color.B < 3 * 160 )
						TextBrush.Color = SystemColors.HighlightText;
					else
						TextBrush.Color = SystemColors.WindowText;

					c.Graphics.DrawText( interval.Name, c.TextFormat, bounds.Location, TextBrush );

					if ( ( drawMode & DrawMode.Selected ) != 0 )
					{
						c.Graphics.DrawRectangle(
							new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
							SelectedBrush, 3.0f );
					}

					// a tiny hack to draw rectangle around selected IntervalSettings even, when selection switched to TimelineSetting
					//
					ISelectionContext selCtx = ContextRegistry.GetActiveContext<ISelectionContext>();
					if ( selCtx != null )
					{
						TimelineSetting tiSett = selCtx.LastSelected.As<TimelineSetting>();
						if ( tiSett != null )
						{
							IInterval isett = tiSett.DomNode.Parent.As<IInterval>();
							if ( object.ReferenceEquals(isett, interval) )
							{
								c.Graphics.DrawRectangle(
									new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
									SelectedBrush, 3.0f );
							}
						}
					}

					break;
				case DrawMode.Collapsed:
					c.Graphics.FillRectangle( bounds, CollapsedBrush );
					break;
				case DrawMode.Ghost:
					c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
					bool showRight = ( drawMode & DrawMode.ResizeRight ) != 0;
					float x = showRight ? bounds.Right : bounds.Left;
					c.Graphics.DrawText(
						GetXPositionString( x, c ),
						c.TextFormat,
						new PointF( x, bounds.Bottom - c.FontHeight ),
						TextBrush );
					break;
				case DrawMode.Invalid:
					c.Graphics.FillRectangle( bounds, InvalidBrush );
					break;
			}
		}

        /// <summary>
        /// Draws a key</summary>
        /// <param name="key">Key</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
        {
			LuaScript luaScript = key as LuaScript;
			if ( luaScript != null )
			{
				Color color = key.Color;
				bounds.Width = bounds.Height = KeySize; // key is always square, fixed size

				switch ( drawMode & DrawMode.States )
				{
					case DrawMode.Normal:
						//c.Graphics.FillEllipse( bounds, color );
						c.Graphics.FillRectangle( bounds, color );

						//m_picoTextBrush.Color = Color.Red;
						//c.Graphics.DrawText(
						//	luaScript.Description,
						//	c.TextFormat,
						//	new PointF( bounds.Right + 8, bounds.Y ),
						//	m_picoTextBrush );

						if ( ( drawMode & DrawMode.Selected ) != 0 )
						{
							//D2dAntialiasMode originalAntiAliasMode = c.Graphics.AntialiasMode;
							//c.Graphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
							//c.Graphics.DrawEllipse(
							//	new D2dEllipse(
							//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
							//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
							//	SelectedBrush, 3.0f );
							c.Graphics.DrawRectangle( bounds, SelectedBrush, 3.0f );
							//c.Graphics.DrawEllipse(
							//	new D2dEllipse(
							//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
							//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
							//	SelectedBrush, 3.0f ); 
							//c.Graphics.AntialiasMode = originalAntiAliasMode;
						}
						break;
					case DrawMode.Collapsed:
						//c.Graphics.FillEllipse( bounds, CollapsedBrush );
						c.Graphics.FillRectangle( bounds, CollapsedBrush );
						break;
					case DrawMode.Ghost:
						//c.Graphics.FillEllipse( bounds, Color.FromArgb( 128, color ) );
						c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
						c.Graphics.DrawText(
							GetXPositionString( bounds.Left + KeySize * 0.5f, c ),
							c.TextFormat,
							new PointF( bounds.Right + 16, bounds.Y ),
							m_picoTextBrush );
						break;
					case DrawMode.Invalid:
						//c.Graphics.FillEllipse( bounds, InvalidBrush );
						c.Graphics.FillRectangle( bounds, InvalidBrush );
						break;
				}
			}
			else
			{
				base.Draw( key, bounds, drawMode, c );
			}
        }

		protected void DrawCurves( ICurveSet curveSet, RectangleF bounds, Context c )
		{
			IList<ICurve> curveList = curveSet.Curves;
			if (curveList.Count == 0)
				return;

			ICurve curve = curveList[0];
			DrawCurve( curve, bounds, c );
		}

		//private Pen m_curvePen = new Pen( Color.Black );
		private PointF m_trans;
		private PointF m_scale;// = new Pointf( 1, 1 );

		/// <summary>
		/// Transforms x and y-coordinates from graph space to client space</summary>        
		/// <param name="x">X-coordinate to be transformed</param>
		/// <param name="y">Y-coordinate to be transformed</param>
		/// <returns>Vec2F representing transformed x and y-coordinates in client space</returns>
		public Vec2F GraphToClient( float x, float y )
		{
			Vec2F result = new Vec2F();
			y = 1.0f - y;
			result.X = (float) (m_trans.X + x * m_scale.X);
			result.Y = (float) (m_trans.Y + y * m_scale.Y);
			//result.Y = (float)Math.Floor( result.Y );
			return result;
		}

		protected void DrawCurve( ICurve curve, RectangleF bounds, Context c )
		{
			ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
			if (points.Count <= 1)
				return;

			//float rangeX = curve.MaxX;
			//float rangeY = curve.MaxY;
			m_trans = new PointF( bounds.X, bounds.Y );
			m_scale = new PointF( bounds.Width / curve.MaxX, bounds.Height / curve.MaxY );

			//float step = m_tessellation / m_canvas.Zoom.X;
			float x0 = 0;
			float x1 = curve.MaxX;
			IControlPoint fpt = points[0];
			IControlPoint lpt = points[points.Count - 1];
			float step = 8.0f / bounds.Width;
			List<PointF> pointList = new List<PointF>( (int)bounds.Width / 4 );
			ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator( curve );
			PointF scrPt = new PointF();

			m_fillLinearGradientBrush.StartPoint = bounds.Location;
			m_fillLinearGradientBrush.EndPoint = new PointF( bounds.X, bounds.Bottom );

			D2dGraphics g = c.Graphics;
			g.PushAxisAlignedClip( bounds );

			float strokeWidth = 2;

			// draw pre infinity
			//if (fpt.X > x0)
			{
				float start = x0;
				float end = Math.Min( fpt.X, x1 );
				float rangeX = end - start;
				for (float x = 0; x < rangeX; x += step)
				{
					float xv = start + x;
					float y = cv.Evaluate( xv );
					scrPt = GraphToClient( xv, y );
					//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
					pointList.Add( scrPt );
				}
				scrPt = GraphToClient( end, cv.Evaluate( end ) );
				//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
				pointList.Add( scrPt );
				if (pointList.Count > 1)
				{
					//g.DrawLines( m_infinityPen, pointList.ToArray() );
					PointF firstPoint = pointList[0];
					PointF lastPoint = pointList[pointList.Count-1];
					pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
					pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );

					//g.FillPolygon( pointList, Color.Black );
					g.FillPolygon( pointList, m_fillLinearGradientBrush );
				}
			}

			int leftIndex = 0;
			int rightIndex = points.Count - 1;
			//ComputeIndices( curve, out leftIndex, out rightIndex );
			if (curve.CurveInterpolation == InterpolationTypes.Linear)
			{
				for (int i = leftIndex; i < rightIndex; i++)
				{
					IControlPoint p1 = points[i];
					IControlPoint p2 = points[i + 1];
					PointF cp1 = GraphToClient( p1.X, p1.Y );
					PointF cp2 = GraphToClient( p2.X, p2.Y );
					//g.DrawLine( m_curvePen, cp1.X, cp1.Y, cp2.X, cp2.Y );
					//g.DrawLine( cp1.X, cp1.Y, cp2.X, cp2.Y, Color.Black, strokeWidth );
					g.FillPolygon( pointList, m_fillLinearGradientBrush );
				}
			}
			else
			{
				for (int i = leftIndex; i < rightIndex; i++)
				{
					IControlPoint p1 = points[i];
					IControlPoint p2 = points[i + 1];
					if (p1.TangentOutType == CurveTangentTypes.Stepped)
					{
						PointF cp1 = GraphToClient( p1.X, p1.Y );
						PointF cp2 = GraphToClient( p2.X, p2.Y );
						//g.DrawLine( m_curvePen, cp1.X, cp1.Y, cp2.X, cp1.Y );
						g.DrawLine( cp1.X, cp1.Y, cp2.X, cp1.Y, Color.Black, strokeWidth );
						//g.DrawLine( m_curvePen, cp2.X, cp1.Y, cp2.X, cp2.Y );
						g.DrawLine( cp2.X, cp1.Y, cp2.X, cp2.Y, Color.Black, strokeWidth );
					}
					else if (p1.TangentOutType != CurveTangentTypes.SteppedNext)
					{
						float start = Math.Max( p1.X, x0 );
						float end = Math.Min( p2.X, x1 );
						pointList.Clear();
						float rangeX = end - start;
						for (float x = 0; x < rangeX; x += step)
						{
							float xv = start + x;
							float y = cv.Evaluate( xv );
							scrPt = GraphToClient( xv, y );
							//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
							pointList.Add( scrPt );
						}
						scrPt = GraphToClient( end, cv.Evaluate( end ) );
						//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
						pointList.Add( scrPt );
						if (pointList.Count > 1)
						{
							//g.DrawLines( m_curvePen, pointList.ToArray() );
							//g.DrawLines( pointList, Color.Black, strokeWidth );
							PointF firstPoint = pointList[0];
							PointF lastPoint = pointList[pointList.Count-1];
							pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
							pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );
							//pointList.Add( new PointF( firstPoint.X, firstPoint.Y) );
							//g.DrawPolygon( pointList, Color.Black, strokeWidth );
							//g.FillPolygon( pointList, Color.Black );
							g.FillPolygon( pointList, m_fillLinearGradientBrush );
						}
					}
				}// for (int i = leftIndex; i < rightIndex; i++)
			}

			//draw post-infinity.
			//if (lpt.X < x1)
			{
				pointList.Clear();
				float start = Math.Max( x0, lpt.X );
				float end = x1;
				float rangeX = end - start;
				for (float x = 0; x < rangeX; x += step)
				{
					float xv = start + x;
					float y = cv.Evaluate( xv );
					scrPt = GraphToClient( xv, y );
					//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
					pointList.Add( scrPt );
				}
				scrPt = GraphToClient( end, cv.Evaluate( end ) );
				//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
				pointList.Add( scrPt );
				if (pointList.Count > 1)
				{
					//g.DrawLines( m_infinityPen, pointList.ToArray() );
					PointF firstPoint = pointList[0];
					PointF lastPoint = pointList[pointList.Count-1];
					pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
					pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );
					//g.FillPolygon( pointList, Color.Black );
					g.FillPolygon( pointList, m_fillLinearGradientBrush );
				}
			}

			g.PopAxisAlignedClip();
		}

		//private float m_tessellation = 4.0f;

		/// <summary>
		/// The brush used for drawing text on intervals, keys, and markers</summary>
		protected D2dSolidColorBrush m_picoTextBrush;
		private D2dLinearGradientBrush m_fillLinearGradientBrush;
		private bool m_picoDisposed;
    }
}
