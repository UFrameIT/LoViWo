using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using static JSONManager;

/*
public class ParsingDictionary
{

    public static Dictionary<string, Func<Scroll.ScrollFact, Fact>> parseFactDictionary = new Dictionary<string, Func<Scroll.ScrollFact, Fact>>() {
        {MMTURIs.Point, PointFact.parseFact},
        {MMTURIs.Metric, LineFact.parseFact},
        {MMTURIs.Angle, AngleFact.parseFact},
        {MMTURIs.LineType, RayFact.parseFact},
        {MMTURIs.OnLine, OnLineFact.parseFact},
        //90Degree-Angle
        {MMTURIs.Eq, AngleFact.parseFact}
    };

}
*/

public abstract class Fact
{
    private int _id;
    public string Label;
    public int Id
    {
        get { return _id; }
        set
        {
            _id = value;
            Label = ((Char)(64 + _id + 1)).ToString();
        }
    }
    public GameObject Representation;
    public string backendURI;

    public string format(float t)
    {
        return t.ToString("0.0000").Replace(',', '.');
    }

    public abstract Boolean hasDependentFacts();

    public abstract int[] getDependentFactIds();

    public abstract override bool Equals(System.Object obj);

    public abstract override int GetHashCode();

    public static string getLetter(int Id)
    {
        return ((Char)(64 + Id + 1)).ToString();
    }
}

public class AddFactResponse
{
    public string uri;

    public static AddFactResponse sendAdd(string path, string body)
    {
        if (!GameState.ServerRunning)
        {
            Debug.LogWarning("Server not running");
            return new AddFactResponse();
        }
        Debug.Log(body);

        UnityWebRequest www = UnityWebRequest.Put(path, body);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 1;

        AsyncOperation op = www.SendWebRequest();
        while (!op.isDone) { }
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning(www.error);
            return new AddFactResponse();
        }
        else
        {
            string answer = www.downloadHandler.text;
            return JsonUtility.FromJson<AddFactResponse>(answer);
        }
    }
}

public class CogwheelFact : Fact
{
    public Vector3 Point;
    public Vector3 Normal;
    public float Radius;
    public float InsideRadius;
    public float OutsideRadius;


    public CogwheelFact(int i, Vector3 P, Vector3 N, float R, float iR, float oR, List<Fact> facts)
    {
        this.Id = i;
        this.Point = P;
        this.Normal = N;
        this.Radius = R;
        this.InsideRadius = iR;
        this.OutsideRadius = oR;

        List<MMTTerm> tupleArguments = new List<MMTTerm>
        {
            new OMF(P.x),
            new OMF(P.y),
            new OMF(P.z)
        };

        List<MMTTerm> wheelOfArguments = new List<MMTTerm>
        {
            new OMF(this.Radius),
            new OMF(this.InsideRadius),
            new OMF(this.OutsideRadius),
            new OMA(new OMS(MMTURIs.Tuple), tupleArguments),
            new OMF((float)this.Id)
        };

        MMTTerm tp = new OMS(MMTURIs.Cogwheel);
        MMTTerm df = new OMA(new OMS(MMTURIs.CogwheelOf), wheelOfArguments);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return false;
    }

    public override int[] getDependentFactIds()
    {
        return null;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            CogwheelFact p = (CogwheelFact)obj;
            return this.Point.Equals(p.Point) && this.Normal.Equals(p.Normal) && this.Radius.Equals(p.Radius);
        }
    }

    public override int GetHashCode()
    {
        return this.Point.GetHashCode() ^ this.Normal.GetHashCode() ^ this.Radius.GetHashCode();
    }
}

public class CogwheelEqsysFact : Fact
{
    public int[] CogwheelIds;
    
