using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ConeCogwheelGenerator : MonoBehaviour, CogwheelGenerator
{
    private Mesh mesh;
    private float angle = 360.0f;
    private float bottomClearanceFactor = 0.1f;

    private float height;
    private int cogCount;

    private float pitchDiameter;
    private float outsideDiameter;
    private float insideDiameter;
    private float cogTopHeight;
    private float cogBaseHeight;
    private float cogHeight;
    private float module;
    private float bottomClearance;

    private float cogAngle;
    private float radiusAdjustment;

    //Only needed for involute-toothing:
    //private float pressureAngle;
    //private float baseDiameter;

    //Default: Every 0.5° of the circle segment, there starts a new triangle
    private float angleAccuracy = 0.5f;

    public void setCogwheelValues(float height, int cogCount, float radius)
    {
        if (height <= 0 || cogCount <= 1 || radius <= 0)
        {
            Debug.Log("ConeCogwheel-Generation: Height, cogCount or radius is invalid!");
            return;
        }

        this.height = height;
        this.pitchDiameter = radius * 2;
        this.cogCount = cogCount;
        this.module = pitchDiameter / cogCount;
        this.bottomClearance = bottomClearanceFactor * this.module;

        this.cogTopHeight = this.module;
        this.cogBaseHeight = this.module + this.bottomClearance;
        this.cogHeight = this.cogTopHeight + this.cogBaseHeight;
        this.outsideDiameter = this.pitchDiameter + 2 * this.cogTopHeight;
        this.insideDiameter = this.pitchDiameter - 2 * this.cogBaseHeight;

        this.cogAngle = this.angle / this.cogCount;
        this.angleAccuracy = this.cogAngle / 80;
        this.radiusAdjustment = this.cogHeight / (80/4);

        CreateConeCogwheel();
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

        posAngle = absoluteAngle;
        negAngle = 0;

        float changingRadius = radius; 

        //Draw cogs
        for (float x = negAngle; x < posAngle; x += angleAccuracy)
        {
            float nextAngle;
            int times;

            if (x % angleAccuracy != 0.0f)
            {
                times = (int) Math.Round((double)(x / angleAccuracy), MidpointRounding.ToEven);
                x = angleAccuracy * (float)times;
            }

            if (x + angleAccuracy > posAngle)
                nextAngle = posAngle;
            else
                nextAngle = x + angleAccuracy;

            float circlePointX = radius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
            float circlePointZ = radius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);
            
            float remainder = x % this.cogAngle;

            if (remainder >= 0 && remainder < (this.cogAngle/8)){/*Nothing to do*/}
            else if (remainder >= (this.cogAngle/8) && remainder < (3 * this.cogAngle/8))
            {
                //Ascending edge of cone
                changingRadius += radiusAdjustment;

                //If start of cog
                if (remainder == (this.cogAngle / 8))
                {
                    changingRadius = radius + radiusAdjustment;
                    float firstPointX = radius * Mathf.Cos(x * Mathf.Deg2Rad);
                    float firstPointZ = radius * Mathf.Sin(x * Mathf.Deg2Rad);
                    verticeList.Add(new Vector3(firstPointX, 0, firstPointZ));
                    verticeList.Add(new Vector3(firstPointX, height, firstPointZ));

                    verticeList.Add(new Vector3(circlePointX, 0, circlePointZ));
                    verticeList.Add(new Vector3(circlePointX, height, circlePointZ));

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
                else {
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
            else if (remainder >= (3 * this.cogAngle/8) && remainder < (5 * this.cogAngle/8))
            {
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
            else if (remainder >= (5 * this.cogAngle/8) && remainder < (7 * this.cogAngle/8))
            {
                //Descending edge of cone
                changingRadius -= radiusAdjustment;

                //If end of cog
                if (remainder == ((7 * this.cogAngle/8) - this.angleAccuracy))
                {
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
            else {/*Nothing to do*/}
        }

        mesh = new Mesh();
        mesh.vertices = verticeList.ToArray();
        mesh.triangles = triangleList.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateNormals();
    }
}
