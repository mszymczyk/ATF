using Sce.Atf;

namespace SettingsEditor
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Group image resource resource filename</summary>
        [ImageResource("SelectedPreset.png")]
        public static readonly string SelectedPreset;
	}
}
