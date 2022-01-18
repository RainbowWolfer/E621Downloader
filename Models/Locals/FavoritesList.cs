﻿using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class FavoritesList: ICloneable {
		public static List<FavoritesList> Table { get; set; } = new List<FavoritesList>();

		public static void Modify(List<string> newLists, List<string> containLists, string path, PathType type) {
			foreach(string item in newLists) {
				Table.Add(new FavoritesList(item));
			}
			foreach(FavoritesList list in Table) {
				if(containLists.Contains(list.Name)) {
					//add in this list if not in list
					if(!list.Items.Any(i => i.Path == path && i.Type == type)) {
						list.Items.Add(new FavoriteItem(type, path));
					}
				} else {
					//remove in this list if in list
					if(list.Items.Any(i => i.Path == path && i.Type == type)) {
						list.Items.RemoveAll(i => i.Path == path && i.Type == type);
					}
				}
			}

			Save();
		}

		public static async void Save() {
			await Local.WriteFavoritesLists();
		}

		//------------------------------------------------------------

		public string Name { get; set; } = "Undefined";
		public List<FavoriteItem> Items { get; set; } = new List<FavoriteItem>();

		public FavoritesList(string name) {
			Name = name;
		}

		public FavoritesList() { }

		public bool Contains(PathType type, string path) {
			foreach(FavoriteItem item in Items) {
				if(item.Type == type && item.Path == path) {
					return true;
				}
			}
			return false;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}

	public class FavoriteItem: ICloneable {
		public PathType Type { get; set; }
		public string Path { get; set; }
		public FavoriteItem(PathType type, string path) {
			Type = type;
			Path = path;
		}
		public FavoriteItem() { }

		public object Clone() {
			return MemberwiseClone();
		}
	}

	public class PathTypeException: Exception {
		public PathTypeException() : base("Path Type Error") { }
	}
}
