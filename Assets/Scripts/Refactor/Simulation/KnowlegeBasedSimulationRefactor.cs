using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using static JSONManager;


/**
 * provides functionality for extracting and solving the equationsystem returned by the server
 */
public class KnowlegeBasedSimulationRefactor : MonoBehaviour
{
    /**
     * takes a list of simplified facts and a list of the current Values of interest
     * trys solving the equationsystem
     * if successfull returns a Dictionary mapping the values of interest to the calculated valuse from the equation system 
     */
    public static Dictionary<ValueOfInterest, float> knowledgeBasedSimulation(List<ValueOfInterest> valuesOfIntrest, List<SimplifiedFact> sFactList)
    {

        if (sFactList == null)
        {
            return null;
        }
        else
        {
            //extract all simplifiedEquationSystemFacts out of the list of all simplified facts
            List<SEqsysFact> sEqsysFacts = sFactList.FindAll(sFact => sFact.GetType().Equals(typeof(SEqsysFact)))
                                                    .Select(sFact => (SEqsysFact)sFact)
                                                    .ToList();
            SEqsysFact eqsysFactForSimulation = null;
            //extract the most recent simplifiedEquationSystemFact from the list of all simplifiedEquationSystemFacts
            //to be used for subsequent calculations
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

                //parse the equation system in the simplified equation-system fact into a form
                //that can be solved by the MathNet.Numerics.LinearAlgebra library
                //(list of lists of doubles for representing Matrix A
                // list of doubles for representing the vecto b
                // list of MMTTerms for representing the variable for which are to be solved)
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

                    Matrix<double> A = Matrix<double>.Build.DenseOfRows(AData);
                    Vector<double> b = Vector<double>.Build.DenseOfEnumerable(bData);

                    //parse the equations encoded in the glsTuple into a human readable form
                    string[] equations = equationsToString(A, b, variables, valuesOfIntrest);

                    /****DETERMINE HOW MANY SOLUTIONS THE LINEAR-EQUATION-SYSTEM HAS AND REACT CORRESPONDINGLY****/
                    int rankA = A.Rank();

                    if (AData.Count != bData.Count)
                    {
                        Debug.Log("Row-Count of Matrix A and Vector b were not equal!");
                        return null;
                    }

                    Tuple<List<Matrix<double>>, List<Vector<double>>, List<List<MMTTerm>>> independentMatrices = seperateIndependentEqsysComponents(A, b, variables);
                    List<int> ARanks = new List<int>();
                    List<int> AExtendedRanks = new List<int>();
                    List<string[]> equationsList = new List<string[]>();
                    List<string> numbersOfSolutions = new List<string>();

                    Dictionary<ValueOfInterest, float> discoveredVoiVals = new Dictionary<ValueOfInterest, float>();

                    for (int i = 0; i < independentMatrices.Item1.Count; i++)
                    {
                        int ARank = independentMatrices.Item1[i].Rank();
                        ARanks.Add(ARank);
                        int AExtendedRank = independentMatrices.Item1[i].InsertColumn(independentMatrices.Item1[i].ColumnCount, independentMatrices.Item2[i]).Rank();
                        AExtendedRanks.Add(AExtendedRank);
                        int numberOfVariables = independentMatrices.Item3[i].Count;


                        if (ARank != AExtendedRank)
                        {
                            numbersOfSolutions.Add("0");
                            equationsList.Add(new string[0]);

                            Debug.Log(String.Format("The linear EquationSystem has NO solution. Reason: rank of A and rank of AExtended are not equal. RankA = {0}, RankAExtended = {1}", ARank, AExtendedRank));
                            
                            continue;
                        }

                        string[] eqs = equationsToString(independentMatrices.Item1[i], independentMatrices.Item2[i], independentMatrices.Item3[i], valuesOfIntrest);                        


                        if (ARank < numberOfVariables)
                        {
                            numbersOfSolutions.Add("∞");
                            equationsList.Add(eqs);

                            Debug.Log(String.Format("The linear EquationSystem has an infinite number of solutions. Reason: rank of A = rank of AExtended = {0} < varNum = {1}", rankA, numberOfVariables));
                            continue;
                        }
                        else if (ARank == numberOfVariables)
                        {
                            numbersOfSolutions.Add("1");
                            equationsList.Add(eqs);

                            Debug.Log(String.Format("The linear EquationSystem has exactly one solution. Reason: rank of A = rank of AExtended = {0} = varNum = {1}", rankA, numberOfVariables));
                            Vector<double> glsSolution = independentMatrices.Item1[i].Solve(independentMatrices.Item2[i]);
                            getNewlyDiscoveredAvsMap(discoveredVoiVals, independentMatrices.Item3[i], glsSolution, valuesOfIntrest);

                            continue;
                        }
                        else
                        {
                            numbersOfSolutions.Add("?");
                            equationsList.Add(new string[0]);

                            Debug.Log(String.Format("Rank of A = rank of AExtended = {0} > varNum = {1}", rankA, numberOfVariables));

                            continue;
                        }   
                    }
                    Debug.Log("test");
                    CommunicationEvents.showEquationSystemsEvent.Invoke(equationsList, ARanks, AExtendedRanks, numbersOfSolutions);

                    return discoveredVoiVals;              
                }
            }
            else
            {
                return null;
            }
        }
    }

    /*
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
    */


    //function that maps the values in the solution of our equation-system to the corresponding values of interest
    //takes the variables of the eqaution-system, the solution of the eqaution system and a List of all relevant values of interest as input
    private static void getNewlyDiscoveredAvsMap(Dictionary<ValueOfInterest, float> discoveredVoiVals, List<MMTTerm> variables, Vector<double> glsSolution, List<ValueOfInterest> valuesOfIntrest)
    {
        //dictionary that will map the values of interest to the corresponding values in the solution of our eqaution system
        //Dictionary<ValueOfInterest, float> discoveredVoiVals = new Dictionary<ValueOfInterest, float>();

        /*
         * for each value of interest
         * see wether one of the variables in the equation-system corresponds to that value of interest
         * and if yes, map the corresponding value from the solution of the eqsys to that value of interest
         */
        if (glsSolution.Count.Equals(variables.Count)) //only continue if our solution has a value for each of our variables
        {
            foreach (ValueOfInterest valueOfInterest in valuesOfIntrest)
            {
                if (valueOfInterest.getRelevantFact() == null)
                {
                    continue;
                }
                //get the fact representing the object associated with the value of interest
                Fact fact = valueOfInterest.getRelevantFact();

                //see if one of the variable in our equation-system represents the value of interest
                MMTTerm correspondingVariable = null;
                if (fact.GetType().Equals(typeof(CogwheelFact)))
                {
                    //Debug.Log("getNewlyDiscoveredAvsMap: encountered Cogwheel valOfInt in valsOfInt.");
                    correspondingVariable = variables.Find(variable => variable.isSimplifiedCogwheelAvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(7)).value).f.Equals((float)((CogwheelFact)fact).Id));
                }
                else if (fact.GetType().Equals(typeof(ChainFact)))
                {
                    //Debug.Log("getNewlyDiscoveredAvsMap: encountered Chain valOfInt in valsOfInt.");
                    correspondingVariable = variables.Find(variable => variable.isSimplifiedChainCvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(1)).value).f.Equals((float)((ChainFact)fact).Id));
                }
                else if (fact.GetType().Equals(typeof(ShaftFact)))
                {
                    //Debug.Log("getNewlyDiscoveredAvsMap: encountered Shaft valOfInt in valsOfInt.");
                    correspondingVariable = variables.Find(variable => variable.isSimplifiedShaftAvTerm()
                                                                        && ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(0)).value).f.Equals((float)((ShaftFact)fact).Id));
                }

                //if the value of interest is represented by one of the variables
                //then get the value from the solution corresponding to that variable from the solution
                //and map the value to the value of interest
                if (correspondingVariable != null)
                {
                    double discoveredVal = glsSolution.ElementAt(variables.IndexOf(correspondingVariable));
                    discoveredVoiVals.Add(valueOfInterest, (float)discoveredVal);
                }
            }
        }
        else
        {
            Debug.Log("KnowledgeBasedSimulation.getNewlyDiscoveredAvsMap: Variables and GLS-Solution don't have the same number of elements.");
        }

    }

    //function for parseing the equations encoded in the glsTuple into a human readable form
    private static string[] equationsToString(Matrix<double> A, Vector<double> b, List<MMTTerm> variables, List<ValueOfInterest> valuesOfInterest)
    {
        List<string> equations = new List<String>();

        if (A.RowCount == b.Count)
        {
            for (int i = 0; i < A.RowCount; i++) //for each equation is the equation system
            {
                string equation = ""; //have a string into which the equation is written

                for (int j = 0; j < A.ColumnCount; j++) //for each variable in the current equation
                {
                    //get the value of interest represented by the current variable
                    MMTTerm variable = variables.ElementAt(j);
                    ValueOfInterest voi = variableGetValueOfIntrest(valuesOfInterest, variable);
                    //get the name of said value of interest
                    string voiName = voi.getName();

                    if (j == A.ColumnCount - 1)
                    {
                        equation += A.Row(i)[j].ToString() + "*" + voiName + " = " + b[i].ToString();
                    }
                    else
                    {
                        equation += A.Row(i)[j].ToString() + "*" + voiName + " + ";
                    }
                }

                equations.Add(equation);
            }

            return equations.ToArray();
        }
        else
            return equations.ToArray();
    }


    //function that given the current values of interest and a variable from our equation-system
    //returns the value of interest that is represented by the variable
    //(if no value of interest is found null is returned)
    private static ValueOfInterest variableGetValueOfIntrest(List<ValueOfInterest> valuesOfInterest, MMTTerm variable)
    {
        ValueOfInterest valueOfInterest = null;

        //see what 'kind' of value of interest is beeing represented by the variable
        if (variable.isSimplifiedCogwheelAvTerm())
        {
            //get the id of the object fact associated with the variable
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(7)).value).f;
            //find the value of interest that has an object fact associated with it
            //that has the same id as the object fact associated with the variable
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }
        else if (variable.isSimplifiedChainCvTerm())
        {
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(1)).value).f;
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }
        else if (variable.isSimplifiedShaftAvTerm())
        {
            float id = ((OMF)((RECARG)((OMA)((OMA)variable).arguments.ElementAt(0)).arguments.ElementAt(0)).value).f;
            valueOfInterest = valuesOfInterest.Find(voi => voi.getRelevantFact() != null && ((float)voi.getRelevantFact().Id).Equals(id));
        }

        return valueOfInterest;
    }



    private static Tuple<List<Matrix<double>>, List<Vector<double>>, List<List<MMTTerm>>> seperateIndependentEqsysComponents(Matrix<double> A, Vector<double> b, List<MMTTerm> variables)
    {
        /*
         * 1. Turn matrix A into a list of arrays containing the matrix rows
         * Turn vector b into a list of double values
         *
         * 2. Initialize the lists in which to gather the a matrices, b vectors and variable lists of our independent equation systems 
         *
         * 3. Create a ‘group’ of equations in which to gather all equations that share ‘non-0 variables’ across them. Remove the first row out of the row list and add it to the group (do the same for the first b value int the list of b values). Iterate over the remaining rows (and by extension b values) and remove and add all rows (and corresponding b values) which share a ‘non-0 variable’ with any row collected in the group. Repeat this until no such rows remain in the rows list.
         *
         * 4. Turn the list of collected rows of the group into a matrix. Remove all columns out of the matrix that contain only zeros
         *
         * 5. Once all rows of a group are collected save all variables out of the input variables list that are not 0 in any of the rows in a list.
         *
         * 6. add the created matrix, a vector created out of the collected b values, and the created list of variables to the lists created in 2.
         *
         * 7. repeat 3. - 6. until no rows remain in the rows list
         *
         * 8. create a tuple out of the Lists created in 2. and return it
         */



        // 1.
        List<double[]> ARows = A.ToRowArrays().ToList();
        List<double> bValues = b.ToList();

        // 2.
        List<Matrix<double>> Matrices = new List<Matrix<double>>();
        List<List<double>> bValuesGroups = new List<List<double>>();
        List<List<MMTTerm>> variablesGroups = new List<List<MMTTerm>>();

        // 7.
        while (ARows.Count > 0)
        {
            //3.
            List<double[]> currentGroup = new List<double[]>();
            currentGroup.Add(ARows.ElementAt(0));
            ARows.RemoveAt(0);

            //
            List<double> currentGroup_bValues = new List<double>();
            currentGroup_bValues.Add(bValues.ElementAt(0));
            bValues.RemoveAt(0);

            //
            BitArray currentGroupNon0Variables = new BitArray(currentGroup.ElementAt(0).Length);
            for (int i = 0; i < currentGroupNon0Variables.Length; i++)
            {
                currentGroupNon0Variables[i] = System.Convert.ToBoolean(currentGroup.ElementAt(0)[i]);
            }

            //
            bool expandetGroup;
            do
            {
                //
                List<double[]> ARowsList = ARows.ToList();

                //
                expandetGroup = false;


                for (int i = ARowsList.Count - 1; i >= 0; i--)
                {
                    //
                    BitArray equationNon0Variables = new BitArray(ARowsList[i].Length);
                    for (int j = 0; j < ARowsList[i].Length; j++)
                    {
                        equationNon0Variables[j] = Convert.ToBoolean(ARowsList[i][j]);
                    }

                    //
                    BitArray equationNon0VariablesClone = (BitArray)equationNon0Variables.Clone();
                    if (equationNon0VariablesClone.And(currentGroupNon0Variables).Cast<bool>().Contains(true))
                    {
                        //
                        expandetGroup = true;

                        //
                        currentGroup.Add(ARowsList[i]);
                        ARows.Remove(ARowsList[i]);

                        //
                        currentGroup_bValues.Add(bValues[i]);
                        bValues.RemoveAt(i);

                        //
                        currentGroupNon0Variables.Or(equationNon0Variables);
                        
                    }
                }
            }
            while (expandetGroup == true);

            // 4.
            Matrix<double> matrix = Matrix<double>.Build.DenseOfRows(currentGroup);
            bValuesGroups.Add(currentGroup_bValues);

            //
            for (int i = currentGroupNon0Variables.Count - 1; i >= 0; i--)
            {
                if (!currentGroupNon0Variables[i])
                {
                    //
                    matrix = matrix.RemoveColumn(i);
                }
            }

            // 5.
            List<MMTTerm> currentGroupVariables = new List<MMTTerm>();
            for (int i = 0; i < currentGroupNon0Variables.Count; i++)
            {
                if (currentGroupNon0Variables[i])
                {
                    //
                    currentGroupVariables.Add(variables.ElementAt(i));
                }
            }
            // 6.
            variablesGroups.Add(currentGroupVariables);

            // 6.
            Matrices.Add(matrix);

            //
        }

        // 6.
        List<Vector<double>> bValueVectors = new List<Vector<double>>();
        foreach (List<double> bValueGroup in bValuesGroups)
        {
            bValueVectors.Add(Vector<double>.Build.DenseOfEnumerable(bValueGroup));
        }

        // 7.
        return new Tuple<List<Matrix<double>>, List<Vector<double>>, List<List<MMTTerm>>>(Matrices, bValueVectors, variablesGroups);
    }

    /*
    private static Tuple<List<Matrix<double>>, List<Vector<double>>, List<List<MMTTerm>>> seperateIndependentEqsysComponents(Matrix<double> eqsys, Vector<double> bVector, List<MMTTerm> variables)
    {
        Debug.Log("seperateIndependentEqsysComponents:");

        List<double[]> inputEquations = eqsys.ToRowArrays().ToList();

        List<List<double>> remainingEquations = new List<List<double>>();
        foreach(double[] eq in inputEquations)
        {
            remainingEquations.Add(eq.ToList());
        }

        List<double> remaining_bValues = bVector.ToList();

        List<List<List<double>>> equationGroups = new List<List<List<double>>>();
        List<List<double>> bValueGroups = new List<List<double>>();
        List<List<MMTTerm>> variablesGroups = new List<List<MMTTerm>>();

        while (remainingEquations.Count > 0)
        {
            List<List<double>> currentEquationGroup = new List<List<double>>();
            currentEquationGroup.Add(remainingEquations.ElementAt(0));
            remainingEquations.RemoveAt(0);
            List<double> currentGroup_bValues = new List<double>();
            currentGroup_bValues.Add(remaining_bValues.ElementAt(0));
            remaining_bValues.RemoveAt(0);

            BitArray currentGroupNon0Variables = new BitArray(currentEquationGroup.ElementAt(0).Count);
            Debug.Log("currentGroupVariables:");
            for (int i = 0; i < currentGroupNon0Variables.Length; i++)
            {
                currentGroupNon0Variables[i] = System.Convert.ToBoolean(currentEquationGroup.ElementAt(0)[i]);
                Debug.Log(currentGroupNon0Variables[i]);
            }

            bool expandetGroup;
            do
            {
                List<List<double>> remainingEquationsList = remainingEquations.ToList();
                expandetGroup = false;
                for (int i = 0; i < remainingEquationsList.Count; i++)
                {
                    BitArray equationGroupVariables = new BitArray(remainingEquationsList[i].Count);
                    Debug.Log("equationGroupVariables");
                    for (int j = 0; j < remainingEquationsList[i].Count; j++)
                    {
                        equationGroupVariables[j] = Convert.ToBoolean(remainingEquationsList[i][j]);
                        Debug.Log(equationGroupVariables[j]);
                    }

                    BitArray equationGroupVariablesClone = (BitArray)equationGroupVariables.Clone();
                    if (equationGroupVariablesClone.And(currentGroupNon0Variables).Cast<bool>().Contains(true))
                    {
                        Debug.Log("dependant equations");
                        expandetGroup = true;

                        currentEquationGroup.Add(remainingEquationsList[i]);
                        remainingEquations.Remove(remainingEquationsList[i]);

                        currentGroup_bValues.Add(remaining_bValues[i]);
                        remaining_bValues.Remove(remaining_bValues[i]);

                        currentGroupNon0Variables.Or(equationGroupVariables);
                        Debug.Log("currentGroupNon0Variables");
                        for (int j = 0; j < currentGroupNon0Variables.Count; j++)
                        {
                            Debug.Log(currentGroupNon0Variables[j]);
                        }
                    }
                }
            }
            while (expandetGroup == true);

            List<MMTTerm> currentGroupVariables = new List<MMTTerm>();

            for (int i = currentGroupNon0Variables.Count - 1; i >= 0; i--)
            {
                if (currentGroupNon0Variables[i])
                {
                    currentGroupVariables.Add(variables.ElementAt(i));
                }
                else 
                {
                    foreach (List<double> eq in currentEquationGroup)
                    {
                        //eq.RemoveAt(i);
                    }
                }
            }

            equationGroups.Add(currentEquationGroup);
            bValueGroups.Add(currentGroup_bValues);
            variablesGroups.Add(currentGroupVariables);
        }

        Debug.Log("equationGroups count: " + equationGroups.Count);


        List<Matrix<double>> retMatrixList = new List<Matrix<double>>();
        List<Vector<double>> retVectorList = new List<Vector<double>>();

        for (int i = 0; i < equationGroups.Count; i++)
        {
            retMatrixList.Add(Matrix<double>.Build.DenseOfRows(equationGroups[i]));
            retVectorList.Add(Vector<double>.Build.DenseOfEnumerable(bValueGroups[i]));
        }

        return new Tuple<List<Matrix<double>>, List<Vector<double>>, List<List<MMTTerm>>>(retMatrixList, retVectorList, variablesGroups);
    }
    */
}
