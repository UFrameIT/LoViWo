using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChainPlacementToolOld : MonoBehaviour
{
    public GameObject chainPrefab;

    private List<Tuple<GameObject, bool>> Chain;
    private List<Vector3> chainShape;

    public LineRenderer lineRenderer;

    private GameObject lastCollidedObject;
    private Ray ray;
    private RaycastHit hit;

    private bool toolActive = false;



    public void activate()
    {
        toolActive = true;
        Chain = new List<Tuple<GameObject, bool>>();
        chainShape = new List<Vector3>();
        lineRenderer.positionCount = 0;

    }

    public void deactivate()
    {
        toolActive = false;
        lineRenderer.positionCount = 0;
        Chain = null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!toolActive)
            {
                activate();
            }
            else
            {
                deactivate();
            }
        }

        if (!toolActive)
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
    }

    void CheckMouseButtons()
    {
        /*
         * (large chunk of logic still in here  at the moment. might be better if moved into seperate functions)
         * 
         * general logic:
         * 1. select cogwheels:
         * 1.1 each time a cogwheel is clicked on 
         *       if it is not already in the list, add it to the list
         *       if it is already in the list, but it is the first cogwheel in the list finish step 1
         * 2. determine whether the cogwheels lie clockwise or counterclockwise on the chain
         * 3. determine wich cogwheels are convex and wich concarve on the chain
         * 4. create the chains 'shape':
         * 4.1 get tangent points
         * 4.2 get arc points
         */

        // If a Cogwheel is clicked on...
        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Cogwheel"))
            {
                
                GameObject Cog = hit.collider.gameObject;


                if (!Chain.Exists(x => x.Item1 == Cog)) //add Cog to Chain
                {
                    //

                    Chain.Add(new Tuple<GameObject, bool>(Cog, true));

                    lineRenderer.positionCount += 1;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, Cog.transform.position);

                }
                else if (Cog == Chain[0].Item1) //complete Chain
                {

                    if (Chain.Count > 2)
                    {
                        List<Vector3> poly = new List<Vector3>();
                        for (int i = 0; i < Chain.Count; i++)
                        {
                            poly.Add(Chain[i].Item1.transform.position);
                        }

                        float angl = total_angle(poly, getCogwheelPlane(Chain[0].Item1.transform));
                        //print("angel: " + angl);

                        bool clockwise;

                        if (angl <= 360.01 && angl >= 359.99)
                        {
                            clockwise = true;
                        }
                        else if (angl <= -359.99 && angl >= -360.01)
                        {
                            clockwise = false;
                        }
                        else
                        {
                            print("illegal shape");
                            deactivate();
                            return;
                        }

                        //figure out wich cogs on the chain are convex and wich are concarve
                        for (int i = 0; i < Chain.Count; i++)
                        {
                            if (i == Chain.Count - 1)
                            {
                                Transform t1 = Chain[i].Item1.transform;
                                float r1 = Chain[i].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t2 = Chain[0].Item1.transform;
                                float r2 = Chain[0].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t3 = Chain[1].Item1.transform;
                                float r3 = Chain[1].Item1.GetComponent<Cogwheel>().getRadius();

                                bool cvx = convex(t1, r1, t2, r2, t3, r3, clockwise);
                                GameObject c = Chain[0].Item1;
                                Chain[0] = new Tuple<GameObject, bool>(c, cvx);
                            }
                            else if (i == Chain.Count - 2)
                            {
                                Transform t1 = Chain[i].Item1.transform;
                                float r1 = Chain[i].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t2 = Chain[i + 1].Item1.transform;
                                float r2 = Chain[i + 1].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t3 = Chain[0].Item1.transform;
                                float r3 = Chain[0].Item1.GetComponent<Cogwheel>().getRadius();

                                bool cvx = convex(t1, r1, t2, r2, t3, r3, clockwise);
                                GameObject c = Chain[i + 1].Item1;
                                Chain[i + 1] = new Tuple<GameObject, bool>(c, cvx);
                            }
                            else
                            {
                                Transform t1 = Chain[i].Item1.transform;
                                float r1 = Chain[i].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t2 = Chain[i + 1].Item1.transform;
                                float r2 = Chain[i + 1].Item1.GetComponent<Cogwheel>().getRadius();
                                Transform t3 = Chain[i + 2].Item1.transform;
                                float r3 = Chain[i + 2].Item1.GetComponent<Cogwheel>().getRadius();

                                bool cvx = convex(t1, r1, t2, r2, t3, r3, clockwise);
                                GameObject c = Chain[i + 1].Item1;
                                Chain[i + 1] = new Tuple<GameObject, bool>(c, cvx);
                            }
                        }

                        //create chainPath

                        for (int i = 0; i < Chain.Count; i++)
                        {
                            if (i == Chain.Count - 1)
                            {
                                //
                                lineRenderer.positionCount += 2;

                                int type = get_tangent_type(clockwise, Chain[i].Item2, Chain[0].Item2);

                                Tuple<Vector3, Vector3> tpl1 = CogwheelTangentPoints(Chain[i].Item1.transform, Chain[i].Item1.GetComponent<Cogwheel>().getRadius(), Chain[0].Item1.transform, Chain[0].Item1.GetComponent<Cogwheel>().getRadius(), type);

                                chainShape.Add(tpl1.Item1);

                                chainShape.Add(tpl1.Item2);


                                
                                Transform cog_transform = Chain[i].Item1.transform;
                                float cog_radius = Chain[i].Item1.GetComponent<Cogwheel>().getRadius();

                                Vector3 start = chainShape[chainShape.Count - 3];
                                Vector3 stop = chainShape[chainShape.Count - 2];

                                List<Vector3> arcPoints = get_arc_points(start, stop, cog_transform, cog_radius, clockwise ^ Chain[i].Item2);
                                chainShape.InsertRange(chainShape.Count - 2, arcPoints);

                           
                                
                            }
                            else
                            {
                                lineRenderer.positionCount += 2;

                                int type = get_tangent_type(clockwise, Chain[i].Item2, Chain[i + 1].Item2);

                                Tuple<Vector3, Vector3> tpl1 = CogwheelTangentPoints(Chain[i].Item1.transform, Chain[i].Item1.GetComponent<Cogwheel>().getRadius(), Chain[i + 1].Item1.transform, Chain[i + 1].Item1.GetComponent<Cogwheel>().getRadius(), type);
                                
                                chainShape.Add(tpl1.Item1);

                                chainShape.Add(tpl1.Item2);

                                
                                // add arc points
                                if (i > 0)
                                {
                                    Transform cog_transform = Chain[i].Item1.transform;
                                    float cog_radius = Chain[i].Item1.GetComponent<Cogwheel>().getRadius();

                                    Vector3 start = chainShape[chainShape.Count - 3];
                                    Vector3 stop = chainShape[chainShape.Count - 2];

                                    List<Vector3> arcPoints = get_arc_points(start, stop, cog_transform, cog_radius, clockwise ^ Chain[i].Item2);
                                    chainShape.InsertRange(chainShape.Count - 2, arcPoints);
                                    

                                }
                                
                            }

                        }
                        
                        Transform cog_trans = Chain[0].Item1.transform;
                        float cog_rad = Chain[0].Item1.GetComponent<Cogwheel>().getRadius();

                        Vector3 strt = chainShape[chainShape.Count - 1];
                        Vector3 stp = chainShape[0];

                        List<Vector3> arcPnts = get_arc_points(strt, stp, cog_trans, cog_rad, clockwise ^ Chain[0].Item2);
                        chainShape.InsertRange(chainShape.Count, arcPnts);


                        lineRenderer.positionCount = chainShape.Count;
                        lineRenderer.SetPositions(chainShape.ToArray());

                        GameObject newChain = createChain(Chain, chainShape);

                        int chnId = GameState.Facts.Count;

                        int[] cogIds = Chain.Select(tpl1 => tpl1.Item1.GetComponent<RotatableCogwheel>().getAssociatedFact().Id).ToArray(); //Select(fact => fact.Id).ToArray()
                        bool[] orientatins = Chain.Select(tpl1 => tpl1.Item2).ToArray();

                        // ChainFact newFact = new ChainFact(chnId, cogIds, orientatins, GameState.Facts);
                        ChainFact newFact = null;
                        newFact.Representation = newChain;
                        GameState.Facts.Insert(chnId, newFact);
                        UnityEngine.Debug.Log("Successfully added new ChainFact with backendUri: " + newFact.backendURI);

                        deactivate();

                    }
                    else if (Chain.Count == 2)
                    {
                        
                        int type = get_tangent_type(true, true, true);

                        Tuple<Vector3, Vector3> tpl1 = CogwheelTangentPoints(Chain[0].Item1.transform, Chain[0].Item1.GetComponent<Cogwheel>().getRadius(), Chain[1].Item1.transform, Chain[1].Item1.GetComponent<Cogwheel>().getRadius(), type);
                        chainShape.Add(tpl1.Item1);
                        chainShape.Add(tpl1.Item2);

                        Tuple<Vector3, Vector3> tpl2 = CogwheelTangentPoints(Chain[1].Item1.transform, Chain[1].Item1.GetComponent<Cogwheel>().getRadius(), Chain[0].Item1.transform, Chain[0].Item1.GetComponent<Cogwheel>().getRadius(), type);
                        chainShape.Add(tpl2.Item1);
                        chainShape.Add(tpl2.Item2);

                        List<Vector3> arcPoints1 = get_arc_points(chainShape[1], chainShape[2], Chain[1].Item1.transform, Chain[1].Item1.GetComponent<Cogwheel>().getRadius(), true ^ true);
                        List<Vector3> arcPoints2 = get_arc_points(chainShape[3], chainShape[0], Chain[0].Item1.transform, Chain[0].Item1.GetComponent<Cogwheel>().getRadius(), true ^ true);
                        chainShape.InsertRange(2, arcPoints1);
                        chainShape.InsertRange(chainShape.Count, arcPoints2);

                        lineRenderer.positionCount = chainShape.Count;
                        lineRenderer.SetPositions(chainShape.ToArray());

                        GameObject newChain = createChain(Chain, chainShape);

                                               
                        int chnId = GameState.Facts.Count;

                        int[] cogIds = Chain.Select(tpl1 => tpl1.Item1.GetComponent<RotatableCogwheel>().getAssociatedFact().Id).ToArray(); //Select(fact => fact.Id).ToArray()
                        bool[] orientatins = Chain.Select(tpl1 => tpl1.Item2).ToArray();

                        //ChainFact newFact = new ChainFact(chnId, cogIds, orientatins, GameState.Facts);
                        ChainFact newFact = null;
                        newFact.Representation = newChain;
                        GameState.Facts.Insert(chnId, newFact);
                        UnityEngine.Debug.Log("Successfully added new ChainFact with backendUri: " + newFact.backendURI);
                        


                        deactivate();
                    }
                    else
                    {
                        deactivate();
                    }

                }
                else
                {
                    print("already in List. Not added");
                }
            }

            else
            {
                print("clicked on a not a Cogwheel");
            }

        }

    }

    /* 
     * determines whether Cogwheel c2 sits convex or concarve on the chain.
     * 
     * c2 is positioned between c1 and c3 along the chain.
     * 
     * if clockwise is true the cogwheels c1, c2, c3 are positioned clockwise along the chain,
     * else they are positioned counterclockwise
     */
    private bool convex(Transform c1_transform, float c1_radius, Transform c2_transform, float c2_radius, Transform c3_transform, float c3_radius, bool clockwise)
    {

        Plane plane = getCogwheelPlane(c1_transform);

        Tuple<Vector3, Vector3> tpl1;
        Tuple<Vector3, Vector3> tpl2;

        //looks at the angle between the tangents from c1 to c2 and c1 to c3 to determine whether convex or concarve
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

    /*
     * determines the plane on wich the cogwheel lies
     * 
     * useful for finding out whether two cogwheels lie on the same plane
     * and for getting a normal vektor for angel calculation
     */
    private Plane getCogwheelPlane(Transform transform)
    {
        //get three points on the plane in coordinates relative to cogwheel
        Vector3 pos1 = Vector3.zero;
        Vector3 pos2 = Vector3.zero;
        pos2.z += 1;
        Vector3 pos3 = Vector3.zero;
        pos3.x += 1;

        //translate 'local points' to 'global points'
        pos1 = transform.localToWorldMatrix * new Vector4(pos1.x, pos1.y, pos1.z, 1);
        pos2 = transform.localToWorldMatrix * new Vector4(pos2.x, pos2.y, pos2.z, 1);
        pos3 = transform.localToWorldMatrix * new Vector4(pos3.x, pos3.y, pos3.z, 1);

        //generate a plane form those points
        Plane ret = new Plane(pos1, pos2, pos3);

        return ret;
    }

    /*
     * returns the tangent points along the cogwheels c1 and c2 of the tangent to the two cogwheels
     * 
     * there are four possible tangents that can be constructed given two circles. type indicates wich of those we want to construct
     */
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
        // (so we can just assume that one value is the bigger one and one the smaller one in the calculation)
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

        // avoid bugs if distance is smaller than the sum of the radii
        if (calc_r1 + calc_r2 > dist)
        {
            dist = calc_r1 + calc_r2;
        }

        // temporarily rotate the one cogwheels transform so that both cogwheels transforms share the same rotation
        // is necessary to get the correct points when calling circlePoint
        // is revertet after points are calculated
        Quaternion temp_rotation = c2_transform.rotation;
        c2_transform.rotation = c1_transform.rotation;

        // 'direction' in wich c2 lies relative to c1
        float angle_offset = Vector3.SignedAngle(new Vector3(1, 0, 0), c1_transform.InverseTransformPoint(c2_transform.position), new Vector3(0, -1, 0));
        if (angle_offset < 0)
        {
            angle_offset += 360;
        }
        angle_offset *= Mathf.Deg2Rad;

        // variables for the points we want to calculate
        Vector3 tangPos1;
        Vector3 tangPos2;


        if (calc_type == 0) // 'c1 top to c2 top'
        {
            // 'relative angle of the tangent'
            float angle_a = Mathf.Acos((calc_r1 - calc_r2) / dist);
            // 'angle of the tangent'
            float angle1;
            if (c1_bigger_c2)
            {
                angle1 = angle_a + angle_offset;
            }
            else
            {
                // if we swapped the cogwheels this angle needs to be calculated somewhat differently
                angle1 = ((180f * Mathf.Deg2Rad) - angle_a) + angle_offset;
            }

            tangPos1 = circlePoint(calc_t1, calc_r1, angle1);
            tangPos2 = circlePoint(calc_t2, calc_r2, angle1);
        }
        else if (calc_type == 1) // 'c1 bottom to c2 bottom'
        {
            float angle_b = (360 * Mathf.Deg2Rad) - Mathf.Acos((calc_r1 - calc_r2) / dist);
            float angle2 = angle_b + angle_offset;
            if (c1_bigger_c2)
            {
                angle2 = angle_b + angle_offset;
            }
            else
            {
                angle2 = ((180f * Mathf.Deg2Rad) - angle_b) + angle_offset;
            }

            tangPos1 = circlePoint(calc_t1, calc_r1, angle2);
            tangPos2 = circlePoint(calc_t2, calc_r2, angle2);
        }
        else if (calc_type == 2)
        {
            // im certain there is some logic behind this, however the way I got to said logic
            // was by trying out different things until i got the behaviour i wanted, so.... yeah
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

            tangPos1 = circlePoint(calc_t1, calc_r1, angle4 + (180 * Mathf.Deg2Rad));
            tangPos2 = circlePoint(calc_t2, calc_r2, angle4);
        }

        // revert temporariy rotation of the transform
        c2_transform.rotation = temp_rotation;

        //return the two calculated points as a tuple
        if (c1_bigger_c2)
        {
            return new Tuple<Vector3, Vector3>(tangPos1, tangPos2);
        }
        else
        {
            return new Tuple<Vector3, Vector3>(tangPos2, tangPos1);
        }
    }

    /*
     * returns a point lying on a circle around the cogwheel with given transform and radius
     * angle indicates the position of the point on the circle (relative to the transform)
     */
    private Vector3 circlePoint(Transform transform, float radius, float angle)
    {
        Vector3 pos = Vector3.zero;
        pos.z += radius * Mathf.Sin(angle);
        pos.x += radius * Mathf.Cos(angle);

        pos = transform.localToWorldMatrix * new Vector4(pos.x, pos.y, pos.z, 1);

        return pos;
    }

    /*
     * returns the sum of all angles in a polygon(represented as a list of points) lying on a given plane
     * the method is there to help figure out if a polygon is clockwise or anticlockwise
     */
    private float total_angle(List<Vector3> polygon, Plane plane)
    {
        if (polygon.Count < 3)
        {
            return -1;
        }


        float totl_angle = 0;

        float angle;
        Vector3 pos1;
        Vector3 pos2;
        Vector3 pos3;

        for (int i = 0; i < polygon.Count - 2; i++)
        {
            pos1 = polygon[i];
            pos2 = polygon[i + 1];
            pos3 = polygon[i + 2];

            angle = Vector3.SignedAngle(pos2 - pos1, pos3 - pos2, plane.normal);

            totl_angle += angle;
        }

        pos1 = polygon[polygon.Count - 2];
        pos2 = polygon[polygon.Count - 1];
        pos3 = polygon[0];
        angle = Vector3.SignedAngle(pos2 - pos1, pos3 - pos2, plane.normal);
        totl_angle += angle;

        pos1 = polygon[polygon.Count - 1];
        pos2 = polygon[0];
        pos3 = polygon[1];
        angle = Vector3.SignedAngle(pos2 - pos1, pos3 - pos2, plane.normal);
        totl_angle += angle;

        return totl_angle;
    }


    /*
     * gives the 'type' of tangent we want given whether the two cogwheels we want to connect lie convex or convarve on the chain
     * and whether the cogwheels lie clockwise or counterclockwise around the chain
     */
    private int get_tangent_type(bool clockwise, bool convex1, bool convex2)
    {
        if (clockwise)
        {
            if (convex1 && convex2)
            {
                return 0;
            }
            else if (!convex1 && !convex2)
            {
                return 1;
            }
            else if (convex1 && !convex2)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        else
        {
            if (convex1 && convex2)
            {
                return 1;
            }
            else if (!convex1 && !convex2)
            {
                return 0;
            }
            else if (!convex1 && convex2)
            {
                return 2;
            }
            else
            {
                return 3;
            }

        }

    }

    /*
     * returns a list of points that 'draw' a partial circel from point start to point stop around a cogwheel 
     * either clockwise or counterclockwise
     */
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

    /*
     * given the transform of a cogwheel and a point that lies on a circle around the cogwheel
     * the method returns the angle relative to the cogwheel at wich the point lies
     */
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


    private GameObject createChain(List<Tuple<GameObject, bool>> Chain, List<Vector3> chainShape)
    {
        GameObject chain = Instantiate(chainPrefab);
        chain.GetComponent<Chain>().createChain(Chain, chainShape);
        return chain;
    }
}
