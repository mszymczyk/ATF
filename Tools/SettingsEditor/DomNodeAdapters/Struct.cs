using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts DomNode to a Struct
    /// Name 'Struct' is unfortunate but it relates directly to c++ struct that's being generated for this node.
    /// </summary>
    public class Struct : DomNodeAdapter
    {
        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            m_schema = DomNode.Type.GetTag<Schema>();
            // DomNode.Type.Name is in form: SettingsEditor:PresetTypeName
            //
            //string[] s = DomNode.Type.Name.Split( ':' );
            //m_name = s[1];
            SettingGroup s = DomNode.Type.GetTag<SettingGroup>();
            m_name = s.Name;
            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the Name of group
        /// Group's name is name of the Preset + 'Group' suffix
        /// </summary>
        public string Name { get { return m_name; } }

        ///// <summary>
        ///// Gets and sets the selected preset</summary>
        //public Preset SelectedPreset
        //{
        //    get
        //    {
        //        //Preset selectedPreset = Presets.First( p => p.Name == SelectedPresetName );
        //        //return selectedPreset;
        //        if (m_selectedPreset == null || m_selectedPreset.Name != SelectedPresetName)
        //            m_selectedPreset = Presets.First( p => p.Name == SelectedPresetName );

        //        return m_selectedPreset;
        //    }
        //    set
        //    {
        //        SelectedPresetName = value.Name;
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the referenced UI object</summary>
        //public Preset SelectedPresetRef
        //{
        //    get { return GetReference<Preset>( m_schema.groupType.selectedPresetRefAttribute ); }
        //    set { SetReference( m_schema.groupType.selectedPresetRefAttribute, value ); }
        //}

        ///// <summary>
        ///// Gets and sets the selected preset</summary>
        //public string SelectedPresetName
        //{
        //    get { return (string) DomNode.GetAttribute( m_schema.groupType.selectedPresetAttribute ); }
        //    set { DomNode.SetAttribute( m_schema.groupType.selectedPresetAttribute, value ); }
        //}

        /// <summary>
        /// Gets and sets expanded property
        /// Expanded is used to remember state of the tree.
        /// This aids editing because tree is not collapsed between document reloads.
        /// </summary>
        public bool Expanded
        {
            get { return (bool)DomNode.GetAttribute( m_schema.structType.expandedAttribute ); }
            set { DomNode.SetAttribute( m_schema.structType.expandedAttribute, value ); }
        }

        /// <summary>
        /// Gets the list of all tracks in the group</summary>
        public IList<Struct> NestedStructures
        {
            get { return GetChildList<Struct>( m_schema.structType.structChild ); }
        }

        private Schema m_schema;
        private string m_name;
        //private Preset m_selectedPreset;
    }
}



