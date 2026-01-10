using UnityEngine;

[System.Serializable]
public class ChassisSensor
{
    // 데이터 그릇 생성
    public ChassisSensorData ChassisData = new ChassisSensorData();

    // (옵션) 헬퍼 메서드: 횡슬립(Drift) 각도 계산
    // 차가 바라보는 방향과 실제 진행 방향 사이의 각도
    public float GetSlipAngle()
    {
        if (ChassisData.speedMs < 2.0f) return 0f; // 너무 느리면 계산 안 함

        // 아크탄젠트로 횡방향 비율 계산 (라디안 -> 도)
        return Mathf.Atan2(ChassisData.localVelocity.x, ChassisData.localVelocity.z) * Mathf.Rad2Deg;
    }
}