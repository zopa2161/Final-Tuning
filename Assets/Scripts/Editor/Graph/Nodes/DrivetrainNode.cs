using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DrivetrainNode : BaseCarModuleNode 
{
    private FloatField _finalTorqueInField;
    private FloatField _torqueFLField;
    private FloatField _torqueFRField;
    private FloatField _torqueRLField;
    private FloatField _torqueRRField;
    
    
    
    public DrivetrainNode() 
    {
        title = "Drivetrain Module";
        
        // 입력 포트
        var finalTorqueInPort = CreatePort("Final Torque In", Direction.Input, Port.Capacity.Single, typeof(float));
        _finalTorqueInField = CreateValueField();
        finalTorqueInPort.Add(_finalTorqueInField);
        inputContainer.Add(finalTorqueInPort);

        // 출력 포트
        var torqueFLPort = CreatePort("Torque FL Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _torqueFLField = CreateValueField();
        torqueFLPort.Add(_torqueFLField);
        outputContainer.Add(torqueFLPort);

        var torqueFRPort = CreatePort("Torque FR Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _torqueFRField = CreateValueField();
        torqueFRPort.Add(_torqueFRField);
        outputContainer.Add(torqueFRPort);

        var torqueRLPort = CreatePort("Torque RL Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _torqueRLField = CreateValueField();
        torqueRLPort.Add(_torqueRLField);
        outputContainer.Add(torqueRLPort);

        var torqueRRPort = CreatePort("Torque RR Out", Direction.Output, Port.Capacity.Multi, typeof(float));
        _torqueRRField = CreateValueField();
        torqueRRPort.Add(_torqueRRField);
        outputContainer.Add(torqueRRPort);
    
        
        // 변경사항 반영
        RefreshExpandedState();
        RefreshPorts();
    }

    public override void Bind(ITunableVehicle vehicle)
    {
        base.Bind(vehicle);
        this.ModuleData = vehicle.ModuleMap[typeof(DrivetrainModule)];
    }

    public override void Render()
    {
        if (_context == null) return;
        _finalTorqueInField.value = (float)System.Math.Round(this._context.StateHub.Transmission.finalOutputTorque,2);
        _torqueFLField.value = (float)System.Math.Round(this._context.StateHub.Drivetrain.Torques[0],2);
        _torqueFRField.value = (float)System.Math.Round(this._context.StateHub.Drivetrain.Torques[1],2);
        _torqueRLField.value = (float)System.Math.Round(this._context.StateHub.Drivetrain.Torques[2],2);
        _torqueRRField.value = (float)System.Math.Round(this._context.StateHub.Drivetrain.Torques[3],2);
        
        
        


    }
}