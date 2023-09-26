using System.Collections.Generic;

namespace Data.CharacterData
{
    [System.Serializable]
    public class CharacterStatsData
    {
        #region Public Fields

        public string id;

        public string givenName;
        
        //Speed character will normally move at
        public float baseSpeed;

        //Max health of character
        public int baseHealth;

        public int currentHealth;

        //Max shield of character
        public int baseShields;

        public int currentShield;
        
        public int shootingScore;

        public int agilityScore;
        
        public int damageScore;

        public int passingScore;

        //Distance from start position, character is able to move
        public float movementDistance;

        //Distance from the idle character where they are able to intercept passes
        public float passInterceptionDistance;

        //Distance from the idle character where they can melee enemies moving close-by
        public float passiveMeleeDistance;

        //Cosmetic items
        public int costumeIndex;

        public string meepleElementTypeRef;

        public List<string> abilityReferences;

        public List<string> heldItems;

        public string classReferenceType;


        #endregion

        #region Constructor

        /// <summary>
        /// Add Element type when creating this, otherwise they will have no element
        /// Add Abilities when creating this, otherwise they will have no abilities
        /// </summary>
        public CharacterStatsData()
        {
            this.baseSpeed = 10;
            this.baseHealth = 10;
            this.currentHealth = this.baseHealth;
            this.baseShields = 10;
            this.currentShield = this.baseShields;
            this.movementDistance = 5f;
            this.damageScore = 1;
            this.costumeIndex = 0;
            //Type must be added whenever creating a new meeple
            this.abilityReferences = new List<string>();
            //Abilities must also be added when creating a new meeple
        }

        #endregion
    }
}