using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.UIElements;

//튜닝 툴의 상태를 저장하는 메모장.
public class TuningContext : ISubController
{
    //현재 선택된 차량-> 시뮬 & 윈도우
    public ITunableVehicle TargetCar { get; private set; } // = simManager.Focused
    
    private Dictionary<Type, BaseModule> _comparisonMap;
    
    public Dictionary<Type, BaseModule> ComparisonMap => _comparisonMap;

    public IReadOnlyDictionary<Type, BaseModule> LiveModuleMap => TargetCar.ModuleMap;
    
    public event Action OnTargetCarChanged;
    public event Action OnComparisonChanged;
    
    public TuningContext(TuningRepository tuningRepository)
    {
        _comparisonMap = new Dictionary<Type, BaseModule>();
        foreach (var moduleType in tuningRepository.GetAllModuleList())
        {
            var db = tuningRepository.GetMouModuleDatabase(moduleType);
            if (db != null && db.Count > 0)
            {
                var defaultSO = db.Datas[0];
                SetComparisonSO(moduleType, defaultSO);
            }
        }
        
    }
    
    
    public void SetTargetCar(ITunableVehicle vehicle) // GameObject나 Component를 받아서
    {
        if (TargetCar == vehicle) return;
        
        TargetCar = vehicle;
        OnTargetCarChanged?.Invoke();
    }
    
    public BaseModule GetLiveData(Type type)
    {
        if (TargetCar == null) return null;
        return TargetCar.ModuleMap.GetValueOrDefault(type);
    }
    
    

    public void SetComparisonSO(Type type, BaseModule module)
    {
        if (_comparisonMap.ContainsKey(type)) _comparisonMap[type] = module;
        else
        {
            //Debug.LogWarning($"Comparison SO {type} is not found. so Automatically Generated");
            _comparisonMap.Add(type, module);
        }
        OnComparisonChanged?.Invoke();//이 타입의 모듈만 새로 그려주세요.
        
    }

    public void Initialize(VisualElement root)
    {
        //throw new NotImplementedException();
    }
}
