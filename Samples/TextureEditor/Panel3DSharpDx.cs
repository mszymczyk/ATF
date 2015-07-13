//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

using Sce.Atf;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace TextureEditor
{
    /// <summary>
    /// A control for using OpenGL to do the painting</summary>
    /// <remarks>This class's constructor initializes OpenGL so that other tools that use OpenGL, such as
    /// the Texture Manager, work even if BeginPaint has not been called. This allows
    /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
	public class Panel3DSharpDx : InteropControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>This constructor initializes OpenGL so that other tools that use OpenGL, such as
        /// the Texture Manager, work even if BeginPaint has not been called. This allows
        /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
		public Panel3DSharpDx()
        {
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

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
		protected override void OnPaint( PaintEventArgs e )
		{
			m_context.ClearRenderTargetView( m_renderView, Color.Black );

            // Instantiate Vertex buffer from vertex data
            var vertices = Buffer.Create(m_device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-0.5f, 0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
                                  });

            m_context.InputAssembler.InputLayout = m_inputLayout;
            m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            m_context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
            vertices.Dispose();
            m_context.VertexShader.Set(m_vsPassThrough);
            m_context.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f));
            m_context.PixelShader.Set(m_psColor);
            m_context.OutputMerger.SetTargets(m_renderView);

            RasterizerStateDescription rsd = RasterizerStateDescription.Default();
            rsd.IsFrontCounterClockwise = true;
            RasterizerState rs = new RasterizerState(m_device, rsd);
            m_context.Rasterizer.State = rs;
            rs.Dispose();

            m_context.PixelShader.SetSampler(0, m_ssPoint);

            if ( m_texSRV != null )
            {
                m_context.PixelShader.SetShaderResource(0, m_texSRV);
            }
            else
            {
                m_context.PixelShader.SetShaderResource(0, null);
            }

            m_context.Draw(4, 0);

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

			// Create Device and SwapChain
			m_swapChain.ResizeBuffers( 1, ClientSize.Width, ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None );

			// New RenderTargetView from the backbuffer
			var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
			m_renderView = new RenderTargetView( m_device, backBuffer );
			backBuffer.Dispose();

			m_context.Flush();
			m_context.ClearState();

			m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
			m_context.OutputMerger.SetTargets( m_renderView );

            Invalidate();

            System.Diagnostics.Debug.WriteLine("ClientSize: {0}", ClientSize);
		}

        /// <summary>
        /// Client entry to initialize custom OpenGL resources</summary>
        protected virtual void Initialize() 
        {
            // Compile Vertex and Pixel shaders

            Assembly assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();

            var resourceName = "TextureEditor.Resources.shaders.fx";
            string shadersFxText;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                shadersFxText = reader.ReadToEnd();
            }

            var vertexShaderByteCode = ShaderBytecode.Compile(shadersFxText, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_vsPassThrough = new VertexShader(m_device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.Compile(shadersFxText, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            m_psColor = new PixelShader(m_device, pixelShaderByteCode);

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
        }

        /// <summary>
        /// Client entry to unload custom OpenGL resources</summary>
        protected virtual void Shutdown() 
        {
            if (m_psColor != null)
            {
                m_psColor.Dispose();
                m_psColor = null;
            }
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
												new Rational( 60, 1 ), Format.R8G8B8A8_UNorm ),
						IsWindowed = true,
						OutputHandle = Handle,
						SampleDescription = new SampleDescription( 1, 0 ),
						SwapEffect = SwapEffect.Discard,
						Usage = Usage.RenderTargetOutput
					};

					// Create Device and SwapChain
					Device.CreateWithSwapChain( DriverType.Hardware, DeviceCreationFlags.None, desc, out m_device, out m_swapChain );
					m_context = m_device.ImmediateContext;

					// Ignore all windows events
					var factory = m_swapChain.GetParent<Factory>();
					factory.MakeWindowAssociation( Handle, WindowAssociationFlags.IgnoreAll );
					factory.Dispose();

					// New RenderTargetView from the backbuffer
					var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
					m_renderView = new RenderTargetView( m_device, backBuffer );
					backBuffer.Dispose();

					m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
					m_context.OutputMerger.SetTargets( m_renderView );


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
				m_context.ClearState();
				m_context.Flush();
				m_device.Dispose();
				m_context.Dispose();
				m_swapChain.Dispose();
			}

            m_isStarted = false;
        }

        public void showResource( Uri resUri )
        {
            if ( m_tex != null )
            {
                m_tex.Dispose();
                m_tex = null;
                m_texSRV.Dispose();
                m_texSRV = null;
            }

            SharpDX.Direct3D11.Resource res = SharpDX.Direct3D11.Resource.FromFile(m_device, resUri.AbsolutePath);
            if ( res != null )
            {
                m_tex = res;
                m_texSRV = new ShaderResourceView(m_device, res);
            }

            Invalidate();
        }

        //private IntPtr m_hdc;
		private Device m_device;
		private SwapChain m_swapChain;
		private DeviceContext m_context;
		private RenderTargetView m_renderView;
        private bool m_isStarted;

        private InputLayout m_inputLayout;
        //private Buffer m_screenQuad;

        private VertexShader m_vsPassThrough;
        private PixelShader m_psColor;
        private SamplerState m_ssPoint;

        private SharpDX.Direct3D11.Resource m_tex;
        private ShaderResourceView m_texSRV;
    }
}
