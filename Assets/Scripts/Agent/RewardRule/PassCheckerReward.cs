[System.Serializable]
public class PassCheckerReward : BaseRewardRule
{
    public override RewardResult CalculateReward(RewardInputData input)
    {
        var reward = 0f;
        var trigger = input.VehicleCtx.SensorHub.TriggerResultByName["Body"];
        var current = input.CurrentCheckerNumber;
        if (trigger.isPass)
        {
            if (trigger.passedCheckerNumber == current)
            {
                reward = 1f * weight;
            }
            else

            {
                reward = -1f * weight;
            }
        }

        return new RewardResult()
        {
            reward = reward,
            endEpisode = false
        };
    }
        
}