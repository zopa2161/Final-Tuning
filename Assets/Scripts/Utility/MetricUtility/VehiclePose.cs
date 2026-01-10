using Newtonsoft.Json;
using UnityEngine;


[System.Serializable]
[JsonConverter(typeof(VehiclePoseConverter))]
public struct VehiclePose
{
    public float time;      // 기록된 시간 (검색용)
    public Vector3 position;
    public Quaternion rotation;

    public VehiclePose(float t, Vector3 pos, Quaternion rot)
    {
        time = t;
        position = pos;
        rotation = rot;
    }
}