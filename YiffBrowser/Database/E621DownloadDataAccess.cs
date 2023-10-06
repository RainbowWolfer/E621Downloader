﻿using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Database {
	public static class E621DownloadDataAccess {

		public const string DatabaseFileName = "PostsInfo.db";

		public static async ValueTask<StorageFile> CheckDatabase() {
			StorageFolder folder = Local.DownloadFolder;
			if (folder == null) {
				return null;
			}
			try {
				StorageFile file = await folder.GetFileAsync(DatabaseFileName);
				file ??= await CreateDatabase(folder);
				return file;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return await CreateDatabase(folder);
			}
		}

		public async static ValueTask<StorageFile> CreateDatabase(IStorageFolder folder) {
			StorageFile file = await folder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists);

			using SqliteConnection connection = await OpenConnection(file.Path);

			string tableCommand =
				"CREATE TABLE IF NOT EXISTS " +
				"PostsInfo(" +
					"PostID INTEGER PRIMARY KEY, " +
					"PostJson TEXT NULL" +
				")";

			SqliteCommand createTable = new(tableCommand, connection);

			createTable.ExecuteReader();

			return file;
		}

		public static async Task AddOrUpdatePost(E621Post post) {
			StorageFile file = await CheckDatabase();
			if (file == null) {
				return;
			}

			string json = JsonConvert.SerializeObject(post);

			using SqliteConnection connection = await OpenConnection(file.Path);

			SqliteCommand insertCommand = new() {
				Connection = connection,
				CommandText = "INSERT OR REPLACE INTO PostsInfo VALUES (@ID, @JSON);"
			};

			insertCommand.Parameters.AddWithValue("@ID", post.ID);
			insertCommand.Parameters.AddWithValue("@JSON", json);

			insertCommand.ExecuteReader();
		}

		public static async ValueTask<E621Post> GetPostInfo(int postID) {
			StorageFile file = await CheckDatabase();
			if (file == null) {
				return null;
			}

			using SqliteConnection connection = await OpenConnection(file.Path);

			SqliteCommand selectCommand = new($"SELECT PostID, PostJson FROM PostsInfo WHERE PostID = {postID};", connection);

			SqliteDataReader query = await selectCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);

			if (await query.ReadAsync()) {
				int id = query.GetInt32(0);
				string json = query.GetString(1);
				E621Post post = JsonConvert.DeserializeObject<E621Post>(json);
				return post;
			}

			return null;
		}


		public static async ValueTask<SqliteConnection> OpenConnection(string filePath) {
			SqliteConnection db = new($"Filename={filePath}");
			await db.OpenAsync();
			return db;
		}

	}
}
