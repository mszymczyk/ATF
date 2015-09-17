//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Timelines;
using picoAnimClipEditor.DomNodeAdapters;

namespace picoAnimClipEditor
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

			//m_picoTextBrush = graphics.CreateSolidBrush( SystemColors.WindowText );
		}

		///// <summary>
		///// Disposes unmanaged resources</summary>
		///// <param name="disposing">Whether or not Dispose invoked this method</param>
		//protected override void Dispose( bool disposing )
		//{
		//	if ( !m_picoDisposed )
		//	{
		//		if ( disposing )
		//		{
		//			m_picoTextBrush.Dispose();
		//		}

		//		// dispose any unmanaged resources.

		//		m_picoDisposed = true;
		//	}
		//	// always call base regardles.
		//	base.Dispose( disposing );
		//}

		///// <summary>
		///// Draws an interval</summary>
		///// <param name="interval">Interval</param>
		///// <param name="bounds">Bounding rectangle, in screen space</param>
		///// <param name="drawMode">Drawing mode</param>
		///// <param name="c">Drawing context</param>
		//protected override void Draw( IInterval interval, RectangleF bounds, DrawMode drawMode, Context c )
		//{
		//	Color color = interval.Color;
		//	switch ( drawMode & DrawMode.States )
		//	{
		//		case DrawMode.Normal:
		//			RectangleF realPart = new RectangleF(
		//				bounds.X,
		//				bounds.Y,
		//				GdiUtil.TransformVector( c.Transform, interval.Length ),
		//				bounds.Height );
		//			bool hasTail = realPart.Width < MinimumDrawnIntervalLength;

		//			float h = color.GetHue();
		//			float s = color.GetSaturation();
		//			float b = color.GetBrightness();
		//			Color endColor = ColorUtil.FromAhsb( color.A, h, s * 0.3f, b );
		//			c.Graphics.FillRectangle(
		//				realPart,
		//				new PointF( 0, realPart.Top ), new PointF( 0, realPart.Bottom ),
		//				color, endColor );

		//			if ( hasTail )
		//			{
		//				endColor = ColorUtil.FromAhsb( 64, h, s * 0.3f, b );
		//				RectangleF tailPart = new RectangleF(
		//					realPart.Right,
		//					bounds.Y,
		//					bounds.Width - realPart.Width,
		//					bounds.Height );
		//				c.Graphics.FillRectangle( tailPart, endColor );
		//			}

		//			// pico
		//			// add line at the left border of interval
		//			// this helps to notice where intervals begin/end when they are tightly packed next to each other
		//			//
		//			Color lineColor = ColorUtil.FromAhsb( endColor.A, 360.0f - h, 1.0f, 0.5f );
		//			c.Graphics.DrawLine( new PointF( realPart.Left, realPart.Top ), new PointF( realPart.Left, realPart.Bottom ), lineColor, 2 );

		//			if ( color.R + color.G + color.B < 3 * 160 )
		//				TextBrush.Color = SystemColors.HighlightText;
		//			else
		//				TextBrush.Color = SystemColors.WindowText;

		//			c.Graphics.DrawText( interval.Name, c.TextFormat, bounds.Location, TextBrush );

		//			if ( ( drawMode & DrawMode.Selected ) != 0 )
		//			{
		//				c.Graphics.DrawRectangle(
		//					new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
		//					SelectedBrush, 3.0f );
		//			}
		//			break;
		//		case DrawMode.Collapsed:
		//			c.Graphics.FillRectangle( bounds, CollapsedBrush );
		//			break;
		//		case DrawMode.Ghost:
		//			c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
		//			bool showRight = ( drawMode & DrawMode.ResizeRight ) != 0;
		//			float x = showRight ? bounds.Right : bounds.Left;
		//			c.Graphics.DrawText(
		//				GetXPositionString( x, c ),
		//				c.TextFormat,
		//				new PointF( x, bounds.Bottom - c.FontHeight ),
		//				TextBrush );
		//			break;
		//		case DrawMode.Invalid:
		//			c.Graphics.FillRectangle( bounds, InvalidBrush );
		//			break;
		//	}
		//}

		///// <summary>
		///// Draws a key</summary>
		///// <param name="key">Key</param>
		///// <param name="bounds">Bounding rectangle, computed during layout phase</param>
		///// <param name="drawMode">Drawing mode</param>
		///// <param name="c">Drawing context</param>
		//protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
		//{
		//	LuaScript luaScript = key as LuaScript;
		//	if ( luaScript != null )
		//	{
		//		Color color = key.Color;
		//		bounds.Width = bounds.Height = KeySize; // key is always square, fixed size

		//		switch ( drawMode & DrawMode.States )
		//		{
		//			case DrawMode.Normal:
		//				//c.Graphics.FillEllipse( bounds, color );
		//				c.Graphics.FillRectangle( bounds, color );

		//				//m_picoTextBrush.Color = Color.Red;
		//				//c.Graphics.DrawText(
		//				//	luaScript.Description,
		//				//	c.TextFormat,
		//				//	new PointF( bounds.Right + 8, bounds.Y ),
		//				//	m_picoTextBrush );

		//				if ( ( drawMode & DrawMode.Selected ) != 0 )
		//				{
		//					//D2dAntialiasMode originalAntiAliasMode = c.Graphics.AntialiasMode;
		//					//c.Graphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
		//					//c.Graphics.DrawEllipse(
		//					//	new D2dEllipse(
		//					//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
		//					//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
		//					//	SelectedBrush, 3.0f );
		//					c.Graphics.DrawRectangle( bounds, SelectedBrush, 3.0f );
		//					//c.Graphics.DrawEllipse(
		//					//	new D2dEllipse(
		//					//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
		//					//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
		//					//	SelectedBrush, 3.0f ); 
		//					//c.Graphics.AntialiasMode = originalAntiAliasMode;
		//				}
		//				break;
		//			case DrawMode.Collapsed:
		//				//c.Graphics.FillEllipse( bounds, CollapsedBrush );
		//				c.Graphics.FillRectangle( bounds, CollapsedBrush );
		//				break;
		//			case DrawMode.Ghost:
		//				//c.Graphics.FillEllipse( bounds, Color.FromArgb( 128, color ) );
		//				c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
		//				c.Graphics.DrawText(
		//					GetXPositionString( bounds.Left + KeySize * 0.5f, c ),
		//					c.TextFormat,
		//					new PointF( bounds.Right + 16, bounds.Y ),
		//					m_picoTextBrush );
		//				break;
		//			case DrawMode.Invalid:
		//				//c.Graphics.FillEllipse( bounds, InvalidBrush );
		//				c.Graphics.FillRectangle( bounds, InvalidBrush );
		//				break;
		//		}
		//	}
		//	else
		//	{
		//		base.Draw( key, bounds, drawMode, c );
		//	}
		//}

		///// <summary>
		///// The brush used for drawing text on intervals, keys, and markers</summary>
		//protected D2dSolidColorBrush m_picoTextBrush;
		//private bool m_picoDisposed;
    }
}
