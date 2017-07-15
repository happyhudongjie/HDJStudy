using UnityEngine;
using System.Collections;

public class LogExcute : MonoBehaviour {

    GameManger mgr = null;

	void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if(mgr == null) 
                mgr = Camera.main.transform.GetComponent<GameManger>();

            if(mgr != null)
            {
                mgr.GetMgr(mgrType.Log).OnExcute(mgrType.Log, "LogExcute");
            }
        }
    }
}
