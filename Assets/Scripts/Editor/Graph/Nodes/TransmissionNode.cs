using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

public class TransmissionNode : BaseCarModuleNode
{
    private FloatField _torqueInField;
    private FloatField _rpmInField;
    private FloatField _finalTorqueOutField;
    
    private IntegerField _currentGearField;
    private FloatField _currentRatioField;
    private FloatField _feedbackRpmField;
    
    private ProgressBar _shiftingBar;

  

    public TransmissionNode() 
    {
        title = "Transmission";

        // 입력 포트
        var torqueInPort = CreatePort("Torque In", Direction.Input, Port.Capacity.Single, typeof(float));
        _torqueInField = CreateValueField();
        torqueInPort.Add(_torqueInField);
        inputContainer.Add(torqueInPort);

        var rpmInPort = CreatePort("RPM In", Direction.Input, Port.Capacity.Single, typeof(float));
        _rpmInField = CreateValueField();
        rpmInPort.Add(_rpmInField);
        inputContainer.Add(rpmInPort);

        // 출력 포트
        var finalTorquePort = CreatePort("Final Torque Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _finalTorqueOutField = CreateValueField();
        finalTorquePort.Add(_finalTorqueOutField);
        outputContainer.Add(finalTorquePort);
        var feedbackPort = CreatePort("Feedback Out",Direction.Output, Port.Capacity.Single, typeof(float));
        _feedbackRpmField = CreateValueField();
        feedbackPort.Add(_feedbackRpmField);
        outputContainer.Add(feedbackPort);

        // 데이터 표시 필드
        _currentGearField = new IntegerField("Current Gear");
        _currentGearField.SetEnabled(false);
        mainContainer.Add(_currentGearField);

        _currentRatioField = new FloatField("Current Ratio");
        _currentRatioField.SetEnabled(false);
        mainContainer.Add(_currentRatioField);
        
        
        //=== 상태값==
       
        _shiftingBar = new ProgressBar();
        _shiftingBar.highValue = 1.0f;
        _shiftingBar.lowValue = 0.0f;
        
        var container = new VisualElement();

        container.Add(_shiftingBar);
        
        mainContainer.Add(container);
        
        RefreshExpandedState();
        RefreshPorts();
    }

    public override void Bind(ITunableVehicle vehicle)
    {
        base.Bind(vehicle);
        this.ModuleData = vehicle.ModuleMap[typeof(TransmissionModule)];
    }

    public override void Render()
    {
        if (_context == null) return;
        _rpmInField.value = Mathf.Round(this._context.StateHub.Engine.currentRPM);
        _currentGearField.value = this._context.StateHub.Transmission.currentGearIndex;
        _currentRatioField.value = this._context.StateHub.Transmission.currentRatio;
        _feedbackRpmField.value = this._context.StateHub.Transmission.feedbackRPM;
        _finalTorqueOutField.value = this._context.StateHub.Transmission.finalOutputTorque;
            
        var timer =this._context.StateHub.Transmission.shiftTimer / this._context.StateHub.Transmission.shiftTime;
        _shiftingBar.value = timer;
    }
}