using System.ComponentModel;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Loads the game schema, and annotates
    /// the types with display information and PropertyDescriptors.</summary>
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader( Schema schema, IDynamicSchema dynamicSchema )
        {
			m_paramSchema = schema;
			m_dynamicSchemaHandler = dynamicSchema;
            m_dynamicSchemaHandler.Loader = this;
            m_dynamicSchemaHandler.Schema = schema;
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "SettingsEditor/Schemas");
			Load( "settingsFile.xsd" );
        }

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
				m_paramSchema.Initialize( typeCollection );

                // register extensions
				m_paramSchema.settingsFileType.Type.Define( new ExtensionInfo<DocumentEditingContext>() );
				m_paramSchema.settingsFileType.Type.Define( new ExtensionInfo<UniqueIdValidator>() );
				m_paramSchema.settingsFileType.Type.Define( new ExtensionInfo<Document>() );
                m_paramSchema.settingsFileType.Type.Define( new ExtensionInfo<TreeView>() );

                //m_paramSchema.groupType.Type.Define( new ExtensionInfo<Group>() );
                //m_paramSchema.presetType.Type.Define( new ExtensionInfo<Preset>() );
                m_paramSchema.structType.Type.Define( new ExtensionInfo<Struct>() );

				// Descriptors for armorType.
				string general = "General".Localize();
				var paramFileDescriptors = new PropertyDescriptorCollection( null );
				paramFileDescriptors.Add( new AttributePropertyDescriptor(
						   "SettingsDescFile".Localize(),
						   m_paramSchema.settingsFileType.settingsDescFileAttribute,
						   general,
						   "File describing structure of settings file".Localize(),
						   true
					) );

                paramFileDescriptors.Add(new AttributePropertyDescriptor(
                           "ShaderConstantsOutputPath".Localize(),
                           m_paramSchema.settingsFileType.shaderOutputFileAttribute,
                           general,
                           "Optional path for writing the shader constant buffer.".Localize(),
                           false
                    ) );

				if ( m_dynamicSchemaHandler != null )
					m_dynamicSchemaHandler.InitGui( m_paramSchema );

				m_paramSchema.settingsFileType.Type.SetTag( paramFileDescriptors );

				// only one namespace
                break;
            }            
        }

		private Schema m_paramSchema;
		private IDynamicSchema m_dynamicSchemaHandler;
    }
}
