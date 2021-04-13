using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject movingCogwheel;

    private bool rotate = false;
    private float angularVelocity = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            rotate = !rotate;
        }

        if (rotate)
        {
            movingCogwheel.transform.RotateAround(movingCogwheel.transform.position, movingCogwheel.transform.up, this.angularVelocity * Time.deltaTime);
        }
    }
}
