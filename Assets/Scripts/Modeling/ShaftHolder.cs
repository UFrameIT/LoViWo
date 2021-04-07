using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ShaftHolder : MonoBehaviour
{
    public float innerRadius;
    public float thickness;

    private float borderWidth = 1.0f;
    private Mesh mesh;

    //Every 0.5° of the inner circle, there starts a new triangle
    private float angleAccuracy = 0.5f;

    public void generateMesh(float innerRadius, float thickness)
    {
        this.innerRadius = Mathf.Abs(innerRadius);
        this.thickness = Mathf.Abs(thickness);

        CreateShaftHolder();
    }

    public void CreateShaftHolder()
    {
        float halfSquareLength = this.innerRadius + this.borderWidth;

        float angle = 360.0f;
        List<Vector3> verticeList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        float posAngle = angle;
        float negAngle = 0;

        int verticeCounter = 0;

        //HINT Triangles: Gegen den Uhrzeigersinn für Unterseite
        //HINT Triangles: Mit dem Uhrzeigersinn für Oberseite

        //Adding Triangles independent of inner circle
        Vector3 upperRightCornerBase = new Vector3(halfSquareLength, 0, halfSquareLength);
        Vector3 upperRightCornerTop = new Vector3(halfSquareLength, thickness, halfSquareLength);
        Vector3 upperLeftCornerBase = new Vector3(-halfSquareLength, 0, halfSquareLength);
        Vector3 upperLeftCornerTop = new Vector3(-halfSquareLength, thickness, halfSquareLength);
        Vector3 lowerLeftCornerBase = new Vector3(-halfSquareLength, 0, -halfSquareLength);
        Vector3 lowerLeftCornerTop = new Vector3(-halfSquareLength, thickness, -halfSquareLength);
        Vector3 lowerRightCornerBase = new Vector3(halfSquareLength, 0, -halfSquareLength);
        Vector3 lowerRightCornerTop = new Vector3(halfSquareLength, thickness, -halfSquareLength);

        //Adding corner points
        verticeList.Add(upperRightCornerBase);
        int upperRightCornerBase_Index = verticeCounter++;
        verticeList.Add(upperRightCornerTop);
        int upperRightCornerTop_Index = verticeCounter++;
        verticeList.Add(upperLeftCornerBase);
        int upperLeftCornerBase_Index = verticeCounter++;
        verticeList.Add(upperLeftCornerTop);
        int upperLeftCornerTop_Index = verticeCounter++;
        verticeList.Add(lowerLeftCornerBase);
        int lowerLeftCornerBase_Index = verticeCounter++;
        verticeList.Add(lowerLeftCornerTop);
        int lowerLeftCornerTop_Index = verticeCounter++;
        verticeList.Add(lowerRightCornerBase);
        int lowerRightCornerBase_Index = verticeCounter++;
        verticeList.Add(lowerRightCornerTop);
        int lowerRightCornerTop_Index = verticeCounter++;

        //Circle points
        Vector3 circlePointBase_0_1 = new Vector3(0, 0, innerRadius);
        Vector3 circlePointTop_0_1 = new Vector3(0, thickness, innerRadius);
        Vector3 circlePointBase_m1_0 = new Vector3(-innerRadius, 0, 0);
        Vector3 circlePointTop_m1_0 = new Vector3(-innerRadius, thickness, 0);
        Vector3 circlePointBase_0_m1 = new Vector3(0, 0, -innerRadius);
        Vector3 circlePointTop_0_m1 = new Vector3(0, thickness, -innerRadius);
        Vector3 circlePointBase_1_0 = new Vector3(innerRadius, 0, 0);
        Vector3 circlePointTop_1_0 = new Vector3(innerRadius, thickness, 0);

        //Adding circle points
        verticeList.Add(circlePointBase_0_1);
        int circlePointBase_0_1_Index = verticeCounter++;
        verticeList.Add(circlePointTop_0_1);
        int circlePointTop_0_1_Index = verticeCounter++;
        verticeList.Add(circlePointBase_m1_0);
        int circlePointBase_m1_0_Index = verticeCounter++;
        verticeList.Add(circlePointTop_m1_0);
        int circlePointTop_m1_0_Index = verticeCounter++;
        verticeList.Add(circlePointBase_0_m1);
        int circlePointBase_0_m1_Index = verticeCounter++;
        verticeList.Add(circlePointTop_0_m1);
        int circlePointTop_0_m1_Index = verticeCounter++;
        verticeList.Add(circlePointBase_1_0);
        int circlePointBase_1_0_Index = verticeCounter++;
        verticeList.Add(circlePointTop_1_0);
        int circlePointTop_1_0_Index = verticeCounter++;
        
        //Adding triangles for top, back and front
        triangleList.Add(upperLeftCornerBase_Index);
        triangleList.Add(circlePointBase_0_1_Index);
        triangleList.Add(upperRightCornerBase_Index);
        triangleList.Add(upperRightCornerTop_Index);
        triangleList.Add(circlePointTop_0_1_Index);
        triangleList.Add(upperLeftCornerTop_Index);

        triangleList.Add(upperLeftCornerTop_Index);
        triangleList.Add(upperLeftCornerBase_Index);
        triangleList.Add(upperRightCornerTop_Index);
        triangleList.Add(upperLeftCornerBase_Index);
        triangleList.Add(upperRightCornerBase_Index);
        triangleList.Add(upperRightCornerTop_Index);

        triangleList.Add(lowerLeftCornerBase_Index);
        triangleList.Add(circlePointBase_m1_0_Index);
        triangleList.Add(upperLeftCornerBase_Index);
        triangleList.Add(upperLeftCornerTop_Index);
        triangleList.Add(circlePointTop_m1_0_Index);
        triangleList.Add(lowerLeftCornerTop_Index);

        triangleList.Add(lowerLeftCornerTop_Index);
        triangleList.Add(lowerLeftCornerBase_Index);
        triangleList.Add(upperLeftCornerTop_Index);
        triangleList.Add(lowerLeftCornerBase_Index);
        triangleList.Add(upperLeftCornerBase_Index);
        triangleList.Add(upperLeftCornerTop_Index);
        
        triangleList.Add(lowerRightCornerBase_Index);
        triangleList.Add(circlePointBase_0_m1_Index);
        triangleList.Add(lowerLeftCornerBase_Index);
        triangleList.Add(lowerLeftCornerTop_Index);
        triangleList.Add(circlePointTop_0_m1_Index);
        triangleList.Add(lowerRightCornerTop_Index);

        triangleList.Add(lowerRightCornerTop_Index);
        triangleList.Add(lowerLeftCornerBase_Index);
        triangleList.Add(lowerLeftCornerTop_Index);
        triangleList.Add(lowerRightCornerTop_Index);
        triangleList.Add(lowerRightCornerBase_Index);
        triangleList.Add(lowerLeftCornerBase_Index);

        triangleList.Add(upperRightCornerBase_Index);
        triangleList.Add(circlePointBase_1_0_Index);
        triangleList.Add(lowerRightCornerBase_Index);
        triangleList.Add(lowerRightCornerTop_Index);
        triangleList.Add(circlePointTop_1_0_Index);
        triangleList.Add(upperRightCornerTop_Index);

        triangleList.Add(upperRightCornerTop_Index);
        triangleList.Add(lowerRightCornerBase_Index);
        triangleList.Add(lowerRightCornerTop_Index);
        triangleList.Add(upperRightCornerTop_Index);
        triangleList.Add(upperRightCornerBase_Index);
        triangleList.Add(lowerRightCornerBase_Index);

        for (float x = negAngle; x < posAngle; x += angleAccuracy)
        {
            float nextAngle;
            int times;

            if (x % angleAccuracy != 0.0f)
            {
                times = (int)Math.Round((double)(x / angleAccuracy), MidpointRounding.ToEven);
                x = angleAccuracy * (float)times;
            }

            if (x + angleAccuracy > posAngle)
                nextAngle = posAngle;
            else
                nextAngle = x + angleAccuracy;

            float pointX = innerRadius * Mathf.Cos(x * Mathf.Deg2Rad);
            float pointZ = innerRadius * Mathf.Sin(x * Mathf.Deg2Rad);
            float nextPointX = innerRadius * Mathf.Cos(nextAngle * Mathf.Deg2Rad);
            float nextPointZ = innerRadius * Mathf.Sin(nextAngle * Mathf.Deg2Rad);

            //Upper-Right Quater
            if (0 <= x && x < 90.0f)
            {
                verticeList.Add(upperRightCornerBase);
                int cornerBaseIndex = verticeCounter++;
                verticeList.Add(upperRightCornerTop);
                int cornerTopIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, 0, pointZ));
                int circlePoint1BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, thickness, pointZ));
                int circlePoint1TopIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, 0, nextPointZ));
                int circlePoint2BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, thickness, nextPointZ));
                int circlePoint2TopIndex = verticeCounter++;

                //Add triangle for lower side
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(cornerBaseIndex);
                //Add triangle for upper side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(cornerTopIndex);

                //Add Triangles for inner side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(circlePoint1TopIndex);

            }
            //Upper-Left Quater
            else if (90.0f <= x && x < 180.0f)
            {
                verticeList.Add(upperLeftCornerBase);
                int cornerBaseIndex = verticeCounter++;
                verticeList.Add(upperLeftCornerTop);
                int cornerTopIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, 0, pointZ));
                int circlePoint1BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, thickness, pointZ));
                int circlePoint1TopIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, 0, nextPointZ));
                int circlePoint2BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, thickness, nextPointZ));
                int circlePoint2TopIndex = verticeCounter++;

                //Add triangle for lower side
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(cornerBaseIndex);
                //Add triangle for upper side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(cornerTopIndex);

                //Add Triangles for inner side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(circlePoint1TopIndex);
            }
            //Lower-Left Quater
            else if (180.0f <= x && x < 270.0f)
            {
                verticeList.Add(lowerLeftCornerBase);
                int cornerBaseIndex = verticeCounter++;
                verticeList.Add(lowerLeftCornerTop);
                int cornerTopIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, 0, pointZ));
                int circlePoint1BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, thickness, pointZ));
                int circlePoint1TopIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, 0, nextPointZ));
                int circlePoint2BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, thickness, nextPointZ));
                int circlePoint2TopIndex = verticeCounter++;

                //Add triangle for lower side
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(cornerBaseIndex);
                //Add triangle for upper side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(cornerTopIndex);

                //Add Triangles for inner side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(circlePoint1TopIndex);
            }
            //Lower-Right Quater
            else
            {
                verticeList.Add(lowerRightCornerBase);
                int cornerBaseIndex = verticeCounter++;
                verticeList.Add(lowerRightCornerTop);
                int cornerTopIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, 0, pointZ));
                int circlePoint1BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(pointX, thickness, pointZ));
                int circlePoint1TopIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, 0, nextPointZ));
                int circlePoint2BaseIndex = verticeCounter++;
                verticeList.Add(new Vector3(nextPointX, thickness, nextPointZ));
                int circlePoint2TopIndex = verticeCounter++;

                //Add triangle for lower side
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(cornerBaseIndex);
                //Add triangle for upper side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(cornerTopIndex);

                //Add Triangles for inner side
                triangleList.Add(circlePoint1TopIndex);
                triangleList.Add(circlePoint1BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2BaseIndex);
                triangleList.Add(circlePoint2TopIndex);
                triangleList.Add(circlePoint1TopIndex);
            }
        }

        mesh = new Mesh();
        mesh.vertices = verticeList.ToArray();
        mesh.triangles = triangleList.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateNormals();

        // Use this to save the Mesh, created from Mesh API
        /*
        UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/Resources/Prefabs/Models/ShaftHolderR1H1.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        */
    }
}
