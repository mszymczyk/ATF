//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace pico
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class ResourcesRegistry
    {
		/// <summary>
		/// Pause icon name</summary>
		[ImageResource( "Pause16.png", "Pause24.png", "Pause32.png" )]
		public static readonly string PauseImage;

		/// <summary>
		/// Play icon name</summary>
		[ImageResource( "Play16.png", "Play24.png", "Play32.png" )]
		public static readonly string PlayImage;

		private const string ResourcePath = "pico.Resources.";

		static ResourcesRegistry()
		{
			ResourceUtil.Register( typeof( ResourcesRegistry ), ResourcePath );
		}
	}
}
