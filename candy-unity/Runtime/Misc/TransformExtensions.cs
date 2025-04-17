using System.Collections.Generic;
using UnityEngine;

namespace Candy.Unity
{
    public static class TransformExtensions
    {
        public static void ClearChilds(this Transform transform)
        {
            List<GameObject> objectsToDestroy = new List<GameObject>();
            foreach (Transform child in transform)
            {
                objectsToDestroy.Add(child.gameObject);
            }
            foreach (GameObject obj in objectsToDestroy)
            {
                Object.Destroy(obj);
            }
        }

        public static void ClearChildsImmediate(this Transform transform)
        {
            List<GameObject> objectsToDestroy = new List<GameObject>();
            foreach (Transform child in transform)
            {
                objectsToDestroy.Add(child.gameObject);
            }
            foreach (GameObject obj in objectsToDestroy)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
