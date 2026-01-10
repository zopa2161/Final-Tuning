using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.RewardWindow
{
    public class RewardTuningRepository : ISubController
    {
        private RewardProfileDatabase _rewardProfileDatabase;
        private ISubController _subControllerImplementation;
        public RewardProfileDatabase RewardProfileDatabase => _rewardProfileDatabase;
        
        public void Initialize(VisualElement root)
        {
            SetupDatabase();
        }
        
        private void SetupDatabase()
        {
            _rewardProfileDatabase = AssetDatabase.LoadAssetAtPath<RewardProfileDatabase>("Assets/Resources/Database/RewardProfileDatabase.asset");
            if (_rewardProfileDatabase == null)
            {
                _rewardProfileDatabase = ScriptableObject.CreateInstance<RewardProfileDatabase>();
                AssetDatabase.CreateAsset(_rewardProfileDatabase, "Assets/Resources/Database/RewardProfileDatabase.asset");
                AssetDatabase.SaveAssets();
            }
        }

        public void CreateProfile()
        {
            var newProfile = ScriptableObject.CreateInstance<RewardProfile>();
            RegisterProfile(newProfile, "new Profile");
        }

        public void SaveProfile(RewardProfile profile, string newName)
        {
            var newProfile = Object.Instantiate(profile);
            RegisterProfile(newProfile, newName);
        }

        private void RegisterProfile(RewardProfile profile, string name)
        {
            string path = $"Assets/Resources/RewardProfiles/{name}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            // 파일명 중복 처리 등으로 변경된 실제 이름을 프로필 Name에 반영
            profile.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            
            AssetDatabase.CreateAsset(profile, path);
            _rewardProfileDatabase.Add(profile);
   
            EditorUtility.SetDirty(_rewardProfileDatabase);
            AssetDatabase.SaveAssets();
        }

        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}