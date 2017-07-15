using UnityEngine;
using System.Collections;
using UnityEditor;

public class OpenScene : Editor {

    [SerializeField]
    private static int _currentScene = -1;

    [MenuItem("OpenScene/RunDragView")]
    public static void OpenAndRunDragViewScene()
    {
        int index = GetSceneIndex("DragView");

        if(index == -1)
        {
            return;
        }

        if (_currentScene == index)
            return;
        OpenScenes(index);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("OpenScene/DragView")]
    public static void OpenDragView()
    {
        int index = GetSceneIndex("DragView");
        if (index == -1)
            return;

        OpenScenes(index);
    }


    [MenuItem("OpenScene/NGUITool")]
    public static void OpenNGUITool()
    {
        int index = GetSceneIndex("NGUITool");
        if (index == -1)
            return;

        OpenScenes(index);
    }


    [MenuItem("OpenScene/NGUITool")]
    public static void OpenScriptableObject()
    {
        int index = GetSceneIndex("ScriptableObject");
        if (index == -1)
            return;

        OpenScenes(index);
    }


    [MenuItem("OpenScene/Log")]
    public static void OpenLog()
    {
        int index = GetSceneIndex("Log");
        if (index == -1)
            return;

        OpenScenes(index);
    }



    public static int GetSceneIndex(string name)
    {
        int index = -1;

        for(int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            string fname = System.IO.Path.GetFileName(EditorBuildSettings.scenes[i].path);
            if(fname == name + ".unity")
            {
                return i;
            }
        }
        return index;
    }

    public static void OpenScenes(int i)
    {
        if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
        {
            _currentScene = i;
            EditorApplication.OpenScene(EditorBuildSettings.scenes[i].path);
        }
    }
}
