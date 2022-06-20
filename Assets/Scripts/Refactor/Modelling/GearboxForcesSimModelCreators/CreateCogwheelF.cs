using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateCogwheelF : MonoBehaviour, CreateModel
{
    public GameObject cogwheelGeneratorPrefab;
    public InputField radiusInput;
    public InputField cogCountInput;
    public InputField heightInput;
    public InputField frictionInput;

    public void setGeneratorPrefab(GameObject prefab) {
        this.cogwheelGeneratorPrefab = prefab;
    }

    public void createCogwheel() {
        float height;
        int cogCount;
        float radius;
        float friction;

        if (float.TryParse(radiusInput.text, out radius) &&
            int.TryParse(cogCountInput.text, out cogCount) &&
            float.TryParse(heightInput.text, out height) &&
            float.TryParse(frictionInput.text, out friction))
        {
            GameObject cogwheel = Instantiate(cogwheelGeneratorPrefab);
            cogwheel.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");
            cogwheel.GetComponentInChildren<Cogwheel>().generateMesh(height, cogCount, radius);
            //Set Layer for all children
            foreach (Transform t in cogwheel.GetComponentsInChildren<Transform>()) {
                t.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");
            }

            Debug.Log("ivoke positionCogwheelFEvent");
            CommunicationEvents.positionCogwheelFEvent.Invoke(cogwheel, friction);
        }
        else
            Debug.Log("Inputs for height, cogCount or radius could not be parsed correctly!");
    }
}
