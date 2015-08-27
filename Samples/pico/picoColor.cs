using System;
using System.IO;
using System.Text;
using System.Drawing;

using Sce.Atf;

namespace pico
{
	public class picoColor
	{
		public static Color fromRGB( float r, float g, float b )
		{
			float rc = MathUtil.Clamp<float>( r, 0, 1 );
			float gc = MathUtil.Clamp<float>( g, 0, 1 );
			float bc = MathUtil.Clamp<float>( b, 0, 1 );

			int ri = (int)( rc * 255 );
			int gi = (int)( gc * 255 );
			int bi = (int)( bc * 255 );

			return Color.FromArgb( 255, ri, gi, bi );
		}
	}
}
