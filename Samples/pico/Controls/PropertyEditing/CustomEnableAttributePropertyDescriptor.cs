//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.Controls.PropertyEditing
{
	public interface ICustomEnableAttributePropertyDescriptorCallback
	{
		bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor );
	};

	public class CustomEnableAttributePropertyDescriptorCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public enum Condition
		{
			ReadOnlyIfSetToTrue,
			ReadOnlyIfSetToFalse
		};

		public CustomEnableAttributePropertyDescriptorCallback( AttributeInfo attributeInfo, Condition condition )
		{
			if ( attributeInfo.Type.Type != AttributeTypes.Boolean )
			{
				throw new ArgumentException( "attributeInfo must be Boolean type" );
			}
			m_attributeInfo = attributeInfo;
			m_condition = condition;
		}

		public CustomEnableAttributePropertyDescriptorCallback( Func<DomNode, CustomEnableAttributePropertyDescriptor, bool> isReadOnlyPredicate )
		{
			m_isReadOnlyPredicate = isReadOnlyPredicate;
		}

		public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
		{
			if ( m_isReadOnlyPredicate != null )
			{
				bool bval = m_isReadOnlyPredicate( domNode, descriptor );
				return bval;
			}
			else
			{
				object val = domNode.GetAttribute( m_attributeInfo );
				bool bval = (bool)val;

				if ( m_condition == Condition.ReadOnlyIfSetToTrue )
				{
					return bval;
				}
				else
				{
					return !bval;
				}
			}
		}

		Func<DomNode, CustomEnableAttributePropertyDescriptor, bool> m_isReadOnlyPredicate;
		private AttributeInfo m_attributeInfo;
		private Condition m_condition;
	}

    /// <summary>
    /// Node adapter to get PropertyDescriptors from from metadata
	/// Based on Atf's CustomTypeDescriptorNodeAdapter.
	/// Implements disabling inputs based on selected properties
	/// </summary>
	public class CustomEnableAttributePropertyDescriptor : AttributePropertyDescriptor
	{
		public CustomEnableAttributePropertyDescriptor(
			string name,
			AttributeInfo attribute,
			string category,
			string description,
			bool isReadOnly,
			object editor
			, ICustomEnableAttributePropertyDescriptorCallback callback
			)

			: base( name, attribute, category, description, isReadOnly, editor, null )
		{
			m_callback = callback;
		}

		public override bool IsReadOnlyComponent( object component )
		{
			DomNode domNode = GetNode( component );
			bool readOnly =  m_callback.IsReadOnly( domNode, this );
			return readOnly;
		}

		//private Func<DomNode, AttributeInfo, bool> m_isReadOnlyPredicate;
		ICustomEnableAttributePropertyDescriptorCallback m_callback;
	};
}
