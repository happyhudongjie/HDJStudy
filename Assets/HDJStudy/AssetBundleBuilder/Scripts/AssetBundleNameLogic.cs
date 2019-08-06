using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetPipeline
{
	public static class AssetBundleNameLogic 
	{
		public static void UpdateCommon(string filePath,Action<string> updateAction){
			UpdateCheck (filePath,importer =>{
				if(!filePath.IsController()){
					if(importer.UpdateBundleName(GameResPath.AllCommonBundleName)){
						updateAction.SafeInvoke(filePath);
					}
				}
				else{
					var bundleName = GameResPath.AllBaseAnimationName;
					if(importer.UpdateBundleName(bundleName)){
						updateAction.SafeInvoke(filePath);
					} 

					var dependencies = AssetDatabase.GetDependencies(filePath,false);
					for(var i = 0;i<dependencies.Length;i++){
						var refPath = dependencies[i];
						UpdateCommonDependence(refPath,updateAction,bundleName);
					}
				}
			});
		}

		private static void UpdateCommonDependence(string filePath,Action<string> updateAction,string bundleName){
			UpdateCheck (filePath, importer => {
				if(importer.UpdateBundleName(bundleName)){
					updateAction.SafeInvoke(filePath);
				}
			}, false);
		}

		public static void UpdateShader(string filePath,Action<string> updateAction){
			if (filePath.IsFilterShader ()) {
				return;
			}
			UpdateCheck (filePath, importer => {
				if(importer.UpdateBundleName(GameResPath.AllShaderBundleName))	{
					updateAction.SafeInvoke(filePath);
				}
			});
		}

		public static void UpdateUIAtlas(string filePath,Action<string> udpateAction){
			UpdateCheck (filePath, importer => {
				if(importer.UpdateBundleName(importer.GetAssetBundleName(ResGroup.UIAtlas))){
					udpateAction.SafeInvoke(filePath);
				}	
			});
		}

		public static void UpdateUIPrefab(string filePath,Action<string> updateAction){
			UpdateCheck (filePath, importer => {
				if(importer.UpdateBundleName(importer.GetAssetBundleName(ResGroup.UIPrefab))){
					updateAction.SafeInvoke(filePath);
				}	
			});

			//handle uiprefab dependence
			var depedencies = AssetDatabase.GetDependencies(filePath,false);
			for (var j = 0; j < depedencies.Length; j++) {
				var refPath = depedencies [j];
				if (refPath.IsCommonAsset ())
					continue;
				if (refPath.IsTextureFile ()) {
					//UIPrefab中引用到的贴图都要统一放在CommonTextures目录下
					if(refPath.IsGameUITexture()){
						UpdateUITexture (refPath, updateAction);
					}
				}
			}
		}

		public static void UpdateUITexture(string filePath, Action<string> updateAction){
			UpdateCheck (filePath, importer => {
				if(importer.UpdateBundleName(importer.GetAssetBundleName(ResGroup.UITexture))){
					updateAction.SafeInvoke(filePath);
				}	
			});
		}

		private static void UpdateCheck(string filePath,Action<AssetImporter> action,bool checkCommon = true)
		{
			if (checkCommon && filePath.IsCommonAsset ()) {
				return;
			}
			var importer = GetAssetImporter (filePath);
			if (importer != null) {
				action (importer);
			}
		}


		private static AssetImporter GetAssetImporter(string filePath){
			AssetImporter importer = null;
			if (!string.IsNullOrEmpty (filePath)) {
				importer = AssetImporter.GetAtPath (filePath);
			}
			return importer;
		}

		private static void SafeInvoke(this Action<string> action,string path){
			if (action != null) {
				action (path);
			}
		}
	}
}
