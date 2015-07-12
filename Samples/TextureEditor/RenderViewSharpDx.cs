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
            m_designControl = new Panel3DSharpDx();
        }

		///// <summary>
		///// Gets DesignControl</summary>
		//public DesignControl ViewControl
		//{
		//	get { return m_designControl; }
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

		//		float aspect = (float)m_designControl.Width / (float)m_designControl.Height;

		//		m_designControl.Camera.Frustum.SetPerspective(
		//			(float)Math.PI / 4,
		//			aspect,
		//			sphere.Radius * 0.01f,
		//			sphere.Radius * 5.0f);

		//		m_designControl.Camera.ZoomOnSphere(sphere);   
		//	}

		//}
        #region IInitializable Members

        /// <summary>
        /// Registers rendering control and subscribes to ActiveDocumentChanged event</summary>
        void IInitializable.Initialize()
        {
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

        }

        #endregion
        
        private object m_context;

		//[Import(AllowDefault = false)]
		//private IDocumentRegistry m_documentRegistry;

		[Import(AllowDefault = false)]
		private IControlHostService m_controlHostService;

        private Panel3DSharpDx m_designControl;
    }
}
