using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameDebuger  {

	public static void Log(object message,string color = null){
	
		var log = message ?? "Null";
		#if UNITY_EDITOR
		if(!string.IsNullOrEmpty(color)){
			log = string.Format("<color={0}>{1}</color>",color,log);
		}
		#endif
		Debug.Log (log);
	}
}
