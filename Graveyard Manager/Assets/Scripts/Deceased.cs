using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deceased
{
    private string name;
    private string surname;
    private int age;
    private string deathCause;
    // Chance to be visited next time
    [Range(0f, 1f)]
    private float visitChance;
    public float VisitChance
    {
        get
        {
            return visitChance;
        }
    }
    [Range(0f, 1f)]
    private float graveStateRatio = 1f;

    private List<bool> last12thVisits;
    private int graveAge = 0;
    public int GraveAge
    {
        get
        {
            return graveAge;
        }
    }
    private int monthsSpendMorgue = 0;
    public int MonthsSpendMorgue
    {
        get
        {
            return monthsSpendMorgue;
        }
    }

    public Deceased(string _name, string _surname, int _age, string _deathCause, float _visitChance)
    {
        this.name = _name;
        this.surname = _surname;
        this.age = _age;
        this.deathCause = _deathCause;
        this.visitChance = _visitChance;
        this.last12thVisits = new List<bool>();
    }

    public Deceased()
    {
        DeceasedData deceasedData = GameManager.instance.deceasedData;
        int rdmNameId = Random.Range(0, deceasedData.nameList.Length);
        int rdmSurnameId = Random.Range(0, deceasedData.surnameList.Length);
        int rdmDeathCauseId = Random.Range(0, deceasedData.deathCauseList.Length);

        this.name = deceasedData.nameList[rdmNameId];
        this.surname = deceasedData.surnameList[rdmSurnameId];
        this.age = Random.Range(0, 100);
        this.deathCause = deceasedData.deathCauseList[rdmDeathCauseId];
        this.visitChance = Random.Range(0f, 1f);
        this.last12thVisits = new List<bool>();
    }

    public string Profile()
    {
        return string.Format("<b>{0} {1}, {2}</b>\n<i>{3}</i>", this.name, this.surname, this.age, this.deathCause);
    }

    public string GraveInfo()
    {
        return string.Format("<b>{0} {1}, {2}</b>\n<i>Buried {3} months ago.</i>", this.name, this.surname, this.age, this.graveAge);
    }

    public void Visit(bool isVisited)
    {
        if (last12thVisits.Count >= 12)
        { 
            last12thVisits.RemoveAt(0);
        }
        last12thVisits.Add(isVisited);
    }

    public void Age()
    {
        graveAge++;
    }

    public void StayInMorgue()
    {
        monthsSpendMorgue++;
    }

    public enum GraveState { Abandoned, Correct, WellMaintain}
    public GraveState GetGraveState()
    {
        // Sum of the first 12th digit
        int sum12th = 0;
        for (int i = 1; i <= 12; i++)
        {
            sum12th += i;
        }

        int visitSum = 0;
        // In case the grave is younger than a year we simulate the future months as if the grave was visited
        int simulatedMonth = 12 - last12thVisits.Count;
        for (int i = 1; i <= 12; i++)
        {
            if (i <= simulatedMonth)
            {
                visitSum += i;
            }
            else if (last12thVisits[i - (simulatedMonth + 1)])
            {
                visitSum += i;
            }
        }

        
        graveStateRatio = (float)visitSum / (float)sum12th;

        if (graveStateRatio >= 0.8f)
        {
            return GraveState.WellMaintain;
        }
        else if (graveStateRatio <= 0.2f)
        {
            return GraveState.Abandoned;
        }
        else
        {
            return GraveState.Correct;
        }
    }

    public int UnburyMalus()
    {
        return -1 * Mathf.RoundToInt(20f * GameManager.SCurve(this.graveStateRatio));
    }

    
}


