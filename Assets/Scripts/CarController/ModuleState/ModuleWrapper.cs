using UnityEngine;

public class ModuleWrapper
{
    private BaseModule _module;
    [SerializeReference] // 인스펙터에서 보려면 필요
    private BaseModuleState _state= null;

    public BaseModule Module => _module;
    public BaseModuleState State => _state;

    public void Initialize(BaseModule module)
    {
        _module = module;
        _state = _module.CreateState(); 
    }
    
    public void RunCalculation(VehicleContext context)
    {
        if (_module != null && _state != null)
        {
            _module.Calculate(context);
        }
    }

    public void Initialize(VehicleStateHub stateHub)
    {
        _module.InitializeState(_state,stateHub);
    }
    
}
