using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Applications;

namespace SettingsEditor
{
	public class DocumentControl : UserControl
	{
		public DocumentControl( DomNode root )
		{
			//m_paramFile = paramFile;
			RootNode = root;

			m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
			m_propertyGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
			m_propertyGrid.Dock = DockStyle.Fill;
			//m_propertyGrid.Bind( m_paramFile.RootNode.Cast<DocumentEditingContext>() );
			m_propertyGrid.Bind( root.As<DocumentEditingContext>() );

			//m_splitContainer = new SplitContainer();
			//m_splitContainer.Text = "Dom Explorer";
			////m_splitContainer.Panel1.Controls.Add(  );
			//m_splitContainer.Panel2.Controls.Add( m_propertyGrid );

			Dock = DockStyle.Fill;
			//Controls.Add( m_splitContainer );
			Controls.Add( m_propertyGrid );
		}

		//public SettingsFile ParamFile { get { return m_paramFile; } }

		//private SettingsFile m_paramFile;
		//private readonly SplitContainer m_splitContainer;
		public DomNode RootNode { get; set; }

		private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;
	}
}
