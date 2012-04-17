using System.Web.UI.WebControls;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.EnterpriseWebFramework;
using RedStapler.StandardLibrary.EnterpriseWebFramework.Controls;

namespace RedStapler.TestWebSite.TestPages {
	public partial class ActionControls: EwfPage {
		public partial class Info {
			protected override void init( DBConnection cn ) {}
		}

		protected override void LoadData( DBConnection cn ) {
			ph.AddControlsReturnThis(
				new Box( new PostBackButton( new DataModification(),
				                             () => { },
				                             new ButtonActionControlStyle( "Tiny Post Back Button", ButtonActionControlStyle.ButtonSize.ShrinkWrap ),
				                             false ) ) );
			ph.AddControlsReturnThis(
				new Box( EwfLink.Create( SubFolder.General.GetInfo(), new ButtonActionControlStyle( "Tiny EWF Link", ButtonActionControlStyle.ButtonSize.ShrinkWrap ) ) ) );
			ph.AddControlsReturnThis( new Box( new ToggleButton( new ButtonActionControlStyle( "Tiny Toggle Button", ButtonActionControlStyle.ButtonSize.ShrinkWrap ) ) ) );

			ph.AddControlsReturnThis(
				new Box( new PostBackButton( new DataModification(), () => { } )
				         	{ ActionControlStyle = new ButtonActionControlStyle { Text = "Post Back Button" }, UsesSubmitBehavior = false, Width = Unit.Pixel( 200 ) } ) );
			ph.AddControlsReturnThis( new Box( new EwfLink( EwfTableDemo.GetInfo() ) { ActionControlStyle = new ButtonActionControlStyle { Text = "EWF Link" } } ) );
			ph.AddControlsReturnThis( new Box( new ToggleButton { ActionControlStyle = new ButtonActionControlStyle { Text = "Toggle button" } } ) );

			ph.AddControlsReturnThis(
				new Box( new PostBackButton( new DataModification(), () => { } )
				         	{
				         		ActionControlStyle = new ButtonActionControlStyle( ButtonActionControlStyle.ButtonSize.Large ) { Text = "Large Post Back Button" },
				         		UsesSubmitBehavior = false
				         	} ) );
			ph.AddControlsReturnThis(
				new Box( new EwfLink( EwfTableDemo.GetInfo() )
				         	{ ActionControlStyle = new ButtonActionControlStyle( ButtonActionControlStyle.ButtonSize.Large ) { Text = "Large EWF Link" } } ) );
			ph.AddControlsReturnThis(
				new Box( new ToggleButton
				         	{ ActionControlStyle = new ButtonActionControlStyle( ButtonActionControlStyle.ButtonSize.Large ) { Text = "Large Toggle Button" } } ) );
		}
	}
}