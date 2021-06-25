using System.Collections.Generic;
using UnityEngine;
using JsonSubTypes;
using Newtonsoft.Json;

/*
 * For all SceneObjects we need the information the CreateModel's AND the MoveTool's use
 */
[JsonConverter(typeof(JsonSubtypes), "kind")]
[JsonSubtypes.KnownSubType(typeof(ConeCogwheelObject), "ConeCogwheelObject")]
[JsonSubtypes.KnownSubType(typeof(GeneratorObject), "GeneratorObject")]
[JsonSubtypes.KnownSubType(typeof(ShaftObject), "ShaftObject")]
[JsonSubtypes.KnownSubType(typeof(ShaftHolderObject), "ShaftHolderObject")]
public abstract class SceneObject
{
    //id is needed for interlocking-/connected-Objects
    public int id;
    public string kind;
    public Vector3Object position;
    public Vector3Object rotation;

    public static List<SceneObject> FromJSON(string json)
    {
        List<SceneObject> sceneObjects = JsonConvert.DeserializeObject<List<SceneObject>>(json);
        return sceneObjects;
    }

    public static string ToJSON(List<SceneObject> objects) {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(objects);
        return json;
    }
}

public class ConeCogwheelObject : SceneObject
{
    public float radius;
    public int cogCount;
    public float height;
    public int[] interlockingObjects;
}

public class GeneratorObject : SceneObject
{
    public float height;
    public int[] connectedObjects;
}

public class ShaftObject : SceneObject
{
    public float radius;
    public float length;
    public int[] connectedObjects;
}

public class ShaftHolderObject : SceneObject
{
    public float radius;
    public float thickness;
}

public class Vector3Object
{
    public float x;
    public float y;
    public float z;

    public Vector3Object(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
