using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represent the deceased and its grave.
/// </summary>
public class Deceased
{
    # region Attributes
    private string name;
    private string surname;
    private int age;
    private string deathCause;

    /// <summary>
    /// The chances this person has to be visited each months. (in unit %)
    /// </summary>
    private float visitChance;
    public float VisitChance{ get{ return visitChance; } }
    /// <summary>
    /// Use to indicate the state of the grave. (in unit %)
    /// </summary>
    private float graveStateRatio = 1f;
    public enum GraveState { Abandoned, Correct, WellMaintain }

    /// <summary>
    /// True: visited that month // False: not visited that month
    /// </summary>
    private List<bool> lastVisits;

    /// <summary>
    /// How hold this grave is. (in months)
    /// </summary>
    private int graveAge = 0;
    public int GraveAge { get { return graveAge; } }
    public void IncrementGraveAge()
    {
        graveAge++;
    }
    /// <summary>
    /// The time spend in the morgue. (in months)
    /// </summary>
    private int morgueTime = 0;
    public int MorgueTime{ get { return morgueTime; } }
    public void StayInMorgue()
    {
        morgueTime++;
    }
    #endregion

    #region Constructor
    public Deceased()
    {
        DeceasedData deceasedData = GameManager.instance.deceasedData;
        int rdmNameId = Random.Range(0, deceasedData.nameList.Length);
        int rdmSurnameId = Random.Range(0, deceasedData.surnameList.Length);
        int rdmDeathCauseId = Random.Range(0, deceasedData.deathCauseList.Length);
        this.name = deceasedData.nameList[rdmNameId];
        this.surname = deceasedData.surnameList[rdmSurnameId];
        this.deathCause = deceasedData.deathCauseList[rdmDeathCauseId];

        this.age = Random.Range(0, 100);
        this.visitChance = Random.Range(0f, 1f);
        this.lastVisits = new List<bool>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// The informations display in the morgue.
    /// </summary>
    /// <returns></returns>
    public string Profile()
    {
        return string.Format("<b>{0} {1}, {2}</b>\n<i>{3}</i>", this.name, this.surname, this.age, this.deathCause);
    }

    /// <summary>
    /// The informations display when the mouse if over the grave.
    /// </summary>
    /// <returns></returns>
    public string GraveInfo()
    {
        return string.Format("<b>{0} {1}, {2}</b>\n<i>Buried {3} months ago.</i>", this.name, this.surname, this.age, this.graveAge);
    }

    /// <summary>
    /// When this grave is visited.
    /// </summary>
    /// <param name="isVisited">True if is visited, False otherwise.</param>
    public void Visit(bool isVisited)
    {
        // If we get to the max registered visits, forget the first one.
        if (lastVisits.Count >= GameManager.instance.param.maxRegisteredVisits)
        { 
            lastVisits.RemoveAt(0);
        }
        lastVisits.Add(isVisited);
    }

    /// <summary>
    /// Compute the grave state.
    /// Based on the last visits.
    /// </summary>
    /// <returns></returns>
    public GraveState GetGraveState()
    {
        int n = GameManager.instance.param.maxRegisteredVisits;
        // Sum of the first nth digit
        int sumnth = 0;
        for (int i = 1; i <= n; i++)
        {
            sumnth += i;
        }

        int visitSum = 0;
        // In case the grave is younger than a year we simulate the virtual past months as if the grave was visited.
        int simulatedMonth = n - lastVisits.Count;
        // Sum of the nth first digit, but only count the visited months.
        for (int i = 1; i <= n; i++)
        {
            if (i <= simulatedMonth)
            {
                visitSum += i;
            }
            else if (lastVisits[i - (simulatedMonth + 1)])
            {
                visitSum += i;
            }
        }
    
        graveStateRatio = (float)visitSum / (float)sumnth;

        if (graveStateRatio >= GameManager.instance.param.wellMaintainState)
        {
            return GraveState.WellMaintain;
        }
        else if (graveStateRatio <= GameManager.instance.param.abandonedState)
        {
            return GraveState.Abandoned;
        }
        else
        {
            return GraveState.Correct;
        }
    }

    /// <summary>
    /// Compute the malus when you unbury a grave.
    /// </summary>
    /// <returns></returns>
    public int UnburyMalus()
    {
        return -1 * Mathf.RoundToInt(GameManager.instance.param.unburyMalus * GameManager.SCurve(this.graveStateRatio));
    }
    #endregion

}


