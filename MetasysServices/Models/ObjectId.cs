using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Policy;

namespace JohnsonControls.Metasys.BasicServices
{

    /// <summary>
    /// A Metasys Object Identifier
    /// </summary>
    /// <remarks>
    /// Wherever an ObjectId is required you can pass a string instead.
    ///
    /// This is because this is just a token type to signify that an object identifier is required.
    /// As such it has implicit conversions to/from string.
    /// </remarks>
    public record struct ObjectId(string Value) : IEquatable<ObjectId>, IEquatable<string>, IEquatable<Guid>
    {

        /// <summary>
        /// Creates a default instance of <see cref="ObjectId"/> that is synonymous
        /// with an empty string.
        /// </summary>
        public ObjectId() : this("")
        {
        }

        /// <summary>
        /// Implicitly convert an <see cref="ObjectId"/> to <see cref="string"/>
        /// </summary>
        /// <param name="objectId"></param>
        public static implicit operator string(ObjectId objectId)
        {
            return objectId.Value;
        }

        /// <summary>
        /// Implicitly convert a <see cref="string"/> to <see cref="ObjectId"/>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ObjectId(string value)
        {
            return new ObjectId(value);
        }

        /// <summary>
        /// Implicitly convert a <see cref="Guid"/> to <see cref="ObjectId"/>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ObjectId(Guid value)
        {
            return new ObjectId(value.ToString());
        }

        /// <summary>
        /// <s>Implicitly convert a <see cref="ObjectId"/> to <see cref="Guid"/></s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated.</b> This method is not guaranteed to always succeed
        /// in the future. While Metasys object identifiers have always safely converted to Guids
        /// they make no guarantee that they will always. Nor does the REST API spec document
        /// them as Guids.
        /// <para>
        /// To avoid issues, use methods from this library that take an <see cref="ObjectId"/> for
        /// object identifiers. You can even pass strings to those methods as strings will implicitly
        /// convert to ObjectId.
        /// </para>
        /// </remarks>
        /// <param name="objectId"></param>
        // As of writing this should never throw as Metasys currently uses
        // guids for identifiers. But it doesn't advertise this and doesn't
        // guarantee it. Which is why the Guid operators have been deprecated
        // and why this class was created.
        // This operator is as safe as using Guids in all of the demo apps
        // today. Any one of them could throw if an object id was goofed up.
        [Obsolete("This is not guaranteed to always succeed. Please use methods that use strings or ObjectIds for object identifiers instead.")]
        public static implicit operator Guid(ObjectId objectId)
        {
            if (Guid.TryParse(objectId, out Guid value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(nameof(objectId), "Value must contain a valid guid.");
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return Value;
        }


        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(ObjectId other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(string other)
        {
            return Value == other;
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(Guid other)
        {
            return other.ToString() == Value;
        }

    }
}
