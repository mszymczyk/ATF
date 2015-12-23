using Sce.Atf.Dom;
using System.ComponentModel;

namespace SettingsEditor
{
	public interface IDynamicSchema
	{
		void AddDynamicAttributes( DomNodeType dnt );
		void InitGui( DomNodeType dnt, PropertyDescriptorCollection propertyDescriptorCollection );
	}
}
