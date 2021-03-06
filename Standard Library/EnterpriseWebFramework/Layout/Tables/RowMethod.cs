﻿using RedStapler.StandardLibrary.DataAccess;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// A method that performs an operation on the row with the given unique identifier.
	/// </summary>
	public delegate void RowMethod( DBConnection cn, object uniqueIdentifier );
}