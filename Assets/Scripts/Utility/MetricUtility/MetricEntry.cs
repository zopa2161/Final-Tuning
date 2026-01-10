using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class MetricEntry
{
    public string Name;

    public string Unit;

    public float minValue;
    public float maxValue;
    
    public List<float> Values;
    [JsonIgnore]
    private Func<float> OnRecord;

    public MetricEntry(string name, string unit,float min, float max, Func<float> onRecord)
    {
        Name = name;
        Unit = unit;
        minValue = min;
        maxValue = max;
        OnRecord = onRecord;
        Values = new List<float>();
    }
    
    public void Capture()
    {
        if (OnRecord != null)
        {
            float val = OnRecord.Invoke();
            Values.Add(val);
        }
    }
    public bool IsEmpty()
    {
        if (Values.Count < 1) return true;
        return false;
    }
}