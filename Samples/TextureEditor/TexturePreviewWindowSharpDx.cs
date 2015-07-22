//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Sce.Atf;
using Sce.Atf.Applications;

using AtfKeys = Sce.Atf.Input.Keys;
using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;
using AtfMouseEventArgs = Sce.Atf.Input.MouseEventArgs;
using AtfMessage = Sce.Atf.Input.Message;
using AtfDragEventArgs = Sce.Atf.Input.DragEventArgs;

using WfKeys = System.Windows.Forms.Keys;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using WfMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using WfMessage = System.Windows.Forms.Message;
using WfDragEventArgs = System.Windows.Forms.DragEventArgs;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using System.Collections.Generic;

namespace TextureEditor
{
    /// <summary>
    /// A control for using OpenGL to do the painting</summary>
    /// <remarks>This class's constructor initializes OpenGL so that other tools that use OpenGL, such as
    /// the Texture Manager, work even if BeginPaint has not been called. This allows
    /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
	public class TexturePreviewWindowSharpDX : InteropControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>This constructor initializes OpenGL so that other tools that use OpenGL, such as
        /// the Texture Manager, work even if BeginPaint has not been called. This allows
        /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
		public TexturePreviewWindowSharpDX( IContextRegistry contextRegistry )
        {
			m_textureSelectionContext = new TextureSelectionContext( contextRegistry );
			SizeChanged += new EventHandler( this.MyButton1_SizeChanged );
            StartSharpDxIfNecessary();
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
             StopSharpDx(disposing);
             base.Dispose(disposing);
        }

		[StructLayout(LayoutKind.Explicit, Size = 64)]
		struct ConstantBufferData
		{
			[FieldOffset(0)]
			public int xOffset;
			[FieldOffset(4)]
			public int yOffset;
			[FieldOffset(8)]
			public int mipLevel;
			[FieldOffset(12)]
			public int sliceIndex;
			[FieldOffset( 16 )]
			public float gamma;
		};

		class PsShaderWrap : IDisposable
		{
			public PsShaderWrap( Device device, string sourceCode, string entryName )
			{
				var pixelShaderByteCode = ShaderBytecode.Compile(sourceCode, entryName, "ps_4_0", ShaderFlags.None, EffectFlags.None);
				m_ps = new PixelShader(device, pixelShaderByteCode);
			}

			~ PsShaderWrap()
			{
				Dispose(false);
			}

			public void Dispose( bool disposing )
			{
				if (!disposing)
					return;

				if (m_ps != null)
				{
					m_ps.Dispose();
					m_ps = null;
				}
			}
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public PixelShader m_ps;
		}

		void SubmitFullscreenQuad()
		{
			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-1, -1, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1, -1, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1,  1, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  1, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}

