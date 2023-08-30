using System;
using Runtime.Misc;
using UnityEngine;

public class TempItem : MonoBehaviour
{

    [SerializeField] private int someInt = 5;

    private float someFloat = 3;

    private bool someBool;
    
    
    public int GetSomeInt()
    {
        return someInt;
    }
    
    
}
