using Data;
using Data.Elements;
using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Melee Turret")]
    public class MeleeTurretCreationData: CreationData
    {
        #region Serialized Fields

        [Header("Base Info")]
        [Tooltip("Melee Damage")]
        [SerializeField] private int meleeDamage;

        [SerializeField] private ElementTyping elementTyping;

        [SerializeField] private bool dealsKnockback;

        #endregion
        
        #region Proximity Perameter Getters

        public int GetDamageAmount()
        {
            return meleeDamage;
        }

        public ElementTyping GetElement()
        {
            return elementTyping;
        }

        public bool GetDoesKnockback()
        {
            return dealsKnockback;
        }

        #endregion
    }
}