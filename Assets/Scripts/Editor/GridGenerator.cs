using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace GarawellCase
{
   public class GridGenerator : EditorWindow
   {
       public GameObject dotPrefab;
       public GameObject hLinePrefab;
       public GameObject vLinePrefab;
       public GameObject squarePrefab;
       public int horizontalSize;
       public int verticalSize;
   
       [MenuItem("Tools/Grid Generator")]
       public static void ShowWindow()
       {
           GetWindow<GridGenerator>("Grid Generator");
       }
   
       private void OnGUI()
       {
           GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
   
           dotPrefab = (GameObject)EditorGUILayout.ObjectField("Dot Prefab", dotPrefab, typeof(GameObject), false);
           hLinePrefab = (GameObject)EditorGUILayout.ObjectField("Horizontal Line Prefab", hLinePrefab, typeof(GameObject), false);
           vLinePrefab = (GameObject)EditorGUILayout.ObjectField("Vertical Line Prefab", vLinePrefab, typeof(GameObject), false);
           squarePrefab = (GameObject)EditorGUILayout.ObjectField("Square Prefab", squarePrefab, typeof(GameObject), false);
   
           horizontalSize = EditorGUILayout.IntField("Horizontal Size", horizontalSize);
           verticalSize = EditorGUILayout.IntField("Vertical Size", verticalSize);
   
           if (GUILayout.Button("Generate Grid"))
           {
               GenerateGrid();
           }
       }
   
       private void GenerateGrid()
       {
           GameObject gridObject = new GameObject("Grid");
   
           GameObject dotsGroup = new GameObject("Dots");
           dotsGroup.transform.parent = gridObject.transform;
   
           GameObject hLinesGroup = new GameObject("HLines");
           hLinesGroup.transform.parent = gridObject.transform;
   
           GameObject vLinesGroup = new GameObject("VLines");
           vLinesGroup.transform.parent = gridObject.transform;
           
           GameObject fillGroup = new GameObject("Fill");
           fillGroup.transform.parent = gridObject.transform;
   
           for (int x = 0; x <= horizontalSize; x++)
           {
               for (int y = 0; y <= verticalSize; y++)
               {
                   var dot = (GameObject)PrefabUtility.InstantiatePrefab(dotPrefab, dotsGroup.transform);
                   dot.transform.position = new Vector3(x, y, 0);
   
                   if (x < horizontalSize)
                   {
                       var hLine = (GameObject)PrefabUtility.InstantiatePrefab(hLinePrefab, hLinesGroup.transform);
                       hLine.transform.position = new Vector3(x + 0.5f, y, 0);
                   }
   
                   if (y < verticalSize)
                   {
                       var vLine = (GameObject)PrefabUtility.InstantiatePrefab(vLinePrefab, vLinesGroup.transform);
                       vLine.transform.position = new Vector3(x, y+0.5f, 0);
                   }

                   if (x < horizontalSize && y < verticalSize)
                   {
                       var square = (GameObject)PrefabUtility.InstantiatePrefab(squarePrefab, fillGroup.transform);
                       square.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                   }
               }
           }
           
           gridObject.layer = LayerMask.NameToLayer("Grid");
           var grid = gridObject.AddComponent<GridController>();
           grid.Width = horizontalSize;
           grid.Height = verticalSize;
           
           var collider = gridObject.AddComponent<BoxCollider2D>();
           collider.offset = new Vector2(horizontalSize / 2f, verticalSize / 2f);
           collider.size = new Vector2(horizontalSize + 0.75f, verticalSize + 0.75f);
       }
   }               
}