    public CogwheelEqsysFact(int i, int[] ids, List<Fact> facts)
    {
        this.Id = i;
        this.CogwheelIds = ids;

        List<MMTTerm> typeArguments = new List<MMTTerm>
        {
            new OMS(MMTURIs.Prop)
        };

        List<int> cogwheelIdList = new List<int>(this.CogwheelIds);
        List<MMTTerm> listArguments = new List<MMTTerm>();
        cogwheelIdList
            .Select(cogwheelId =>
                new OMS((facts.Find(x => x.Id == cogwheelId) as CogwheelFact).backendURI))
            .ToList()
            .ForEach(oms => listArguments.Add(oms));

        List<MMTTerm> eqsysArguments = new List<MMTTerm>
        {
            new OMA(new OMS(MMTURIs.ListOf), listArguments)
        };

        MMTTerm tp = new OMA(new OMS(MMTURIs.List), typeArguments);
        MMTTerm df = new OMA(new OMS(MMTURIs.CogwheelEquationSystem), eqsysArguments);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return this.CogwheelIds;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            CogwheelEqsysFact p = (CogwheelEqsysFact)obj;
            return this.CogwheelIds.Equals(p.CogwheelIds);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        new List<int>(this.CogwheelIds).ForEach (x => hashcode ^= x) ;
        return hashcode;
    }
}



public class ChainFact : Fact
{
    public int[] CogwheelIds;
    public bool[] CogwheelOrientations;

    public ChainFact(int i, int[] ids, bool[] orientations, List<Fact> facts)
    {
        this.Id = i;
        this.CogwheelIds = ids;
        this.CogwheelOrientations = orientations;

        List<int> cogwheelIdList = new List<int>(this.CogwheelIds);
        List<bool> cogwheelOrientations = new List<bool>(this.CogwheelOrientations);
        List<MMTTerm> listArguments = new List<MMTTerm>();
        
        List<MMTTerm> tuple_list = new List<MMTTerm>();

        cogwheelIdList
            .Select(cogwheelId =>
                new OMS((facts.Find(x => x.Id == cogwheelId) as CogwheelFact).backendURI))
            .ToList()
            .ForEach(oms => listArguments.Add(oms));

        listArguments.Zip(orientations, (first, second) => new Tuple<MMTTerm, bool>(first, second))
            .Select(tpl =>
                new OMA (new OMS(MMTURIs.Tuple), new List<MMTTerm> { tpl.Item1, tpl.Item2 ? new OMS(MMTURIs.Convex) : new OMS(MMTURIs.Concarve) }))
            .ToList()
            .ForEach(tpl => tuple_list.Add(tpl));



        List<MMTTerm> chainOfArgs = new List<MMTTerm>
        {
            new OMA(new OMS(MMTURIs.ListOf), tuple_list),
            new OMF((float)this.Id)
        };


        MMTTerm tp = new OMS(MMTURIs.Chain);
        MMTTerm df = new OMA(new OMS(MMTURIs.ChainOf), chainOfArgs);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return false;
    }

    public override int[] getDependentFactIds()
    {
        return this.CogwheelIds;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ChainFact p = (ChainFact)obj;
            return this.CogwheelIds.Equals(p.CogwheelIds) && this.CogwheelOrientations.Equals(p.CogwheelOrientations);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        new List<int>(this.CogwheelIds).ForEach(x => hashcode ^= x);
        return hashcode;
    }
}



public class CogChainEqsysFact : Fact
{
    public int[] CogwheelIds;
    public int[] ChainIds;

