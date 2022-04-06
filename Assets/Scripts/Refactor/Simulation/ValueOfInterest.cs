using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static JSONManager;

public class ValueOfInterest
{
    private string name;
    private Fact relevantFact;
    private string relevantValue; //MMTURI

    public ValueOfInterest(string name)
    {
        this.name = name;
    }

    public string getName()
    {
        return name;
    }

    public void setRelevantFactAndValue(Fact relevantFact, string relevantValue)
    {
        this.relevantFact = relevantFact;
        this.relevantValue = relevantValue;
    }

    public Fact getRelevantFact()
    {
        return this.relevantFact;
    }

    public string getRelevantValue()
    {
        return this.relevantValue;
    }

}
