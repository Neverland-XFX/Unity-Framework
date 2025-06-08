using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using GameLogic.Binding;
using GameLogic.Binding.Services;
using GameLogic.Commands;
using GameLogic.Model;
using GameLogic.Prefs;
using GameLogic.Services;
using UnityFramework;

namespace GameLogic.ViewModel
{
    public class BattleViewModel : ViewModelBase
    {
        private SimpleCommand _homeCommand;
        private SimpleCommand _resetCommand;
        private BattleModel _battleModel;
        
        public ICommand HomeCommand => _homeCommand;
        public ICommand RestartCommand => _resetCommand;
        public string Score
        {
            get => _battleModel.Score;
            set
            {
                _battleModel.Score = value;
                RaisePropertyChanged(nameof(Score));
            }
        }

        public BattleViewModel() 
        {
            _homeCommand = new SimpleCommand(GoHome);
            _resetCommand = new SimpleCommand(UniTask.UnityAction(Reset));
            _battleModel = new BattleModel();
            _battleModel.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }


        private async UniTaskVoid Reset()
        {
            await GameModule.Scene.LoadSceneAsync("scene_battle");
    
            BattleSystem.Instance.DestroyRoom();
            BattleSystem.Instance.LoadRoom().Forget();
        }

        private void GoHome()
        {
            
        }
    }
}