using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="DeceasedData")]
public class DeceasedData : ScriptableObject
{
    // Some causes inspired by https://www.wattpad.com/5323846-50-funny-ways-to-die
    public string[] deathCauseList;
    public string[] nameList;
    public string[] surnameList;
}
