using System;
using System.Collections.Generic;

public interface IMetricHub
{
    // 0. 기록 세션 등록
    void RegisterSession(MetricSession session);
    // 1. 등록 (공급자 연결)
    void RegisterSource(string id, string unit, Func<float> provider);

    // 2. 조회 (그래프 뷰용)
    float GetValue(string id);
    
    // 3. 전체 데이터 접근 (레코더용)
    IEnumerable<string> GetAllIds();
    
    
}