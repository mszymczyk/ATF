//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

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

		public void updateIcon()
		{
			string absFile = pico.Paths.LocalPathToPicoDataAbsolutePath( m_filename );
			if ( string.IsNullOrEmpty(absFile) )
			{
				m_iconName = "picoAnimClipEditor.Resources.track.png";
				return;
			}

			string animdata =  absFile + "data";
			if ( System.IO.File.Exists( animdata ) )
			{
				System.DateTime animDate = File.GetLastWriteTime( absFile );
				System.DateTime animdataDate = File.GetLastWriteTime( animdata );
				if ( animDate > animdataDate )
				{
					m_iconName = "picoAnimClipEditor.Resources.animdataOutdated.png";
				}
				else
				{
					m_iconName = "picoAnimClipEditor.Resources.animdataOk.png";
				}
			}
			else
			{
				m_iconName = "picoAnimClipEditor.Resources.animdataMissing.png";
			}
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

		public string IconName
		{
			get { return m_iconName; }
		}

		private string m_category;
		private string m_userName;
		private string m_filename;
		private string m_iconName;
    }
}



