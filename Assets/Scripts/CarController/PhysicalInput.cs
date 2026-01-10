using UnityEngine;
using System;

[Serializable]
public class PhysicalInput
{
    
    private float _rise;
    private float _fall;
    
    public PhysicalInput(float rise, float fall)
    {
        _rise = rise;
        _fall = fall;
    }

    public float tick(float current, float target, float dt)
    {
        // 1. 데드존 설정 (약간 더 여유있게 0.001f ~ 0.01f 추천)
        float deadzone = 0.01f;

        // 2. Target이 데드존 안에 있으면, 목표를 완벽한 0으로 고정합니다.
        // (Current는 검사하지 않습니다. Current가 0이 될 때까지 부드럽게 움직여야 하니까요)
        if (Mathf.Abs(target) < deadzone)
        {
            target = 0f;
        }
        
        // 4. 기존 로직 수행 (이제 target이 0이므로 부드럽게 0으로 수렴함)
        var speed = target > current ? _rise : _fall;
        var currentValue = Mathf.MoveTowards(current, target, speed * dt);
    
        return currentValue;
    }

}