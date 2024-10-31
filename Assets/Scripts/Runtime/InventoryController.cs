using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GarawellCase
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private float _pickupOffset = 1f;
        [SerializeField] private float _pickupMinDelta = 1f;
        [SerializeField] private float _inventoryScale = 0.85f;
        [SerializeField] private List<InventoryItem> _itemPrefabs;
        [SerializeField] private List<Transform> _itemSlots;
        [SerializeField] private bool _debugMode;
        [SerializeField] private Transform _itemPoolParent;
        
        private List<InventoryItem> _itemPool;
        private List<InventoryItem> _items;
        private List<InventoryItem> _availableItems;

        private Camera _mainCamera;
        private InventoryItem _selectedItem;
        private Vector3 _initialPos;
        private Vector3 _offset;
        
        private LayerMask _inventoryMask;
        private LayerMask _gridMask;

        private bool _checkedForSpace;
        private bool _draggable;
        private bool _moved;
        private bool _droppable;
        private bool _finished;
        
        private GridPanel _gridPanel;
        
        private Coroutine _availabilityRoutine;

        #region Unity Methods
        public void Initialize()
        {
            _gridPanel = FindFirstObjectByType<GridPanel>();
            _items = new List<InventoryItem>();
            _mainCamera = Camera.main;
            _inventoryMask = LayerMask.GetMask("InventoryItem");
            _gridMask = LayerMask.GetMask("Grid");
            _availableItems = new List<InventoryItem>();
            _checkedForSpace = false;
            _draggable = true;
            _droppable = false;
            _moved = false;
            _finished = false;

            if (_itemPool == null)
            {
                int id = 0;
                _itemPool = new List<InventoryItem>();
                for (int i = 0; i < _itemPrefabs.Count; i++)
                {
                    for (int r = 0; r < 4; r++)
                    {
                        var itemToSpawn = _itemPrefabs[i];
                        if (r > 1 && itemToSpawn.IsSymmetric)
                            break;
                        
                        var item = Instantiate(itemToSpawn, _itemPoolParent);
                        item.SetId(id++);
                        item.gameObject.name = $"{itemToSpawn.name}_r{r}_c({item.Complexity})";
                        item.transform.localPosition = new Vector3(3f * r, -3f * i, 0f);
                        item.Flip.rotation = Quaternion.Euler(0f, 0f, r*90f);
                        _itemPool.Add(item);
                    }
                }
            }
            
            InputController.Instance.Pressed += OnPressed;
            InputController.Instance.Dragged += OnDragged;
            InputController.Instance.Released += OnReleased;

            GridController.MoveCompleted += UpdateItems;
        }
        
        private void OnDestroy()
        {
            if (!InputController.Instance)
                return;
            
            InputController.Instance.Pressed -= OnPressed;
            InputController.Instance.Dragged -= OnDragged;
            InputController.Instance.Released -= OnReleased;
            
            GridController.MoveCompleted -= UpdateItems;
        }
        #endregion
        
        #region Input Listeners
        private void OnPressed(Vector2 position)
        {
            if (!_draggable)
                return;

            _moved = false;
            _droppable = false;
            Debug.Log("Droppable = false");
            
            var ray = _mainCamera.ScreenPointToRay(position);
            var hit = Physics2D.GetRayIntersection(ray, 12f, _inventoryMask);
            //Debug.Log("Pressed: " + hit);
            if (hit)
            {
                var item = hit.collider.GetComponentInChildren<InventoryItem>();
                if (!item)
                    return;
                
                _checkedForSpace = false;
                _selectedItem = item;
                var itemTransform = _selectedItem.transform;
                var itemPos = itemTransform.position;
                
                _initialPos = itemPos;

                _offset = itemPos - (Vector3)hit.point;
                AudioController.Instance.PlayFx(AudioFxType.ItemPickup);

                _selectedItem.Pivot.DOKill();
                _selectedItem.Pivot.DOScale(1.05f * _gridPanel.ActiveGrid.transform.localScale, 0.05f);
                _selectedItem.Pivot.DOLocalMove((_pickupOffset - 2f*item.BottomOffset) * Vector3.up, 0.05f)
                    .OnComplete(() =>
                    {
                        _droppable = true;
                        GameManager.Instance.Vibrate(VibrationType.Light);
                    });
            }
        }
        
        private void OnDragged(Vector2 position)
        {
            if (!_selectedItem)
                return;

            _moved = true;
            var worldPos = _mainCamera.ScreenToWorldPoint(position);
            worldPos.z = transform.position.z;
            var itemPos = worldPos + _offset;
            _selectedItem.transform.position = itemPos;
            var rayPos = _selectedItem.Pivot.position;
            rayPos.z = -1f;

            var ray = new Ray(rayPos, Vector3.forward);
            var hit = Physics2D.GetRayIntersection(ray, 12f, _gridMask);
            if(_droppable && hit)
            {
                if (!_checkedForSpace)
                {
                    var itemToCheck = _itemPool.First(i => i.ID == _selectedItem.ID);
                    if (!_gridPanel.ActiveGrid.CheckForSpace(itemToCheck))
                    {
                        /*_selectedItem.Pivot.DOKill();
                        _selectedItem.Pivot.DOLocalMove(Vector3.zero, 0.1f);
                        _selectedItem.Pivot.DOScale(_inventoryScale, 0.1f);
                        _selectedItem.transform.DOMove(_initialPos, 0.1f);
                        _selectedItem = null;
                        GameManager.Instance.ResetGridHighlights();*/
                        AudioController.Instance.PlayFx(AudioFxType.Wrong);
                        if (_items.Count <= 1)
                        {
                            GameManager.Instance.FinishGame(false);
                        }
                        _checkedForSpace = true;
                        return;
                    }
                    _checkedForSpace = true;
                }
                _gridPanel.ActiveGrid.HighlightShape(_selectedItem);
            }
            else
            {
                _gridPanel.ActiveGrid.ResetHighlight();
            }
        }
        
        private void OnReleased(Vector2 position)
        {
            if (!_selectedItem)
                return;
            
            _draggable = false;
            //Debug.Log("Draggable = false");
            
            var itemTransform = _selectedItem.transform;
            var dist = Vector3.Distance(_initialPos, itemTransform.position);
            
            var rayPos = _selectedItem.Pivot.position;
            rayPos.z = -1f;

            var ray = new Ray(rayPos, Vector3.forward);
            var hit = Physics2D.GetRayIntersection(ray, 12f, _gridMask);
            
            if(_droppable && _moved && dist > 0.1f && hit && _gridPanel.ActiveGrid.AddShape(_selectedItem))
            {
                _items.Remove(_selectedItem);
                Destroy(_selectedItem.gameObject);
            }
            else
            {
                _selectedItem.Pivot.DOKill();
                _selectedItem.Pivot.DOLocalMove(Vector3.zero, 0.05f);
                _selectedItem.Pivot.DOScale(_inventoryScale, 0.05f);
                itemTransform.DOKill();
                itemTransform.DOMove(_initialPos, 0.05f)
                    .OnComplete(UpdateItems);
                GameManager.Instance.ResetGridHighlights();
            }
            
            _droppable = false;
            _checkedForSpace = false;
            _selectedItem = null;
        }

        #endregion

        private void UpdateItems()
        {
            if (_finished)
                return;
            
            _draggable = true;
            if (_debugMode)
            {
                ResetInventory();
                return;
            }

            if (_items.Count < 1)
            {
                /*if(_availabilityRoutine != null)
                    StopCoroutine(_availabilityRoutine);
                _availabilityRoutine = StartCoroutine(CheckAvailabilityAndGenerate());*/
                CheckAvailabilityAndGenerate();
            }
            else
            {
                var availableAny = false;
                foreach(var item in _items)
                {
                    var itemToCheck = _itemPool.First(i => i.ID == item.ID);
                    availableAny |= _gridPanel.ActiveGrid.CheckForSpace(itemToCheck);
                }

                if (!availableAny)
                {
                    _selectedItem = null;
                    Debug.Log("No Space Available");
                    StartCoroutine(FinishGameRoutine(false));
                    return;
                }
            }

        }

        private void GenerateItems()
        {
            if (_debugMode)
            {
                for (int i = 0; i < _itemSlots.Count; i++)
                {
                    var item = Instantiate(_itemPrefabs[i], _itemSlots[i]);
                    _items.Add(item);
                }

                return;
            }

            var maxComplexity = GameManager.Instance.CurrentLevel switch
            {
                < 10 => 2,
                < 15 => 3,
                < 25 => 4,
                < 50 => 5,
                _ => 6
            };

            var prefabs = new List<InventoryItem>(_itemPool);
            for(int i = _items.Count; i < _itemSlots.Count; i++)
            {
                var complexity = Random.Range(0f, 1f) switch
                {
                    < 0.325f => 1,
                    < 0.56f => 2,
                    < 0.75f => 3,
                    < 0.875f => 4,
                    < 0.95f => 5,
                    _ => 6
                };
                //complexity = Math.Clamp(complexity, 1, maxComplexity);
                
                var orderedByClosestComplexity = prefabs.OrderBy(it => Math.Abs(it.Complexity - complexity)).ToList();
                var closestComplexity = orderedByClosestComplexity[0].Complexity;
                var items = orderedByClosestComplexity.Where(it => it.Complexity == closestComplexity).ToList();
                var slot = _itemSlots[i];
                var index = Random.Range(0, items.Count);
                if (index >= items.Count)
                    Debug.Log("Index was bigger");
                var itemToSpawn = index < items.Count ? items[index] : prefabs[index];
                var item = Instantiate(itemToSpawn, slot.position, Quaternion.identity, slot);
                item.SetId(itemToSpawn.ID);
                
                item.Pivot.localScale = _inventoryScale * Vector3.one;
                
                _items.Add(item);
                if (prefabs.Count > 2)
                    prefabs.Remove(itemToSpawn);
            }
        }
        
        private void GenerateAvailableItems()
        {
            var maxComplexity = 6;
            var lastComplexity = 0;
            for(int i = 0; i < _itemSlots.Count; i++)
            {
                var complexity = Random.Range(0f, 1f) switch
                {
                    < 0.325f => 1,
                    < 0.56f => 2,
                    < 0.75f => 3,
                    < 0.875f => 4,
                    < 0.96f => 5,
                    _ => 6
                };

                if (lastComplexity > 3 && complexity >= lastComplexity)
                {
                    complexity = Random.Range(1, lastComplexity);
                }
                if(complexity > lastComplexity)
                    lastComplexity = complexity;

                var orderedByClosestComplexity = _availableItems.OrderBy(it => Math.Abs(it.Complexity - complexity)).ToList();
                var closestComplexity = orderedByClosestComplexity[0].Complexity;
                var items = orderedByClosestComplexity.Where(it => it.Complexity == closestComplexity).ToList();

                if (items.Count == 0)
                {
                    Debug.Log($"No items found with complexity: {closestComplexity}");
                    GenerateItems();
                    return;
                }
                
                var slot = _itemSlots[i];
                var index = Random.Range(0, items.Count);
                var itemToSpawn = items[index];
                var item = Instantiate(itemToSpawn, slot.position, Quaternion.identity, slot);
                item.SetId(itemToSpawn.ID);
                item.Pivot.localScale = _inventoryScale * Vector3.one;
                
                _items.Add(item);
                
                if (_availableItems.Count > 2)
                {
                    _availableItems.Remove(itemToSpawn);
                }
            }
        }

        public void ResetInventory()
        {
            foreach (var item in _itemPool)
            {
                item.Pivot.localScale = _gridPanel.ActiveGrid.transform.localScale * 1.05f;
            }
            
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                _items.RemoveAt(i);
                Destroy(item.gameObject);
            }
            _items.Clear();
            GenerateItems();
            _draggable = true;
            _finished = false;
        }

        private void CheckAvailabilityAndGenerate()
        {
            _availableItems.Clear();
            foreach (var item in _itemPool)
            {
                if(_gridPanel.ActiveGrid.CheckForSpace(item))
                    _availableItems.Add(item);
            }
            GenerateAvailableItems();
        }

        private IEnumerator FinishGameRoutine(bool success)
        {
            _finished = true;
            yield return new WaitForSeconds(0.3f);
            GameManager.Instance.FinishGame(success);
        }
    }
}

