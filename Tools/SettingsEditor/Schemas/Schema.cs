// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "params.xsd" "Schema.cs" "SettingsEditor" "SettingsEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

using SettingsEditor;

namespace SettingsEditor
{

    public class Schema
    {
        public const string NS = "SettingsEditor";

		public Schema( IDynamicSchema dynamicSchema )
		{
			m_dynamicSchemaHandler_ = dynamicSchema;
		}

        public void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
			settingsFileType.Type = getNodeType( "SettingsEditor", "settingsFileType" );

			// we must add dynamic attributes here, becase as soon as we start calling GetAttributeInfo
			// attributes become frozen
			//
			if ( m_dynamicSchemaHandler_ != null )
				m_dynamicSchemaHandler_.AddDynamicAttributes( settingsFileType.Type );

			// after this call to GetAttributeInfo attributes become frozen
			// 
			settingsFileType.settingsDescFileAttribute = settingsFileType.Type.GetAttributeInfo( "settingsDescFile" );

			settingsFileRootElement = getRootElement( NS, "settingsFile" );
        }

		//public void AddDynamicAttributes( DomNodeType dnt )
		//{
		//	AttributeInfo ai = new AttributeInfo( "attrBoolean", AttributeType.BooleanType );
		//	dnt.Define( ai );
		//}

		public class settingsFileTypeClass
        {
            public DomNodeType Type;
			public AttributeInfo settingsDescFileAttribute;
        }

		public settingsFileTypeClass settingsFileType = new settingsFileTypeClass();

        public ChildInfo settingsFileRootElement;

		private IDynamicSchema m_dynamicSchemaHandler_;
    }
}
