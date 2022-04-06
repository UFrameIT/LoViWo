using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using MathNet.Numerics.LinearAlgebra;
using static JSONManager;

public class KnowlegeBasedSimulationRefactor : MonoBehaviour
{
    public static Dictionary<ValueOfInterest, float> knowledgeBasedSimulation(List<Fact> facts, List<ValueOfInterest> valuesOfIntrest, List<SimplifiedFact> sFactList)
    {
        
        if (sFactList == null)
        { 
            return null;
        }
        else
        {
            List<SEqsysFact> sEqsysFacts = sFactList.FindAll(sFact => sFact.GetType().Equals(typeof(SEqsysFact)))
                                                    .Select(sFact => (SEqsysFact)sFact)
                                                    .ToList();
            SEqsysFact eqsysFactForSimulation = null;

            if (sEqsysFacts.Count == 0)
            {
                Debug.Log("KnowledgeBasedSimulation: sFactList contains no SEqsysFact.");
                return null;
            }
            else if (sEqsysFacts.Count > 1)
            {
                Debug.Log("KnowledgeBasedSimulation: sFactList contains more than one SEqsysFact. Using newest one for Simulation.");
                eqsysFactForSimulation = sEqsysFacts.ElementAt(sEqsysFacts.Count - 1);
            }
            else
                eqsysFactForSimulation = sEqsysFacts.ElementAt(0);

            if (eqsysFactForSimulation != null)
            {
                //Prepare Data (parse equations) for gls-solver

                Debug.Log("eqsysFactForSimulation: " + eqsysFactForSimulation);

                Tuple<List<List<double>>, List<double>, List<MMTTerm>> glsTuple = eqsysFactForSimulation.parseEquationSystem();

                if (glsTuple == null)
                {
                    Debug.Log("KnowledgeBasedSimulation: Sth. went wrong while parsing the EquationSystem.");
                    return null;
                }
                else
                {
                    List<List<double>> AData = glsTuple.Item1;
                    List<double> bData = glsTuple.Item2;
                    List<MMTTerm> variables = glsTuple.Item3;

                    int numberOfVariables = AData[0].Count;

                    Matrix<double> A = Matrix<double>.Build.DenseOfRows(AData);
                    Vector<double> b = Vector<double>.Build.DenseOfEnumerable(bData);

                    string[] equations = equationsToString(AData, bData, variables, valuesOfIntrest);

                    /****DETERMINE HOW MANY SOLUTIONS THE LINEAR-EQUATION-SYSTEM HAS AND REACT CORRESPONDINGLY****/
                    int rankA = A.Rank();

                    if (AData.Count == bData.Count)
                    {
                        for (int i = 0; i < AData.Count; i++)
                        {
                            AData[i].Add(bData[i]);
                        }

                        Matrix<double> AExtended = Matrix<double>.Build.DenseOfRows(AData);
                        int rankAExtended = AExtended.Rank();
                        string solutions = "";

                        //TODO: SHOW EQUATION-SYSTEM / RANKS ETC. / MAYBE WITH USING COMMUNICATIONEVENTS

                        if (rankA != rankAExtended)
                        {
                            solutions = "0";
                            Debug.Log(String.Format("The linear EquationSystem has NO solution. Reason: rank of A and rank of AExtended are not equal. RankA = {0}, RankAExtended = {1}", rankA, rankAExtended));
                            CommunicationEvents.showEquationSystemEvent.Invoke(equations, rankA, rankAExtended, solutions);
                            return null;
                        }
                        else if (rankA < numberOfVariables)
                        {
                            solutions = "∞";
                            Debug.Log(String.Format("The linear EquationSystem has an infinite number of solutions. Reason: rank of A = rank of AExtended = {0} < varNum = {1}", rankA, numberOfVariables));
                            CommunicationEvents.showEquationSystemEvent.Invoke(equations, rankA, rankAExtended, solutions);
                            return null;
                        }
                        else if (rankA == numberOfVariables)
                        {
                            solutions = "1";
                            Debug.Log(String.Format("The linear EquationSystem has exactly one solution. Reason: rank of A = rank of AExtended = {0} = varNum = {1}", rankA, numberOfVariables));

                            CommunicationEvents.showEquationSystemEvent.Invoke(equations, rankA, rankAExtended, solutions);

                            //Solve GLS of the form 'A * x = b':
                            Vector<double> glsSolution = A.Solve(b);

                            //Map the glsSolution to the variables and to the corresponding cogwheels
                            return getNewlyDiscoveredAvsMap(variables, glsSolution, valuesOfIntrest);
                        }
                        else
                        {
                            solutions = "?";
                            Debug.Log(String.Format("Rank of A = rank of AExtended = {0} > varNum = {1}", rankA, numberOfVariables));
                            CommunicationEvents.showEquationSystemEvent.Invoke(equations, rankA, rankAExtended, solutions);
                            return null;
                        }
                    }
                    else
                    {
                        Debug.Log("Row-Count of Matrix A and Vector b were not equal!");
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }

    private static List<SimplifiedFact> listSimplifiedFacts()
    {
        UnityWebRequest request = UnityWebRequest.Get(GameSettings.ServerAdress + "/fact/list");
        request.method = UnityWebRequest.kHttpVerbGET;
        AsyncOperation op = request.SendWebRequest();
        while (!op.isDone) { }
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(request.error);
            return null;
        }
        else
        {
            string response = request.downloadHandler.text;
            Debug.Log("KnowledgeBasedSimulation: Json-Response from /fact/list-endpoint: " + response);
            return SimplifiedFact.FromJSON(response);
        }
    }

    private static Dictionary<ValueOfInterest, float> getNewlyDiscoveredAvsMap(List<MMTTerm> variables, Vector<double> glsSolution, List<ValueOfInterest> valuesOfIntrest)
    {
        Dictionary<ValueOfInterest, float> discoveredAvsMap = new Dictionary<ValueOfInterest, float>();

        //Find KeyValuePairs<Fact,float> ...
        if (glsSolution.Count.Equals(variables.Count))
        {
            foreach (ValueOfInterest valueOfInterest in valuesOfIntrest)
            {
                if (valueOfInterest.getRelevantFact() == null)
                {
                    continue;
                }
                Fact fact = valueOfInterest.getRelevantFact();

                if (fact.GetType().Equals(typeof(CogwheelFact)))
                {
                    Debug.Log("getNewlyDiscoveredAvsMap: encountered Cogwheel valOfInt in valsOfInt.");
                    /*
                    MMTTerm knownAvVariable = variables.Find(variable => variable.isSimplifiedCogwheelAvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(0)).value).f.Equals(((CogwheelFact)fact).Radius)
                                                                        && ((OMF)((OMA)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(0)).f.Equals(((CogwheelFact)fact).Point.x)
                                                                        && ((OMF)((OMA)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(1)).f.Equals(((CogwheelFact)fact).Point.y)
                                                                        && ((OMF)((OMA)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(2)).f.Equals(((CogwheelFact)fact).Point.z));
                    */
                    MMTTerm knownAvVariable = variables.Find(variable => variable.isSimplifiedCogwheelAvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(7)).value).f.Equals((float)((CogwheelFact)fact).Id));
                    //If an element was found
                    if (knownAvVariable != null)
                    {
                        double newlyDiscoveredAv = glsSolution.ElementAt(variables.IndexOf(knownAvVariable));
                        //Now we know that the Fact = fact should rotate with angularVelocity = newlyDiscoveredAv
                        discoveredAvsMap.Add(valueOfInterest, (float)newlyDiscoveredAv);
                    }
                }
                //ToDo
                else if (fact.GetType().Equals(typeof(ChainFact)))
                {
                    Debug.Log("getNewlyDiscoveredAvsMap: encountered Chain valOfInt in valsOfInt.");
                    MMTTerm knownCvVariable;
                    knownCvVariable = variables.Find(variable => variable.isSimplifiedChainCvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(1)).value).f.Equals((float)((ChainFact)fact).Id));
                    if (knownCvVariable != null)
                    {
                        Debug.Log("knownCvVariable not null.");
                        double newlyDiscoveredCv = glsSolution.ElementAt(variables.IndexOf(knownCvVariable));
                        //Now we know that the Fact = fact should rotate with angularVelocity = newlyDiscoveredAv
                        discoveredAvsMap.Add(valueOfInterest, (float)newlyDiscoveredCv);
                    }
                    else
                    {
                        Debug.Log("knownCvVariable null.");
                    }
                }
                else if (fact.GetType().Equals(typeof(ShaftFact)))
                {
                    Debug.Log("getNewlyDiscoveredAvsMap: encountered Shaft valOfInt in valsOfInt.");
                    MMTTerm knownAvVariable;
                    knownAvVariable = variables.Find(variable => variable.isSimplifiedShaftAvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(0)).value).f.Equals((float)((ShaftFact)fact).Id));
                    if (knownAvVariable != null)
                    {
                        Debug.Log("knownCvVariable not null.");
                        double newlyDiscoveredCv = glsSolution.ElementAt(variables.IndexOf(knownAvVariable));
                        //Now we know that the Fact = fact should rotate with angularVelocity = newlyDiscoveredAv
                        discoveredAvsMap.Add(valueOfInterest, (float)newlyDiscoveredCv);
                    }
                    else
                    {
                        Debug.Log("knownCvVariable null.");
                    }
                }

            }
        }
        else
        {
            Debug.Log("KnowledgeBasedSimulation.getNewlyDiscoveredAvsMap: Variables and GLS-Solution don't have the same number of elements.");
            return null;
        }

        //Build relative complement A \ B => discoveredAvsMap \ knownAvMap
        return discoveredAvsMap.ToDictionary(x => x.Key, x => x.Value);
    }

    private static string[] equationsToString(List<List<double>> AData, List<double> bData, List<MMTTerm> variables, List<ValueOfInterest> valuesOfInterest)
    {
        List<string> equations = new List<String>();

        if (AData.Count == bData.Count)
        {
            for (int i = 0; i < AData.Count; i++)
            {
                string equation = "";

                for (int j = 0; j < AData[i].Count; j++)
                {
                    MMTTerm variable = variables.ElementAt(j);

                    if (valuesOfInterest == null)
                    {
                        Debug.Log("valuesOfInterest == null");
                    }
                    if (variable == null)
                    {
                        Debug.Log("variable == null");
                    }

                    //debug
                    Debug.Log("values of intrest:");
                    foreach (ValueOfInterest v in valuesOfInterest)
                    {
                        Debug.Log(v + " :");
                        Debug.Log("voi name :"+ v.getName());
                        Debug.Log("voi relevant fact :" + v.getRelevantFact());
                        Debug.Log("voi relevant value :" + v.getRelevantValue());
                    }
                    Debug.Log("variable: " + variable);

                    ValueOfInterest voi = variableGetValueOfIntrest(valuesOfInterest, variable);
                    string voiName = voi.getName();

                    //TODO

                    if (j == AData[i].Count - 1)
                    {
                        equation += AData[i][j].ToString() + "*" + voiName + " = " + bData[i].ToString();
                    }
                    else
                    {
                        equation += AData[i][j].ToString() + "*" + voiName + " + ";
                    }
                }

                equations.Add(equation);
            }

            return equations.ToArray();
        }
        else
            return equations.ToArray();
    }

    private static ValueOfInterest variableGetValueOfIntrest(List<ValueOfInterest> valuesOfInterest, MMTTerm variable)
    {
        Debug.Log("test");
        ValueOfInterest valueOfInterest = null;

        if (variable.isSimplifiedCogwheelAvTerm())
        {
            Debug.Log("Cogwheel");
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(7)).value).f;
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }
        else if (variable.isSimplifiedChainCvTerm())
        {
            Debug.Log("Chain");
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(1)).value).f;
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }
        else if (variable.isSimplifiedShaftAvTerm())
        {
            Debug.Log("Shaft");
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(0)).value).f;
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }

        return valueOfInterest;
    }
}
