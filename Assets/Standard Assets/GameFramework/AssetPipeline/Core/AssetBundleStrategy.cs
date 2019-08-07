using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace AssetPipeline{

	public class AssetbundleStrategyManager{
		
	}

	public abstract class AssetbundleStrategyInfo{
		public string Name;
	}

	public class AssetbundleStrategyFolderInfo:AssetbundleStrategyInfo{
		public Dictionary<string,ABResStrategyDataInfo> children;

	}

	public class ABResStrategyDataInfo{
		public string Id;
		public bool IsSafestrategy;
		public List<string> assets;
 	}
}

