using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RewardProfileSearchPicker : ScriptableObject, ISearchWindowProvider
{
    private List<RewardProfile> _items;
    private Action<RewardProfile> _onSelect;
    private string _title;

    public void Init(string title, List<RewardProfile> items, Action<RewardProfile> onSelect)
    {
        _title = title;
        _items = items;
        _onSelect = onSelect;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent(_title), 0)
        };

        foreach (var item in _items)
        {
            string displayName = string.IsNullOrEmpty(item.name) ? item.GetType().Name : item.name;

            tree.Add(new SearchTreeEntry(new GUIContent(displayName))
            {
                level = 1,
                userData = item
            });
        }

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var selected = SearchTreeEntry.userData as RewardProfile;
        _onSelect?.Invoke(selected);
        return true;
    }
}
