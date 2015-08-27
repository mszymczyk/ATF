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

			m_errorIcon = SystemIcons.Error;
			m_warningIcon = SystemIcons.Warning;
			m_infoIcon = SystemIcons.Information;

			//m_errorImage = new Bitmap( SystemIcons.Error.ToBitmap(), 16, 16 );
			//m_warningImage = new Bitmap( SystemIcons.Warning.ToBitmap(), 16, 16 );
			//m_infoImage = new Bitmap( SystemIcons.Information.ToBitmap(), 16, 16 );
			//m_errorImage = new Bitmap( SystemIcons.Error.ToBitmap() );
			//m_warningImage = new Bitmap( SystemIcons.Warning.ToBitmap() );
			//m_infoImage = new Bitmap( SystemIcons.Information.ToBitmap() );
			m_errorImage = _ScaleIcon( SystemIcons.Error );
			m_warningImage = _ScaleIcon( SystemIcons.Warning );
			m_infoImage = _ScaleIcon( SystemIcons.Information );

			filterTextBox.TextChanged += filterTextBox_TextChanged;

			checkBoxErrors.CheckedChanged += checkBox_CheckedChanged;
			checkBoxWarnings.CheckedChanged += checkBox_CheckedChanged;
			checkBoxInfos.CheckedChanged += checkBox_CheckedChanged;

			m_dt = new DataTable();

			// Two columns.
			//
			m_dt.Columns.Add( "Type", typeof( int ) );
			m_dt.Columns.Add( "Name", typeof( string ) );
			m_dt.Columns.Add( "Value", typeof( string ) );

			GenerateFlat();

			bindData( m_dt.DefaultView );

			updateCheckBoxes();
			updateRowFilter();
		}

		void checkBox_CheckedChanged( object sender, EventArgs e )
		{
			updateRowFilter();
		}

		public void GenerateFlat()
		{
			int items = 30;
			for (var i = 0; i < items; i++)
			{
				CreateItem();
			}
		}

		private void CreateItem()
		{
			int typ = s_random.Next( 0, 3 );

			if (typ == 0)
				m_numErrors += 1;
			else if (typ == 1)
				m_numWarnings += 1;
			else if (typ == 2)
				m_numInfos += 1;

			var name = CreateString( s_random.Next( 12, 21 ) );
			object value = CreateString( s_random.Next( 15, 36 ) );

			//var data =
			//	new DataItem(
			//		parent,
			//		name,
			//		value );

			m_dt.Rows.Add( typ, name, value );

			//return data;
		}

		private static string CreateString( int characters )
		{
			var sb = new StringBuilder();

			var max = Alphabet.Length;
			for (var i = 0; i < characters; i++)
			{
				var ch = Alphabet[s_random.Next( 0, max )];
				sb.Append( ch );
			}

			return sb.ToString();
		}

		private Image _ScaleIcon( Icon sourceIcon )
		{
			Size iconSize = SystemInformation.SmallIconSize;
			Bitmap bitmap = new Bitmap( iconSize.Width, iconSize.Height );

			using (Graphics g = Graphics.FromImage( bitmap ))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.DrawImage( sourceIcon.ToBitmap(), new Rectangle( Point.Empty, iconSize ) );
			}

			//Icon smallerErrorIcon = Icon.FromHandle( bitmap.GetHicon() );
			return bitmap;
		}

		void filterTextBox_TextChanged( object sender, EventArgs e )
		{
			//if (m_dataView == null)
			//	return;

			//string text = filterTextBox.Text;
			//if ( text.Length < 3 )
			//{
			//	m_dataView.RowFilter = String.Empty;
			//	return;
			//}

			//string userTextFixed = EscapeLikeValue( text );
			//string filter = string.Format( "Name LIKE '*{0}*'", userTextFixed );
			//m_dataView.RowFilter = filter;

			updateRowFilter();
		}

		public void bindData( DataView dataView )
		{
			m_dataView = dataView;

			dataGridView1.MouseClick += dataGridView1_MouseClick;
			dataGridView1.CellFormatting += dataGridView1_CellFormatting;

			dataGridView1.DataSource = dataView;
		}

		void dataGridView1_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
		{
			//if ( dataGridView1.Columns[e.ColumnIndex].Name == "Type" )
			if ( e.ColumnIndex == 0 )
			{
				if ( e.Value != null )
				{
					int ival = (int)e.Value;
					//if ( ival == 0 )
					//{
					//	e.Value = m_errorIcon;
					//}
					//else if (ival == 1)
					//{
					//	e.Value = m_warningIcon;
					//}
					//else if (ival == 2)
					//{
					//	e.Value = m_infoIcon;
					//}
					if (ival == 0)
					{
						e.Value = m_errorImage;
					}
					else if (ival == 1)
					{
						e.Value = m_warningImage;
					}
					else if (ival == 2)
					{
						e.Value = m_infoImage;
					}
				}
			}
		}

		private void dataGridView1_MouseClick( object sender, MouseEventArgs e )
		{
			dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;

			if (e.Button == MouseButtons.Right)
			{
				ContextMenu m = new ContextMenu();
				m.MenuItems.Add( new MenuItem( "Cut" ) );
				MenuItem copy = new MenuItem( "Copy" );
				copy.Click += copy_Click;
				m.MenuItems.Add( copy );
				m.MenuItems.Add( new MenuItem( "Paste" ) );

				int currentMouseOverRow = dataGridView1.HitTest( e.X, e.Y ).RowIndex;

				if (currentMouseOverRow >= 0)
				{
					m.MenuItems.Add( new MenuItem( string.Format( "Do something to row {0}", currentMouseOverRow.ToString() ) ) );
					dataGridView1.ClearSelection();
					DataGridViewRow row = dataGridView1.Rows[currentMouseOverRow];
					row.Selected = true;
				}

				m.Show( dataGridView1, new Point( e.X, e.Y ) );
			}
		}

		void copy_Click( object sender, EventArgs e )
		{
			StringBuilder sb = new StringBuilder();
			foreach( DataGridViewRow row in dataGridView1.SelectedRows )
			{
				string col0 = row.Cells[0].Value.ToString();
				string col1 = row.Cells[1].Value.ToString();
				sb.Append( col0 );
				sb.Append( ',' );
				sb.AppendLine( col1 );
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
			if (m_numErrors != 1)
				checkBoxErrors.Text = string.Format( "{0} Errors", m_numErrors );
			else
				checkBoxErrors.Text = "1 Error";

			if (m_numWarnings != 1)
				checkBoxWarnings.Text = string.Format( "{0} Warnings", m_numWarnings );
			else
				checkBoxWarnings.Text = "1 Warning";

			if (m_numInfos != 1)
				checkBoxInfos.Text = string.Format( "{0} Messages", m_numInfos );
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
				rowFilterTextBoxPart = string.Format( "(Name LIKE '*{0}*' OR Value LIKE '*{0}*')", userTextFixed );

				finalFilterValuee = string.Format( "{0} AND {1}", rowFilterCheckBoxesPart, rowFilterTextBoxPart );
			}

			//string userTextFixed = EscapeLikeValue( text );
			//string filter = string.Format( "Name LIKE '*{0}*'", userTextFixed );
			//m_dataView.RowFilter = filter;
			m_dataView.RowFilter = finalFilterValuee;
		}

		private DataView m_dataView;
		private Icon m_errorIcon;
		private Icon m_warningIcon;
		private Icon m_infoIcon;
		private Image m_errorImage;
		private Image m_warningImage;
		private Image m_infoImage;

		private int m_numErrors;
		private int m_numWarnings;
		private int m_numInfos;
		private DataTable m_dt;
		private static readonly Random s_random = new Random();
		private const string Alphabet = "     ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		//private string m_rowFilterCheckBoxesPart;
	}
}
