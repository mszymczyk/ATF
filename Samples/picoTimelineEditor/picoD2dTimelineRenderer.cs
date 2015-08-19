//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Timelines;
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
				}

				// dispose any unmanaged resources.

				m_picoDisposed = true;
			}
			// always call base regardles.
			base.Dispose( disposing );
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

		/// <summary>
		/// The brush used for drawing text on intervals, keys, and markers</summary>
		protected D2dSolidColorBrush m_picoTextBrush;
		private bool m_picoDisposed;
    }
}
