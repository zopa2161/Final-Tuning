using UnityEngine.UIElements;

public class TransmissionWidget : PipelineWidgetBase
{
    private Label _gearIndex;
    private Label _gearRatio;

    private ProgressBar _rpm;
    private ProgressBar _shiftProgress;
    private ProgressBar _clutch;
    
    private VerticalGauge _torque;
    private Label _torqueText;
    
    public TransmissionWidget(VisualElement root) : base(root)
    {
        _gearIndex = root.Q<Label>("gear-index-lbl");
        _gearRatio = root.Q<Label>("gear-ratio-lbl");
        
        _rpm = root.Q<ProgressBar>("rpm-bar");
        _shiftProgress = root.Q<ProgressBar>("shift-bar");
        _clutch = root.Q<ProgressBar>("clutch-bar");
        
        _torque = root.Q<VerticalGauge>("trans-torque-gauge");
        
        _torqueText = root.Q<Label>("torque-val-lbl");
    }

    public override void Update(VehicleContext context)
    {
        var transMission = context.StateHub.Transmission;
        _gearIndex.text = transMission.currentGearIndex.ToString();
        _gearRatio.text = transMission.currentRatio.ToString();

        var maxRpm = context.StateHub.Engine.maxRPM;
        var gearInfos = transMission.gearInfos;

        var currentLowThreshold = maxRpm * gearInfos[transMission.currentGearIndex - 1].downshiftRpmPercent; 
        var currentHighThreshold = maxRpm * gearInfos[transMission.currentGearIndex -1].upshiftRpmPercent;

        var value = (transMission.feedbackRPM - currentLowThreshold) / (currentHighThreshold - currentLowThreshold);
        if (value > 0) _rpm.value = value;
        else _rpm.value = 0;
        _shiftProgress.value = transMission.shiftTimer / transMission.shiftTime;
        _clutch.value = transMission.clutchRatio;

        _torque.Value = transMission.finalOutputTorque/1000;
        _torqueText.text = transMission.finalOutputTorque.ToString("F0") + " Nm";
    }
}