    public CogChainEqsysFact(int i, int[] cogIds, int[] chainIds, Dictionary<Fact, float> knownAvMap, List<Fact> facts)
    {
        this.Id = i;
        this.CogwheelIds = cogIds;
        this.ChainIds = chainIds;

        List<MMTTerm> typeArguments = new List<MMTTerm>
        {
            new OMS(MMTURIs.Prop)
        };

        List<int> cogwheelIdList = new List<int>(this.CogwheelIds);
        List<MMTTerm> cogListArguments = new List<MMTTerm>();
        cogwheelIdList
            .Select(cogwheelId =>
                new OMS((facts.Find(x => x.Id == cogwheelId) as CogwheelFact).backendURI))
            .ToList()
            .ForEach(oms => cogListArguments.Add(oms));

        List<int> chainlIdList = new List<int>(this.ChainIds);
        List<MMTTerm> chainListArguments = new List<MMTTerm>();
        chainlIdList
            .Select(chainId =>
                new OMS((facts.Find(x => x.Id == chainId) as ChainFact).backendURI))
            .ToList()
            .ForEach(oms => chainListArguments.Add(oms));

        List<MMTTerm> knownAvListArguments = new List<MMTTerm>();
        foreach(KeyValuePair<Fact, float> entry in knownAvMap)
        {
            Fact fact = entry.Key;
            float av = entry.Value;
            if (fact.GetType().Equals(typeof(CogwheelFact)))
            {
                List<MMTTerm> tupleArguments = new List<MMTTerm>
                {
                    new OMS((fact as CogwheelFact).backendURI),
                    new OMF(av)
                };
                    
                
                MMTTerm fixedAvTuple = new OMA(new OMS(MMTURIs.Tuple), tupleArguments);
                knownAvListArguments.Add(fixedAvTuple);
            }
        }

        MMTTerm cogList;
        cogList = new OMA(new OMS(MMTURIs.ListOf), cogListArguments);
        /*
        if (!(cogListArguments.Count == 0))
        {
            cogList = new OMA(new OMS(MMTURIs.ListOf), cogListArguments);
        }
        else
        {
            cogList = new OMS(MMTURIs.Nil);
        }
        */

        MMTTerm chainList;
        if (!(chainListArguments.Count == 0))
        {
            chainList = new OMA(new OMS(MMTURIs.ListOf), chainListArguments);
            Debug.Log("chainListArguments.Count > 0 in chneqsys");
        }
        else
        {
            List<MMTTerm> NiltypeArguments = new List<MMTTerm>
            {
                new OMS(MMTURIs.Chain)
            };
            chainList = new OMA(new OMS(MMTURIs.Nil), NiltypeArguments);
            Debug.Log("chainListArguments.Count = 0 in chneqsys");
        }

        MMTTerm knownAvList;
        knownAvList = new OMA(new OMS(MMTURIs.ListOf), knownAvListArguments);
        /*
        if (!(knownAvListArguments.Count == 0))
        {
            knownAvList = new OMA(new OMS(MMTURIs.ListOf), knownAvListArguments);
        }
        else
        {
            knownAvList = new OMS(MMTURIs.Nil);
        }
        */


        List<MMTTerm> eqsysArguments = new List<MMTTerm>
        {
            cogList,
            chainList,
            knownAvList
        };

        MMTTerm tp = new OMA(new OMS(MMTURIs.List), typeArguments);
        MMTTerm df = new OMA(new OMS(MMTURIs.CogChainEquationSystem), eqsysArguments);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return this.CogwheelIds;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            CogwheelEqsysFact p = (CogwheelEqsysFact)obj;
            return this.CogwheelIds.Equals(p.CogwheelIds);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        new List<int>(this.CogwheelIds).ForEach(x => hashcode ^= x);
        return hashcode;
    }
}


public class TestEqsysFact : Fact
{
    public int[] CogwheelIds;
    public int[] ChainIds;

