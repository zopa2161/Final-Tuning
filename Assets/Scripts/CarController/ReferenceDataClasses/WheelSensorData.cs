using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
//값만을 담은 데이터 클래스
public class WheelSensorData
{
    public float rpm;
    public float forwardSlip; // 오타 수정: forward -> forward
    public float sidewaysSlip; // 오타 수정: sideway -> sideways
    public bool isGrounded;
    public float compression;
}
[System.Serializable]
public class WheelSensor
{
    // 4바퀴 데이터 그릇 미리 생성
    public WheelSensorData fl = new WheelSensorData();
    public WheelSensorData fr = new WheelSensorData();
    public WheelSensorData rl = new WheelSensorData();
    public WheelSensorData rr = new WheelSensorData();


    public WheelSensorData[] SensorDatas = new WheelSensorData[4];

    public WheelSensor()
    {
        SensorDatas = new WheelSensorData[4] { fl, fr, rl, rr };
    }
    public WheelSensorData this[WheelLoc loc] => SensorDatas[(int)loc];

    public float GetAvgRPM()
    {
        float sum = 0f;
        int count = 0;

        foreach (var sensor in SensorDatas)
        {
            // 접지되지 않은 바퀴는 헛돌아서 RPM이 비정상적으로 높을 수 있음.
            // 필요하다면 isGrounded 체크를 추가해도 됨.
            
            // 후진 시 RPM은 음수이므로, 엔진 회전수 매칭을 위해 절대값 사용
            
            sum += Mathf.Abs(sensor.rpm);
            count++;
        }

        return count > 0 ? sum / count : 0f;
    }
   
    public void ResetEpisode()
    {
        foreach (var sensor in SensorDatas)
        {
            sensor.rpm = 0f;
        }
    }
}