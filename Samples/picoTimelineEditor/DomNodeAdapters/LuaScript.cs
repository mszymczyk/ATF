//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Controls.Timelines;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
    public class LuaScript : Key
    {
		/// <summary>
		/// Gets and sets the event's name</summary>
		public string SourceCode
		{
			get { return (string) DomNode.GetAttribute( Schema.luaScriptType.sourceCodeAttribute ); }
			set { DomNode.SetAttribute( Schema.luaScriptType.sourceCodeAttribute, value ); }
		}
    }
}




