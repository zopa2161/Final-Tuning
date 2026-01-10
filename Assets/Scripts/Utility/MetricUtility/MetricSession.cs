
//하나의 기록 세션
using Newtonsoft.Json;
using System.Collections.Generic;
[System.Serializable]
public class MetricSession
{       
        public string sessionName;
        public string StartTime;

        public VehicleProfile VehicleProfile;
        public string SimName;
        
        public MetricEntry TimeStampEntry;
        public List<MetricEntry> metricEntries;

        public List<VehiclePose> trackData;
        
        private float _currentTime;

        public MetricSession()
        {
                metricEntries = new List<MetricEntry>();
                trackData = new List<VehiclePose>();
                TimeStampEntry = new MetricEntry("TimpStamp", "s", 0, 0, () =>_currentTime );
        }

        public void CaptureAll(float currentTime)
        {       _currentTime = currentTime;
                // 1. 시간 기록 (X축)
                TimeStampEntry.Capture();

                // 2. 데이터 기록 (Y축들)
                foreach (var entry in metricEntries)
                {
                        entry.Capture();
                }
        }

    
        public MetricEntry MetricByName(string name)
        {
                foreach (var entry in metricEntries)
                {
                        if(name == entry.Name) return entry;
                }
                return null;
        }

        public bool IsEmpty()
        {
                return TimeStampEntry.IsEmpty();
        }
        
}