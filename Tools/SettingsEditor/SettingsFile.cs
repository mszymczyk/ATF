using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Dom;
using Sce.Atf.Applications;

using SettingsEditor;

namespace SettingsEditor
{
	public class SettingsFile
	{
		public SettingsFile( IDynamicSchema dynamicSchema )
		{
			m_schema = new Schema( dynamicSchema );
			m_schemaLoader = new SchemaLoader( m_schema, dynamicSchema );

			SavedByUser = false;
		}

		public string DescFilePath { get; set; }

		public string DescFileRelativePath
		{
			get { return (string) RootNode.GetAttribute( Schema.settingsFileType.settingsDescFileAttribute ); }
			set { RootNode.SetAttribute( Schema.settingsFileType.settingsDescFileAttribute, value ); }
		}

		public string PathRelativeToData { get; set; }

		//public DomNode RootNode { get; set; }
		//public Document Document { get; set; }
		public DateTime LoadedWriteTime { get; set; }
		//public DocumentControl Control { get; set; }
		//public ControlInfo ControlInfo { get; set; }

		//public bool SavedByUser { get; set; }

		//public Schema Schema { get { return m_schema; } }
		//public SchemaLoader SchemaLoader { get { return m_schemaLoader; } }

		//private Schema m_schema;
		//private SchemaLoader m_schemaLoader;
	}
}
