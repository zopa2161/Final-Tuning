
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrivetrainState : BaseModuleState
{
        // 각 바퀴로 최종 전달될 토크 (Actuator가 가져갈 값)
        public float torqueFL;
        public float torqueFR;
        public float torqueRL;
        public float torqueRR;
        
        
        //public Dictionary<string, float> Torques= new Dictionary<string,float>();
        public float[] Torques = new float[4];


        public override void OnPush()
        {
                throw new NotImplementedException();
        }

        public override void ResetEpisode()
        {
                torqueFL = 0f;
                torqueFR = 0f;
                torqueRL = 0f;
                torqueRR = 0f;
                
                Torques[0] = 0f;
                Torques[1] = 0f;
                Torques[2] = 0f;
                Torques[3] = 0f;
        }
}