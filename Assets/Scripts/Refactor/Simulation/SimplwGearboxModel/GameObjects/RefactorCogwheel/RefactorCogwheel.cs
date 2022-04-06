using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RefactorCogwheel : MonoBehaviour, Cogwheel
{
    public Material cogMaterial;

    private SimulatedObject simulatedObject;

    private Mesh mesh;
    private float angle = 360.0f;
    private float bottomClearanceFactor = 0.1f;

    private float height;
    private int cogCount;

    private float radius;
    private float pitchDiameter;
    private float outsideDiameter;
    private float insideDiameter;
    private float cogTopHeight;
    private float cogBaseHeight;
    private float cogHeight;
    public float module;
    private float bottomClearance;

    public float cogAngle;
    private float radiusAdjustment;

    //Only needed for involute-toothing:
    //private float pressureAngle;
    //private float baseDiameter;

    //Default: Every 0.5° of the circle segment, there starts a new triangle
    private float angleAccuracy = 0.5f;

    private List<Vector3> relativeCogInputVectors = new List<Vector3>();

    //-> THE RELATIVE COG INPUT VECTORS ALWAYS POINT TO THE MIDDLE OF A COG, RELATIVE TO THE POSITION OF THE COGWHEEL
    public void setSimulatedObject(SimulatedObject simObj)
    {
        this.simulatedObject = simObj;
    }

    public SimulatedObject getSimulatedObject()
    {
        return this.simulatedObject;
    }

    public List<Vector3> getRelativeCogInputVectors()
    {
        return relativeCogInputVectors;
    }

    public float getHeight()
    {
        return this.height;
    }

    public float getModule()
    {
        return this.module;
    }

    public float getCogAngle()
    {
        return this.cogAngle;
    }

    public int getCogCount()
    {
        return this.cogCount;
    }

    public float getPitchDiameter()
    {
        return this.pitchDiameter;
    }

    public float getRadius()
    {
        return this.radius;
    }

    public float getInsideRadius()
    {
        return this.insideDiameter / 2.0f;
    }

    public float getOutsideRadius()
    {
        return this.outsideDiameter / 2.0f;
    }

    public void generateMesh(float height, int cogCount, float radius)
    {
        if (height <= 0 || cogCount <= 1 || radius <= 0)
        {
            Debug.Log("ConeCogwheel-Generation: Height, cogCount or radius is invalid!");
            return;
        }

        this.height = height;
        this.radius = radius;
        this.pitchDiameter = radius * 2;
        this.cogCount = cogCount;
        this.module = pitchDiameter / cogCount;
        this.bottomClearance = bottomClearanceFactor * this.module;

        this.cogTopHeight = this.module;
        this.cogBaseHeight = this.module + this.bottomClearance;
        this.cogHeight = this.cogTopHeight + this.cogBaseHeight;
        this.outsideDiameter = this.pitchDiameter + 2 * this.cogTopHeight;
        this.insideDiameter = this.pitchDiameter - 2 * this.cogBaseHeight;

        this.cogAngle = this.angle / (float)this.cogCount;
        //We define the angleAccuracy in such a way, that 80 steps in the for-loop result in one cog
        this.angleAccuracy = this.cogAngle / 80.0f;
        //One Cog consists of 1/8 starting-gap, 2/8 rising-edge, 2/8 constant-edge, 2/8 falling-edge, 1/8 ending-gap
        //Therefore in 80/4 = 20 steps, the radius for the cog must rise from radius to radius+cogHeight
        this.radiusAdjustment = this.cogHeight / (float)(80 / 4);

        calculateRelativeCogInputVectors();
        CreateConeCogwheel();

        this.GetComponentInChildren<OutsideRadiusCollider>().setRadiusAndHeight(this.outsideDiameter / 2.0f, height);
        this.GetComponentInChildren<InsideRadiusCollider>().setRadiusAndHeight(this.insideDiameter / 2.0f, height);
    }

    private void calculateRelativeCogInputVectors()
    {
        float radius = this.pitchDiameter / 2;

        //At x * cogAngle degrees (with x ∈ ℕ0), there's always the middle of a gap
        //So at (x + 0.5) * cogAngle degrees (with x ∈ ℕ0), there's always the middle of a cog
        //-> THE RELATIVE COG INPUT VECTORS ALWAYS POINT TO THE MIDDLE OF A COG, RELATIVE TO THE POSITION OF THE COGWHEEL
        for (float i = 0.5f; i < this.cogCount; i += 1f)
        {
            float currentAngle = i * this.cogAngle;
            float pointX = radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float pointZ = radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector3 relativeVector = new Vector3(pointX, 0, pointZ);
            relativeCogInputVectors.Add(relativeVector);
        }
    }

    private void CreateConeCogwheel()
    {
        float absoluteAngle = Mathf.Abs(angle);
        List<Vector3> verticeList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        //Center-Point of lower side
        Vector3 center0 = new Vector3(0, 0, 0);
        int center0Index = 0;
        verticeList.Add(center0);

        //Center-Point of upper side
        Vector3 center1 = new Vector3(0, height, 0);
        int center1Index = 1;
        verticeList.Add(center1);

        float posAngle = absoluteAngle / 2;
        float negAngle = posAngle * -1;

        float radius = insideDiameter / 2;

        int i = 2;
        //Draw cylinder
        for (float x = negAngle; x < posAngle; x += angleAccuracy)
        {
            float nextAngle;

            if (x + angleAccuracy > posAngle)
                nextAngle = posAngle;
            else
                nextAngle = x + angleAccuracy;

            float newPointX = radius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
            float newPointZ = radius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);

            if (i == 2)
            {
                //Add first Points at the beginning of the angle
                float firstPointX = radius * Mathf.Cos(negAngle * Mathf.Deg2Rad);
                float firstPointZ = radius * Mathf.Sin(negAngle * Mathf.Deg2Rad);
                verticeList.Add(new Vector3(firstPointX, 0, firstPointZ));
                verticeList.Add(new Vector3(firstPointX, height, firstPointZ));

                //Adding triangles for left side
                if (absoluteAngle != 360)
                {
                    triangleList.Add(center0Index);
                    triangleList.Add(center1Index);
                    triangleList.Add(i + 1);
                    triangleList.Add(center0Index);
                    triangleList.Add(i + 1);
                    triangleList.Add(i);
                }

                i += 2;
            }

            verticeList.Add(new Vector3(newPointX, 0, newPointZ));
            verticeList.Add(new Vector3(newPointX, height, newPointZ));

            //Adding triangles for upper- and lower-side
            triangleList.Add(center0Index);
            triangleList.Add(i - 2);
            triangleList.Add(i);
            triangleList.Add(center1Index);
            triangleList.Add(i + 1);
            triangleList.Add(i - 1);
            //Adding triangles for front side
            triangleList.Add(i - 2);
            triangleList.Add(i - 1);
            triangleList.Add(i + 1);
            triangleList.Add(i - 2);
            triangleList.Add(i + 1);
            triangleList.Add(i);

            if (nextAngle == posAngle && absoluteAngle != 360)
            {
                //Adding triangles for right side
                triangleList.Add(center0Index);
                triangleList.Add(i + 1);
                triangleList.Add(center1Index);
                triangleList.Add(center0Index);
                triangleList.Add(i);
                triangleList.Add(i + 1);
            }

            i += 2;
        }

        //Create cylinder-mesh
        mesh = new Mesh();
        mesh.vertices = verticeList.ToArray();
        mesh.triangles = triangleList.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();

        posAngle = absoluteAngle;
        negAngle = 0;

        float changingRadius = radius;

        //Only set this, so that there are no errors because newCog isn't instantiated
        //-> newCog will be a new GameObject() at the start of every cog
        GameObject newCog = this.gameObject;

        //These bools are only additional security-mechanisms to avoid accidential entrance into if-/else-cases because of floating-point-arithmetic-inaccuracies
        bool addedCog = false;
        bool newCogEntry = true;
        bool cogHeightRadiuLeveled = false;
        bool cogStartDone = false;
        bool cogEndDone = false;

        //Draw cogs
        for (float x = negAngle; x < posAngle; x += angleAccuracy)
        {
            float nextAngle;
            int times;

            //Leveling x because of floating-point-arithmetic inaccuracies
            if (x % angleAccuracy != 0.0f)
            {
                times = (int)Math.Round((double)(x / angleAccuracy), MidpointRounding.ToEven);
                x = angleAccuracy * (float)times;
            }

            if (x + angleAccuracy > posAngle)
                nextAngle = posAngle;
            else
                nextAngle = x + angleAccuracy;

            float circlePointX = radius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
            float circlePointZ = radius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);

            float remainder = x % this.cogAngle;

            //Leveling remainder because of floating-point-arithmetic inaccuracies
            if (remainder % angleAccuracy != 0.0f)
            {
                times = (int)Math.Round((double)(remainder / angleAccuracy), MidpointRounding.ToEven);
                remainder = angleAccuracy * (float)times;
            }

            //Every cog has 8 (respectively 5) parts: 1/8 for the starting gap, 2/8 for the rising edge,
            //2/8 for the constant edge, 2/8 for the falling edge and 1/8 for the ending gap
            //So at x * cogAngle degrees (with x ∈ ℕ), there's always the middle of a gap
            if (remainder >= 0 && remainder < (this.cogAngle / 8))
            {
                if (newCogEntry)
                {
                    newCogEntry = false;
                    addedCog = false;
                    cogHeightRadiuLeveled = false;

                    //New cog GameObject
                    i = 0;
                    newCog = new GameObject();
                    newCog.gameObject.AddComponent<MeshFilter>();
                    newCog.gameObject.AddComponent<MeshRenderer>();
                    newCog.gameObject.AddComponent<MeshCollider>();
                    verticeList = new List<Vector3>();
                    triangleList = new List<int>();
                }
            }
            else if (remainder >= (this.cogAngle / 8) && remainder < (3 * this.cogAngle / 8))
            {
                //Ascending edge of cone
                changingRadius += radiusAdjustment;

                //If start of cog
                if (remainder >= (this.cogAngle / 8) && remainder < ((this.cogAngle / 8) + this.angleAccuracy) && !cogStartDone)
                {
                    cogStartDone = true;

                    changingRadius = radius + radiusAdjustment;
                    float firstPointX = radius * Mathf.Cos(x * Mathf.Deg2Rad);
                    float firstPointZ = radius * Mathf.Sin(x * Mathf.Deg2Rad);
                    verticeList.Add(new Vector3(firstPointX, 0, firstPointZ));
                    verticeList.Add(new Vector3(firstPointX, height, firstPointZ));

                    verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                    verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

                    //The conePoint has to be on the same angle as circlePoint, but with radius = radius + radiusAdjustment == changingRadius
                    float conePointX = changingRadius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
                    float conePointZ = changingRadius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);
                    verticeList.Add(new Vector3(conePointX, 0, conePointZ));
                    verticeList.Add(new Vector3(conePointX, height, conePointZ));

                    //Adding triangles for front side
                    triangleList.Add(i);
                    triangleList.Add(i + 1);
                    triangleList.Add(i + 5);
                    triangleList.Add(i);
                    triangleList.Add(i + 5);
                    triangleList.Add(i + 4);
                    //Adding triangle for lower side
                    triangleList.Add(i);
                    triangleList.Add(i + 4);
                    triangleList.Add(i + 2);
                    //Adding triangle for upper side
                    triangleList.Add(i + 1);
                    triangleList.Add(i + 3);
                    triangleList.Add(i + 5);

                    i += 6;
                }
                else
                {
                    verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                    verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

                    float conePointX = changingRadius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
                    float conePointZ = changingRadius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);
                    verticeList.Add(new Vector3(conePointX, 0, conePointZ));
                    verticeList.Add(new Vector3(conePointX, height, conePointZ));

                    //Adding triangles for front side
                    triangleList.Add(i - 2);
                    triangleList.Add(i - 1);
                    triangleList.Add(i + 3);
                    triangleList.Add(i - 2);
                    triangleList.Add(i + 3);
                    triangleList.Add(i + 2);
                    //Adding triangles for lower side
                    triangleList.Add(i - 4);
                    triangleList.Add(i - 2);
                    triangleList.Add(i + 2);
                    triangleList.Add(i - 4);
                    triangleList.Add(i + 2);
                    triangleList.Add(i);
                    //Adding triangles for upper side
                    triangleList.Add(i - 3);
                    triangleList.Add(i + 3);
                    triangleList.Add(i - 1);
                    triangleList.Add(i - 3);
                    triangleList.Add(i + 1);
                    triangleList.Add(i + 3);

                    i += 4;
                }
            }
            else if (remainder >= (3 * this.cogAngle / 8) && remainder < (5 * this.cogAngle / 8))
            {
                if (!cogHeightRadiuLeveled)
                {
                    cogHeightRadiuLeveled = true;
                    changingRadius = radius + this.cogHeight;
                }

                //Constand edge of cone
                float outsideRadius = outsideDiameter / 2;

                verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

                float conePointX = outsideRadius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
                float conePointZ = outsideRadius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);
                verticeList.Add(new Vector3(conePointX, 0, conePointZ));
                verticeList.Add(new Vector3(conePointX, height, conePointZ));

                //Adding triangles for front side
                triangleList.Add(i - 2);
                triangleList.Add(i - 1);
                triangleList.Add(i + 3);
                triangleList.Add(i - 2);
                triangleList.Add(i + 3);
                triangleList.Add(i + 2);
                //Adding triangles for lower side
                triangleList.Add(i - 4);
                triangleList.Add(i - 2);
                triangleList.Add(i + 2);
                triangleList.Add(i - 4);
                triangleList.Add(i + 2);
                triangleList.Add(i);
                //Adding triangles for upper side
                triangleList.Add(i - 3);
                triangleList.Add(i + 3);
                triangleList.Add(i - 1);
                triangleList.Add(i - 3);
                triangleList.Add(i + 1);
                triangleList.Add(i + 3);

                i += 4;
            }
            else if (remainder >= (5 * this.cogAngle / 8) && remainder < (7 * this.cogAngle / 8))
            {
                //Descending edge of cone
                changingRadius -= radiusAdjustment;

                //If end of cog
                if (remainder >= ((7 * this.cogAngle / 8) - this.angleAccuracy) && remainder < (7 * this.cogAngle / 8) && !cogEndDone)
                {
                    cogEndDone = true;

                    verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                    verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

                    //Adding triangles for front side
                    triangleList.Add(i - 1);
                    triangleList.Add(i + 1);
                    triangleList.Add(i);
                    triangleList.Add(i);
                    triangleList.Add(i - 2);
                    triangleList.Add(i - 1);
                    //Adding triangle for lower side
                    triangleList.Add(i);
                    triangleList.Add(i - 4);
                    triangleList.Add(i - 2);
                    //Adding triangle for upper side
                    triangleList.Add(i - 1);
                    triangleList.Add(i - 3);
                    triangleList.Add(i + 1);

                    i += 2;
                }
                else
                {
                    verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                    verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

                    float conePointX = changingRadius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
                    float conePointZ = changingRadius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);
                    verticeList.Add(new Vector3(conePointX, 0, conePointZ));
                    verticeList.Add(new Vector3(conePointX, height, conePointZ));

                    //Adding triangles for front side
                    triangleList.Add(i - 2);
                    triangleList.Add(i - 1);
                    triangleList.Add(i + 3);
                    triangleList.Add(i - 2);
                    triangleList.Add(i + 3);
                    triangleList.Add(i + 2);
                    //Adding triangles for lower side
                    triangleList.Add(i - 4);
                    triangleList.Add(i - 2);
                    triangleList.Add(i + 2);
                    triangleList.Add(i - 4);
                    triangleList.Add(i + 2);
                    triangleList.Add(i);
                    //Adding triangles for upper side
                    triangleList.Add(i - 3);
                    triangleList.Add(i + 3);
                    triangleList.Add(i - 1);
                    triangleList.Add(i - 3);
                    triangleList.Add(i + 1);
                    triangleList.Add(i + 3);

                    i += 4;
                }
            }
            else
            {
                if (!addedCog)
                {
                    addedCog = true;
                    newCogEntry = true;
                    cogStartDone = false;
                    cogEndDone = false;

                    //New Cog as child-GameObject of cylinder
                    mesh = new Mesh();
                    mesh.vertices = verticeList.ToArray();
                    mesh.triangles = triangleList.ToArray();
                    newCog.GetComponent<MeshFilter>().mesh = mesh;
                    newCog.GetComponent<MeshCollider>().sharedMesh = mesh;
                    newCog.GetComponent<MeshCollider>().convex = true;
                    newCog.GetComponent<MeshRenderer>().material = cogMaterial;
                    mesh.RecalculateNormals();
                    newCog.transform.parent = this.gameObject.transform;
                    newCog.transform.localPosition = new Vector3(0, 0, 0);
                    newCog.transform.localEulerAngles = new Vector3(0, 0, 0);
                }
            }
        }

        /* Use this to save the Mesh, created from Mesh API
        UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/Resources/Prefabs/Models/GeneratorCogwheel.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        */
    }


    /**
     * 
     */
    public Tuple<List<RefactorCogwheel>, List<RefactorCogwheel>> getIntersectingAndInterlockingCogwheels()
    {
        Collider thisCollider = this.GetComponentInChildren<OutsideRadiusCollider>().GetComponent<MeshCollider>();

        //get all Colliders in "SimulatedObjects" layer within a bounding box within the dimensions of the Cogwheel
        Collider[] intersectingColliders = Physics.OverlapBox(this.transform.position,
                                                  new Vector3(this.pitchDiameter / 2.0f, this.height / 2.0f, this.pitchDiameter / 2.0f),
                                                  this.transform.rotation,
                                                  LayerMask.GetMask("SimulatedObjects"));
        //select the inside and outside colliders of the Cogwheels that are not this Cogwheel
        List<Collider> sectColsBox = intersectingColliders.Where(c => c.gameObject.GetComponent<InsideRadiusCollider>() 
                                                                   && c.gameObject.transform.parent.gameObject != this.gameObject).ToList();
        List<Collider> lockColsBox = intersectingColliders.Where(c => c.gameObject.GetComponent<OutsideRadiusCollider>()
                                                                   && c.gameObject.transform.parent.gameObject != this.gameObject).ToList();
        //select those colliders that are actually intersecting with other cogwheels insideColliders
        List<Collider> sectCols = new List<Collider>();
        foreach (Collider c in sectColsBox)
        {
            Vector3 v = new Vector3();
            float f;
            bool colliding = Physics.ComputePenetration(thisCollider, this.transform.position, this.transform.rotation,
                                                        c, c.gameObject.transform.position, c.gameObject.transform.rotation, out v, out f);
            if (colliding)
            {
                sectCols.Add(c);
            }
        }
        //select those colliders that are actually intersecting with other cogwheels outsideColliders
        List<Collider> lockCols = new List<Collider>();
        foreach (Collider c in lockColsBox)
        {
            Vector3 v = new Vector3();
            float f;
            bool colliding = Physics.ComputePenetration(thisCollider, this.transform.position, this.transform.rotation,
                                                        c, c.gameObject.transform.position, c.gameObject.transform.rotation, out v, out f);
            if (colliding)
            {
                lockCols.Add(c);
            }
        }
        //get Cogwheels associated with the colliders
        List<RefactorCogwheel> intersecting = sectCols.Select(c => c.gameObject.GetComponent<InsideRadiusCollider>().transform.parent.GetComponent<RefactorCogwheel>()).ToList();
        List<RefactorCogwheel> interlocking = lockCols.Select(c => c.gameObject.GetComponent<OutsideRadiusCollider>().transform.parent.GetComponent<RefactorCogwheel>()).ToList();

        return new Tuple<List<RefactorCogwheel>, List<RefactorCogwheel>>(intersecting, interlocking);
    }

}
