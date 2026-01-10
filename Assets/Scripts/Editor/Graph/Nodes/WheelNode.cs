using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class WheelNode : BaseCarModuleNode
{
    private int _wheelIndex; // 0:FL, 1:FR, 2:RL, 3:RR
    private VehicleContext _context;

    // UI 요소
    private VisualElement _tireGraphic; // 회전할 타이어 모양
    private CircularGauge _wheelRpm;
    private Label _rpmText;
    private Label _slipLabel;
    
    private FloatField _fieldTorqueIn;
    private FloatField _fieldBrakeTorqueIn;

    public int WheelIndex => _wheelIndex;

    public WheelNode(int index) 
    {
        _wheelIndex = index;
        title = GetWheelName(index);

        // 1. 타이어 그래픽 컨테이너 (중앙 정렬)
        var graphicContainer = new VisualElement();
        graphicContainer.style.height = 100;
        graphicContainer.style.justifyContent = Justify.Center;
        graphicContainer.style.alignItems = Align.Center;

        // 2. 타이어 모양 (직사각형)
        _tireGraphic = new VisualElement();
        _tireGraphic.style.width = 40;
        _tireGraphic.style.height = 70;
        _tireGraphic.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // 타이어 색 (검정)
        _tireGraphic.style.borderTopLeftRadius = 5;
        _tireGraphic.style.borderTopRightRadius = 5;
        _tireGraphic.style.borderBottomLeftRadius = 5;
        _tireGraphic.style.borderBottomRightRadius = 5;
        //_tireGraphic.style.borderColor = 2;
        _tireGraphic.style.borderBottomColor = Color.gray;

        graphicContainer.Add(_tireGraphic);
        mainContainer.Add(graphicContainer);

        // 3. 정보 표시
        _slipLabel = new Label("Grip: 1.0");

        // 스타일: 노드 크기를 좀 작게
        style.width = 200;
        
        //=== Wheel RPM 설정 ===
        var container = new VisualElement();
        container.style.alignItems = Align.Center; // 가운데 정렬
        container.style.justifyContent = Justify.Center;
        container.style.height = 120;
        
        // 2. 게이지 생성 및 스타일
        _wheelRpm = new CircularGauge();
        _wheelRpm.style.width = 100;
        _wheelRpm.style.height = 100;
        _wheelRpm.ProgressColor = new Color(1f, 0.3f, 0.3f); // 빨간색 (RPM 느낌)
        
        // 3. 텍스트 생성 (게이지 위에 겹쳐 보이게 하려면 position: absolute 사용)
        _rpmText = new Label("0");
        _rpmText.style.position = Position.Absolute;
        _rpmText.style.fontSize = 18;
        _rpmText.style.color = Color.white;
        _rpmText.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        // 4. 조립
        container.Add(_wheelRpm);
        container.Add(_rpmText); // 나중에 추가한 게 위에 그려짐
        
        // 노드의 메인 컨테이너에 추가 (ExtensionContainer에 넣어도 됨)
        mainContainer.Add(container);
        mainContainer.Add(_slipLabel);

        var pInTorque = CreatePort("Torque In", Direction.Input, Port.Capacity.Single, typeof(float));
        _fieldTorqueIn = CreateValueField();
        pInTorque.Add(_fieldTorqueIn);
        inputContainer.Add(pInTorque);

        var pInBrakeTorque = CreatePort("Brake Torque In", Direction.Input, Port.Capacity.Single, typeof(float));
        _fieldBrakeTorqueIn = CreateValueField();
        pInBrakeTorque.Add(_fieldBrakeTorqueIn);
        inputContainer.Add(pInBrakeTorque);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override void Bind(ITunableVehicle vehicle)
    {
        _context = vehicle.Context;
    }

    public override void Render()
    {
        if (_context == null) return;

        // 1. 데이터 가져오기 (배열 인덱스 접근)
        float steerAngle = _context.StateHub.Steering.GetSteer(_wheelIndex);
        float slip = _context.SensorHub.wheelSensor.SensorDatas[_wheelIndex].sidewaysSlip; // 0(Grip) ~ 1(Slip) 가정
        var rpm = _context.SensorHub.wheelSensor.SensorDatas[_wheelIndex].rpm;
        var torque = _context.StateHub.Drivetrain.Torques[_wheelIndex];
        var brakeTorque = _context.StateHub.Brake.GetTorque(_wheelIndex);
        
        // 2. 조향 시각화 (회전)
        _tireGraphic.style.rotate = new Rotate(steerAngle);
        _wheelRpm.Value = Mathf.Clamp01(rpm / 1000);
        _rpmText.text = $"{rpm:F0}";
        _fieldTorqueIn.value = (float)System.Math.Round(torque, 2);
        _fieldBrakeTorqueIn.value = (float)System.Math.Round(brakeTorque, 2);

        // 3. 슬립 시각화 (색상 변경: 초록 -> 빨강)
        // 슬립이 0.2 넘어가면 붉어지기 시작
        Color gripColor = Color.green;
        Color slipColor = Color.red;
        _tireGraphic.style.borderBottomColor = Color.Lerp(gripColor, slipColor, slip * 2f); 

        // 4. 수치 갱신
        _slipLabel.text = $"Slip: {slip:F2}";
        //suspensionBar.value = suspension;
    }

    private string GetWheelName(int index)
    {
        return index switch
        {
            0 => "FL (Front-Left)",
            1 => "FR (Front-Right)",
            2 => "RL (Rear-Left)",
            3 => "RR (Rear-Right)",
            _ => $"Wheel {index}"
        };
    }
}