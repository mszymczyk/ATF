//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;
using Sce.Atf.Adaptation;

namespace TextureEditor
{
    /// <summary>
    /// Component to edit resource meta-data.
    /// </summary>
	//[Export( typeof( ResourceMetadataEditor ) )]
	[Export( typeof( IInitializable ) )]    
    [PartCreationPolicy(CreationPolicy.Shared)]
	public class ResourceMetadataEditor : IInitializable
	//public class ResourceMetadataEditor : EditingContext, IInitializable
    {
        public ResourceMetadataEditor()
        {
            m_propertyGrid = new PropertyGrid();
            m_controlInfo = new ControlInfo(
                "Resource Metadata".Localize(),
                "Edits selected resource metadata".Localize(),
                StandardControlGroup.Hidden);
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
			if (m_resourceLister == null || m_resourceMetadataService == null)
				return;

			m_resourceLister.SelectionChanged += resourceLister_SelectionChanged;
            m_controlHostService.RegisterControl(m_propertyGrid, m_controlInfo, null);

        }

        #endregion

		private void resourceLister_SelectionChanged( object sender, EventArgs e )
		{
			Uri resUri = m_resourceLister.LastSelected;
			object[] mdatadata = m_resourceMetadataService.GetMetadata( m_resourceLister.Selection ).ToArray();
			if (mdatadata.Length > 0)
			{
				DomNode mdatadata0 = mdatadata[0] as DomNode;
				var edContext = mdatadata0.Cast<ResourceMetadataEditingContext>();
				m_defaultContext.SelectionContext = edContext as ISelectionContext;
				m_defaultContext.SelectionContext.Selection = mdatadata;
				//m_propertyGrid.Bind( edContext );
				//m_propertyGrid.Bind( m_defaultContext );
				//edContext.Selection = mdatadata;
				//m_contextRegistry.ActiveContext = m_defaultContext;
				m_contextRegistry.ActiveContext = mdatadata;
			}
			else
			{
				//m_propertyGrid.Bind( mdatadata );
			}
			//m_contextRegistry.ActiveContext = mdatadata;
		}

		[Import( AllowDefault = true )]
		private ResourceLister m_resourceLister = null;

		[Import( AllowDefault = true )]
		private IResourceMetadataService m_resourceMetadataService = null;

        [Import(AllowDefault = false)]
        private ControlHostService m_controlHostService = null;

		[Import( AllowDefault = false )]
		private IContextRegistry m_contextRegistry;

        private readonly ControlInfo m_controlInfo;
        private readonly PropertyGrid m_propertyGrid;
		private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
	}
}
