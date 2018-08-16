using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the game parameters.
/// </summary>
[CreateAssetMenu(menuName ="GM/Params")]
public class Params : ScriptableObject
{
    [Tooltip("The base value to calculate the malus when a grave is unburied.")]
    public float unburyMalus;
    [Tooltip("The base value to decide if a grave is abandoned.")]
    public float abandonedState;
    [Tooltip("The base value to decide if a grave is well maintain.")]
    public float wellMaintainState;
    [Tooltip("The maximum of visit registered in \"Deceased.lastVisits\".")]
    public int maxRegisteredVisits;
    [Tooltip("How much time for the unbury animation")]
    public float unburyAnimationPause;
}
