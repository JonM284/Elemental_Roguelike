using Data.Elements;
using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Wall Creation")]
    public class WallCreationData: CreationData
    {
        #region Serialized Fields

        [Header("WALL CREATION DATA")] 
        [SerializeField] private Status.Status applicableStatus;

        [SerializeField] private int damage;

        [SerializeField] private bool isArmorPiercing;

        [SerializeField] private ElementTyping elementTyping;

        [Header("Ball Effect")] 
        [SerializeField] private int amountDecreaseShot;

        [Header("Projectile Effect")] 
        [SerializeField] private bool isStopProjectiles;

        #endregion

        #region WallCreation Getters

        public Status.Status GetStatus()
        {
            return applicableStatus;
        }

        public int GetDamage()
        {
            return damage;
        }

        public bool IsArmorPiercing()
        {
            return isArmorPiercing;
        }

        public ElementTyping GetElementType()
        {
            return elementTyping;
        }

        public int GetShotDecreaseAmount()
        {
            return amountDecreaseShot;
        }

        public bool IsStopProjectiles()
        {
            return isStopProjectiles;
        }
        
        #endregion
    }
}