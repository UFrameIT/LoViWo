using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class KnowledgeBasedSimulation
{
    public static void startKnowledgeBasedSimulation() {
        List<SimplifiedFact> sFactList = GameState.LastKBSimulationResult.Item2;

        //Only send server-request when global fact-list has changed
        if (!GameState.LastKBSimulationResult.Item1.Equals(GameState.Facts))
        {
            if (GameState.ServerRunning)
            {
                CogwheelEqsysFact eqsysFact = addEqsysFact();
                List<SimplifiedFact> response = listSimplifiedFacts();

                if (response != null)
                {
                    sFactList = response;
                }
                else {
                    Debug.Log("KnowledgeBasedSimulation: /fact/list reponse is null. Using LastKBSimuationResult.");
                }
            }
            else
            {
                Debug.LogWarning("KnowledgeBasedSimulation: Cannot send server-request, because FrameIT-Server is not running.");
            }
        }

        if (sFactList != null)
        {
            Debug.Log("Works up to here. Todo: Prepare for GLS-Solver");
            //prepare Data for gls-solver
            //solve
            //use result to rotate cogwheels (and their connected parts)
        }
        else {
            Debug.Log("KnowledgeBasedSimulation: Cannot simulate, sFactList is null.");
        }
    }

    private static CogwheelEqsysFact addEqsysFact() {
        int eqsysFactId = GameState.Facts.Count;
        int[] cogIds = GameState.Facts.FindAll(fact => fact.GetType().Equals(typeof(CogwheelFact))).Select(fact => fact.Id).ToArray();
        CogwheelEqsysFact eqsys = new CogwheelEqsysFact(eqsysFactId, cogIds);
        GameState.Facts.Insert(eqsysFactId, eqsys);
        return eqsys;
    }

    private static List<SimplifiedFact> listSimplifiedFacts() {
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
}
