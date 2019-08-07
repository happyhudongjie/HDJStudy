using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using System.IO;
using System.Text;

namespace AssetPipeline
{
	public class VersionConfig{
		
		public enum ServerType{
			Null = -1,
			Default,
			Beta,
		}

		//标记当前版本信息类型，用于区分游戏公测服、游戏审核服、游戏Beta服
		public ServerType serverType = ServerType.Default;
	}

	public enum CompressType
	{
		Raw = 0,
		UnityLZMA = 1,
		UnityLZ4 = 2,
		CustomZip = 10,
		CustomLZMA = 11,
		CustomLZ4 = 12,
		CustomTex = 13,
	}

	public class ResConfig{

		public VersionConfig.ServerType servertType = VersionConfig.ServerType.Default;
		//记录本次打包版本号，版本号根据上一个版本进行递增
		public long Version;
		//记录本次打包资源的资源清单CRC值
		public uint lz4CRC;
		public uint lzmaCRC;
		public uint tileTexCRC;
		//记录该版本资源打包时间
		public long BuildTime;
		//资源压缩类型
		public CompressType compressType;
		//记录了AssetBundle文件的总大小（单位： byte）
		public long TotalFileSize;
		//标记当前资源是否是小包，从小包升级为整包后改标记置为false
		public bool isMiniRes;
		//以资源名_ResType为Key
		public Dictionary<string,ResInfo> Manifest;

		public ResConfig(){
			compressType = CompressType.UnityLZ4;
			Manifest = new Dictionary<string, ResInfo> ();
		}

		public ResInfo GetResInfo(string key){
			if (Manifest.ContainsKey (key))
				return Manifest [key];
			return null;
		}

		public string ToFileName(){
			return "resConfig_"+Version+".json";
		}

		public string ToRemoteName(){
			return "resConfig_"+Version+".tz";
		}

		public string GetRemoteFile(long version){
			return "resConfig_"+version+".tz";
		}


		public void SaveFile(string path,bool compress){
			byte[] fileBytes = SerializeToMemoryStream ();
			if (compress) {
				byte[] bytes = ZipLibUtils.Compress (fileBytes);
			} else {
				FileHelper.WriteAllBytes (path, fileBytes);
			}
		}

		internal byte[] SerializeToMemoryStream(){
			using (var memoryStream = new MemoryStream ()) {
				using (var binaryWriter = new BinaryWriter (memoryStream, Encoding.UTF8)) {
					string ver = "verl";
					binaryWriter.Write (ver);
					binaryWriter.Write (Version);
					binaryWriter.Write (lz4CRC);
					binaryWriter.Write (lzmaCRC);
					binaryWriter.Write (tileTexCRC);
					binaryWriter.Write (BuildTime);
					binaryWriter.Write ((int)compressType);
					binaryWriter.Write (TotalFileSize);
					binaryWriter.Write (isMiniRes);
					binaryWriter.Write (Manifest.Count);

					foreach (KeyValuePair<string,ResInfo> keyValuePair in Manifest) {
						ResInfo info = keyValuePair.Value;
						binaryWriter.Write (info.bundleName);
						binaryWriter.Write (info.CRC);
						binaryWriter.Write (info.Hash ?? string.Empty);
						binaryWriter.Write ((int)info.remoteZipType);
						binaryWriter.Write (info.MD5);
						binaryWriter.Write (info.size);
						binaryWriter.Write (info.isPackageRes);
						binaryWriter.Write (info.preload);
						binaryWriter.Write (info.Dependencies.Count);
						for (int j = 0; j < info.Dependencies.Count; j++) {
							binaryWriter.Write (info.Dependencies [j]);
						}
					}
					return memoryStream.ToArray ();
				}
			}
		}

		//检查依赖自引用
		public void CheckSelfDependencied(){
			foreach (var item in Manifest) {
				string name = item.Key;
				ResInfo resInfo = item.Value;
				List<string> list = new List<string> ();
				for (int i = 0; i < resInfo.Dependencies.Count; i++) {
					string bundleName = resInfo.Dependencies [i];
					GetDependenciesRecursive (bundleName, ref list);
				}
				foreach (string dependName in list) {
					if (dependName == name) {
						StringBuilder sb = new StringBuilder ("检查Assetbundle引用错误： " + name);
						sb.AppendLine ();
						sb.Append (name);
						foreach (string str in list) {
							sb.Append ("-> " + str);
						}
						Debug.LogError (sb);
						break;
					}
				}
			}
		}

