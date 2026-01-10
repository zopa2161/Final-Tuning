using Sirenix.OdinInspector;
using System;
using UnityEngine;
[Serializable]

public abstract class BaseAspirationModule 
{
    [SerializeField]
    [Range(0f,2f)]
    protected float _intakeEfficiency;

    public abstract float GetPowerMultiplier(float currentRPM, float throttleAmount);
    
}
