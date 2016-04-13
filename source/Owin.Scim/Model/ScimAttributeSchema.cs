namespace Owin.Scim.Model
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// A complex type that defines service provider attributes and their qualities.
    /// </summary>
    public sealed class ScimAttributeSchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScimAttributeSchema"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="description">The description.</param>
        /// <param name="multiValued">if set to <c>true</c> [multi valued].</param>
        /// <param name="mutability">The mutability.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="returned">The returned.</param>
        /// <param name="uniqueness">The uniqueness.</param>
        /// <param name="caseExact">if set to <c>true</c> [case exact].</param>
        /// <param name="subAttributes">The sub attributes.</param>
        /// <param name="canonicalValues">The canonical values.</param>
        /// <param name="referenceTypes">The reference types.</param>
        public ScimAttributeSchema(
            string name, 
            ScimDataType dataType, 
            string description, 
            bool multiValued, 
            string mutability, 
            bool required, 
            string returned, 
            string uniqueness, 
            bool caseExact, 
            IEnumerable<ScimAttributeSchema> subAttributes, 
            ISet<object> canonicalValues, 
            IEnumerable<string> referenceTypes)
        {
            Name = name;
            Type = dataType;
            Description = description;
            MultiValued = multiValued;
            Mutability = mutability;
            Required = required;
            Returned = returned;
            ReferenceTypes = referenceTypes;
            SubAttributes = subAttributes;
            Uniqueness = uniqueness;
            CaseExact = caseExact;
            CanonicalValues = canonicalValues;
        }

        /// <summary>
        /// Gets the collection of suggested canonical values that MAY be used(e.g., "work" and "home").
        /// </summary>
        /// <value>The canonical values.</value>
        [JsonProperty("canonicalValues")]
        public ISet<object> CanonicalValues { get; private set; }

        /// <summary>
        /// Gets a boolean value that specifies whether or not a string attribute is case sensitive.
        /// </summary>
        /// <value><c>true</c> if [case exact]; otherwise, <c>false</c>.</value>
        [JsonProperty("caseExact")]
        public bool CaseExact { get; private set; }

        /// <summary>
        /// Gets the attribute's human-readable description.
        /// </summary>
        /// <value>The description.</value>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Gets a boolean value indicating the attribute's plurality.
        /// </summary>
        /// <value><c>true</c> if [multi valued]; otherwise, <c>false</c>.</value>
        [JsonProperty("multiValued")]
        public bool MultiValued { get; private set; }

        /// <summary>
        /// Gets a single keyword indicating the circumstances under which the value of the attribute can be (re)defined.
        /// </summary>
        /// <value>The mutability.</value>
        [JsonProperty("mutability")]
        public string Mutability { get; private set; }

        /// <summary>
        /// Gets the attribute's name.
        /// </summary>
        /// <value>The name.</value>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a boolean value that specifies whether or not the attribute is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        [JsonProperty("required")]
        public bool Required { get; private set; }

        /// <summary>
        /// Gets a single keyword that indicates when an attribute and associated values are returned in response to a GET request or in response to a PUT, POST, or PATCH request.
        /// </summary>
        /// <value>The returned.</value>
        [JsonProperty("returned")]
        public string Returned { get; private set; }

        /// <summary>
        /// Gets a multi-valued array of JSON strings that indicate the SCIM resource types that may be referenced.
        /// </summary>
        /// <value>The reference types.</value>
        [JsonProperty("referenceTypes")]
        public IEnumerable<string> ReferenceTypes { get; private set; }

        /// <summary>
        /// Gets the sub-attributes schema definitions for a complex type.
        /// </summary>
        /// <value>The sub-attribute schema definitions.</value>
        [JsonProperty("subAttributes")]
        public IEnumerable<ScimAttributeSchema> SubAttributes { get; private set; }

        /// <summary>
        /// Gets the attribute's data type.
        /// </summary>
        /// <value>The data type.</value>
        [JsonProperty("type")]
        public string Type { get; private set; }

        /// <summary>
        /// Gets a single keyword value that specifies how the service provider enforces uniqueness of attribute values.
        /// </summary>
        /// <value>The uniqueness.</value>
        [JsonProperty("uniqueness")]
        public string Uniqueness { get; private set; }

        public bool ShouldSerializeCaseExact()
        {
            return Type == ScimDataType.String;
        }

        public bool ShouldSerializeReferenceTypes()
        {
            return Type == ScimDataType.Reference;
        }
    }
}