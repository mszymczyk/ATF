//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace TextureEditor
{
    /// <summary>
    /// Node adapter to get PropertyDescriptors from NodeType and other metadata</summary>
	public class TexturePropertyDescriptorNodeAdapter : DomNodeAdapter, ICustomTypeDescriptor
	//public class TexturePropertyDescriptorNodeAdapter : CustomTypeDescriptorNodeAdapter
	{
		static bool CopySourceFileIsReadOnlyPredicate( DomNode domNode, AttributeInfo attributeInfo )
		{
			TextureMetadata tp = domNode.Cast<TextureMetadata>();
			return tp.CopySourceFile;
		}

		class CustomEnableAttributePropertyDescriptor : AttributePropertyDescriptor
		{
			public CustomEnableAttributePropertyDescriptor(
				string name,
				AttributeInfo attribute,
				string category,
				string description,
				bool isReadOnly,
				object editor
				, DomNode domNode
				, Func<DomNode, AttributeInfo, bool> isReadOnlyPredicate = null
				)

				: base( name, attribute, category, description, isReadOnly, editor, null )
			{
				m_domNode = domNode;
				m_attributeInfo2 = attribute;
				m_isReadOnlyPredicate = isReadOnlyPredicate;
				//m_isReadOnly2 = isReadOnly;
			}

			///// <summary>
			///// When overridden in a derived class, gets the result value of the property on a component</summary>
			///// <param name="component">The component with the property for which to retrieve the value</param>
			///// <returns>The value of a property for a given component.</returns>
			//public override object GetValue( object component )
			//{
			//	DomNode node = GetNode( component );
			//	TextureMetadata tm = component.As<TextureMetadata>();
			//	if ( tm != null )
			//	{
			//		if ( tm.CopySourceFile )
			//			m_isReadOnly2 = true;
			//		else
			//			m_isReadOnly2 = false;
			//	}
			//	else
			//	{
			//		m_isReadOnly2 = false;
			//	}

			//	return base.GetValue( component );
			//}

			/// <summary>
			/// When overridden in a derived class, gets a value indicating whether this property is read-only</summary>
			public override bool IsReadOnly
			{
				//get { return m_isReadOnly2; }
				get
				{
					//TextureMetadata tp = m_domNode.Cast<TextureMetadata>();
					//System.Diagnostics.Debug.WriteLine( "IsReadOnly: {0}, {1}", tp.Uri, tp.CopySourceFile );
					//return tp.CopySourceFile;
					if ( m_isReadOnlyPredicate != null )
						return m_isReadOnlyPredicate( m_domNode, m_attributeInfo2 );
					else
						return false;
				}
			}

			/// <summary>
			/// Tests equality of property descriptor with object</summary>
			/// <param name="obj">Object to compare to</param>
			/// <returns>True iff property descriptors are identical</returns>
			/// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
			public override bool Equals( object obj )
			{
				var other = obj as CustomEnableAttributePropertyDescriptor;

				// If true is returned, then GetNode() must also succeed when it calls
				//  DomNode.Type.IsValid(m_attributeInfo), otherwise this AttributePropertyDescriptor
				//  will be considered identical in a dictionary, but its GetValue() will fail.
				return
					other != null &&
					m_attributeInfo2.Equivalent( other.m_attributeInfo2 ) &&
					m_domNode.Equals( other.m_domNode );
			}

			/// <summary>
			/// Gets hash code for property descriptor</summary>
			/// <returns>Hash code</returns>
			/// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
			public override int GetHashCode()
			{
				return m_attributeInfo2.GetEquivalentHashCode();
			}

			private readonly AttributeInfo m_attributeInfo2;
			private DomNode m_domNode;
			private Func<DomNode, AttributeInfo, bool> m_isReadOnlyPredicate;
			//private bool m_isReadOnly2;
		};

        /// <summary>
        /// Creates an array of property descriptors that are associated with the adapted DomNode's
        /// DomNodeType. No duplicates are in the array (based on the property descriptor's Name
        /// property).</summary>
        /// <returns>Array of property descriptors</returns>
        protected virtual System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
			//HashSet<string> names = new HashSet<string>();
			//List<System.ComponentModel.PropertyDescriptor> result = new List<System.ComponentModel.PropertyDescriptor>();
			////result.AddRange( base.GetPropertyDescriptors() );

			//DomNodeType nodeType = DomNode.Type;
			//while ( nodeType != null )
			//{
			//	PropertyDescriptorCollection propertyDescriptors = nodeType.GetTag<PropertyDescriptorCollection>();
			//	if ( propertyDescriptors != null )
			//	{
			//		foreach ( System.ComponentModel.PropertyDescriptor propertyDescriptor in propertyDescriptors )
			//		{
			//			// Use combination of category and name, to allow having properties with the
			//			// same display name under different categories.
			//			string fullName = string.Format( "{0}_{1}", propertyDescriptor.Category, propertyDescriptor.Name );

			//			// Filter out duplicate names, so derived type data overrides base type data)
			//			if ( !names.Contains( fullName ) )
			//			{
			//				names.Add( fullName );
			//				result.Add( propertyDescriptor );
			//			}
			//		}
			//	}
			//	nodeType = nodeType.BaseType;
			//}

			TextureMetadata tm = DomNode.Cast<TextureMetadata>();
			string uriExt = System.IO.Path.GetExtension( tm.Uri.LocalPath );

			string group_Metadata = "Metadata".Localize();

			List<System.ComponentModel.PropertyDescriptor> textureMetadataTypeProperyCollection = new List<System.ComponentModel.PropertyDescriptor>();

			textureMetadataTypeProperyCollection.Add(
				new AttributePropertyDescriptor(
						"URI".Localize(),
						Schema.resourceMetadataType.uriAttribute,
						group_Metadata,
						"Uri".Localize(),
						true
						)
			);

			if ( uriExt == ".dds" )
			{
				textureMetadataTypeProperyCollection.Add(
					new AttributePropertyDescriptor(
							 "Copy source file".Localize(),
							 Schema.textureMetadataType.copySourceFileAttribute,
							 group_Metadata,
							 "Copies source file without any modifications".Localize(),
							 false,
							 new BoolEditor()
							 )
				);
			}

			textureMetadataTypeProperyCollection.Add(
				new CustomEnableAttributePropertyDescriptor(
						 "Generate mipmaps".Localize(),
						 Schema.textureMetadataType.genMipMapsAttribute,
						 group_Metadata,
						 "Controlls mipmap generation".Localize(),
						 false,
						 new BoolEditor()
						 , DomNode
						 , CopySourceFileIsReadOnlyPredicate
						 )
			);

			textureMetadataTypeProperyCollection.Add(
				new CustomEnableAttributePropertyDescriptor(
						 "Flip Y".Localize(),
						 Schema.textureMetadataType.flipYAttribute,
						 group_Metadata,
						 "Flips image vertically".Localize(),
						 false,
						 new BoolEditor()
						 , DomNode
						 //, (DomNode domNode, AttributeInfo attributeInfo) => 
						 //   {
						 //	   TextureMetadata tp = domNode.Cast<TextureMetadata>();
						 //	   return tp.CopySourceFile;
						 //   }
						 , CopySourceFileIsReadOnlyPredicate
						 )
			);

			textureMetadataTypeProperyCollection.Add(
				new CustomEnableAttributePropertyDescriptor(
						 "Width".Localize(),
						 Schema.textureMetadataType.widthAttribute,
						 group_Metadata,
						 "Sets exported image's width".Localize(),
						 false,
				//new BoundedIntEditor( -1, 16 * 1024 * 1024 )
						 new NumericEditor( typeof( int ) )
						 , DomNode
						 , CopySourceFileIsReadOnlyPredicate
						 )
			);

			textureMetadataTypeProperyCollection.Add(
				new CustomEnableAttributePropertyDescriptor(
						 "Height".Localize(),
						 Schema.textureMetadataType.heightAttribute,
						 group_Metadata,
						 "Sets exported image's height".Localize(),
						 false,
						 new NumericEditor( typeof( int ) )
						 , DomNode
						 , CopySourceFileIsReadOnlyPredicate
						 )
			);

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
				var apd = new CustomEnableAttributePropertyDescriptor(
					"Format".Localize(),
					Schema.textureMetadataType.formatAttribute,
					group_Metadata,
					"Specifies format of exported texture".Localize(),
					false,
					formatEditor
					, DomNode
					, ( DomNode domNode, AttributeInfo attributeInfo ) =>
					{
						TextureMetadata tp = domNode.Cast<TextureMetadata>();
						if ( tp.CopySourceFile )
							return true;
						return tp.ExtendedFormat != SharpDX.DXGI.Format.Unknown;
					}
				);
				textureMetadataTypeProperyCollection.Add( apd );
			}

			{
				var formatNames = Enum.GetValues( typeof( SharpDX.DXGI.Format ) );
				var formatEditor = new LongEnumEditor( typeof( SharpDX.DXGI.Format ), null );
				formatEditor.MaxDropDownItems = 10;
				var apd = new CustomEnableAttributePropertyDescriptor(
					"ExtendedFormat".Localize(),
					Schema.textureMetadataType.extendedFormatAttribute,
					group_Metadata,
					"Specifies format of exported texture (advanced)".Localize(),
					false,
					formatEditor
					, DomNode
					, ( DomNode domNode, AttributeInfo attributeInfo ) =>
						{
							TextureMetadata tp = domNode.Cast<TextureMetadata>();
							if ( tp.CopySourceFile )
								return true;
							return tp.Format != SharpDX.DXGI.Format.Unknown;
						}
				);
				textureMetadataTypeProperyCollection.Add( apd );
			}

			return textureMetadataTypeProperyCollection.ToArray();
        }

		/// <summary>
		/// Gets class name for the node. Default is to return the node type name.</summary>
		/// <returns>Class name</returns>
		protected virtual string GetClassName()
		{
			return DomNode.Type.Name;
		}

		#region ICustomTypeDescriptor Members

		/// <summary>
		/// Returns the properties for this instance of a component</summary>
		/// <returns>A System.ComponentModel.PropertyDescriptorCollection that represents the properties for this component instance</returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return new PropertyDescriptorCollection( GetPropertyDescriptors() );
		}

		/// <summary>
		/// Returns the properties for this instance of a component using the attribute array as a filter</summary>
		/// <param name="attributes">An array of type System.Attribute that is used as a filter</param>
		/// <returns>A System.ComponentModel.PropertyDescriptorCollection that represents the filtered properties for this component instance</returns>
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties( Attribute[] attributes )
		{
			return new PropertyDescriptorCollection( GetPropertyDescriptors() );
		}

		/// <summary>
		/// Returns the class name of this instance of a component</summary>
		/// <returns>The class name of the object, or null if the class does not have a name</returns>
		String ICustomTypeDescriptor.GetClassName()
		{
			return GetClassName();
		}

		/// <summary>
		/// Returns a collection of custom attributes for this instance of a component</summary>
		/// <returns>An System.ComponentModel.AttributeCollection containing the attributes for this object</returns>
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes( this, true );
		}

		/// <summary>
		/// Returns the name of this instance of a component</summary>
		/// <returns>The name of the object, or null if the object does not have a name</returns>
		String ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName( this, true );
		}

		/// <summary>
		/// Returns a type converter for this instance of a component</summary>
		/// <returns>A System.ComponentModel.TypeConverter that is the converter for this object, 
		/// or null if there is no System.ComponentModel.TypeConverter for this object</returns>
		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter( this, true );
		}

		/// <summary>
		/// Returns the default event for this instance of a component</summary>
		/// <returns>An System.ComponentModel.EventDescriptor that represents the default event for this object,
		/// or null if this object does not have events</returns>
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent( this, true );
		}

		/// <summary>
		/// Returns the default property for this instance of a component</summary>
		/// <returns>A System.ComponentModel.PropertyDescriptor that represents the default property for this object, 
		/// or null if this object does not have properties</returns>
		System.ComponentModel.PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty( this, true );
		}

		/// <summary>
		/// Returns an editor of the specified type for this instance of a component</summary>
		/// <param name="editorBaseType">A System.Type that represents the editor for this object</param>
		/// <returns>An System.Object of the specified type that is the editor for this object, 
		/// or null if the editor cannot be found</returns>
		object ICustomTypeDescriptor.GetEditor( Type editorBaseType )
		{
			return TypeDescriptor.GetEditor( this, editorBaseType, true );
		}

		/// <summary>
		/// Returns the events for this instance of a component using the specified attribute array as a filter</summary>
		/// <param name="attributes">An array of type System.Attribute that is used as a filter</param>
		/// <returns>An System.ComponentModel.EventDescriptorCollection that represents the filtered events for this component instance</returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents( Attribute[] attributes )
		{
			return TypeDescriptor.GetEvents( this, attributes, true );
		}

		/// <summary>
		/// Returns the events for this instance of a component</summary>
		/// <returns>An System.ComponentModel.EventDescriptorCollection that represents the events for this component instance</returns>
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents( this, true );
		}

		/// <summary>
		/// Returns an object that contains the property described by the specified property descriptor</summary>
		/// <param name="pd">A System.ComponentModel.PropertyDescriptor that represents the property whose owner is to be found</param>
		/// <returns>An System.Object that represents the owner of the specified property</returns>
		object ICustomTypeDescriptor.GetPropertyOwner( System.ComponentModel.PropertyDescriptor pd )
		{
			return DomNode;
		}

		#endregion
    }
}
