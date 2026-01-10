[System.Serializable]
public class SteeringState : BaseModuleState
{
    // 최종적으로 휠 콜라이더에 적용될 각도
    public float finalSteerAngleFL; // 왼쪽 바퀴 각도
    public float finalSteerAngleFR; // 오른쪽 바퀴 각도
    public float finalSteerAngleRL; // 뒤-좌 (4WS용, 기본 0)
    public float finalSteerAngleRR; // 뒤-우 (4WS용, 기본 0)
    
    
    // 현재 스티어링 휠(핸들)이 돌아간 정도 (-1.0 ~ 1.0)
    // 키보드 입력은 0->1로 확 바뀌지만, 실제 핸들은 부드럽게 돌아가므로 이를 추적함
   

    public float GetSteer(int index)
    {
        if (index == 0) return finalSteerAngleFL;
        if (index == 1) return finalSteerAngleFR;
        
        return 0f;
        
    }
    public override void OnPush()
    {
        throw new System.NotImplementedException();
    }

    public override void ResetEpisode()
    {
        finalSteerAngleFL = 0f;
        finalSteerAngleFR = 0f;
        finalSteerAngleRL = 0f;
        finalSteerAngleRR = 0f;
        
    }
}