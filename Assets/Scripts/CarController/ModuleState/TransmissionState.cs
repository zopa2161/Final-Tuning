using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransmissionState : BaseModuleState
{
    public int currentGearIndex =1;
    public float currentRatio;

    public float gearCount;
    public float shiftTime;
    public float shiftTimer;
    public bool isShifting;       // 변속 중인가?

    public float clutchRatio = 0;

    public List<GearInfo> gearInfos;
    public float feedbackRPM;
    public float finalOutputTorque;

    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
        currentGearIndex = 1;
       
        shiftTimer = 0f;
        isShifting = false;
 
        feedbackRPM = 1f;
        finalOutputTorque = 1f;
    }
}