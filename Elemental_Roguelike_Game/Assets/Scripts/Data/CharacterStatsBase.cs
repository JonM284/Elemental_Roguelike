using UnityEngine;


namespace Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Custom Data/CharacterData")]
    public class CharacterStatsBase : ScriptableObject
    {

        #region Public Fields

        public int baseHealth = 10;

        public int baseShields = 10;

        public float baseSpeed = 1;
    
        //TBD
        public int baseDamage = 1;

        public Color characterColor = Color.white;
    
        
        #endregion


    }   
}