		private void GetDependenciesRecursive(string bundleName,ref List<string> dependencies){
			var resInfo = GetResInfo (bundleName);
			if (resInfo != null) {
				foreach (string dependency in resInfo.Dependencies) {
					if(!dependencies.Contains(dependency))//防止循环依赖
					{
						dependencies.Add (dependency);
						GetDependenciesRecursive (bundleName,ref dependencies);
					}
				}
			}
		}
	}	

	public class ResInfo {

		//项目内bundle名字
		public string bundleName;
		//当前资源包CRC值
		//注：相同资源使用不同的压缩方式打包时，计算出的CRC是一样的
		public uint CRC;
		//当前资源包Hash128值
		//注：如果现在打包Android资源，但是贴图的PC平台导入配置修改了，会导致Hash变化，但是打包出来的CRC是一样的
		//简单来说就是Hash变了，CRC可能不变，但Hash不变，CRC也不会变
		public string Hash;
		//标记Bundle文件放在CDN上的压缩类型，不是指打包Bundle时的压缩类型
		//如果是使用LZ4或不压缩方式打包资源，需要再用Zip压缩一遍，上传给CDN，这样可以有效减少用户的下载数据总量
		public CompressType remoteZipType;
		//记录资源包文件MD5值（压缩后）
		public string MD5;
		//记录资源包文件大小（压缩后）
		public long size;
		//标记该资源为包内资源，小包或者更新过的资源都将设置为false
		public bool isPackageRes;
		//标记该资源包是否需要预加载
		public bool preload;
		//当前资源包依赖资源包key列表
		public List<string> Dependencies;

		private const string Extension = ".ab";

		public ResInfo(){
			remoteZipType = CompressType.UnityLZ4;
			Dependencies = new List<string> ();
		}

		public string GetABPath(string dir,bool withExtension = false){
			return string.Format ("{0}/{1}_{2}{3}", dir, bundleName, CRC, Extension);
		}

		public string GetExportPath(string dir){
			return dir + "/" + bundleName;
		}

		public string GetManifestPath(string dir){
			if (remoteZipType == CompressType.CustomTex)
				return dir + "/" + Path.ChangeExtension (bundleName, ".json");
			else
				return dir + "/" + bundleName + ".manifest";
		}

		public string GetRemotePath(string dir){
			if (remoteZipType == CompressType.CustomZip)
				return GetABPath (dir) + ".zip";
			return GetABPath (dir);
		}

		public void MakeMiniRes(){
			Hash = null;
		}

	}

	/// <summary>
	/// Unity打包bundle后生成的总资源清单YAML文件对应数据类
	/// </summary>
	public class RawAssetManifest{
		public int ManifestFileVersion{ get; set;}
		public uint CRC{ get; set;}

		[YamlMember(Alias = "AssetBundleManifest")]
		public RawBundleManifest Manifest{get;set;}

		public class RawBundleManifest{	
			public Dictionary<string,RawBundleInfo> AssetBundleInfos{ get; set;}

			public class RawBundleInfo{
				public string Name{ get; set;}
				public Dictionary<string,string> Dependencies { get; set;}
			}
		}
	}

	/// <summary>
	/// Unity打包Bundle后每个Bundle对应YAML文件的数据类
	/// </summary>
	public class RawBundleManifest{
		public int ManifestFileVersion{ get; set;}
		public uint CRC{ get; set;}
		public Dictionary<string,HashInfo> Hashes{get;set;}
		public int HashAppended{ get; set;}
		public List<object> ClassTypes{ get; set;}
		public List<string> Assets{get;set;}
		public List<string> Dependencies{ get; set;}

		public class HashInfo{
			public int serializedVersion{ get; set;}
			public string Hash{ get; set;}
		}
	}

	public class DllVersion{
		public long Version;
		public Dictionary<string,DllInfo> Manifest;
		public VersionConfig.ServerType serverType = VersionConfig.ServerType.Default;

		public DllVersion(){
			Manifest = new Dictionary<string, DllInfo> ();
		}

		public string ToFileName(){
			return "dllVersion_" + Version + ".json";
		}

		public static string GetFileName(long version){
			return "dllVersion_" + version + ".json";
		}
	}

	public class DllInfo{
		public string dllName;
		public string MD5;
		public long size;

		public string ToFileName(){
			return dllName + "_" + MD5 + ".dll";
		}
	}

	/// <summary>
	/// 小包资源配置，标记了ResConfig中哪些资源为包内资源，哪些资源为游戏时下载资源
	/// </summary>
	public class MiniResConfig{
		//存放小包缺失资源的Key,以及其替代资源的信息
		public Dictionary<string,string> replaceResConfig = new Dictionary<string, string>();
	}

}