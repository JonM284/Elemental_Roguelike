using System.Collections.Generic;
using Runtime.Abilities;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterAbilityManager: MonoBehaviour
    {

        #region Serialize Fields

        [SerializeField] private List<Ability> abilities = new List<Ability>();

        #endregion


    }
}