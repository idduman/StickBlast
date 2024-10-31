using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GarawellCase
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _scoreSlider;
        [SerializeField] private TMP_Text _comboNumberText;
        [SerializeField] private Animator _comboAnimator;
        [SerializeField] private ParticleSystem _comboParticle;
        [SerializeField] private TMP_Text _popupNumberText;
        [SerializeField] private Animator _popupNumberAnimator;
        [SerializeField] private GameObject _finishGamePanel;
        [SerializeField] private Animator _noSpaceAnimator;
        [SerializeField] private TMP_Text _finishGameScoreText;
        [SerializeField] private Image _medalImage;
        [SerializeField] private Animator _finalScoresAnimator;
        [SerializeField] private GameObject _noSpacePanel;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _playAgainButton;
        [SerializeField] private GameObject _nextLevelButton;
        [SerializeField] private GameObject _bestScoreObject;

        [SerializeField] private Sprite _grayMedalSprite;
        [SerializeField] private Sprite _bronzeMedalSprite;
        [SerializeField] private Sprite _goldMedalSprite;

        [SerializeField] private TMP_Text _starText;
        [SerializeField] private Transform _starTransform;
        [SerializeField] private Transform _starPrefab;

        [SerializeField] private Animator _optionsPanelAnimator;
        [SerializeField] private Image _soundImage;
        [SerializeField] private Sprite _soundOnSprite;
        [SerializeField] private Sprite _soundOffSprite;
        [SerializeField] private GameObject _quitPanel;

        [SerializeField] private Transform[] _finishStarTransforms;

        private int _score;
        private int _maxScore;
        private float _currentScore;
        private Vector3 _playAgainButtonPos;
        
        private Sequence _starSequence;

        private static readonly int _animationPerStar = 5;

        private void Awake()
        {
            _playAgainButtonPos = _playAgainButton.transform.localPosition;
        }

        private void Update()
        {
            if(_currentScore < _score)
            {
                _currentScore += Mathf.Max(Mathf.Pow(1.8f,(_score - _currentScore)/5f), 20f) * Time.deltaTime;
                _currentScore = Mathf.Clamp(_currentScore, 0f, _score);
                _scoreText.SetText($"{Mathf.RoundToInt(_currentScore)} / {_maxScore}");
                _scoreSlider.value = Mathf.Clamp(_currentScore / _maxScore, 0f, 1f);
            }
            else
            {
                _currentScore = _score;
                _scoreText.SetText($"{_score} / {_maxScore}");
            }
        }
        
        public void Initialize()
        {
            _quitPanel.SetActive(false);
            _optionsPanelAnimator.gameObject.SetActive(false);
            _finishGamePanel.SetActive(false);
            _finalScoresAnimator.gameObject.SetActive(false);
            _playAgainButton.gameObject.SetActive(false);
            _medalImage.gameObject.SetActive(false);
            _bestScoreObject.gameObject.SetActive(false);
            _nextLevelButton.gameObject.SetActive(false);
            _noSpacePanel.gameObject.SetActive(false);
            _winPanel.gameObject.SetActive(false);

            foreach (var star in _finishStarTransforms)
            {
                star.gameObject.SetActive(false);
            }

            _playAgainButton.transform.localPosition = _playAgainButtonPos;
        }
        
        public void SetScore(int score)
        {
            _score = score;
        }

        public void SetScoreText(int score, int maxScore)
        {
            _score = score;
            _maxScore = maxScore;
            _currentScore = _score;
            _scoreText.SetText($"{_score} / {_maxScore}");
            _scoreSlider.value = Mathf.Clamp(_currentScore / _maxScore, 0f, 1f);
        }

        public void DisplayCombo(int value)
        {
            _comboNumberText.SetText(value.ToString());
            _comboAnimator.Play("Combo");
            _comboParticle.Play();
        }

        public void DisplayScorePopup(int value, Vector3 position)
        {
            _popupNumberAnimator.transform.position = position;
            _popupNumberText.SetText($"+{value}");
            _popupNumberAnimator.Play("ScorePopup");
        }
        
        public void SetStarText(int starAmount)
        {
            _starText.SetText(starAmount.ToString());
        }
        
        public void SetLevelText(int level)
        {
            _levelText.SetText($"Level {level}");
        }

        public void PlayStarAnimation(Vector3 position)
        {
            var star = Instantiate(_starPrefab, position, Quaternion.identity, transform);
            star.DOMove(_starTransform.position, 0.75f)
                .OnComplete(() =>
                {
                    GameManager.Instance.AddStar(1);
                    PunchStar();
                    star.gameObject.SetActive(false);
                    Destroy(star.gameObject, 0.1f);
                });
        }

        private void PunchStar()
        {
            _starSequence.Kill();
            _starSequence = DOTween.Sequence()
                .Append(_starTransform.DOScale(1.25f * Vector3.one, 0.05f))
                .Append(_starTransform.DOScale(1f * Vector3.one, 0.05f))
                .OnKill(() => _starTransform.localScale = Vector3.one)
                .OnComplete(() => _starTransform.localScale = Vector3.one);
        }

        public void NextLevelButton()
        {
            Debug.Log("NextLevel");
            _finishGamePanel.SetActive(false);
            GameManager.Instance.NextLevel();
        }

        public void OpenOptionsPanel()
        {
            if(_optionsPanelAnimator.gameObject.activeSelf)
                CloseOptionsPanel();
            
            _soundImage.sprite = AudioController.Instance.Mute ? _soundOffSprite : _soundOnSprite;
            _optionsPanelAnimator.gameObject.SetActive(true);
            _optionsPanelAnimator.Play("Open");
        }
        public void CloseOptionsPanel()
        {
            _quitPanel.SetActive(false);
            _optionsPanelAnimator.Play("Close");
            StartCoroutine(CloseOptionsPanelRoutine());
        }

        public void ToggleSound()
        {
            AudioController.Instance.Mute = !AudioController.Instance.Mute;
            if(!AudioController.Instance.Mute)
                AudioController.Instance.PlayFx(AudioFxType.Click);
            _soundImage.sprite = AudioController.Instance.Mute ? _soundOffSprite : _soundOnSprite;
        }
        
        public void ReplayButton()
        {
            Debug.Log("Replay");
            GameManager.Instance.LoadGame();
        }
        
        public void QuitButton()
        {
            _quitPanel.SetActive(true);
        }

        public void QuitConfirm()
        {
            GameManager.Instance.QuitGame();
        }

        public void QuitCancel()
        {
            _quitPanel.SetActive(false);
        }
        
        public void ActivateFinishGamePanel(bool success, bool isBestScore, int starCount)
        {
            var medalType = starCount switch
            {
                3 => MedalType.Gold,
                2 => MedalType.Silver,
                1 => MedalType.Bronze,
                _ => MedalType.None
            };
            _medalImage.sprite = medalType switch
            {
                MedalType.Gold => _goldMedalSprite,
                MedalType.Silver => _grayMedalSprite,
                MedalType.Bronze => _bronzeMedalSprite,
                _ => null
            };
            
            _finishGamePanel.SetActive(true);
            
            if(!success)
                _noSpacePanel.SetActive(true);
            else
                _winPanel.SetActive(true);
            
            StartCoroutine(FinishGameRoutine(success, isBestScore, medalType, starCount));
        }

        private IEnumerator FinishGameRoutine(bool success, bool isBestScore, MedalType medalType, int starCount)
        {
            yield return new WaitForSeconds(1.8f);
            _finishGameScoreText.SetText(_score.ToString());
            _finalScoresAnimator.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            if (success)
            {
                var starUiPos = _starTransform.position;
                var starSequence = DOTween.Sequence();

                for (int i = 0; i < starCount; i++)
                {
                    var finishStarTransform = _finishStarTransforms[i];
                    var destination = finishStarTransform.position;
                    for (int s = 0; s < _animationPerStar; s++)
                    {
                        var star = Instantiate(_starPrefab, starUiPos, Quaternion.identity, transform);
                        starSequence.Insert(i * 1f + s * 0.1f,
                            star.DOMove(destination, 0.5f)
                                .OnComplete(() =>
                                {
                                    star.gameObject.SetActive(false);
                                    Destroy(star.gameObject,0.05f);
                                })
                        );
                    }
                
                    starSequence.InsertCallback(i * 1f + _animationPerStar * 0.1f,
                        () => finishStarTransform.gameObject.SetActive(true));
                }
                _starSequence.Play();
            
                while (_starSequence != null && _starSequence.IsPlaying())
                    yield return null;
                
                if (medalType != MedalType.None)
                {
                    yield return new WaitForSeconds(0.5f);
                    _medalImage.gameObject.SetActive(true);
                }
            }
            
            if (isBestScore)
            {
                yield return new WaitForSeconds(0.5f);
                _bestScoreObject.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(1.5f);
            
            if (!success)
            {
                var pos = _playAgainButton.transform.localPosition;
                _playAgainButton.transform.localPosition = new Vector3(0f, pos.y, 0f);
            }
            else
            {
                _nextLevelButton.gameObject.SetActive(true);
            }
            _playAgainButton.gameObject.SetActive(true);
        }
        
        private IEnumerator CloseOptionsPanelRoutine()
        {
            yield return new WaitForSeconds(0.51f);
            _optionsPanelAnimator.gameObject.SetActive(false);
        }
    }
}