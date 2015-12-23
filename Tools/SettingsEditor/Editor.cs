//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

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

		//SettingsFile CreateNewParamFile( Uri uri )
		//{
		//	SettingsCompiler compiler = new SettingsCompiler();
		//	compiler.ReflectSettings( "D:\\_pico\\cur_code\\external\\atf\\Tools\\SettingsEditor\\Compiler\\SampleSettings.cs" );

		//	DynamicSchema dynamicSchema = new DynamicSchema( compiler );
		//	SettingsFile pf = new SettingsFile( dynamicSchema );

		//	var rootNode = new DomNode( pf.Schema.settingsFileType.Type, pf.Schema.settingsFileRootElement );
		//	//rootNode.SetAttribute( pf.Schema.settingsFileType.settingsDescFileAttribute, );

		//	pf.RootNode = rootNode;

		//	//string filePath = Directory.GetCurrentDirectory() + "\\paramFileTest.xml";
		//	//FileMode fileMode = File.Exists( filePath ) ? FileMode.Truncate : FileMode.OpenOrCreate;
		//	//using (FileStream stream = new FileStream( filePath, fileMode ))
		//	//{
		//	//	var writer = new DomXmlWriter( pf.SchemaLoader.TypeCollection );
		//	//	writer.PersistDefaultAttributes = true;
		//	//	writer.Write( rootNode, stream, new Uri( filePath ) );
		//	//}

		//	pf.Document = pf.RootNode.Cast<Document>();
		//	pf.Document.Uri = uri;

		//	pf.Control = new DocumentControl( pf );

		//	var edContext = rootNode.Cast<DocumentEditingContext>();
		//	edContext.Set( rootNode );
		//	edContext.SettingsFile = pf;

		//	// set active context and select orc object.
		//	m_contextRegistry.ActiveContext = rootNode;

		//	string filePath = uri.LocalPath;
		//	string fileName = Path.GetFileName( filePath );

		//	ControlInfo controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);

		//	m_controlHostService.RegisterControl( pf.Control, controlInfo, this );

		//	return pf;
		//}

		Document CreateNewParamFile( Uri settingsFile, string descFile, bool createNew )
		{
			SettingsCompiler compiler = new SettingsCompiler();
			string descFullPath = Path.GetFullPath(descFile);
			compiler.ReflectSettings( descFullPath );
			compiler.GenerateHeaderIfChanged( descFullPath );

			DynamicSchema dynamicSchema = new DynamicSchema( compiler );
			Schema schema = new Schema( dynamicSchema );
			SchemaLoader schemaLoader = new SchemaLoader( schema, dynamicSchema );

			//SettingsFile pf = new SettingsFile( dynamicSchema );

			DomNode rootNode = null;
			if ( createNew )
			{
				rootNode = new DomNode( schema.settingsFileType.Type, schema.settingsFileRootElement );
			}
			else
			{
				// read existing document using standard XML reader
				using ( FileStream stream = new FileStream( settingsFile.LocalPath, FileMode.Open, FileAccess.Read ) )
				{
					DomXmlReader reader = new DomXmlReader( schemaLoader );
					rootNode = reader.Read( stream, settingsFile );
				}

				//pf.ExplicitlySavedByUser = true;
			}

			string filePath = settingsFile.LocalPath;
			string fileName = Path.GetFileName( filePath );

			//pf.RootNode = rootNode;

			//pf.DescFilePath = descFullPath;
			//string descFileRelativePath = Globals.GetPathRelativeToCode( descFullPath );
			//pf.DescFileRelativePath = descFileRelativePath;
			//pf.PathRelativeToData = Globals.GetPathRelativeToData( filePath );

			FileInfo fileInfo = new FileInfo( descFullPath );
			//pf.LoadedWriteTime = fileInfo.LastWriteTime;

			//rootNode.SetAttribute( pf.Schema.settingsFileType.settingsDescFileAttribute, descFileRelativePath );

			DocumentControl control = new DocumentControl( rootNode );
			ControlInfo controlInfo = new ControlInfo( fileName, filePath, StandardControlGroup.Center );
			//pf.ControlInfo = controlInfo;

			Document document = rootNode.Cast<Document>();
			document.Schema = schema;
			document.SchemaLoader = schemaLoader;
			//pf.Document.SettingsFile = pf;
			document.Control = control;
			document.ControlInfo = controlInfo;
			document.ExplicitlySavedByUser = !createNew;

			document.PathRelativeToData = Globals.GetPathRelativeToData( filePath );
			document.DescFilePath = descFullPath;
			string descFileRelativePath = Globals.GetPathRelativeToCode( descFullPath );
			document.DescFileRelativePath = descFileRelativePath;

			document.Uri = settingsFile;

			rootNode.InitializeExtensions();

			//m_settingFiles.Add( document.Uri, document );

			var edContext = rootNode.Cast<DocumentEditingContext>();
			edContext.Set( rootNode );
			//edContext.ParamFile = pf;

			// set active context and select orc object.
			m_contextRegistry.ActiveContext = rootNode;

			m_controlHostService.RegisterControl( control, controlInfo, this );

			m_fileWatcherService.Register( descFullPath );

			//document.UriChanged += document_UriChanged;

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
				if ( !string.IsNullOrEmpty(descFullPath) )
				{
					Document document = CreateNewParamFile( uri, descFullPath, false );
					return document;
				}
			}
			else if ( m_descFileToUseWhenCreatingNewDocument != null )
			{
				//// create new document by creating a Dom node of the root type defined by the schema
				//node = new DomNode( UISchema.UIType.Type, UISchema.UIRootElement );
				//UI ui = node.As<UI>();
				//ui.LongName = "UI";

				Document document = CreateNewParamFile( uri, m_descFileToUseWhenCreatingNewDocument, true );
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

		public void Reload( Document document )
		{
			Uri uri = document.Uri;
			string descFilePath = document.DescFilePath;
			m_documentService.Close( document );
			m_descFileToUseWhenCreatingNewDocument = descFilePath;
			m_documentService.OpenExistingDocument( this, uri );
			m_descFileToUseWhenCreatingNewDocument = null;
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
		//private Dictionary<Uri, Document> m_settingFiles = new Dictionary<Uri, Document>();
    }   
}
