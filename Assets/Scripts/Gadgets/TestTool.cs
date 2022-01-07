using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTool : MonoBehaviour
{

    private GameObject lastCollidedObject;

    public LineRenderer lineRenderer;

    private bool circleToolActive = false;
    private bool tangentToolActive = false;
    private bool xyzToolActive = false;
    private bool miscTestsToolActive = true;

    private Ray ray;
    private RaycastHit hit;

    private GameObject currentCog;
    private GameObject previousCog;

    private GameObject Cog1;
    private GameObject Cog2;
    private GameObject Cog3;

    private Plane selectedPlane;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!circleToolActive && !tangentToolActive && !xyzToolActive && !miscTestsToolActive)
        {
            return;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                CheckMouseButtons();
            }
        }

        if (Input.GetKey(KeyCode.T) && miscTestsToolActive)
        {
            miscTestsTool();
        }

    }

    void CheckMouseButtons()
    {


        // If a Cogwheel is clicked on...
        if (Input.GetMouseButtonDown(0))
        {

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel"))
            {
                if (circleToolActive == true)
                {
                    drawCircleAroundCog();
                }

                if (tangentToolActive == true)
                {
                    previousCog = currentCog;
                    currentCog = hit.collider.gameObject;


                    print("tangent Tool");

                    if (previousCog != null)
                    {

                        selectedPlane = getCogwheelPlane(previousCog.transform);

                        if (selectedPlane.ClosestPointOnPlane(currentCog.transform.position) == currentCog.transform.position)
                        {
                            print("Cogwheel is on Plane");
                        }
                        else
                        {
                            print("Cogwheel is not on Plane");
                            return;
                        }


                        GameObject cog1;
                        GameObject cog2;
                        Vector3 pos1;
                        Vector3 pos2;
                        if (currentCog.GetComponent<Cogwheel>().getRadius() >= previousCog.GetComponent<Cogwheel>().getRadius())
                        {
                            cog1 = currentCog;
                            cog2 = previousCog;
                        }
                        else
                        {
                            cog1 = previousCog;
                            cog2 = currentCog;
                        }


                        pos1 = cog1.transform.position;
                        pos2 = cog2.transform.position;

                        Quaternion temp_rotation = cog2.transform.rotation;
                        cog2.transform.rotation = cog1.transform.rotation;

                        float R = cog1.GetComponent<Cogwheel>().getRadius();
                        float r = cog2.GetComponent<Cogwheel>().getRadius();
                        float D = Vector3.Distance(pos1, pos2);

                        if (R + r > D)
                        {
                            D = R + r;
                            print("D set to r + R");
                        }

                        float angle_offset = Vector3.SignedAngle(new Vector3(1, 0, 0), cog1.transform.InverseTransformPoint(cog2.transform.position), new Vector3(0, -1, 0));
                        if (angle_offset < 0)
                        {
                            angle_offset += 360;
                        }
                        angle_offset *= Mathf.Deg2Rad;

                        lineRenderer.positionCount = 8;

                        //type1
                        print("R: " + R + "   r: " + r + "   D: " + D);

                        print("Math.Acos(0) :" + Mathf.Acos(0));

                        float angle_a = Mathf.Acos((R - r) / D);

                        print("angle_offset: " + angle_offset);
                        print("angle_a: " + angle_a);

                        float angle1 = angle_a + angle_offset;
                        print("angle1: " + angle1);

                        Vector3 tangPos1 = circlePoint(cog1.transform, R, angle1);
                        Vector3 tangPos2 = circlePoint(cog2.transform, r, angle1);

                        lineRenderer.SetPosition(0, tangPos1);
                        lineRenderer.SetPosition(1, tangPos2);

                        //type2
                        float angle_b = (360 * Mathf.Deg2Rad) - Mathf.Acos((R - r) / D);

                        //print("angle_b: " + angle_b);

                        float angle2 = angle_b + angle_offset;
                        //print("angle2: " + angle2);

                        Vector3 tangPos3 = circlePoint(cog2.transform, r, angle2);
                        Vector3 tangPos4 = circlePoint(cog1.transform, R, angle2);

                        lineRenderer.SetPosition(2, tangPos3);
                        lineRenderer.SetPosition(3, tangPos4);

                        //type3
                        float angle_c = (360 * Mathf.Deg2Rad) - Mathf.Asin((R + r) / D);
                        angle_c += (90 * Mathf.Deg2Rad);

                        //print("angle_c: " + angle_c);

                        float angle3 = angle_c + angle_offset;
                        //print("angle3: " + angle3);

                        Vector3 tangPos5 = circlePoint(cog1.transform, R, angle3);
                        print("angle3: " + angle3);
                        print("R: " + R);
                        Vector3 tangPos6 = circlePoint(cog2.transform, r, angle3 + (180 * Mathf.Deg2Rad));

                        lineRenderer.SetPosition(4, tangPos5);
                        lineRenderer.SetPosition(5, tangPos6);

                        //type4
                        float angle_d = Mathf.Asin((R + r) / D);
                        angle_d += (90 * Mathf.Deg2Rad);

                        //print("angle_d: " + angle_d);

                        float angle4 = angle_d + angle_offset;
                        //print("angle4: " + angle4);

                        Vector3 tangPos7 = circlePoint(cog2.transform, r, angle4);
                        Vector3 tangPos8 = circlePoint(cog1.transform, R, angle4 + (180 * Mathf.Deg2Rad));

                        lineRenderer.SetPosition(6, tangPos7);
                        lineRenderer.SetPosition(7, tangPos8);



                        cog2.transform.rotation = temp_rotation;
                    }

                }


                if (xyzToolActive == true)
                {
                    previousCog = currentCog;
                    currentCog = hit.collider.gameObject;

                    Transform transform = hit.collider.gameObject.transform;

                    if (previousCog != null) 
                    {
                        xyzTool(transform, previousCog.transform);
                    }

                }

            }

        }

    }

    private void drawCircleAroundCog()
    {
        //print("Drawing circle");
        Cogwheel cog = hit.collider.gameObject.GetComponent<Cogwheel>();
        Transform cogTransform = hit.collider.gameObject.transform;
        Vector3 cogPos = cogTransform.position;
        float cogRadius = cog.getRadius();

        lineRenderer.positionCount = 9;

        for (int p = 0; p < lineRenderer.positionCount; p++)
        {
            Vector3 pos = circlePoint(cogTransform, cogRadius, 0.125f * Mathf.PI * p);

            lineRenderer.SetPosition(p, pos);
        }
    }

    private Vector3 circlePoint(Transform transform, float radius, float angle)
    {
        Vector3 pos = Vector3.zero;
        pos.z += radius * Mathf.Sin(angle);
        pos.x += radius * Mathf.Cos(angle);

        pos = transform.localToWorldMatrix * new Vector4(pos.x, pos.y, pos.z, 1);

        return pos;
    }

    private Plane getCogwheelPlane(Transform transform)
    {
        Vector3 pos1 = Vector3.zero;
        Vector3 pos2 = Vector3.zero;
        pos2.z += 1;
        Vector3 pos3 = Vector3.zero;
        pos3.x += 1;

        pos1 = transform.localToWorldMatrix * new Vector4(pos1.x, pos1.y, pos1.z, 1);
        pos2 = transform.localToWorldMatrix * new Vector4(pos2.x, pos2.y, pos2.z, 1);
        pos3 = transform.localToWorldMatrix * new Vector4(pos3.x, pos3.y, pos3.z, 1);

        Plane ret = new Plane(pos1, pos2, pos3);

        return ret;
    }

    private void xyzTool(Transform transform1, Transform transform2)
    {
        Vector3 origin = new Vector3(0, 0, 0);
        Vector3 pos_x = new Vector3(20, 0, 0);
        Vector3 pos_y = new Vector3(0, 15, 0);
        Vector3 pos_z = new Vector3(0, 0, 10);

        Quaternion temp_rotation = transform1.rotation;
        if (transform2 != null)
        {
            transform1.rotation = transform2.rotation;
        }

        origin = transform1.localToWorldMatrix * new Vector4(origin.x, origin.y, origin.z, 1);
        pos_x = transform1.localToWorldMatrix * new Vector4(pos_x.x, pos_x.y, pos_x.z, 1);
        pos_y = transform1.localToWorldMatrix * new Vector4(pos_y.x, pos_y.y, pos_y.z, 1);
        pos_z = transform1.localToWorldMatrix * new Vector4(pos_z.x, pos_z.y, pos_z.z, 1);

        lineRenderer.positionCount = 5;

        lineRenderer.SetPosition(0, pos_x);
        lineRenderer.SetPosition(1, origin);
        lineRenderer.SetPosition(2, pos_y);
        lineRenderer.SetPosition(3, origin);
        lineRenderer.SetPosition(4, pos_z);

        transform1.rotation = temp_rotation;
    }

    private void miscTestsTool()
    {
        /*
        print("angle: " + angle);

        Vector3 origin = new Vector3(0, 0, 0);
        Vector3 indicated = new Vector3(0, 0, 0);

        indicated.z += 10 * Mathf.Sin(angle);
        indicated.x += 10 * Mathf.Cos(angle);

        origin = transform.localToWorldMatrix * new Vector4(origin.x, origin.y, origin.z, 1);
        indicated = transform.localToWorldMatrix * new Vector4(indicated.x, indicated.y, indicated.z, 1);

        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, indicated);
        lineRenderer.SetPosition(1, origin);
        */

        print(GameSettings.archivesPath);

    }

    private List<Vector3> get_arc_points(Vector3 start, Vector3 stop, Transform cog_transform, float cog_radius, bool clockwise)
    {
        float start_angle = get_circlePoint_angle(cog_transform, start);
        float stop_angle = get_circlePoint_angle(cog_transform, stop);

        List<Vector3> points = new List<Vector3>();

        float angle_difference;

        if (clockwise)
        {
            angle_difference = stop_angle - start_angle;
        }
        else
        {
            angle_difference = start_angle - stop_angle;
        }

        if (angle_difference < 0)
        {
            angle_difference += 360f * Mathf.Deg2Rad;
        }


        if (clockwise)
        {
            for (int i = 1; i < (angle_difference / (9f * Mathf.Deg2Rad)); i++)
            {
                Vector3 point = circlePoint(cog_transform, cog_radius, start_angle + (i * (9f * Mathf.Deg2Rad)));
                points.Add(point);
            }
        }
        else
        {
            for (int i = (int)(angle_difference / (9f * Mathf.Deg2Rad)) - 1; i > 0; i--)
            {
                Vector3 point = circlePoint(cog_transform, cog_radius, stop_angle + (i * (9f * Mathf.Deg2Rad)));
                points.Add(point);
            }
        }

        return points;
    }

    private float get_circlePoint_angle(Transform transform, Vector3 point)
    {
        Vector3 vec_0 = new Vector3(1, 0, 0);
        Vector3 vec_a = transform.InverseTransformPoint(point);

        float angle = Vector3.SignedAngle(vec_a, vec_0, new Vector3(0, 1, 0));

        if (angle < 0)
        {
            angle += 360;
        }

        return angle * Mathf.Deg2Rad;
    }

    private Tuple<Vector3, Vector3> CogwheelTangentPoints(Transform c1_transform, float c1_radius, Transform c2_transform, float c2_radius, int type)
    {
        // it is expected that both cogwheels lie on the same plane


        Transform calc_t1; //Transform of the bigger Cogwheel
        Transform calc_t2; //Transform of the smaller Cogwheel
        float calc_r1; //radius of the bigger Cogwheel
        float calc_r2; //radius of the smaller Cogwheel

        int calc_type; //

        float dist; //distance between both cog centers

        // whether c1 is bigger than c2. If not values are swapped for following calculations
        bool c1_bigger_c2 = c1_radius >= c2_radius;
        if (c1_bigger_c2)
        {
            calc_t1 = c1_transform;
            calc_t2 = c2_transform;
            calc_r1 = c1_radius;
            calc_r2 = c2_radius;

            calc_type = type;
        }
        else
        {
            calc_t1 = c2_transform;
            calc_t2 = c1_transform;
            calc_r1 = c2_radius;
            calc_r2 = c1_radius;

            calc_type = type;

        }
        dist = Vector3.Distance(calc_t1.position, calc_t2.position);

        //avoid bugs if distance is smaller than the um of the radii
        if (calc_r1 + calc_r2 > dist)
        {
            dist = calc_r1 + calc_r2;
        }

        Quaternion temp_rotation = c2_transform.rotation;
        c2_transform.rotation = c1_transform.rotation;

        //
        float angle_offset = Vector3.SignedAngle(new Vector3(1, 0, 0), c1_transform.InverseTransformPoint(c2_transform.position), new Vector3(0, -1, 0));
        if (angle_offset < 0)
        {
            angle_offset += 360;
        }
        angle_offset *= Mathf.Deg2Rad;

        //
        Vector3 tangPos1;
        Vector3 tangPos2;


        if (calc_type == 0)
        {
            float angle_a = Mathf.Acos((calc_r1 - calc_r2) / dist);
            float angle1 = angle_a + angle_offset;

            tangPos1 = circlePoint(calc_t1, calc_r1, angle1);
            tangPos2 = circlePoint(calc_t2, calc_r2, angle1);
        }
        else if (calc_type == 1)
        {
            float angle_b = (360 * Mathf.Deg2Rad) - Mathf.Acos((calc_r1 - calc_r2) / dist);
            float angle2 = angle_b + angle_offset;

            tangPos1 = circlePoint(calc_t1, calc_r1, angle2);
            tangPos2 = circlePoint(calc_t2, calc_r2, angle2);
        }
        else if (calc_type == 2)
        {
            float angle_c = (360 * Mathf.Deg2Rad) - Mathf.Asin((calc_r1 + calc_r2) / dist);
            if (c1_bigger_c2)
            {
                angle_c += (90 * Mathf.Deg2Rad);
            }
            else
            {
                angle_c -= (90 * Mathf.Deg2Rad);
            }

            float angle3 = angle_c + angle_offset;

            tangPos1 = circlePoint(calc_t1, calc_r1, angle3);
            tangPos2 = circlePoint(calc_t2, calc_r2, angle3 + (180 * Mathf.Deg2Rad));
        }
        else
        {
            float angle_d = Mathf.Asin((calc_r1 + calc_r2) / dist);
            if (c1_bigger_c2)
            {
                angle_d += (90 * Mathf.Deg2Rad);
            }
            else
            {
                angle_d -= (90 * Mathf.Deg2Rad);
            }

            float angle4 = angle_d + angle_offset;

            tangPos1 = circlePoint(calc_t2, calc_r2, angle4);
            tangPos2 = circlePoint(calc_t1, calc_r1, angle4 + (180 * Mathf.Deg2Rad));
        }


        c2_transform.rotation = temp_rotation;

        if (c1_bigger_c2)
        {
            return new Tuple<Vector3, Vector3>(tangPos1, tangPos2);
        }
        else
        {
            return new Tuple<Vector3, Vector3>(tangPos2, tangPos1);
        }
    }

    private bool convex(Transform c1_transform, float c1_radius, Transform c2_transform, float c2_radius, Transform c3_transform, float c3_radius, bool clockwise)
    {

        Plane plane = getCogwheelPlane(c1_transform);

        Tuple<Vector3, Vector3> tpl1;
        Tuple<Vector3, Vector3> tpl2;

        if (clockwise)
        {
            tpl1 = CogwheelTangentPoints(c1_transform, c1_radius, c2_transform, c2_radius, 0);
            tpl2 = CogwheelTangentPoints(c1_transform, c1_radius, c3_transform, c3_radius, 0);
        }
        else
        {
            tpl1 = CogwheelTangentPoints(c1_transform, c1_radius, c2_transform, c2_radius, 1);
            tpl2 = CogwheelTangentPoints(c1_transform, c1_radius, c3_transform, c3_radius, 1);
        }

        Vector3 pos1_1 = tpl1.Item1;
        Vector3 pos1_2 = tpl1.Item2;
        Vector3 pos2_1 = tpl2.Item1;
        Vector3 pos2_2 = tpl2.Item2;

        lineRenderer.positionCount = 4;

        lineRenderer.SetPosition(0, pos1_2);
        lineRenderer.SetPosition(1, pos1_1);
        lineRenderer.SetPosition(2, pos2_1);
        lineRenderer.SetPosition(3, pos2_2);

        float angle;

        if (clockwise)
        {
            angle = Vector3.SignedAngle(pos1_2 - pos1_1, pos2_2 - pos2_1, plane.normal);
        }
        else
        {
            angle = Vector3.SignedAngle(pos2_2 - pos2_1, pos1_2 - pos1_1, plane.normal);
        }


        if (angle >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}