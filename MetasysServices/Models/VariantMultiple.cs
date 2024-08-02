using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// A structure for grouping Variant values with the same id.
    /// </summary>
    public class VariantMultiple
    {
        /// <summary>The object id.</summary>
        public ObjectId Id { private set; get; }

        /// <summary>The list of Variants.</summary>
        public IEnumerable<Variant> Values { set; get; }

        internal VariantMultiple(ObjectId id, IEnumerable<Variant> values)
        {
            Id = id;
            Values = values;
        }

        /// <summary>
        /// Returns a value indicating whither this instance has values equal to a specified object.
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is VariantMultiple)
            {
                var other = (VariantMultiple)obj;
                bool areEqual = this.Id.Equals(other.Id);
                if (areEqual)
                {
                    if (this.Values == null ^ other.Values == null)
                    {
                        return false;
                    }
                    else if (this.Values != null && other.Values != null)
                    {
                        return Enumerable.SequenceEqual(this.Values, other.Values);
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
            if (Values != null)
            {
                var arrCode = 0;
                foreach (var item in Values)
                {
                    arrCode += item.GetHashCode();
                }
                code = (code * 7) + arrCode;
            }
            return code;
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
