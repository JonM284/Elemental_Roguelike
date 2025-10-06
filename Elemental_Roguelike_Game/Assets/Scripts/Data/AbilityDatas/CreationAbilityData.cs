using UnityEngine;

namespace Data.AbilityDatas
{
    [CreateAssetMenu(menuName = "Maulball/Ability/Creation Ability Data")]
    public class CreationAbilityData: AbilityData
    {

        [Header("Creation Specific")]
        //ToDo: change to addressable system
        public GameObject creationPrefab;
        
        
        
        public float maxTurnsAlive;
        public bool isExplosion;
        public bool isSavePointedDirection;
        public bool isHitBall;
        public bool isPulsing;
        public int pulseAmount;
        
        [Header("Wall")]
        public bool isWall;
        public Vector3 wallCenter;
        public Vector3 wallHalfExtents;

    }
}