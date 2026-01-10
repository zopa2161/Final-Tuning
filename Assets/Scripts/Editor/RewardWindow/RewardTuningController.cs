
// 리워드 튜닝 윈도우의 메인 컨트롤러.

using System;
using System.Collections.Generic;
using Editor.RewardWindow;
using NUnit;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public enum RewardTuningMode
{
        Live,
        NonLive,
}
public class RewardTuningController
{
        private EnvironmentManager _envManager;
        
        //==Sub Controller===
        private RewardTuningRepository _repository;
        private RewardTuningInspector _inspector;
        
        private List<ISubController> _controllers = new List<ISubController>();
        
        //===Side Controller===
        private RewardProfileSearchPicker _rewardPicker;
        private SaveRewardProfilePopup _saveRewardProfilePopup;

        private RewardTuningMode _mode;
        private Button _saveButton;
        private Button _createButton;
        
        public void Initialize(VisualElement root)
        {
                _envManager = UnityEngine.Object.FindFirstObjectByType<EnvironmentManager>();
                if (_envManager == null) _mode = RewardTuningMode.NonLive;
                else _mode = RewardTuningMode.Live;
                
                _repository = new RewardTuningRepository();
                _repository.Initialize(root);
                _inspector = new RewardTuningInspector(_mode,(v) => 
                        {
                                if (_repository.RewardProfileDatabase != null)OpenModuleListPopup(v);
                        }, () => {
                                _repository.CreateProfile();//=> 빈 프로파일 생성/ 포커스 바꿔주기.
                                _inspector.SetTarget(_repository.RewardProfileDatabase.Datas[^1]);
                        }, 
                        (p,n) => {
                                _repository.SaveProfile(p,n);
                        });
                _inspector.Initialize(root);

                _inspector.SetTarget(_mode != RewardTuningMode.NonLive
                        ? _envManager.OriginalRewardProfile
                        : _repository.RewardProfileDatabase.Datas[0]);
                _controllers.Add(_inspector);

        }
        public void OpenModuleListPopup(Vector2 screenPos)
        {
                // 1. DB에서 리스트 가져오기
            

                if (_rewardPicker == null)
                {
                        _rewardPicker = ScriptableObject.CreateInstance<RewardProfileSearchPicker>();
                }

                // 2. 내용물만 갈아끼우기 (Re-Initialize)
                _rewardPicker.Init(
                        title: $"Select Profile",
                        items: _repository.RewardProfileDatabase.Datas,
                        onSelect: (selectedData) => 
                        {
                                _inspector.SetTarget(selectedData);
                        }
                );

                // 3. 띄우기
                SearchWindow.Open(new SearchWindowContext(screenPos), _rewardPicker);
        
        }
        
}