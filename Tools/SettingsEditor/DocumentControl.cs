using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls;
using Sce.Atf.Applications;
using System.Drawing;
using Sce.Atf;

namespace SettingsEditor
{
    public class DocumentControl : UserControl
    {
        public DocumentControl( Editor editor )
        {
            m_editor = editor;

            //m_treeControl = new TreeControl()
            //{
            //    ImageList = ResourceUtil.GetImageList16(),
            //    StateImageList = ResourceUtil.GetImageList16()
            //    //LabelEditMode = TreeControl.LabelEditModes.EditOnF2
            //};

            //m_treeControl.Dock = DockStyle.Fill;
            //m_treeControl.ShowRoot = true;

            //m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            //TreeView treeView = RootNode.As<TreeView>();
            //treeView.Schema = schema;
            //m_treeControlAdapter.TreeView = treeView;

            ////foreach ( Struct group in RootNode.Children.AsIEnumerable<Struct>() )
            //foreach( Struct group in RootNode.Subtree.AsIEnumerable<Struct>() )
            //{
            //    if ( group.Expanded )
            //        m_treeControlAdapter.Expand( group );
            //}

            //m_treeControl.MouseUp += treeControl_MouseUp;
            ////m_treeControl.MouseDoubleClick += m_treeControl_MouseDoubleClick;
            //m_treeControl.NodeExpandedChanged += m_treeControl_NodeExpandedChanged;
            ////m_treeControl.NodeLabelEdited += m_treeControl_NodeLabelEdited;
            //m_treeControlAdapter.LastHitChanged += treeControlAdapter_LastHitChanged;

            m_filteredTreeControlEditor = new FilteredTreeControlEditor( editor.CommandService );
            m_filteredTreeControlEditor.Control.Dock = DockStyle.Fill;
            //m_filteredTreeControlEditor.TreeView = new FilteredTreeView( treeView, m_filteredTreeControlEditor.DefaultFilter );
            //m_filteredTreeControlEditor.TreeView = new FilteredTreeView( treeView, CustomFilter );
            m_filteredTreeControlEditor.TreeControl.NodeExpandedChanged += m_treeControl_NodeExpandedChanged;

            //foreach ( Struct group in RootNode.Subtree.AsIEnumerable<Struct>() )
            //{
            //    if ( group.Expanded )
            //        m_filteredTreeControlEditor.TreeControlAdapter.Expand( group );
            //}

            m_filteredTreeControlEditor.SearchInputUI.Updated += SearchInputUI_Updated;

            m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            m_propertyGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
            m_propertyGrid.Dock = DockStyle.Fill;
            //m_propertyGrid.Bind( root.As<DocumentEditingContext>() );

            m_splitContainer = new SplitContainer();
            m_splitContainer.Text = "Split Container";
            //m_splitContainer.Panel1.Controls.Add( m_treeControl );
            m_splitContainer.Panel1.Controls.Add( m_filteredTreeControlEditor.Control );
            m_splitContainer.Panel2.Controls.Add( m_propertyGrid );
            m_splitContainer.Dock = DockStyle.Fill;

            Dock = DockStyle.Fill;
            Controls.Add( m_splitContainer );
        }

        public void Setup( DomNode root, Schema schema )
        {
            RootNode = root;
            //Schema = schema;
            RootNode.AttributeChanged += RootNode_AttributeChanged;

            TreeView treeView = RootNode.As<TreeView>();

            m_filteredTreeControlEditor.TreeView = new FilteredTreeView( treeView, CustomFilter );

            foreach ( Struct group in RootNode.Subtree.AsIEnumerable<Struct>() )
            {
                if ( group.Expanded )
                    m_filteredTreeControlEditor.TreeControlAdapter.Expand( group );
            }

            m_propertyGrid.Bind( root.As<DocumentEditingContext>() );

        }

        private void SearchInputUI_Updated( object sender, System.EventArgs e )
        {
            // we're filling property editor's search bar while typing in tree's search bar
            // this is to further narrow search so user can quicker find what he's after
            // it's kind of hack, setting only m_propertyGrid.PropertyGridView.FilterPattern
            // will filter properly, but filling search bar is more intuitive
            // because user will see what's actually happening
            // to set this search text box we need to find it among other controls
            // PropertyGrid doesn't expose any API to do it directly
            //
            foreach ( ToolStripItem tsi in m_propertyGrid.ToolStrip.Items )
            {
                if ( tsi is ToolStripAutoFitTextBox )
                {
                    ToolStripAutoFitTextBox t = tsi as ToolStripAutoFitTextBox;
                    t.Text = m_filteredTreeControlEditor.SearchInputUI.SearchPattern;
                    m_propertyGrid.PropertyGridView.FilterPattern = m_filteredTreeControlEditor.SearchInputUI.SearchPattern;
                    break;
                }
            }
        }

        /// <summary>
        /// Callback to determine if an item in the tree is filtered in (return true) or out</summary>
        /// <param name="item">Item tested for filtering</param>
        /// <returns>True if filtered in, false if filtered out</returns>
        private bool CustomFilter( object item )
        {
            // filter first tries to match group name and if failed checks all settings
            //
            DomNode dn = item.As<DomNode>();
            SettingGroup group = dn.Type.GetTag<SettingGroup>();
            if ( group != null )
            {
                if ( m_filteredTreeControlEditor.SearchInputUI.IsNullOrEmpty()
                    || m_filteredTreeControlEditor.SearchInputUI.Matches( group.Name ) )
                {
                    return true;
                }

                foreach( Setting sett in group.Settings )
                {
                    if ( m_filteredTreeControlEditor.SearchInputUI.IsNullOrEmpty()
                        || m_filteredTreeControlEditor.SearchInputUI.Matches( sett.Name ) )
                    {
                        return true;
                    }
                }

                return false;
            }

            return true; // Don't filter anything if the context doesn't implement IItemView
        }

