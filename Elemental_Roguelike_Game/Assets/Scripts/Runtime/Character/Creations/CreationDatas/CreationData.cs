using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Character.Creations.CreationDatas
{

    public abstract class CreationData: ScriptableObject
    {
        #region Serialized Fields

        [Header("Base Data")]
        [Tooltip("Creations are damageable, please select health.")]
        [SerializeField] private int health;
        
        [Tooltip("Can this creation be destroyed?")]
        [SerializeField] private bool isIndestructible;

        [Tooltip("Range of detection and usage of creation")]
        [SerializeField] private float radius;
        
        [Header("Hidden Proximity")]
        [Tooltip("Is this creation hidden from the other team until walking into discovery range?")]
        [SerializeField] private bool isHidden;

        [Header("Round Timer")]
        [Tooltip("How many rounds does this stay after being created?")]
        [SerializeField] private int roundStayAmount;
        [Tooltip("Does it just stay infinitely?")]
        [SerializeField] private bool isInfiniteTime;

        #endregion

        #region Public Fields

        public AssetReference creationRef;

        #endregion

        #region Class Implementation

        public int GetHealth()
        {
            return health;
        }

        public bool GetIsHidden()
        {
            return isHidden;
        }

        public float GetRadius()
        {
            return radius;
        }

        public int GetRoundStayAmountMax()
        {
            return roundStayAmount;
        }

        public bool GetIsInfinite()
        {
            return isInfiniteTime;
        }

        public bool GetIsIndestructible()
        {
            return isIndestructible;
        }

        public LayerMask GetHiddenLayer()
        {
            return LayerMask.NameToLayer("HIDDEN");
        }

        #endregion


    }
}