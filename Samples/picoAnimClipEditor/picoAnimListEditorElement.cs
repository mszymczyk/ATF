//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace picoAnimClipEditor
{
    /// <summary>
    /// Class that adapts a DomNode to an event; a base class for adapters for Intervals, Markers, and Keys</summary>
    public class picoAnimListEditorElement
    {
		public picoAnimListEditorElement( string category, string animUserName, string animFilename )
		{
			m_category = category;
			m_userName = animUserName;
			m_filename = animFilename;
		}

		public string Category
		{
			get { return m_category; }
		}

		public string UserName
		{
			get { return m_userName; }
		}

		public string FileName
		{
			get { return m_filename; }
		}

		private string m_category;
		private string m_userName;
		private string m_filename;
    }
}



