using UnityEngine.UIElements;

public class BrakeWidget : PipelineWidgetBase
{

    private ProgressBar _frontBrakeTorque;
    private ProgressBar _rearBrakeTorque;
    private IndicatorLamp _absLamp;

    public BrakeWidget(VisualElement root) : base(root)
    {
        _frontBrakeTorque = root.Q<ProgressBar>("front-bar");
        _rearBrakeTorque = root.Q<ProgressBar>("rear-bar");
        _absLamp = root.Q<IndicatorLamp>("abs-lamp");
        
    }

    public override void Update(VehicleContext context)
    {
        var state = context.StateHub.Brake;
        _frontBrakeTorque.value = state.torqueFL / (state.maxBrakeTorque/2);
        _frontBrakeTorque.title = state.torqueFL.ToString("F0");
        _rearBrakeTorque.value = state.torqueRL / (state.maxBrakeTorque/2);
        _rearBrakeTorque.title = state.torqueRL.ToString("F0");
        _absLamp.IsOn = state.isABSActive;
        
    }
}