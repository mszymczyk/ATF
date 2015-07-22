using System;

using Sce.Atf.Dom;

namespace TextureEditor
{
	/// <summary>
	/// Texture metadata</summary>
	public class TextureMetadata : DomNodeAdapter
	{
		public static readonly string TEXTURE_PRESET_UNKNOWN = "Unknown";
		public static readonly string TEXTURE_PRESET_COLOR_BC1_SRGB = "Color (Compressed BC1 SRGB)";
		public static readonly string TEXTURE_PRESET_COLOR_BC3_SRGB = "Color+Alpha (Compressed BC3 SRGB)";
		public static readonly string TEXTURE_PRESET_COLOR_BC7_SRGB = "Color+Alpha HiQuality (Compressed BC7 SRGB)";
		public static readonly string TEXTURE_PRESET_COLOR_SRGB = "Color+Alpha HiQuality (Uncompressed R8G8B8A8 SRGB)";
		public static readonly string TEXTURE_PRESET_COLOR_BC6H_HDR_UNORM = "Color HDR (Compressed BC6H UNORM)";
		public static readonly string TEXTURE_PRESET_COLOR_HDR_UNORM = "Color+Alpha HDR (Uncompressed R16G16B16A16 FLOAT)";
		public static readonly string TEXTURE_PRESET_NORMALMAP_BC5 = "Normal (Compressed BC5 SNORM)";
		public static readonly string TEXTURE_PRESET_NORMALMAP_RG8 = "Normal (Uncompressed RG8 SNORM)";
		public static readonly string TEXTURE_PRESET_AMBIENT_BC4 = "Ambient (Compressed BC4 UNORM)";
		public static readonly string TEXTURE_PRESET_AMBIENT_R8 = "Ambient (Uncompressed R8 UNORM)";
		public static readonly string TEXTURE_PRESET_SPECULARMAP_BC1 = "Specular (Compressed BC1 UNORM)";
		public static readonly string TEXTURE_PRESET_SPECULARMAP_UNORM = "Specular (Uncompressed R8G8B8A8 UNORM)";
		public static readonly string TEXTURE_PRESET_CUSTOM_FORMAT = "Custom Format";

		public Uri Uri
		{
			get { return GetAttribute<Uri>( Schema.resourceMetadataType.uriAttribute ); }
			set { SetAttribute( Schema.resourceMetadataType.uriAttribute, value ); }
		}

		/// <summary>
		/// Gets or sets game object name</summary>
		public string Keywords
		{
			get { return GetAttribute<string>(Schema.resourceMetadataType.keywordsAttribute); }
			set { SetAttribute(Schema.resourceMetadataType.keywordsAttribute, value); }
		}

		public bool GenMipMaps
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.genMipMapsAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.genMipMapsAttribute, value ); }
		}

		public bool ForceSourceSrgb
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.forceSourceSrgbAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.forceSourceSrgbAttribute, value ); }
		}

		public bool FlipY
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.flipYAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.flipYAttribute, value ); }
		}

		public bool CopySourceFile
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.copySourceFileAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.copySourceFileAttribute, value ); }
		}

		//public SharpDX.DXGI.Format Format
		//{
		//	get
		//	{
		//		string sFormat = GetAttribute<string>( Schema.textureMetadataType.formatAttribute );
		//		SharpDX.DXGI.Format eFormat = SharpDX.DXGI.Format.Unknown;
		//		if ( Enum.TryParse<SharpDX.DXGI.Format>( sFormat, out eFormat ) )
		//		{
		//			return eFormat;
		//		}
		//		else
		//		{
		//			throw new Exception( "Unsupported format" );
		//			//return SharpDX.DXGI.Format.Unknown;
		//		}
		//	}
		//	set { SetAttribute( Schema.textureMetadataType.formatAttribute, value ); }
		//}
		public string Preset
		{
			get { return GetAttribute<string>( Schema.textureMetadataType.presetAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.presetAttribute, value ); }
		}

		public SharpDX.DXGI.Format Format
		{
			get
			{
				string sFormat = GetAttribute<string>( Schema.textureMetadataType.formatAttribute );
				SharpDX.DXGI.Format eFormat = SharpDX.DXGI.Format.Unknown;
				if ( Enum.TryParse<SharpDX.DXGI.Format>( sFormat, out eFormat ) )
				{
					return eFormat;
				}
				else
				{
					throw new Exception( "Unsupported format" );
				}
			}
			set { SetAttribute( Schema.textureMetadataType.formatAttribute, value ); }
		}

		public int Width
		{
			get { return GetAttribute<int>( Schema.textureMetadataType.widthAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.widthAttribute, value ); }
		}

		public int Height
		{
			get { return GetAttribute<int>( Schema.textureMetadataType.heightAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.heightAttribute, value ); }
		}
		//public int Depth
		//{
		//	get { return GetAttribute<int>( Schema.textureMetadataType.depthAttribute ); }
		//	set { SetAttribute( Schema.textureMetadataType.depthAttribute, value ); }
		//}


	}
}
