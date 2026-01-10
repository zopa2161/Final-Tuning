using System.Collections.Generic;
using UnityEngine;

public class ReplaySystem
{
    // 특정 시간(time)에 차량이 어디에 있어야 하는지 계산해주는 함수
    public static void EvaluatePose(List<VehiclePose> track, float time, out Vector3 outPos, out Quaternion outRot)
    {
        // 데이터가 없으면 기본값
        if (track == null || track.Count == 0)
        {
            outPos = Vector3.zero;
            outRot = Quaternion.identity;
            return;
        }

        // 1. 범위 밖 처리 (시작 전이나 끝난 후)
        if (time <= track[0].time)
        {
            outPos = track[0].position;
            outRot = track[0].rotation;
            return;
        }
        if (time >= track[track.Count - 1].time)
        {
            var last = track[track.Count - 1];
            outPos = last.position;
            outRot = last.rotation;
            return;
        }

        // 2. 이진 탐색(Binary Search) 등으로 time 바로 앞뒤의 프레임 찾기
        // (최적화를 위해선 지난번 프레임 인덱스를 캐싱하는게 좋지만, 일단 간단하게 구현)
        int indexA = 0;
        for (int i = 0; i < track.Count - 1; i++)
        {
            if (time >= track[i].time && time < track[i+1].time)
            {
                indexA = i;
                break;
            }
        }
        int indexB = indexA + 1;

        VehiclePose frameA = track[indexA];
        VehiclePose frameB = track[indexB];

        // 3. 보간 비율(t) 계산 (0~1 사이)
        float range = frameB.time - frameA.time;
        float t = (time - frameA.time) / range;

        // 4. 선형 보간 (Lerp / Slerp)
        outPos = Vector3.Lerp(frameA.position, frameB.position, t);
        outRot = Quaternion.Slerp(frameA.rotation, frameB.rotation, t);
    }
}