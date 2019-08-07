using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetPipeline{
	public static class GameResPath{

		//常用文件
		public const string MINIRESCONFIG_FILE = "assetbundleStrategy.tz";
		public const string DLLVERSION_FILE = "dllVersion.json";
		public const string RESCONFIG_FILE = "resconfig.tz";

		//特殊的AB
		public const string AllShaderBundleName = "common/allshader";
		public const string AllCommonBundleName = "common/allcommon";
		public const string AllBaseAnimationName = "common/baseac";

		//本地打包用文件夹
		public const string EXPROT_FOLDER = "_GameBundles";
		public const string PATCH_BUNDLE_ROOT = "patch_resources";

		//常用本地文件夹
		public const string BUNDLE_ROOT = "gameres";

		//远程文件夹
		public const string RESCONFIG_ROOT = "resconfig";

		public const string AssetBundleStrategyJsonPath = "/Editor/GameRes/AssetBuildModule/Config/assetbundleStrategy.json";
	}
}
