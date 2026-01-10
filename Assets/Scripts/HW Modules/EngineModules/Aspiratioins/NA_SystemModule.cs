using System;
using UnityEngine;

[Serializable]
public class NA_System : BaseAspirationModule
{
    [Tooltip("스로틀 반응성 (ITB 튜닝 시 증가)")]
    public float throttleResponse = 10f; 

    public override float GetPowerMultiplier(float currentRPM, float throttleAmount)
    {
        // NA는 부스트가 없으므로 기본 효율만 리턴
        return _intakeEfficiency;
    }
}