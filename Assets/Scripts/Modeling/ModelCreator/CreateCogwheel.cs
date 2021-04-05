using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateCogwheel : MonoBehaviour, CreateModel
{
    public GameObject cogwheelGeneratorPrefab;
    public InputField radiusInput;
    public InputField cogCountInput;
    public InputField heightInput;

    public void setGeneratorPrefab(GameObject prefab) {
        this.cogwheelGeneratorPrefab = prefab;
    }

    public void createCogwheel() {
        float height;
        int cogCount;
        float radius;

        if (float.TryParse(radiusInput.text, out radius) &&
            int.TryParse(cogCountInput.text, out cogCount) &&
            float.TryParse(heightInput.text, out height))
        {
            GameObject cogwheel = Instantiate(cogwheelGeneratorPrefab);
            cogwheel.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");
            cogwheel.GetComponentInChildren<Cogwheel>().generateMesh(height, cogCount, radius);

            CommunicationEvents.positionCogwheelEvent.Invoke(cogwheel);
        }
        else
            Debug.Log("Inputs for height, cogCount or radius could not be parsed correctly!");
    }
}
