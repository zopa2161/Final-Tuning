using UnityEngine;

public class AutoInputProvider : IInputProvider
{
    private float[] _inputs = new float[3];

    public float[] GetInput()
    {
        // 1. Steering: -1 ~ 1 사이로 왔다갔다 (Sin파)
        _inputs[0] = Mathf.Sin(Time.time * 2.0f); 

        // 2. Throttle: 0 ~ 1 사이로 밟았다 뗐다 (PingPong)
        _inputs[1] = Mathf.PingPong(Time.time * 0.5f, 1.0f);

        // 3. Brake: 안 밟음
        _inputs[2] = 0f;

        return _inputs;
    }
}