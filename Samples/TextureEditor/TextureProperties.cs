//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using SharpDX.Direct3D11;

namespace TextureEditor
{
	/// <summary>
	/// Class describing properties of texture resource
	public class TextureProperties : CustomTypeDescriptor
	{
		/// <summary>
		/// Constructor with parameters</summary>
		/// <param name="name">Item name</param>
		/// <param name="type">Item data type</param>
		/// <param name="value">Item value</param>
		//public TextureProperties( string name, DataType type, object value )
		public TextureProperties( Uri file, SharpDX.Direct3D11.Resource res, TexturePreviewWindowSharpDX previewWindow )
		{
			FileUri = file;
			m_res = res;
			m_previewWindow = previewWindow;
			//Name = name;
			//Type = type;
			//Value = value;

			SharpDX.DXGI.Format tmpFormat = SharpDX.DXGI.Format.Unknown;

			ResourceDimension dim = m_res.Dimension;
			if (dim == ResourceDimension.Texture1D)
			{
				Texture1D tex = m_res as Texture1D;
				Texture1DDescription desc = tex.Description;
				Format = desc.Format.ToString();
				Width = desc.Width;
				Height = 0;
				Depth = 0;
				MipLevels = desc.MipLevels;
				ArraySize = desc.ArraySize;
				CubeMap = (desc.OptionFlags & ResourceOptionFlags.TextureCube) > 0;
				tmpFormat = desc.Format;
			}
			else if ( dim == ResourceDimension.Texture2D )
			{
				Texture2D tex = m_res as Texture2D;
				Texture2DDescription desc = tex.Description;
				Format = desc.Format.ToString();
				Width = desc.Width;
				Height = desc.Height;
				Depth = 0;
				MipLevels = desc.MipLevels;
				ArraySize = desc.ArraySize;
				CubeMap = (desc.OptionFlags & ResourceOptionFlags.TextureCube) > 0;
				if ( CubeMap )
					ArraySize /= 6;
				tmpFormat = desc.Format;
			}
			else if (dim == ResourceDimension.Texture3D)
			{
				Texture3D tex = m_res as Texture3D;
				Texture3DDescription desc = tex.Description;
				Format = desc.Format.ToString();
				Width = desc.Width;
				Height = desc.Height;
				Depth = desc.Depth;
				MipLevels = desc.MipLevels;
				ArraySize = 0;
				CubeMap = (desc.OptionFlags & ResourceOptionFlags.TextureCube) > 0;
				tmpFormat = desc.Format;
			}
			else
			{
				throw new Exception( "Unsupported resource type type" );
			}

			if ( tmpFormat == SharpDX.DXGI.Format.B8G8R8A8_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.B8G8R8X8_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.BC1_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.BC2_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.BC3_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.BC7_UNorm_SRgb
				||	tmpFormat == SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb
				)
			{
				DoGammaToLinearConversion = false;
				m_isSrgbFormat = true;
			}
			else
			{
				DoGammaToLinearConversion = true;
			}

		}

		/// <summary>
		/// Uri of a texture</summary>
		[PropertyEditingAttribute]
		public Uri FileUri { get; set; }

		///// <summary>
		///// Gets item name</summary>
		//[PropertyEditingAttribute]
		//public string Name { get; set; }

		///// <summary>
		///// Gets item data type</summary>
		//[PropertyEditingAttribute]
		//public DataType Type { get; set; }

		///// <summary>
		///// Gets item value</summary>
		//[PropertyEditingAttribute]
		//public object Value { get; set; }

