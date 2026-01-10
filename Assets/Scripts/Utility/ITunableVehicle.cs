     using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITunableVehicle
{
    VehicleProfile Profile { get; }
    VehicleContext Context { get; }
    GameObject CachedGameObject { get; }
    GameObject GhostVehicle { get; }
    IReadOnlyDictionary<Type, BaseModule> ModuleMap { get; }
    
}