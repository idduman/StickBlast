using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace GarawellCase
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoSingleton<AudioController>
    {
        [SerializeField] private AudioClip _itemPickupSound;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _fillSound;
        [SerializeField] private AudioClip _multiFillSound;
        [SerializeField] private AudioClip _completionSound;
        [SerializeField] private AudioClip _popSound;
        [SerializeField] private AudioClip _wrongSound;
        [SerializeField] private AudioClip _gameOverSound;
        [SerializeField] private AudioClip _gameWinSound;
        [SerializeField] private AudioSource _fillSource;

        private AudioSource _audioSource;
        private AudioSource _Source;

        private bool _mute;
        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                _audioSource.mute = value;
                _fillSource.mute = value;
            }
        }

        private static readonly float[] Pitches = {1f, 1.122f, 1.26f, 1.335f, 1.5f, 1.68f, 1.888f, 2f};

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            Mute = false;
        }

        public void PlayFx(AudioFxType type)
        {
            switch (type)
            {
                case AudioFxType.ItemPickup:
                    _audioSource.PlayOneShot(_itemPickupSound);
                    break;
                case AudioFxType.Click:
                    _audioSource.PlayOneShot(_clickSound);
                    break;
                case AudioFxType.Fill:
                    var pitch = Pitches[GameManager.Instance.ComboCount % 7];
                    int octave = Math.Clamp(GameManager.Instance.ComboCount / 7, 0, 2);
                    _fillSource.pitch = pitch * (float)Math.Pow(2, octave);
                    _fillSource.PlayOneShot(_fillSound);
                    break;
                case AudioFxType.MultiFill:
                    _audioSource.PlayOneShot(_multiFillSound);
                    break;
                case AudioFxType.Completion:
                    _audioSource.PlayOneShot(_completionSound);
                    break;
                case AudioFxType.Pop:
                    _audioSource.PlayOneShot(_popSound);
                    break;
                case AudioFxType.Wrong:
                    _audioSource.PlayOneShot(_wrongSound);
                    break;
                case AudioFxType.GameOver:
                    _audioSource.PlayOneShot(_gameOverSound);
                    break;
                case AudioFxType.Win:
                    _audioSource.PlayOneShot(_gameWinSound);
                    break;
            }
        }
    }
}

