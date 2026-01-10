using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BrakeState : BaseModuleState
{
    // 각 휠에 적용될 최종 브레이크 토크
    public float torqueFL;
    public float torqueFR;
    public float torqueRL;
    public float torqueRR;

    public float maxBrakeTorque;
    // (옵션) UI 표시용: ABS가 작동 중인가?
    public bool isABSActive;

    public float GetTorque(int index)
    {
        switch (index)
        {
            case 0 :
                return torqueFL;
            case 1 :
                return torqueFR;
            case 2 :
                return torqueRL;
            case 3 :
                return torqueRR;
            default:
                return 0.0f;
        }
    }

    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
        torqueFL = 0f;
        torqueFR = 0f;
        torqueRL = 0f;
        torqueRR = 0f;
        isABSActive = false;
    }
}
