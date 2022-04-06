using System.Linq;
using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MMTURICollection
{
    public string Cogwheel = "http://mathhub.info/LoViVo?Cogwheel3D?cogwheel";
    public string CogwheelOf = "http://mathhub.info/LoViVo?Cogwheel3D?cogwheelOf";
    public string Nil = "http://gl.mathhub.info/MMT/LFX/Datatypes?ListSymbols?nil_constant";
    public string List = "http://gl.mathhub.info/MMT/LFX/Datatypes?ListSymbols?ListType";
    public string ListOf = "http://gl.mathhub.info/MMT/LFX/Datatypes?ListSymbols?list";
    public string CogwheelEquationSystem = "http://mathhub.info/LoViVo?Cogwheel3D?eqsys";
    public string Product = "http://gl.mathhub.info/MMT/LFX/Sigma?Symbols?Product";
    public string Tuple = "http://gl.mathhub.info/MMT/LFX/Sigma?Symbols?Tuple";
    public string Prop = "http://mathhub.info/MitM/Foundation?Logic?prop";

    public string CogwheelCogwheelInteraction = "http://mathhub.info/LoViVo?Cogwheel3D?cog_cog_interlocking";
    public string DeclareCogwheelCogwheelInteraction = "http://mathhub.info/LoViVo?Cogwheel3D?declare_cog_cog_interlocking";

    public string Chain = "http://mathhub.info/LoViVo?Chain?chain";
    public string ChainOf = "http://mathhub.info/LoViVo?Chain?chainOf";
    public string Convex = "http://mathhub.info/LoViVo?Chain?convex";
    public string Concarve = "http://mathhub.info/LoViVo?Chain?concarve";
    public string ChainVelocity = "http://mathhub.info/LoViVo?Chain?chain_velovity";
    public string CogChainEquationSystem = "http://mathhub.info/LoViVo?Chain?eqsys2";

    public string CogwheelChainInteractionCovex = "http://mathhub.info/LoViVo?Chain?cog_chain_interlocking_conv";
    public string DeclareCogwheelChainInteractionCovex = "http://mathhub.info/LoViVo?Chain?declare_cog_chain_interlocking_conv";
    public string CogwheelChainInteractionCocarve = "http://mathhub.info/LoViVo?Chain?cog_chain_interlocking_conc";
    public string DeclareCogwheelChainInteractionCocarve = "http://mathhub.info/LoViVo?Chain?declare_cog_chain_interlocking_conc";


    public string Shaft = "http://mathhub.info/LoViVo?Shaft?shaft";
    public string ShaftOf = "http://mathhub.info/LoViVo?Shaft?create_shaft";
    public string ShaftAngularVelocity = "http://mathhub.info/LoViVo?Shaft?shaft_angular_velocity";

    public string ShaftCogwheelInterlocking = "http://mathhub.info/LoViVo?Shaft?shaft_cog_interlocking";
    public string DeclareShaftCogwheelInterlocking = "http://mathhub.info/LoViVo?Shaft?declare_shaft_cog_interlocking";

    public string Motor = "http://mathhub.info/LoViVo?Motor?motor";
    public string MotorOf = "http://mathhub.info/LoViVo?Motor?create_motor";

    public string MotorShaftInterlocking = "http://mathhub.info/LoViVo?Motor?Motor_shaft_interlocking";
    public string DeclareMotorShaftInterlocking = "http://mathhub.info/LoViVo?Motor?declare_motor_shaft_interlocking";

    public string GearboxEquationSystem = "http://mathhub.info/LoViVo?Gearbox?gearbox_eqsys";
    public string GearboxEquationSystem2 = "http://mathhub.info/LoViVo?Gearbox?gearbox_eqsys2";

    public string TestEquationSystem = "http://mathhub.info/LoViVo?Test?TestEqsys";

    public string Record = "http://gl.mathhub.info/MMT/LFX/Records?Symbols?Recexp";
    public string CogwheelAngularVelocity = "http://mathhub.info/LoViVo?Cogwheel3D?angular_velocity";
    public string Addition = "http://mathhub.info/MitM/Foundation?RealLiterals?plus_real_lit";
    public string Multiplication = "http://mathhub.info/MitM/Foundation?RealLiterals?times_real_lit";
    public string Minus = "http://mathhub.info/MitM/Foundation?RealLiterals?minus_real_lit";
    
    public string Point = "http://mathhub.info/MitM/core/geometry?3DGeometry?point";
    public string LineType = "http://mathhub.info/MitM/core/geometry?Geometry/Common?line_type";
    public string LineOf = "http://mathhub.info/MitM/core/geometry?Geometry/Common?lineOf";
    public string OnLine = "http://mathhub.info/MitM/core/geometry?Geometry/Common?onLine";
    public string Ded = "http://mathhub.info/MitM/Foundation?Logic?ded";
    public string Eq = "http://mathhub.info/MitM/Foundation?Logic?eq";
    public string Metric = "http://mathhub.info/MitM/core/geometry?Geometry/Common?metric";
    public string Angle = "http://mathhub.info/MitM/core/geometry?Geometry/Common?angle_between";
    public string Sketch = "http://mathhub.info/MitM/Foundation?InformalProofs?proofsketch";
    public string RealLit = "http://mathhub.info/MitM/Foundation?RealLiterals?real_lit";
}

