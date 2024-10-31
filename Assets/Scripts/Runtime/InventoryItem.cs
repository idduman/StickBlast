using UnityEngine;

namespace GarawellCase
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private int _complexity = 1;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _flip;
        [SerializeField] private Transform _shape;
        [SerializeField] private bool _symmetric;

        public int ID { get; private set; }

        public bool IsSymmetric => _symmetric;
        private float _bottomOffset;
        public float BottomOffset => _bottomOffset;
        public Transform Pivot => _pivot;
        public Transform Flip => _flip;
        public int Complexity => _complexity;
        public SpriteRenderer[] Segments {get; private set;}
        private void Start()
        {
            Segments = _shape.GetComponentsInChildren<SpriteRenderer>();
            _bottomOffset = 0f;
            foreach (var s in Segments)
            {
                var localPoint = transform.InverseTransformPoint(s.bounds.min);
                if(localPoint.y < _bottomOffset)
                    _bottomOffset = localPoint.y;
            }
            
        }
        
        public void SetId(int id)
        {
            ID = id;
        }
    }
}

