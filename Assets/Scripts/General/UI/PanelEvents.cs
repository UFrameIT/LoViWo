using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static CommunicationEvents;

public class PanelEvents : MonoBehaviour
{
    public GameObject window;
    public Text panelTitle;

    private EventSystem system;

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
        CommunicationEvents.closeUIEvent.AddListener(DeleteWindow);
        CommunicationEvents.openPanelEvent.AddListener(DeleteWindow);
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }

        }
    }

    public void setTitle(string title)
    {
        panelTitle.text = title;
    }

    public void DeleteWindow() {
        Destroy(window);
    }
}
