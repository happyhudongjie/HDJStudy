using System.Text;
using System;
using UnityEditor;
using UnityEngine;


namespace AssetPipeline{
	
	public class AssetBundleBuilder : AssetPostprocessor {

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
}