    public TestEqsysFact(int i, int[] cogIds, int[] chainIds, Dictionary<Fact, float> knownAvMap, List<Fact> facts)
    {
        this.Id = i;
        this.CogwheelIds = cogIds;
        this.ChainIds = chainIds;

        List<MMTTerm> tupletypeArguments = new List<MMTTerm>
        {
           new OMS(MMTURIs.RealLit),
           new OMS(MMTURIs.RealLit)
        };
        List<MMTTerm> typeArguments = new List<MMTTerm>
        {
            //new OMA(new OMS(MMTURIs.Product), tupletypeArguments)
            new OMS(MMTURIs.Cogwheel)
        };

        List<int> cogwheelIdList = new List<int>(this.CogwheelIds);
        List<MMTTerm> cogListArguments = new List<MMTTerm>();
        cogwheelIdList
            .Select(cogwheelId =>
                new OMS((facts.Find(x => x.Id == cogwheelId) as CogwheelFact).backendURI))
            .ToList()
            .ForEach(oms => cogListArguments.Add(oms));

        List<int> chainlIdList = new List<int>(this.ChainIds);
        List<MMTTerm> chainListArguments = new List<MMTTerm>();
        chainlIdList
            .Select(chainId =>
                new OMS((facts.Find(x => x.Id == chainId) as ChainFact).backendURI))
            .ToList()
            .ForEach(oms => chainListArguments.Add(oms));

        List<MMTTerm> knownAvListArguments = new List<MMTTerm>();
        foreach (KeyValuePair<Fact, float> entry in knownAvMap)
        {
            Fact fact = entry.Key;
            float av = entry.Value;
            if (fact.GetType().Equals(typeof(CogwheelFact)))
            {
                List<MMTTerm> tupleArguments = new List<MMTTerm>
                {
                    new OMS((fact as CogwheelFact).backendURI),
                    new OMF(av)
                };


                MMTTerm fixedAvTuple = new OMA(new OMS(MMTURIs.Tuple), tupleArguments);
                knownAvListArguments.Add(fixedAvTuple);
            }
        }

        MMTTerm cogList;
        cogList = new OMA(new OMS(MMTURIs.ListOf), cogListArguments);
        /*
        if (!(cogListArguments.Count == 0))
        {
            cogList = new OMA(new OMS(MMTURIs.ListOf), cogListArguments);
        }
        else
        {
            cogList = new OMS(MMTURIs.Nil);
        }
        */

        MMTTerm chainList;
        if (!(chainListArguments.Count == 0))
        {
            chainList = new OMA(new OMS(MMTURIs.ListOf), chainListArguments);
        }
        else
        {
            List<MMTTerm> NiltypeArguments = new List<MMTTerm>
            {
                new OMS(MMTURIs.Chain)
            };
            chainList = new OMA(new OMS(MMTURIs.Nil), NiltypeArguments);
        }

        MMTTerm knownAvList;
        knownAvList = new OMA(new OMS(MMTURIs.ListOf), knownAvListArguments);
        /*
        if (!(knownAvListArguments.Count == 0))
        {
            knownAvList = new OMA(new OMS(MMTURIs.ListOf), knownAvListArguments);
        }
        else
        {
            knownAvList = new OMS(MMTURIs.Nil);
        }
        */

        
        /*List<MMTTerm> arguments = new List<MMTTerm>
        {
            cogList,
            chainList
        };*/

        List<MMTTerm> arguments;
        if (chainListArguments.Count > 0 )
        {
            arguments = new List<MMTTerm>
            {
                chainListArguments[0],
            };
            Debug.Log("chainListArguments.Count > 0");
        }
        else
        {           
            arguments = new List<MMTTerm>();
            Debug.Log("chainListArguments.Count = 0");         
        }
        
        /*List<MMTTerm> numList = new List<MMTTerm>
        {
            new OMA (new OMS(MMTURIs.Tuple), new List<MMTTerm>{ new OMF(1.0f), new OMF(1.5f)}),
            new OMA (new OMS(MMTURIs.Tuple), new List<MMTTerm>{ new OMF(2.0f), new OMF(2.5f)}),
            new OMA (new OMS(MMTURIs.Tuple), new List<MMTTerm>{ new OMF(3.0f), new OMF(3.5f)}),
            new OMA (new OMS(MMTURIs.Tuple), new List<MMTTerm>{ new OMF(4.0f), new OMF(4.5f)})
        };
        List<MMTTerm> arguments = new List<MMTTerm>
        {
            new OMA(new OMS(MMTURIs.ListOf), numList),
            new OMF(2.0f)
        };*/
        


        MMTTerm tp = new OMA(new OMS(MMTURIs.List), typeArguments);
        //MMTTerm df = new OMA(new OMS("http://mathhub.info/LoViVo?Test?Chain_List_Cogs_test"), arguments);
        MMTTerm df = new OMA(new OMS("http://mathhub.info/LoViVo?Test?chain_test"), arguments);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return this.CogwheelIds;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            CogwheelEqsysFact p = (CogwheelEqsysFact)obj;
            return this.CogwheelIds.Equals(p.CogwheelIds);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        new List<int>(this.CogwheelIds).ForEach(x => hashcode ^= x);
        return hashcode;
    }
}

