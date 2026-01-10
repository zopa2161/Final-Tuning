using System;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private CarMainSystem _car;
    [SerializeField] private Track _track;
    public Transform spawn;

    public CarMainSystem CarSystem => _car;
    public Track Track => _track;
    
    public void Setup()
    {
        _car.Setup();
        _car.CarAgent.OnEpisodeReset += Spawn;
        _track.Setup();
        Spawn();
    }

    private void Spawn()
    {   
        
        _car.ReStart();
        _car.TeleportTo(spawn);
    }
}