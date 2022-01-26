﻿using E621Downloader.Models;
using E621Downloader.Models.Download;
using E621Downloader.Models.Locals;
using E621Downloader.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Pages {
	public sealed partial class SettingsPage: Page {
		public static bool isDownloadPathChangingHandled;
		public string Version => "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public SettingsPage() {
			this.InitializeComponent();
			ClearDownloadPathButton.IsEnabled = Local.DownloadFolder != null;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			DownloadPathTextBlock.Text = Local.DownloadFolder == null ? "No Download Path Selected" : Local.DownloadFolder.Path;
			CustomHostToggle.IsOn = LocalSettings.Current.customHostEnable;
			NullImageToggle.IsOn = LocalSettings.Current.showNullImages;
			BlackListToggle.IsOn = LocalSettings.Current.showBlackListed;
			CycleListToggle.IsOn = LocalSettings.Current.cycleList;
			CustomHostButton.IsEnabled = LocalSettings.Current.customHostEnable;
		}

		private async void BlackListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var result = await PopUp(this, "Black List", Local.BlackList);
			if(result.Item1 == ContentDialogResult.Primary) {
				if(!App.CompareTwoArray(result.Item2, result.Item3)) {
					Local.WriteBlackList(result.Item3);
				}
			}
		}

		private async void FollowListButton_Tapped(object sender, TappedRoutedEventArgs e) {
			var result = await PopUp(this, "Follow List", Local.FollowList);
			if(result.Item1 == ContentDialogResult.Primary) {
				if(!App.CompareTwoArray(result.Item2, result.Item3)) {
					Local.WriteFollowList(result.Item3);
				}
			}
		}


		private static async Task<(ContentDialogResult, string[], string[])> PopUp(Page page, string title, string[] list) {
			string[] oldValue = list;
			ContentDialog dialog = new ContentDialog() {
				Title = title,
				PrimaryButtonText = "Confirm",
				SecondaryButtonText = "Cancel",
			};
			var manager = new ListManager(page, oldValue, dialog);
			dialog.Content = manager;
			ContentDialogResult result = await dialog.ShowAsync();
			string[] newValue = manager.GetCurrentTags();
			return (result, oldValue, newValue);
		}

		public static async Task FollowListManage(Page page) {
			var result = await PopUp(page, "Follow List", Local.FollowList);
			if(result.Item1 == ContentDialogResult.Primary) {
				if(!App.CompareTwoArray(result.Item2, result.Item3)) {
					Local.WriteFollowList(result.Item3);
				}
			}
		}

		private async void DownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(Local.DownloadFolder == null) {
				Debug.WriteLine("no download path");
			} else {
				Debug.WriteLine(Local.DownloadFolder.Path);
			}
			FolderPicker pick = new FolderPicker() { FileTypeFilter = { "*" } };
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				string token = StorageApplicationPermissions.FutureAccessList.Add(result);
				Debug.WriteLine(token);
				await Local.WriteTokenToFile(token);
				DownloadPathTextBlock.Text = Local.DownloadFolder.Path;
				ClearDownloadPathButton.IsEnabled = true;
			}
			isDownloadPathChangingHandled = false;
		}

		private async void ClearDownloadPathButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if(DownloadsManager.HasDownloading()) {
				await MainPage.CreatePopupDialog("Warning", "There is something downloading.\ncannot clear download path.");
				return;
			}
			Local.ClearToken(Local.GetToken());
			if(Local.GetToken() == null) {
				ClearDownloadPathButton.IsEnabled = false;
			}
			DownloadPathTextBlock.Text = "No Path Selected";
		}

		private void BlackListToggle_Toggled(object sender, RoutedEventArgs e) {
			LocalSettings.Current.showBlackListed = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void NullImageToggle_Toggled(object sender, RoutedEventArgs e) {
			LocalSettings.Current.showNullImages = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private void CustomHostToggle_Toggled(object sender, RoutedEventArgs e) {
			LocalSettings.Current.customHostEnable = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
			CustomHostButton.IsEnabled = (sender as ToggleSwitch).IsOn;
			CustomHostButton.Content = LocalSettings.Current.customHostEnable ? LocalSettings.Current.customHost : "E926.net";
		}

		private void CycleListToggle_Toggled(object sender, RoutedEventArgs e) {
			LocalSettings.Current.cycleList = (sender as ToggleSwitch).IsOn;
			LocalSettings.Save();
		}

		private async void CustomHostButton_Tapped(object sender, TappedRoutedEventArgs e) {
			ContentDialog dialog = new ContentDialog() {
				Title = "Custom Host",
			};
			var content = new CustomHostInputDialog(dialog, LocalSettings.Current.customHost ?? "");
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Confirm) {
				CustomHostButton.Content = content.InputText;
				LocalSettings.Current.customHost = content.InputText;
				LocalSettings.Save();
			}
		}
	}
}
