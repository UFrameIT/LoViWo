using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePanel : MonoBehaviour
{
    public Canvas parentCanvas;
    public GameObject prefab;
    public string panelTitle;
    public GameObject generatorPrefab;

    public void instantiatePrefab() {
        CommunicationEvents.openPanelEvent.Invoke();
        GameObject panel = Instantiate(prefab);
        Canvas canvas;
        if (parentCanvas != null)
            canvas = parentCanvas;
        else
            canvas = GameObject.Find("ModelsCanvas").GetComponent<Canvas>();

        panel.transform.SetParent(canvas.transform);
        panel.GetComponentInChildren<DragWindow>().canvas = canvas;
        panel.transform.localScale = new Vector3(1, 1, 1);
        panel.transform.localPosition = new Vector3(-350,0,0);
        panel.GetComponentInChildren<PanelEvents>().setTitle(panelTitle);
        CreateModel model = panel.GetComponentInChildren<CreateModel>();
        if (model != null)
            model.setGeneratorPrefab(generatorPrefab);
    }
}
