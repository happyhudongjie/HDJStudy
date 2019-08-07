using LitJson;
using System.IO;
using System.Text;
using Debug = UnityEngine.Debug;
using System;

namespace AssetPipeline{
	public class FileHelper  {

		public static void SaveJsonObj(object obj,string savePath,bool isCompress = false,bool prettyPrint = false){
			string json = JsonMapper.ToJson (obj, prettyPrint);
			SaveJsonText (json, savePath, isCompress);
		}

		public static void SaveJsonText(string json,string savePath,bool isCompress = false){
			if (isCompress) {
				WriteAllBytes (savePath, ZipLibUtils.Compress (Encoding.UTF8.GetBytes (json)));
			} else {
				WriteAllText (savePath, json);
			}
		}

		public static void WriteAllBytes(string path,byte[] bytes){
			string dir = Path.GetDirectoryName (path);
			CreateDirectore (dir);
			File.WriteAllBytes (path, bytes);
		}

		public static void WriteAllText(string path,string text){
			string dir = Path.GetDirectoryName (path);
			CreateDirectore (dir);
			File.WriteAllText (path, text);
		}

		public static void CreateDirectore(string path){
			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}
		}

		#region Json Func 仅用于框架代码
		public static T ReadJsonFile<T>(string path,bool isUncompress = false){
			try{
				if(isUncompress){
					var bytes = ZipLibUtils.Uncompress(File.ReadAllBytes(path));
					return JsonMapper.ToObject<T>(Encoding.UTF8.GetString(bytes));
				}else{
					return JsonMapper.ToObject<T>(File.ReadAllText(path));
				}
			}
			catch(Exception e){
				Debug.LogError (e.Message);
			}
			return default(T);
		}
		#endregion

		#region Sync Read File
		public static byte[] ReadAllBytes(string path){
			try{
				byte[] data = File.ReadAllBytes(path);
				return data;
			}
			catch(Exception e){
				Debug.LogError (e.Message);
			}
			return null;
		}

		public static string ReadAllText(string path){
			try{
				string data = File.ReadAllText(path);
				return data;
			}catch(Exception e){
				Debug.LogError (e.Message);
			}
			return null;
		}
		#endregion
	}
}

