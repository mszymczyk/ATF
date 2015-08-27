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
	public class picoLogDataTable
	{
		public picoLogDataTable()
		{
			m_dt = new DataTable();
		
			// Two columns.
			//
			m_dt.Columns.Add( "Type", typeof( int ) );
			m_dt.Columns.Add( "Name", typeof( string ) );
			m_dt.Columns.Add( "Value", typeof( string ) );

			GenerateFlat();
		}

		public DataTable Data
		{
			get { return m_dt; }
		}

		public DataView DataTableView
		{
			get { return m_dt.DefaultView; }
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

		private DataTable m_dt;
		private static readonly Random s_random = new Random();
		private const string Alphabet = "     ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	}
}