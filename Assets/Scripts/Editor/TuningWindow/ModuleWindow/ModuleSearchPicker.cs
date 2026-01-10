using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView; // 필수 네임스페이스
using UnityEngine;

// SearchWindow는 반드시 ScriptableObject여야 합니다.
public class ModuleSearchPicker : ScriptableObject, ISearchWindowProvider
{
    private List<BaseModule> _items;
    private Action<BaseModule> _onSelect;
    private string _title;

    // 초기화 함수
    public void Init(string title, List<BaseModule> items, Action<BaseModule> onSelect)
    {
        _title = title;
        _items = items;
        _onSelect = onSelect;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            // 1. 최상위 헤더 (무조건 있어야 함)
            new SearchTreeGroupEntry(new GUIContent(_title), 0)
        };

        // 2. 데이터 리스트 추가
        foreach (var item in _items)
        {
            // 아이템 이름이 없으면 타입명이라도 표시
            string displayName = string.IsNullOrEmpty(item.name) ? item.GetType().Name : item.name;

            tree.Add(new SearchTreeEntry(new GUIContent(displayName))
            {
                level = 1, // 들여쓰기 (헤더의 자식임을 표시)
                userData = item // 선택 시 돌려받을 데이터
            });
        }

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var selected = SearchTreeEntry.userData as BaseModule;
        _onSelect?.Invoke(selected); // 선택된 데이터를 콜백으로 넘김
        return true;
    }
}
