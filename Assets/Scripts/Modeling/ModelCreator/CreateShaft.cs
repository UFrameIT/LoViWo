using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateShaft : MonoBehaviour, CreateModel
{
    public GameObject shaftPrefab;
    public InputField radiusInput;
    public InputField lengthInput;

    public void setGeneratorPrefab(GameObject prefab) {
        this.shaftPrefab = prefab;
    }

    public void createShaft() {
        float radius;
        float length;

        if (float.TryParse(radiusInput.text, out radius) &&
            float.TryParse(lengthInput.text, out length))
        {
            float diameter = radius * 2.0f;

            GameObject shaft = Instantiate(shaftPrefab);
            shaft.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");

            shaft.transform.localScale = new Vector3(diameter, length, diameter);

            CommunicationEvents.positionShaftEvent.Invoke(shaft);
        }
        else
            Debug.Log("Inputs for radius or length could not be parsed correctly!");
    }
}
