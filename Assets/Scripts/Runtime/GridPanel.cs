using System;
using System.Collections.Generic;
using UnityEngine;

namespace GarawellCase
{
    public class GridPanel : MonoBehaviour
    {
        [SerializeField] private List<GridController> _gridList;
        public GridController ActiveGrid { get; private set; }

        private void Awake()
        {
            ActiveGrid = GetComponentInChildren<GridController>();
        }

        public int MaxDifficulty => _gridList.Count - 1;

        public void Initialize()
        {
            if(ActiveGrid)
                ActiveGrid.ResetGrid();
        }

        public void SetActiveGrid(int difficulty)
        {
            if(difficulty >= _gridList.Count)
                throw new ArgumentOutOfRangeException("Grid difficulty is out of range");
            
            var grid = _gridList[difficulty];
            if (ActiveGrid && ActiveGrid != grid)
            {
                ActiveGrid.gameObject.SetActive(false);

                ActiveGrid = grid;
                grid.gameObject.SetActive(true);
            }
        }
    }
}