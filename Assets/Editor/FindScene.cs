using UnityEngine; 
using UnityEditor; 
using System; 
using System.Collections.Generic; 

public class CheckScene : Editor 
{ 
    [MenuItem("GameObject/Obtain All Object")] 
    public static void GetAllObjectsInScene() 
    {
        MeshCollider[] pAllObjects = (MeshCollider[])Resources.FindObjectsOfTypeAll(typeof(MeshCollider)); 
        List<MeshCollider> pReturn = new List<MeshCollider>(); 
        foreach (MeshCollider pObject in pAllObjects) 
        { 
            if (pObject.hideFlags == HideFlags.NotEditable) 
            { 
                continue; 
            } 
            if (pObject.hideFlags == HideFlags.HideAndDontSave) 
            { 
                continue; 
            } 

            pReturn.Add(pObject); 
        }
        Dictionary<int, GameObject> _dic = new Dictionary<int, GameObject>();
        UnityEngine.Object[] _obj;
        int i = 0;
          foreach (MeshCollider value in pReturn) 
        { 
            EditorGUIUtility.PingObject(value.gameObject);
            _dic.Add(i, value.gameObject);
            i++;
        }
        _obj = new UnityEngine.Object[i];
        for(int j = 0; j < i; j++)
        {
            _obj[j] = _dic[j];
        }
        Selection.objects = _obj;
        pReturn.Clear(); 
        pReturn = null; 
    } 
} 