public class ShaftFact : Fact
{
    public int[] CogwheelIds;


    public ShaftFact(int i, int[] ids, List<Fact> facts)
    {
        this.Id = i;
        this.CogwheelIds = ids;

        List<int> cogwheelIdList = new List<int>(this.CogwheelIds);
        List<MMTTerm> cogListArguments = new List<MMTTerm>();
        cogwheelIdList
            .Select(cogwheelId =>
                new OMS((facts.Find(x => x.Id == cogwheelId) as CogwheelFact).backendURI))
            .ToList()
            .ForEach(oms => cogListArguments.Add(oms));

        List<MMTTerm> shaftOfArgs = new List<MMTTerm>
        {
            new OMA(new OMS(MMTURIs.ListOf), cogListArguments),
            new OMF((float)this.Id)
        };

        MMTTerm tp = new OMS(MMTURIs.Shaft);
        MMTTerm df = new OMA(new OMS(MMTURIs.ShaftOf), shaftOfArgs);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return this.CogwheelIds;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ChainFact p = (ChainFact)obj;
            return this.CogwheelIds.Equals(p.CogwheelIds);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        new List<int>(this.CogwheelIds).ForEach(x => hashcode ^= x);
        return hashcode;
    }
}

public class MotorFact : Fact
{
    public int connectedShaftId;
    public float driveSpeed;


    public MotorFact(int id, int shaft_id, float vel, List<Fact> facts)
    {
        this.Id = id;
        this.connectedShaftId = shaft_id;
        this.driveSpeed = vel;

        OMS connectedShaft = new OMS((facts.Find(x => x.Id == connectedShaftId) as ShaftFact).backendURI);

        List<MMTTerm> motorOfArgs = new List<MMTTerm>
        {
            connectedShaft,
            new OMF(this.driveSpeed),
            new OMF((float)this.Id)
        };

        MMTTerm tp = new OMS(MMTURIs.Motor);
        MMTTerm df = new OMA(new OMS(MMTURIs.MotorOf), motorOfArgs);

        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        int[] dependantFacts = {this.connectedShaftId};
        return dependantFacts;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            MotorFact p = (MotorFact)obj;
            return this.connectedShaftId.Equals(p.connectedShaftId) && this.driveSpeed.Equals(p.driveSpeed);
        }
    }

    public override int GetHashCode()
    {
        int hashcode = 1;
        hashcode ^= this.connectedShaftId;
        hashcode ^= (int)this.driveSpeed;
        return hashcode;
    }
}


