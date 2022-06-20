using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static JSONManager;

/**
 * 
 * 
 */
public class ValueOfInterest
{
    private string name;
    private Fact relevantFact;    //Fact representing the Objec to which this value of interest applies

    public ValueOfInterest(string name)
    {
        this.name = name;
    }

    public string getName()
    {
        return name;
    }

    public void setRelevantFact(Fact relevantFact)
    {
        this.relevantFact = relevantFact;
    }

    public Fact getRelevantFact()
    {
        return this.relevantFact;
    }

}
