using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// A helper to work with Metasys Objects collections.
    /// </summary>
    public static class MetasysObjectCollectionExtension
    {
        /// <summary>
        /// Returns the first MetasysObject in the collection with the given name.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name">The name of the Metasys object.</param>
        /// <returns></returns>
        public static MetasysObject FindByName(this IEnumerable<MetasysObject> source, string name)
        {
            var metasysObject = source.FirstOrDefault(f => f.Name == name);
            if (metasysObject == null)
            {
                throw new Exception($"Metasys object not found in the collection ({name}).");
            }
            return metasysObject;
        }

        /// <summary>
        /// Returns the MetasysObject in the collection with the given ID.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id">The Id of the Metasys Object.</param>
        /// <returns></returns>
        public static MetasysObject FindById(this IEnumerable<MetasysObject> source, ObjectId id)
        {
            var metasysObject = source.FirstOrDefault(f => f.Id == id);
            if (metasysObject == null)
            {
                throw new Exception($"Metasys object not found in the collection ({id}).");
            }
            return metasysObject;
        }
    }
}
