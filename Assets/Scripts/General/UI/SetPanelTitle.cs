using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanelTitle : MonoBehaviour
{
    public Text panelTitle;

    public void setTitle(string title) {
        panelTitle.text = title;
    }
}
