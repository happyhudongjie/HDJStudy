using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System;


namespace AssetPipeline
{
	public enum ResGroup{
		None = 0,
		Common = 1,
		Scene = 2,
		Audio = 3,
		Config = 4,
		Script = 5,

		UIPrefab = 10,
		UIAtlas = 11,
		UIFont = 12,
		Image = 13,			//对话框头像，地图缩略图等一类需要异步加载的图像资源
		UITexture = 14,		//被UIPrefab直接引用到的一些图像资源
		StreamScene = 15,	//用于流式加载的场景资源
		RawScene = 16,  	//流场景prefab
		PlotCameraPath = 17,
		Model = 20,
		Effect = 30,
		TileMap = 40

	}
	public static class BuildBundlePath {

		private static Dictionary<int,string> _resGroupMap;
		public static Dictionary<int,string> resGroupMap{
			get{ 
				if (_resGroupMap == null) {
					_resGroupMap = new Dictionary<int, string> ();
					ResGroup[] array = (ResGroup[])Enum.GetValues (typeof(ResGroup));
					for (int i = 0; i < array.Length; i++) {
						ResGroup resGroup = array [i];
						_resGroupMap.Add ((int)resGroup, resGroup.ToString ().ToLower () + "/");
					}
				}
				return _resGroupMap;
			}
		}

		#region 资源路径配置

		public static HashSet<string> CommondFilePath = new HashSet<string>{
			"Assets/HDJStudy/AssetBundleBuilder/AudioMixer/djAudioMixer.mixer",
			"Assets/HDJStudy/AssetBundleBuilder/AudioMixer/djAudioMixer.mixer"
		};

		public static HashSet<string> FilterShaderFilePath = new HashSet<string>{
			"Assets/T4M/Shaders/ShaderModel3/Bump/T4M 4 TexturesBump.shader",
			"Assets/Shaders/T4M/T4M 4 Textures.shader",
		};

		public static string[] UITextureFolder = {
			"Assets/HDJStudy/AssetBundleBuilder/UI/Atlas/CommonTextures"	
		};

		public static string[] UIPrefabFolder = {
			"Assets/HDJStudy/AssetBundleBuilder/UI/Prefabs"	
		};

		public static string[] UIAtlasFolder = {
			"Assets/HDJStudy/AssetBundleBuilder/UI/Atlas"	
		};

		public static string[] UIFontFolder = { 
			"Assets/HDJStudy/AssetBundleBuilder/UI/Fonts"	
		};

		public static HashSet<string> ImageFilePath = new HashSet<string>{};
		#endregion

		#region 辅助资源判断

		public static bool IsCommonAsset(this string path){
			return CommondFilePath.Contains (path);
		}

		public static bool IsController(this string path){
			return path.EndsWith (".controller");
		}

		public static bool IsFilterShader(this string path){
			return FilterShaderFilePath.Any (path.EndsWith);
		}

		/*
		 * OrdinalIgnoreCase 忽略大小写比较
		*/
		public static bool IsTextureFile(this string path){
			return path.EndsWith (".png", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".jpg", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".tga", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".psd", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".tif", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".gif", StringComparison.OrdinalIgnoreCase)
			|| path.EndsWith (".bmp", StringComparison.OrdinalIgnoreCase);

		}

		public static bool IsGameUITexture(this string path){
			return UITextureFolder.Any(path.StartsWith);
		}

		public static bool IsPrefabFile(this string path){
			return path.EndsWith (".prefab", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsUIPrefab(this string path){
			return UIPrefabFolder.Any (path.StartsWith);
		}

		public static bool IsUIAtals(this string path){
			return UIAtlasFolder.Any (path.StartsWith);
		}
		#endregion

		#region 辅助函数

		public static bool UpdateBundleName(this AssetImporter importer,string bundleName){
			bundleName = bundleName.ToLower ();
			if (!CheckBundleNameValid (bundleName)) {
				string errorMessage = string.Format ("BundleName 命名不合法：{0} 、你AssetPath: {1}", bundleName, importer.assetPath);
				Debug.LogError (errorMessage, AssetDatabase.LoadAssetAtPath<UnityEngine.Object> (importer.assetPath));
				EditorUtility.DisplayDialog ("错误", "资源命名不符合规范\n" + errorMessage, "确定");
			}
			var oldBundleName = importer.assetBundleName;
			if (oldBundleName != bundleName) {
				importer.SetAssetBundleNameAndVariant (bundleName, null);
				return true;
			}
			return false;
		}

		private static bool CheckBundleNameValid(string bundleName){
			return !Regex.IsMatch(bundleName,@"[\u4e00-\u9fa5\s（）]");
		}

		public static string GetAssetBundleName(this AssetImporter importer,ResGroup resGroup){
			var assetName = Path.GetFileNameWithoutExtension (importer.assetPath);
			return GetBundleName (assetName, resGroup);
		}

		private static string GetBundleName(string assetName, ResGroup resGroup){
			if (string.IsNullOrEmpty (assetName)) {
				Debug.LogErrorFormat ("资源名异常空： {0}", assetName);
				return null;
			}
			var bundleName = assetName.ToLower ();
			if (resGroup == ResGroup.None)
				return bundleName;

			bundleName = resGroupMap [(int)resGroup] + bundleName;
			return bundleName;
		}

		#endregion
	}

	public class BuildBundleStrategy{
		//自定义小包代替资源信息
		public Dictionary<string,string> replaceResConfig;
		//小包必须资源信息
		public Dictionary<string,string> minResConfig;
		//更新ResConfig的时候回读取来赋值，需要每个独立配置
		public Dictionary<string,bool> preloadConfig;

		public BuildBundleStrategy(){
			replaceResConfig = new Dictionary<string, string> ();
			minResConfig = new Dictionary<string, string> ();
			preloadConfig = new Dictionary<string, bool> ();
		}
	}
		
}

