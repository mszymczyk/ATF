using System;
using System.ComponentModel;
using System.Reflection;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;

namespace SettingsEditor
{
	public class DynamicSchema : IDynamicSchema
	{
		private static readonly AttributeType Float4Type = new AttributeType( "Float4", typeof( float[] ), 4 );

		public DynamicSchema( SettingsCompiler compiler )
		{
			m_compiler = compiler;
		}

		public void AddDynamicAttributes( DomNodeType dnt )
		{
			//AttributeInfo ai = new AttributeInfo( "attrBoolean", AttributeType.BooleanType );
			//dnt.Define( ai );

			foreach( SettingGroup settGroup in m_compiler.ReflectedSettings )
			{
				foreach ( Setting sett in settGroup.Settings )
				{
					if ( sett is BoolSetting )
					{
						BoolSetting bsett = (BoolSetting) sett;

						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.BooleanType );
						ai.DefaultValue = bsett.Value;
						dnt.Define( ai );
					}
					else if ( sett is IntSetting )
					{
						IntSetting isett = (IntSetting) sett;

						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.IntType );
						ai.DefaultValue = isett.Value;
						dnt.Define( ai );
					}
					else if ( sett is EnumSetting )
					{
						EnumSetting esett = (EnumSetting) sett;

						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.IntType );
						//ai.DefaultValue = esett.EnumType.GetEnumName( esett.Value );
						ai.DefaultValue = (int)esett.Value;
						dnt.Define( ai );
					}
					else if (sett is FloatSetting)
					{
						FloatSetting fsett = (FloatSetting) sett;

						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.FloatType );
						ai.DefaultValue = fsett.Value;
						dnt.Define( ai );
					}
					else if (sett is StringSetting)
					{
						StringSetting ssett = (StringSetting) sett;

						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.StringType );
						ai.DefaultValue = ssett.Value ?? "";
						dnt.Define( ai );
					}
					else if (sett is Float4Setting)
					{
						Float4Setting fsett = (Float4Setting) sett;
						AttributeInfo ai = new AttributeInfo( sett.LongName, Float4Type );
						ai.DefaultValue = new float[4] { fsett.Value.X, fsett.Value.Y, fsett.Value.Z, fsett.Value.W };
						dnt.Define( ai );
					}
					else if (sett is ColorSetting)
					{
						ColorSetting csett = (ColorSetting) sett;
						AttributeInfo ai = new AttributeInfo( sett.LongName, AttributeType.IntType );
						ai.DefaultValue = csett.asInt();
						dnt.Define( ai );
					}
					else
					{
						throw new Exception( "Unsupported setting type!" );
					}
				}
			}

			//AttributeType StringArrayType = new AttributeType( "StringArray", typeof( string[] ), Int32.MaxValue );
			//AttributeInfo ainfo = new AttributeInfo( "StringArrayTest", StringArrayType );
			////AttributeInfo ainfo = new AttributeInfo( "StringArrayTest", AttributeType.StringType );
			//dnt.Define( ainfo );

		}
		public void InitGui( DomNodeType dnt, PropertyDescriptorCollection propertyDescriptorCollection )
		{
			foreach (SettingGroup settGroup in m_compiler.ReflectedSettings)
			{
				foreach (Setting sett in settGroup.Settings)
				{
					if (sett is BoolSetting)
					{
						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							new BoolEditor()
							) );
					}
					else if (sett is IntSetting)
					{
						IntSetting isett = (IntSetting) sett;

						object editor = null;
						TypeConverter converter = null;
						if (isett.MinValue.HasValue && isett.MaxValue.HasValue)
							editor = new BoundedIntEditor( isett.MinValue.Value, isett.MaxValue.Value );
						else if (isett.MinValue.HasValue || isett.MaxValue.HasValue)
						{
							converter = new BoundedIntConverter( isett.MinValue, isett.MaxValue );
						}

						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							editor,
							converter
							) );
					}
					else if (sett is EnumSetting)
					{
						EnumSetting esett = (EnumSetting) sett;

						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );

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
						var apd = new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							formatEditor,
							//new EnumTypeConverter( enumNames, enumValues )
							//new IntEnumTypeConverter( esett.EnumType )
							new IntEnumTypeConverter( enumNames, enumValues )
							);
						propertyDescriptorCollection.Add( apd );
					}
					else if (sett is FloatSetting)
					{
						FloatSetting fsett = (FloatSetting) sett;

						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							new BoundedFloatEditor( fsett.MinValue, fsett.MaxValue )
							) );
					}
					else if (sett is Float4Setting)
					{
						Float4Setting fsett = (Float4Setting) sett;

						var editor = new NumericTupleEditor( typeof( float ), new string[] { "X", "Y", "Z", "W" } );

						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							editor
							) );
					}
					else if (sett is ColorSetting)
					{
						ColorSetting fsett = (ColorSetting) sett;

						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false,
							new ColorPickerEditor(),
							new IntColorConverter()
							) );
					}
					else if (sett is StringSetting)
					{
						AttributeInfo ai = dnt.GetAttributeInfo( sett.LongName );
						propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
							sett.DisplayName,
							ai,
							sett.Group.Name,
							sett.HelpText,
							false
							) );
					}
					else
					{
						throw new Exception( "Unsupported setting type!" );
					}
				}
			}

			//{
			//	AttributeInfo ai = dnt.GetAttributeInfo( "StringArrayTest" );
			//	propertyDescriptorCollection.Add( new AttributePropertyDescriptor(
			//		"StringArray",
			//		ai,
			//		"StringSample",
			//		"StringSampleHelp",
			//		false,
			//		new ArrayEditor()
			//		) );
			//}
		}

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
}
