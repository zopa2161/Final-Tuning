using UnityEngine;

public class CollisionDispatcher : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 1. 실제로 부딪힌 내 차의 콜라이더를 찾음
        // (여러 접촉점 중 첫 번째 접촉점의 콜라이더를 기준으로 함)
        Collider myCollider = collision.GetContact(0).thisCollider;

        // 2. 그 콜라이더(자식)에 붙어있는 Proxy를 찾음
        var proxy = myCollider.GetComponent<CollisionProxy>();

        // 3. Proxy가 있다면, 강제로 이벤트를 발생시킴 (배달!)
        if (proxy != null)
        {
            // Proxy에 "야, 너 부딪혔대!"라고 알려주는 함수 호출
            proxy.OnDispatchCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var proxy = other.GetComponent<CollisionProxy>();
        if (proxy != null)
        {
            proxy.OnDispatchTrigger(other);
        }
    }
}