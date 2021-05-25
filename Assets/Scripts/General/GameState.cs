using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    public static bool ServerRunning = false;

    //Global List of Facts
    public static List<Fact> Facts = new List<Fact>();
    //List of Facts, used for Simulation
    public static List<Fact> TemporaryFacts = new List<Fact>();
}
