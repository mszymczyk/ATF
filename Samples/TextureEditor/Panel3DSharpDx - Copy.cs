//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

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
			m_context.ClearRenderTargetView( m_renderView, Color.Red );
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
			// Create Device and SwapChain
			m_swapChain.ResizeBuffers( 1, ClientSize.Width, ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None );

			// New RenderTargetView from the backbuffer
			var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
			m_renderView = new RenderTargetView( m_device, backBuffer );
			//backBuffer.Dispose();

			m_context.Flush();
			m_context.ClearState();

			m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
			m_context.OutputMerger.SetTargets( m_renderView );  
		}

        /// <summary>
        /// Client entry to initialize custom OpenGL resources</summary>
        protected virtual void Initialize() 
        { 
        }

        /// <summary>
        /// Client entry to unload custom OpenGL resources</summary>
        protected virtual void Shutdown() 
        { 
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
					//factory.Dispose();

					// New RenderTargetView from the backbuffer
					var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
					m_renderView = new RenderTargetView( m_device, backBuffer );
					//backBuffer.Dispose();

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

		private IntPtr m_hdc;
		private Device m_device;
		private SwapChain m_swapChain;
		private DeviceContext m_context;
		private RenderTargetView m_renderView;
        private bool m_isStarted;
    }
}
