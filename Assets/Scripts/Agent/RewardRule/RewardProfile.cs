using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ML-Agents/Reward Profile")]
public class RewardProfile : SerializedScriptableObject
{
    [SerializeField]
    public string Name;

    public int ID;
    
    [Title("Reward Rules")]
    [SerializeReference] // 다형성 직렬화의 핵심!
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<BaseRewardRule> rules = new List<BaseRewardRule>();
    
    // (Odin용 헬퍼: 인스펙터에서 클래스 이름 예쁘게 보이게)
    private string GetTypeString(BaseRewardRule rule) => rule.GetType().Name;

    public RewardResult RewardStep(RewardInputData data)
    {
        var results = rules.Where(r => r.isEnabled).Select(r => r.CalculateReward(data)).ToList();
        if (results.Any(r => r .endEpisode))
        {
            //Debug.Log("endEpisode true");
        }
        return new RewardResult
        {
            reward = results.Sum(r => r.reward),
            endEpisode = results.Any(r => r.endEpisode)
            
        };
    }
}