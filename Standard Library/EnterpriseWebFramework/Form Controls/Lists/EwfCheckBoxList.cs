﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.EnterpriseWebFramework.Controls;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A check box list that allows multiple items to be selected.
	/// NOTE: Consider using something like the multi select feature of http://harvesthq.github.com/chosen/ to provide a space-saving mode for this control.
	/// </summary>
	public class EwfCheckBoxList<ItemIdType>: WebControl, ControlTreeDataLoader, ControlWithCustomFocusLogic {
		private readonly IEnumerable<EwfListItem<ItemIdType>> items;
		private readonly IEnumerable<ItemIdType> selectedItemIds;
		private readonly string caption;
		private readonly bool includeSelectAndDeselectAllButtons;
		private readonly byte numberOfColumns;

		private readonly Dictionary<EwfListItem<ItemIdType>, BlockCheckBox> checkBoxesByItem = new Dictionary<EwfListItem<ItemIdType>, BlockCheckBox>();

		/// <summary>
		/// Creates a check box list.
		/// </summary>
		public EwfCheckBoxList( IEnumerable<EwfListItem<ItemIdType>> items, IEnumerable<ItemIdType> selectedItemIds, string caption = "",
		                        bool includeSelectAndDeselectAllButtons = false, byte numberOfColumns = 1 ) {
			this.items = items.ToArray();
			this.selectedItemIds = selectedItemIds.ToArray();
			this.caption = caption;
			this.includeSelectAndDeselectAllButtons = includeSelectAndDeselectAllButtons;
			this.numberOfColumns = numberOfColumns;
		}

		void ControlTreeDataLoader.LoadData( DBConnection cn ) {
			CssClass = CssClass.ConcatenateWithSpace( "ewfStandardCheckBoxList" );

			var table = new DynamicTable { Caption = caption };
			if( includeSelectAndDeselectAllButtons ) {
				table.AddActionLink( new ActionButtonSetup( "Select All", new CustomButton( string.Format( @"toggleCheckBoxes( '{0}', true )", ClientID ) ) ) );
				table.AddActionLink( new ActionButtonSetup( "Deselect All", new CustomButton( string.Format( @"toggleCheckBoxes( '{0}', false )", ClientID ) ) ) );
			}

			var itemsPerColumn = (int)Math.Ceiling( (decimal)items.Count() / numberOfColumns );
			var cells = new List<EwfTableCell>();
			for( byte i = 0; i < numberOfColumns; i += 1 ) {
				var maxIndex = Math.Min( ( i + 1 ) * itemsPerColumn, items.Count() );
				var place = new PlaceHolder();
				for( var j = i * itemsPerColumn; j < maxIndex; j += 1 ) {
					var item = items.ElementAt( j );
					var isChecked = selectedItemIds.Contains( item.Id );

					var checkBox = new BlockCheckBox( isChecked, label: item.Label );
					checkBox.AddOnClickJsMethod( "changeCheckBoxColor( this )" );
					if( isChecked )
						checkBox.CssClass = "checkedChecklistCheckboxDiv";

					place.Controls.Add( checkBox );
					checkBoxesByItem.Add( item, checkBox );
				}
				cells.Add( new EwfTableCell( place ) );
			}
			table.AddRow( cells.ToArray() );
			Controls.Add( table );
		}

		void ControlWithCustomFocusLogic.SetFocus() {
			if( items.Any() )
				( checkBoxesByItem[ items.First() ] as ControlWithCustomFocusLogic ).SetFocus();
		}

		/// <summary>
		/// Gets the selected item IDs in the post back.
		/// </summary>
		public IEnumerable<ItemIdType> GetSelectedItemIdsInPostBack( PostBackValueDictionary postBackValues ) {
			return items.Where( i => checkBoxesByItem[ i ].IsCheckedInPostBack( postBackValues ) ).Select( i => i.Id ).ToArray();
		}

		/// <summary>
		/// Returns true if the selections changed on this post back.
		/// </summary>
		public bool SelectionsChangedOnPostBack( PostBackValueDictionary postBackValues ) {
			return items.Any( i => checkBoxesByItem[ i ].ValueChangedOnPostBack( postBackValues ) );
		}

		/// <summary>
		/// Returns the tag that represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }
	}
}