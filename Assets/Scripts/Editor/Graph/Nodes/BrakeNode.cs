using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class BrakeNode : BaseCarModuleNode
{
    private FloatField _brakeInField;
    private FloatField _sleepValueInField;
    private FloatField _brakeTorqueFLField;
    private FloatField _brakeTorqueFRField;
    private FloatField _brakeTorqueRLField;
    private FloatField _brakeTorqueRRField;

    public BrakeNode()
    {
        title = "Brake Module";
        
        // 입력 포트
        var brakeInPort = CreatePort("Brake In", Direction.Input, Port.Capacity.Single, typeof(float));
        _brakeInField = CreateValueField();
        brakeInPort.Add(_brakeInField);
        inputContainer.Add(brakeInPort);

        var sleepValueInPort = CreatePort("Sleep Value In", Direction.Input, Port.Capacity.Single, typeof(float));
        _sleepValueInField = CreateValueField();
        sleepValueInPort.Add(_sleepValueInField);
        inputContainer.Add(sleepValueInPort);

        // 출력 포트
        var brakeTorqueFLPort = CreatePort("Brake Torque FL Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _brakeTorqueFLField = CreateValueField();
        brakeTorqueFLPort.Add(_brakeTorqueFLField);
        outputContainer.Add(brakeTorqueFLPort);

        var brakeTorqueFRPort = CreatePort("Brake Torque FR Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _brakeTorqueFRField = CreateValueField();
        brakeTorqueFRPort.Add(_brakeTorqueFRField);
        outputContainer.Add(brakeTorqueFRPort);

        var brakeTorqueRLPort = CreatePort("Brake Torque RL Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _brakeTorqueRLField = CreateValueField();
        brakeTorqueRLPort.Add(_brakeTorqueRLField);
        outputContainer.Add(brakeTorqueRLPort);

        var brakeTorqueRRPort = CreatePort("Brake Torque RR Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _brakeTorqueRRField = CreateValueField();
        brakeTorqueRRPort.Add(_brakeTorqueRRField);
        outputContainer.Add(brakeTorqueRRPort);
        
        RefreshExpandedState();
        RefreshPorts();
    }

    public override void Bind(ITunableVehicle vehicle)
    {
        base.Bind(vehicle);
        this.ModuleData = vehicle.ModuleMap[typeof(BrakeModule)];
    }

    public override void Render()
    {
        if (_context == null) return;
        _brakeInField.value = (float)System.Math.Round(this._context.Brake, 2);
        _brakeTorqueFLField.value = (float)System.Math.Round(this._context.StateHub.Brake.torqueFL, 2);
        _brakeTorqueFRField.value = (float)System.Math.Round(this._context.StateHub.Brake.torqueFR, 2);
        _brakeTorqueRLField.value = (float)System.Math.Round(this._context.StateHub.Brake.torqueRL, 2);
        _brakeTorqueRRField.value = (float)System.Math.Round(this._context.StateHub.Brake.torqueRR, 2);
    }
}