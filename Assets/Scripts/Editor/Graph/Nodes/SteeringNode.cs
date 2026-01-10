using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SteeringNode : BaseCarModuleNode
{
    private FloatField _fieldSteeringIn;
    private FloatField _fieldAngleFL;
    private FloatField _fieldAngleFR;
    
    public SteeringNode() 
    {
        title = "Steering Module";

        // Input Ports
        // Input
        var pIn = CreatePort("Steering In", Direction.Input, Port.Capacity.Single, typeof(float));
        _fieldSteeringIn = CreateValueField(); 
        pIn.contentContainer.Add(_fieldSteeringIn); // 포트 안에 필드 추가
        inputContainer.Add(pIn);

        // Output FL
        var pOutFL = CreatePort("Steer Angle FL Out", Direction.Output, Port.Capacity.Single, typeof(float));
        _fieldAngleFL = CreateValueField();
        pOutFL.contentContainer.Add(_fieldAngleFL); // 포트 안에 필드 추가
        outputContainer.Add(pOutFL);

        // Output FR
        var pOutFR = CreatePort("Steer Angle FR Out", Direction.Output, Port.Capacity.Single, typeof(float));
        _fieldAngleFR = CreateValueField();
        pOutFR.contentContainer.Add(_fieldAngleFR);
        outputContainer.Add(pOutFR);

        RefreshExpandedState();
        RefreshPorts();
    }
    public override void Bind(ITunableVehicle vehicle)
    {
        base.Bind(vehicle);
        this.ModuleData = vehicle.ModuleMap[typeof(SteeringModule)];
    }
    public override void Render()
    {
        if (_context == null) return;
        // 입력값(Input)은 작으니까 소수점 2자리까지 표시 (0.54)
        _fieldSteeringIn.value = (float)System.Math.Round(_context.SteeringAngle, 2);

        // 바퀴 각도는 굳이 소수점 필요 없으니 정수로 잘라서 넣기 (35.0)
        _fieldAngleFL.value = Mathf.Round(_context.StateHub.Steering.finalSteerAngleFL);
        _fieldAngleFR.value = Mathf.Round(_context.StateHub.Steering.finalSteerAngleFR);
       
        //밑에는 값 시각화 로직.
    }
   
}
