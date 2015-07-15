//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Rendering.Dom;

namespace TextureEditor
{

    /// <summary>
    /// MEF component that registers a Windows Control with IControlHostService that is used to
    /// display a 3D scene using SharpDx. Consider also using
    /// RenderCommands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(RenderViewSharpDx))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenderViewSharpDx : IInitializable
    {
        /// <summary>
        /// Construct render view</summary>
		public RenderViewSharpDx()
        {
        }

		///// <summary>
		///// Gets DesignControl</summary>
		//public DesignControl ViewControl
		//{
		//	get { return m_previewWindow; }
		//}

		///// <summary>
		///// Fits current 3D scene</summary>
		//public void Fit()
		//{
		//	// calculate bounding sphere
		//	if (m_scene.Children.Count > 0)
		//	{
		//		// calculate bounding sphere
		//		Sphere3F sphere = CalcBoundSphere(m_scene.Children[0]);
		//		sphere.Radius *= 2.0f;

		//		float aspect = (float)m_previewWindow.Width / (float)m_previewWindow.Height;

		//		m_previewWindow.Camera.Frustum.SetPerspective(
		//			(float)Math.PI / 4,
		//			aspect,
		//			sphere.Radius * 0.01f,
		//			sphere.Radius * 5.0f);

		//		m_previewWindow.Camera.ZoomOnSphere(sphere);   
		//	}

		//}
        #region IInitializable Members

        /// <summary>
        /// Registers rendering control and subscribes to ActiveDocumentChanged event</summary>
        void IInitializable.Initialize()
        {
			m_designControl = new TexturePreviewWindowSharpDX( m_contextRegistry );
            m_textureViewCommands = new TextureViewCommands(m_commandService, m_designControl);

            ControlInfo cinfo = new ControlInfo("Texture Preview", "texture viewer", StandardControlGroup.Center);
            m_controlHostService.RegisterControl(m_designControl, cinfo, null);

			//m_documentRegistry.ActiveDocumentChanged += (sender, e) =>
			//{
			//	ClearRenderGraph(m_context);
			//	m_context = null;

			//	ModelDocument doc = m_documentRegistry.GetActiveDocument<ModelDocument>();
			//	if (doc != null)
			//	{
			//		m_context = doc.RootNode;
			//		SceneGraphBuilder builder = new SceneGraphBuilder(typeof(IRenderThumbnail));
			//		builder.Build(doc.RootNode, m_scene);
			//		Fit();                                                    
			//	}
			//};

            m_resourceLister.SelectionChanged += resourceLister_SelectionChanged;
        }

        #endregion

        private void resourceLister_SelectionChanged(object sender, EventArgs e)
        {
            Uri resUri = m_resourceLister.LastSelected;
            m_designControl.showResource(resUri);
        }

        //private object m_context;

		//[Import(AllowDefault = false)]
		//private IDocumentRegistry m_documentRegistry;

		[Import(AllowDefault = false)]
		private IControlHostService m_controlHostService;

        [Import(AllowDefault = true)]
        private ResourceLister m_resourceLister = null;

		[Import( AllowDefault = false )]
		private IContextRegistry m_contextRegistry;

        [Import(AllowDefault = false)]
        private ICommandService m_commandService;

        private TexturePreviewWindowSharpDX m_designControl;
        private TextureViewCommands m_textureViewCommands;
    }
}
