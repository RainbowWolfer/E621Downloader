﻿using E621Downloader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace E621Downloader.Views.SubscriptionSection {
	public sealed partial class SubscriptionDownloadDialog: UserControl {
		private (int selected, int local) numbers;
		private bool isSelectedDownload;

		public bool TodayDate => TodayDateBox.IsChecked == true;

		public (int selected, int local) Numbers {
			get => numbers;
			set {
				numbers = value;
				SelectedText.Text = $"· {value.selected} {(value.selected > 1 ? "Posts".Language() : "Post".Language())}" +
				$" {(value.local > 0 ? "({{0}} local posts will not be downloaded)".Language(value.local) : "")}";
				//SelectedText.Text = "Download Selected But Local".Language(value.selected, value.local);
			}
		}

		public bool IsSelectedDownload {
			get => isSelectedDownload;
			set {
				isSelectedDownload = value;
				if(value) {
					DownloadTitleText.Visibility = Visibility.Collapsed;
					SelectedTitleText.Visibility = Visibility.Visible;
				} else {
					DownloadTitleText.Visibility = Visibility.Visible;
					SelectedTitleText.Visibility = Visibility.Collapsed;
				}
			}
		}

		public SubscriptionDownloadDialog() {
			this.InitializeComponent();
		}
	}
}
