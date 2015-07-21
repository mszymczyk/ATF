//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;
using Sce.Atf.Adaptation;
using System.Windows.Forms;
using System.Reflection;

namespace TextureEditor
{
    /// <summary>
    /// Component to edit resource meta-data.
    /// </summary>
	//[Export( typeof( ResourceMetadataEditor ) )]
	[Export( typeof( IInitializable ) )]    
    [PartCreationPolicy(CreationPolicy.Shared)]
	public class TextureMetadataEditor : IInitializable
	//public class ResourceMetadataEditor : EditingContext, IInitializable
    {
        public TextureMetadataEditor()
        {
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
			//if (m_resourceLister == null || m_resourceMetadataService == null)
			//	return;

			m_previewWindow = new TexturePreviewWindowSharpDX(m_contextRegistry);
			m_textureViewCommands = new TextureViewCommands( m_commandService, m_previewWindow, m_owner, m_mainForm, m_schemaLoader );

			ControlInfo cinfo = new ControlInfo("Texture Preview", "texture viewer", StandardControlGroup.CenterPermanent);
			m_controlHostService.RegisterControl(m_previewWindow, cinfo, null);

			m_resourceLister.SelectionChanged += resourceLister_SelectionChanged;

			m_propertyGrid = new PropertyGrid();
			m_controlInfo = new ControlInfo(
				"Source texture Information".Localize(),
				"Displays information about source texture".Localize(),
				StandardControlGroup.Hidden);
			m_controlHostService.RegisterControl(m_propertyGrid, m_controlInfo, null);

			m_helpTextBox = new RichTextBox();
			string aboutFilePath = "TextureEditor.Resources.About.rtf";
			Stream textFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream( aboutFilePath );
			if ( textFileStream != null )
				m_helpTextBox.LoadFile( textFileStream, RichTextBoxStreamType.RichText );

			ControlInfo helpTextBoxInfo = new ControlInfo( "Help", "help", StandardControlGroup.Bottom );
			m_controlHostService.RegisterControl( m_helpTextBox, helpTextBoxInfo, null );

			DomNode rootNode = new DomNode(Schema.textureMetadataEditorType.Type, Schema.textureMetadataEditorRootElement);
			rootNode.InitializeExtensions();

			m_editorRootNode = rootNode;

			if (m_domExplorer != null)
				m_domExplorer.Root = m_editorRootNode;

			//m_editingContext = new ResourceMetadataEditingContext();
			m_contextRegistry.ActiveContext = rootNode;
        }

        #endregion

		private void resourceLister_SelectionChanged( object sender, EventArgs e )
		{
			////Uri resUri = m_resourceLister.LastSelected;
			//object[] mdatadata = m_resourceMetadataService.GetMetadata(m_resourceLister.Selection).ToArray();
			////if (mdatadata.Length > 0)
			////{
			////	DomNode mdatadata0 = mdatadata[0] as DomNode;
			////	var edContext = mdatadata0.Cast<ResourceMetadataEditingContext>();
			////	//m_defaultContext.SelectionContext = edContext as ISelectionContext;
			////	//m_defaultContext.SelectionContext.Selection = mdatadata;
			////	//m_propertyGrid.Bind( edContext );
			////	//m_propertyGrid.Bind( m_defaultContext );
			////	//edContext.Selection = mdatadata;
			////	//m_contextRegistry.ActiveContext = m_defaultContext;
			////	m_contextRegistry.ActiveContext = mdatadata;
			////}
			////else
			////{
			////	//m_propertyGrid.Bind( mdatadata );
			////}
			//////m_contextRegistry.ActiveContext = mdatadata;
			////m_editingContext.Selection = mdatadata;
			////m_editingContext.setSelection(mdatadata);

			////Uri resUri = m_resourceLister.LastSelected;
			//if (mdatadata.Length > 0)
			//{
			//	//m_editorRootNode.GetChildList(Schema.textureMetadataEditorType.textureMetadataChild).Clear();

			//	DomNode mdatadata0 = mdatadata[mdatadata.Length-1] as DomNode;
			//	// this node must be added to root in order for history to work
			//	//
			//	//m_editorRootNode.GetChildList(Schema.textureMetadataEditorType.textureMetadataChild).Add(mdatadata0);

			//	//ResourceMetadataDocument doc = mdatadata0.Cast<ResourceMetadataDocument>();
			//	Uri resUri = mdatadata0.GetAttribute(Schema.resourceMetadataType.uriAttribute) as Uri;
			//	TextureProperties tp = m_previewWindow.showResource(resUri);
			//	//if ( tp != null )
			//	//{
			//	//	m_propertyGrid.Bind(new[] { tp });
			//	//}
			//	var edContext = mdatadata0.Cast<ResourceMetadataEditingContext>();
			//	edContext.SetRange(mdatadata);
			//	m_contextRegistry.ActiveContext = edContext;
			//}



			////var edContext = m_editorRootNode.Cast<ResourceMetadataEditingContext>();
			////edContext.SetRange(mdatadata);

			IEnumerable<Uri> resourceUris = m_resourceLister.Selection;
			List<DomNode> rootNodes = new List<DomNode>();
			foreach (Uri resourceUri in resourceUris)
			{
				string reExt = System.IO.Path.GetExtension(resourceUri.LocalPath).ToLower();

				string metadataFilePath = resourceUri.LocalPath + ".metadata";
				Uri metadataUri = new Uri(metadataFilePath);
				DomNode rootNode = null;

				if (m_loadedNodes.TryGetValue(metadataUri, out rootNode))
				{
				}
				else
				{
					if (File.Exists(metadataFilePath))
					{
						// read existing metadata
						using (FileStream stream = File.OpenRead(metadataFilePath))
						{
							var reader = new DomXmlReader(m_schemaLoader);
							rootNode = reader.Read(stream, metadataUri);
						}
					}
					else
					{
						rootNode = new DomNode(Schema.textureMetadataType.Type, Schema.textureMetadataRootElement);
						rootNode.SetAttribute(Schema.resourceMetadataType.uriAttribute, resourceUri);
					}

					m_loadedNodes.Add(metadataUri, rootNode);

					rootNode.InitializeExtensions();

					ResourceMetadataDocument document = rootNode.Cast<ResourceMetadataDocument>();
					document.Uri = metadataUri;

					// this node must be added to root in order for history to work
					//
					m_editorRootNode.GetChildList(Schema.textureMetadataEditorType.textureMetadataChild).Add(rootNode);
				}

				rootNodes.Add(rootNode);
			}

			if ( rootNodes.Count > 0 )
			{
				DomNode mdatadata0 = rootNodes.Last();

				Uri resUri = mdatadata0.GetAttribute(Schema.resourceMetadataType.uriAttribute) as Uri;
				TextureProperties tp = m_previewWindow.showResource(resUri);
				if (tp != null)
				{
					m_propertyGrid.Bind(new[] { tp });
				}
			}

			var edContext = m_editorRootNode.Cast<ResourceMetadataEditingContext>();
			edContext.SetRange(rootNodes);
		}

		[Import( AllowDefault = true )]
		private ResourceLister m_resourceLister = null;

		[Import( AllowDefault = true )]
		private IResourceMetadataService m_resourceMetadataService = null;

        [Import(AllowDefault = false)]
        private ControlHostService m_controlHostService = null;

		[Import(AllowDefault = false)]
		private IContextRegistry m_contextRegistry = null;

		[Import(AllowDefault = false)]
		private ICommandService m_commandService = null;

		[Import(AllowDefault = true)]
		private DomExplorer m_domExplorer = null;

		[Import(AllowDefault = false)]
		private SchemaLoader m_schemaLoader = null;

		[Import( AllowDefault = false )]
		private IWin32Window m_owner;

		[Import( AllowDefault = false )]
		private MainForm m_mainForm;

		private ControlInfo m_controlInfo;
		private PropertyGrid m_propertyGrid;
		private RichTextBox m_helpTextBox;
		//private SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
		//private ResourceMetadataEditingContext m_editingContext;

		DomNode m_editorRootNode;

		private Dictionary<Uri, DomNode> m_loadedNodes = new Dictionary<Uri, DomNode>();

		private TexturePreviewWindowSharpDX m_previewWindow;
		private TextureViewCommands m_textureViewCommands;
	}
}
