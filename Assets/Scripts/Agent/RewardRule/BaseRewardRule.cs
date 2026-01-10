using UnityEngine;
using System;

[Serializable]
public abstract class BaseRewardRule
{
    [Header("Base Settings")]
    public bool isEnabled = true;
    public float weight = 1.0f; // 보상 가중치 (중요도)

    // [핵심] 입력을 받아 보상(float)을 배출
    public abstract RewardResult CalculateReward(RewardInputData input);

    // (옵션) 에피소드 시작 시 상태 초기화가 필요하다면 (예: 누적 거리 초기화)
    public virtual void OnEpisodeBegin() { }
}