﻿using System;
using System.Linq;
using System.Web.UI;
using RedStapler.StandardLibrary.EnterpriseWebFramework.Ui.Entity;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework.Ui {
	/// <summary>
	/// Standard Library use only.
	/// </summary>
	public static class EwfUiStatics {
		private static AppEwfUiProvider provider;

		internal static void Init( Type globalType ) {
			var appAssembly = globalType.Assembly;
			var typeName = globalType.Namespace + ".Providers.EwfUiProvider";

			if( appAssembly.GetType( typeName ) != null )
				provider = appAssembly.CreateInstance( typeName ) as AppEwfUiProvider;
		}

		/// <summary>
		/// Standard Library use only.
		/// </summary>
		public static AppEwfUiProvider AppProvider {
			get {
				if( provider == null )
					throw new ApplicationException( "EWF UI provider not found in application" );
				return provider;
			}
		}

		/// <summary>
		/// EwfUiMaster use only. Returns the tab mode, or null for no tabs. NOTE: Doesn't return null for no tabs yet.
		/// </summary>
		public static TabMode? GetTabMode( this EntitySetupInfo esInfo ) {
			var modeOverrider = esInfo as TabModeOverrider;
			if( modeOverrider == null )
				return TabMode.Vertical;
			var mode = modeOverrider.GetTabMode();
			if( mode == TabMode.Automatic )
				return ( esInfo.Pages.Count == 1 && esInfo.Pages.Single().Pages.Count() < 8 ) ? TabMode.Horizontal : TabMode.Vertical;
			return mode;
		}

		/// <summary>
		/// Gets the current EWF UI master page. Standard Library use only.
		/// </summary>
		public static AppEwfUiMasterPage AppMasterPage { get { return EwfPage.Instance.Master.Master != null ? getSecondLevelMaster( EwfPage.Instance.Master ) as AppEwfUiMasterPage : null; } }

		private static MasterPage getSecondLevelMaster( MasterPage master ) {
			return master.Master.Master == null ? master : getSecondLevelMaster( master.Master );
		}

		/// <summary>
		/// Clears the content foot and adds the specified actions. Call this only from the page. The first action, if it is a post back button, will use submit
		/// behavior.
		/// </summary>
		public static void SetContentFootActions( params ActionButtonSetup[] actions ) {
			AppMasterPage.SetContentFootActions( actions );
		}

		/// <summary>
		/// Clears the content foot and adds the specified controls. Call this only from the page.
		/// </summary>
		public static void SetContentFootControls( params Control[] controls ) {
			AppMasterPage.SetContentFootControls( controls );
		}
	}
}