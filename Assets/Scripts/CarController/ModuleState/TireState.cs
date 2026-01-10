using UnityEngine;

[System.Serializable]
public class TireState : BaseModuleState
{
    // [결과값] 물리 적용용
    public float widthMm;    // m (림+타이어 합산)
    public float tireWeightKg; // kg (림+타이어 합산)
       // 1.0 기준
    public float calculatedRadius;
    public float totalMass;
    
    // [New] 마찰력 곡선 (WheelCollider에 바로 대입할 값)
    public WheelFrictionCurve forwardFriction;
    public WheelFrictionCurve sidewaysFriction;

    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
     
    }
}