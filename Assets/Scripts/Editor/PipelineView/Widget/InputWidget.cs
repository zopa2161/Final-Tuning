using UnityEngine.UIElements;

public class InputWidget : PipelineWidgetBase
{
    private VisualElement _steeringWheel;

    private VerticalGauge _throttle;
    private VerticalGauge _brake;
    
    public InputWidget(VisualElement root) : base(root)
    {
        _steeringWheel = root.Q<VisualElement>("steering-wheel");
        _throttle = root.Q<VerticalGauge>("throttle-gauge");
        _brake = root.Q<VerticalGauge>("brake-gauge");
    }

    public override void Update(VehicleContext context)
    {

        var targetAngle = context.SteeringAngle * 90f;
        _steeringWheel.style.rotate = new Rotate(new Angle(targetAngle, AngleUnit.Degree));
        _throttle.Value= context.Throttle;
        _brake.Value = context.Brake;
    }
}