using UnityEngine;

[System.Serializable]
public abstract class BaseModuleState
{   
    //프레임 마지막에 호출되어서 갱신
    public abstract void OnPush();

    public abstract void ResetEpisode();
}
