//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
    public class KeyChangeLevel : Key
    {
		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string LevelName
		{
			get { return (string)DomNode.GetAttribute( Schema.keyChangeLevelType.levelNameAttribute); }
			set { DomNode.SetAttribute( Schema.keyChangeLevelType.levelNameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the sound name</summary>
		public string CutsceneFile
		{
			get { return (string)DomNode.GetAttribute( Schema.keyChangeLevelType.cutsceneFileAttribute ); }
			set { DomNode.SetAttribute( Schema.keyChangeLevelType.cutsceneFileAttribute, value ); }
		}
    }
}




