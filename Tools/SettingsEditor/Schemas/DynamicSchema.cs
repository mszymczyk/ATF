using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;
using misz.Gui;
using Sce.Atf;
using pico.Controls.PropertyEditing;

namespace SettingsEditor
{
	public class DynamicSchema : IDynamicSchema
	{
		public DynamicSchema( SettingsCompiler compiler )
		{
			m_compiler = compiler;
		}

        public void AddDynamicAttributesRecurse( SettingGroup structure, Schema schema )
        {
            DomNodeType dnt = new DomNodeType(
                structure.DomNodeTypeFullName,
                schema.structType.Type,
                EmptyArray<AttributeInfo>.Instance,
                EmptyArray<ChildInfo>.Instance,
                EmptyArray<ExtensionInfo>.Instance );
            dnt.SetTag<SettingGroup>( structure );

            PropertyDescriptorCollection propertyDescriptorCollection = dnt.GetTag<PropertyDescriptorCollection>();
            if (propertyDescriptorCollection == null)
            {
                propertyDescriptorCollection = new PropertyDescriptorCollection( null );
                dnt.SetTag<PropertyDescriptorCollection>( propertyDescriptorCollection );
            }

            Loader.AddNodeType( dnt.Name, dnt );

            foreach (Setting sett in structure.Settings)
            {
                if (sett is BoolSetting)
                {
                    BoolSetting bsett = (BoolSetting) sett;

                    AttributeInfo ai = new AttributeInfo( sett.Name, AttributeType.BooleanType );
                    ai.DefaultValue = bsett.Value;
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is IntSetting)
                {
                    IntSetting isett = (IntSetting) sett;

                    AttributeInfo ai = new AttributeInfo( sett.Name, AttributeType.IntType );
                    ai.DefaultValue = isett.Value;
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is FloatSetting)
                {
                    FloatSetting fsett = (FloatSetting) sett;

                    AttributeInfo ai = new AttributeInfo( sett.Name, Schema.FlexibleFloatType );
                    ai.DefaultValue = new float[5] {
                                    fsett.Value,
                                    fsett.SoftMinValue,
                                    fsett.SoftMaxValue,
                                    fsett.StepSize,
                                    fsett.ValueBool ? 1 : 0
                                };
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is EnumSetting)
                {
                    EnumSetting esett = (EnumSetting) sett;

                    AttributeInfo ai = new AttributeInfo( sett.Name, AttributeType.IntType );
                    //ai.DefaultValue = esett.EnumType.GetEnumName( esett.Value );
                    ai.DefaultValue = (int) esett.Value;
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is StringSetting)
                {
                    StringSetting ssett = (StringSetting) sett;

                    AttributeInfo ai = new AttributeInfo( sett.Name, AttributeType.StringType );
                    ai.DefaultValue = ssett.Value ?? "";
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is Float4Setting)
                {
                    Float4Setting fsett = (Float4Setting) sett;
                    AttributeInfo ai = new AttributeInfo( sett.Name, Schema.Float4Type );
                    ai.DefaultValue = new float[4] { fsett.Value.X, fsett.Value.Y, fsett.Value.Z, fsett.Value.W };
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else if (sett is ColorSetting)
                {
                    ColorSetting csett = (ColorSetting) sett;
                    AttributeInfo ai = new AttributeInfo( sett.Name, AttributeType.IntType );
                    ai.DefaultValue = csett.asInt();
                    dnt.Define( ai );
                    ai.SetTag<Setting>( sett );
                }
                else
                {
                    throw new Exception( "Unsupported setting type!" );
                }
            }

            foreach ( SettingGroup nestedStructure in structure.NestedStructures )
            {
                AddDynamicAttributesRecurse( nestedStructure, schema );
            }
        }

		public void AddDynamicAttributes( Schema schema )
		{
            foreach ( SettingGroup structure in m_compiler.RootStructure.NestedStructures )
            {
                AddDynamicAttributesRecurse( structure, schema );
            }
		}

        static List<Pair<DomNodeType, AttributeInfo>> GetDependsOnListValidated( List<string> dependsOn, DomNodeType dnt, SchemaLoader loader )
        {
            List<Pair<DomNodeType, AttributeInfo>> validatedList = new List<Pair<DomNodeType, AttributeInfo>>();

            foreach( string s in dependsOn )
            {
                bool err = true;

                if ( s.Contains( ".") )
                {
                    int lastDotIndex = s.LastIndexOf( '.' );
                    if ( lastDotIndex != -1 )
                    {
                        string fullTypeName = "SettingsEditor:" + s.Substring( 0, lastDotIndex );

                        DomNodeType otherDnt = loader.GetNodeType( fullTypeName );
                        if ( otherDnt != null )
                        {
                            string attributeName = s.Substring( lastDotIndex + 1 );
                            AttributeInfo ai = otherDnt.GetAttributeInfo( attributeName );
                            if ( ai != null )
                            {
                                if ( ai.Type == AttributeType.BooleanType )
                                {
                                    err = false;
                                    validatedList.Add( new Pair<DomNodeType, AttributeInfo>( otherDnt, ai ) );
                                }
                            }
                        }
                    }
                }
                else
                {
                    AttributeInfo ai = dnt.GetAttributeInfo( s );
                    if ( ai != null )
                    {
                        if ( ai.Type == AttributeType.BooleanType )
                        {
                            err = false;
                            validatedList.Add( new Pair<DomNodeType, AttributeInfo>( dnt, ai ) );
                        }
                    }
                }

                if ( err )
                {
                    Outputs.WriteLine( OutputMessageType.Error, s + " is not boolean attribute. DependsOn must refer to boolean attribute" );
                }
            }

            return validatedList;
        }

        static private AttributePropertyDescriptor CreateAttributePropertyDescriptor(
            Setting sett, AttributeInfo ai, object editor, TypeConverter typeConverter, List<Pair<DomNodeType, AttributeInfo>> dependsOn
            )
        {
            AttributePropertyDescriptor apd;
            if ( dependsOn.Count > 0 )
            {
                DependsOnNodes don = new DependsOnNodes( dependsOn );

                apd = new CustomEnableAttributePropertyDescriptor(
                        sett.DisplayName,
                        ai,
                        sett.Category,
                        sett.HelpText,
                        false,
                        editor,
                        typeConverter,
                        don
                    );
            }
            else
            {
                apd = new AttributePropertyDescriptor(
                    sett.DisplayName,
                    ai,
                    sett.Category,
                    sett.HelpText,
                    false,
                    editor,
                    typeConverter
                );
            }

            return apd;
        }

        public void InitGuiRecurse( SettingGroup structure, Schema schema )
        {
            DomNodeType dnt = Loader.GetNodeType( structure.DomNodeTypeFullName );

            foreach (Setting sett in structure.Settings)
            {
                PropertyDescriptorCollection propertyDescriptorCollection = dnt.GetTag<PropertyDescriptorCollection>();

                List<Pair<DomNodeType, AttributeInfo>> dependsOn = GetDependsOnListValidated( sett.DependsOn, dnt, Loader );
                if ( structure.DependsOn != null )
                {
                    List<Pair<DomNodeType, AttributeInfo>> structureDependsOn = GetDependsOnListValidated( structure.DependsOn, dnt, Loader );
                    dependsOn.AddRange( structureDependsOn );
                }

                dependsOn = dependsOn.Distinct().ToList();

                if ( sett is BoolSetting)
                {
                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, new BoolEditor(), null, dependsOn )
                    );
                }
                else if (sett is IntSetting)
                {
                    IntSetting isett = (IntSetting) sett;

                    object editor = null;
                    TypeConverter converter = null;
                    //if (isett.MinValue.HasValue && isett.MaxValue.HasValue)
                    editor = new BoundedIntEditor( isett.MinValue, isett.MaxValue );
                    //else if (isett.MinValue.HasValue || isett.MaxValue.HasValue)
                    //{
                    //    converter = new BoundedIntConverter( isett.MinValue, isett.MaxValue );
                    //}

                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, editor, converter, dependsOn )
                    );
                }
                else if (sett is FloatSetting)
                {
                    FloatSetting fsett = (FloatSetting) sett;

                    object editor;
                    editor = new FlexibleFloatEditor(
                        fsett.MinValue,
                        fsett.MaxValue,
                        fsett.SoftMinValue,
                        fsett.SoftMaxValue,
                        fsett.StepSize,
                        fsett.HasCheckBox
                        );

                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, editor, null, dependsOn )
                    );
                }
                else if (sett is EnumSetting)
                {
                    EnumSetting esett = (EnumSetting) sett;

                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    Type enumType = esett.EnumType;
                    string[] enumNames = enumType.GetEnumNames();
                    for (int i = 0; i < enumNames.Length; ++i)
                    {
                        FieldInfo enumField = enumType.GetField( enumNames[i] );
                        EnumLabelAttribute attr = enumField.GetCustomAttribute<EnumLabelAttribute>();
                        enumNames[i] = attr != null ? attr.Label : enumNames[i];
                    }

                    Array values = enumType.GetEnumValues();
                    int[] enumValues = new int[enumNames.Length];
                    for (int i = 0; i < enumNames.Length; ++i)
                        enumValues[i] = (int) values.GetValue( i );

                    //FieldInfo enumField = enumType.GetField( enumNames[i] );
                    //EnumLabelAttribute attr = enumField.GetCustomAttribute<EnumLabelAttribute>();
                    //string enumLabel = attr != null ? attr.Label : enumNames[i];

                    //var formatNames = Enum.GetValues( esett.EnumType );
                    //var formatEditor = new LongEnumEditor( esett.EnumType );
                    //var formatEditor = new EnumUITypeEditor( enumNames, enumValues );
                    var formatEditor = new LongEnumEditor();
                    formatEditor.DefineEnum( enumNames );
                    formatEditor.MaxDropDownItems = 10;

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, formatEditor, new IntEnumTypeConverter( enumNames, enumValues ), dependsOn )
                    );
                }
                else if (sett is Float4Setting)
                {
                    Float4Setting fsett = (Float4Setting) sett;

                    var editor = new NumericTupleEditor( typeof( float ), new string[] { "X", "Y", "Z", "W" } );

                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, editor, null, dependsOn )
                    );
                }
                else if (sett is ColorSetting)
                {
                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, new ColorPickerEditor(), new IntColorConverter(), dependsOn )
                    );
                }
                else if (sett is StringSetting)
                {
                    AttributeInfo ai = dnt.GetAttributeInfo( sett.Name );

                    propertyDescriptorCollection.Add(
                        CreateAttributePropertyDescriptor( sett, ai, null, null, dependsOn )
                    );
                }
                else
                {
                    throw new Exception( "Unsupported setting type!" );
                }
            }

            foreach (SettingGroup nestedStructure in structure.NestedStructures)
            {
                InitGuiRecurse( nestedStructure, schema );
            }
        }

		public void InitGui( Schema schema )
		{
            foreach (SettingGroup structure in m_compiler.RootStructure.NestedStructures)
            {
                InitGuiRecurse( structure, schema );
            }
		}

        public void CreateNodesRecurse( DomNode parent, SettingGroup parentStructure )
        {
            // shallow copy children
            //
            List<DomNode> children = new List<DomNode>();
            foreach (DomNode domNode in parent.Children)
            {
                children.Add( domNode );
            }

            // clear children list
            //
            if ( parent.Type == Schema.settingsFileType.Type )
                parent.GetChildList( Schema.settingsFileType.structChild ).Clear();
            else
                parent.GetChildList( Schema.structType.structChild ).Clear();

            // add groups in correct order, creating missing groups along the way
            //
            foreach (SettingGroup structure in parentStructure.NestedStructures)
            {
                DomNodeType groupType = Loader.GetNodeType( structure.DomNodeTypeFullName );
                DomNode group = children.Find( n => n.Type == groupType );

                if (group == null)
                {
                    group = new DomNode( groupType );

                    //// create preset
                    ////
                    //DomNodeType presetType = Loader.GetNodeType( "SettingsEditor:" + structure.Name );
                    //DomNode preset = new DomNode( presetType );

                    //// IdAttribute's default value is empty
                    //// must set it to anything non-empty so UniqueIdValidator can rename it correctly
                    //// otherwise, exception would be thrown
                    ////
                    //if (presetType.IdAttribute != null)
                    //    preset.SetAttribute( presetType.IdAttribute, structure.Name );

                    //group.GetChildList( Schema.groupType.presetsChild ).Add( preset );
                }

                if (parent.Type == Schema.settingsFileType.Type)
                    parent.GetChildList( Schema.settingsFileType.structChild ).Add( group );
                else
                    parent.GetChildList( Schema.structType.structChild ).Add( group );


                CreateNodesRecurse( group, groupType.GetTag<SettingGroup>() );
            }

        }

        public void CreateNodes( DomNode rootNode )
        {
            CreateNodesRecurse( rootNode, m_compiler.RootStructure );
        }

        public SchemaLoader Loader { set; get; }
        public Schema Schema { set; get; }

        private int[] GetEnumIntValues( Type type )
		{
			System.Array valuesArray = Enum.GetValues( type );
			int[] intArray = new int[valuesArray.Length];
			for (int i = 0; i < valuesArray.Length; ++i)
			{
				intArray[i] = (int) valuesArray.GetValue( i );
			}
			return intArray;
		}

		SettingsCompiler m_compiler;
	}

    public class DependsOnNodes : ICustomEnableAttributePropertyDescriptorCallback
    {
        public DependsOnNodes()
        {
             m_dependsOnList = new List<Pair<DomNodeType, AttributeInfo>>();
        }

        public DependsOnNodes( List<Pair<DomNodeType, AttributeInfo>> dependsOn )
        {
            m_dependsOnList = dependsOn;
        }

        public void Add( DomNodeType dnt, AttributeInfo attrInfo )
        {
            m_dependsOnList.Add( new Pair<DomNodeType, AttributeInfo>( dnt, attrInfo ) );
        }

        public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
        {
            DomNode root = domNode.GetRoot();

            foreach ( Pair<DomNodeType, AttributeInfo> p in m_dependsOnList )
            {
                foreach( DomNode dn in root.Subtree )
                {
                    if ( dn.Type == p.First )
                    {
                        bool bval = (bool)dn.GetAttribute( p.Second );
                        if ( !bval )
                            return true;
                    }
                }
            }

            return false;
        }

        List<Pair<DomNodeType, AttributeInfo>> m_dependsOnList;
    }

}
