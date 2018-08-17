using UnityEngine;

/// <summary>
/// Object containing the lists of names, surnames and death causes.
/// </summary>
[CreateAssetMenu(menuName ="GM/DeceasedData")]
public class DeceasedData : ScriptableObject
{
    public string[] deathCauseList;
    public string[] nameList;
    public string[] surnameList;
}
