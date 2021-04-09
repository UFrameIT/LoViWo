using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Rotatable : Connectable
{
    void rotate(float angularVelocity);
    void stopRotation();
}