/*
public class OnLineFact : Fact
{
    //Id's of the Point and the Line it's on
    public int Pid, Rid;

    public OnLineFact(int i, int pid, int rid)
    {
        this.Id = i;
        this.Pid = pid;
        this.Rid = rid;
        PointFact pf = CommunicationEvents.Facts.Find((x => x.Id == pid)) as PointFact;
        RayFact rf = CommunicationEvents.Facts.Find((x => x.Id == rid)) as RayFact;
        string pURI = pf.backendURI;
        string rURI = rf.backendURI;

        //Set Label to StringConcatenation of Points
        this.Label = pf.Label + " ∈ " + rf.Label;

        List<MMTTerm> innerArguments = new List<MMTTerm>
        {
            new OMS(rURI),
            new OMS(pURI)
        };

        List<MMTTerm> outerArguments = new List<MMTTerm>
        {
            new OMA(new OMS(MMTURIs.OnLine), innerArguments)
        };

        //OMS constructor generates full URI
        MMTTerm tp = new OMA(new OMS(MMTURIs.Ded), outerArguments);
        MMTTerm df = null;

        //TODO: rework fact list + labeling
        MMTSymbolDeclaration mmtDecl = new MMTSymbolDeclaration(this.Label, tp, df);
        string body = MMTSymbolDeclaration.ToJson(mmtDecl);

        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public OnLineFact(int pid, int rid, string uri)
    {
        this.Pid = pid;
        this.Rid = rid;
        this.backendURI = uri;
    }

    public static OnLineFact parseFact(Scroll.ScrollFact fact)
    {
        String uri = fact.@ref.uri;
        String lineUri = ((OMS)((OMA)((OMA)((Scroll.ScrollSymbolFact)fact).tp).arguments[0]).arguments[0]).uri;
        String pointUri = ((OMS)((OMA)((OMA)((Scroll.ScrollSymbolFact)fact).tp).arguments[0]).arguments[1]).uri;
        if (CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(lineUri)) &&
            CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointUri)))
        {
            int pid = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointUri)).Id;
            int rid = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(lineUri)).Id;
            return new OnLineFact(pid, rid, uri);
        }
        //If dependent facts do not exist return null
        else
        {
            return null;
        }
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return new int[] { Pid, Rid };
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            OnLineFact o = (OnLineFact)obj;
            return this.Pid.Equals(o.Pid) && this.Rid.Equals(o.Rid);
        }
    }

    public override int GetHashCode()
    {
        return this.Pid ^ this.Rid;
    }
}


public class AngleFact : DirectedFact
{
    //Id's of the 3 Point-Facts, where Pid2 is the point, where the angle is
    public int Pid1, Pid2, Pid3;

    //only for temporary Use of AngleFacts
    public AngleFact() { }

    public AngleFact(int i, int pid1, int pid2, int pid3)
    {
        this.Id = i;
        this.Pid1 = pid1;
        this.Pid2 = pid2;
        this.Pid3 = pid3;
        PointFact pf1 = CommunicationEvents.Facts.Find((x => x.Id == pid1)) as PointFact;
        PointFact pf2 = CommunicationEvents.Facts.Find((x => x.Id == pid2)) as PointFact;
        PointFact pf3 = CommunicationEvents.Facts.Find((x => x.Id == pid3)) as PointFact;

        string p1URI = pf1.backendURI;
        string p2URI = pf2.backendURI;
        string p3URI = pf3.backendURI;
        float v = Vector3.Angle((pf1.Point - pf2.Point), (pf3.Point - pf2.Point));

        MMTDeclaration mmtDecl;

        if (Mathf.Abs(v - 90.0f) < 0.01)
        {
            v = 90.0f;
            //Label is currently set to Fact.setId
            //Set Label to StringConcatenation of Points
            this.Label = "⊾" + pf1.Label + pf2.Label + pf3.Label;
            mmtDecl = generate90DegreeAngleDeclaration(v, p1URI, p2URI, p3URI);
        }
        else
        {
            //Label is currently set to Fact.setId
            //Set Label to StringConcatenation of Points
            this.Label = "∠" + pf1.Label + pf2.Label + pf3.Label;
            mmtDecl = generateNot90DegreeAngleDeclaration(v, p1URI, p2URI, p3URI);
        }

        Debug.Log("angle: " + v);

        string body = MMTDeclaration.ToJson(mmtDecl);

        Debug.Log(body);
        AddFactResponse res = AddFactResponse.sendAdd(GameSettings.ServerAdress + "/fact/add", body);
        this.backendURI = res.uri;
        Debug.Log(this.backendURI);
    }

    public AngleFact(int Pid1, int Pid2, int Pid3, string backendURI)
    {
        this.Pid1 = Pid1;
        this.Pid2 = Pid2;
        this.Pid3 = Pid3;
        this.backendURI = backendURI;
    }

    public static AngleFact parseFact(Scroll.ScrollFact fact)
    {
        String uri;
        String pointAUri;
        String pointBUri;
        String pointCUri;
        int pid1;
        int pid2;
        int pid3;

        //If angle is not a 90Degree-Angle
        if (fact.GetType().Equals(typeof(Scroll.ScrollValueFact)))
        {
            uri = fact.@ref.uri;
            pointAUri = ((OMS)((OMA)((Scroll.ScrollValueFact)fact).lhs).arguments[0]).uri;
            pointBUri = ((OMS)((OMA)((Scroll.ScrollValueFact)fact).lhs).arguments[1]).uri;
            pointCUri = ((OMS)((OMA)((Scroll.ScrollValueFact)fact).lhs).arguments[2]).uri;
            //If dependent facts do not exist return null
            if (!CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointAUri)) |
                !CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointBUri)) |
                !CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointCUri)))
            {
                return null;
            }

            pid1 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointAUri)).Id;
            pid2 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointBUri)).Id;
            pid3 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointCUri)).Id;
        }
        //If angle is a 90Degree-Angle
        else
        {
            uri = fact.@ref.uri;
            pointAUri = ((OMS)((OMA)((OMA)((OMA)((Scroll.ScrollSymbolFact)fact).tp).arguments[0]).arguments[1]).arguments[0]).uri;
            pointBUri = ((OMS)((OMA)((OMA)((OMA)((Scroll.ScrollSymbolFact)fact).tp).arguments[0]).arguments[1]).arguments[1]).uri;
            pointCUri = ((OMS)((OMA)((OMA)((OMA)((Scroll.ScrollSymbolFact)fact).tp).arguments[0]).arguments[1]).arguments[2]).uri;
            //If dependent facts do not exist return null
            if (!CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointAUri)) |
                !CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointBUri)) |
                !CommunicationEvents.Facts.Exists(x => x.backendURI.Equals(pointCUri)))
            {
                return null;
            }

            pid1 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointAUri)).Id;
            pid2 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointBUri)).Id;
            pid3 = CommunicationEvents.Facts.Find(x => x.backendURI.Equals(pointCUri)).Id;
        }

        return new AngleFact(pid1, pid2, pid3, uri);
    }

    private MMTDeclaration generate90DegreeAngleDeclaration(float val, string p1URI, string p2URI, string p3URI)
    {

        MMTTerm argument = new OMA(
            new OMS(MMTURIs.Eq),
            new List<MMTTerm> {
                new OMS(MMTURIs.RealLit),
                new OMA(
                    new OMS(MMTURIs.Angle),
                    new List<MMTTerm> {
                        new OMS(p1URI),
                        new OMS(p2URI),
                        new OMS(p3URI)
                    }
                ),
                new OMF(val)
            }
        );

        MMTTerm tp = new OMA(new OMS(MMTURIs.Ded), new List<MMTTerm> { argument });
        MMTTerm df = null;

        return new MMTSymbolDeclaration(this.Label, tp, df);
    }

    private MMTDeclaration generateNot90DegreeAngleDeclaration(float val, string p1URI, string p2URI, string p3URI)
    {
        MMTTerm lhs =
            new OMA(
                new OMS(MMTURIs.Angle),
                new List<MMTTerm> {
                    new OMS(p1URI),
                    new OMS(p2URI),
                    new OMS(p3URI)
                }
            );

        MMTTerm valueTp = new OMS(MMTURIs.RealLit);
        MMTTerm value = new OMF(val);

        return new MMTValueDeclaration(this.Label, lhs, valueTp, value);
    }

    public override Boolean hasDependentFacts()
    {
        return true;
    }

    public override int[] getDependentFactIds()
    {
        return new int[] { Pid1, Pid2, Pid3 };
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            AngleFact a = (AngleFact)obj;
            return this.Pid1.Equals(a.Pid1) && this.Pid2.Equals(a.Pid2) && this.Pid3.Equals(a.Pid3);
        }
    }

    public override int GetHashCode()
    {
        return this.Pid1 ^ this.Pid2 ^ this.Pid3;
    }
}
*/
