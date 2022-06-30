using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterStatsBase : ScriptableObject
{

    #region Public Fields

    public int baseHealth = 10;

    public int baseShields;

    public float baseSpeed = 1;
    
    //TBD
    public int baseDamage = 1;

    public Color characterColor = Color.white;
    
    

    #endregion


}
