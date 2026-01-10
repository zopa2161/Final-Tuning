using UnityEngine;

[System.Serializable]
public class EngineState : BaseModuleState
{
    public float maxRPM;
    
    public float currentRPM;
    public float generatedTorque; // 이번 프레임에 만든 토크
    //public float currentHeat;     // 현재 온도
    //public float fuelConsumption;
    public float maxTorque;
    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
        currentRPM = 1f;
        generatedTorque = 1f;
    }
}