		/// <summary>
		/// Format of texture</summary>
		[PropertyEditingAttribute]
		public string Format { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int Width { get; set; }

		/// <summary>
		/// Height of texture</summary>
		[PropertyEditingAttribute]
		public int Height { get; set; }

		/// <summary>
		/// Depth of texture</summary>
		[PropertyEditingAttribute]
		public int Depth { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int MipLevels { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public int ArraySize { get; set; }

		/// <summary>
		/// Width of texture</summary>
		[PropertyEditingAttribute]
		public bool CubeMap { get; set; }

		/// <summary>
		/// Mip to display </summary>
		[PropertyEditingAttribute(false)]
		public int VisibleMip { get; set; }

		/// <summary>
		/// Slice to display </summary>
		[PropertyEditingAttribute(false)]
		public int VisibleSlice { get; set; }

		/// <summary>
		/// Slice to display </summary>
		//[PropertyEditingAttribute( false )]
		public bool DoGammaToLinearConversion { get; set; }

		#region Property Editing

		/// <summary>
		/// Attribute of DataItem</summary>
		[AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
		public class PropertyEditingAttribute : Attribute
		{
			public bool ReadOnly { get; set; }
			public PropertyEditingAttribute()
			{
				ReadOnly = true;
			}

			public PropertyEditingAttribute( bool readOnly )
			{
				ReadOnly = readOnly;
			}
		}

		/// <summary>
		/// PropertyDescriptor with additional information for a property</summary>
		public class PropertyPropertyDescriptor : System.ComponentModel.PropertyDescriptor
		{
			/// <summary>
			/// Constructor with parameters</summary>
			/// <param name="property">PropertyInfo for property</param>
			/// <param name="ownerType">Owning type</param>
			public PropertyPropertyDescriptor( PropertyInfo property, Type ownerType )
				: base( property.Name, (Attribute[]) property.GetCustomAttributes( typeof( Attribute ), true ) )
			{
				m_property = property;
				m_ownerType = ownerType;
				m_readOnly = true;

				foreach (Attribute attr in property.GetCustomAttributes(typeof(PropertyEditingAttribute), false) )
				{
					PropertyEditingAttribute p = attr as PropertyEditingAttribute;
					m_readOnly = p.ReadOnly;
				}

				m_category = m_readOnly ? "SrcInfo" : "Misc";
			}

			/// <summary>
			/// Gets owning type</summary>
			public Type OwnerType
			{
				get { return m_ownerType; }
			}

			/// <summary>
			/// Gets PropertyInfo for property</summary>
			public PropertyInfo Property
			{
				get { return m_property; }
			}

			/// <summary>
			/// Gets whether this property is read-only</summary>
			public override bool IsReadOnly
			{
				//get { return GetChildProperties().Count <= 0; }
				//get { return true; }
				get { return m_readOnly; }
				//set { m_readOnly = value; }
			}

			public void SetReadOnly( bool readOnly )
			{
				m_readOnly = readOnly;
			}

			/// <summary>
			/// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"></see></summary>
			public override string Category
			{
				get { return m_category; }
			}

			public void SetCategory( string category )
			{
				m_category = category;
			}

			/// <summary>
			/// Returns whether resetting an object changes its value</summary>
			/// <param name="component">Component to test for reset capability</param>
			/// <returns>Whether resetting a component changes its value</returns>
			public override bool CanResetValue( object component )
			{
				return false;
			}

			/// <summary>
			/// Resets the value for this property of the component to the default value</summary>
			/// <param name="component"></param>
			public override void ResetValue( object component )
			{
			}

			/// <summary>
			/// Determines whether the value of this property needs to be persisted</summary>
			/// <param name="component">Component with the property to be examined for persistence</param>
			/// <returns>True iff the property should be persisted</returns>
			public override bool ShouldSerializeValue( object component )
			{
				return false;
			}

			/// <summary>
			/// Gets the type of the component this property is bound to</summary>
			public override Type ComponentType
			{
				get { return m_property.DeclaringType; }
			}

			/// <summary>
			/// Gets the type of the property</summary>
			public override Type PropertyType
			{
				get { return m_property.PropertyType; }
			}

			/// <summary>
			/// Gets the current value of property on component</summary>
			/// <param name="component">Component with the property value that is to be set</param>
			/// <returns>New value</returns>
			public override object GetValue( object component )
			{
				return m_property.GetValue( component, null );
			}

			/// <summary>
			/// Sets the value of the component to a different value</summary>
			/// <param name="component">Component with the property value that is to be set</param>
			/// <param name="value">New value</param>
			public override void SetValue( object component, object value )
			{
				m_property.SetValue( component, value, null );

				ItemChanged.Raise( component, EventArgs.Empty );
			}

			///// <summary>
			///// Gets an editor of the specified type</summary>
			///// <param name="editorBaseType">The base type of editor, 
			///// which is used to differentiate between multiple editors that a property supports</param>
			///// <returns>Instance of the requested editor type, or null if an editor cannot be found</returns>
			//public override object GetEditor( Type editorBaseType )
			//{
			//	if (m_property.PropertyType.Equals( typeof( DataItem ) ))
			//		return m_nestedCollectionEditor ?? (m_nestedCollectionEditor = new NestedCollectionEditor());

			//	return base.GetEditor( editorBaseType );
			//}

			/// <summary>
			/// Event that is raised after an item changed</summary>
			public event EventHandler ItemChanged;

			private readonly Type m_ownerType;
			private readonly PropertyInfo m_property;
			private string m_category;
			private bool m_readOnly;
		}


		///// <summary>
		///// PropertyDescriptor with additional information for a property</summary>
		//public class LimitedIntPropertyDescriptor : Sce.Atf.Dom.PropertyDescriptor
		//{
		//	/// <summary>
		//	/// Constructor with parameters</summary>
		//	/// <param name="property">PropertyInfo for property</param>
		//	/// <param name="ownerType">Owning type</param>
		//	public LimitedIntPropertyDescriptor( PropertyInfo property, Type ownerType )
		//	//public LimitedIntPropertyDescriptor()
		//		//: base( property.Name, (Attribute[]) property.GetCustomAttributes( typeof( Attribute ), true ) )
		//		: base( property.Name, property.GetType(), "Misc".Localize(), "Misc".Localize(), false, new BoundedIntEditor(0, 10), null )
		//	{
		//		m_property = property;
		//		m_ownerType = ownerType;
		//	}

		//	///// <summary>
		//	///// Gets owning type</summary>
		//	//public Type OwnerType
		//	//{
		//	//	get { return m_ownerType; }
		//	//}

		//	///// <summary>
		//	///// Gets PropertyInfo for property</summary>
		//	//public PropertyInfo Property
		//	//{
		//	//	get { return m_property; }
		//	//}

		//	///// <summary>
		//	///// Gets whether this property is read-only</summary>
		//	//public override bool IsReadOnly
		//	//{
		//	//	//get { return GetChildProperties().Count <= 0; }
		//	//	get { return true; }
		//	//}

		//	/// <summary>
		//	/// Returns whether resetting an object changes its value</summary>
		//	/// <param name="component">Component to test for reset capability</param>
		//	/// <returns>Whether resetting a component changes its value</returns>
		//	public override bool CanResetValue( object component )
		//	{
		//		return false;
		//	}

		//	/// <summary>
		//	/// Resets the value for this property of the component to the default value</summary>
		//	/// <param name="component"></param>
		//	public override void ResetValue( object component )
		//	{
		//	}

		//	///// <summary>
		//	///// Determines whether the value of this property needs to be persisted</summary>
		//	///// <param name="component">Component with the property to be examined for persistence</param>
		//	///// <returns>True iff the property should be persisted</returns>
		//	//public override bool ShouldSerializeValue( object component )
		//	//{
		//	//	return false;
		//	//}

		//	///// <summary>
		//	///// Gets the type of the component this property is bound to</summary>
		//	//public override Type ComponentType
		//	//{
		//	//	get { return m_property.DeclaringType; }
		//	//}

		//	///// <summary>
		//	///// Gets the type of the property</summary>
		//	//public override Type PropertyType
		//	//{
		//	//	get { return m_property.PropertyType; }
		//	//}

		//	/// <summary>
		//	/// Gets the current value of property on component</summary>
		//	/// <param name="component">Component with the property value that is to be set</param>
		//	/// <returns>New value</returns>
		//	public override object GetValue( object component )
		//	{
		//		return m_property.GetValue( component, null );
		//	}

		//	/// <summary>
		//	/// Sets the value of the component to a different value</summary>
		//	/// <param name="component">Component with the property value that is to be set</param>
		//	/// <param name="value">New value</param>
		//	public override void SetValue( object component, object value )
		//	{
		//		m_property.SetValue( component, value, null );

		//		//ItemChanged.Raise( component, EventArgs.Empty );
		//	}

		//	/////// <summary>
		//	/////// Gets an editor of the specified type</summary>
		//	/////// <param name="editorBaseType">The base type of editor, 
		//	/////// which is used to differentiate between multiple editors that a property supports</param>
		//	/////// <returns>Instance of the requested editor type, or null if an editor cannot be found</returns>
		//	////public override object GetEditor( Type editorBaseType )
		//	////{
		//	////	if (m_property.PropertyType.Equals( typeof( DataItem ) ))
		//	////		return m_nestedCollectionEditor ?? (m_nestedCollectionEditor = new NestedCollectionEditor());

		//	////	return base.GetEditor( editorBaseType );
		//	////}

		//	///// <summary>
		//	///// Event that is raised after an item changed</summary>
		//	//public event EventHandler ItemChanged;

		//	private readonly Type m_ownerType;
		//	private readonly PropertyInfo m_property;
		//}

		/// <summary>
		/// Returns a collection of property descriptors for the object represented by this type descriptor</summary>
		/// <returns>System.ComponentModel.PropertyDescriptorCollection containing the property descriptions for the object 
		/// represented by this type descriptor. The default is System.ComponentModel.PropertyDescriptorCollection.Empty.</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			var props = new PropertyDescriptorCollection( null );

			foreach (var property in GetType().GetProperties())
			{
				var propertyDesc =
                    new PropertyPropertyDescriptor( property, GetType() );

				propertyDesc.ItemChanged += PropertyDescItemChanged;

				foreach (Attribute attr in propertyDesc.Attributes)
				{
					if (attr.GetType().Equals( typeof( PropertyEditingAttribute ) ))
						props.Add( propertyDesc );
				}
			}

			PropertyInfo DoGammaToLinearConversionProp = GetType().GetProperty( "DoGammaToLinearConversion" );
			PropertyPropertyDescriptor ppd = new PropertyPropertyDescriptor( DoGammaToLinearConversionProp, GetType() );
			ppd.ItemChanged += PropertyDescItemChanged;
			ppd.SetCategory( "Misc" );
			if ( m_isSrgbFormat )
				ppd.SetReadOnly( true );
			else
				ppd.SetReadOnly( false );
			props.Add( ppd );

			return props;
		}

		/// <summary>
		/// Returns an object that contains the property described by the specified property descriptor</summary>
		/// <param name="pd">Property descriptor for which to retrieve the owning object</param>
		/// <returns>System.Object that owns the given property specified by the type descriptor. The default is null.</returns>
		public override object GetPropertyOwner( System.ComponentModel.PropertyDescriptor pd )
		{
			return this;
		}

		private void PropertyDescItemChanged( object sender, EventArgs e )
		{
			m_previewWindow.Invalidate();
			ItemChanged.Raise( this, EventArgs.Empty );
		}

		#endregion

		/// <summary>
		/// Event that is raised after an item changed</summary>
		public event EventHandler ItemChanged;

		private SharpDX.Direct3D11.Resource m_res;
		private TexturePreviewWindowSharpDX m_previewWindow;
		private bool m_isSrgbFormat;
	}
}

