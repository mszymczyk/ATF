﻿using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Applications;
using System.Windows.Forms;

namespace SettingsEditor
{
    /// <summary>
    /// Used for updating PropertyEditor on Undo/Redo</summary>
	public class DocumentEditingContext : EditingContext, IObservableContext, IPropertyEditingContext//, IInstancingContext
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// Subscribes to events for DomNode tree changes and raises Reloaded event.</summary>
        protected override void OnNodeSet()
        {
			DomNode.AttributeChanged += DomNode_AttributeChanged;
			DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += (sender,e)=> ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));

            SelectionChanged += DocumentEditingContext_SelectionChanged;

            Reloaded.Raise(this, EventArgs.Empty);
            base.OnNodeSet();

            m_saveFileTimer_.Interval = 300;
            m_saveFileTimer_.Tick += ( object sender, EventArgs e ) =>
                {
                    Document document = this.Cast<Document>();
                    document.SaveImpl();
                    m_saveFileTimer_.Stop();
                };
        }

        private void DocumentEditingContext_SelectionChanged(object sender, EventArgs e)
        {
            // Reloaded causes tree control to reload and is very unintuitive because some subtrees collapse
            //
            //Reloaded.Raise(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine("Selection changed!");
            Document document = this.Cast<Document>();
            // during loading phase, this control might be not yet initialized
            //
            if (document.Control != null)
                document.Control.Rebind();
        }

        #region IPropertyEditingContext Members

        /// <summary>
        /// Gets an enumeration of the items with properties</summary>
        public IEnumerable<object> Items
		{
			get
			{
                // if group was selected pick first preset, user doesn't need to expand group in order to select preset
                // this is just less mouse clicks
                // TODO: pick active preset
                //
                //if (LastSelected.Is<Group>())
                //{
                //    System.Diagnostics.Debug.WriteLine( "LastSelected: Group: " + LastSelected.Cast<Group>().DomNode.Type.Name );
                //}
                //else if (LastSelected.Is<Preset>())
                //{
                //    System.Diagnostics.Debug.WriteLine( "LastSelected: Preset: " + LastSelected.Cast<Preset>().Name );
                //}
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine( "LastSelected: null" );
                //}

                //Group group = LastSelected.As<Group>();
                //if ( group != null )
                //{
                //    if ( group.Presets.Count > 0 )
                //        return new object[] { group.Presets[0] };
                //}
                //else
                //{
                    return new object[] { LastSelected };
                //}

                //return null;
            }
		}

		/// <summary>
		/// Gets an enumeration of the property descriptors for the items</summary>
		public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
		{
			//get { return GetPropertyDescriptors(); }
			get { return PropertyUtils.GetSharedProperties( Items ); }
		}

		#endregion

        #region IObservableContext Members
        /// <summary>
        /// Event handler for node inserted in DomNode tree.</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;
        /// <summary>
        /// Event handler for node removed from DomNode tree.</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;
        /// <summary>
        /// Event handler for node changed in DomNode tree.</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;
        /// <summary>
        /// Event that is raised when the DomNode tree has been reloaded.</summary>
        public event EventHandler Reloaded;
        #endregion

        //#region IInstancingContext Members

        ///// <summary>
        ///// Returns whether the context can copy the selection</summary>
        ///// <returns>True iff the context can copy</returns>
        //public bool CanCopy()
        //{
        //    //return Selection.Any<UIObject>()
        //    //     || Selection.Any<Curve>();
        //    return Selection.Any<Preset>();
        //}

        ///// <summary>
        ///// Copies the selection. Returns a data object representing the copied items.</summary>
        ///// <returns>Data object representing the copied items; e.g., a
        ///// System.Windows.Forms.IDataObject object</returns>
        //public object Copy()
        //{
        //    // TODO: check if all selected nodes are from the same group
        //    //
        //    IEnumerable<Preset> presets = Selection.AsIEnumerable<Preset>();

        //    IEnumerable<DomNode> rootNodes = DomNode.GetRoots( presets.AsIEnumerable<DomNode>() );
            
        //    List<object> copies = new List<object>( DomNode.Copy( rootNodes ) );
        //    return new DataObject( copies.ToArray() );
        //}

        ///// <summary>
        ///// Returns whether the context can insert the data object</summary>
        ///// <param name="insertingObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        ///// <returns>True iff the context can insert the data object</returns>
        //public bool CanInsert( object insertingObject )
        //{
        //    IDataObject dataObject = (IDataObject)insertingObject;
        //    object[] items = dataObject.GetData( typeof( object[] ) ) as object[];
        //    if ( items == null || items.Length == 0 )
        //        return false;

        //    IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
        //    DomNode parent = m_insertionParent.As<DomNode>();
        //    if ( m_insertionParent.Is<Preset>() )
        //        parent = parent.Parent;

        //    if ( parent != null )
        //    {
        //        foreach ( DomNode child in childNodes )
        //        {
        //            //// can't add child to parent if it will cause a cycle
        //            //foreach ( DomNode ancestor in parent.Lineage )
        //            //    if ( ancestor == child )
        //            //        return false;

        //            // don't add child to the same parent
        //            if ( parent == child.Parent )
        //                return false;

        //            // make sure parent can hold child of this type
        //            if ( !CanParent( parent, child.Type ) )
        //                return false;
        //        }

        //        return true;
        //    }
        //    //else
        //    //{
        //    //    EmptyRef emptyRef = m_insertionParent as EmptyRef;
        //    //    if ( emptyRef != null )
        //    //    {
        //    //        foreach ( DomNode child in childNodes )
        //    //            if ( !CanReference( emptyRef, child.Type ) )
        //    //                return false;

        //    //        return true;
        //    //    }
        //    //}

        //    return false;
        //}

        ///// <summary>
        ///// Inserts new object of given type using a transaction. Called by automated scripts during testing.</summary>
        ///// <typeparam name="T">Type of object to insert</typeparam>
        ///// <param name="insertingObject">DomNode that contains inserted object</param>
        ///// <param name="insertionParent">Parent where object is inserted</param>
        ///// <returns>Inserted object</returns>
        //public T Insert<T>( DomNode insertingObject, DomNode insertionParent ) where T : class
        //{
        //    //SetInsertionParent( insertionParent );
        //    //insertingObject.SetAttribute( UISchema.UIType.nameAttribute, typeof( T ).Name );
        //    //DataObject dataObject = new DataObject( new object[] { insertingObject } );

        //    //ITransactionContext transactionContext = this.As<ITransactionContext>();
        //    //transactionContext.DoTransaction(
        //    //    delegate
        //    //    {
        //    //        Insert( dataObject );
        //    //    }, "Scripted Insert Object" );

        //    //T newItem = null;
        //    //ChildInfo childInfo = GetChildInfo( insertionParent, insertingObject.Type );
        //    //if ( childInfo != null )
        //    //{
        //    //    if ( childInfo.IsList )
        //    //    {
        //    //        IList<DomNode> list = insertionParent.GetChildList( childInfo );
        //    //        //This assumes the new object is always appended at the end of the list
        //    //        DomNode newNode = list[list.Count - 1];
        //    //        newItem = newNode.As<T>();
        //    //    }
        //    //    else
        //    //    {
        //    //        DomNode newNode = insertionParent.GetChild( childInfo );
        //    //        newItem = newNode.As<T>();
        //    //    }
        //    //}

        //    //return newItem;
        //    return null;
        //}

        ///// <summary>
        ///// Inserts the data object into the context</summary>
        ///// <param name="insertingObject">Data to insert</param>
        //public void Insert( object insertingObject )
        //{
        //    IDataObject dataObject = (IDataObject)insertingObject;
        //    object[] items = dataObject.GetData( typeof( object[] ) ) as object[];
        //    if ( items == null || items.Length == 0 )
        //        return;

        //    IEnumerable<DomNode> childNodes = items.AsIEnumerable<DomNode>();
        //    // if no items are parented, then we should clone the items, which must be from the clipboard
        //    bool fromScrap = true;
        //    foreach ( DomNode child in childNodes )
        //    {
        //        if ( child.Parent != null )
        //        {
        //            fromScrap = false;
        //            break;
        //        }
        //    }
        //    if ( fromScrap )
        //    {
        //        childNodes = DomNode.Copy( childNodes );
        //        // init extensions for copied DomNodes
        //        foreach ( DomNode child in childNodes )
        //            child.InitializeExtensions();
        //    }

        //    DomNode parent = m_insertionParent.As<DomNode>();
        //    if ( m_insertionParent.Is<Preset>() )
        //       parent = parent.Parent;

        //    if ( parent != null )
        //    {
        //        ITransactionContext transactionContext = this.Cast<ITransactionContext>();
        //        transactionContext.DoTransaction(
        //            delegate
        //            {
        //                foreach ( DomNode child in childNodes )
        //                {
        //                    ChildInfo childInfo = GetChildInfo( parent, child.Type );
        //                    if ( childInfo != null )
        //                    {
        //                        if ( childInfo.IsList )
        //                        {
        //                            IList<DomNode> list = parent.GetChildList( childInfo );
        //                            list.Add( child );
        //                        }
        //                        else
        //                        {
        //                            parent.SetChild( childInfo, child );
        //                        }
        //                    }
        //                }
        //            }, "Insert objects" );
        //    }
        //    //else
        //    //{
        //    //    EmptyRef emptyRef = m_insertionParent as EmptyRef;
        //    //    if ( emptyRef != null )
        //    //    {
        //    //        foreach ( DomNode child in childNodes )
        //    //        {
        //    //            UIRef uiRef = UIRef.New( child.As<UIObject>() );
        //    //            emptyRef.Parent.SetChild( emptyRef.ChildInfo, uiRef.DomNode );
        //    //        }
        //    //    }
        //    //}
        //    }

        ///// <summary>
        ///// Tests if can delete selected items</summary>
        ///// <returns>True iff can delete selected items</returns>
        //public bool CanDelete()
        //{
        //    //return Selection.Any<DomNode>(); // 
        //    if ( Selection.All<Preset>() )
        //    {
        //        foreach( DomNode node in Selection.AsIEnumerable<DomNode>() )
        //        {
        //            if ( !node.Parent.Is<Group>() )
        //                return false;

        //            Group group = node.Parent.Cast<Group>();
        //            if ( group.Presets.Count == 1 )
        //                return false;
        //        }
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Deletes selected items</summary>
        //public void Delete()
        //{
        //    IEnumerable<DomNode> rootNodes = DomNode.GetRoots( Selection.AsIEnumerable<DomNode>() );
        //    foreach ( DomNode node in rootNodes )
        //        if ( node.Parent != null )
        //            node.RemoveFromParent();

        //    Selection.Clear();
        //}

        ///// <summary>
        ///// Use DOM type metadata to determine if we can parent a child type to a parent</summary>
        ///// <param name="parent">Parent</param>
        ///// <param name="childType">Child type</param>
        ///// <returns></returns>
        //private bool CanParent( DomNode parent, DomNodeType childType )
        //{
        //    return GetChildInfo( parent, childType ) != null;
        //}

        //// use Dom type metadata to get matching child metadata
        //private ChildInfo GetChildInfo( DomNode parent, DomNodeType childType )
        //{
        //    foreach ( ChildInfo childInfo in parent.Type.Children )
        //        if ( childInfo.Type.IsAssignableFrom( childType ) )
        //            return childInfo;
        //    return null;
        //}

        ///// <summary>
        ///// Sets the insertion point as the user clicks and drags over the TreeControl. 
        ///// The insertion point determines where paste and drag and drop operations insert new objects into the UI data.</summary>
        ///// <param name="insertionParent">Parent where object is inserted</param>
        //public void SetInsertionParent( object insertionParent )
        //{
        //    m_insertionParent = insertionParent;
        //}

        //private object m_insertionParent;

        //#endregion

        #region TransactionContext Members
        /// <summary>
        /// Performs custom actions after a transaction ends</summary>
        protected override void OnEnded()
		{
			base.OnEnded();

            if ( m_saveFileTimer_.Enabled )
                m_saveFileTimer_.Stop();

            m_saveFileTimer_.Start();
            //// the caveat here is that when user starts using sliders, this callback will get called
            //// many times, every time slider value changes
            //// ideally, there should be some mechanism of "merging" operations, this could be implemented similar to
            //// what's in HistoryContext
            ////
            //Document document = this.Cast<Document>();
            //document.SaveImpl();
		}

        #endregion

        void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
        {
            ItemChanged.Raise( this, new ItemChangedEventArgs<object>( e.DomNode ) );

            Document document = this.Cast<Document>();
            //Group group = e.DomNode.As<Group>();
            //if ( group != null )
            //{
            //    if ( e.AttributeInfo.Equivalent( document.Schema.groupType.expandedAttribute ) )
            //        return;
            //}

            //Preset preset = e.DomNode.As<Preset>();
            //if ( preset != null )
            //    group = preset.Group;

            Struct structure = e.DomNode.As<Struct>();
            if ( structure == null )
                return;

            if ( e.AttributeInfo.Equivalent( document.Schema.structType.expandedAttribute ) )
                return;

            if (e.NewValue is string)
            {
                if (e.AttributeInfo.Name == "shaderOutputFile")
                {
                    SettingsCompiler compiler = new SettingsCompiler();
                    string descFullPath = document.DescFilePath;
                    compiler.ReflectSettings( descFullPath );
                    compiler.GenerateHeaderIfChanged( descFullPath, Globals.GetCodeFullPath( (string) e.NewValue ) );
                    return;
                }
            }

            SettingGroup settingGroup = e.DomNode.Type.GetTag<SettingGroup>();
            string structureName = settingGroup.DomNodeTypeFullName;
            string[] namespaceAndName = structureName.Split( ':' );
            structureName = namespaceAndName[1];

            ZMQHubMessage msg = new ZMQHubMessage( "settings" );

            if ( e.AttributeInfo.Type.Equals( AttributeType.BooleanType ) )
            {
                msg.appendString( "setInt" );
                msg.appendString( document.PathRelativeToData );
                //msg.appendString( group.Name );
                //msg.appendString( preset.Name );
                msg.appendString( structureName );
                msg.appendString( "" );
                msg.appendString( e.AttributeInfo.Name );
                msg.appendInt( 1 );
                bool bval = (bool)e.NewValue;
                msg.appendInt( bval ? 1 : 0 );
            }
            else if ( e.AttributeInfo.Type.Equals( AttributeType.IntType ) )
            {
                msg.appendString( "setInt" );
                msg.appendString( document.PathRelativeToData );
                //msg.appendString( group.Name );
                //msg.appendString( preset.Name );
                msg.appendString( structureName );
                msg.appendString( "" );
                msg.appendString( e.AttributeInfo.Name );
                msg.appendInt( 1 );
                msg.appendInt( (int)e.NewValue );
            }
            else if ( e.AttributeInfo.Type.Equals( Schema.FlexibleFloatType ) )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                //msg.appendString( group.Name );
                //msg.appendString( preset.Name );
                msg.appendString( structureName );
                msg.appendString( "" );
                msg.appendString( e.AttributeInfo.Name );
                msg.appendInt( 2 );
                float[] farray = (float[])e.NewValue;
                msg.appendFloat( farray[0] );
                msg.appendFloat( farray[4] );
            }
            else if ( e.AttributeInfo.Type.Equals( AttributeType.StringType ) )
            {
                msg.appendString( "setString" );
                msg.appendString( document.PathRelativeToData );
                //msg.appendString( group.Name );
                //msg.appendString( preset.Name );
                msg.appendString( structureName );
                msg.appendString( "" );
                msg.appendString( e.AttributeInfo.Name );
                msg.appendString( (string)e.NewValue );
            }
            else if ( e.AttributeInfo.Type.Equals( Schema.Float4Type ) )
            {
                msg.appendString( "setFloat" );
                msg.appendString( document.PathRelativeToData );
                //msg.appendString( group.Name );
                //msg.appendString( preset.Name );
                msg.appendString( structureName );
                msg.appendString( "" );
                msg.appendString( e.AttributeInfo.Name );
                float[] farray = (float[])e.NewValue;
                msg.appendInt( farray.Length );
                foreach ( float f in farray )
                    msg.appendFloat( f );
            }
            else
            {
                Outputs.WriteLine( OutputMessageType.Error, "Unsupported attribute type!" );
                return;
            }

            ZMQHubService.send( msg );

            //pico.Hub.HubMessage msg = new pico.Hub.HubMessage( "settings" );

            //if (e.AttributeInfo.Type.Equals( AttributeType.BooleanType ))
            //{
            //    msg.appendString( "setInt" );
            //    msg.appendString( document.PathRelativeToData );
            //    //msg.appendString( group.Name );
            //    //msg.appendString( preset.Name );
            //    msg.appendString( structureName );
            //    msg.appendString( "" );
            //    msg.appendString( e.AttributeInfo.Name );
            //    msg.appendInt( 1 );
            //    bool bval = (bool) e.NewValue;
            //    msg.appendInt( bval ? 1 : 0 );
            //}
            //else if (e.AttributeInfo.Type.Equals( AttributeType.IntType ))
            //{
            //    msg.appendString( "setInt" );
            //    msg.appendString( document.PathRelativeToData );
            //    //msg.appendString( group.Name );
            //    //msg.appendString( preset.Name );
            //    msg.appendString( structureName );
            //    msg.appendString( "" );
            //    msg.appendString( e.AttributeInfo.Name );
            //    msg.appendInt( 1 );
            //    msg.appendInt( (int) e.NewValue );
            //}
            //else if (e.AttributeInfo.Type.Equals( Schema.FlexibleFloatType ))
            //{
            //    msg.appendString( "setFloat" );
            //    msg.appendString( document.PathRelativeToData );
            //    //msg.appendString( group.Name );
            //    //msg.appendString( preset.Name );
            //    msg.appendString( structureName );
            //    msg.appendString( "" );
            //    msg.appendString( e.AttributeInfo.Name );
            //    msg.appendInt( 2 );
            //    float[] farray = (float[]) e.NewValue;
            //    msg.appendFloat( farray[0] );
            //    msg.appendFloat( farray[4] );
            //}
            //else if (e.AttributeInfo.Type.Equals( AttributeType.StringType ))
            //{
            //    msg.appendString( "setString" );
            //    msg.appendString( document.PathRelativeToData );
            //    //msg.appendString( group.Name );
            //    //msg.appendString( preset.Name );
            //    msg.appendString( structureName );
            //    msg.appendString( "" );
            //    msg.appendString( e.AttributeInfo.Name );
            //    msg.appendString( (string) e.NewValue );
            //}
            //else if (e.AttributeInfo.Type.Equals( Schema.Float4Type ))
            //{
            //    msg.appendString( "setFloat" );
            //    msg.appendString( document.PathRelativeToData );
            //    //msg.appendString( group.Name );
            //    //msg.appendString( preset.Name );
            //    msg.appendString( structureName );
            //    msg.appendString( "" );
            //    msg.appendString( e.AttributeInfo.Name );
            //    float[] farray = (float[]) e.NewValue;
            //    msg.appendInt( farray.Length );
            //    foreach (float f in farray)
            //        msg.appendFloat( f );
            //}
            //else
            //{
            //    Outputs.WriteLine( OutputMessageType.Error, "Unsupported attribute type!" );
            //    return;
            //}

            //pico.Hub.HubService.sendS( msg );
        }

		void DomNode_ChildInserted( object sender, ChildEventArgs e )
		{
			ItemInserted.Raise( this, new ItemInsertedEventArgs<object>( e.Index, e.Child, e.Parent ) );
		}

        // timer used to defer saving settings to file, usefull for reducing save operations while user is draging sliders
        //
        System.Windows.Forms.Timer m_saveFileTimer_ = new System.Windows.Forms.Timer();
    }
}
