using UnityEngine;

//차량의 기본 상태값 전반을 필드로 가지는 클래스
public class VehicleContext
{
    public float deltaTime;
    
    private float _steeringAngle=0;

    private float _throttle=0;

    private float _brake=0;
    

    public float SteeringAngle
    {
        get => _steeringAngle;
        set => _steeringAngle = Mathf.Clamp(value, -1f, 1f);
    }

    public float Throttle
    {
        get => _throttle;
        set => _throttle = Mathf.Clamp(value, 0f, 1f);
    }

    public float Brake
    {
        get => _brake;
        set => _brake = Mathf.Clamp(value, 0f, 1f);
    }

  

    public VehicleStateHub StateHub { get; private set; }//자기들 끼리 초기화
    public SensorHub SensorHub { get; set; }
    
    public IMetricHub MetricHub { get; private set; }
    
    public void Setup()
    {
        StateHub = new VehicleStateHub();
        SensorHub = new SensorHub();
        SensorHub.Setup();

        MetricHub = new MetricHub();
        var mh = MetricHub as MetricHub;
        mh.Setup(this);
        
        
    }

    public void Update()
    {
        deltaTime = Time.deltaTime;
        //Debug.Log("deltaTime : " + deltaTime);
    }

    public void ResetEpisode()
    {
        StateHub.ResetEpisode();
        SensorHub.ResetEpisode();
    }
 
    
}