        private void RootNode_AttributeChanged( object sender, AttributeEventArgs e )
        {
            Document document = this.As<Document>();
            Struct group = e.DomNode.As<Struct>();
            if ( group != null && e.AttributeInfo.Equivalent( document.Schema.structType.expandedAttribute ) )
            {
                if ( group.Expanded )
                    //m_treeControlAdapter.Expand( group );
                    m_filteredTreeControlEditor.TreeControlAdapter.Expand( group );
                else
                    //m_treeControlAdapter.Collapse( group );
                    m_filteredTreeControlEditor.TreeControlAdapter.Collapse( group );
            }
        }

        private void m_treeControl_NodeExpandedChanged( object sender, TreeControl.NodeEventArgs e )
        {
            Struct group = e.Node.Tag.As<Struct>();
            if ( group != null && group.Expanded != e.Node.Expanded )
            {
                ITransactionContext transactionContext = RootNode.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        group.Expanded = e.Node.Expanded;
                    }, e.Node.Expanded ? "Struct expanded" : "Struct collapsed" );
            }
        }

        //void m_treeControl_NodeLabelEdited( object sender, TreeControl.NodeEventArgs e )
        //{
        //    Preset preset = e.Node.Tag.As<Preset>();
        //    Group group = preset.Group;

        //    ITransactionContext transactionContext = RootNode.As<ITransactionContext>();
        //    transactionContext.DoTransaction(
        //        delegate
        //        {
        //            preset.Name = e.Node.Label;
        //            //if (group.SelectedPreset == preset)
        //            //    group.SelectedPreset = preset;
        //        }, string.Format( "Preset name: '{0}' -> '{1}'", preset.Name, e.Node.Label ) );
        //}

        private void treeControlAdapter_LastHitChanged( object sender, System.EventArgs e )
        {
            //DocumentEditingContext editingContext = RootNode.Cast<DocumentEditingContext>();
            //editingContext.SetInsertionParent( m_treeControlAdapter.LastHit );
        }

        //private void treeControl_MouseUp( object sender, MouseEventArgs e )
        //{
        //    if ( e.Button == MouseButtons.Right )
        //    {
        //        IEnumerable<object> commands =
        //            m_editor.ContextMenuCommandProviders.GetCommands( m_treeControlAdapter.TreeView, m_treeControlAdapter.LastHit );

        //        Point screenPoint = m_treeControl.PointToScreen( new Point( e.X, e.Y ) );
        //        m_editor.CommandService.RunContextMenu( commands, screenPoint );
        //    }
        //}

        //void m_treeControl_MouseDoubleClick( object sender, MouseEventArgs e )
        //{
        //    Preset preset = m_treeControlAdapter.LastHit.As<Preset>();
        //    if (preset != null)
        //    {
        //        Group group = preset.Group;
        //        //Preset prevPreset = group.SelectedPreset;
        //        Preset prevPreset = group.SelectedPresetRef;
        //        if (prevPreset != preset)
        //        {
        //            ITransactionContext transactionContext = RootNode.As<ITransactionContext>();
        //            transactionContext.DoTransaction(
        //                delegate
        //                {
        //                    //group.SelectedPreset = preset;
        //                    group.SelectedPresetRef = preset;
        //                }, "Active preset: " + ((prevPreset != null) ? prevPreset.Name : "null") + " -> " + preset.Name );

        //            //// refreshing group causes selection to change and effectively clears it leaving property editor empty
        //            //// 
        //            //m_treeControlAdapter.Refresh( prevPreset );
        //            //m_treeControlAdapter.Refresh( preset );
        //        }
        //    }
        //}

        public void Rebind()
        {
            System.Diagnostics.Debug.WriteLine( "DocumentControl.Rebind" );
            m_propertyGrid.Bind( null );
            m_propertyGrid.Bind( RootNode.As<DocumentEditingContext>() );
        }

        /// <summary>
        /// Selects DomNode in tree control. This is really hacky way to do it.
        /// We need to remap DomNode to TreeControl.Node, don't know any better way to do it.
        /// </summary>
        /// <param name="domNode">Group or Preset to select</param>
        public void SetSelectedDomNode( DomNode domNode )
        {
            //var paths = m_treeControlAdapter.GetPaths( domNode );
            var paths = m_filteredTreeControlEditor.TreeControlAdapter.GetPaths( domNode );
            var list = paths.ToList();
            if ( list.Count > 0 )
            {
                //TreeControl.Node n = m_treeControlAdapter.ExpandPath( list[0] );
                TreeControl.Node n = m_filteredTreeControlEditor.TreeControlAdapter.ExpandPath( list[0] );
                //m_treeControl.SetSelection( n );
                m_filteredTreeControlEditor.TreeControl.SetSelection( n );
            }
        }


        public DomNode RootNode { get; set; }
        //public Schema Schema { get; set; }
        public ControlInfo ControlInfo { get; set; }

        //private TreeControl m_treeControl;
        //private TreeControlAdapter m_treeControlAdapter;
        private FilteredTreeControlEditor m_filteredTreeControlEditor;

        private readonly SplitContainer m_splitContainer;
		private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

        private Editor m_editor;
    }
}
