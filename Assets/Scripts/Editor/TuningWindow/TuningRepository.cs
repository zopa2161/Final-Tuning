using Newtonsoft.Json;
using PlasticGui.WorkspaceWindow.Items;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor; // AssetDatabase 사용을 위해 필수
using UnityEngine;
using UnityEngine.UIElements;

public class TuningRepository : ISubController
{
    private Dictionary<Type, BaseModuleDatabase> _moduleDatabaseMap;
    private CarPrefabDatabase _carPrefabDatabase;
    public CarPrefabDatabase CarPrefabDatabase => _carPrefabDatabase;
    public TuningRepository(Type[] dataTypes)
    {
        _moduleDatabaseMap = new Dictionary<Type, BaseModuleDatabase>();
        SetupDatabases(dataTypes);
    }

    public void Initialize(VisualElement root)
    {
    }

    public event Action<string> OnFloatingWarning;

    private void SetupDatabases(Type[] dataTypes)
    {
        _moduleDatabaseMap.Clear();
        foreach (var type in dataTypes)
        {
            var database = AssetDatabase.LoadAssetAtPath<BaseModuleDatabase>($"Assets/Resources/Database/{type.Name}Database.asset");
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<BaseModuleDatabase>();
                AssetDatabase.CreateAsset(database, $"Assets/Resources/Database/{type.Name}Database.asset");
           
                AssetDatabase.CreateFolder("Assets/Resources", type.Name);
            }
            
            _moduleDatabaseMap[type] = database;
            
        }
        //차량 db 셋업
        _carPrefabDatabase = AssetDatabase.LoadAssetAtPath<CarPrefabDatabase>("Assets/Resources/Database/CarPrefabDatabase.asset");
        if (_carPrefabDatabase == null)
        {
            _carPrefabDatabase = ScriptableObject.CreateInstance<CarPrefabDatabase>();
            AssetDatabase.CreateAsset(_carPrefabDatabase, "Assets/Resources/Database/CarPrefabDatabase.asset");
        }
    }
    public BaseModuleDatabase GetMouModuleDatabase(Type type) => _moduleDatabaseMap[type];
    
    public List<Type> GetAllModuleList() => _moduleDatabaseMap.Keys.ToList();

    public void CreateNewModule(BaseModule baseModule, string name)
    {
        var type = baseModule.GetType();
        var database = _moduleDatabaseMap[type];
        var newData = ScriptableObject.CreateInstance(type) as BaseModule;
        
        newData.CopyFrom(baseModule);
        var nameField = typeof(BaseModule).GetField("moduleName", BindingFlags.NonPublic | BindingFlags.Instance);
        nameField.SetValue(newData, name);
        
        AssetDatabase.CreateAsset(newData, $"Assets/Resources/{type.Name}/{name}.asset");
        database.Add(newData);
        
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }
    public void SaveSession(string path, string session)
    {
        if (session == null) return;
        
        File.WriteAllText(path, session);
        
        Debug.Log($"[Repo] 세션 저장 완료: {path}");
    }

    public MetricSession LoadSession(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {path}");
            return null;
        }

        try
        {
            // 1. 텍스트 읽기
            string json = File.ReadAllText(path);

            // 2. DTO로 역직렬화 (저장할 때 썼던 그 클래스)
            var session= JsonConvert.DeserializeObject<MetricSession>(json);
            
            return session;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"세션 로드 실패: {e.Message}");
            return null;
        }
    }
}