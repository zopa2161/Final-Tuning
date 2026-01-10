using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollsionDetector : IVehiclePartConfig,IVehiclePartRun,IDisposable
{
    private readonly CollisionProxy _proxy;
    private readonly string _partName;
    private CollisionResult _collisionResult;

    public CollsionDetector(CollisionProxy proxy,string partName)
    {
        _proxy = proxy;
        _proxy.OnHit += OnHit;
        _partName = partName;
        _collisionResult = new CollisionResult();
    }
    public void LinkSensor(SensorHub sensorHub)
    {
        sensorHub.CollisionResultByName[_partName] = _collisionResult;
    }
    public void ApplySettings(VehicleStateHub stateHub)
    {
        throw new NotImplementedException();
    }
    public void OnSensing()
    {
    }
    public void OnActuate(VehicleContext context) { }
    public void OnVisualUpdate() { }
    
    public void Dispose()
    {
        _proxy.OnHit -= OnHit;
    }

    private void OnHit(CollisionProxy proxy, Collision col)
    {
        _collisionResult.isCollision = true;
        if (col.gameObject.CompareTag("LeftWall") || col.gameObject.CompareTag("RightWall"))
        {
            _collisionResult.collisionType = CollisionType.Wall;
            
            //충돌 방향 정하는 로직
            Vector3 contactPoint = col.GetContact(0).point;
            Transform carTransform = proxy.transform.parent; 
        
            // 월드 좌표 -> 차체 기준 로컬 좌표 변환
            // 예: 차 중심이 (0,0,0)이고 앞범퍼 충돌이면 localPoint는 (0, 0, 2.5)가 됨
            Vector3 localPoint = carTransform.InverseTransformPoint(contactPoint);
        
            var absX = Mathf.Abs(localPoint.x);
            var absZ = Mathf.Abs(localPoint.z);

            // 앞/뒤 vs 좌/우 판별
            if (absZ > absX) 
            {
                // Z축(앞뒤) 거리가 X축(앞뒤) 거리보다 멂 -> Front or Rear
                _collisionResult.dir = localPoint.z > 0 ? ImpactDirection.Front : ImpactDirection.Rear;
            }
            else
            {
                // X축(좌우) 거리가 Z축(앞뒤) 거리보다 멂 -> Right or Left
                _collisionResult.dir = localPoint.x > 0 ? ImpactDirection.Right : ImpactDirection.Left;
            }
        
            //Debug.Log($"충돌 감지! 지점:{localPoint} / 방향:{_collisionResult.dir}");
        }
    }
    
    
}