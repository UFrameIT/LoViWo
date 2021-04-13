using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interlockable
{
    void addInterlockingPart(Interlockable part);
    void activatePhysics();
    void deactivatePhysics();
}
