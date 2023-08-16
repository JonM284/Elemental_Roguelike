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

        [Header("Round Timer")]
        [Tooltip("How many rounds does this stay after being created?")]
        [SerializeField] private int roundStayAmount;
        [Tooltip("Does it just stay infinitely?")]
        [SerializeField] private bool isInfiniteTime;

        [Header("Layers")]
        [Tooltip("This will determine what to perform the action to")]
        [SerializeField] private LayerMask checkLayers;
        
        #endregion

        #region Public Fields

        public AssetReference creationRef;

        #endregion

        #region Class Implementation

        public int GetHealth()
        {
            return health;
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

        public LayerMask GetLayers()
        {
            return checkLayers;
        }

        #endregion


    }
}