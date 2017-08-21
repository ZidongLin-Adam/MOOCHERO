using UnityEngine;
using System.Collections;
//用于游戏逻辑状态机的生成
public class GLStateMachineGenerator {
    //根据名字创建并获取状态机实例
    static public BaseGLStateMachine getGLStateMachineByName(string name,GameManager gm)
    {
        BaseGLStateMachine instance = null;
        switch (name)
        {
            case "prepare":
                instance = GLStateMachineFactory<PrepareGLStateMachine>.Create(gm);
                break;
            case "end":
                instance = GLStateMachineFactory<EndGLStateMachine>.Create(gm);
                break;
            case "compete":
                instance = GLStateMachineFactory<CompeteGLStateMachine>.Create(gm);
                break;
            case "getTreasure":
                instance = GLStateMachineFactory<GetTreasureGLStateMachine>.Create(gm);
                break;
            case "transport":
                instance = GLStateMachineFactory<TransportGLStateMachine>.Create(gm);
                break;
        }
        return instance;
    }
    //根据状态机实例判断其名字
    static public string getNameByGLStateMachine(BaseGLStateMachine sm)
    {
        string smName = "";
        switch (sm.GetType().FullName)
        {
            case "PrepareGLStateMachine":
                smName = "prepare";
                break;
            case "EndGLStateMachine":
                smName = "end";
                break;
            case "CompeteGLStateMachine":
                smName = "compete";
                break;
            case "GetTreasureGLStateMachine":
                smName = "getTreasure";
                break;
            case "TransportGLStateMachine":
                smName = "transport";
                break;
        }
        return smName;
    }
}
//工厂类模板 创建实例并执行init函数
public class GLStateMachineFactory<T> where T : BaseGLStateMachine, new()
{
    static public BaseGLStateMachine Create(GameManager gm)
    {
        var instance = new T();
        instance.setGameManager(gm);
        instance.Init();
        return instance;
    }
}
