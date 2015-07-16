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
						new Sce.Atf.Dom.PropertyDescriptor[] {
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
									} ) );

				PropertyDescriptorCollection textureMetadataTypeProperyCollection = new PropertyDescriptorCollection( null );

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

				textureMetadataTypeProperyCollection.Add(
					new AttributePropertyDescriptor(
							 "Force Source Srgb".Localize(),
							 Schema.textureMetadataType.forceSourceSrgbAttribute,
							 group_Metadata,
							 "Treats source image as srgb".Localize(),
							 false,
							 new BoolEditor()
							 )
				);

				textureMetadataTypeProperyCollection.Add(
					new AttributePropertyDescriptor(
							 "Width".Localize(),
							 Schema.textureMetadataType.widthAttribute,
							 group_Metadata,
							 "Sets exported image's width".Localize(),
							 false,
							//new BoundedIntEditor( -1, 16 * 1024 * 1024 )
							 new NumericEditor( typeof( int ) )
							 )
				);

				textureMetadataTypeProperyCollection.Add(
					new AttributePropertyDescriptor(
							 "Height".Localize(),
							 Schema.textureMetadataType.heightAttribute,
							 group_Metadata,
							 "Sets exported image's height".Localize(),
							 false,
							 //new BoundedIntEditor( -1, 16 * 1024 * 1024 )
							 new NumericEditor( typeof(int) )
							 )
				);

				//textureMetadataTypeProperyCollection.Add(
				//	new AttributePropertyDescriptor(
				//			 "Depth".Localize(),
				//			 Schema.textureMetadataType.depthAttribute,
				//			 group_Metadata,
				//			 "Sets exported image's depth".Localize(),
				//			 false,
				//			//new BoundedIntEditor( -1, 16 * 1024 * 1024 )
				//			 new NumericEditor( typeof( int ) )
				//			 )
				//);
				//// create images used for showing emotion
				//// for enum OrcEmotion 
				//var emoVals = Enum.GetValues( typeof( IntendedUsage ) );

				//// Shows how to edit enum that is stored as string.
				//var emotionEditor = new LongEnumEditor( typeof( IntendedUsage ), null );
				//textureMetadataTypeProperyCollection.Add(
				//  new AttributePropertyDescriptor(
				//		 "IntendedUsage".Localize(),
				//		 Schema.textureMetadataType.intendedUsageAttribute,
				//		 group_Metadata,
				//		 "Specifies how this texture will be used".Localize(),
				//		 false,
				//		 emotionEditor
				//		 ) );

				{
					List<string> popularFormats = new List<string>();

					// compressed
					// https://msdn.microsoft.com/pl-pl/library/hh308955.aspx
					// https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531(v=vs.85).aspx
					//
					popularFormats.Add( SharpDX.DXGI.Format.Unknown.ToString() );

					popularFormats.Add( SharpDX.DXGI.Format.BC1_UNorm_SRgb.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC2_UNorm_SRgb.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC3_UNorm_SRgb.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC4_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC5_SNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC7_UNorm_SRgb.ToString() );

					popularFormats.Add( SharpDX.DXGI.Format.R8G8B8A8_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.R8_UNorm.ToString() );

					popularFormats.Add( SharpDX.DXGI.Format.BC1_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC2_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC3_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC4_SNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC5_UNorm.ToString() );
					popularFormats.Add( SharpDX.DXGI.Format.BC7_UNorm.ToString() );

					//var formatEditor = new LongEnumEditor( typeof(SharpDX.DXGI.Format), null );
					var formatEditor = new LongEnumEditor();
					formatEditor.DefineEnum( popularFormats.ToArray(), null );
					formatEditor.MaxDropDownItems = 10;
					var apd =  new AttributePropertyDescriptor(
							 "Format".Localize(),
							 Schema.textureMetadataType.formatAttribute,
							 group_Metadata,
							 "Specifies format of exported texture".Localize(),
							 false,
							 formatEditor
							 );
					textureMetadataTypeProperyCollection.Add( apd );
				}

				{
					var formatNames = Enum.GetValues( typeof( SharpDX.DXGI.Format ) );
					var formatEditor = new LongEnumEditor( typeof(SharpDX.DXGI.Format), null );
					formatEditor.MaxDropDownItems = 10;
					var apd =  new AttributePropertyDescriptor(
							 "ExtendedFormat".Localize(),
							 Schema.textureMetadataType.extendedFormatAttribute,
							 group_Metadata,
							 "Specifies format of exported texture (advanced)".Localize(),
							 false,
							 formatEditor
							 );
					textureMetadataTypeProperyCollection.Add( apd );
				}

				//// Shows how to edit enum that is stored as string.
				//var emotionEditor = new LongEnumEditor( typeof( IntendedUsage ), null );
				//textureMetadataTypeProperyCollection.Add(
				//  new AttributePropertyDescriptor(
				//		 "IntendedUsage".Localize(),
				//		 Schema.textureMetadataType.intendedUsageAttribute,
				//		 group_Metadata,
				//		 "Specifies how this texture will be used".Localize(),
				//		 false,
				//		 emotionEditor
				//		 ) );
				Schema.textureMetadataType.Type.SetTag( textureMetadataTypeProperyCollection );

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

		///// <summary>
		///// Enum used for orc character level.</summary>
		//private enum IntendedUsage
		//{
		//	Unspecified,
		//	Color,
		//	ColorHiQuality,
		//	NormalMap,
		//	Ambient,
		//	OrigFormat,
		//	ManualFormat
		//};
	}
}
