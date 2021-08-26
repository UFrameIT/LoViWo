using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KnowledgeBasedSimulationPanel : MonoBehaviour
{
    public GameObject equationsContent;
    public GameObject equationsPrefab;
    public GameObject rankAText;
    public GameObject solutionsText;
    public GameObject rankAbText;

    // Start is called before the first frame update
    void Start()
    {
        //KnowledgeBasedSimulationPanel should not be visible on startup
        Deactivate();

        CommunicationEvents.showEquationSystemEvent.AddListener(showEquations);
        CommunicationEvents.generatorOffEvent.AddListener(Deactivate);
    }

    public void showEquations(string[] equations, int rankA, int rankAb, string solutions) {
        this.gameObject.SetActive(true);

        //Clear previous content
        foreach (Transform child in equationsContent.transform) {
            GameObject.Destroy(child.gameObject);
        }

        //Create an object for each equation
        foreach (string equation in equations) {
            GameObject equationObj = Instantiate(equationsPrefab);
            equationObj.transform.SetParent(equationsContent.transform, false);
            equationObj.GetComponentInChildren<TMP_Text>().text = equation;
        }

        rankAText.GetComponentInChildren<TMP_Text>().text = "Rank(A): " + rankA.ToString();
        rankAbText.GetComponentInChildren<TMP_Text>().text = "Rank(A|b): " + rankAb.ToString();
        solutionsText.GetComponentInChildren<TMP_Text>().text = "Solutions: " + solutions;
    }

    public void Deactivate() {
        this.gameObject.SetActive(false);
    }
}
