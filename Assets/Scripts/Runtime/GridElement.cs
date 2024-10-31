using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GarawellCase
{
    public class GridElement : MonoBehaviour
    {
        [SerializeField] private Color FillColor = new Color(0.9686f, 0.7254f, 0.2901f, 1f);
        [SerializeField] private Color HighlightColor = new Color(0.9686f, 0.7254f, 0.2901f, 0.5f);
        [SerializeField] private Color _emptyColor = new Color(0.1921f, 0.1568f, 0.4823f, 0.6509f);
        private static readonly Color ClearColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private ParticleSystem _popParticle;
        [SerializeField] private ParticleSystem _shatterParticle;
        
        public SpriteRenderer Sprite => _sprite;
        public ParticleSystem PopParticle => _popParticle;
        public ParticleSystem ShatterParticle => _shatterParticle;
        
        public GridElementType Type;
        private SpriteRenderer _sprite;
        private Transform _spriteTransform;
        private Vector3 _initialScale;

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _spriteTransform = _sprite.transform;
            _initialScale = _sprite.transform.localScale;
        }

        public bool IsFilled => State == GridElementState.Filled;
        private GridElementState _state = GridElementState.Empty;
        public GridElementState State
        {
            get => _state;
            set
            {
                _state = value;
                if(!_sprite)
                    _sprite = GetComponentInChildren<SpriteRenderer>();

                if (!_spriteTransform)
                    _spriteTransform = _sprite.transform;
                
                switch (_state)
                {
                    case GridElementState.Empty:
                        _sprite.color = _emptyColor;
                        _spriteTransform.localScale = _initialScale;
                        break;
                    case GridElementState.Highlight:
                        _sprite.color = HighlightColor;
                        if(Type is GridElementType.HorizontalLine or GridElementType.VerticalLine)
                            _spriteTransform.localScale = 
                                new Vector3(_initialScale.x*2f, _initialScale.y, _initialScale.z);
                        break;
                    case GridElementState.Filled:
                        _sprite.color = FillColor;
                        if(Type is GridElementType.HorizontalLine or GridElementType.VerticalLine)
                            _spriteTransform.localScale = 
                                new Vector3(_initialScale.x*2f, _initialScale.y, _initialScale.z);
                        break;
                }
            }
        }
    }
}
