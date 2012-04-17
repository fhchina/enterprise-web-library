﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.EnterpriseWebFramework.CssHandling;
using RedStapler.StandardLibrary.EnterpriseWebFramework.DisplayLinking;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// A drop down list or radio button list that allows exactly one value to be selected.
	/// </summary>
	public class EwfListControl: WebControl, ControlTreeDataLoader, FormControl<string>, ControlWithCustomFocusLogic {
		internal class CssElementCreator: ControlCssElementCreator {
			internal const string CssClass = "ewfListControlWrapper";

			CssElement[] ControlCssElementCreator.CreateCssElements() {
				return new[] { new CssElement( "ListControlDropDownStyle", "div." + CssClass + " > select" ) };
			}
		}

		/// <summary>
		/// The possible forms an EwfListControl can take.
		/// </summary>
		public enum ListControlType {
			/// <summary>
			/// A drop down.
			/// </summary>
			DropDownList,

			/// <summary>
			/// A radio button list laid out horizontally.
			/// </summary>
			HorizontalRadioButton,

			/// <summary>
			/// A radio button list laid out vertically.
			/// </summary>
			VerticalRadioButton
		}

		private class DisplayLinkData {
			public string ListItemValue { get; set; }
			public bool ControlsVisibleWhenSelected { get; set; }
			public Control[] Controls { get; set; }
		}

		private FreeFormRadioList freeFormRadioList;
		private DropDownList dropDownList;
		private readonly List<ListItem> listItems = new List<ListItem>();
		private string durableValue = "";
		private bool durableValueSet;
		private readonly List<Action> selectedIndexChangedEventHandlers = new List<Action>();
		private readonly List<DisplayLinkData> displayLinkDataPackages = new List<DisplayLinkData>();
		private List<EwfCheckBox> checkBoxes;
		private PostBackButton defaultSubmitButton;

		/// <summary>
		/// Gets or sets the type of list control this is.
		/// </summary>
		public ListControlType Type { get; set; }

		/// <summary>
		/// Gets or sets auto post back for this control.
		/// </summary>
		public bool AutoPostBack { get; set; }

		/// <summary>
		/// Sets the value for this list control. Do not pass null. Do not use the getter; it is obsolete.
		/// </summary>
		public string Value {
			get { return GetPostBackValue( AppRequestState.Instance.EwfPageRequestState.PostBackValues ); }
			set {
				if( value == null )
					throw new ApplicationException(
						"You cannot use null for a selected value.  The underlying ASP.NET controls do not support null.  Use empty string instead." );
				if( !listItems.Any( li => li.Value == value ) )
					return;
				durableValue = value;
				durableValueSet = true;
			}
		}

		// NOTE: Roll into LoadData.
		private void setValueOnPhysicalControl( string val ) {
			if( dropDownList != null )
				dropDownList.SelectedValue = val;
			else
				freeFormRadioList.SelectedValue = val;
		}

		/// <summary>
		/// EWF ToolTip to display on this control. Setting ToolTipControl will ignore this property.
		/// </summary>
		public override string ToolTip { get; set; }

		/// <summary>
		/// Control to display inside the tool tip. Do not pass null. This will ignore the ToolTip property.
		/// </summary>
		public Control ToolTipControl { get; set; }

		/// <summary>
		/// Creates a new list control.
		/// </summary>
		public EwfListControl() {
			Type = ListControlType.DropDownList;
		}

		string FormControl<string>.DurableValue {
			get {
				if( !durableValueSet && listItems.Any() )
					return listItems.First().Value;
				return durableValue;
			}
		}

		string FormControl.DurableValueAsString { get { return ( this as FormControl<string> ).DurableValue; } }

		/// <summary>
		/// Add a new list item to the list control.
		/// </summary>
		public void AddItem( string label, string valueAsString ) {
			AddItem( label, valueAsString, "" );
		}

		/// <summary>
		/// Add a new list item with the given css class to the list control.
		/// Do not pass null for css class. If you do not want to use a css class, 
		/// use the other overload of this method.
		/// </summary>
		public void AddItem( string label, string valueAsString, string cssClass ) {
			var listItem = new ListItem( label, valueAsString );
			if( cssClass.Length > 0 )
				listItem.Attributes[ "class" ] = cssClass;
			listItems.Add( listItem );
		}

		/// <summary>
		/// Do not use.
		/// </summary>
		public void RemoveItems( string valueAsString ) {
			var list = new List<ListItem>( listItems );
			foreach( var item in list ) {
				if( item.Value == valueAsString )
					listItems.Remove( item );
			}
		}

		/// <summary>
		/// Do not use.
		/// </summary>
		public void ClearItems() {
			listItems.Clear();
		}

		/// <summary>
		/// Returns the first item with the given value.  If the item is not found, returns null.
		/// </summary>
		public ListItem GetItemByValue( string valueAsString ) {
			return listItems.FirstOrDefault( item => item.Value == valueAsString );
		}

		/// <summary>
		/// Returns the index of the first item with the given value.  If the item is not found, returns null.
		/// </summary>
		internal int? GetIndexByValue( string valueAsString ) {
			var cnt = 0;
			foreach( var item in listItems ) {
				if( item.Value == valueAsString )
					return cnt;
				cnt++;
			}
			return null;
		}

		/// <summary>
		/// Returns the index of the selected value.
		/// </summary>
		internal int? SelectedIndex { get { return GetIndexByValue( Value ); } }

		/// <summary>
		/// Fills the given list control with items "Yes" and "No" and values of true and false.
		/// </summary>
		public void FillWithYesNo() {
			FillWithTrueFalse( "Yes", "No" );
		}

		/// <summary>
		/// Fills the given list control with items trueText and falseText and values of true and false.
		/// </summary>
		public void FillWithTrueFalse( string trueText, string falseText ) {
			AddItem( trueText, true.ToString() );
			AddItem( falseText, false.ToString() );
		}

		/// <summary>
		/// Fills the given list control with an item whose value is the empty string, and whose text is the given text.
		/// </summary>
		public void FillWithBlank( string blankText ) {
			AddItem( blankText, "" );
		}

		/// <summary>
		/// Do not use.
		/// NOTE: Do not use this method right now. If the first item in the list is created by FillWithBlank, no initial value is explicitly specified, and a value not in the list is posted back, the handler incorrectly fires.
		/// Another way of explaining this is that our leniance in accepting selected values that aren't in the list (the client comes back with a "4", but there is no "4", so we keep it as "") is biting us here.
		/// </summary>
		public void AddSelectedIndexChangedEventHandler( Action method ) {
			selectedIndexChangedEventHandlers.Add( method );
		}

		/// <summary>
		/// Creates a display link that toggles the visibility of the given controls when the given list item is selected and unselected.
		/// </summary>
		public void AddDisplayLink( string listItemValue, bool controlsVisibleWhenSelected, params WebControl[] controls ) {
			displayLinkDataPackages.Add( new DisplayLinkData
			                             	{ ListItemValue = listItemValue, ControlsVisibleWhenSelected = controlsVisibleWhenSelected, Controls = controls } );
		}

		/// <summary>
		/// Creates a display link that toggles the visibility of the given controls when the given list item is selected and unselected.
		/// </summary>
		public void AddDisplayLink( string listItemValue, bool controlsVisibleWhenSelected, params HtmlControl[] controls ) {
			displayLinkDataPackages.Add( new DisplayLinkData
			                             	{ ListItemValue = listItemValue, ControlsVisibleWhenSelected = controlsVisibleWhenSelected, Controls = controls } );
		}

		/// <summary>
		/// Assigns this to submit the given PostBackButton. This will disable the button's submit behavior. Do not pass null.
		/// </summary>
		public void SetDefaultSubmitButton( PostBackButton pbb ) {
			defaultSubmitButton = pbb;
		}

		void ControlTreeDataLoader.LoadData( DBConnection cn ) {
			if( Type == ListControlType.DropDownList ) {
				dropDownList = new DropDownList();
				foreach( var listItem in listItems ) {
					if( listItem.Text.IsNullOrWhiteSpace() )
						listItem.Text = HttpUtility.HtmlDecode( "&nbsp;" );
					dropDownList.Items.Add( listItem );
				}
				dropDownList.AutoPostBack = AutoPostBack;
				foreach( var selectedIndexChangedHandler in selectedIndexChangedEventHandlers ) {
					var handler = selectedIndexChangedHandler;
					dropDownList.SelectedIndexChanged += delegate { handler(); };
				}
				Controls.Add( dropDownList );

				addToolTipIfNeccesary( dropDownList );

				foreach( var displayLink in displayLinkDataPackages ) {
					ListControlToControlArrayDisplayLink.AddToPage( dropDownList,
					                                                GetIndexByValue( displayLink.ListItemValue ).Value,
					                                                displayLink.ControlsVisibleWhenSelected,
					                                                displayLink.Controls );
				}

				EwfPage.Instance.MakeControlPostBackOnEnter( dropDownList, defaultSubmitButton );
			}
			else {
				checkBoxes = new List<EwfCheckBox>();
				freeFormRadioList = new FreeFormRadioList( UniqueID );

				foreach( var listItem in listItems ) {
					var checkBox = new EwfCheckBox( listItem.Text ) { AutoPostBack = AutoPostBack };
					if( defaultSubmitButton != null )
						checkBox.SetDefaultSubmitButton( defaultSubmitButton );
					freeFormRadioList.AddCheckBox( listItem.Value, checkBox );
					checkBoxes.Add( checkBox );
				}

				foreach( var selectedIndexChangedHandler in selectedIndexChangedEventHandlers )
					freeFormRadioList.AddSelectedIndexChangedHandler( selectedIndexChangedHandler );

				var container = Type == ListControlType.HorizontalRadioButton
				                	? (Control)new ControlLine( checkBoxes.ToArray() )
				                	: ControlStack.CreateWithControls( true, checkBoxes.ToArray() );

				Controls.Add( container );

				addToolTipIfNeccesary( container );

				foreach( var displayLink in displayLinkDataPackages ) {
					FreeFormRadioListToControlArrayDisplayLink.AddToPage( freeFormRadioList,
					                                                      displayLink.ListItemValue,
					                                                      displayLink.ControlsVisibleWhenSelected,
					                                                      displayLink.Controls );
				}
			}

			if( !durableValueSet && listItems.Any() )
				durableValue = listItems.First().Value;
			setValueOnPhysicalControl( AppRequestState.Instance.EwfPageRequestState.PostBackValues.GetValue( this ) );
		}

		/// <summary>
		/// Hooks up the tool tip for this control to the given target control
		/// </summary>
		private void addToolTipIfNeccesary( Control targetControl ) {
			if( ToolTip != null || ToolTipControl != null )
				new ToolTip( ToolTipControl ?? EnterpriseWebFramework.Controls.ToolTip.GetToolTipTextControl( ToolTip ), targetControl );
		}

		void FormControl.AddPostBackValueToDictionary( PostBackValueDictionary postBackValues ) {
			if( dropDownList != null )
				postBackValues.Add( this, dropDownList.SelectedValue );
			// Don't add a value in radio button mode since the check boxes do this themselves.
		}

		/// <summary>
		/// Gets the post back value.
		/// </summary>
		public string GetPostBackValue( PostBackValueDictionary postBackValues ) {
			return freeFormRadioList != null ? freeFormRadioList.SelectedValue : postBackValues.GetValue( this );
		}

		/// <summary>
		/// Returns true if the value changed on this post back.
		/// </summary>
		public bool ValueChangedOnPostBack( PostBackValueDictionary postBackValues ) {
			return freeFormRadioList != null ? freeFormRadioList.ValueChangedOnPostBack( postBackValues ) : postBackValues.ValueChangedOnPostBack( this );
		}

		void ControlWithCustomFocusLogic.SetFocus() {
			if( dropDownList != null )
				Page.SetFocus( dropDownList );
			else if( checkBoxes.Any() )
				( checkBoxes.First() as ControlWithCustomFocusLogic ).SetFocus();
		}

		/// <summary>
		/// Returns the div tag, which represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Div; } }

		/// <summary>
		/// Renders this control after applying the appropriate CSS classes.
		/// </summary>
		protected override void Render( HtmlTextWriter writer ) {
			CssClass = CssClass.ConcatenateWithSpace( CssElementCreator.CssClass );
			if( Width == Unit.Empty )
				CssClass = CssClass.ConcatenateWithSpace( "unspecifiedWidth" );
			// NOTE: Sometimes add ewfStandard here, we think.
			base.Render( writer );
		}
	}
}