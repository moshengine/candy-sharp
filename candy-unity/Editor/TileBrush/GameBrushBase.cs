using UnityEditor;
using UnityEngine;

namespace Candy.Unity.Editor
{
    public abstract class GameBrushBase : GridBrushBase
    {
        public GameObject Prefab;

        protected Vector3 _anchor = new(0.5f, 0.5f, 0f);

        protected GameObject _currentInstance;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Debug.Log($"Painting {Prefab.name}");

            if (Prefab == null)
                return;

            var isOccupied = IsOccupied(gridLayout, brushTarget, position);
            if (isOccupied)
            {
                return;
            }

            _currentInstance = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
            if (_currentInstance != null)
            {
                Undo.RegisterCreatedObjectUndo(_currentInstance, $"Paint {GetBrushName()}");
                _currentInstance.transform.SetParent(brushTarget.transform);
                _currentInstance.transform.position = GetCellCenter(gridLayout, position);
            }
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            EraseObjectAtPosition(brushTarget, GetCellCenter(gridLayout, position));
        }

        public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, Vector3Int pivot)
        {
            // We don't need to implement Pick as we're using a fixed prefab
        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Debug.LogWarning($"Flood Fill is not supported for this type of brush");
        }

        public virtual bool IsOccupied(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var cellCenter = gridLayout.LocalToWorld(gridLayout.CellToLocalInterpolated(position + _anchor));
            var objectInCell = GetObjectInCell(brushTarget.transform, cellCenter);
            if (objectInCell != null)
            {
                return true;
            }
            return false;
        }

        protected abstract bool EraseObjectAtPosition(GameObject brushTarget, Vector3 cellCenter);
        protected abstract bool ShouldEraseObject(GameObject obj);
        protected abstract string GetBrushName();

        protected GameObject GetObjectInCell(Transform parent, Vector3 cellCenter)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (Vector3.Distance(child.position, cellCenter) < 0.1f)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        protected Vector3 GetCellCenter(GridLayout gridLayout, Vector3Int position)
        {
            return gridLayout.LocalToWorld(gridLayout.CellToLocalInterpolated(position + _anchor));
        }
    }
}
