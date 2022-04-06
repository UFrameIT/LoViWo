using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideRadiusCollider : MonoBehaviour
{
    public MeshCollider insideCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setRadiusAndHeight(float radius, float height)
    {
        this.transform.localScale = new Vector3(radius * 2.0f, height / 2.0f, radius * 2.0f);
        this.transform.localPosition = new Vector3(0.0f, height / 2.0f, 0.0f);
        this.gameObject.layer = LayerMask.NameToLayer("SimulatedObjects");
    }
}
