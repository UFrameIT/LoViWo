using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateShaftHolder : MonoBehaviour, CreateModel
{
    public GameObject shaftHolderPrefab;
    public InputField radiusInput;
    public InputField thicknessInput;

    public void setGeneratorPrefab(GameObject prefab) {
        this.shaftHolderPrefab = prefab;
    }

    public void createShaftHolder() {
        float radius;
        float thickness;

        if (float.TryParse(radiusInput.text, out radius) &&
            float.TryParse(thicknessInput.text, out thickness))
        {
            float diameter = radius * 2.0f;
            float shaftHolderBorderWidth = 1.0f;

            GameObject shaftHolder = Instantiate(shaftHolderPrefab);
            shaftHolder.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");
            //After the generateMesh-Call, the head of the shaftHolder has the following properties: width = (2*radius + 2*borderWidth), height = (2*radius + 2*borderWidth), depth = thickness
            shaftHolder.GetComponentInChildren<ShaftHolder>().generateMesh(radius, thickness);

            Transform stand = shaftHolder.transform.GetChild(0);
            Transform bottom = shaftHolder.transform.GetChild(1);

            //width = z, height = y, thickness = x
            stand.localScale = new Vector3(stand.localScale.x * thickness, stand.localScale.y * radius, 2 * radius + 2 * shaftHolderBorderWidth);
            bottom.localScale = new Vector3(bottom.localScale.x * radius, bottom.localScale.y * thickness, stand.localScale.z * 2);
            stand.localPosition = new Vector3((-(stand.localScale.y/2 + shaftHolderBorderWidth + radius)), stand.localPosition.y * thickness, stand.localPosition.z);
            bottom.localPosition = new Vector3((-(shaftHolderBorderWidth + radius + stand.localScale.y - bottom.localScale.x/2)), bottom.localPosition.y * thickness, bottom.localPosition.z);

            CommunicationEvents.positionShaftHolderEvent.Invoke(shaftHolder);
        }
        else
            Debug.Log("Inputs for radius or thickness could not be parsed correctly!");
    }
}
