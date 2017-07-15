using UnityEngine;
using System.Collections;

public enum mgrType
{
    Defalt,
    Log
}

public class GameStateMgr  {

    public mgrType mgrType = mgrType.Defalt;

    public GameStateMgr(mgrType type = mgrType.Defalt)
    {
        mgrType = type;
    }


    public void Init()
    {

    }

    public virtual void OnExcute(mgrType type,object obj)
    {

    }

    public virtual void OnReset()
    {

    }

    public virtual void OnUpdate()
    {

    }


    public virtual void OnClean()
    {

    }
}
