using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class WallCollisionPenalty : BaseRewardRule
{
    [BoxGroup("Reward Settings")] 
    public float frontCollisionPenalty = -1f;
    [BoxGroup("Reward Settings")] 
    public float rearCollisionPenalty = -1f;
    [BoxGroup("Reward Settings")] 
    public float sideCollisionPenalty = -1f;

    public override RewardResult CalculateReward(RewardInputData input)
    {
        var result = new RewardResult();
        var bodyCollision = input.CollisionResultByName["Body"];
        if (bodyCollision.isCollision)
        {
            if (bodyCollision.collisionType == CollisionType.Wall)
            {
                if (bodyCollision.dir == ImpactDirection.Front)
                {
                    result.endEpisode = true;
                    result.reward = frontCollisionPenalty * weight;
                    

                }
                else
                {
                    result.reward = sideCollisionPenalty * weight;
                    result.endEpisode = true;
                }
            }
            
            //Debug.Log("Collision detected" + bodyCollision.dir);
        }
        return result;
    }

}