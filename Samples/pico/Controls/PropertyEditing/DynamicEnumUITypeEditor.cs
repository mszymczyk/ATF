﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.Controls.PropertyEditing
{
	public interface DynamicEnumUITypeEditorLister
	{
		string[] GetNames( object instance );
	}

    /// <summary>
    /// UITypeEditor to handle dynamic "enumerations", where string display names are associated
    /// with integer values. For a TypeConverter, consider the EnumTypeConverter or the
    /// ExclusiveEnumTypeConverter.</summary>
    public class DynamicEnumUITypeEditor : UITypeEditor
    {
        /// <summary>
        /// Default constructor</summary>
        public DynamicEnumUITypeEditor( DynamicEnumUITypeEditorLister lister )
        {
			m_lister = lister;
        }

		private int m_maxDropDownItems = 6;
		/// <summary>
		/// Gets or sets the maximum number of items to be shown in the drop-down portion
		/// of the EnumEditor. Default is 6</summary>
		public int MaxDropDownItems
		{
			get { return m_maxDropDownItems; }
			set
			{
				m_maxDropDownItems = value;
				if ( m_maxDropDownItems < 2 )
					m_maxDropDownItems = 2;
			}
		}

		///// <summary>
		///// Constructor with enum names</summary>
		///// <param name="names">Enum names array</param>
		///// <remarks>Enum values default to successive powers of 2, starting with 1</remarks>
		//public DynamicEnumUITypeEditor(string[] names)
		//{
		//	DefineEnum(names);
		//}

		///// <summary>
		///// Constructor with enum names and values</summary>
		///// <param name="names">Enum names array</param>
		///// <param name="values">Enum values array</param>
		//public DynamicEnumUITypeEditor(string[] names, int[] values)
		//{
		//	DefineEnum(names, values);
		//}

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names array</param>
        /// <remarks>Enum values default to successive integers, starting with 0. Enum names
        /// with the format "EnumName=X" are parsed so that EnumName gets the value X, where X is
        /// an int.</remarks>
        private void DefineEnum(string[] names)
        {
            EnumUtil.ParseEnumDefinitions(names, out m_names, out m_values);
        }

        /// <summary>
        /// Defines the enum names and values</summary>
        /// <param name="names">Enum names array</param>
        /// <param name="values">Enum values array</param>
        private void DefineEnum(string[] names, int[] values)
        {
            if (names == null || values == null || names.Length != values.Length)
                throw new ArgumentException("names and/or values null, or of unequal length");

            m_names = names;
            m_values = values;
        }

        /// <summary>
        /// Gets the editor style used by the System.Drawing.Design.UITypeEditor.EditValue(
        /// System.IServiceProvider,System.Object) method</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
        /// additional context information</param>
        /// <returns>A System.Drawing.Design.UITypeEditorEditStyle value that indicates the style
        /// of editor used by the System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)
        /// method. If the System.Drawing.Design.UITypeEditor does not support this method,
        /// System.Drawing.Design.UITypeEditor.GetEditStyle() returns System.Drawing.Design.UITypeEditorEditStyle.None.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Edits the specified object's value using the editor style indicated by the
        /// System.Drawing.Design.UITypeEditor.GetEditStyle() method</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
        /// additional context information</param>
        /// <param name="provider">An System.IServiceProvider that this editor can use to obtain services</param>
        /// <param name="value">The object to edit</param>
        /// <returns>The new value of the object. If the value of the object has not changed, this should
        /// return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            m_editorService =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (m_editorService != null)
            {
				string[] names = m_lister.GetNames( context.Instance );
				DefineEnum( names );

				ListBox listBox = new ListBox();

				// the problem with this control is that it flickers when mouse is hovering over it
				// tried following solution, but it didn't work, will get back to it later
				//
				//// set double buffering
				//// see for more info http://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form
				////
				////Taxes: Remote Desktop Connection and painting
				////http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
				//if ( !System.Windows.Forms.SystemInformation.TerminalServerSession )
				//{
				//	System.Reflection.PropertyInfo aProp = typeof( System.Windows.Forms.Control ).GetProperty( "DoubleBuffered",
				//				System.Reflection.BindingFlags.NonPublic | 
				//				System.Reflection.BindingFlags.Instance );

				//	aProp.SetValue( listBox, true, null );
				//}

				listBox.DrawMode = DrawMode.OwnerDrawFixed;
				listBox.DrawItem += listBox_DrawItem;
				listBox.MouseMove += listBox_MouseMove;

				for ( int i = 0; i < m_names.Length; i++ )
				{
					listBox.Items.Add( m_names[i] );
					if ( m_names[i].Equals( value ) ||
						m_values[i].Equals( value ) )
					{
						listBox.SelectedIndex = i;
					}
				}

				int nRows = Math.Min( m_names.Length, m_maxDropDownItems );
				// add some extra height because number of rows actually visible may be one less than requested, due to
				// listBox.IntegralHeight being set to true
				//
				listBox.Height = nRows * listBox.ItemHeight + listBox.ItemHeight / 2;

				// size control so all strings are completely visible
				using ( Graphics g = listBox.CreateGraphics() )
				{
					float width = 0f;

					foreach ( string name in m_names )
					{
						float w = g.MeasureString( name, listBox.Font ).Width;
						width = Math.Max( width, w );
					}

					float height = m_names.Length * listBox.ItemHeight;
					int scrollBarThickness = SystemInformation.VerticalScrollBarWidth;
					if ( height > listBox.Height - 4 ) // vertical scrollbar?
						width += SystemInformation.VerticalScrollBarWidth;

					if ( width > listBox.Width )
						listBox.Width = (int) width + 6;
				}

				listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
				listBox.MouseDown += listBox_OnMouseDown;
				listBox.MouseUp += listBox_OnMouseUp;
				listBox.MouseLeave += listBox_OnMouseLeave;
				listBox.PreviewKeyDown += listBox_OnPreviewKeyDown;

				m_editorService.DropDownControl( listBox );
				int index = listBox.SelectedIndex;
				if ( index >= 0 )
				{
					object newValue = null;
					if ( value is int )
						newValue = m_values[index];
					else
						newValue = m_names[index];
					// be careful to return the same object if the value didn't change
					if ( !newValue.Equals( value ) )
						value = newValue;
				}
            }

            return value;
        }

        /// <summary>
        /// Performs custom actions on ListBox SelectedIndexChanged events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_listBoxMouseDown)
            {
                m_listBoxMouseDown = false;
                m_editorService.CloseDropDown();
            }
        }

        private void listBox_OnMouseLeave(object sender, EventArgs e)
        {
            m_listBoxMouseDown = false;
        }

        private void listBox_OnMouseUp(object sender, MouseEventArgs e)
        {
            m_listBoxMouseDown = false;
        }

        private void listBox_OnMouseDown(object sender, MouseEventArgs e)
        {
            m_listBoxMouseDown = true;
        }

        private void listBox_OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Return ||
                e.KeyData == Keys.Tab ||
                e.KeyData == (Keys.Tab | Keys.Shift))
            {
                m_editorService.CloseDropDown();
            }
            else if (e.KeyData == Keys.Escape)
            {
                ((ListBox)sender).ClearSelected();
                m_editorService.CloseDropDown();
            }
        }

        /// <summary>
        /// Performs custom actions on ListBox MouseMove events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listBox_MouseMove(object sender, MouseEventArgs e)
        {

            ListBox listBox = sender as ListBox;
            if (e.Y <= m_names.Length * listBox.ItemHeight)
            {
                int hoverIndex = e.Y / listBox.ItemHeight + listBox.TopIndex;
                if (hoverIndex != m_hoverIndex)
                {
                    m_hoverIndex = hoverIndex;
                    listBox.Invalidate();
                }
            }
            else
                m_hoverIndex = -1;

        }

        /// <summary>
        /// Performs custom actions on ListBox DrawItem events</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            // Determine the forecolor based on whether or not the item is selected.
            Brush brush;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                brush = Brushes.White;
            }
            else
            {
                brush = Brushes.Black;
            }

            if (e.Index >= 0)
            {
                if (e.Index == m_hoverIndex && (e.State & DrawItemState.Selected) == 0)
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);

                // Get the item text.
                string text = m_names[e.Index];
                // Draw the item text.
                e.Graphics.DrawString(text, ((Control)sender).Font, brush, e.Bounds.X, e.Bounds.Y);
            }
        }

		private DynamicEnumUITypeEditorLister m_lister;
		private string[] m_names = EmptyArray<string>.Instance;
		private int[] m_values = EmptyArray<int>.Instance;
        private IWindowsFormsEditorService m_editorService;
        private int m_hoverIndex = -1;
        private bool m_listBoxMouseDown;
    }
}
