using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Database/BaseModule Database", fileName = "BaseModuleDatabase")]
public partial class BaseModuleDatabase : SerializedScriptableObject
{
    [SerializeField]
    private List<BaseModule> datas = new List<BaseModule>();

    public IReadOnlyList<BaseModule> Datas => datas;
    public int Count => datas.Count;
    
    private void SetID(BaseModule target, int id)
    {
        
        var field = typeof(BaseModule).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, id);
       
#if UNITY_EDITOR
        EditorUtility.SetDirty(target);
#endif
    }
    
    private void ReorderDatas()
    {
        var field = typeof(BaseModule).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < datas.Count; i++)
        {
            field.SetValue(datas[i], i);
#if UNITY_EDITOR
            EditorUtility.SetDirty(datas[i]);
#endif
        }
    }
    
    public void Add(BaseModule newData)
    {
        datas.Add(newData);
        SetID(newData, datas.Count - 1);
    }

    public void Remove(BaseModule data)
    {
        datas.Remove(data);
        ReorderDatas();
    }
    
    public BaseModule Select(int id)
        => datas[id];
    
    public T Select<T>(int id) where T : BaseModule
        => Select(id) as T;

    public BaseModule Select(string ModuleName)
        => datas.Find(item => item.ModuleName == ModuleName);
    public T Select<T>(string ModuleName) where T : BaseModule
        => Select(ModuleName) as T;

    public BaseModule Select(Func<BaseModule, bool> predicate)
        => datas.FirstOrDefault(predicate);
    public T Select<T>(Func<T, bool> predicate) where T : BaseModule
        => datas.FirstOrDefault(x => predicate(x as T)) as T;

    public BaseModule[] Selects(Func<BaseModule, bool> predicate)
        => datas.Where(predicate).ToArray();
    public T[] Selects<T>(Func<T, bool> predicate) where T : BaseModule
        => datas.Where(x => predicate(x as T)).Cast<T>().ToArray();

    public T[] GetDatas<T>() => datas.Cast<T>().ToArray();
    
    public void SortByModuleName()
    {
        datas.Sort((x, y) => String.Compare(x.ModuleName, y.ModuleName, StringComparison.Ordinal));
        ReorderDatas();
    }
}

public partial class BaseModuleDatabase
{

    private readonly static Dictionary<Type, BaseModuleDatabase> databasesByType = new();

    public static BaseModuleDatabase GetDatabase(Type type)
    {

        if (!databasesByType.ContainsKey(type))
            databasesByType[type] = Resources.Load<BaseModuleDatabase>($"Database/{type.Name}Database");
        return databasesByType[type];
    }

    public static BaseModuleDatabase GetDatabase<T>() where T : BaseModule
        => GetDatabase(typeof(T));

    public static T StaticSelect<T>(int id) where T : BaseModule
        => GetDatabase<T>().Select<T>(id);
    public static T StaticSelect<T>(string codeName) where T : BaseModule
        => GetDatabase<T>().Select<T>(codeName);
    public static T StaticSelect<T>(Func<T, bool> predicate) where T : BaseModule
        => GetDatabase<T>().Select(predicate);

    public static BaseModule StaticSelect(Type type, Func<BaseModule, bool> predicate)
        => GetDatabase(type).Select(predicate);
    public static BaseModule StaticSelect(Type type, int id)
        => GetDatabase(type).Select(id);
    public static BaseModule StaticSelect(Type type, string codeName)
        => GetDatabase(type).Select(codeName);

    public static BaseModule[] StaticSelects(Type type, Func<BaseModule, bool> predicate)
        => GetDatabase(type).Selects(predicate);
}