using System;
using System.Web.UI.WebControls;
using RedStapler.StandardLibrary.JavaScriptWriting;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework.Controls {
	/// <summary>
	/// The behavior for one or more clickable table elements. The click script will add rollover behavior to the table element(s), unless it is used on a field
	/// of an EWF table or an item of a column primary table. Column hover behavior is not possible with CSS.
	/// </summary>
	public class ClickScript {
		private string url;
		private Action method;
		private string script;

		/// <summary>
		/// Creates a script that redirects to the specified page. Passing null for pageInfo will result in no script being added.
		/// </summary>
		public static ClickScript CreateRedirectScript( PageInfo pageInfo ) {
			if( EwfPage.Instance is AutoDataModifier )
				return CreatePostBackScript( () => EwfPage.Instance.EhRedirect( pageInfo ) );
			return new ClickScript { url = pageInfo == null ? "" : pageInfo.GetUrl() };
		}

		/// <summary>
		/// Do not call. This method will be deleted.
		/// </summary>
		public static ClickScript CreateRedirectScript( string url ) {
			if( EwfPage.Instance is AutoDataModifier )
				return CreatePostBackScript( () => EwfPage.Instance.EhRedirect( url ) );
			return new ClickScript { url = url };
		}

		/// <summary>
		/// Creates a script that posts the page back and executes the specified method.
		/// Do not pass null for method.
		/// </summary>
		public static ClickScript CreatePostBackScript( Action method ) {
			return new ClickScript { method = method };
		}

		/// <summary>
		/// Creates a custom script. A semicolon will be added to the end of the script. Do not pass null for script.
		/// </summary>
		public static ClickScript CreateCustomScript( string script ) {
			return new ClickScript { script = script };
		}

		private ClickScript() {}

		internal void SetUpClickableControl( WebControl clickableControl ) {
			if( url == "" || script == "" )
				return;

			clickableControl.CssClass = clickableControl.CssClass.ConcatenateWithSpace( "ewfClickable" );

			Func<string> scriptGetter;
			if( url != null )
				scriptGetter = () => "location.href = '" + EwfPage.Instance.GetClientUrl( url ) + "'; return false";
			else if( method != null ) {
				var externalHandler = new ExternalPostBackEventHandler();
				externalHandler.PostBackEvent += method;

				// NOTE: Remove this hack when DynamicTable is gone.
				if( clickableControl is TableRow )
					clickableControl.Parent.Parent.Controls.Add( externalHandler );
				else
					clickableControl.Controls.Add( externalHandler );

				scriptGetter = () => PostBackButton.GetPostBackScript( externalHandler, true );
			}
			else
				scriptGetter = () => script;

			// Defer script generation until after all controls have IDs.
			EwfPage.Instance.PreRender += delegate { clickableControl.AddJavaScriptEventScript( JsWritingMethods.onclick, scriptGetter() ); };
		}
	}
}