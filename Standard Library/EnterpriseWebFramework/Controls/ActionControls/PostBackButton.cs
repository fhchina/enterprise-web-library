﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.JavaScriptWriting;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// A control that, when clicked, causes a post back and executes code.
	/// </summary>
	public class PostBackButton: WebControl, ControlTreeDataLoader, IPostBackEventHandler, ControlWithJsInitLogic, ActionControl {
		private readonly DataModification dataModification;

		/// <summary>
		/// Gets or sets the display style of this button. Do not set this to null.
		/// Choices are: ButtonActionControlStyle (default), BoxActionControlStyle, ImageActionControlStyle, CustomActionControlStyle, and TextActionControlStyle.
		/// </summary>
		public ActionControlStyle ActionControlStyle { get; set; }

		/// <summary>
		/// True if this button should act like a submit button (respond to the enter key). Doesn't work with the text or custom action control styles.
		/// </summary>
		public bool UsesSubmitBehavior { get; set; }

		/// <summary>
		/// Setting the content control will cause clicking the button to display a confirmation window with the given control as content displayed to the user.
		/// When this is set, this PostBackButton may not use submit behavior. This may not be used to display more than a small amount of content (e.g. some text 
		/// and a link), and absolutely may not be used with form controls.
		/// </summary>
		public Control ConfirmationWindowContentControl { get; set; }

		private Unit width = Unit.Empty;
		private Unit height = Unit.Empty;

		/// <summary>
		/// Creates a post back button. You may pass null for the clickHandler.
		/// </summary>
		public PostBackButton( DataModification dataModification, Action clickHandler, ActionControlStyle actionControlStyle, bool usesSubmitBehavior = true ) {
			if( dataModification == EwfPage.Instance.PostBackDataModification )
				throw new ApplicationException( "The post back data modification should only be executed by the framework." );
			this.dataModification = dataModification;

			ClickHandler = clickHandler;
			ActionControlStyle = actionControlStyle;
			UsesSubmitBehavior = usesSubmitBehavior;
		}

		/// <summary>
		/// Creates a post back button.
		/// </summary>
		public PostBackButton( DataModification dataModification, ActionControlStyle actionControlStyle, bool usesSubmitBehavior = true )
			: this( dataModification, null, actionControlStyle, usesSubmitBehavior ) {}

		/// <summary>
		/// Creates a post back button. Do not pass null for the data modification.
		/// </summary>
		// This constructor is needed because of ActionButtonSetups, which take the text in the ActionButtonSetup instead of here and the submit behavior will be overridden.
		public PostBackButton( DataModification dataModification, Action clickHandler ): this( dataModification, clickHandler, new ButtonActionControlStyle() ) {}

		/// <summary>
		/// Sets the method to be invoked when this button is clicked.
		/// </summary>
		public Action ClickHandler { private get; set; }

		/// <summary>
		/// Gets or sets the width of this button. Doesn't work with the text action control style.
		/// </summary>
		public override Unit Width { get { return width; } set { width = value; } }

		/// <summary>
		/// Gets or sets the height of this button. Only works with the image action control style.
		/// </summary>
		public override Unit Height { get { return height; } set { height = value; } }

		private ModalWindow confirmationWindow;

		void ControlTreeDataLoader.LoadData( DBConnection cn ) {
			if( TagKey == HtmlTextWriterTag.Button ) {
				// IE7 (or at least IE8 Compatibility View) unconditionally submits a value for all "button" elements with type "button" that are on the page,
				// regardless of what was clicked. We prevent these bad submissions from raising post back events by mangling the "name" attribute so it does not
				// correspond to the unique ID of any control. The __EVENTTARGET field will ensure that post back events are still raised for buttons that are actually
				// clicked.
				Attributes.Add( "name", UniqueID + ( UsesSubmitBehavior ? "" : "@" ) );

				Attributes.Add( "value", "v" );
				Attributes.Add( "type", UsesSubmitBehavior ? "submit" : "button" );
			}

			if( ConfirmationWindowContentControl != null ) {
				if( UsesSubmitBehavior )
					throw new ApplicationException( "PostBackButton cannot be the submit button and also have a confirmation message." );
				confirmationWindow = new ModalWindow( ConfirmationWindowContentControl, title: "Confirmation", postBackButton: this );
			}
			else if( !UsesSubmitBehavior )
				this.AddJavaScriptEventScript( JsWritingMethods.onclick, GetPostBackScript( this, true ) );

			CssClass = CssClass.ConcatenateWithSpace( "ewfClickable" );
			ActionControlStyle.SetUpControl( this, "", width, height, setWidth );
		}

		// NOTE: Nobody is currently passing false for the second parameter, but we expect to use false when we implement AutoPostBack behavior ourselves.
		internal static string GetPostBackScript( Control targetControl, bool isEventPostBack ) {
			if( !( targetControl is IPostBackEventHandler ) && isEventPostBack )
				throw new ApplicationException( "The target must be a post back event handler." );
			var pbo = new PostBackOptions( targetControl, isEventPostBack ? EwfPage.EventPostBackArgument : "" );
			return EwfPage.Instance.ClientScript.GetPostBackEventReference( pbo ) + "; return false";
		}

		private void setWidth( Unit w ) {
			base.Width = w;
		}

		string ControlWithJsInitLogic.GetJsInitStatements() {
			if( ConfirmationWindowContentControl != null ) {
				this.AddJavaScriptEventScript( JsWritingMethods.onclick,
				                               "$( '#" + ( confirmationWindow as EtherealControl ).Control.ClientID + "' ).dialog( 'open' ); return false" );
			}
			return ActionControlStyle.GetJsInitStatements( this );
		}

		void IPostBackEventHandler.RaisePostBackEvent( string eventArgument ) {
			EwfPage.Instance.EhValidateAndModifyData( topValidator => EwfPage.Instance.ExecuteDataModificationValidations( dataModification, topValidator ),
			                                          dataModification.ModifyData );
			if( ClickHandler != null ) {
				var canRun = false;
				EwfPage.Instance.EhExecute( () => canRun = true );
				if( canRun )
					ClickHandler();
			}
		}

		/// <summary>
		/// Returns the tag that represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey { get { return UsesSubmitBehavior ? HtmlTextWriterTag.Button : GetTagKey( ActionControlStyle ); } }

		internal static HtmlTextWriterTag GetTagKey( ActionControlStyle actionControlStyle ) {
			// NOTE: In theory, we should always return the button tag, but buttons are difficult to style in IE7.
			return actionControlStyle is TextActionControlStyle || actionControlStyle is CustomActionControlStyle ? HtmlTextWriterTag.A : HtmlTextWriterTag.Button;
		}

		internal static void AddButtonAttributes( WebControl control ) {
			control.Attributes.Add( "name", control.UniqueID );
			control.Attributes.Add( "value", "v" );
			control.Attributes.Add( "type", "button" );
		}
	}
}