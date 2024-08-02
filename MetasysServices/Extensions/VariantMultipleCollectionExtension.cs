using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// A helper to work with VariantMultiple collections.
    /// </summary>
    public static class VariantMultipleCollectionExtension
    {

        /// <summary>
        /// Returns the Variant object of the collection with the given attribute name.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name">The name of the attribute</param>
        /// <returns></returns>
        public static Variant FindAttributeByName(this VariantMultiple source, string name)
        {
            var value = source.Values.SingleOrDefault(s => s.Attribute == name);
            return value ?? throw new MetasysException($"Attribute not found in the Variant collection ({name}).");
        }

        /// <summary>
        /// Returns the VariantMultiple in the collection with the given ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id">The Id of the Metasys Object.</param>
        /// <returns></returns>
        public static VariantMultiple FindById(this IEnumerable<VariantMultiple> source, ObjectId id)
        {
            var multiples = source.FirstOrDefault(f => f.Id == id);
            return multiples ?? throw new Exception($"VariantMultiple not found in the collection ({id}).");
        }
    }
}
