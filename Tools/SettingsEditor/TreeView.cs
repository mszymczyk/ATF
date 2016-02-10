using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts root DomNode to ITreeView, IItemView, and IObservableContext so it can be
    /// displayed by the TreeLister</summary>
    public class TreeView : DomNodeAdapter, ITreeView, IItemView, IObservableContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the tree view's DomNode</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += root_AttributeChanged;
            DomNode.ChildInserted += root_ChildInserted;
            DomNode.ChildRemoved += root_ChildRemoved;
            Reloaded.Raise( this, EventArgs.Empty );

            base.OnNodeSet();
        }

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when a tree item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event that is raised when a tree item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event that is raised when a tree item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event that is raised when the tree is reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

        #region ITreeView Members

        /// <summary>
        /// Gets the root object of the tree</summary>
        public object Root
        {
            get { return DomNode; }
        }

        /// <summary>
        /// Gets the children of the given parent object</summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            DomNode node = parent as DomNode;
            if (node != null)
            {
                // get child Dom nodes and empty reference "slots"
                foreach (ChildInfo childInfo in node.Type.Children)
                {
                    if (childInfo.IsList)
                    {
                        foreach (DomNode child in node.GetChildList(childInfo))
                            yield return child;
                    }
                    else
                    {
                        DomNode child = node.GetChild(childInfo);
                        if (child != null)
                        {
                            yield return child;
                        }
                        //else if (childInfo.Type == UISchema.UIRefType.Type)
                        //{
                        //    yield return new EmptyRef(node, childInfo);
                        //}
                    }
                }
            }
        }

        #endregion

        #region IItemView Members

        /// <summary>
        /// Gets item's display information</summary>
        /// <param name="item">Item being displayed</param>
        /// <param name="info">Item info, to fill out</param>
        public void GetInfo( object item, ItemInfo info )
        {
            DomNode node = item as DomNode;
            info.Label = node.Type.Name;
            info.AllowLabelEdit = false;

            Struct s = node.As<Struct>();
            if (s != null)
            {
                info.Label = s.Name;
                info.IsLeaf = s.NestedStructures.Count == 0;
            }

            //if (node.Type.BaseType == Schema.presetType.Type)
            //if ( node.Is<Preset>() )
            //{
            //    Preset preset = node.Cast<Preset>();
            //    info.IsLeaf = true;
            //    info.Label = preset.Name;
            //    info.AllowLabelEdit = true;
            //    Group group = node.Parent.Cast<Group>();
            //    //if ( group.SelectedPreset == preset )
            //    if ( group.SelectedPresetRef == preset )
            //        //info.FontStyle = System.Drawing.FontStyle.Bold;
            //        info.ImageIndex = info.GetImageList().Images.IndexOfKey( Resources.SelectedPreset );
            //}
            //else if ( node.Is<Group>() )
            //{
            //    Group group = node.Cast<Group>();
            //    SettingGroup settGroup = node.Type.GetTag<SettingGroup>();
            //    //IList<DomNode> presets = node.GetChildList(Schema.groupType.presetsChild);
            //    //info.IsLeaf = presets.Count == 0;
            //    info.IsLeaf = group.Presets.Count == 0 || !settGroup.HasPresets;
            //    info.Label = settGroup.Name;
            //}
            else if (node.Type == Root.As<Document>().Schema.settingsFileType.Type)
            {
                info.Label = "Settings";
            }
        }

        #endregion

        private void root_AttributeChanged( object sender, AttributeEventArgs e )
        {
            ItemChanged.Raise( this, new ItemChangedEventArgs<object>( e.DomNode ) );

            //// if it's selectedPreset attribute, refresh all children nodes of TreeControl
            ////
            //Group group = e.DomNode.As<Group>();
            //if ( group != null && e.AttributeInfo.Equivalent(Schema.groupType.selectedPresetRefAttribute) )
            //{
            //    foreach( Preset preset in group.Presets )
            //    {
            //        ItemChanged.Raise( this, new ItemChangedEventArgs<object>( preset.DomNode ) );
            //    }
            //}
        }

        private void root_ChildInserted( object sender, ChildEventArgs e )
        {
            ItemInserted.Raise( this, new ItemInsertedEventArgs<object>( -1, e.Child, e.Parent ) );
        }

        private void root_ChildRemoved( object sender, ChildEventArgs e )
        {
            ItemRemoved.Raise( this, new ItemRemovedEventArgs<object>( -1, e.Child, e.Parent ) );
        }

        private int GetChildIndex(object child, object parent)
        {
            // get child index by re-constructing what we'd give the tree control
            System.Collections.IEnumerable treeChildren = GetChildren(parent);
            int i = 0;
            foreach (object treeChild in treeChildren)
            {
                if (treeChild.Equals(child))
                    return i;
                i++;
            }
            return -1;
        }
    }
}
