using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateGenerator : MonoBehaviour, CreateModel
{
    public GameObject generatorPrefab;
    public InputField heightInput;

    public void setGeneratorPrefab(GameObject prefab)
    {
        this.generatorPrefab = prefab;
    }

    public void createGenerator()
    {
        float height;

        if (float.TryParse(heightInput.text, out height) && height >= 10.0f)
        {
            GameObject generator = Instantiate(generatorPrefab);
            generator.gameObject.layer = LayerMask.NameToLayer("CurrentlyEdited");

            //Adjust generator-GameObject, so that height of the stand is correct
            Transform rotatingPart = generator.transform.GetChild(0);
            Transform standPart = generator.transform.GetChild(1);
            rotatingPart.position = new Vector3(rotatingPart.position.x, height, rotatingPart.position.z);
            standPart.position = new Vector3(standPart.position.x, height/2.0f, standPart.position.z);
            standPart.localScale = new Vector3(standPart.localScale.x, height, standPart.localScale.z);

            CommunicationEvents.positionGeneratorEvent.Invoke(generator);
        }
        else
            Debug.Log("Input for height could not be parsed correctly OR height isn't >= 10!");
    }
}
