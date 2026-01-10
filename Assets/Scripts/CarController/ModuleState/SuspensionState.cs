
[System.Serializable]
public class SuspensionState : BaseModuleState
{
    // [결과값] Unity WheelCollider용
    public float finalSpringRate;      // N/m
    public float finalDamper;          // Ns/m
    public float finalTargetPos;       // 0.0 ~ 1.0
    public float finalDistance;        // m

    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
        
    }
}