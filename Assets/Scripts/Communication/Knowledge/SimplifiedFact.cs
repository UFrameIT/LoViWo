using System.Collections;
using System.Collections.Generic;
using static JSONManager;
using JsonSubTypes;
using Newtonsoft.Json;

/*
 * SimplifiedFact: Class used for deserialization of ServerResponses
 */
[JsonConverter(typeof(JsonSubtypes), "kind")]
[JsonSubtypes.KnownSubType(typeof(SSymbolFact), "general")]
[JsonSubtypes.KnownSubType(typeof(SValueFact), "veq")]
[JsonSubtypes.KnownSubType(typeof(SEqsysFact), "eqsys")]
public abstract class SimplifiedFact
{
    public string kind;
    public UriReference @ref;
    public string label;

    public static List<SimplifiedFact> FromJSON(string json)
    {
        List<SimplifiedFact> sFacts = JsonConvert.DeserializeObject<List<SimplifiedFact>>(json);
        return sFacts;
    }
}

public class UriReference
{
    public string uri;

    public UriReference(string uri)
    {
        this.uri = uri;
    }
}

/**
    * Class used for deserializing incoming symbol-declarations from mmt
    */
public class SSymbolFact : SimplifiedFact
{
    public MMTTerm tp;
    public MMTTerm df;
}

/**
* Class used for deserializing incoming value-declarations from mmt
*/
public class SValueFact : SimplifiedFact
{
    public MMTTerm lhs;
    public MMTTerm valueTp;
    public MMTTerm value;
    public MMTTerm proof;
}

/**
* Class used for deserializing incoming eqsys-declarations from mmt
*/
public class SEqsysFact : SSymbolFact
{
    public List<List<MMTTerm>> equations;
}
