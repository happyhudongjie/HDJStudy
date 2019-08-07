using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System;
using UnityEditor.Callbacks;

public static class GameDebugerConsole  {

	private static int _recordScriptId;

	[OnOpenAsset(0)]
	private static bool OnOpenAsset(int instanceId,int line){
		if (instanceId == _recordScriptId) {
			return false;
		}

		var stackTrace = GetConsoleStackTrace ();
		if (!string.IsNullOrEmpty (stackTrace) && Regex.IsMatch (stackTrace, @"GameDebuger.cs:|GameDebugerConsole.cs:")) {
			var matches = Regex.Match (stackTrace, @"\(at (.+)\)");
			while (matches.Success) {
				var pathLine = matches.Groups [1].Value;
				if (!pathLine.Contains ("GameDebuger")) {
					var msgs = pathLine.Split (':');
					var script = AssetDatabase.LoadMainAssetAtPath (msgs [0]);
					_recordScriptId = script.GetInstanceID ();
					return AssetDatabase.OpenAsset (_recordScriptId, Convert.ToInt32 (msgs [1]));
				}
				matches = matches.NextMatch ();
			}
		}
		return false;
	}

	private static string GetConsoleStackTrace(){
		var consoleType = typeof(EditorWindow).Assembly.GetType ("UnityEditor.ConsoleWindow");
		var consoleInstance = consoleType.GetField ("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic).GetValue (null);

		if (consoleInstance != null && EditorWindow.focusedWindow == consoleInstance) {
			var textField = consoleType.GetField ("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
			return textField.GetValue (consoleInstance).ToString ();
		}
		return null;
	}
}
