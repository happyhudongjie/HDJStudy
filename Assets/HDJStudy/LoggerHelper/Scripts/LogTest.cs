using UnityEngine;
using System.Collections;
using Mogo.Util;

public class LogTest : GameStateMgr {

    LoggerHelper log;
    public LogTest()
        : base(mgrType.Log)
    {
        log = new LoggerHelper();
    }

    public override void OnExcute(mgrType type, object obj)
    {
        if (obj == null) return;
        if(type == mgrType.Log)
        {
            string str = obj as string;
            Debug.Log(str+"\t进入重写的Excute~~~~~~~~~");
        }
    }
}
