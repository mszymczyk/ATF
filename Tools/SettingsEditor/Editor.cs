//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace SettingsEditor
{
    /// <summary>
    /// Creates rootnode and one child of type Orc.</summary>
    [Export(typeof(IInitializable))]
	[Export( typeof( IDocumentClient ) )]
	[Export( typeof( Editor ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public class Editor : IDocumentClient, IControlHostClient, IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finish MEF intialization for the component by creating DomNode tree for application data.</summary>
        void IInitializable.Initialize()
		{
			//m_mainform.Shown += (sender, e) =>
			//	{
			//		//Schema schema = new Schema();
			//		//SchemaLoader schemaLoader = new SchemaLoader( schema );

			//		//// create root node.
			//		//var rootNode = new DomNode(schema.settingsFileType.Type, schema.settingsFileRootElement);

			//		//rootNode.InitializeExtensions();

			//		//var edContext = rootNode.Cast<DocumentEditingContext>();
			//		//edContext.Set( rootNode );

			//		//// set active context and select orc object.
			//		//m_contextRegistry.ActiveContext = rootNode;

			//		//string filePath = Directory.GetCurrentDirectory() + "\\paramFileTest.xml";
			//		//FileMode fileMode = File.Exists( filePath ) ? FileMode.Truncate : FileMode.OpenOrCreate;
			//		//using (FileStream stream = new FileStream( filePath, fileMode ))
			//		//{
			//		//	var writer = new DomXmlWriter( schemaLoader.TypeCollection );
			//		//	writer.PersistDefaultAttributes = true;
			//		//	writer.Write( rootNode, stream, new Uri(filePath) );
			//		//}

			//		CreateNewParamFile();
			//	};

			{
				string descr = "Root path for settings description files".Localize();
				var codeDir =
					new BoundPropertyDescriptor( typeof( Globals ), () => Globals.CodeDirectory,
						"SettingsDescRoot".Localize(),
						null,
						descr,
						new FolderBrowserDialogUITypeEditor( descr ), null );

				m_settingsService.RegisterSettings( this, codeDir );
				m_settingsService.RegisterUserSettings( "SettingsDescRoot".Localize(), codeDir );
			}
			{
				string descr = "Root path for settings files".Localize();
				var dataDir =
					new BoundPropertyDescriptor( typeof( Globals ), () => Globals.DataDirectory,
						"SettingsRoot".Localize(),
						null,
						descr,
						new FolderBrowserDialogUITypeEditor( descr ), null );

				m_settingsService.RegisterSettings( this, dataDir );
				m_settingsService.RegisterUserSettings( "SettingsRoot".Localize(), dataDir );
			}

			m_mainForm.AllowDrop = true;
			m_mainForm.DragEnter += mainForm_DragEnter;
			m_mainForm.DragDrop += mainForm_DragDrop;

			m_fileWatcherService.FileChanged += fileWatcherService_FileChanged;
		}

		private string DataDirectory
		{
			get
			{
				return Globals.DataDirectory;
			}
			set
			{
				Globals.DataDirectory = value;
			}
		}

        #endregion

        Document CreateNewParamFile( Uri settingsFile, string descFile, string shaderOutputPath, bool createNew )
		{
            string descFullPath = Path.GetFullPath( descFile );

            SettingsCompiler compiler;
            if ( m_reloadInfo != null )
            {
                compiler = m_reloadInfo.m_compiler;
            }
            else
            {
                compiler = new SettingsCompiler();
			    compiler.ReflectSettings( descFullPath );
            }

            compiler.GenerateHeaderIfChanged( descFullPath, shaderOutputPath );

            DynamicSchema dynamicSchema = new DynamicSchema( compiler );
			Schema schema = new Schema( dynamicSchema );
			SchemaLoader schemaLoader = new SchemaLoader( schema, dynamicSchema );

			DomNode rootNode = null;
			if ( createNew )
			{
				rootNode = new DomNode( schema.settingsFileType.Type, schema.settingsFileRootElement );

                dynamicSchema.CreateNodes(rootNode);
            }
            else
			{
				// read existing document using standard XML reader
				using ( FileStream stream = new FileStream( settingsFile.LocalPath, FileMode.Open, FileAccess.Read ) )
				{
                    SettingsEditor.DomXmlReader reader = new SettingsEditor.DomXmlReader( schemaLoader );
					rootNode = reader.Read( stream, settingsFile );
				}

                dynamicSchema.CreateNodes( rootNode );
            }

            //foreach (Group group in rootNode.Children.AsIEnumerable<Group>())
            //{
            //    if (string.IsNullOrEmpty( group.SelectedPresetName ))
            //    {
            //        group.SelectedPresetName = group.Presets[0].Name;
            //    }
            //}

            string filePath = settingsFile.LocalPath;
			string fileName = Path.GetFileName( filePath );

			FileInfo fileInfo = new FileInfo( descFullPath );

            Document document = rootNode.Cast<Document>();
            document.Schema = schema;
            document.SchemaLoader = schemaLoader;

            DocumentControl control;
            ControlInfo controlInfo;
            if ( m_reloadInfo != null )
            {
                control = m_reloadInfo.m_documentControl;
                control.Setup( rootNode, schema );
                controlInfo = m_reloadInfo.m_documentControl.ControlInfo;
            }
            else
            {
                control = new DocumentControl( this );
                control.Setup( rootNode, schema );
                controlInfo = new ControlInfo( fileName, filePath, StandardControlGroup.Center );
                control.ControlInfo = controlInfo;
            }
			
			// IsDocument needs to be set to persist layout and placement of document
			// after editor is closed
			//
            controlInfo.IsDocument = true;

            document.ControlInfo = controlInfo;
            document.Control = control;

			document.DescFilePath = descFullPath;
            document.PathRelativeToData = Globals.GetPathRelativeToData(filePath);
            string descFileRelativePath = Globals.GetPathRelativeToCode( descFullPath );
            document.DescFileRelativePath = descFileRelativePath;

			document.Uri = settingsFile;

			if (!createNew)
				document.ExplicitlySavedByUser = true;

			rootNode.InitializeExtensions();

			var edContext = rootNode.Cast<DocumentEditingContext>();
			edContext.Set( rootNode );

			// set active context and select orc object.
			m_contextRegistry.ActiveContext = rootNode;

            if ( m_reloadInfo == null )
			    m_controlHostService.RegisterControl( control, controlInfo, this );

			m_fileWatcherService.Register( descFullPath );

            // if file is being reloaded, try setting last valid selection
            // this might be not possible, because group names might have changed
            //
            if ( m_reloadInfo != null && m_reloadInfo.m_selectedGroupName != null )
            {
                foreach( Struct s in rootNode.Subtree.AsIEnumerable<Struct>() )
                {
                    if (s.DomNode.Type.Name == m_reloadInfo.m_selectedGroupName)
                    {
                        control.SetSelectedDomNode( s.DomNode );
                    }
                }
            }

            // save it so new groups\settings get written to disk
            //
            document.SaveImpl();

            return document;
		}

		#region IDocumentClient Members

		/// <summary>
		/// Gets information about the document client, such as the file type and file
		/// extensions it supports, whether or not it allows multiple documents to be open, etc.</summary>
		public DocumentClientInfo Info
		{
			get { return s_info; }
		}

		/// <summary>
		/// Returns whether the client can open or create a document at the given URI</summary>
		/// <param name="uri">Document URI</param>
		/// <returns>True iff the client can open or create a document at the given URI</returns>
		public bool CanOpen( Uri uri )
		{
			return s_info.IsCompatibleUri( uri );
		}

		/// <summary>
		/// Info describing our document type</summary>
		private static DocumentClientInfo s_info =
            new DocumentClientInfo(
				"SettingsFile".Localize(),   // file type
				".settings",                      // file extension
				null,                       // "new document" icon
				null );                      // "open document" icon

		/// <summary>
		/// Opens or creates a document at the given URI</summary>
		/// <param name="uri">Document URI</param>
		/// <returns>Document, or null if the document couldn't be opened or created</returns>
		public IDocument Open( Uri uri )
		{
			//DomNode node = null;
			string filePath = uri.LocalPath;

			if ( File.Exists( filePath ) )
			{
				string pathRelativeToData = Globals.GetPathRelativeToData( uri.LocalPath );
				if (string.IsNullOrEmpty( pathRelativeToData ))
					throw new InvalidSettingsPathException( uri );

				//// read existing document using standard XML reader
				//using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
				//{
				//	DomXmlReader reader = new DomXmlReader( m_schemaLoader );
				//	node = reader.Read( stream, uri );
				//}
				string descFullPath = ExtractDescFile( uri );
                string shaderOutputPath = ExtractShaderFile( uri );
				if ( !string.IsNullOrEmpty(descFullPath) )
				{
					Document document = CreateNewParamFile( uri, descFullPath, shaderOutputPath, false );
					return document;
				}
			}
			else if ( m_descFileToUseWhenCreatingNewDocument != null )
			{
				//// create new document by creating a Dom node of the root type defined by the schema
				//node = new DomNode( UISchema.UIType.Type, UISchema.UIRootElement );
				//UI ui = node.As<UI>();
				//ui.Name = "UI";

				Document document = CreateNewParamFile( uri, m_descFileToUseWhenCreatingNewDocument, "", true );
				return document;
			}

			//Document document = null;
			//if (node != null)
			//{
			//	// Initialize Dom extensions now that the data is complete; after this, all Dom node
			//	//  adapters will have been bound to their underlying Dom node.
			//	node.InitializeExtensions();

			//	// get the root node's UIDocument adapter
			//	document = node.As<Document>();
			//	document.Uri = uri;

			//	// only allow 1 open document at a time
			//	Document activeDocument = m_documentRegistry.GetActiveDocument<Document>();
			//	if (activeDocument != null)
			//		Close( activeDocument );
			//}

			//return document;

			//SettingsFile pf = CreateNewParamFile( uri, descFile );

			//return pf.Document;
			return null;
		}
		
		/// <summary>
		/// Makes the document visible to the user</summary>
		/// <param name="document">Document to show</param>
		public void Show( IDocument document )
		{
			// set the active document and context; as there is only one editing context in
			//  a document, the document is also a context.
			m_contextRegistry.ActiveContext = document;
			m_documentRegistry.ActiveDocument = document;
		}

		/// <summary>
		/// Saves the document at the given URI</summary>
		/// <param name="document">Document to save</param>
		/// <param name="uri">New document URI</param>
		public void Save( IDocument document, Uri uri )
		{
			string pathRelativeToData = Globals.GetPathRelativeToData( uri.LocalPath );
			if ( string.IsNullOrEmpty(pathRelativeToData) )
				throw new Exception( "Path is not relative to SettingsRoot" );

			Document doc = document as Document;
			doc.PathRelativeToData = pathRelativeToData;

			//Document doc = null;
			////if (m_settingFiles.TryGetValue( e.OldUri, out doc ))
			//if ( m_settingFiles.ContainsValue( doc ) )
			//{
			//	//m_settingFiles.Remove( e.OldUri );
			//	m_settingFiles
			//	m_settingFiles.Add( doc.Uri, document );
			//}
			//else
			//{
			//	throw new Exception( string.Format( "Document {0} not present on list", e.OldUri.ToString() ) );
			//}

			//bool found = false;
			//foreach ( var pair in m_settingFiles)
			//{
			//	if (doc.Equals( pair.Value ))
			//	{
			//		found = true;
			//		m_settingFiles.Remove( pair.Key );
			//		break;
			//	}
			//}

			//if (!found)
			//{
			//	throw new Exception( string.Format( "Document {0} not present on list", doc.Uri.ToString() ) );
			//}

			//m_settingFiles.Add( docu

			//SettingsFile pf = doc.Cast<DocumentEditingContext>().ParamFile;
			//pf.ExplicitlySavedByUser = true;
			doc.ExplicitlySavedByUser = true;
			doc.SaveImpl();
		}

		/// <summary>
		/// Closes the document and removes any views of it from the UI</summary>
		/// <param name="document">Document to close</param>
		public void Close( IDocument document )
		{
			//SettingsFile pf = document.Cast<Document>().SettingsFile;
			Document doc = document.Cast<Document>();

			//doc.UriChanged -= document_UriChanged;

			//DocumentEditingContext editingContext = document.Cast<DocumentEditingContext>();
            if ( m_reloadInfo == null )
			    m_controlHostService.UnregisterControl( doc.Control );
			m_contextRegistry.RemoveContext( document );
			m_documentRegistry.Remove( document );

			//m_settingFiles.Remove( document.Uri );
			m_fileWatcherService.Unregister( doc.DescFilePath );
		}

		#endregion

		#region IControlHostClient Members

		/// <summary>
		/// Notifies the client that its Control has been activated. Activation occurs when
		/// the Control gets focus, or a parent "host" Control gets focus.</summary>
		/// <param name="control">Client Control that was activated</param>
		/// <remarks>This method is only called by IControlHostService if the Control was previously
		/// registered for this IControlHostClient.</remarks>
		public void Activate( Control control )
		{
			DocumentControl pfControl = (DocumentControl) control;
			if (pfControl != null)
			{
				Document pfDocument = pfControl.RootNode.Cast<Document>();
				m_contextRegistry.ActiveContext = pfDocument;
				m_documentRegistry.ActiveDocument = pfDocument;
			}
		}

		/// <summary>
		/// Notifies the client that its Control has been deactivated. Deactivation occurs when
		/// another Control or "host" Control gets focus.</summary>
		/// <param name="control">Client Control that was deactivated</param>
		/// <remarks>This method is only called by IControlHostService if the Control was previously
		/// registered for this IControlHostClient.</remarks>
		public void Deactivate( Control control )
		{
		}

		/// <summary>
		/// Requests permission to close the client's Control</summary>
		/// <param name="control">Client Control to be closed</param>
		/// <returns>True if the Control can close, or false to cancel</returns>
		/// <remarks>
		/// 1. This method is only called by IControlHostService if the Control was previously
		/// registered for this IControlHostClient.
		/// 2. If true is returned, the IControlHostService calls its own
		/// UnregisterControl. The IControlHostClient has to call RegisterControl again
		/// if it wants to re-register this Control.</remarks>
		public bool Close( Control control )
		{
			//D2dTimelineControl timelineControl = (D2dTimelineControl) control;
			//TimelineDocument timelineDocument = (TimelineDocument) timelineControl.TimelineDocument;

			//if (timelineDocument != null)
			//	return m_documentService.Close( timelineDocument );
			DocumentControl pfControl = (DocumentControl) control;
			if (pfControl != null)
			{
				Document pfDocument = pfControl.RootNode.Cast<Document>();
				m_documentService.Close( pfDocument );
			}

			return true;
		}

		#endregion

		private string ExtractDescFile( Uri uri )
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load( uri.LocalPath );
			}
			catch ( Exception ex )
			{
				string msg = ex.Message;
				return string.Empty;
			}

			XmlElement documentElement = xmlDoc.DocumentElement;
			string descFile = documentElement.GetAttribute( "settingsDescFile" );
			if ( string.IsNullOrEmpty( descFile ) )
				return string.Empty;

			string descFileFull = Globals.GetCodeFullPath( descFile );
			return descFileFull;
		}
        private string ExtractShaderFile( Uri uri )
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load( uri.LocalPath );
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return string.Empty;
            }

            XmlElement documentElement = xmlDoc.DocumentElement;
            string shaderFile = documentElement.GetAttribute( "shaderOutputFile" );
            if (string.IsNullOrEmpty( shaderFile ))
                return string.Empty;
            string shaderFileFull = Globals.GetCodeFullPath( shaderFile );
            return shaderFileFull;
        }

        private class ReloadInfo
        {
            public SettingsCompiler m_compiler = null;
            public DocumentControl m_documentControl = null;
            public string m_selectedGroupName = null;
        }

		public void Reload( Document document )
		{
            Uri settingsFile = document.Uri;
            string descFilePath = document.DescFilePath;

            SettingsCompiler compiler = null;
            try
            {
                compiler = new SettingsCompiler();
                //string descFullPath = Path.GetFullPath( descFile );
                compiler.ReflectSettings( descFilePath );
            }
            catch (Exception ex)
            {
                Outputs.WriteLine( OutputMessageType.Error, string.Format( "Reload failed! Exception while processing '{0}': {1}", descFilePath, ex.Message ) );
                return;
            }

            m_reloadInfo = new ReloadInfo();
            m_reloadInfo.m_compiler = compiler;
            m_reloadInfo.m_documentControl = document.Control;

            m_documentService.Close( document );
            m_descFileToUseWhenCreatingNewDocument = descFilePath;
            ISelectionContext selectionContext = document.As<ISelectionContext>();
            if ( selectionContext != null )
            {
                Struct group = selectionContext.LastSelected.As<Struct>();
                if ( group != null )
                    m_reloadInfo.m_selectedGroupName = group.DomNode.Type.Name;
            }

            m_documentService.OpenExistingDocument( this, settingsFile );
            m_descFileToUseWhenCreatingNewDocument = null;
            //m_groupNameToRestoreAfterFileReload = null;

            //m_controlToUseWhenReloading = null;
            m_reloadInfo = null;
        }

        private void mainForm_DragEnter( object sender, DragEventArgs e )
		{
			e.Effect = DragDropEffects.None;
			if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
			{
				string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
				foreach ( string file in files )
				{
					string ext = Path.GetExtension( file );
					if ( ext != ".cs" )
						return;

					string picoPath = Globals.GetPathRelativeToCode( file );
					if ( string.IsNullOrEmpty( picoPath ) )
						return;
				}

				e.Effect = DragDropEffects.Copy;
			}
		}

		private void mainForm_DragDrop( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
			{
				string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
				foreach ( string file in files )
				{
					m_descFileToUseWhenCreatingNewDocument = file;
					m_documentService.OpenNewDocument( this );
					m_descFileToUseWhenCreatingNewDocument = null;
				}

				e.Effect = DragDropEffects.Copy;
			}
		}

		/// <summary>
		/// Performs custom actions when FileChanged event occurs. 
		/// Updates current document if necessary.</summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">FileSystemEventArgs containing event data</param>
		void fileWatcherService_FileChanged( object sender, FileSystemEventArgs e )
		{
			Uri descFileUri = new Uri( e.FullPath );
			//SettingsFile settingsFile;
			//if ( m_settingFiles.TryGetValue( uri, out settingsFile ) )
			//{
			//	Reload( settingsFile.Document );
			//}
			//foreach( Document doc in m_settingFiles.Values )
			foreach( Document doc in m_documentRegistry.Documents )
			{
				if ( doc.DescFilePath == descFileUri.LocalPath )
				{
					FileInfo fileInfo = new FileInfo( doc.DescFilePath );
					DateTime lastWriteTime = fileInfo.LastWriteTime;
					if (lastWriteTime > doc.LoadedWriteTime)
					{
						Reload( doc );
					}
				
					break;
				}
			}
		}

        //void document_UriChanged( object sender, UriChangedEventArgs e )
        //{
        //	Document document = sender.Cast<Document>();
        //	Document doc = null;
        //	if (m_settingFiles.TryGetValue( e.OldUri, out doc ))
        //	{
        //		m_settingFiles.Remove( e.OldUri );
        //		m_settingFiles.Add( doc.Uri, document );
        //	}
        //	else
        //	{
        //		throw new Exception( string.Format( "Document {0} not present on list", e.OldUri.ToString() ) );
        //	}
        //}

        /// <summary>
        /// Gets all context menu command providers</summary>
        public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
        {
            get
            {
                return
                    m_contextMenuCommandProviders == null
                        ? EmptyEnumerable<IContextMenuCommandProvider>.Instance
                        : m_contextMenuCommandProviders.GetValues();
            }
        }

        public ICommandService CommandService
        {
            get { return m_commandService; }
        }

        [Import( AllowDefault = false )]
		private MainForm m_mainForm = null; //initialize to null to avoid incorrect compiler warning

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry = null; //initialize to null to avoid incorrect compiler warning

		[Import( AllowDefault = false )]
		private IDocumentRegistry m_documentRegistry = null;

		[Import( AllowDefault = false )]
		private IDocumentService m_documentService = null;

		[Import( AllowDefault = false )]
		private IControlHostService m_controlHostService = null;

		[Import( AllowDefault=true )]
		private IFileWatcherService m_fileWatcherService = null;

		[Import( AllowDefault = false )]
		private SettingsService m_settingsService = null;

        private string m_descFileToUseWhenCreatingNewDocument = null;
        //private string m_groupNameToRestoreAfterFileReload = null;
        // we use the same control when reloading description file
        // this way we can keep it's layout and placement in the editor
        // without this, old control is closed and new one is docked in center
        //
        //private DocumentControl m_controlToUseWhenReloading = null;
        private ReloadInfo m_reloadInfo = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        [Import( AllowDefault = false )]
        private ICommandService m_commandService = null;
    }
}
