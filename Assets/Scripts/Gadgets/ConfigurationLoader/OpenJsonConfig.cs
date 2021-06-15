using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneLoaderDictionary {
    public static Dictionary<Type, Action<SceneObject>> loadSceneObjectDictionary = new Dictionary<Type, Action<SceneObject>>() {
        {typeof(ConeCogwheelObject), OpenJsonConfig.loadConeCogwheelObject},
        {typeof(GeneratorObject), OpenJsonConfig.loadGeneratorObject},
        {typeof(ShaftObject), OpenJsonConfig.loadShaftObject},
        {typeof(ShaftHolderObject), OpenJsonConfig.loadShaftHolderObject}
    };
}

public class OpenJsonConfig : MonoBehaviour
{
    public static Dictionary<Type, GameObject> prefabDictionary;
    public static List<Tuple<SceneObject, GameObject>> addedSceneObjects;

    public GameObject prefabCogwheel;
    public GameObject prefabGenerator;
    public GameObject prefabShaft;
    public GameObject prefabShaftHolder;

    void Start()
    {
        prefabDictionary = new Dictionary<Type, GameObject>() {
            {typeof(ConeCogwheelObject), prefabCogwheel},
            {typeof(GeneratorObject), prefabGenerator},
            {typeof(ShaftObject), prefabShaft},
            {typeof(ShaftHolderObject), prefabShaftHolder}
        };
    }

    public void openJsonConfig()
    {
        addedSceneObjects = new List<Tuple<SceneObject, GameObject>>();

        string path = EditorUtility.OpenFilePanel("Load Json Config...", "Assets\\JsonConfigurations", "json");
        if (".json".Equals(Path.GetExtension(path)))
        {
            string json = File.ReadAllText(path);
            List<SceneObject> sceneObjects = SceneObject.FromJSON(json);

            for (int i = 0; i < sceneObjects.Count; i++) {
                SceneLoaderDictionary.loadSceneObjectDictionary[sceneObjects[i].GetType()].Invoke(sceneObjects[i]);
            }
        }
        else {
            Debug.Log("OpenJsonConfig: Selected file was not a json-file.");
        }
    }

    public static void loadConeCogwheelObject(SceneObject obj) {
        if (obj.GetType().Equals(typeof(ConeCogwheelObject))) {
            ConeCogwheelObject ccObject = (ConeCogwheelObject)obj;

            GameObject cogwheel = Instantiate(prefabDictionary[obj.GetType()]);
            cogwheel.GetComponentInChildren<Cogwheel>().generateMesh(ccObject.height, ccObject.cogCount, ccObject.radius);

            cogwheel.transform.position = new Vector3(ccObject.position.x, ccObject.position.y, ccObject.position.z);
            cogwheel.transform.eulerAngles = new Vector3(ccObject.rotation.x, ccObject.rotation.y, ccObject.rotation.z);

            foreach (int id in ccObject.interlockingObjects) {
                Tuple<SceneObject, GameObject> tuple = addedSceneObjects.Find(tp => tp.Item1.id.Equals(id));
                if (tuple != null) {
                    cogwheel.GetComponentInChildren<Interlockable>().addInterlockingPart(tuple.Item2.GetComponentInChildren<Interlockable>());
                }
            }

            //Set Layer for gameobject and all its children
            SetLayerRecursively(cogwheel, LayerMask.NameToLayer("Cogwheel"));

            //Create new CogwheelFact and add to global FactList
            int cogId = GameState.Facts.Count;
            float radius = cogwheel.GetComponentInChildren<Cogwheel>().getRadius();
            CogwheelFact newFact = new CogwheelFact(cogId, cogwheel.transform.position, cogwheel.transform.up, radius);
            newFact.Representation = cogwheel;
            GameState.Facts.Insert(cogId, newFact);
            UnityEngine.Debug.Log("Successfully added new CogwheelFact with backendUri: " + newFact.backendURI);

            addedSceneObjects.Add(new Tuple<SceneObject, GameObject>(obj, cogwheel));
        }
    }

    public static void loadGeneratorObject(SceneObject obj)
    {
        if (obj.GetType().Equals(typeof(GeneratorObject)))
        {
            GeneratorObject gObject = (GeneratorObject)obj;

            GameObject generator = Instantiate(prefabDictionary[obj.GetType()]);
            generator.transform.position = new Vector3(gObject.position.x, gObject.position.y, gObject.position.z);
            generator.transform.eulerAngles = new Vector3(gObject.rotation.x, gObject.rotation.y, gObject.rotation.z);

            Transform rotatingPart = generator.transform.GetChild(0);
            Transform standPart = generator.transform.GetChild(1);
            rotatingPart.position = new Vector3(rotatingPart.position.x, gObject.height, rotatingPart.position.z);
            standPart.position = new Vector3(standPart.position.x, gObject.height / 2.0f, standPart.position.z);
            standPart.localScale = new Vector3(standPart.localScale.x, gObject.height, standPart.localScale.z);

            foreach (int id in gObject.connectedObjects)
            {
                Tuple<SceneObject, GameObject> tuple = addedSceneObjects.Find(tp => tp.Item1.id.Equals(id));
                if (tuple != null)
                {
                    generator.GetComponentInChildren<Connectable>().addConnectedPart(tuple.Item2.GetComponentInChildren<Rotatable>());
                }
            }

            //Set Layer for gameobject and all its children
            SetLayerRecursively(generator, LayerMask.NameToLayer("Generator"));

            addedSceneObjects.Add(new Tuple<SceneObject, GameObject>(obj, generator));
        }
    }

