using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Playmaker Creation")]
    public class PasserCreationData: CreationData
    {
        
        #region Serialized Fields

        [Header("Base Info")]
        [SerializeField] private float passingForce;

        #endregion
        
        #region Proximity Perameter Getters

        public float GetPassForce()
        {
            return passingForce;
        }

        #endregion
        
        
    }
}