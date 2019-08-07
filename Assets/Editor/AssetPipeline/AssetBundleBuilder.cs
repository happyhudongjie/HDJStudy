using System.Text;
using System;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace AssetPipeline{

	/// <summary>
	/// 自动设置资源BundleName
	/// </summary>
	public class GameResPostprocessor : AssetPostprocessor {

		private static readonly StringBuilder sb = new StringBuilder();
		public static void OnPostprocessAllAssets(string[] importedAssets,string[] deletedAssets,string[] moveAssets,string[] movedFromAssetPaths){
			sb.Length = 0;
			var updateAction = new Action<string> (s => 
			{
				sb.AppendLine(string.Format("Update AssetsBundle {1}:{0}",s,AssetImporter.GetAtPath(s).assetBundleName));
			});

			foreach (var assetPath in importedAssets) {
				if (assetPath.IsCommonAsset ()) {
					AssetBundleNameLogic.UpdateCommon (assetPath, updateAction);
				} 
				else if (assetPath.IsPrefabFile ()) {
					if (assetPath.IsUIPrefab ()) {
						AssetBundleNameLogic.UpdateUIPrefab (assetPath, updateAction);
					} else if (assetPath.IsUIAtals ()) {
						AssetBundleNameLogic.UpdateUIAtlas (assetPath, updateAction);
					}
				}
			}
		}
	}

	public class AssetBundleBuilder:EditorWindow{
		public static AssetBundleBuilder Instance;

		public ResConfig _oldResConfig;
		public ResConfig _curResConfig;
		private BuildBundleStrategy _buildBundleStrategy;

		private const long MaxVersion = 999999999999;

		[MenuItem("GameResource/AssetBundleBuilder #&e")]
		public static void ShowWindow(){
			if (Instance == null) {
				var window = GetWindow<AssetBundleBuilder> (false, "AssetBundleBuilder", true);
				window.minSize = new Vector2 (872f, 680f);
				window.Show ();
			} else {
				Instance.Close ();
			}
		}

		//当前项目里BundleName分组信息
		private static Dictionary<ResGroup,List<string>> _projectBundleNameGroups;
		//当前项目里未使用的BundleName集合
		private static HashSet<string> _unusedBundleNameSet;
		//当前项目里BundleName总数
		private static int _projectBundleNameTotalCount;
		//当前版本资源配置BundleName分组信息
		private Dictionary<ResGroup,List<string>> _manifestBundleNameGroups;

		private static void RefreshBundleNameData(){
			if (_projectBundleNameGroups == null) {
				_projectBundleNameGroups = new Dictionary<ResGroup, List<string>> ();
				var resGroupEnums = Enum.GetValues (typeof(ResGroup));
				foreach (ResGroup resGroup in resGroupEnums) {
					_projectBundleNameGroups.Add (resGroup, new List<string> ());
				}
			} else {
				foreach (var resGroupList in _projectBundleNameGroups.Values) {
					resGroupList.Clear ();
				}
			}

			var unusedBundleNames = AssetDatabase.GetUnusedAssetBundleNames ();
			_unusedBundleNameSet = new HashSet<string> (unusedBundleNames);

			var bundleNames = AssetDatabase.GetAllAssetBundleNames ();
			_projectBundleNameTotalCount = bundleNames.Length;

			foreach (var bundleName in bundleNames) {
				var resGroup = GetResGroupFromBundleName (bundleName);
				_projectBundleNameGroups [resGroup].Add (bundleName);
			}

		}

		private static ResGroup GetResGroupFromBundleName(string bundleName){
			if (!bundleName.Contains ("/"))
				return ResGroup.None;
			var resGroupEnums = Enum.GetValues (typeof(ResGroup));
			return resGroupEnums.Cast<ResGroup> ().FirstOrDefault (resGroup => bundleName.StartsWith (resGroup.ToString ().ToLower ()));
		}

		private void RefreshReConfigData(ResConfig resConfig){
			_curResConfig = resConfig;
			if (_manifestBundleNameGroups == null) {
				_manifestBundleNameGroups = new Dictionary<ResGroup, List<string>> ();
				var resGroupEnums = Enum.GetValues (typeof(ResGroup));
				foreach (ResGroup resGroup in resGroupEnums) {
					_manifestBundleNameGroups.Add (resGroup, new List<string> ());
				}
			} else {
				foreach (var resGroupList in _manifestBundleNameGroups.Values) {
					resGroupList.Clear ();
				}
			}

			if (_curResConfig != null) {
				foreach (var pair in _curResConfig.Manifest) {
					var resGroup = GetResGroupFromBundleName (pair.Key);
					_manifestBundleNameGroups [resGroup].Add (pair.Key);
				}
			}
		}

		#region Edit UI

		private bool _showProgess = true;
		private Vector2 _leftContentScroll;
		public bool _slientMode = false;	//静默下载
		//标识哪些资源分组需要重新更新BundleName
		private UpdateBundleFlag _updateResGroupMask = UpdateBundleFlag.Everything;

		[Flags]
		public enum UpdateBundleFlag
		{
			Everything = -1,
			Nothing = 0,
			UI = 2,
			Model = 4,
			Effect = 8,
			Scene = 16,
			Audio = 32,
			Config = 64,
			Script =128,
			StreamScene = 0x01 << 8,
			RawScene = 0x01 << 9
		}

		private void OnGUI(){
			EditorGUILayout.Space ();
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (10f);
			EditorGUILayout.BeginVertical (GUILayout.Width (430f));
			_leftContentScroll = EditorGUILayout.BeginScrollView (_leftContentScroll);
			{
				//打包选项
				EditorGUILayout.BeginVertical ("HelpBox");
				GUILayout.Label ("打包", "BoldLabel");
				_updateResGroupMask = (UpdateBundleFlag)EditorGUILayout.EnumMaskField ("ResGroup", _updateResGroupMask);
				if (GUILayout.Button ("更新所有BundleName", "LargeButton", GUILayout.Height (50f))) {
					if (_updateResGroupMask == UpdateBundleFlag.Nothing)
						return;
					if (EditorUtility.DisplayDialog ("确认", "是否重新设置所有资源BundleName?", "继续", "取消"))
					{
						EditorApplication.delayCall += delegate {
							Debug.Log ("设置所有资源bundleName");
							EditorApplication.delayCall += UpdateAllBundleName;
						};
					}
				}
				EditorGUILayout.Space ();

				if (GUILayout.Button ("清空所有BundleName", "LargeButton", GUILayout.Height (50f))) {
					int option = EditorUtility.DisplayDialogComplex ("确认","是否清空所有资源BundleName?","全部清空","Cancel","清空未使用的");
					if (option != 1) {
						EditorApplication.delayCall += () => {
							CleanUpBundleName(option == 0);
						};
					}
				}
				EditorGUILayout.Space ();

				GUI.color = Color.red;
				if (GUILayout.Button ("一键打包新版本整包资源（不更新BundleName）", "LargeButton", GUILayout.Height (50f))) {

					long nextVer = 0;
					if (_curResConfig == null) {
						string filePath = EditorUtility.OpenFilePanel ("加载版本资源配置信息", GetResConfigRoot(), "json");
						var match = Regex.Match (filePath, @"resConfig_(\d+)");
						if (match.Success && match.Groups.Count > 0) {
							nextVer = long.Parse (match.Groups [1].Value) + 1;
						}
					} else {
						nextVer = _curResConfig.Version + 1;
					}
					string tip = nextVer == 0
						? "当前版本ResConfig为空，资源版本号将归0，请确认？"
						: "本次打包资源版本号为： " + nextVer;
					if (EditorUtility.DisplayDialog ("确认", tip, "继续", "取消")) {

						bool generateTotalRes = EditorUtility.DisplayDialog ("生成包内资源", "是否生成包内资源？", "生成", "不生成");
						EditorApplication.delayCall += () => {
							BuildAll(nextVer,generateTotalRes);
						};
					}
				}
				EditorGUILayout.EndVertical ();
			}
			GUI.color = Color.red;
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
		}
		#endregion

		#region Build AssetBundle

		private void BuildAll(long nextVer,bool generateTotalRes = false){
			var stopWatch = Stopwatch.StartNew ();
			var exportDir = GetExportBundlePath ();

			//根据已设置好的BundleName信息生成AssetBundleBuild列表
			List<AssetBundleBuild> lz4ResList;
			List<AssetBundleBuild> lzmaResList;

			var lz4Options = BuildAssetBundleOptions.ChunkBasedCompression;
			var lzmaOptions = BuildAssetBundleOptions.None;

			GenerateAssetBundleBuildList (out lz4ResList, out lzmaResList);
			//先打包UI资源，使用LZ4压缩方式打包
			BuildBundles(exportDir+"/lz4",lz4ResList,lz4Options);
			//打包其他资源，使用LZMA压缩方式打包
			BuildBundles(exportDir+"/lzma",lzmaResList,lzmaOptions);
			uint allTexCRC32 = 0;
			bool skip;
			var newResConfig = GenerateResConfig (exportDir, nextVer, allTexCRC32, out skip);
			if (newResConfig != null) {
				BackupAssetBundle (newResConfig, exportDir);
			} else if (_slientMode && !skip) {
				throw new SystemException ("生成 newResConfig失败");
			}
			if(generateTotalRes){
				GenerateTotalRes ();
			}
			stopWatch.Stop ();
			var elapsed = stopWatch.Elapsed;
			if (!_slientMode) {
				//延迟一帧，不影响编译
				EditorApplication.delayCall += () => {
					EditorUtility.DisplayDialog ("提示",
						string.Format ("打包项目资源总耗时:{0:00}:{1:00}:{2:00}:{3:00}\n", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10), "OK");
				};
			} else {
				Debug.Log (string.Format ("打包项目资源总耗时:{0:00}:{1:00}:{2:00}:{3:00}\n", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10));
			}

		}

		private void BackupAssetBundle(ResConfig newResConfig,string exportDir){
			//打包资源完毕，备份当前版本资源到gameres_{CRC}目录
			try{
				string lz4ExportRoot = exportDir+"/lz4";
				string lzmaExportRoot = exportDir+"/lzma";

				if(newResConfig != null){
					var backupDir = GetBackupDir(newResConfig);
					//先删除之前已存在的资源目录
					if(FileUtil.DeleteFileOrDirectory(backupDir)){
						Debug.Log("旧版本资源目录已存在，将清空后重新备份： "+backupDir);
					}
					string lz4BackupRoot = backupDir+"/lz4";
					string lzmaBackupRoot = backupDir+"/lzma";
					string tileMapBackupRoot = backupDir+"/custom";
					Directory.CreateDirectory(lz4BackupRoot);
					Directory.CreateDirectory(lzmaBackupRoot);

					//先备份AssetBundleManifest的信息
					FileUtil.CopyFileOrDirectory(lz4ExportRoot+"/lz4",lz4BackupRoot+"/lz4");
					FileUtil.CopyFileOrDirectory(lz4ExportRoot+"/lz4.manifest",lz4BackupRoot+"/lz4.manifest");

					FileUtil.CopyFileOrDirectory(lzmaExportRoot+"/lzma",lzmaBackupRoot+"/lzma");
					FileUtil.CopyFileOrDirectory(lzmaExportRoot+"/lzma.manifest",lzmaBackupRoot+"/lzma.manifest");

					int finishedCount = 0;
					//备份该版本资源的Bundle以及manifest文件到备份目录
					foreach(var resInfo in newResConfig.Manifest.Values){
						string bundleExportDir = GetBundleBackupDir(resInfo,exportDir);
						string bundleBackupDir = GetBundleBackupDir(resInfo,backupDir);

						var bundleFileInfo = new FileInfo(resInfo.GetExportPath(bundleExportDir));
						var bundleManifest = resInfo.GetManifestPath(bundleExportDir);
						if(bundleFileInfo.Exists && File.Exists(bundleManifest)){
							var backupBundlePath =resInfo.GetABPath(bundleBackupDir);
							var backupBundleManifest = resInfo.GetManifestPath(bundleBackupDir);
							Directory.CreateDirectory(Path.GetDirectoryName(backupBundlePath));

							FileUtil.CopyFileOrDirectory(bundleFileInfo.FullName,backupBundlePath);
							FileUtil.CopyFileOrDirectory(bundleManifest,backupBundleManifest);

							//对于压缩类型为CompressType.CustomZip进行压缩备份
							if(resInfo.remoteZipType == CompressType.CustomZip){
								var exportZipPath = resInfo.GetRemotePath(bundleExportDir);
								if(!File.Exists(exportZipPath))
									ZipManager.CompressFile(bundleFileInfo.FullName,exportZipPath);

								var zipFileInfo = new FileInfo(exportZipPath);
								if(zipFileInfo.Exists){
									var backupZipPath = resInfo.GetRemotePath(bundleBackupDir);
									FileUtil.CopyFileOrDirectory(exportZipPath,backupZipPath);
									resInfo.MD5 = MD5Hashing.HashFile(backupZipPath);
									resInfo.size = zipFileInfo.Length;
								}else{
									throw new Exception("压缩Bundle异常，请检查： "+bundleFileInfo.FullName);
								}
							}else{
								resInfo.MD5 = MD5Hashing.HashFile(backupBundlePath);
								resInfo.size = bundleFileInfo.Length;
							}
							//统计压缩后该版本资源的总大小
							newResConfig.TotalFileSize += resInfo.size;
						}
						else{
							throw new Exception(string.Format("打包异常，在打包目录找不到该文件或其Manifest文件： {0} \n ManifestPath:{1} \n bundlePath: {2}"
								,resInfo.bundleName,bundleManifest,bundleFileInfo.FullName));
						}

						finishedCount+=1;
						if(_showProgess){
							EditorUtility.DisplayProgressBar("备份AssetBundle中",string.Format("{0} / {1} ",finishedCount,newResConfig.Manifest.Count),finishedCount/(float)newResConfig.Manifest.Count);
						}
					}


					//备份完该版本Bundle资源，保存newResConfig信息
					string jsonPath = GetResConfigRoot() +"/"+newResConfig.ToFileName();
					FileHelper.SaveJsonObj (newResConfig, jsonPath, false, true);
					string jzPath = GetResConfigRoot () + "/" + newResConfig.ToRemoteName ();
					newResConfig.SaveFile (jzPath, true);
					newResConfig.CheckSelfDependencied ();
					RefreshReConfigData (newResConfig);
					EditorPrefs.SetString (GetLastResConfigPathHash (), jsonPath);
				}else{
					throw new Exception("curResConfig is null");
				}
			}catch(Exception e){
				if (_slientMode)
					throw;
				Debug.LogException (e);
				EditorUtility.DisplayDialog ("提示", "备份当前版本资源失败，详情查看log!!!", "OK");
			}

			if (_showProgess)
				EditorUtility.ClearProgressBar ();
		}

		private void GenerateAssetBundleBuildList(out List<AssetBundleBuild> lz4ResList,out List<AssetBundleBuild> lzmaResList){
			lz4ResList = new List<AssetBundleBuild> ();
			lzmaResList = new List<AssetBundleBuild> ();

			foreach (var pair in _projectBundleNameGroups) {
				var resGroup = pair.Key;
				if (resGroup == ResGroup.TileMap)
					continue;
				if (resGroup == ResGroup.Common) {
					var bundleNames = pair.Value;
					foreach (var bundleName in bundleNames) {
						var abb = new AssetBundleBuild {
							assetBundleName = bundleName,
							assetNames = AssetDatabase.GetAssetPathsFromAssetBundle (bundleName)
						};
						//lz4和lzma里面都会有依赖common里面的资源，所以两种压缩资源都需要加载common部分
						lz4ResList.Add (abb);
						lzmaResList.Add (abb);
					}
				} else if (resGroup == ResGroup.UIPrefab
				        || resGroup == ResGroup.UIAtlas
				        || resGroup == ResGroup.UIFont
				        || resGroup == ResGroup.UITexture
				        || resGroup == ResGroup.Image
				        || resGroup == ResGroup.PlotCameraPath
				        || resGroup == ResGroup.StreamScene
				        || resGroup == ResGroup.RawScene) {
					var bundleNames = pair.Value;
					foreach (var bundleName in bundleNames) {
						var abb = new AssetBundleBuild {
							assetBundleName = bundleName,
							assetNames = AssetDatabase.GetAssetPathsFromAssetBundle (bundleName)
						};
						lz4ResList.Add (abb);
					}
				} else {
					var bundleNames = pair.Value;
					foreach (var bundleName in bundleNames) {
						var abb = new AssetBundleBuild {
							assetBundleName = bundleName,
							assetNames = AssetDatabase.GetAssetPathsFromAssetBundle (bundleName)
						};
						lzmaResList.Add (abb);
					}
				}
			}
		}

		private void BuildBundles(string exportDir,List<AssetBundleBuild> abbList,BuildAssetBundleOptions buildOptions){
			CreateDirectory (exportDir);
			if (abbList.Count > 0) {
				var mainfest = BuildPipeline.BuildAssetBundles (exportDir, abbList.ToArray (), buildOptions, EditorUserBuildSettings.activeBuildTarget);
				if (mainfest == null) {
					throw new Exception ("BuildAassetBundles Error");
				}
			}
		}

		private void CreateDirectory(string dir){
			if (!Directory.Exists (dir)) {
				Directory.CreateDirectory (dir);
			}
		}

		/// <summary>
		/// 根据unity打包生成的manifest信息生成自定义的ResConfig信息
		/// </summary>
		private ResConfig GenerateResConfig(string exportDir,long nextVer,uint allTexCRC32,out bool skip){
		
			skip = false;
			string lz4Root = exportDir + "/lz4";
			string lzmaRoot = exportDir + "/lzma";
			string lz4ManifestPath = lz4Root + "/lz4.manifest";
			var lz4Manifest = LoadYAMLObj<RawAssetManifest> (lz4ManifestPath);
			if (lz4Manifest == null) {
				Debug.LogError ("解析Manifest文件失败： " + lz4ManifestPath);
				return null;
			}

			string lzmaManifestPath = lzmaRoot + "/lzma.manifest";
			var lzmaManifest = LoadYAMLObj<RawAssetManifest>(lzmaManifestPath);
			if (lzmaManifest == null) {
				Debug.LogError ("解析Manifest文件失败： " + lzmaManifestPath);
				return null;
			}

			//此次打包的资源ManifestCRC与上个版本一致时，询问用户是否跳过
			skip = _curResConfig != null && _curResConfig.lzmaCRC == lzmaManifest.CRC && _curResConfig.lz4CRC == lz4Manifest.CRC && _curResConfig.tileTexCRC == allTexCRC32;
			if (skip && (_slientMode || EditorUtility.DisplayDialog ("提示", "本次打包资源ManifestCRC与上次一致，是否跳过？", "跳过", "备份"))) {
				return null;
			}

			var newResConfig = new ResConfig {
				Version = nextVer,
				lz4CRC = lz4Manifest.CRC,
				lzmaCRC = lzmaManifest.CRC,
				tileTexCRC = allTexCRC32,
				BuildTime = DateTimeToUnixTimestamp(DateTime.Now)
			};

			//生成UI资源与Common资源ResInfo的信息
			foreach(var bundleInfo in lz4Manifest.Manifest.AssetBundleInfos.Values){
				string bundleName = bundleInfo.Name;
				var bundleManifest = LoadYAMLObj<RawBundleManifest>(lz4Root+"/"+bundleName+".manifest");
				if (bundleManifest != null) {
					var resInfo = new ResInfo {
						bundleName = bundleName,
						CRC = bundleManifest.CRC,
						Hash = bundleManifest.Hashes ["AssetFileHash"].Hash
					};
					UpdatePreloadFlag (resInfo);
					UpdateRemoteZipType (resInfo);

					foreach (var dependency in bundleInfo.Dependencies.Values) {
						if (string.IsNullOrEmpty (dependency)) {
							Debug.LogError (bundleInfo.Name);
						}
						//无需记录common类的资源依赖，因为这部分资源加载了就不释放了
						if (dependency.StartsWith ("common/"))
							continue;
						resInfo.Dependencies.Add (dependency);
					}
					newResConfig.Manifest.Add (bundleName, resInfo);
				} else {
					Debug.LogError ("解析BundleManifest文件失败： " + bundleInfo.Name);
				}
			}

			//生成其他资源ResInfo信息
			foreach(var bundleInfo in lzmaManifest.Manifest.AssetBundleInfos.Values){
				string bundleName = bundleInfo.Name;
				if (bundleName.StartsWith ("common/"))
					continue;
				var bundleManifest = LoadYAMLObj<RawBundleManifest> (lzmaRoot + "/" + bundleName + ".manifest");
				if (bundleManifest != null) {

					var resInfo = new ResInfo {
						bundleName = bundleName,
						CRC = bundleManifest.CRC,
						Hash = bundleManifest.Hashes ["AssetFileHash"].Hash
					};
					UpdatePreloadFlag (resInfo);
					UpdateRemoteZipType (resInfo);
					foreach (var dependency in bundleInfo.Dependencies.Values) {
						//无需记录common类的资源依赖，因为这部分资源加载了就不释放了
						if (dependency.StartsWith ("common/"))
							continue;
						resInfo.Dependencies.Add (dependency);
					}
					newResConfig.Manifest.Add (bundleName, resInfo);
				} else {
					Debug.LogError ("解析BundleManifest文件失败： " + bundleInfo.Name);
					return null;
				}
			}
			return newResConfig;
		}

		//更新标记
		private void UpdatePreloadFlag(ResInfo resInfo){
			/*var resGroup = GetResGroupFromBundleName (resInfo.bundleName);
			if (resGroup == ResGroup.Common) {
				resInfo.preload = true;
			}
			else if(_buildBundleStrategy.preloadConfig.ContainsKey(resInfo.bundleName)){
				resInfo.preload = true;
			}*/
		}

		/// <summary>
		/// 默认为CustomZip,实际测试发现部分资源压缩后变化不大，所以不采用Zip压缩方式
		/// </summary>
		/// <param name="resInfo">Res info.</param>
		private void UpdateRemoteZipType(ResInfo resInfo){
			var resGroup = GetResGroupFromBundleName (resInfo.bundleName);

			if (resGroup == ResGroup.UIPrefab
			    || resGroup == ResGroup.UIAtlas
			    || resGroup == ResGroup.UIFont
			    || resGroup == ResGroup.UITexture
			    || resGroup == ResGroup.Image
			    || resGroup == ResGroup.Common
			    || resGroup == ResGroup.StreamScene
			    || resGroup == ResGroup.RawScene
			    || resGroup == ResGroup.PlotCameraPath) {
			
				resInfo.remoteZipType = CompressType.UnityLZ4;
			} else if (resGroup == ResGroup.TileMap) {
				resInfo.remoteZipType = CompressType.CustomTex;
			} else {
				resInfo.remoteZipType = CompressType.UnityLZMA;
			}
		}
		public T LoadYAMLObj<T>(string path){
			using (var sr = new StreamReader (path)) {
				var yamlParser = new YamlDotNet.Serialization.DeserializerBuilder ();
				return yamlParser.Build ().Deserialize<T> (sr);
			}
		}

		private long DateTimeToUnixTimestamp(DateTime dateTime){
			return (dateTime - new DateTime (1970, 1, 1, 0, 0, 0, 0).ToLocalTime ()).Ticks / 10000;
		}
		#endregion

		private string[] ClearPath = new[] {"Asset/HDJStudy/AssetBundleBuilder/UI"};
		private readonly StringBuilder _bundleNameLogger = new StringBuilder();
		private int _changeCount;

		//更新所有的资源ab名kan 
		private void UpdateAllBundleName(){
			ClearBundleName ();
			_bundleNameLogger.Length = 0;
			var stopWatch = Stopwatch.StartNew ();
			AssetDatabase.RemoveUnusedAssetBundleNames ();

			var uiCount = (_updateResGroupMask & UpdateBundleFlag.UI) != UpdateBundleFlag.UI ? -1 : UpdateUI();
			var modelCount = (_updateResGroupMask & UpdateBundleFlag.Model) != UpdateBundleFlag.Model ? -1 : UpdateModel ();
			var effCount= (_updateResGroupMask & UpdateBundleFlag.Effect) != UpdateBundleFlag.Effect ? -1 : UpdateEffect ();
			var sceneCount =  (_updateResGroupMask & UpdateBundleFlag.Scene) != UpdateBundleFlag.Scene ? -1 : UpdateScene ();
			var audioCount =  (_updateResGroupMask & UpdateBundleFlag.Audio) != UpdateBundleFlag.Audio ? -1 : UpdateAudio ();
			var configCount = (_updateResGroupMask & UpdateBundleFlag.Config) != UpdateBundleFlag.Config ? -1 : UpdateConfig ();
			var scriptCount = (_updateResGroupMask & UpdateBundleFlag.Script) != UpdateBundleFlag.Script ? -1 : UpdateScript ();
			var streamSceneCount = (_updateResGroupMask & UpdateBundleFlag.StreamScene) != UpdateBundleFlag.StreamScene ? -1 : UpdateStreamScene ();
			var rawSceneCount =  (_updateResGroupMask & UpdateBundleFlag.RawScene) != UpdateBundleFlag.RawScene ? -1 : UpdateRawScene ();
			//最后更新公共资源的bundleName,防止被前面流程覆盖掉
			var commonCount=  UpdateCommon();

			stopWatch.Stop ();
			var elapsed = stopWatch.Elapsed;
			var tips = string.Format ("资源BundleName变更数量\n-1表示跳过该组资源检查\nCommon: {0}\nUI: {1}: {1}\nModel: {2}\nEffect: {3}\nScene: {4}\nAudio: {5}\nConfig: {6}\nScript: {7}\nStreamScene: {8}\nRawScene: {9}",
				           commonCount, uiCount, modelCount, effCount, sceneCount, audioCount, configCount, scriptCount, streamSceneCount, rawSceneCount);
			string desc = string.Format ("更新项目资源的BundleName总耗时：{0:00}:{1:00}:{2:00}:{3:00}\n", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10) + tips;
			Debug.Log (desc);
			if (_bundleNameLogger.Length > 0) {
				Debug.Log (_bundleNameLogger);
			}
			AssetDatabase.Refresh ();
			RefreshBundleNameData ();
		}

		private void ClearBundleName(){
			string[] guids = AssetDatabase.FindAssets ("",ClearPath);
			for (int i = 0, imax = guids.Length; i < imax; i++) {
				var guid = guids [i];
				string path = AssetDatabase.GUIDToAssetPath (guid);
				AssetImporter assetImporter = AssetImporter.GetAtPath (path);
				assetImporter.assetBundleName = string.Empty;
			}
		
		}
		private int UpdateUI(){
			var updateAction = UpdateAssetBundleNameAction ();

			var GUIDs = AssetDatabase.FindAssets ("t:Prefab", BuildBundlePath.UIAtlasFolder);
			for (int i = 0, imax = GUIDs.Length; i < imax; i++) 
			{
				string resPath = AssetDatabase.GUIDToAssetPath (GUIDs[i]);
				AssetBundleNameLogic.UpdateUIAtlas (resPath, updateAction);

				if (_showProgess)
					EditorUtility.DisplayProgressBar ("处理UIAtlas中", string.Format (" {0} / {1} ", i, GUIDs.Length), i / (float)GUIDs.Length);
			}

			GUIDs = AssetDatabase.FindAssets ("t:Prefab",BuildBundlePath.UIFontFolder);
			for (int i = 0, imax = GUIDs.Length; i < imax; i++) 
			{
				string resPath = AssetDatabase.GUIDToAssetPath (GUIDs[i]);
				AssetBundleNameLogic.UpdateUIAtlas (resPath, updateAction);

				if (_showProgess)
					EditorUtility.DisplayProgressBar ("处理UIFont中", string.Format (" {0} / {1} ", i, GUIDs.Length), i / (float)GUIDs.Length);
			}

			GUIDs = AssetDatabase.FindAssets ("t:Prefab",BuildBundlePath.UIPrefabFolder);
			for (int i = 0, imax = GUIDs.Length; i < imax; i++) 
			{
				string resPath = AssetDatabase.GUIDToAssetPath (GUIDs[i]);
				AssetBundleNameLogic.UpdateUIPrefab (resPath, updateAction);

				if (_showProgess)
					EditorUtility.DisplayProgressBar ("处理UIPrefab中", string.Format (" {0} / {1} ", i, GUIDs.Length), i / (float)GUIDs.Length);
			}

			int index = 0;
			//代码动态加载的图片列表
			foreach(var imagePath in BuildBundlePath.ImageFilePath){
				AssetBundleNameLogic.UpdateImage (imagePath, updateAction);

				if(_showProgess)
					EditorUtility.DisplayProgressBar ("处理Image资源中", string.Format (" {0} / {1} ", index,BuildBundlePath.ImageFilePath.Count), index / (float)BuildBundlePath.ImageFilePath.Count);
			}

			if (_showProgess)
				EditorUtility.ClearProgressBar ();
			return _changeCount;
		}

		private int UpdateModel(){
			return _changeCount;
		}

		private int UpdateEffect(){
			return _changeCount;
		}

		private int UpdateScene(){
			return _changeCount;
		}

		private int UpdateAudio(){
			return _changeCount;
		}

		private int UpdateConfig(){
			return _changeCount;
		}

		private int UpdateScript(){
			return _changeCount;
		}

		private int UpdateStreamScene(){
			return _changeCount;
		}

		private int UpdateRawScene(){
			return _changeCount;
		}

		/// <summary>
		/// Common类型资源，开始游戏钱全部加载进游戏中
		/// </summary>
		/// <returns>The common.</returns>
		private int UpdateCommon(){
			var updateAction = UpdateAssetBundleNameAction ();
			foreach (string filePath in BuildBundlePath.CommondFilePath) {
				AssetBundleNameLogic.UpdateCommon (filePath, updateAction);
			}
			return _changeCount;
		}

		private void CleanUpBundleName(bool cleanAll){
			AssetDatabase.RemoveUnusedAssetBundleNames ();
			if(cleanAll){
				var allBundleNames = AssetDatabase.GetAllAssetBundleNames ();
				for (int i = 0, max = allBundleNames.Length; i < max; i++) {
					var bundleName = allBundleNames [i];
					AssetDatabase.RemoveAssetBundleName (bundleName, true);
					if (_showProgess)
						EditorUtility.DisplayProgressBar ("移除所有资源BundleName中",string.Format(" {0} / {1} ",i,max),i/(float)max);
				}
				if (_showProgess) {
					EditorUtility.ClearProgressBar ();
				}
			}
			RefreshBundleNameData ();
			AssetDatabase.Refresh ();
			EditorUtility.DisplayDialog ("确认", cleanAll ? "清空所有资源BundleName成功" : "清空未使用的BundleName成功", "Yes");
		}

		private string GetLastResConfigPathHash(){
			int hash = Application.dataPath.GetHashCode ();
			string lastResConfigPathHash = string.Format ("LastResConfigPath{0}", hash);
			return lastResConfigPathHash;
		}
		#region Helper Func

		private Action<string> UpdateAssetBundleNameAction(){
			_changeCount = 0;
			return s => {
				_changeCount++;
				_bundleNameLogger.AppendLine(string.Format("Update AssetBundle {1}：{0}",s,AssetImporter.GetAtPath(s).assetBundleName));
			};
		}

		private string GetExportBundlePath(){
			return GetExportPlatformPathByBuildTarget (EditorUserBuildSettings.activeBuildTarget) + "/" + GameResPath.BUNDLE_ROOT;
		}

		private string GetExportPlatformPathByBuildTarget(BuildTarget buildTarget){
			string platformRoot;
			if (buildTarget == BuildTarget.Android) {
				platformRoot = GameResPath.EXPROT_FOLDER + "/Android";
			} else if (buildTarget == BuildTarget.iOS) {
				platformRoot = GameResPath.EXPROT_FOLDER + "/IOS";
			} else {
				platformRoot = GameResPath.EXPROT_FOLDER + "/PC";
			}
			return platformRoot;
		}

		//根据resConfig版本号，获取版本资源导出目录
		private string GetBackupDir(ResConfig resConfig){
			if (resConfig == null)
				return null;
			return GetBackupRoot () + "/" + GameResPath.BUNDLE_ROOT + "_" + resConfig.Version;
		}

		private string GetBackupRoot(){
			return GetExportBundlePath () + "/backup";
		}

		private string GetBundleBackupDir(ResInfo resInfo,string backupDir){
			if(resInfo.remoteZipType == CompressType.UnityLZMA)
				return backupDir +"/lzma";
			else if(resInfo.remoteZipType == CompressType.CustomTex)
				return backupDir+"/custom";
			else
				return backupDir+"/lz4";
		}

		public string GetResConfigRoot(){
			return GetExportPlatformPathByBuildTarget (EditorUserBuildSettings.activeBuildTarget) + "/" + GameResPath.RESCONFIG_ROOT;
		}
		#endregion


		#region 生成StreamingAssets资源

		private void GenerateTotalRes(){
			GeneratePackageBundle (false);
		}

		private void GenerateMiniRes(){
			GeneratePackageBundle (true);
		}


		private void GeneratePackageBundle(bool isMiniRes){
			if (isMiniRes) {
				GenerateAssetbundleStrategyPackageBundle ();
				return;
			} else {
				//清空小包配置
				var jsonstreamingAssetsPath = Application.streamingAssetsPath +"/"+GameResPath.MINIRESCONFIG_FILE;
				//先清空StreamingAsset资源mlu
				FileUtil.DeleteFileOrDirectory(jsonstreamingAssetsPath);
			}

			if (_curResConfig == null) {
				EditorUtility.DisplayDialog ("确认", "当前版本信息为空，请重新确认？", "Yes");
				return;
			}

			//先清空StreamingAsset资源目录
			string packageDir = Application.streamingAssetsPath+"/"+GameResPath.BUNDLE_ROOT;
			FileUtil.DeleteFileOrDirectory (packageDir);
			Directory.CreateDirectory (packageDir);

			var stopWatch = new Stopwatch ();
			stopWatch.Start ();
			var backUpDir = GetBackupDir (_curResConfig);
			var finishedCount = 0;
			var bundleCount = _curResConfig.Manifest.Count;
			long totalFileLength = 0L;

			try{
				var miniResConfig = isMiniRes?new MiniResConfig():null;
				foreach(var resInfo in _curResConfig.Manifest.Values){
					//小包模式下，只拷贝必需资源到包内
					resInfo.isPackageRes = true;
					if(isMiniRes && !_buildBundleStrategy.minResConfig.ContainsKey(resInfo.bundleName)){
						resInfo.isPackageRes = false;
						string replaceKey = "";
						if(!_buildBundleStrategy.replaceResConfig.TryGetValue(resInfo.bundleName, out replaceKey)){
							Debug.LogError("该BundleName未设置replaceKey,请检查： "+resInfo.bundleName);
						}
						miniResConfig.replaceResConfig.Add(resInfo.bundleName,replaceKey);
					}

					if(resInfo.isPackageRes){
						string bundleBackupDir = GetBundleBackupDir(resInfo,backUpDir);
						var inputFile = resInfo.GetABPath(bundleBackupDir);
						if(File.Exists(inputFile)){
							var outputFile = resInfo.GetABPath(packageDir,EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
							var dir = Path.GetDirectoryName(outputFile);
							totalFileLength += resInfo.size;
							Directory.CreateDirectory(dir);
							FileUtil.CopyFileOrDirectory(inputFile,outputFile);
						}else{
							throw new Exception("生成包内资源异常，不存在该Bundle文件： "+inputFile);
						}
					}

					finishedCount +=1;
					if(_showProgess)
						EditorUtility.DisplayProgressBar("拷贝AssetBundle中",string.Format(" {0} / {1} ",finishedCount,bundleCount),(float)finishedCount/bundleCount);
				}

				if(isMiniRes){
					//小包模式下，需要生成MiniResConfig到包内
					FileHelper.SaveJsonObj(miniResConfig, Application.streamingAssetsPath +"/"+GameResPath.MINIRESCONFIG_FILE,true);
				}

				if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
					EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows){
					//生成一个假信息，如果打包后没有对dll进行处理，则打包出来的不会进行dll更新
					var dllVersion = new DllVersion(){
						Version = MaxVersion,
					};
					FileHelper.SaveJsonObj(dllVersion,Application.streamingAssetsPath+"/"+GameResPath.DLLVERSION_FILE);
				}

				string packageResConfigPath = Path.Combine(Application.streamingAssetsPath,GameResPath.RESCONFIG_FILE);
				_curResConfig.isMiniRes = isMiniRes;
				_curResConfig.SaveFile(packageResConfigPath,true);
			}catch(Exception e){
				Debug.LogError (e.Message);
				if (_showProgess)
					EditorUtility.ClearProgressBar ();
				AssetDatabase.Refresh ();
				return;
			}

			if (_showProgess)
				EditorUtility.ClearProgressBar ();

			AssetDatabase.Refresh ();
			stopWatch.Stop ();
			var elapsed = stopWatch.Elapsed;
			string hint = string.Format ("迁移资源到StremmingAssets耗时： {0:00}:{1:00}:{2:00}:{3:00}\n包内资源大小为:{4}",
				elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10, EditorUtility.FormatBytes (totalFileLength));
			if (!_showProgess && !_slientMode) {
				EditorUtility.DisplayDialog ("提示", hint, "Yes");
			}
			Debug.Log (hint);
		}

		/// <summary>
		/// 拷贝安全包到StreamingAsset资源目录
		/// 非安全包资源设置未下载状态
		/// </summary>
		private void GenerateAssetbundleStrategyPackageBundle(){
			if (_curResConfig == null) {
				EditorUtility.DisplayDialog ("确认", "当前版本信息为空，请重新确认？", "Yes");
				return;
			}

			//先清空StreamingAsset资源目录
			FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath+"/"+GameResPath.BUNDLE_ROOT);
			string packageDir = Application.streamingAssetsPath + "/" + GameResPath.BUNDLE_ROOT;
			Directory.CreateDirectory (packageDir);

			var stopwatch = new Stopwatch ();
			stopwatch.Start ();
			var backupDir = GetBackupDir (_curResConfig);
			var finishedCount = 0;
			var bundleCount = _curResConfig.Manifest.Count;
			long totalFileLength = 0L;
			try{
				var jsonPath = Application.dataPath+GameResPath.AssetBundleStrategyJsonPath;
				var info = AssetPipeline.FileHelper.ReadJsonFile<AssetbundleStrategyFolderInfo>(jsonPath);
				//不要使用安全包，万一其他包算漏了，就会出问题，放过来算
				var downloads = new HashSet<string>();
				foreach(var keyValue in info.children){
					var data = keyValue.Value;
					if(data.IsSafestrategy)
						continue;
					downloads.UnionWith(data.assets);
				}

				foreach(var resInfo in _curResConfig.Manifest.Values){
					//小包模式下，只拷贝必需资源到包内
					resInfo.isPackageRes = true;
					if(downloads.Contains(resInfo.bundleName)){
						resInfo.isPackageRes = false;
					}

					if(resInfo.isPackageRes){
						string bundleBackupDir =GetBundleBackupDir(resInfo,backupDir);
						var inputFile = resInfo.GetABPath(bundleBackupDir);
						if(File.Exists(inputFile)){
							var outputFile = resInfo.GetABPath(packageDir,EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
							var dir = Path.GetDirectoryName(outputFile);
							totalFileLength+=resInfo.size;
							Directory.CreateDirectory(dir);
							FileUtil.CopyFileOrDirectory(inputFile,outputFile);
						}else{
							throw new Exception("生成包内资源异常，不存在该Bundle文件： "+inputFile);
						}
					}

					finishedCount+=1;
					if(_showProgess){
						EditorUtility.DisplayProgressBar("拷贝AssetBundle中",string.Format(" {0} / {1} ",finishedCount,bundleCount),(float)finishedCount/bundleCount);
					}
				}

				//copy 策略包到streamingAssetsPath
				var jsonstreamingAssetsPath = Application.streamingAssetsPath+"/"+GameResPath.MINIRESCONFIG_FILE;
				//先清空StreamingAssets资源目录
				FileUtil.DeleteFileOrDirectory(jsonstreamingAssetsPath);
				//压缩
				FileHelper.SaveJsonText(FileHelper.ReadAllText(jsonPath),jsonstreamingAssetsPath,true);

				if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
					||EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows){
					//生成一个假信息，如果打包后没有对dll进行处理，则打包出来的不会进行dll更新
					var dllVersion = new DllVersion(){
						Version = MaxVersion,
					};
					FileHelper.SaveJsonObj(dllVersion,Application.streamingAssetsPath+"/"+GameResPath.DLLVERSION_FILE);
				}

				string packageResConfigPath = Path.Combine(Application.streamingAssetsPath,GameResPath.RESCONFIG_FILE);

				ResConfig safeResConfig = new ResConfig{
					Version = _curResConfig.Version,
					lz4CRC = _curResConfig.lz4CRC,
					lzmaCRC = _curResConfig.lzmaCRC,
					BuildTime = _curResConfig.BuildTime,
					compressType = _curResConfig.compressType,
					isMiniRes = true,
				};
				foreach(var res in _curResConfig.Manifest.Values){
					if(res.isPackageRes){
						safeResConfig.TotalFileSize += res.size;
						safeResConfig.Manifest.Add(res.bundleName,res);
					}
					else{
						var saferes = res.DeepCopy();
						saferes.MakeMiniRes();
						safeResConfig.Manifest.Add(saferes.bundleName,saferes);
					}
				}
			}catch(Exception e){
				Debug.LogError (e.Message);
				if (_showProgess)
					EditorUtility.ClearProgressBar ();
				AssetDatabase.Refresh ();
				return;
			}

			if (_showProgess)
				EditorUtility.ClearProgressBar ();

			AssetDatabase.Refresh ();
			stopwatch.Stop ();
			var elapsed = stopwatch.Elapsed;
			string hint = string.Format ("迁移资源到StremmingAssets耗时： {0:00}:{1:00}:{2:00}:{3:00}\n包内资源大小为:{4}",
				              elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10, EditorUtility.FormatBytes (totalFileLength));
			if (!_showProgess && !_slientMode) {
				EditorUtility.DisplayDialog ("提示", hint, "Yes");
			}
			Debug.Log (hint);
		}
		#endregion


	}

}
