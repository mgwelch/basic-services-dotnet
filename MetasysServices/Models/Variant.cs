using JohnsonControls.Metasys.BasicServices.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Variant is a structure that holds information about an attribute/property
    /// value from a single Metasys object.
    /// </summary>
    /// <remarks>
    /// If the returned property is an array of values the ArrayValue will hold
    /// each of these values in a new Variant object.
    /// </remarks>
    public class Variant
    {
        private const string Reliable = "reliabilityEnumSet.reliable";

        private const string Unsupported = "statusEnumSet.unsupportedObjectType";

        private const string Array = "dataTypeEnumSet.arrayDataType";

        /// <summary>The string representation of the item type.</summary>
        /// <value>
        /// Type of the item (e.g. 'string', 'numeric', 'boolean', 'object').
        /// </value>
        public string ItemType { private set; get; }

        /// <summary>The string representation of the value.</summary>
        /// <value>
        /// String value as specified in the MSSDA Bulletin stringValue or a translated string if
        /// a type of enumeration.
        /// </value>
        public string StringValue { private set; get; }

        /// <summary>The enumeration key of the StringValue.</summary>
        /// <value>
        /// The pre-translated StringValue as an enumeration key or the StringValue if it
        /// was not an enumeration key. Null if value is not a string, array, or unsupported type.
        /// </value>
        public string StringValueEnumerationKey { private set; get; }

        /// <summary>The numeric representation of the value.</summary>
        /// <value>
        /// Numeric value as specified in the MSSDA Bulletin rawValue.
        /// The numeric value for an Array will be 0.
        /// </value>
        public double NumericValue { private set; get; }

        /// <summary>The boolean representation of the value.</summary>
        /// <value>
        /// 0 if false, numeric value is 0, or attribute is non-numeric.
        /// 1 if true or numeric value not equal to 0.
        /// </value>
        public bool BooleanValue { private set; get; }

        /// <summary>The object representation of the value.</summary>
        /// <value>
        /// Object value specified as Json.
        /// </value>
        public object ObjectValue { private set; get; }

        /// <summary>An array of Variant values.</summary>
        /// <value>Null unless value is an array.</value>
        public Variant[] ArrayValue { private set; get; }

        /// <summary>The attribute from the Metasys object.</summary>
        public string Attribute { private set; get; }

        /// <summary>The id of the Metasys object.</summary>
        public ObjectId Id { private set; get; }

        /// <summary>The reliability of the value.</summary>
        /// <value>
        /// If the attribute is "presentValue": the reliability of the value.
        /// Otherwise reliable by default.
        /// </value>
        public string Reliability { private set; get; }

        /// <summary>The reliability enumeration key of the reliability.</summary>
        /// <value>
        /// If the attribute is "presentValue": the reliability enumeration key of the reliability.
        /// Otherwise reliabilityEnumSet.reliable by default.
        /// </value>
        public string ReliabilityEnumerationKey { private set; get; }

        /// <summary>The priority of the value.</summary>
        /// <value>
        /// If the attribute is "presentValue": the priority of the value.
        /// Otherwise null by default.
        /// </value>
        public string Priority { private set; get; }

        /// <summary>The priority enumeration key of the priority.</summary>
        /// <value>
        /// If the attribute is "presentValue": the priority enumeration key of the priority.
        /// Otherwise null by default.
        /// </value>
        public string PriorityEnumerationKey { private set; get; }

        /// <summary>Boolean representation of the reliability of the value.</summary>
        /// <value>True if the reliability of the value is reliable.</value>
        public bool IsReliable => ReliabilityEnumerationKey == Reliable;

        /// <summary>
        /// The Metasys API version taken as a reference to process the JToken.
        /// </summary>
        private ApiVersion apiVersion;

        private CultureInfo _CultureInfo;

        internal Variant()
        {
        }

        internal Variant(ObjectId id, JToken token, string attribute, CultureInfo cultureInfo, ApiVersion apiVersion)
        {
            this.apiVersion = apiVersion;
            _CultureInfo = cultureInfo;
            Id = id;
            Attribute = attribute;
            ReliabilityEnumerationKey = Reliable;
            Reliability = ResourceManager.Localize(Reliable, _CultureInfo);
            Priority = null;
            PriorityEnumerationKey = null;
            StringValueEnumerationKey = null;
            StringValue = null;
            NumericValue = 0;
            ArrayValue = null;
            BooleanValue = false;
            ProcessToken(token);
            if (apiVersion > ApiVersion.v2)
            {
                // From v3 onwards we may have condition property
                ProcessCondition(token);
            }
        }

        /// <summary>
        /// Parses the JToken type and assigns values as specified in the MSSDA Bulletin.
        /// </summary>
        private void ProcessToken(JToken token)
        {
            if (token == null || token["item"] == null || token["item"][Attribute] == null)
            {
                // return unsupported attribute result
                ItemType = "unsupported";
                NumericValue = 0;
                StringValueEnumerationKey = Unsupported;
                StringValue = ResourceManager.Localize(StringValueEnumerationKey, _CultureInfo);
                return;
            }
            JToken attributeToken = token["item"][Attribute];
            // switch on attributeToken type and set the fields appropriately
            switch (attributeToken.Type)
            {
                case JTokenType.Integer:
                    ItemType = "numeric";
                    NumericValue = attributeToken.Value<double>();
                    StringValue = NumericValue.ToString(_CultureInfo);
                    BooleanValue = Convert.ToBoolean(NumericValue);
                    ArrayValue = null;
                    break;
                case JTokenType.Float:
                    ItemType = "numeric";
                    NumericValue = attributeToken.Value<double>();
                    StringValue = NumericValue.ToString(_CultureInfo);
                    BooleanValue = Convert.ToBoolean(NumericValue);
                    break;
                case JTokenType.String:
                    ItemType = "string";
                    NumericValue = 0;
                    StringValueEnumerationKey = attributeToken.Value<string>();
                    StringValue = ResourceManager.Localize(StringValueEnumerationKey, _CultureInfo);
                    break;
                case JTokenType.Array:
                    ProcessArray(attributeToken);
                    break;
                case JTokenType.Boolean:
                    ItemType = "boolean";
                    if ((bool)(attributeToken) == true)
                    {
                        NumericValue = 1;
                        BooleanValue = true;
                        StringValue = Convert.ToString(BooleanValue, _CultureInfo);
                    }
                    else
                    {
                        NumericValue = 0;
                        BooleanValue = false;
                        StringValue = Convert.ToString(BooleanValue, _CultureInfo);
                    }
                    break;
                case JTokenType.Object:
                    // It is assumed the attribute read was the presentValue
                    if (apiVersion < ApiVersion.v3)
                    {
                        // From v3 onwards presentValue is threated like other attributes and additional information are moved in Condition property.
                        ProcessPresentValue(attributeToken);
                    }
                    else
                    {
                        ItemType = "object";
                        ObjectValue = attributeToken;
                    }
                    break;
                default:
                    NumericValue = 0;
                    StringValueEnumerationKey = Unsupported;
                    StringValue = ResourceManager.Localize(StringValueEnumerationKey, _CultureInfo);
                    ObjectValue = null;
                    break;
            }
        }

        /// <summary>Parses a JArray and adds each item as a Variant.</summary>
        private void ProcessArray(JToken token)
        {
            JArray arr = JArray.Parse(token.ToString());
            ArrayValue = new Variant[arr.Count];
            int index = 0;
            foreach (var item in arr.Children())
            {
                string json = "{\"item\": { }}";
                var t = JToken.Parse(json);
                t["item"][Attribute] = item;
                ArrayValue[index] = new Variant(Id, t, Attribute, _CultureInfo, apiVersion);
                index++;
            }
            NumericValue = 0;
            StringValueEnumerationKey = Array;
            StringValue = ResourceManager.Localize(StringValueEnumerationKey, _CultureInfo);
        }

        /// <summary>Searches the JObject for reliability and priority fields and uses the field called "value" as value for result.</summary>
        internal void ProcessPresentValue(JToken token)
        {
            if (Attribute.Equals("presentValue"))
            {
                JToken valueToken = token["value"];
                JToken reliabilityToken = token["reliability"];
                JToken priorityToken = token["priority"];

                if (reliabilityToken != null)
                {
                    ReliabilityEnumerationKey = reliabilityToken.ToString();
                    Reliability = ResourceManager.Localize(ReliabilityEnumerationKey, _CultureInfo);
                }
                if (priorityToken != null)
                {
                    PriorityEnumerationKey = priorityToken.ToString();
                    Priority = ResourceManager.Localize(PriorityEnumerationKey, _CultureInfo);
                }
                string json = "{\"item\":{}}";
                var t = JToken.Parse(json);
                t["item"][Attribute] = valueToken;
                ProcessToken(t);
            }
            else
            {
                ProcessToken(null);
            }
        }

        /// <summary>Retrieve non-normal conditions for the current attribute (if any). </summary>
        internal void ProcessCondition(JToken token)
        {
            var conditionToken = token["condition"];
            if (conditionToken == null)
            {
                return; // condition section not available
            }
            conditionToken = conditionToken[Attribute];
            if (conditionToken == null)
            {
                return; // condition is normal for this attribute
            }
            JToken reliabilityToken = conditionToken["reliability"];
            JToken priorityToken = conditionToken["priority"];

            if (reliabilityToken != null)
            {
                ReliabilityEnumerationKey = reliabilityToken.ToString();
                Reliability = ResourceManager.Localize(ReliabilityEnumerationKey, _CultureInfo);
            }
            if (priorityToken != null)
            {
                PriorityEnumerationKey = priorityToken.ToString();
                Priority = ResourceManager.Localize(PriorityEnumerationKey, _CultureInfo);
            }
        }

        /// <summary>
        /// Returns a value indicating whither this instance has values equal to a specified object.
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is Variant)
            {
                var other = (Variant)obj;
                bool areEqual = (((this.Id == null && other.Id == null) ||
                    (this.Id != null && this.Id.Equals(other.Id))) &&
                    ((this.Attribute == null && other.Attribute == null) ||
                        (this.Attribute != null && this.Attribute.Equals(other.Attribute))) &&
                    (this.NumericValue == other.NumericValue) &&
                    ((this.StringValueEnumerationKey == null && other.StringValueEnumerationKey == null) ||
                        (this.StringValueEnumerationKey != null &&
                            this.StringValueEnumerationKey.Equals(other.StringValueEnumerationKey))) &&
                    (this.BooleanValue == other.BooleanValue) &&
                    ((this.PriorityEnumerationKey == null && other.PriorityEnumerationKey == null) ||
                        (this.PriorityEnumerationKey != null &&
                            this.PriorityEnumerationKey.Equals(other.PriorityEnumerationKey))) &&
                    ((this.ReliabilityEnumerationKey == null && other.ReliabilityEnumerationKey == null) ||
                        (this.ReliabilityEnumerationKey != null &&
                            this.ReliabilityEnumerationKey.Equals(other.ReliabilityEnumerationKey))));
                if (areEqual)
                {
                    if (this.ArrayValue == null ^ other.ArrayValue == null)
                    {
                        return false;
                    }
                    else if (this.ArrayValue != null && other.ArrayValue != null)
                    {
                        for (int i = 0; i < this.ArrayValue.Length; i++)
                        {
                            if (!this.ArrayValue[i].Equals(other.ArrayValue[i]))
                            {
                                return false;
                            }
                        }
                    }
                }

                return areEqual;
            }
            return false;
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            var code = 13;
            code = (code * 7) + Id.GetHashCode();
            if (Attribute != null)
                code = (code * 7) + Attribute.GetHashCode();
            code = (code * 7) + NumericValue.GetHashCode();
            if (StringValueEnumerationKey != null)
                code = (code * 7) + StringValueEnumerationKey.GetHashCode();
            code = (code * 7) + BooleanValue.GetHashCode();
            if (PriorityEnumerationKey != null)
                code = (code * 7) + PriorityEnumerationKey.GetHashCode();
            if (ReliabilityEnumerationKey != null)
                code = (code * 7) + ReliabilityEnumerationKey.GetHashCode();
            var arrCode = 13;
            if (ArrayValue != null)
            {
                foreach (var item in ArrayValue)
                {
                    arrCode = (arrCode * 7) + item.GetHashCode();
                }
            }
            return code + arrCode;
        }

        /// <summary>
        /// Return a pretty JSON string of the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