public static class JSONManager
{
    //could init the strings of MMTURIs with JSON or other settings file instead
    public static MMTURICollection MMTURIs = new MMTURICollection();

    [JsonConverter(typeof(JsonSubtypes), "kind")]
    public class MMTTerm
    {
        public string kind;

        public override bool Equals(object obj)
        {
            if (!(obj is MMTTerm)) return false;

            MMTTerm term = (MMTTerm)obj;
            return kind.Equals(term.kind);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode();
        }

        public bool isSimplifiedCogwheelAvTerm()
        {
            return this.GetType().Equals(typeof(OMA))
                        && ((OMA)this).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)this).applicant).uri.Equals(MMTURIs.CogwheelAngularVelocity)
                        && ((OMA)this).arguments.Count == 1
                        && ((OMA)this).arguments.ElementAt(0).GetType().Equals(typeof(OMA))
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)((OMA)this).arguments.ElementAt(0)).applicant).uri.Equals(MMTURIs.Record)
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.Count == 8
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0).GetType().Equals(typeof(RECARG))
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0)).name.Equals("pitchRadius")
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0)).value.GetType().Equals(typeof(OMF))
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3).GetType().Equals(typeof(RECARG))
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).name.Equals("position")
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value.GetType().Equals(typeof(OMA))
                        && ((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).applicant).uri.Equals(MMTURIs.Tuple)
                        && ((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.Count == 3
                        && ((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(0).GetType().Equals(typeof(OMF))
                        && ((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(1).GetType().Equals(typeof(OMF))
                        && ((OMA)((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(3)).value).arguments.ElementAt(2).GetType().Equals(typeof(OMF));
        }

        public bool isSimplifiedChainCvTerm()
        {
            return this.GetType().Equals(typeof(OMA))
                        && ((OMA)this).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)this).applicant).uri.Equals(MMTURIs.ChainVelocity)
                        && ((OMA)this).arguments.Count == 1
                        && ((OMA)this).arguments.ElementAt(0).GetType().Equals(typeof(OMA))
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)((OMA)this).arguments.ElementAt(0)).applicant).uri.Equals(MMTURIs.Record)
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.Count == 2
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0).GetType().Equals(typeof(RECARG))
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0)).name.Equals("tuple_list")
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(1).GetType().Equals(typeof(RECARG))
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(1)).name.Equals("ID");                    
        }

        public bool isSimplifiedShaftAvTerm()
        {
            return this.GetType().Equals(typeof(OMA))
                        && ((OMA)this).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)this).applicant).uri.Equals(MMTURIs.ShaftAngularVelocity)
                        && ((OMA)this).arguments.Count == 1
                        && ((OMA)this).arguments.ElementAt(0).GetType().Equals(typeof(OMA))
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).applicant.GetType().Equals(typeof(OMS))
                        && ((OMS)((OMA)((OMA)this).arguments.ElementAt(0)).applicant).uri.Equals(MMTURIs.Record)
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.Count == 1
                        && ((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0).GetType().Equals(typeof(RECARG))
                        && ((RECARG)((OMA)((OMA)this).arguments.ElementAt(0)).arguments.ElementAt(0)).name.Equals("ID");
        }

    }

    public class OMA : MMTTerm
    {
        public MMTTerm applicant;
        public List<MMTTerm> arguments;

        public OMA(MMTTerm applicant, List<MMTTerm> arguments)
        {
            this.applicant = applicant;
            this.arguments = arguments;
            this.kind = "OMA";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OMA)) return false;

            OMA oma = (OMA)obj;
            return kind.Equals(oma.kind)
                    && applicant.Equals(oma.applicant)
                    && arguments.SequenceEqual(oma.arguments);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ applicant.GetHashCode() ^ arguments.Aggregate(0, (total, next) => total ^= next.GetHashCode());
        }
    }

    public class RECARG : MMTTerm
    {
        public string name;
        public MMTTerm value;

        public RECARG(string name, MMTTerm value)
        {
            this.name = name;
            this.value = value;
            this.kind = "RECARG";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RECARG)) return false;

            RECARG recarg = (RECARG)obj;
            return kind.Equals(recarg.kind)
                    && name.Equals(recarg.name)
                    && value.Equals(recarg.value);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ name.GetHashCode() ^ value.GetHashCode();
        }
    }

    public class OMS : MMTTerm
    {
        public string uri;

        public OMS(string uri)
        {
            this.uri = uri;
            this.kind = "OMS";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OMS)) return false;

            OMS oms = (OMS)obj;
            return kind.Equals(oms.kind)
                    && uri.Equals(oms.uri);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ uri.GetHashCode();
        }
    }

    public class OMSTR : MMTTerm
    {
        [JsonProperty("string")]
        public string s;

        public OMSTR(string s)
        {
            this.s = s;
            this.kind = "OMSTR";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OMSTR)) return false;

            OMSTR omstr = (OMSTR)obj;
            return kind.Equals(omstr.kind)
                    && s.Equals(omstr.s);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ s.GetHashCode();
        }
    }


    public class OMF : MMTTerm
    {
        [JsonProperty("float")]
        public float f;

        public OMF(float f)
        {
            this.f = f;
            this.kind = "OMF";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is OMF)) return false;

            OMF omf = (OMF)obj;
            return kind.Equals(omf.kind)
                    && f.Equals(omf.f);
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode() ^ f.GetHashCode();
        }
    }

    public class MMTDeclaration
    {
        public static MMTDeclaration FromJson(string json)
        {
            MMTDeclaration mmtDecl = JsonConvert.DeserializeObject<MMTDeclaration>(json);
            return mmtDecl;
        }
        public static string ToJson(MMTDeclaration mmtDecl)
        {
            string json = JsonConvert.SerializeObject(mmtDecl);
            return json;
        }
    }

    /**
     * MMTSymbolDeclaration: Class for facts without values, e.g. Points
     */
    public class MMTSymbolDeclaration : MMTDeclaration
    {
        public string kind = "general";
        public string label;
        public MMTTerm tp;
        public MMTTerm df;

        /**
         * Constructor used for sending new declarations to mmt
         */
        public MMTSymbolDeclaration(string label, MMTTerm tp, MMTTerm df)
        {
            this.label = label;
            this.tp = tp;
            this.df = df;
        }
    }

    /**
     * MMTValueDeclaration: Class for facts with values, e.g. Distances or Angles
     */
    public class MMTValueDeclaration : MMTDeclaration
    {
        public string kind = "veq";
        public string label;
        public MMTTerm lhs;
        public MMTTerm valueTp;
        public MMTTerm value;

        /**
         * Constructor used for sending new declarations to mmt
         */
        public MMTValueDeclaration(string label, MMTTerm lhs, MMTTerm valueTp, MMTTerm value)
        {
            this.label = label;
            this.lhs = lhs;
            this.valueTp = valueTp;
            this.value = value;
        }
    }

}