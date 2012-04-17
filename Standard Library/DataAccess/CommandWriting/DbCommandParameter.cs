﻿using System;
using System.Data.Common;
using RedStapler.StandardLibrary.DatabaseSpecification;
using RedStapler.StandardLibrary.DatabaseSpecification.Databases;

namespace RedStapler.StandardLibrary.DataAccess.CommandWriting {
	/// <summary>
	/// A parameter for a database command.
	/// </summary>
	public class DbCommandParameter {
		private string name;
		private readonly DbParameterValue value;
		private DbParameter parameter;

		/// <summary>
		/// Creates a command parameter.
		/// </summary>
		public DbCommandParameter( string name, DbParameterValue value ) {
			this.name = name;
			this.value = value;
		}

		internal string Name {
			set {
				if( parameter != null )
					throw new ApplicationException( "Name cannot be set after the ADO.NET parameter object has been created." );
				name = value;
			}
		}

		/// <summary>
		/// Returns true if the value of this parameter is null.
		/// </summary>
		internal bool ValueIsNull { get { return value.Value == null; } }

		/// <summary>
		/// Returns @abc for sql server and :abc for oracle.
		/// </summary>
		public string GetNameForCommandText( DatabaseInfo databaseInfo ) {
			return databaseInfo.ParameterPrefix + name;
		}

		/// <summary>
		/// Returns the ADO.NET parameter object for this parameter. The ADO.NET parameter object is created on the first call to this method.
		/// </summary>
		public DbParameter GetAdoDotNetParameter( DatabaseInfo databaseInfo ) {
			if( parameter != null )
				return parameter;
			parameter = databaseInfo.CreateParameter();

			// SQL Server requires @ in front of parameter names. Although Oracle requires : in front of parameters in the command text, it does not require this
			// colon in front of parameter names here.
			// NOTE: I think we should use the colon even though it is not required, because it will change 3 lines plus a comment into one line.
			if( databaseInfo is SqlServerInfo )
				name = "@" + name;
			parameter.ParameterName = name;

			parameter.Value = value.Value ?? DBNull.Value;
			if( value.DbTypeString != null )
				databaseInfo.SetParameterType( parameter, value.DbTypeString );
			return parameter;
		}
	}
}