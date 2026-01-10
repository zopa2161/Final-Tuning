using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Tuning/Steering Module")]
public class SteeringModule : BaseModule
{
    [BoxGroup("Basic Specs")]
    [SerializeField]
    [Range(0f, 90f)]
    private float _maxSteerAngle = 35f;
    
    [BoxGroup("Geometry")]
    [Tooltip("애커만 비율 (0 = 평행 조향, 1 = 완전 애커만). 높을수록 안쪽 바퀴가 더 많이 꺾임.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _ackermannFactor = 0.5f;

    [BoxGroup("Geometry")]
    [Tooltip("안쪽 바퀴 추가 각도 비율 (애커만 강도). 보통 1.1~1.3배.")]
    [SerializeField]
    private float _innerWheelMultiplier = 1.2f;
    
    [BoxGroup("Safety")]
    [Tooltip("속도 감응형 조향 제한 곡선 (X: km/h, Y: 0~1 비율). 고속에서 각도 제한.")]
    [SerializeField]
    
    private AnimationCurve _speedSensitivity = new AnimationCurve(
        new Keyframe(0f, 1f),    // 정지 시 100%
        new Keyframe(100f, 0.5f), // 100km/h에서 50%만 허용
        new Keyframe(300f, 0.2f)  // 300km/h에서 20%만 허용
    );
    
    public override void Calculate(VehicleContext context)
    {
        SteeringState myState = context.StateHub.Steering;
        if (myState == null) return;

        float rawInput = context.SteeringAngle; // 에이전트에 의해 움직인 일반 값.
        
        // 2. 속도 감응형 제한 적용 (Speed Sensitivity)
        // 속도가 빠를수록 핸들이 덜 돌아가게 만듦 (안전장치)
        float speedFactor = _speedSensitivity.Evaluate(context.SensorHub.ChassisSensor.ChassisData.speedKmh);
        float currentMaxAngle = _maxSteerAngle * speedFactor;
        
        // 3. 기본 목표 각도 계산
        // (입력 * 현재 허용된 최대각)
        float baseAngle = rawInput * currentMaxAngle;
        
        // 4. 애커만 지오메트리 (Ackermann Geometry) 적용
        // 안쪽 바퀴(Inner Wheel)를 더 많이 꺾어야 함.
        
        float angleFL = baseAngle;
        float angleFR = baseAngle;

        // _ackermannFactor가 0이면 평행(Parallel), 1이면 계산된 값 적용
        float ackermannBonus = (_innerWheelMultiplier - 1.0f) * _ackermannFactor;

        if (rawInput > 0) // 우회전 (Right Turn)
        {
            // 오른쪽(FR)이 안쪽 바퀴 -> 더 많이 꺾임
            // angleFR은 양수(+) 상태이므로 더 키워줌
            angleFR *= (1.0f + ackermannBonus);
        }
        else if (rawInput < 0) // 좌회전 (Left Turn)
        {
            // 왼쪽(FL)이 안쪽 바퀴 -> 더 많이 꺾임
            // angleFL은 음수(-) 상태이므로 절대값을 키워줌
            angleFL *= (1.0f + ackermannBonus);
        }
        
        myState.finalSteerAngleFL = angleFL;
        myState.finalSteerAngleFR = angleFR;
        
        // (4WS가 없다면 후륜은 0도 유지)
        myState.finalSteerAngleRL = 0f;
        myState.finalSteerAngleRR = 0f;
    }

    public override void OnCalculate()
    {
        throw new System.NotImplementedException();
    }

    public override BaseModuleState CreateState()
    {
        return new SteeringState();
    }
}
