using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pico.LogOutput
{
	public partial class picoLogOutputForm3 : UserControl
	{
		public picoLogOutputForm3()
		{
			InitializeComponent();

			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
			dataGridView1.MouseClick += dataGridView1_MouseClick;
			dataGridView1.KeyUp += dataGridView1_KeyUp;

			filterTextBox.TextChanged += filterTextBox_TextChanged;

			checkBoxErrors.CheckedChanged += checkBox_CheckedChanged;
			checkBoxWarnings.CheckedChanged += checkBox_CheckedChanged;
			checkBoxInfos.CheckedChanged += checkBox_CheckedChanged;

			m_logDataTable = new picoLogDataTable();
			m_dt = m_logDataTable.Data;
			m_dv = m_logDataTable.DataView;

			dataGridView1.DataSource = m_dv;

			updateCheckBoxes();
			updateRowFilter();
		}

		public void setup( Icons icons )
		{
			m_icons = icons;
		}

		public void clearLog()
		{
			m_logDataTable.Clear();
			updateCheckBoxes();
			updateRowFilter();
		}

		public void addDataItem( DataItem dataItem )
		{
			m_logDataTable.AddItem( dataItem );

			if ( dataGridView1.SelectedRows.Count == 0 )
				dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;

			updateCheckBoxes();
			updateRowFilter();
		}

		void checkBox_CheckedChanged( object sender, EventArgs e )
		{
			updateRowFilter();
		}

		void filterTextBox_TextChanged( object sender, EventArgs e )
		{
			updateRowFilter();
		}

		private void buttonClear_Click( object sender, EventArgs e )
		{
			clearLog();
		}

		void dataGridView1_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
		{
			if ( e.ColumnIndex == 0 )
			{
				if ( e.Value != null )
				{
					int ival = (int)e.Value;
					if (ival == DataItem.Type_Error)
					{
						e.Value = m_icons.ErrorIcon;
					}
					else if (ival == DataItem.Type_Warning)
					{
						e.Value = m_icons.WarningIcon;
					}
					else if (ival == DataItem.Type_Info)
					{
						e.Value = m_icons.InfoIcon;
					}
				}
			}
			//else if ( e.ColumnIndex == 2 )
			//{
			//	var cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
			//	// Set the Cell's ToolTipText.
			//	cell.ToolTipText = cell.Value.ToString();
			//}
		}

		private void dataGridView1_MouseClick( object sender, MouseEventArgs e )
		{
			if (e.Button == MouseButtons.Right)
			{
				int currentMouseOverRow = dataGridView1.HitTest( e.X, e.Y ).RowIndex;
				if (currentMouseOverRow >= 0)
				{
					// check if right-clicked selected row
					//
					bool clickedOnSelectedRow = false;
					foreach (DataGridViewRow row in dataGridView1.SelectedRows)
					{
						if ( row.Index == currentMouseOverRow )
						{
							clickedOnSelectedRow = true;
							break;
						}
					}

					if (!clickedOnSelectedRow && Control.ModifierKeys == Keys.Control)
					{
						// clicked new row, but control was pressed
						// add this row to selection
						//
						DataGridViewRow row = dataGridView1.Rows[currentMouseOverRow];
						row.Selected = true;
						clickedOnSelectedRow = true;
					}

					if (clickedOnSelectedRow)
					{
					}
					else
					{
						// clicked on non-selected row and Control key wasn't pressed
						// clear selection and select row that was right-clicked
						//
						dataGridView1.ClearSelection();
						DataGridViewRow row = dataGridView1.Rows[currentMouseOverRow];
						row.Selected = true;
					}

					ContextMenu m = new ContextMenu();

					MenuItem copyDescription;
					if (dataGridView1.SelectedRows.Count > 1)
						copyDescription = new MenuItem( "Copy Descriptions" );
					else
						copyDescription = new MenuItem( "Copy Description" );
					copyDescription.Click += copyDescription_Click;


					MenuItem copyTag;
					if (dataGridView1.SelectedRows.Count > 1)
						copyTag = new MenuItem( "Copy Tags" );
					else
						copyTag = new MenuItem( "Copy Tag" );
					copyTag.Click += copyTag_Click;


					MenuItem copyFileLine;
					if (dataGridView1.SelectedRows.Count > 1)
						copyFileLine = new MenuItem( "Copy Files and Lines" );
					else
						copyFileLine = new MenuItem( "Copy File and Line" );
					copyFileLine.Click += copyFileAndLine_Click;


					MenuItem copyRowAsCSV;
					if (dataGridView1.SelectedRows.Count > 1)
						copyRowAsCSV = new MenuItem( "Copy Rows as CSV" );
					else
						copyRowAsCSV = new MenuItem( "Copy Row as CSV" );
					copyRowAsCSV.Click += copyRowAsCSV_Click;

					m.MenuItems.Add( copyDescription );
					m.MenuItems.Add( copyTag );
					m.MenuItems.Add( copyFileLine );
					m.MenuItems.Add( copyRowAsCSV );
					//m.MenuItems.Add( new MenuItem( "Paste" ) );
					//m.MenuItems.Add( new MenuItem( string.Format( "Do something to row {0}", currentMouseOverRow.ToString() ) ) );

					m.Show( dataGridView1, new Point( e.X, e.Y ) );
				}
			}
		}

		void dataGridView1_KeyUp( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode == Keys.End && e.Modifiers == Keys.Control )
			{
				dataGridView1.ClearSelection();
				dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
			}
		}

		//void copyDescription_Click( object sender, EventArgs e )
		//{
		//	StringBuilder sb = new StringBuilder();
		//	foreach( DataGridViewRow row in dataGridView1.SelectedRows )
		//	{
		//		string col0 = row.Cells[0].Value.ToString();
		//		string col1 = row.Cells[1].Value.ToString();
		//		sb.Append( col0 );
		//		sb.Append( ',' );
		//		sb.AppendLine( col1 );
		//	}

		//	string str = sb.ToString();

		//	System.Windows.Forms.Clipboard.SetText( str );
		//}

		void copyDescription_Click( object sender, EventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach (DataGridViewRow row in dataGridView1.SelectedRows)
			{
				sb.AppendLine( row.Cells[2].Value.ToString() );
			}

			string str = sb.ToString();

			System.Windows.Forms.Clipboard.SetText( str );
		}

		void copyTag_Click( object sender, EventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach (DataGridViewRow row in dataGridView1.SelectedRows)
			{
				sb.AppendLine( row.Cells[3].Value.ToString() );
			}

			string str = sb.ToString();

			System.Windows.Forms.Clipboard.SetText( str );
		}

		void copyFileAndLine_Click( object sender, EventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach (DataGridViewRow row in dataGridView1.SelectedRows)
			{
				sb.Append( row.Cells[4].Value.ToString() );
				int lineNo = (int)row.Cells[5].Value;
				sb.Append( '(' );
				sb.Append( lineNo.ToString() );
				sb.Append( ')' );
				sb.AppendLine();
			}

			string str = sb.ToString();

			System.Windows.Forms.Clipboard.SetText( str );
		}

		void copyRowAsCSV_Click( object sender, EventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach (DataGridViewRow row in dataGridView1.SelectedRows)
			{
				int type = (int)row.Cells[0].Value;
				string description = row.Cells[2].Value.ToString();
				string tag = row.Cells[3].Value.ToString();
				string file = row.Cells[4].Value.ToString();
				int line = (int) row.Cells[5].Value;

				if (type == DataItem.Type_Error)
					sb.Append( "Error" );
				else if (type == DataItem.Type_Warning)
					sb.Append( "Warning" );
				else if (type == DataItem.Type_Info)
					sb.Append( "Info" );

				sb.Append( ",\"" );
				sb.Append( description );
				sb.Append( "\",\"" );
				sb.Append( tag );
				sb.Append( "\",\"" );
				sb.Append( file );
				sb.Append( "(" );
				sb.Append( line.ToString() );
				sb.Append( ")\"" );
	
				sb.AppendLine();
			}

			string str = sb.ToString();

			System.Windows.Forms.Clipboard.SetText( str );
		}

		public static string EscapeLikeValue( string valueWithoutWildcards )
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < valueWithoutWildcards.Length; i++)
			{
				char c = valueWithoutWildcards[i];
				if (c == '*' || c == '%' || c == '[' || c == ']')
					sb.Append( "[" ).Append( c ).Append( "]" );
				else if (c == '\'')
					sb.Append( "''" );
				else
					sb.Append( c );
			}
			return sb.ToString();
		}

		private void updateCheckBoxes()
		{
			if (m_logDataTable.NumErrors != 1)
				checkBoxErrors.Text = string.Format( "{0} Errors", m_logDataTable.NumErrors );
			else
				checkBoxErrors.Text = "1 Error";

			if (m_logDataTable.NumWarnings != 1)
				checkBoxWarnings.Text = string.Format( "{0} Warnings", m_logDataTable.NumWarnings );
			else
				checkBoxWarnings.Text = "1 Warning";

			if (m_logDataTable.NumInfos != 1)
				checkBoxInfos.Text = string.Format( "{0} Messages", m_logDataTable.NumInfos );
			else
				checkBoxInfos.Text = "1 Message";
		}

		private void updateRowFilter()
		{
			string rowFilterCheckBoxesPart = "(";
			if (checkBoxErrors.Checked)
			{
				rowFilterCheckBoxesPart += "(Type = 0)";
			}

			if (checkBoxWarnings.Checked)
			{
				if (checkBoxErrors.Checked)
					rowFilterCheckBoxesPart += " OR (Type = 1)";
				else
					rowFilterCheckBoxesPart += "(Type = 1)";
			}

			if (checkBoxInfos.Checked)
			{
				if (checkBoxErrors.Checked || checkBoxWarnings.Checked)
					rowFilterCheckBoxesPart += " OR (Type = 2)";
				else
					rowFilterCheckBoxesPart += "(Type = 2)";
			}

			rowFilterCheckBoxesPart += ")";

			if ( rowFilterCheckBoxesPart.Length <= 2 )
			{
				rowFilterCheckBoxesPart = "(Type = 8)"; // non-existient one
			}

			string finalFilterValuee = null;

			string rowFilterTextBoxPart = string.Empty;
			string text = filterTextBox.Text;
			if (text.Length < 3)
			{
				//m_dataView.RowFilter = String.Empty;
				//return;
				finalFilterValuee = rowFilterCheckBoxesPart;
			}
			else
			{
				string userTextFixed = EscapeLikeValue( text );
				rowFilterTextBoxPart = string.Format( "(Description LIKE '*{0}*' OR Tag LIKE '*{0}*' OR File LIKE '*{0}*')", userTextFixed );

				finalFilterValuee = string.Format( "{0} AND {1}", rowFilterCheckBoxesPart, rowFilterTextBoxPart );
			}

			//string userTextFixed = EscapeLikeValue( text );
			//string filter = string.Format( "Name LIKE '*{0}*'", userTextFixed );
			//m_dataView.RowFilter = filter;
			m_dv.RowFilter = finalFilterValuee;
		}

		private picoLogDataTable m_logDataTable;
		private DataTable m_dt;
		private DataView m_dv;
		private Icons m_icons;

		private static readonly Random s_random = new Random();
		private const string Alphabet = "     ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		//private string m_rowFilterCheckBoxesPart;
	}
}
