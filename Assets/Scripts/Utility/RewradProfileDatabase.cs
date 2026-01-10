using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardProfileDatabase", menuName = "ScriptableObjects/RewardProfileDatabase")]
public class RewardProfileDatabase : SerializedScriptableObject
{
    [SerializeField]
    private List<RewardProfile> _datas = new List<RewardProfile>();

    public List<RewardProfile> Datas
    {
        get => _datas;
        set => _datas = value;
    }

    [ShowInInspector, ReadOnly]
    private Dictionary<int, RewardProfile> rewardProfileById = new Dictionary<int, RewardProfile>();

    private void OnValidate()
    {
        rewardProfileById.Clear();
        foreach (var profile in _datas)
        {
            if (profile != null && !rewardProfileById.ContainsKey(profile.ID))
            {
                rewardProfileById.Add(profile.ID, profile);
            }
        }
    }

    public RewardProfile GetProfileById(int id)
    {
        rewardProfileById.TryGetValue(id, out var profile);
        return profile;
    }

    public void Add(RewardProfile profile)
    {
        _datas.Add(profile);
        SetID(profile, _datas.Count - 1);
    }
    
    private void SetID(RewardProfile target, int id)
    {

        target.ID = id;
#if UNITY_EDITOR
        EditorUtility.SetDirty(target);
#endif
    }
}