using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace T0yK4T.Tools
{
	/// <summary>
	/// OBSOLETE!
	/// </summary>
	[Obsolete("Please use the built-in SQLiteConnectionStringBuilder!")]
	public class SQLiteConnectionStringBuilder_
	{
		private string dataSource;
		private int version;
		private bool newdb;
		private bool compress;

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		public SQLiteConnectionStringBuilder_()
		{
			this.dataSource = "database.db";
			this.version = 0;
			this.newdb = true;
			this.compress = true;
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		/// <param name="DataSource"></param>
		/// <param name="Version"></param>
		/// <param name="New"></param>
		/// <param name="Compress"></param>
		public SQLiteConnectionStringBuilder_(string DataSource, int Version, bool New, bool Compress)
		{
			this.dataSource = DataSource;
			this.version = Version;
			this.newdb = New;
			this.compress = Compress;
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		public string DataSource
		{
			get { return this.dataSource; }
			set { this.dataSource = value; }
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		public int Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		public bool New
		{
			get { return this.newdb; }
			set { this.newdb = value; }
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		public bool Compress
		{
			get { return this.compress; }
			set { this.compress = value; }
		}

		//"Data Source=database.db;Version=3;New=False;Compress=True;"
		/// <summary>
		/// /// OBSOLETE!
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "Data Source=" + this.dataSource + ";Version=" + this.version.ToString() + ";New=" + this.newdb.ToString() + ";Compress=" + this.compress.ToString() + ";";
		}

		/// <summary>
		/// OBSOLETE!
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static implicit operator string(SQLiteConnectionStringBuilder_ builder)
		{
			return builder.ToString();
		}
	}
}
