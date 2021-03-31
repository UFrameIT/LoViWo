using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateCogwheel : MonoBehaviour
{
    public GameObject cogwheelGeneratorPrefab;
    public InputField radiusInput;
    public InputField cogCountInput;
    public InputField heightInput;

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

            //TODO: Remove
            cogwheel.transform.position = new Vector3(20, (float)(radius * 2 + 0.5), 0);
            CommunicationEvents.positionCogwheelEvent.Invoke(cogwheel);
        }
        else
            Debug.Log("Inputs for height, cogCount or radius could not be parsed correctly!");
    }
}
