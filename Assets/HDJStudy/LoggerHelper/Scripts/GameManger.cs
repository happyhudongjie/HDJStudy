using UnityEngine;
using System.Collections.Generic;

public class GameManger : MonoBehaviour {

    public List<GameStateMgr> mgrList = new List<GameStateMgr>();

    bool isInit = false;

    void Awake()
    {
        if (isInit) return;
        RegisterMgr(mgrType.Log, new LogTest());
        isInit = true;
    }
	
    /// <summary>
    /// 注册管理器
    /// </summary>
    /// <param name="type"></param>
    /// <param name="mgr"></param>
    void RegisterMgr(mgrType type,GameStateMgr mgr)
    {
        if (mgrList.Count == 0)
        {
            mgrList.Add(mgr);
        }
        else
        {
            for(int i = 0; i < mgrList.Count; i++)
            {
                if(mgrList[i].mgrType == type)
                {
                    return;
                }
            }
            mgrList.Add(mgr);
        }
    }

    public GameStateMgr GetMgr(mgrType type)
    {
        GameStateMgr mgr = null;
        for(int i = 0; i < mgrList.Count; i++)
        {
            if(type == mgrList[i].mgrType)
            {
                return mgrList[i];
            }
        }
        return mgr;
    }
}
