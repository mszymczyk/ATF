// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "settingsFile.xsd" "Schema.cs" "SettingsEditor" "SettingsEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace SettingsEditor
{
    public class Schema
    {
        public const string NS = "SettingsEditor";

        public Schema(IDynamicSchema dynamicSchema)
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
            settingsFileType.Type = getNodeType("SettingsEditor", "settingsFileType");
            settingsFileType.structChild = settingsFileType.Type.GetChildInfo( "group" );

            //groupType.Type = getNodeType("SettingsEditor", "groupType");
            //groupType.selectedPresetRefAttribute = groupType.Type.GetAttributeInfo( "selectedPresetRef" );
            //groupType.expandedAttribute = groupType.Type.GetAttributeInfo( "expanded" );
            //groupType.presetsChild = groupType.Type.GetChildInfo( "preset" );
            //groupType.Type.SetTag<Schema>(this);

            //presetType.Type = getNodeType("SettingsEditor", "presetType");
            //presetType.Type.SetTag<Schema>(this);
            //presetType.nameAttribute = presetType.Type.GetAttributeInfo( "name" );

            structType.Type = getNodeType( "SettingsEditor", "structType" );
            structType.expandedAttribute = structType.Type.GetAttributeInfo( "expanded" );
            structType.structChild = structType.Type.GetChildInfo( "group" );
            structType.Type.SetTag<Schema>( this );

            // we must add dynamic attributes here, because as soon as we start calling GetAttributeInfo
            // attributes become frozen
            //
            if ( m_dynamicSchemaHandler_ != null)
                m_dynamicSchemaHandler_.AddDynamicAttributes(this);

            // after this call to GetAttributeInfo attributes become frozen
            // 
            settingsFileType.settingsDescFileAttribute = settingsFileType.Type.GetAttributeInfo("settingsDescFile");
            settingsFileType.shaderOutputFileAttribute = settingsFileType.Type.GetAttributeInfo("shaderOutputFile"); 

            settingsFileRootElement = getRootElement(NS, "settingsFile");
        }

        public class settingsFileTypeClass
        {
            public DomNodeType Type;
            public AttributeInfo settingsDescFileAttribute;
            public AttributeInfo shaderOutputFileAttribute;
            public ChildInfo structChild;
        }

        //public class groupTypeClass
        //{
        //    public DomNodeType Type;
        //    public AttributeInfo selectedPresetRefAttribute;
        //    public AttributeInfo expandedAttribute;
        //    public ChildInfo presetsChild;
        //}

        //public class presetTypeClass
        //{
        //    public DomNodeType Type;
        //    public AttributeInfo nameAttribute;
        //}

        public class structTypeClass
        {
            public DomNodeType Type;
            public AttributeInfo expandedAttribute;
            public ChildInfo structChild;
        }

        public settingsFileTypeClass settingsFileType = new settingsFileTypeClass();
        //public groupTypeClass groupType = new groupTypeClass();
        //public presetTypeClass presetType = new presetTypeClass();
        public structTypeClass structType = new structTypeClass();
        public ChildInfo settingsFileRootElement;

        private IDynamicSchema m_dynamicSchemaHandler_;

        public static readonly AttributeType Float4Type = new AttributeType( "Float4", typeof( float[] ), 4 );
        // this type is to support FlexibleFloatInputControl
        // 5 floats are: value, soft min, soft max, step size, checkbox state - Enabled!=0 Disabled==0
        //
        public static readonly AttributeType FlexibleFloatType = new AttributeType( "FlexibleFloatType", typeof( float[] ), 5 );
    }
}
