using UnityEditor;
using UnityEngine;

//https://longerdewo.iteye.com/blog/2170414
[CustomEditor(typeof(JSTimer))]
public class JSTimerInspector : Editor {

	JSTimer _mJsTimer;
	private bool _coolDownToggle = true;
	private bool _timerToggle = true;

	public override void OnInspectorGUI(){
		_mJsTimer = target as JSTimer;
		if (_mJsTimer == null)
			return;

		var coolDownTaskList = _mJsTimer.CdTasks;
		GUILayout.Label ("=============================");
		_coolDownToggle = EditorGUILayout.Foldout (_coolDownToggle, string.Format ("CoolDown:{0}", coolDownTaskList.Count));
		GUILayout.Label ("=============================");
		if (_coolDownToggle) {
			for (int i = 0, max = coolDownTaskList.Count; i < max; ++i) {
				DrawCoolDownTask (coolDownTaskList [i]);
			}
		}

		var timerTaskList = _mJsTimer.TimerTasks;
		GUILayout.Label ("=============================");
		_timerToggle = EditorGUILayout.Foldout (_timerToggle, string.Format ("Timer:{0}", timerTaskList.Count));
		GUILayout.Label ("=============================");
		if (_timerToggle) {
			for (int i = 0, max = timerTaskList.Count; i < max; ++i) {
				DrawTimerTask (timerTaskList [i]);
			}
		}

		this.Repaint ();
	}

	private void DrawCoolDownTask(JSTimer.CdTask cdTask){
		GUILayout.BeginVertical ("GroupBox");
		{
			GUILayout.Label ("name: " + cdTask.taskName);
			if (cdTask.isValid) {
				GUILayout.Label (string.Format ("剩余时间： {0}/{1}", cdTask.remainTime, cdTask.totalTime));
				GUILayout.Label ("更新频率： " + cdTask.updateFrequence);
				GUILayout.Label ("timeScale: " + cdTask.timeScale);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Finish")) {
					cdTask.remainTime = 0f;
				}
				if (GUILayout.Button ("Cancel")) {
					_mJsTimer.CancelCd (cdTask.taskName);
				}
				if (GUILayout.Button (cdTask.isPause ? "Resume" : "Pause")) {
					cdTask.isPause = !cdTask.isPause;
				}
				GUILayout.EndHorizontal ();
			} else {
				GUILayout.Label("已失效");
			}
		}

		GUILayout.EndVertical();
	}

	private void DrawTimerTask(JSTimer.TimerTask timerTask){
		GUILayout.BeginVertical ("GroupBox");
		{
			GUILayout.Label ("name: " + timerTask.taskName);
			if (timerTask.isValid) {
				GUILayout.Label ("累计时间：" + timerTask.cumulativeTime);
				GUILayout.Label ("更新频率： " + timerTask.updateFrequence);
				GUILayout.Label ("timeScale: " + timerTask.timeScale);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Cancel")) {
					_mJsTimer.CancelTimer (timerTask.taskName);
				}
				if (GUILayout.Button (timerTask.isPause ? "Resume" : "Pause")) {
					timerTask.isPause = !timerTask.isPause;
				}
				GUILayout.EndHorizontal ();
			} else {
				GUILayout.Label("已失效");
			}
		}
		GUILayout.EndVertical();
	}
}
