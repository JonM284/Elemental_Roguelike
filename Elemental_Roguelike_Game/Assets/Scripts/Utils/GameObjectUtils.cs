using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Project.Scripts.Utils
{
    public static class GameObjectUtils
    {

        public static GameObject Clone(this GameObject objectToClone, Transform parent)
        {
            var clonedObject = GameObject.Instantiate(objectToClone, parent);
            return clonedObject;
        }

    }
}