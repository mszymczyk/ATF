//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace TextureEditor
{
    /// <summary>
    /// Creates root node and one child of type Orc.</summary>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IInitializable
    {
        [ImportingConstructor]
        public Editor(
            ISettingsService settingsService            
			//ResourceLister resourceLister
            )
        {
            m_settingsService = settingsService;            
			//m_resourceLister = resourceLister;            
            
            //to-do wire it to to command service
            ResolveOnLoad = true;
        }

        #region IInitializable Members

        /// <summary>
        /// Finish MEF initialization for the component by creating DomNode tree for application data.</summary>
        void IInitializable.Initialize()
        {
			RegisterSettings();

            m_mainform.Shown += (sender, e) =>
                {
					//// create root node.
					//var rootNode = new DomNode(Schema.gameType.Type, Schema.gameRootElement);
					//rootNode.SetAttribute(Schema.gameType.nameAttribute, "Game");

					//// create Orc game object and add it to rootNode.
					//var orc = CreateOrc();
					//rootNode.GetChildList(Schema.gameType.gameObjectChild).Add(orc);

					//// add a child Orc.
					//var orcChildList = orc.GetChildList(Schema.orcType.orcChild);
					//orcChildList.Add(CreateOrc("Child Orc1"));

					//rootNode.InitializeExtensions();

					//var edContext = rootNode.Cast<GameEditingContext>();
					//edContext.Set(orc);

					//// set active context and select orc object.
					//m_contextRegistry.ActiveContext = rootNode;
                    
                };
        }

        #endregion


		///// <summary>
		///// Gets or sets the root resource directory for finding assets</summary>        
		//public string ResourceRoot
		//{
		//	get
		//	{
		//		if (Globals.ResourceRoot != null)
		//			return Globals.ResourceRoot.LocalPath;
		//		return null;
		//	}
		//	set
		//	{
		//		if (!value.EndsWith( "\\" ))
		//			value += "\\";

		//		Uri uri = new Uri( value, UriKind.RelativeOrAbsolute );
		//		if (!uri.IsAbsoluteUri)
		//		{
		//			Uri startPath = new Uri( Application.StartupPath + "\\" );
		//			uri = new Uri( startPath, value );
		//		}

		//		Globals.ResourceRoot = uri;

		//		IResourceFolder rootResourceFolder = new CustomFileSystemResourceFolder( Globals.ResourceRoot.LocalPath );
		//		if (m_resourceLister != null)
		//			m_resourceLister.SetRootFolder( rootResourceFolder );
		//	}
		//}

		private void RegisterSettings()
		{
			//string descr = "Root path for all resources".Localize();
			//var resourceRoot =
			//	new BoundPropertyDescriptor( this, () => ResourceRoot,
			//		"ResourceRoot".Localize( "A user preference and the name of the preference in the settings file" ),
			//		null,
			//		descr,
			//		new FolderBrowserDialogUITypeEditor( descr ), null );


			//m_settingsService.RegisterSettings( this, resourceRoot );
			//m_settingsService.RegisterUserSettings( "Resources".Localize(), resourceRoot );

			var resolveOnLoad =
                new BoundPropertyDescriptor( this, () => ResolveOnLoad,
					"Resolve on load".Localize( "A user preference and the name of the preference in the settings file" ),
					null,
					"Resolve sub-documents on load".Localize() );

			string docs = "Documents".Localize();
			m_settingsService.RegisterSettings( docs, resolveOnLoad );
			m_settingsService.RegisterUserSettings( docs, resolveOnLoad );

		}

		public bool ResolveOnLoad
		{
			get;// { return GameDocument.ResolveOnLoad; }
			set;// { GameDocument.ResolveOnLoad = value; }
		}

		//private readonly ResourceLister m_resourceLister = null;        
		private readonly ISettingsService m_settingsService;

        [Import(AllowDefault = false)]
        private MainForm m_mainform = null; //initialize to null to avoid incorrect compiler warning

		//[Import(AllowDefault = false)]
		//private IContextRegistry m_contextRegistry = null; //initialize to null to avoid incorrect compiler warning
    }   
}
