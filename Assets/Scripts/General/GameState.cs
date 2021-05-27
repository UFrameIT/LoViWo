using System;
using System.Collections.Generic;

public static class GameState
{
    public static bool ServerRunning = false;

    //Global List of Facts
    public static List<Fact> Facts = new List<Fact>();

    //Caching last result
    public static Tuple<List<Fact>,List<SimplifiedFact>> LastKBSimulationResult = 
        new Tuple<List<Fact>, List<SimplifiedFact>>(new List<Fact>(),null);
}
