using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace GarawellCase
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private int _comboScore = 10;

        public int ComboScore => _comboScore;
        public int CurrentLevel { get; private set; }
        public int CurrentScore { get; private set; }
        public int Difficulty { get; private set; }

        private int _currentStarAmount;
        public int CurrentStarAmount
        {
            get => _currentStarAmount;
            private set
            {
                _currentStarAmount = value;
                _uiController.SetStarText(_currentStarAmount);
            }
        }

        private int _comboCount;
        public int ComboCount
        {
            get => _comboCount;
            set
            {
                if(value > 1 && value > _comboCount)
                    _uiController.DisplayCombo(value);

                _comboCount = value;
            }
        }

        private Camera _camera;
        private GridPanel _gridPanel;
        private InventoryController _inventory;
        private UIController _uiController;
        private bool _finished;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            _camera = Camera.main;
            _gridPanel = FindFirstObjectByType<GridPanel>();
            _inventory = FindFirstObjectByType<InventoryController>();
            _uiController = FindFirstObjectByType<UIController>();
            
            Difficulty = PlayerPrefs.GetInt("Difficulty", 0);
            Difficulty = Math.Min(Difficulty, _gridPanel.MaxDifficulty);
            
            _gridPanel.Initialize();
            _gridPanel.SetActiveGrid(Difficulty);
            _inventory.Initialize();
            
            Vibration.Init();
            
            LoadGame();
        }

        /*#if UNITY_EDITOR
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F))
                FinishGame();
s
            if(Input.GetKeyDown(KeyCode.R))
                LoadGame();
        }
        #endif*/

        public void LoadGame()
        {
            CurrentScore = 0;
            CurrentStarAmount = 0;
            CurrentLevel = 0;
            _comboCount = 0;

            _finished = false;
            
            _gridPanel.ActiveGrid.ResetGrid();
            _gridPanel.SetActiveGrid(Difficulty);
            
            _uiController.Initialize();
            _uiController.SetScoreText(CurrentScore, _gridPanel.ActiveGrid.PointsToWin);
            _uiController.SetLevelText(Difficulty + 1);
            
            _inventory.ResetInventory();
            InputController.Instance.enabled = true;
        }

        public void NextLevel()
        {

            Difficulty = Math.Min(Difficulty + 1, _gridPanel.MaxDifficulty);
            PlayerPrefs.SetInt("Difficulty", Difficulty);

            
            LoadGame();
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }

        public void AddScore(int score)
        {
            CurrentScore += score;
            _uiController.SetScore(CurrentScore);
            if (!_finished && CurrentScore >= _gridPanel.ActiveGrid.PointsToWin)
            {
                FinishGame(true);
            }
        }

        public void AddLevel()
        {
            if (_finished)
                return;

            CurrentLevel++;
        }

        public void NoticeFilledPositions(List<Vector3> filledPositions)
        {
            if (filledPositions.Count == 0)
                return;

            
            AudioController.Instance.PlayFx(AudioFxType.Fill);
            if(filledPositions.Count > 1)
                AudioController.Instance.PlayFx(AudioFxType.MultiFill);
            
            var meanPos = new Vector3(filledPositions.Sum(f => f.x) / filledPositions.Count,
                filledPositions.Sum(f => f.y) / filledPositions.Count, 0);
            
            _uiController.DisplayScorePopup(_comboScore * ComboCount,
                _camera.WorldToScreenPoint(meanPos));
        }
        
        public void ResetGridHighlights()
        {
            _gridPanel.ActiveGrid.ResetHighlight();
        }

        public void ShakeCamera()
        {
            _camera.transform.DOShakePosition(0.5f, 0.5f, 50);
            Vibrate(VibrationType.Strong);
        }

        public void FinishGame(bool success)
        {
            if (_finished)
                return;
            
            _finished = true;
            
            InputController.Instance.enabled = false;
            
            var bestScore = PlayerPrefs.GetInt("BestScore", 0);
            var isBestScore = bestScore < CurrentScore;
            
            if (isBestScore)
                PlayerPrefs.SetInt("BestScore", CurrentScore);
            
            StartCoroutine(FinishGameRoutine(success, isBestScore));
        }

        public void PlayStarAnimation(Vector3 worldPosition)
        {
            var screenPosition = _camera.WorldToScreenPoint(worldPosition);
            _uiController.PlayStarAnimation(screenPosition);
        }

        public void Vibrate(VibrationType vibrationType)
        {
            switch (vibrationType)
            {
                case VibrationType.Light:
                    Vibration.VibratePop();
                    break;
                case VibrationType.Strong:
                    Vibration.Vibrate();
                    break;
                case VibrationType.Pulse:
                    Vibration.VibrateNope();
                    break;  
            }
        }

        public void AddStar(int amount)
        {
            CurrentStarAmount += amount;
        }
        
        private IEnumerator FinishGameRoutine(bool success, bool isBestScore)
        {
            yield return new WaitForSeconds(0.75f);
            AudioController.Instance.PlayFx(success ? AudioFxType.Win :AudioFxType.GameOver);
            var starCount = Math.Clamp(CurrentStarAmount / _gridPanel.ActiveGrid.StarsForMedal, 0, 3);
            _uiController.ActivateFinishGamePanel(success, isBestScore, starCount);
        }
    }
}

