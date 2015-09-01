//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace pico.LogOutput
{
	enum MessageType
	{
		Error = 0,
		Warning = 1,
		Info = 2
	}

	public class DataItem
	{
		public static readonly int Type_Error = 0;
		public static readonly int Type_Warning = 1;
		public static readonly int Type_Info = 2;

		public int Type	{ get; set; }
		//public int Ordinal { get; set; }
		public string Group { get; set; }
		public string Description { get; set; }
		public string Tag { get; set; }
		public string File { get; set; }
		public int Line { get; set; }
	}

	public class picoLogDataTable
	{
		public picoLogDataTable()
		{
			m_dt = new DataTable();

			m_dt.Columns.Add( "Type", typeof( int ) );
			m_dt.Columns.Add( "Ordinal", typeof( int ) );
			m_dt.Columns.Add( "Group", typeof( string ) );
			m_dt.Columns.Add( "Description", typeof( string ) );
			m_dt.Columns.Add( "Tag", typeof( string ) );
			m_dt.Columns.Add( "File", typeof( string ) );
			m_dt.Columns.Add( "Line", typeof( int ) );

			GenerateFlat();
		}

		public int MaxRows { get { return m_maxRows; } }
		public int NumErrors { get { return m_numErrors; } }
		public int NumWarnings { get { return m_numWarnings; } }
		public int NumInfos { get { return m_numInfos; } }

		public DataTable Data
		{
			get { return m_dt; }
		}

		public DataView DataView
		{
			get { return m_dt.DefaultView; }
		}

		public void Clear()
		{
			m_dt.Clear();
			m_numErrors = 0;
			m_numWarnings = 0;
			m_numInfos = 0;
		}

		public void AddItem( DataItem item )
		{
			if ( m_dt.Rows.Count >= MaxRows )
				m_dt.Rows.RemoveAt( 0 );

			if (item.Type == DataItem.Type_Error)
				m_numErrors += 1;
			else if (item.Type == DataItem.Type_Warning)
				m_numWarnings += 1;
			else if (item.Type == DataItem.Type_Info)
				m_numInfos += 1;

			m_ordinal += 1;
			m_dt.Rows.Add( item.Type, m_ordinal, item.Group, item.Description, item.Tag, item.File, item.Line );
		}

		public void GenerateFlat()
		{
			int items = 30;
			for (var i = 0; i < items; i++)
			{
				DataItem di = CreateItem();
				AddItem( di );
			}
		}

		private DataItem CreateItem()
		{
			DataItem di = new DataItem();

			di.Type = s_random.Next( 0, 3 );
			di.Group = "Common";
			di.Description = CreateString( s_random.Next( 12, 21 ) );
			di.Tag = CreateString( s_random.Next( 15, 36 ) );
			di.File = CreateString( s_random.Next( 15, 36 ) );
			di.Line = s_random.Next( 0, 10000 );

			return di;
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

		private DataTable m_dt;
		private int m_maxRows = 100;
		private int m_ordinal;

		private int m_numErrors;
		private int m_numWarnings;
		private int m_numInfos;

		private static readonly Random s_random = new Random( 1973 );
		private static readonly string Alphabet = "     ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	}
}