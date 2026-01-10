using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] 
    private Environment[] _environments;

    private RewardProfile _originalRewardProfile;
    
    public RewardProfile OriginalRewardProfile => _originalRewardProfile;
    
    private void Awake()
    {
        // 3. [Awake 순서 제어]
        // 유니티의 랜덤한 Awake 실행을 막고, 매니저가 직접 순차적으로 초기화합니다.
        foreach (var env in _environments)
        {
            if (env != null)
            {
                env.Setup(); // 각 환경(및 차량) 초기화
            }
        }
        if (_environments.Length > 0 && _environments[0] != null)
        {
            // Environment -> System -> Agent -> Profile 경로로 접근
            // (구조에 맞게 경로 수정 필요할 수 있음)
            _originalRewardProfile = Instantiate(_environments[0].CarSystem.AgentManager.GetRewardProfile());
        }

        foreach (var env in _environments)
        {
            _environments[0].CarSystem.AgentManager.SetRewardProfile(_originalRewardProfile);
        }
    }
    
}