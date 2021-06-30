using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static JSONManager;
using JsonSubTypes;
using Newtonsoft.Json;

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

    public Tuple<List<List<double>>, List<double>, List<MMTTerm>> parseEquationSystem() {
        if (this.equations == null || this.equations.Count == 0)
        {
            Debug.Log("SEqsysFact.ParseEquationSystem: equations null or empty.");
            return null;
        }
        else
        {
            bool equationsHaveTwoElements = equations.Aggregate(true, (total, next) => total &= (next.Count == 2));
            bool equationsAreNormalized = equations.Aggregate(true, (total, next) => total &= (mmtTermEqualsValue(next.ElementAt(0)) || mmtTermEqualsValue(next.ElementAt(1))));

            if (!equationsHaveTwoElements || !equationsAreNormalized)
            {
                Debug.Log("SEqsysFact.ParseEquationSystem: Some equations either don't have 2 elements OR are not normalized.");
                return null;
            }
            else {
                //We want to create a GLS of the form 'A * x = b'
                List<List<double>> AData = new List<List<double>>();
                List<double> bData = new List<double>();
                //variables-list is used to bring elements of AData into the right order, and
                //to know how many elements each row in AData must have
                List<MMTTerm> variables = new List<MMTTerm>();

                bool processingEquationsSuccessfully = true;

                //Foreach equation, we want to add a row in A and an element in b
                foreach (List<MMTTerm> equation in this.equations) {
                    if (!processingEquationsSuccessfully)
                        break;

                    MMTTerm leftSide = equation.ElementAt(0);
                    MMTTerm rightSide = equation.ElementAt(1);

                    if (mmtTermEqualsValue(rightSide))
                    {
                        processingEquationsSuccessfully &= this.addToMatrixA(AData, variables, leftSide);
                        processingEquationsSuccessfully &= this.addToVectorB(bData, rightSide);
                    }
                    else {
                        processingEquationsSuccessfully &= this.addToMatrixA(AData, variables, rightSide);
                        processingEquationsSuccessfully &= this.addToVectorB(bData, leftSide);
                    }
                }

                if (processingEquationsSuccessfully)
                {
                    //Complement missing 0's in AData
                    int ADataColumns = variables.Count();
                    foreach (List<double> row in AData) {
                        for (int i = row.Count; i < ADataColumns; i++) {
                            row.Add(0);
                        }
                    }

                    //TODO: Filter duplicate rows in AData/bData
                    return new Tuple<List<List<double>>, List<double>, List<MMTTerm>>(AData, bData, variables);
                }
                else {
                    return null;
                }
            }
        }
    }

    /**A MMTTerm is a Value, if it has a positive float value (type OMF)
     * OR if it has a negative float value (type OMA(OMS(Minus), OMF))
     **/ 
    private bool mmtTermEqualsValue(MMTTerm term) {
        return ( term.GetType().Equals(typeof(OMF)) ) || 
               ( term.GetType().Equals(typeof(OMA))
                    && ((OMA) term).applicant.GetType().Equals(typeof(OMS))
                    && ((OMS)((OMA)term).applicant).uri.Equals(MMTURIs.Minus)
                    && ((OMA)term).arguments.Count == 1
                    && ((OMA)term).arguments.ElementAt(0).GetType().Equals(typeof(OMF)) );
    }

    /**
     * Used to process the right side of an equation
     * Returns false, if sth. went wrong with the equation
     */
    private bool addToVectorB(List<double> bData, MMTTerm newTerm)
    {
        if (newTerm.GetType().Equals(typeof(OMF)))
        {
            bData.Add( (double) ((OMF)newTerm).f );
            return true;
        }
        else if (newTerm.GetType().Equals(typeof(OMA))
                    && ((OMA)newTerm).applicant.GetType().Equals(typeof(OMS))
                    && ((OMS)((OMA)newTerm).applicant).uri.Equals(MMTURIs.Minus)
                    && ((OMA)newTerm).arguments.Count == 1
                    && ((OMA)newTerm).arguments.ElementAt(0).GetType().Equals(typeof(OMF)))
        {

            bData.Add( (double) (-1.0 * ((OMF)((OMA)newTerm).arguments.ElementAt(0)).f) );
            return true;
        }

        Debug.Log("SimplifiedFact.addToVectorB: newTerm wasn't either an OMF or an OMA(OMS(Minus),OMF)");
        return false;
    }

    /**
     * Used to process the left side of an equation
     * Returns false, if sth. went wrong with the equation
     */
    private bool addToMatrixA(List<List<double>> AData, List<MMTTerm> variables, MMTTerm newTerm) {

        List<double> newADataRow;
        Dictionary<MMTTerm, OMF> multPairs = new Dictionary<MMTTerm, OMF>();

        bool noErrors = true;
        List<MMTTerm> plusTerms = new List<MMTTerm>();
        getPlusTerms(plusTerms, newTerm);

        foreach (MMTTerm plusTerm in plusTerms) {
            //case: Times_real_lit of arguments
            //If the plusTerm is an OMA(OMS(times_real_lit), arguments), we know that one or both of the arguments
            //has to be a Variable, otherwise times_real_lit would have been simplified on the MMT-side
            if (plusTerm.GetType().Equals(typeof(OMA))
                    && ((OMA)plusTerm).applicant.GetType().Equals(typeof(OMS))
                    && ((OMS)((OMA)plusTerm).applicant).uri.Equals(MMTURIs.Multiplication))
            {
                if (!(((OMA)plusTerm).arguments.ElementAt(0).GetType().Equals(typeof(OMF)) ^ ((OMA)plusTerm).arguments.ElementAt(1).GetType().Equals(typeof(OMF))))
                {
                    noErrors = false;
                    Debug.Log("SimplifiedFact.addToMatrixA: Corrupt simplified times_real_lit term.");
                    break;
                }
                else {
                    OMF multiplier;
                    MMTTerm multiplicand;
                    //Element at Index = 0 is OMF
                    if (((OMA)plusTerm).arguments.ElementAt(0).GetType().Equals(typeof(OMF)))
                    {
                        multiplier = (OMF)((OMA)plusTerm).arguments.ElementAt(0);
                        multiplicand = ((OMA)plusTerm).arguments.ElementAt(1);
                    }
                    //Element at Index = 1 is OMF
                    else
                    {
                        multiplier = (OMF)((OMA)plusTerm).arguments.ElementAt(1);
                        multiplicand = ((OMA)plusTerm).arguments.ElementAt(0);
                    }

                    if (!(variables.Contains(multiplicand))) {
                        variables.Add(multiplicand);
                    }

                    multPairs.Add(multiplicand, multiplier);
                }
            }
        }

        newADataRow = new List<double>(new double[variables.Count]);
        foreach (KeyValuePair<MMTTerm, OMF> multPair in multPairs) {
            newADataRow[variables.IndexOf(multPair.Key)] = multPair.Value.f;
        }

        AData.Add(newADataRow);
        return noErrors;
    }

    private void getPlusTerms(List<MMTTerm> plusTerms, MMTTerm newTerm) {
        if (newTerm.GetType().Equals(typeof(OMA))
                && ((OMA)newTerm).applicant.GetType().Equals(typeof(OMS))
                && ((OMS)((OMA)newTerm).applicant).uri.Equals(MMTURIs.Addition))
        {
            //plus_real_lit is a function with two input parameters
            getPlusTerms(plusTerms, ((OMA)newTerm).arguments.ElementAt(0));
            getPlusTerms(plusTerms, ((OMA)newTerm).arguments.ElementAt(1));
        }
        else
        {
            plusTerms.Add(newTerm);
        }
    }
}
