using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static JSONManager;
using JsonSubTypes;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;

/*
 * SimplifiedFact: Class used for deserialization of ServerResponses
 */
[JsonConverter(typeof(JsonSubtypes), "kind")]
[JsonSubtypes.KnownSubType(typeof(SSymbolFact), "general")]
[JsonSubtypes.KnownSubType(typeof(SValueFact), "veq")]
[JsonSubtypes.KnownSubType(typeof(SEqsysFact), "eqsys")]
public abstract class SimplifiedFact
{
    public string kind;
    public UriReference @ref;
    public string label;

    public static List<SimplifiedFact> FromJSON(string json)
    {
        List<SimplifiedFact> sFacts = JsonConvert.DeserializeObject<List<SimplifiedFact>>(json);
        return sFacts;
    }
}

public class UriReference
{
    public string uri;

    public UriReference(string uri)
    {
        this.uri = uri;
    }
}

/**
    * Class used for deserializing incoming symbol-declarations from mmt
    */
public class SSymbolFact : SimplifiedFact
{
    public MMTTerm tp;
    public MMTTerm df;
}

/**
* Class used for deserializing incoming value-declarations from mmt
*/
public class SValueFact : SimplifiedFact
{
    public MMTTerm lhs;
    public MMTTerm valueTp;
    public MMTTerm value;
    public MMTTerm proof;
}

/**
* Class used for deserializing incoming eqsys-declarations from mmt
*/
public class SEqsysFact : SSymbolFact
{
    public List<List<MMTTerm>> equations;

    public Tuple<Matrix<double>, Vector<double>> parseEquationSystem() {
        if (this.equations == null || this.equations.Count == 0)
        {
            Debug.Log("SEqsysFact.ParseEquationSystem: equations null or empty.");
            return null;
        }
        else
        {
            bool eachEquationHasTwoElements = true;
            this.equations.ForEach(equation => eachEquationHasTwoElements &= (equation.Count == 2));
            if (!eachEquationHasTwoElements)
            {
                Debug.Log("SEqsysFact.ParseEquationSystem: Some equations are corrupt and don't have 2 elements.");
                return null;
            }
            else {
                foreach (List<MMTTerm> equation in this.equations) {
                    MMTTerm leftSide = equation.ElementAt(0);
                    MMTTerm rightSide = equation.ElementAt(1);

                    //TODO
                }
                //TODO
                return null;
            }
        }
    }
}
