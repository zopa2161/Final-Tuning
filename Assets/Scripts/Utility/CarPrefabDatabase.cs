    using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CarPrefabDatabase", menuName = "ScriptableObjects/CarPrefabDatabase")]
public class CarPrefabDatabase : SerializedScriptableObject
{
    [SerializeField]
    private List<GameObject> _datas = new List<GameObject>();

    public List<GameObject> Datas
    {
        get => _datas;
        set => _datas = value;
    }

    [ShowInInspector, ReadOnly]
    private Dictionary<int, GameObject> carPrefabById = new Dictionary<int, GameObject>();

    private void OnValidate()
    {
        carPrefabById.Clear();
        foreach (var prefab in _datas)
        {
            if (prefab != null && prefab.TryGetComponent<ITunableVehicle>(out var tunable))
            {
                if (!carPrefabById.ContainsKey(tunable.Profile.ID))
                {
                    carPrefabById.Add(tunable.Profile.ID, prefab);
                }
            }
        }
    }

    public GameObject GetPrefabById(int id)
    {
        carPrefabById.TryGetValue(id, out var prefab);
        return prefab;
    }
}
