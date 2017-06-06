using UnityEngine;
using System.Collections;

public class ddd : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Starst());
	}

    IEnumerator Starst()
    {
        WWW www = new WWW("file://" + Application.dataPath + "/HDJStudy/ScriptableObject/Text/SysData.assetbundle");
        yield return www;

        //转换资源为SysData，这个sd对象将拥有原来在编辑器中设置的数据。                
        DataParse sd = www.assetBundle.mainAsset as DataParse;
        //如打印sd.content[0]，将得到Vector3(1,2,3);               
        print(sd.theData[0]);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
