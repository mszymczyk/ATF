using Sce.Atf.Dom;

namespace TextureEditor
{
	/// <summary>
	/// Texture metadata</summary>
	public class TextureMetadata : DomNodeAdapter
	{
		/// <summary>
		/// Gets or sets game object name</summary>
		public string Keywords
		{
			get { return GetAttribute<string>(Schema.resourceMetadataType.keywordsAttribute); }
			set { SetAttribute(Schema.resourceMetadataType.keywordsAttribute, value); }
		}
	
		//public int Width
		//{
		//	get { return GetAttribute<int>(Schema.textureMetadataType.widthAttribute); }
		//	set { SetAttribute(Schema.textureMetadataType.widthAttribute, value); }
		//}

		//public int Height
		//{
		//	get { return GetAttribute<int>(Schema.textureMetadataType.heightAttribute); }
		//	set { SetAttribute(Schema.textureMetadataType.heightAttribute, value); }
		//}
	}
}
