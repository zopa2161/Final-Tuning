using System;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField]
    private RewardProfile _profile;
    private RewardInputData _inputData;
    private int _currentChecker;
    
    public RewardProfile GetRewardProfile() => _profile;
    
    public void Setup(VehicleContext vehicleContext)
    {
        _currentChecker = 0;
        _inputData = new RewardInputData(vehicleContext);
    }
    
    public RewardResult Step()
    {
        _inputData.CurrentCheckerNumber = _currentChecker;
        var result = _profile.RewardStep(_inputData);
        _currentChecker = _inputData.CurrentCheckerNumber;
        _inputData.Reset();
        return result;
    }
    //초기에 한번 실행된다
    public void SetRewardProfile(RewardProfile profile)
    {
        _profile = profile;
    }
    
}