    public static void loadShaftObject(SceneObject obj)
    {
        if (obj.GetType().Equals(typeof(ShaftObject)))
        {
            ShaftObject shObject = (ShaftObject)obj;

            GameObject shaft = Instantiate(prefabDictionary[obj.GetType()]);
            shaft.transform.position = new Vector3(shObject.position.x, shObject.position.y, shObject.position.z);
            shaft.transform.eulerAngles = new Vector3(shObject.rotation.x, shObject.rotation.y, shObject.rotation.z);

            float diameter = shObject.radius * 2.0f;
            shaft.transform.localScale = new Vector3(diameter, shObject.length, diameter);

            foreach (int id in shObject.connectedObjects)
            {
                Tuple<SceneObject, GameObject> tuple = addedSceneObjects.Find(tp => tp.Item1.id.Equals(id));
                if (tuple != null)
                {
                    shaft.GetComponentInChildren<Connectable>().addConnectedPart(tuple.Item2.GetComponentInChildren<Rotatable>());
                }
            }

            //Set Layer for gameobject and all its children
            SetLayerRecursively(shaft, LayerMask.NameToLayer("Shaft"));

            addedSceneObjects.Add(new Tuple<SceneObject, GameObject>(obj, shaft));
        }
    }

    public static void loadShaftHolderObject(SceneObject obj)
    {
        if (obj.GetType().Equals(typeof(ShaftHolderObject)))
        {
            ShaftHolderObject shhObject = (ShaftHolderObject)obj;

            GameObject shaftHolder = Instantiate(prefabDictionary[obj.GetType()]);

            float diameter = shhObject.radius * 2.0f;
            float shaftHolderBorderWidth = 1.0f;
            shaftHolder.transform.position = new Vector3(shhObject.position.x, shhObject.position.y, shhObject.position.z);
            shaftHolder.transform.eulerAngles = new Vector3(shhObject.rotation.x, shhObject.rotation.y, shhObject.rotation.z);
            //After the generateMesh-Call, the head of the shaftHolder has the following properties: width = (2*radius + 2*borderWidth), height = (2*radius + 2*borderWidth), depth = thickness
            shaftHolder.GetComponentInChildren<ShaftHolder>().generateMesh(shhObject.radius, shhObject.thickness);

            Transform stand = shaftHolder.transform.GetChild(0);
            Transform bottom = shaftHolder.transform.GetChild(1);

            //width = z, height = y, thickness = x
            stand.localScale = new Vector3(stand.localScale.x * shhObject.thickness, stand.localScale.y * shhObject.radius, 2 * shhObject.radius + 2 * shaftHolderBorderWidth);
            bottom.localScale = new Vector3(bottom.localScale.x * shhObject.radius, bottom.localScale.y * shhObject.thickness, stand.localScale.z * 2);
            stand.localPosition = new Vector3((-(stand.localScale.y / 2 + shaftHolderBorderWidth + shhObject.radius)), stand.localPosition.y * shhObject.thickness, stand.localPosition.z);
            bottom.localPosition = new Vector3((-(shaftHolderBorderWidth + shhObject.radius + stand.localScale.y - bottom.localScale.x / 2)), bottom.localPosition.y * shhObject.thickness, bottom.localPosition.z);

            float height = shaftHolder.transform.position.y;
            stand.localScale = new Vector3(stand.localScale.x, height - shhObject.radius - shaftHolderBorderWidth, stand.localScale.z);
            stand.localPosition = new Vector3(-(shaftHolder.transform.GetChild(0).localScale.y / 2 + shhObject.radius + shaftHolderBorderWidth), stand.localPosition.y, stand.localPosition.z);
            bottom.localPosition = new Vector3((-(shaftHolderBorderWidth + shhObject.radius + shaftHolder.transform.GetChild(0).localScale.y - shaftHolder.transform.GetChild(1).localScale.x / 2)), bottom.localPosition.y, bottom.localPosition.z);

            //Set Layer for gameobject and all its children
            SetLayerRecursively(shaftHolder, LayerMask.NameToLayer("ShaftHolder"));

            addedSceneObjects.Add(new Tuple<SceneObject, GameObject>(obj, shaftHolder));
        }
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
