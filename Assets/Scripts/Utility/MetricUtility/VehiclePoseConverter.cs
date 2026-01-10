using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehiclePoseConverter : JsonConverter<VehiclePose>
{

    public override void WriteJson(JsonWriter writer, VehiclePose value, JsonSerializer serializer)
    {
        var root = new JObject();
        
        root["time"] = value.time;

        var positionArray = new JArray();
        positionArray.Add(value.position.x);
        positionArray.Add(value.position.y);
        positionArray.Add(value.position.z);
        
        root["position"] = positionArray;
        
        var rotationArray = new JArray();
        rotationArray.Add(value.rotation.x);
        rotationArray.Add(value.rotation.y);
        rotationArray.Add(value.rotation.z);
        rotationArray.Add(value.rotation.w);
        
        root["rotation"] = rotationArray;
        
        root.WriteTo(writer);
    }
    
    public override VehiclePose ReadJson(JsonReader reader, Type objectType, VehiclePose existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 1. 현재 reader 위치의 JSON을 JObject로 로드
        JObject root = JObject.Load(reader);

        // 2. 값 추출 및 초기화
        VehiclePose pose = new VehiclePose();

        // Time 복구
        if (root["time"] != null)
        {
            pose.time = (float)root["time"];
        }

        // Position 복구 (JArray -> Vector3)
        if (root["position"] is JArray positionArray && positionArray.Count >= 3)
        {
            pose.position = new Vector3(
                (float)positionArray[0],
                (float)positionArray[1],
                (float)positionArray[2]
            );
        }

        // Rotation 복구 (JArray -> Quaternion)
        if (root["rotation"] is JArray rotationArray && rotationArray.Count >= 4)
        {
            pose.rotation = new Quaternion(
                (float)rotationArray[0],
                (float)rotationArray[1],
                (float)rotationArray[2],
                (float)rotationArray[3]
            );
        }

        return pose;
    }
}
