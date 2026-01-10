using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[System.Serializable]
public class ChassisSensorData
{
    // --- [1] 속도 그룹 ---
    public float speedKmh;           // 시속
    public float speedMs;            // 초속
    public float forwardSpeed;       // 전진 속도 (+:전진, -:후진)
    public float sideSpeed;          // 횡이동 속도 (드리프트 시 중요)

    // --- [2] 벡터 그룹 (월드/로컬) ---
    public Vector3 velocityVector;   // 월드 속도
    public Vector3 localVelocity;    // 로컬 속도 (x:횡, z:종)
    public Vector3 angularVelocity;  // 회전 속도 (Local) - Pitch/Roll/Yaw Rate

    // --- [3] 가속도 그룹 (G-Force) ---
    public Vector3 worldAcceleration; // 월드 가속도
    public Vector3 localAcceleration; // 로컬 가속도 (운전자가 느끼는 G값: x=횡G, z=가속/감속G)

    // --- [4] 자세 & 안정성 (Stability) ---
    public float slipAngle;          // 차체 미끄러짐 각도 (Drift Angle, Degree)
    public float pitchAngle;         // 언덕 경사각 (-90 ~ 90)
    public float rollAngle;          // 좌우 기울기 (전복 위험 감지용)
    public float yawAngle;           // 차체 회전 각도 (0 ~ 360)
    public Vector3 upVector;         // 월드 기준 차의 윗방향 (전복 감지용: y가 음수면 뒤집힘)

    
    // -------------------------------------------------------------
    // [ML-Agents용] 관찰값 배열 캐싱 (GC 방지)
    // -------------------------------------------------------------
    // 배열 크기: 3(로컬속도) + 3(각속도) + 3(로컬가속도) + 3(자세) + 1(슬립) + 1(Yaw) = 14개 (예시)
    public float[] observationArray = new float[14];

    // 센싱이 끝난 후 이 함수를 호출하면 배열이 최신화됨
    public void UpdateObservationArray()
    {
        // 1. 로컬 속도 (정규화 필요 시 여기서 나누기)
        observationArray[0] = localVelocity.x;
        observationArray[1] = localVelocity.y;
        observationArray[2] = localVelocity.z;

        // 2. 각속도 (Yaw Rate 등)
        observationArray[3] = angularVelocity.x;
        observationArray[4] = angularVelocity.y;
        observationArray[5] = angularVelocity.z;

        // 3. 로컬 가속도 (G-Force)
        observationArray[6] = localAcceleration.x;
        observationArray[7] = localAcceleration.y;
        observationArray[8] = localAcceleration.z;

        // 4. 자세 (Orientation) - 정규화된 Up Vector 추천
        observationArray[9] = upVector.x;
        observationArray[10] = upVector.y;
        observationArray[11] = upVector.z;

        // 5. 슬립 앵글 (라디안이나 정규화된 값 추천)
        observationArray[12] = slipAngle / 180f; // -1 ~ 1 로 정규화
        
        // 6. Yaw 앵글 (0~1 정규화)
        observationArray[13] = yawAngle / 360f;
    }
    
    public void ResetEpisode()
    {
        localVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        localAcceleration = Vector3.zero;
        upVector = Vector3.up;
        slipAngle = 0f;
        yawAngle = 0f;
        pitchAngle = 0f;
        rollAngle = 0f;
        speedKmh = 0f;
        speedMs = 0f;
        forwardSpeed = 0f;
        sideSpeed = 0f;
        observationArray = new float[14];
    }
}