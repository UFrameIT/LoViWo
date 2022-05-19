using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquationsDisplayPanel : MonoBehaviour
{
    public GameObject equationsContent;
    public GameObject equationsPrefab;
    public GameObject rankAText;
    public GameObject solutionsText;
    public GameObject rankAbText;
    public GameObject NumberEquationSystemsText;
    public GameObject CurrentEquationSystemText;

    private int currentEquationSytem;
    private List<string[]> equationsList;
    private List<int> ARanksList;
    private List<int> AbRanksList;
    private List<string> numbersOfsolutions;

    // Start is called before the first frame update
    void Start()
    {
        //KnowledgeBasedSimulationPanel should not be visible on startup
        Deactivate();

        CommunicationEvents.showEquationSystemsEvent.AddListener(showEquationSystems);
    }

    public void showEquationSystems(List<string[]> equationsList, List<int> ARanksList, List<int> AbRanksList, List<string> numbersOfsolutions)
    {
        Debug.Log("showEquationSystems called");
        this.gameObject.SetActive(true);

        this.equationsList = equationsList;
        this.ARanksList = ARanksList;
        this.AbRanksList = AbRanksList;
        this.numbersOfsolutions = numbersOfsolutions;

        this.currentEquationSytem = 0;

        if (equationsList.Count > 0)
        {
            NumberEquationSystemsText.GetComponentInChildren<TMP_Text>().text = "Number of Equation Systems:  " + equationsList.Count.ToString();
            CurrentEquationSystemText.GetComponentInChildren<TMP_Text>().text = "current Equation System:  " + "1";

            showEquationSystem(equationsList[0], ARanksList[0], AbRanksList[0], numbersOfsolutions[0]);
        }

    }

    private void showEquationSystem(string[] equations, int rankA, int rankAb, string numberOfsolutions)
    {

        //Clear previous content
        foreach (Transform child in equationsContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Create an object for each equation
        foreach (string equation in equations)
        {
            GameObject equationObj = Instantiate(equationsPrefab);
            equationObj.transform.SetParent(equationsContent.transform, false);
            equationObj.GetComponentInChildren<TMP_Text>().text = equation;
        }

        rankAText.GetComponentInChildren<TMP_Text>().text = "Rank(A): " + rankA.ToString();
        rankAbText.GetComponentInChildren<TMP_Text>().text = "Rank(A|b): " + rankAb.ToString();
        solutionsText.GetComponentInChildren<TMP_Text>().text = "Solutions: " + numberOfsolutions;
    }

    public void showNextEquationSystem()
    {
        if (this.currentEquationSytem + 1 < equationsList.Count)
        {
            this.currentEquationSytem++;

            CurrentEquationSystemText.GetComponentInChildren<TMP_Text>().text = "current Equation System:  "
                + (this.currentEquationSytem + 1);

            showEquationSystem(equationsList[this.currentEquationSytem], ARanksList[this.currentEquationSytem],
                               AbRanksList[this.currentEquationSytem], numbersOfsolutions[this.currentEquationSytem]);
        }        
    }

    public void showPreviousEquationSystem()
    {
        if (this.currentEquationSytem - 1 >= 0)
        {
            this.currentEquationSytem--;

            CurrentEquationSystemText.GetComponentInChildren<TMP_Text>().text = "current Equation System:  "
                + (this.currentEquationSytem + 1);

            showEquationSystem(equationsList[this.currentEquationSytem], ARanksList[this.currentEquationSytem],
                               AbRanksList[this.currentEquationSytem], numbersOfsolutions[this.currentEquationSytem]);
        }
    }




    public void Deactivate()
    {
        this.gameObject.SetActive(false);
    }
}