		void DrawBackground()
		{
			m_context.PixelShader.Set( GetPsShader( "PS_Clear2" ) );
			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-1,  0, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1,  1, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  1, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}
		void DrawGradient()
		{
			m_context.PixelShader.Set( GetPsShader( "PS_Gradient" ) );

			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
									  //new Vector4(-0.95f,  0.15f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
									  //new Vector4( 0.95f,  0.15f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
									  //new Vector4(-0.95f,  0.35f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
									  //new Vector4( 0.95f,  0.35f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                      new Vector4(-1,  0.15f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0.15f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4(-1,  0.35f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0.35f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}

		void DrawTexture()
		{
			int textureFileWidth = 1;
			int textureFileHeight = 1;
			bool cubeMap = false;

			Texture2D tex2D = m_tex as Texture2D;
			if ( tex2D != null )
			{
				Texture2DDescription td = tex2D.Description;

				if ( (td.OptionFlags & ResourceOptionFlags.TextureCube) != 0 )
				{
					cubeMap = true;
				}

				if ( cubeMap )
				{
					textureFileWidth = td.Width * 4;
					textureFileHeight = td.Height * 3;
					cubeMap = true;
				}
				else
				{
					textureFileWidth = td.Width;
					textureFileHeight = td.Height;
				}
			}
			else
			{
				throw new Exception( "Unsupported resource dimension" );
			}

			//float texW = 2.0f;
			//float texH = 2.0f;
			float windowAspect = (float)ClientSize.Width / (float)ClientSize.Height;

			if ( fitSizeRequest )
			{
				fitSizeRequest = false;

				m_texW = 2 * (float)textureFileWidth / (float)ClientSize.Width;
				m_texH = 2 * (float)textureFileHeight / (float)ClientSize.Height;
			}
			else if ( fitInWindowRequest )
			{
				fitInWindowRequest = false;

				m_texW = 2.0f;
				m_texH = 2.0f;
				float texAspect = (float)textureFileWidth / (float)textureFileHeight;
				if ( windowAspect > texAspect )
				{
					//texW = (float)srvDesc.Height / (float)srvDesc.Width;
					m_texW = (float)textureFileWidth / (float)textureFileHeight;
					m_texW *= 2;
					m_texW *= (float)ClientSize.Height / (float)ClientSize.Width;
					//texW *= windowAspect;
				}
				else
				{
					m_texH = (float)textureFileHeight / (float)textureFileWidth;
					m_texH *= 2;
					m_texH *= windowAspect;
				}
			}

			m_context.PixelShader.SetSampler( 0, m_ssPoint );
			m_context.PixelShader.SetShaderResource( 0, m_texSRV );

			float texWh = m_texW * 0.5f * m_texScale;
			float texHh = m_texH * 0.5f * m_texScale;
			float xOffset = m_texPosition.X * m_texScale;
			float yOffset = m_texPosition.Y * m_texScale;

			if ( cubeMap )
			{
				// draw background
				//
				{
					// Instantiate Vertex buffer from vertex data
					var verticesBg = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
										  {
											  new Vector4(-texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
											  new Vector4( texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
											  new Vector4(-texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
											  new Vector4( texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
										  } );

					m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( verticesBg, 32, 0 ) );
					verticesBg.Dispose();

					m_context.PixelShader.Set( GetPsShader( "PS_Clear" ) );
					m_context.Draw( 4, 0 );
				}

				// draw cubemap
				//

				PsShaderWrap ps = null;
				//m_psDictionary.TryGetValue( "PS_Tex2DArray_Sample", out ps );
				m_psDictionary.TryGetValue( "PS_TexCube_Sample", out ps );			
				m_context.PixelShader.Set( ps.m_ps );

				ConstantBufferData data = new ConstantBufferData();
				int visibleMip = Math.Min( m_texProperties.VisibleMip, m_texProperties.MipLevels - 1 );
				data.mipLevel = visibleMip;
				int visibleSlice = Math.Min( m_texProperties.VisibleSlice, m_texProperties.ArraySize - 1 );
				data.sliceIndex = visibleSlice;
				if ( m_texProperties.DoGammaToLinearConversion )
					data.gamma = 2.2f;
				else
					data.gamma = 1.0f;

				var constantBuffer = Buffer.Create( m_device, BindFlags.ConstantBuffer, ref data );
				m_context.PixelShader.SetConstantBuffer( 0, constantBuffer );
				constantBuffer.Dispose();

				float tw = ( texWh * 2 ) / 4;
				float th = ( texHh * 2 ) / 3;
				float tx = -texWh + xOffset + 0;
				float ty = -texHh + yOffset + 0;

				// cubemap layout
				// https://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
				//

				var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
									  {
  										  // face 1
										  //
										  new Vector4(tx,			ty + th,	0.5f, 1.0f), new Vector4(0, 1, 1, 1),
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(1, 1, 1, 1),
										  new Vector4(tx,			ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 1, 1),
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 1, 1),

										  // face 4
										  //
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(0, 1, 4, 1),
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 4, 1),
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 4, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 4, 1),

										  // face 0
										  //
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(0, 1, 0, 1),
										  new Vector4(tx + tw*3,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 0, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 0, 1),
										  new Vector4(tx + tw*3,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 0, 1),

										  // face 5
										  //
										  new Vector4(tx + tw*3,	ty + th,	0.5f, 1.0f), new Vector4(0, 1, 5, 1),
										  new Vector4(tx + tw*4,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 5, 1),
										  new Vector4(tx + tw*3,	ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 5, 1),
										  new Vector4(tx + tw*4,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 5, 1),

										  // face 2
										  //
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(0, 1, 2, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 1, 2, 1),
										  new Vector4(tx + tw,		ty + th*3,	0.5f, 1.0f), new Vector4(0, 0, 2, 1),
										  new Vector4(tx + tw*2,	ty + th*3,	0.5f, 1.0f), new Vector4(1, 0, 2, 1),

										  // face 3
										  //
										  new Vector4(tx + tw,		ty,			0.5f, 1.0f), new Vector4(0, 1, 3, 1),
										  new Vector4(tx + tw*2,	ty,			0.5f, 1.0f), new Vector4(1, 1, 3, 1),
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(0, 0, 3, 1),
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(1, 0, 3, 1)

						  } );

				m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
				vertices.Dispose();

				m_context.Draw( 24, 0 );
			}
			else
			{
				// Instantiate Vertex buffer from vertex data
				var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
									  {
										  new Vector4(-texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
										  new Vector4( texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
										  new Vector4(-texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
										  new Vector4( texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
									  } );

				m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
				vertices.Dispose();

				// draw background
				//
				m_context.PixelShader.Set( GetPsShader( "PS_Clear" ) );
				m_context.Draw( 4, 0 );


				ConstantBufferData data = new ConstantBufferData();
				int visibleMip = Math.Min( m_texProperties.VisibleMip, m_texProperties.MipLevels - 1 );
				data.mipLevel = visibleMip;
				int visibleSlice = Math.Min( m_texProperties.VisibleSlice, m_texProperties.ArraySize - 1 );
				data.sliceIndex = visibleSlice;
				if ( m_texProperties.DoGammaToLinearConversion )
					data.gamma = 2.2f;
				else
					data.gamma = 1.0f;

				var constantBuffer = Buffer.Create( m_device, BindFlags.ConstantBuffer, ref data );
				m_context.PixelShader.SetConstantBuffer( 0, constantBuffer );
				constantBuffer.Dispose();

				PsShaderWrap ps = null;

				if ( m_texSRV.Description.Dimension == ShaderResourceViewDimension.Texture2D )
				{
					m_psDictionary.TryGetValue( "PS_Tex2D_Sample", out ps );
					m_context.PixelShader.Set( ps.m_ps );
				}
				else if ( m_texSRV.Description.Dimension == ShaderResourceViewDimension.Texture2DArray )
				{
					m_psDictionary.TryGetValue( "PS_Tex2DArray_Sample", out ps );
				}

				if ( ps == null )
				{
				}
				else
				{
					m_context.PixelShader.Set( ps.m_ps );
					m_context.Draw( 4, 0 );
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
		protected override void OnPaint( PaintEventArgs e )
		{
			m_context.InputAssembler.InputLayout = m_inputLayout;
			m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			m_context.VertexShader.Set( m_vsPassThrough );

			BlendStateDescription bsd = BlendStateDescription.Default();

			RenderTargetBlendDescription bsd0 = bsd.RenderTarget[0];
			bsd0.IsBlendEnabled = true;
			bsd0.BlendOperation = BlendOperation.Add;
			bsd0.AlphaBlendOperation = BlendOperation.Add;
			bsd0.SourceBlend = BlendOption.SourceAlpha;
			bsd0.SourceAlphaBlend = BlendOption.SourceAlpha;
			bsd0.DestinationBlend = BlendOption.InverseSourceAlpha;
			bsd0.DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
			bsd.RenderTarget[0] = bsd0;

			BlendState bs = new BlendState( m_device, bsd );
			m_context.OutputMerger.SetBlendState( bs );
			bs.Dispose();

			m_context.OutputMerger.SetTargets( m_renderView );
			m_context.ClearRenderTargetView( m_renderView, Color.DarkGray );
			//m_context.OutputMerger.SetTargets( m_renderViewIntermediate );
			//m_context.ClearRenderTargetView( m_renderViewIntermediate, Color.DarkGray );

			RasterizerStateDescription rsd = RasterizerStateDescription.Default();
			rsd.IsFrontCounterClockwise = true;
			RasterizerState rs = new RasterizerState( m_device, rsd );
			m_context.Rasterizer.State = rs;
			rs.Dispose();
			m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );

			//DrawBackground();
			if ( m_tex != null )
				DrawTexture();

			//DrawGradient();

			//m_context.OutputMerger.SetTargets( m_renderView );

			//BlendStateDescription bsd2 = BlendStateDescription.Default();
			//BlendState bs2 = new BlendState( m_device, bsd2 );
			//m_context.OutputMerger.SetBlendState( bs2 );
			//bs2.Dispose();

			//m_context.PixelShader.Set( GetPsShader( "PS_Present" ) );
			//m_context.PixelShader.SetSampler( 0, m_ssPoint );
			//m_context.PixelShader.SetShaderResource( 0, m_renderViewIntermediateSrv );
			//SubmitFullscreenQuad();

			m_swapChain.Present( 0, PresentFlags.None );
		}

        /// <summary>
        /// Performs custom actions after the PaintBackground event occurs</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
			//base.OnPaintBackground( e );
        }

        /// <summary>
        /// Performs custom actions after the <see cref="E:System.Windows.Forms.Control.FontChanged"></see> event event occurs</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (m_isStarted)
                Invalidate();
        }

        //protected override void OnGotFocus(EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("GotFocus");
        //}

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // e.Delta is by default multiple of WHEEL_DELTA which for current versions of windows is 120
            //
            int delta = e.Delta / 120;
            float mult = 1.1f;
            if (delta < 0)
                mult = 0.9f;

			float scaleX = -0.1f / (float)ClientSize.Width;
			float scaleY = 0.1f / (float)ClientSize.Height;
			int xDiff = e.Location.X - ClientSize.Width / 2;
			int yDiff = e.Location.Y - ClientSize.Height / 2;

            for (int idetent = 0; idetent < Math.Abs(delta); ++idetent)
            {
                m_texScale *= mult;

				m_texPosition += new Vector4((float)xDiff * scaleX, (float)yDiff * scaleY, 0.0f, 0.0f);
			}

            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            m_leftMouseButtonDown = true;
            m_mouseMoveStartLocation_ = e.Location;
            Focus();
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_leftMouseButtonDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ( m_leftMouseButtonDown )
            {
                System.Drawing.Size s = new System.Drawing.Size(m_mouseMoveStartLocation_);
                m_mouseMoveStartLocation_ = e.Location;
                System.Drawing.Point locDiff = e.Location - s;

                float scaleX = 2.0f / (float)ClientSize.Width;
                float scaleY = -2.0f / (float)ClientSize.Height;

                m_texPosition += new Vector4((float)locDiff.X * scaleX, (float)locDiff.Y * scaleY, 0.0f, 0.0f);
                Invalidate();
            }
            base.OnMouseMove(e);
        }

		///// <summary>
		///// Begins painting</summary>
		///// <exception cref="InvalidOperationException">Can't make this panel's GL context to be current</exception>
		//protected virtual void BeginPaint()
		//{
		//	StartSharpDxIfNecessary();
		//	//if (!Wgl.wglMakeCurrent(m_hdc, m_hglrc))
		//	//{
		//	//	Util3D.ReportErrors();
		//	//	throw new InvalidOperationException("Can't make this panel's GL context to be current");
		//	//}
		//}

		///// <summary>
		///// Whether the OpenGl buffers should be swapped by the EndPaint method. Is always set to true when
		///// EndPaint finishes. Default is true.</summary>
		//protected bool SwapBuffers = true;

		///// <summary>
		///// Ends painting</summary>
		//protected virtual void EndPaint()
		//{
		//	//if (SwapBuffers)
		//	//	Gdi.SwapBuffers(m_hdc);
		//	//SwapBuffers = true;
		//	m_swapChain.Present( 0, PresentFlags.None );
		//}

		private void MyButton1_SizeChanged(object sender, System.EventArgs e)
		{
			m_renderView.Dispose();

			//m_renderViewIntermediate.Dispose();
			//m_renderViewIntermediateSrv.Dispose();

			m_context.Flush();
			m_context.ClearState();

			// Create Device and SwapChain
			m_swapChain.ResizeBuffers( 1, ClientSize.Width, ClientSize.Height, backBufferFormat, SwapChainFlags.None );

			ResizeBackBuffer();

			//m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
			//m_context.OutputMerger.SetTargets( m_renderView );

            Invalidate();

			//System.Diagnostics.Debug.WriteLine("ClientSize: {0}", ClientSize);
		}

        /// <summary>
        /// Client entry to initialize custom OpenGL resources</summary>
        protected virtual void Initialize() 
        {
            // Compile Vertex and Pixel shaders

            Assembly assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();

            var resourceName = "TextureEditor.Resources.shaders.fx";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                m_shadersFxText = reader.ReadToEnd();
            }

            var vertexShaderByteCode = ShaderBytecode.Compile(m_shadersFxText, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_vsPassThrough = new VertexShader(m_device, vertexShaderByteCode);

            // Layout from VertexShader input signature
            m_inputLayout = new InputLayout(
                m_device,
                ShaderSignature.GetInputSignature(vertexShaderByteCode),
                new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            SamplerStateDescription ssd = SamplerStateDescription.Default();
            ssd.Filter = Filter.MinMagMipPoint;
            m_ssPoint = new SamplerState(m_device, ssd);


			AddPsShader( "PS_Clear" );
			AddPsShader( "PS_Clear2" );
			AddPsShader( "PS_Tex2D_Sample" );
			AddPsShader("PS_Tex2DArray_Sample");
			AddPsShader("PS_Tex2D_Load");
			AddPsShader( "PS_TexCube_Sample" );
			AddPsShader( "PS_Present" );
			AddPsShader( "PS_Gradient" );
		}

        /// <summary>
        /// Client entry to unload custom OpenGL resources</summary>
        protected virtual void Shutdown() 
        {
			//if (m_psColor != null)
			//{
			//	m_psColor.Dispose();
			//	m_psColor = null;
			//}
			//if (m_psTex2DLoad != null)
			//{
			//	m_psTex2DLoad.Dispose();
			//	m_psTex2DLoad = null;
			//} 
			foreach( PsShaderWrap ps in m_psDictionary.Values )
			{
				ps.Dispose();
			}
			m_psDictionary.Clear();

			if (m_vsPassThrough != null)
            {
                m_vsPassThrough.Dispose();
                m_vsPassThrough = null;
            }

            if ( m_ssPoint != null )
            {
                m_ssPoint.Dispose();
                m_ssPoint = null;
            }

            if ( m_tex != null )
            {
                m_tex.Dispose();
                m_tex = null;
            }
            if ( m_texSRV != null )
            {
                m_texSRV.Dispose();
                m_texSRV = null;
            }
        }

		void AddPsShader( string entryName )
		{
			m_psDictionary.Add(entryName, new PsShaderWrap(m_device, m_shadersFxText, entryName));
		}

		PixelShader GetPsShader( string entryName )
		{
			PsShaderWrap ps = null;
			m_psDictionary.TryGetValue( entryName, out ps );
			return ps.m_ps;
		}

		void ResizeBackBuffer()
		{
			// New RenderTargetView from the backbuffer
			var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
			m_renderView = new RenderTargetView( m_device, backBuffer );

			//Texture2DDescription td = new Texture2DDescription();
			//td.ArraySize = 1;
			//td.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
			//td.CpuAccessFlags = CpuAccessFlags.None;
			//td.Format = Format.R32G32B32A32_Float;
			//td.Height = backBuffer.Description.Height;
			//td.MipLevels = 1;
			//td.OptionFlags = ResourceOptionFlags.None;
			//td.SampleDescription.Count = 1;
			//td.SampleDescription.Quality = 0;
			//td.Usage = ResourceUsage.Default;
			//td.Width = backBuffer.Description.Width;
			//Texture2D rt = new Texture2D( m_device, td );

			//m_renderViewIntermediate = new RenderTargetView( m_device, rt );
			//m_renderViewIntermediateSrv = new ShaderResourceView( m_device, rt );
			//rt.Dispose();
			backBuffer.Dispose();
		}

        private void StartSharpDxIfNecessary()
        {
            if (!m_isStarted)
            {
                try
                {
                    // Attempt To Get A Device Context
					//m_hdc = User.GetDC( Handle );
					//if (m_hdc == IntPtr.Zero)
					//	throw new InvalidOperationException( "Can't get device context" );

					// SwapChain description
					var desc = new SwapChainDescription()
					{
						BufferCount = 1,
						ModeDescription= 
								   new ModeDescription( ClientSize.Width, ClientSize.Height,
												new Rational( 60, 1 ), backBufferFormat ),
						IsWindowed = true,
						OutputHandle = Handle,
						SampleDescription = new SampleDescription( 1, 0 ),
						SwapEffect = SwapEffect.Discard,
						Usage = Usage.RenderTargetOutput
					};

					// Create Device and SwapChain
					Device.CreateWithSwapChain( DriverType.Hardware, DeviceCreationFlags.Debug, desc, out m_device, out m_swapChain );
					m_context = m_device.ImmediateContext;

					// Ignore all windows events
					var factory = m_swapChain.GetParent<Factory>();
					factory.MakeWindowAssociation( Handle, WindowAssociationFlags.IgnoreAll );
					factory.Dispose();

					ResizeBackBuffer();
					//m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
					//m_context.OutputMerger.SetTargets( m_renderView );

                    Initialize();
                    m_isStarted = true;
                }
                catch (Exception ex)
                {
                    StopSharpDx(false);
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
                }
            }
        }

        private void StopSharpDx(bool disposing)
        {
            Shutdown();

			//if (disposing && (m_hdc != IntPtr.Zero))
			if (disposing)
			{
				m_renderView.Dispose();
				//m_renderViewIntermediate.Dispose();
				//m_renderViewIntermediateSrv.Dispose();
				m_context.ClearState();
				m_context.Flush();
				m_device.Dispose();
				m_context.Dispose();
				m_swapChain.Dispose();
			}

            m_isStarted = false;
        }

		public SharpDX.Direct3D11.ImageInformation? readImageInformation( Uri resUri )
		{
			SharpDX.Direct3D11.ImageInformation? ii = SharpDX.Direct3D11.ImageInformation.FromFile(resUri.LocalPath);
			return ii;
		}

        public TextureProperties showResource( Uri resUri )
        {
            if (resUri == null)
                return null;

            if ( m_tex != null )
            {
                m_tex.Dispose();
                m_tex = null;
                m_texSRV.Dispose();
                m_texSRV = null;
            }

			TextureProperties tp = null;

			SharpDX.Direct3D11.ImageInformation? ii = SharpDX.Direct3D11.ImageInformation.FromFile( resUri.LocalPath );
            if (ii != null)
            {
                ImageLoadInformation ili = ImageLoadInformation.Default;
                //ImageLoadInformation ili = new ImageLoadInformation();
                ili.MipLevels = ii.Value.MipLevels;
                ili.PSrcInfo = IntPtr.Zero;
				//ili.Format = Format.R8G8B8A8_UNorm_SRgb;

				SharpDX.Direct3D11.Resource res = SharpDX.Direct3D11.Texture2D.FromFile( m_device, resUri.LocalPath, ili );
                //SharpDX.Direct3D11.Resource res = Texture2D.FromFile<Texture2D>(m_device, resUri.AbsolutePath, new ImageLoadInformation()
                //{
                //    Width = ImageLoadInformation.FileDefaultValue,
                //    Height = ImageLoadInformation.FileDefaultValue,
                //    Depth = ImageLoadInformation.FileDefaultValue,
                //    FirstMipLevel = ImageLoadInformation.FileDefaultValue,
                //    MipLevels = 1,
                //    Usage = ResourceUsage.Default,
                //    BindFlags = BindFlags.None,
                //    CpuAccessFlags = CpuAccessFlags.None,
                //    OptionFlags = ResourceOptionFlags.None,
                //    Format = Format.R8G8B8A8_UNorm,
                //    Filter = FilterFlags.None,
                //    MipFilter = FilterFlags.None,
                //    PSrcInfo = IntPtr.Zero
                //});

                if (res != null)
                {
                    m_tex = res;

					if ( (ii.Value.OptionFlags & ResourceOptionFlags.TextureCube) != 0 )
					{
						Texture2D tex = m_tex as Texture2D;
						Texture2DDescription texDesc = tex.Description;

						ShaderResourceViewDescription d = new ShaderResourceViewDescription();
						d.Dimension = ShaderResourceViewDimension.Texture2DArray;
						d.Format = texDesc.Format;
						d.Texture2DArray.ArraySize = texDesc.ArraySize;
						d.Texture2DArray.MipLevels = texDesc.MipLevels;
						d.Texture2DArray.MostDetailedMip = 0;
						m_texSRV = new ShaderResourceView( m_device, m_tex, d );
					}
					else
					{
						m_texSRV = new ShaderResourceView( m_device, res );

						//Texture2D tex = m_tex as Texture2D;
						//Texture2DDescription texDesc = tex.Description;

						//ShaderResourceViewDescription d = new ShaderResourceViewDescription();
						//d.Dimension = ShaderResourceViewDimension.Texture2DArray;
						//d.Format = texDesc.Format;
						//d.Texture2DArray.ArraySize = texDesc.ArraySize;
						//d.Texture2DArray.MipLevels = texDesc.MipLevels;
						//d.Texture2DArray.MostDetailedMip = 0;
						//m_texSRV = new ShaderResourceView( m_device, m_tex, d );
					}

					tp = new TextureProperties(resUri, m_tex, this);
					//m_textureSelectionContext.Selection = new[] { tp };
                }
                else
                {
					//m_textureSelectionContext.Clear();
                }
            }
            else
            {
				//m_textureSelectionContext.Clear();
            }

			fitInWindowRequest = true;
            Invalidate();

			m_texProperties = tp;
			return tp;
        }

        public void fitInWindow()
        {
            m_texPosition = new Vector4(0, 0, 0, 0);
            m_texScale = 1.0f;
			fitInWindowRequest = true;
            Invalidate();
        }
		public void fitSize()
		{
			m_texPosition = new Vector4(0, 0, 0, 0);
			m_texScale = 1.0f;
			fitSizeRequest = true;
			Invalidate();
		}

		public TextureProperties SelectedTexture
		{
			get { return m_texProperties; }
		}

		TextureSelectionContext m_textureSelectionContext;

        //private IntPtr m_hdc;
		private Device m_device;
		private SwapChain m_swapChain;
		private DeviceContext m_context;
		private RenderTargetView m_renderView;
		//private RenderTargetView m_renderViewIntermediate;
		//private ShaderResourceView m_renderViewIntermediateSrv;
		private bool m_isStarted;

        private InputLayout m_inputLayout;
        //private Buffer m_screenQuad;

		private string m_shadersFxText;
        private VertexShader m_vsPassThrough;
		//private PixelShader m_psColor;
		//private PixelShader m_psTex2DLoad;
		private Dictionary<string, PsShaderWrap> m_psDictionary = new Dictionary<string, PsShaderWrap>();
		private SamplerState m_ssPoint;

        private SharpDX.Direct3D11.Resource m_tex;
        private ShaderResourceView m_texSRV;
		private TextureProperties m_texProperties;

        private Vector4 m_texPosition = new Vector4(0, 0, 0, 0);
        private float m_texScale = 1.0f;
		private float m_texW = 2.0f;
		private float m_texH = 2.0f;

        private bool m_leftMouseButtonDown = false;
        private System.Drawing.Point m_mouseMoveStartLocation_;

		private bool fitSizeRequest = false;
		private bool fitInWindowRequest = false;

		private static readonly SharpDX.DXGI.Format backBufferFormat = Format.R8G8B8A8_UNorm_SRgb;
    }
}
