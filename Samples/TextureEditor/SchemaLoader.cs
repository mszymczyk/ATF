//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;


namespace TextureEditor
{
    /// <summary>
    /// Loads the game schema, and annotates
    /// the types with display information and PropertyDescriptors.</summary>
    [Export(typeof(SchemaLoader))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SchemaLoader : XmlSchemaTypeLoader, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public SchemaLoader(PropertyEditor propertyEditor)
        {
            m_propertyEditor = propertyEditor;
            // set resolver to locate embedded .xsd file
			SchemaResolver = new ResourceStreamResolver( Assembly.GetExecutingAssembly(), "TextureEditor/Schemas" );
            Load("textureEditor.xsd");
        }


        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Set custom display options for the 2-column PropertyEditor
            PropertyGridView propertyGridView = m_propertyEditor.PropertyGrid.PropertyGridView;
            if (propertyGridView.CustomizeAttributes != null)
                throw new InvalidOperationException("Someone else set PropertyGridView's CustomizeAttributes already");
            propertyGridView.CustomizeAttributes = new[]
                {
                    new PropertyView.CustomizeAttribute("Orcs".Localize(), horizontalEditorOffset:0, nameHasWholeRow:true),
                    new PropertyView.CustomizeAttribute("Armor".Localize(), horizontalEditorOffset:64),
                    new PropertyView.CustomizeAttribute("Club".Localize(), horizontalEditorOffset:64)
                };
        }

        #endregion

        /// <summary>
        /// Gets the schema namespace</summary>
        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        /// <summary>
        /// Gets the schema type collection</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.
        /// Create property descriptors for types.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
			foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
			{
				m_namespace = typeCollection.TargetNamespace;
				m_typeCollection = typeCollection;
				Schema.Initialize( typeCollection );
				TextureEditorAdapters.Initialize( this );

				string group_Metadata = "Metadata".Localize();


				Schema.resourceMetadataType.Type.SetTag(
					new PropertyDescriptorCollection(
						new PropertyDescriptor[] {
											new AttributePropertyDescriptor(
												"URI".Localize(),
												Schema.resourceMetadataType.uriAttribute,
												group_Metadata,
												"Uri".Localize(),
												true),
											new AttributePropertyDescriptor(
												"Keywords".Localize(),
												Schema.resourceMetadataType.keywordsAttribute,
												group_Metadata,
												"Keywords".Localize(),
												false),
									}));

				PropertyDescriptorCollection textureMetadataTypeProperyCollection = new PropertyDescriptorCollection(null);

				textureMetadataTypeProperyCollection.Add(
					new AttributePropertyDescriptor(
							 "Generate mipmaps".Localize(),
							 Schema.textureMetadataType.genMipMapsAttribute,
							 group_Metadata,
							 "Controlls mipmap generation".Localize(),
							 false,
							 new BoolEditor()
							 )
				);

				Schema.textureMetadataType.Type.SetTag(textureMetadataTypeProperyCollection);

				break;
			}
        }

		protected override void ParseAnnotations(
			XmlSchemaSet schemaSet,
			IDictionary<NamedMetadata, IList<XmlNode>> annotations )
		{
			base.ParseAnnotations( schemaSet, annotations );

			//foreach (var kv in annotations)
			//{
			//	DomNodeType nodeType = kv.Key as DomNodeType;
			//	if (kv.Value.Count == 0) continue;

			//	PropertyDescriptorCollection localDescriptor = nodeType.GetTagLocal<PropertyDescriptorCollection>();
			//	PropertyDescriptorCollection annotationDescriptor = Sce.Atf.Dom.PropertyDescriptor.ParseXml( nodeType, kv.Value );

			//	// if the type already have local property descriptors 
			//	// then add annotation driven property descriptors to it.
			//	if (localDescriptor != null)
			//	{
			//		foreach (System.ComponentModel.PropertyDescriptor propDecr in annotationDescriptor)
			//		{
			//			localDescriptor.Add( propDecr );
			//		}
			//	}
			//	else
			//	{
			//		localDescriptor = annotationDescriptor;
			//	}

			//	if (localDescriptor.Count > 0)
			//		nodeType.SetTag<PropertyDescriptorCollection>( localDescriptor );
			//}
		}

        private readonly PropertyEditor m_propertyEditor;        
    }
}
