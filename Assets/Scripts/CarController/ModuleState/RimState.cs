using UnityEngine;

[System.Serializable]
public class RimState : BaseModuleState
{
    // Tire가 계산할 때 필요한 값들만 공개 (유니티 물리 단위로 변환됨)
    public float radiusM; // 림 반지름 (m)
    public float massKg;  // 림 무게 (kg)

    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
      
    }
}