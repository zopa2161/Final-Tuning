using Codice.CM.Common;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EngineNode : BaseCarModuleNode
{
    private FloatField _fieldThrottleIn;
    private FloatField _fieldFeedbackRpmIn;
    private FloatField _fieldRawTorqueOut;
    private FloatField _fieldRpmOut;
    
    private Port _rpmOutPort;     // 포트 참조를 미리 캐싱해두면 좋음
    private Port _torqueOutPort;
    
    private CircularGauge _rpmGauge;

    private Label _rpmText;

    public EngineNode() 
    {
        title = "Engine Module";
        
        var container = new VisualElement();
        container.style.alignItems = Align.Center; // 가운데 정렬
        container.style.justifyContent = Justify.Center;
        container.style.height = 120;
        
        // 2. 게이지 생성 및 스타일
        _rpmGauge = new CircularGauge();
        _rpmGauge.style.width = 100;
        _rpmGauge.style.height = 100;
        _rpmGauge.ProgressColor = new Color(1f, 0.3f, 0.3f); // 빨간색 (RPM 느낌)
        
        // 3. 텍스트 생성 (게이지 위에 겹쳐 보이게 하려면 position: absolute 사용)
        _rpmText = new Label("0");
        _rpmText.style.position = Position.Absolute;
        _rpmText.style.fontSize = 18;
        _rpmText.style.color = Color.white;
        _rpmText.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        // 4. 조립
        container.Add(_rpmGauge);
        container.Add(_rpmText); // 나중에 추가한 게 위에 그려짐
        
        // 노드의 메인 컨테이너에 추가 (ExtensionContainer에 넣어도 됨)
        mainContainer.Add(container);
        // 입력 포트
        var pInThrottle = CreatePort("Throttle In", Direction.Input, Port.Capacity.Single, typeof(float));
        _fieldThrottleIn = CreateValueField();
        pInThrottle.Add(_fieldThrottleIn);
        inputContainer.Add(pInThrottle);

        var pInFeedbackRpm = CreatePort("Feedback Rpm In", Direction.Input, Port.Capacity.Single, typeof(int));
        _fieldFeedbackRpmIn = CreateValueField();
        pInFeedbackRpm.Add(_fieldFeedbackRpmIn);
        inputContainer.Add(pInFeedbackRpm);
        
        // 출력 포트
        var pOutTorque = CreatePort("Torque Out", Direction.Output, Port.Capacity.Single, typeof(float));
        _fieldRawTorqueOut = CreateValueField();
        pOutTorque.Add(_fieldRawTorqueOut);
        outputContainer.Add(pOutTorque);

        var pOutRpm = CreatePort("Rpm Out", Direction.Output, Port.Capacity.Single, typeof(float));
        _fieldRpmOut = CreateValueField();
        pOutRpm.Add(_fieldRpmOut);
        outputContainer.Add(pOutRpm);

        _rpmOutPort = pOutRpm;
        _torqueOutPort = pOutTorque;
        
        // 변경사항 반영
        RefreshExpandedState();
        RefreshPorts();
    }
    public override void Bind(ITunableVehicle vehicle)
    {
        base.Bind(vehicle);
        this.ModuleData = vehicle.ModuleMap[typeof(EngineModule)];
    }
    public override void Render()
    {
        if (_context == null) return;
        _fieldThrottleIn.value = (float)System.Math.Round(_context.Throttle, 2);
        _fieldFeedbackRpmIn.value = Mathf.Round(this._context.StateHub.Transmission.feedbackRPM);
        _fieldRawTorqueOut.value = (float)System.Math.Round(_context.StateHub.Engine.generatedTorque);
        _fieldRpmOut.value = Mathf.Round(this._context.StateHub.Engine.currentRPM);
        
        //rpm그리기 로직
        float currentRPM = _context.StateHub.Engine.currentRPM;
        float maxRPM = this._context.StateHub.Engine.maxRPM;

        // 1. 게이지 갱신 (0.0 ~ 1.0 정규화)
        _rpmGauge.Value = Mathf.Clamp01(currentRPM / maxRPM);
        _rpmText.text = $"{currentRPM:F0}";

        Color targetColor;

        if (_rpmGauge.Value  < 0.5f)
        {
            // 0% ~ 50%: 초록 -> 노랑
            // (비율을 0~1로 다시 맞추기 위해 * 2)
            targetColor = Color.Lerp(Color.green, Color.yellow, _rpmGauge.Value  * 2f);
        }
        else
        {
            // 50% ~ 100%: 노랑 -> 빨강
            // (비율을 0~1로 맞추기 위해 (ratio - 0.5) * 2)
            targetColor = Color.Lerp(Color.yellow, Color.red, (_rpmGauge.Value - 0.5f) * 2f);
        }
        _rpmText.style.color = new StyleColor(targetColor); 
        _rpmGauge.style.color = new StyleColor(targetColor);
        
        //UpdatePortEdges(_rpmOutPort, currentRPM, 0, maxRPM);
        UpdatePortEdges(_torqueOutPort,_fieldRawTorqueOut.value,0,100);
    }
}