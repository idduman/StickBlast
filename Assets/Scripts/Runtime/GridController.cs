using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace GarawellCase
{
    public class GridController : MonoBehaviour
    {
        public static event Action MoveCompleted;
        
        public int Width;
        public int Height;
        
        [SerializeField] private float _breakInterval = 0.1f;
        [SerializeField] private float _breakDuration = 0.25f;
        [SerializeField] private float _breakDelay = 0.5f;
        [SerializeField] private Color _breakColor = Color.red;
        [SerializeField] private int pointsToWin = 200;
        [SerializeField] private int starsForMedal = 3;
        
        public int PointsToWin => pointsToWin;
        public int StarsForMedal => starsForMedal;

        private BoxCollider2D _collider;
        
        private Transform _dotsParent;
        private Transform _hLinesParent;
        private Transform _vLinesParent;
        private Transform _fillParent;

        private GridElement[] _hLines;
        private GridElement[] _vLines;
        private GridElement[] _dots;
        private GridElement[] _fills;

        private List<GridElement> _highlightedLines = new();
        private bool _validMove;
        
        private Sequence _breakSequence;

        private int _lastAddedSegmentCount;

        private static readonly float SnapTreshold = 0.38f;
        
        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            
            _dotsParent = transform.Find("Dots");
            _dots = _dotsParent.GetComponentsInChildren<GridElement>();
            _hLinesParent = transform.Find("HLines");
            _hLines = _hLinesParent.GetComponentsInChildren<GridElement>();
            _vLinesParent = transform.Find("VLines");
            _vLines = _vLinesParent.GetComponentsInChildren<GridElement>();
            _fillParent = transform.Find("Fill");
            _fills = _fillParent.GetComponentsInChildren<GridElement>();
        }

        public void ResetGrid()
        {
            foreach (var dot in _dots)
                dot.State = GridElementState.Empty;
            
            foreach (var line in _hLines)
                line.State = GridElementState.Empty;
            
            foreach (var line in _vLines)
                line.State = GridElementState.Empty;
            
            foreach (var fill in _fills)
                fill.State = GridElementState.Empty;
        }

        public bool CheckForSpace(InventoryItem item)
        {
            for (float y = -0.5f; y <= Height + 0.5f; y += 0.5f)
            {
                for (float x = -0.5f; x <= Width + 0.5f; x += 0.5f)
                {
                    var globalPos = transform.TransformPoint(new Vector3(x, y, 0f));
                    if (ValidateMove(item, globalPos))
                        return true;
                }
            }
            return false;
        }

        private bool ValidateMove(InventoryItem item, Vector3 position, bool debug = false)
        {
            var offset = position - item.transform.position;
            foreach (var segment in item.Segments)
            {
                var center = segment.bounds.center + offset;
                if(center.x < _collider.bounds.min.x 
                   || center.x > _collider.bounds.max.x
                   || center.y < _collider.bounds.min.y
                   || center.y > _collider.bounds.max.y)
                {
                    _highlightedLines.Clear();
                    return false;
                }
                
                var localPos = transform.InverseTransformPoint(center);
                var segmentSize = segment.bounds.size;
                var isHorizontal = segmentSize.x > segmentSize.y;
                
                var x = isHorizontal ?
                    Math.Clamp(Mathf.FloorToInt(localPos.x), 0,  Width - 1)
                    : Math.Clamp(Mathf.RoundToInt(localPos.x), 0, Width);
                var y = isHorizontal ?
                    Math.Clamp(Mathf.RoundToInt(localPos.y), 0, Height)
                    : Math.Clamp(Mathf.FloorToInt(localPos.y), 0, Height - 1);
                
                var index = isHorizontal ? (x*(Height+1)) + y : (x*Height) + y;
                
                if (isHorizontal && !_hLines[index].IsFilled && Mathf.Abs(localPos.x - _hLines[index].transform.localPosition.x) < SnapTreshold)
                {
                    _highlightedLines.Add(_hLines[index]);
                }
                else if(!isHorizontal && !_vLines[index].IsFilled && Mathf.Abs(localPos.y - _vLines[index].transform.localPosition.y) < SnapTreshold)
                {
                    _highlightedLines.Add(_vLines[index]);
                }
                else
                {
                    _highlightedLines.Clear();
                    return false;
                }
            }
            return true;
        }

        public void ResetHighlight()
        {
            foreach (var line in _highlightedLines)
            {
                line.State = GridElementState.Empty;
            }
            _highlightedLines.Clear();
        }

        public void HighlightShape(InventoryItem item)
        {
            ResetHighlight();

            _validMove = ValidateMove(item, item.transform.position);

            if (_validMove)
            {
                foreach (var line in _highlightedLines)
                    line.State = GridElementState.Highlight;
            }
            else
            {
                _highlightedLines.Clear();
            }
        }
        
        public bool AddShape(InventoryItem item)
        {
            if (!_validMove)
            {
                foreach (var line in _highlightedLines)
                {
                    line.State = GridElementState.Empty;
                }
                _lastAddedSegmentCount = 0;
                _highlightedLines.Clear();
                AudioController.Instance.PlayFx(AudioFxType.Wrong);
                return false;
            }

            foreach (var line in _highlightedLines)
            {
                line.State = GridElementState.Filled;
            }
            _lastAddedSegmentCount = _highlightedLines.Count;
            _highlightedLines.Clear();
            AudioController.Instance.PlayFx(AudioFxType.Click);
            GameManager.Instance.AddLevel();
            UpdateGrid();
            return true;
        }
        
        private bool UpdateCell(int x, int y, out bool isCompleted)
        {
            bool isFilled = false;
            isCompleted = false;
            
            for (int _x = x; _x <= x + 1; _x++)
            {
                for (int _y = y; _y <= y + 1; _y++)
                {
                    var prevXFilled = _x > 0 && _hLines[(_x - 1) * (Height+1) + _y].IsFilled;
                    var xFilled = _x < Width && _hLines[_x * (Height+1) + _y].IsFilled;
                    
                    var prevYFilled = _y > 0 && _vLines[_x * Height + _y - 1].IsFilled;
                    var yFilled = _y < Height && _vLines[_x * Height + _y].IsFilled;
                    
                    _dots[_x*(Height+1) + _y].State = prevXFilled || xFilled || prevYFilled || yFilled
                        ? GridElementState.Filled : GridElementState.Empty;

                    if (_x == x && _y == y)
                    {
                        var nextXFilled = _hLines[_x * (Height + 1) + _y + 1].IsFilled;
                        var nextYFilled = _vLines[(_x + 1) * Height + _y].IsFilled;
                        
                        var isComplete = xFilled && nextXFilled && yFilled && nextYFilled;
                        var fill = _fills[_x * Height + _y];
                        var prevState = fill.IsFilled;
                        _fills[_x*Height + _y].State = isComplete ? GridElementState.Filled : GridElementState.Empty;
                        var currentState = fill.IsFilled;
                        isFilled = isFilled || currentState;

                        isCompleted = (!prevState && currentState);
                    }
                }
            }

            return isFilled;
        }

        private void UpdateGrid(bool updateOnly = false)
        {
            bool[] RowComplete = new bool[Height];
            for (int a = 0; a < RowComplete.Length; a++)
            {
                RowComplete[a] = true;
            }
            
            bool[] ColumnComplete = new bool[Width];
            for (int b = 0; b < ColumnComplete.Length; b++)
            {
                ColumnComplete[b] = true;
            }
            
            List<Vector3> filledPositions = new List<Vector3>();
            Vector2Int _lastFilled = new Vector2Int(-1, -1);
            int completedCount = 0;
            for (int y = 0; y < Height; y++)
            {
                var rowComplete = true;
                for (int x = 0; x < Width; x++)
                {
                    var isFilled = UpdateCell(x, y, out var isCompleted);
                    if (isCompleted)
                    {
                        _lastFilled = new Vector2Int(x, y);
                        filledPositions.Add(_fills[x * Height + y].transform.position);
                        completedCount++;
                    }

                    
                    rowComplete = rowComplete && isFilled;
                    ColumnComplete[x] = ColumnComplete[x] && isFilled;
                }

                if (y < Height)
                    RowComplete[y] = RowComplete[y] && rowComplete;
            }

            if (updateOnly)
                return;

            var comboCount = GameManager.Instance.ComboCount;
            var newComboCount = completedCount > 0 ? comboCount + completedCount : 0;
            GameManager.Instance.ComboCount = newComboCount;
            GameManager.Instance.AddScore(_lastAddedSegmentCount + GameManager.Instance.ComboScore*newComboCount);
            GameManager.Instance.NoticeFilledPositions(filledPositions);

            HashSet<Vector2Int> fillCoords = new();
            HashSet<Vector2Int> extraCoords = new();
            
            for (int j = 0; j < RowComplete.Length; j++)
            {
                if(RowComplete[j])
                    for (int n = 0; n < Width; n++)
                    {
                        var c = new Vector2Int() { x = n, y = j };
                        fillCoords.Add(c);

                        if (j > 0 && !RowComplete[j-1] && _fills[n * Height + j - 1].IsFilled)
                        {
                            extraCoords.Add(new Vector2Int(n, j-1));
                        }

                        if (j < Height - 1 && !RowComplete[j+1] && _fills[n * Height + j + 1].IsFilled)
                        {
                            extraCoords.Add(new Vector2Int(n, j+1));
                        }
                    }
            }

            for (int i = 0; i < ColumnComplete.Length; i++)
            {
                if(ColumnComplete[i])
                    for (int m = 0; m < Height; m++)
                    {
                        var c = new Vector2Int() { x = i, y = m };
                        fillCoords.Add(c);
                        if (i > 0 && _fills[(i - 1) * Height + m].IsFilled
                            && !ColumnComplete[i - 1])
                        {
                            extraCoords.Add(new Vector2Int(i-1, m));
                        }
                        if (i < Width - 1 && _fills[(i + 1) * Height + m].IsFilled
                            && !ColumnComplete[i+1])
                        {
                            extraCoords.Add(new Vector2Int(i+1, m));
                        }
                    }
            }

            if (fillCoords.Count == 0)
            {
                StartCoroutine(MoveCompletedRoutine(completedCount > 0 ? 0.5f : 0.1f));
                return;
            }


            _breakSequence = DOTween.Sequence()
                .OnStart(() =>
                {
                    GameManager.Instance.AddScore(fillCoords.Count * 10);
                    GameManager.Instance.ShakeCamera();
                    AudioController.Instance.PlayFx(AudioFxType.Completion);
                    InputController.Instance.enabled = false;
                });

            
            //Adding fills to be destroyed
            foreach (var c in fillCoords.Distinct())
            {
                var delay = (Math.Abs(c.x - _lastFilled.x) + Math.Abs(c.y - _lastFilled.y)) * _breakInterval;
                
                var fill = _fills[c.x * Height + c.y];
                var fillSprite = fill.Sprite;
                var fillTransform = fillSprite.transform;
                var originalColor = fillSprite.color;

                _breakSequence.Insert(delay,
                    fillSprite.DOColor(_breakColor, _breakDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => fillSprite.color = originalColor));
                _breakSequence.Insert(delay, 
                    fillTransform.DOScale(Vector3.zero, _breakDuration)
                        .OnStart(() =>
                        {
                            fill.PopParticle.Play();
                            
                            AudioController.Instance.PlayFx(AudioFxType.Pop);
                            
                            _hLines[c.x*(Height+1) + c.y].State = GridElementState.Empty;
                            _hLines[c.x*(Height+1) + c.y+1].State = GridElementState.Empty;
                            
                            _vLines[c.x*Height + c.y].State = GridElementState.Empty;
                            _vLines[(c.x+1)*Height + c.y].State = GridElementState.Empty;
                        })
                        .OnComplete(() =>
                        {
                            
                            fill.State = GridElementState.Empty;
                            fillTransform.localScale = Vector3.one;
                            UpdateCell(c.x, c.y, out var isCompleted);
                            GameManager.Instance.PlayStarAnimation(fillTransform.position);
                        })
                    );
            }
            //Adding extra neighboring fills to be destroyed
            foreach (var e in extraCoords.Except(fillCoords).Distinct())
            {
                var extraDelay = (Math.Abs(e.x - _lastFilled.x) + Math.Abs(e.y - _lastFilled.y)) * _breakInterval;
                
                var extraFill = _fills[e.x * Height + e.y];
                var extraFillSprite = extraFill.Sprite;
                var originalColor = extraFillSprite.color;
                
                _breakSequence.Insert(extraDelay,
                    extraFillSprite.DOColor(_breakColor, 0.1f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            extraFillSprite.color = originalColor;
                            extraFill.State = GridElementState.Empty;
                            extraFill.ShatterParticle.Play();
                            UpdateCell(e.x, e.y, out var isCompleted);
                        })

                );
            }
            _breakSequence.OnComplete(() =>
            {
                UpdateGrid(true);
                MoveCompleted?.Invoke();
                InputController.Instance.enabled = true;
            });
            _breakSequence.PrependInterval(_breakDelay);
            _breakSequence.Play();
        }
        
        private IEnumerator MoveCompletedRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            MoveCompleted?.Invoke();
        }
    }
}