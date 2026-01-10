using System;
using UnityEngine;

public class CollisionProxy : MonoBehaviour
{
    public event Action<CollisionProxy, Collision> OnHit;
    public event Action<CollisionProxy, Collider> OnTrigger;
    public string partID = "Body";

    // [기존] 유니티가 직접 호출 (Rigidbody가 있는 경우)
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    // [New] 부모(Dispatcher)가 배달해 줄 때 호출
    public void OnDispatchCollision(Collision collision)
    {
        HandleCollision(collision);
    }

    public void OnDispatchTrigger(Collider other)
    {
        HandleTrigger(other);
    }

    // 공통 처리 로직
    private void HandleCollision(Collision collision)
    {
        // 구독자들에게 알림 (Agent, EventManager 등)
        OnHit?.Invoke(this, collision);
    }

    private void HandleTrigger(Collider other)
    {
        OnTrigger?.Invoke(this, other);
    }
}