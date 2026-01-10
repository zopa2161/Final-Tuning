using UnityEngine;
using UnityEngine.UIElements;

// ==================================================================================
// 개별 바퀴 유닛 제어 클래스
// ==================================================================================
public class WheelUnitWidget
{
    private Label _nameLabel;

    private Label _rpmLabel;

    private Label _driveTrqLabel;

    private Label _brakeTrqLabel;

    private VisualElement _steeringPivot; // 타이어 회전용

    private VisualElement _arrowPivot; // 슬립 화살표 회전용

    private VisualElement _arrowGraphic; // 슬립 색상 변경용

    private int _wheelIndex;

    public int WheelIndex
    {
        get => _wheelIndex;
        set => _wheelIndex = value;
    }

    public WheelUnitWidget(VisualElement root, string name)
    {
        _nameLabel = root.Q<Label>("wheel-name-lbl");
        _nameLabel.text = name; // FL, FR 등 이름 설정

        _rpmLabel = root.Q<Label>("wheel-rpm-lbl");
        _driveTrqLabel = root.Q<Label>("drive-trq-lbl");
        _brakeTrqLabel = root.Q<Label>("brake-trq-lbl");

        _steeringPivot = root.Q<VisualElement>("steering-pivot");
        _arrowPivot = root.Q<VisualElement>("slip-arrow-pivot");
        _arrowGraphic = root.Q<VisualElement>("slip-arrow-graphic");
    }

    /// <summary>
    /// 바퀴 상태 업데이트
    /// </summary>
    /// <param name="rpm">현재 RPM</param>
    /// <param name="driveTorque">구동 토크</param>
    /// <param name="brakeTorque">브레이크 토크</param>
    /// <param name="slipVector">슬립 벡터 (x: 횡방향, y: 종방향) - 타이어 로컬 기준</param>
    /// <param name="steeringAngleDeg">조향 각도 (도)</param>
    public void Update(VehicleContext context)
    {
        float steerAngle = context.StateHub.Steering.GetSteer(_wheelIndex);
        float sidewaysSlip = context.SensorHub.wheelSensor.SensorDatas[_wheelIndex].sidewaysSlip; // 0(Grip) ~ 1(Slip) 가정
        float forwardSlip = context.SensorHub.wheelSensor.SensorDatas[_wheelIndex].forwardSlip; // 0(Grip) ~ 1(Slip) 가정
 
        var rpm = context.SensorHub.wheelSensor.SensorDatas[_wheelIndex].rpm;
        var torque = context.StateHub.Drivetrain.Torques[_wheelIndex];
        var brakeTorque = context.StateHub.Brake.GetTorque(_wheelIndex);

        // 1. 텍스트 정보 업데이트
        _rpmLabel.text = $"{rpm:F0} RPM";
        _driveTrqLabel.text = $"D: {torque:F0}";
        _brakeTrqLabel.text = $"B: {brakeTorque:F0}";

        // 2. 조향 각도 적용 (앞바퀴만 해당되지만, 뒷바퀴는 0도 넣으면 됨)
        // UI Toolkit에서 회전 적용 (스타일의 rotate 속성 변경)
        _steeringPivot.style.rotate = new Rotate(Angle.Degrees(steerAngle));

    
        
        // 1. 슬립 각도 계산 (도 단위)
        float slipAngle = Mathf.Atan2(sidewaysSlip, forwardSlip) * Mathf.Rad2Deg;
        float slipMagnitude = new Vector2(sidewaysSlip, forwardSlip).magnitude; 
        _arrowPivot.style.rotate = new Rotate(new Angle(slipAngle-180f, AngleUnit.Degree));
        Color slipColor = Color.Lerp(Color.green, Color.red, Mathf.Clamp01(slipMagnitude / 0.5f)); 


        if (slipMagnitude < 0.1f) // 슬립이 거의 없으면 (각도가 아니라 크기로 판단 추천)
        {
            slipColor.a = 0.2f;
            
        }
        else
        {
            slipColor.a = 1.0f;
        }
        _arrowGraphic.style.backgroundColor = slipColor;
        // 실제 이미지 사용 시: _arrowGraphic.style.unityBackgroundImageTintColor = slipColor;
    }


}

// ==================================================================================
// 전체 메인 위젯 클래스
// ==================================================================================
public class WheelWidget : PipelineWidgetBase
{
    private WheelUnitWidget[] _units;

    public WheelUnitWidget[] Units => _units;
    public WheelWidget(VisualElement root) : base(root)
    {
        _units = new WheelUnitWidget[4];
        // UXML 템플릿 인스턴스 이름으로 찾기
        _units[0] = new WheelUnitWidget(root.Q("wheel-fl"), "FL");
        _units[0].WheelIndex = 0;
        _units[1] = new WheelUnitWidget(root.Q("wheel-fr"), "FR");
        _units[1].WheelIndex = 1;
        _units[2] = new WheelUnitWidget(root.Q("wheel-rl"), "RL");
        _units[2].WheelIndex = 2;
        _units[3] = new WheelUnitWidget(root.Q("wheel-rr"), "RR");
        _units[3].WheelIndex = 3;
    }
    

    public override void Update(VehicleContext context)
    {
        
    }
}

