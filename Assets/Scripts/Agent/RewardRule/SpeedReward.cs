using UnityEngine;

public class SpeedReward : BaseRewardRule
{
    [SerializeField]
    private float minSpeedTresholdKm = 10f;

    [SerializeField]
    private float maxSpeedTresholdKm = 100f;

   

    public override RewardResult CalculateReward(RewardInputData input)
    {
        var speedKm = input.VehicleCtx.SensorHub.ChassisSensor.ChassisData.speedKmh;

        var reward = speedKm < minSpeedTresholdKm
            ? 0f
            : this.weight * Mathf.Clamp(speedKm, minSpeedTresholdKm, maxSpeedTresholdKm);

        return new RewardResult()
        {
            reward = reward,
            endEpisode = false
        };
    }
}