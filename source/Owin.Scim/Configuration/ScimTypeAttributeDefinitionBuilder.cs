namespace Owin.Scim.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using Canonicalization;

    using Extensions;

    using FluentValidation;

    using Model;

    using Newtonsoft.Json;

    public abstract class ScimTypeAttributeDefinitionBuilder<T, TAttribute> : IScimTypeAttributeDefinition
    {
        private readonly ScimTypeDefinitionBuilder<T> _DeclaringTypeDefinition;

        private readonly PropertyDescriptor _PropertyDescriptor;

        private readonly IList<ICanonicalizationRule> _CanonicalizationRules; 

        protected ScimTypeAttributeDefinitionBuilder(
            ScimTypeDefinitionBuilder<T> typeDefinition,
            PropertyDescriptor propertyDescriptor,
            bool multiValued)
        {
            _DeclaringTypeDefinition = typeDefinition;
            _PropertyDescriptor = propertyDescriptor;
            _CanonicalizationRules = new List<ICanonicalizationRule>();

            // Initialize defaults
            CaseExact = false;
            MultiValued = multiValued;
            Mutability = Mutability.ReadWrite;
            Required = false;
            Returned = Returned.Default;
            Uniqueness = Uniqueness.None;

            var descriptionAttr = propertyDescriptor
                .Attributes
                .Cast<Attribute>()
                .SingleOrDefault(attr => attr is DescriptionAttribute) as DescriptionAttribute;

            if (descriptionAttr != null)
            {
                Description = descriptionAttr.Description.RemoveMultipleSpaces();
            }
        }

        public string Name
        {
            get
            {
                var jsonPropertyAttribute = _PropertyDescriptor.Attributes.OfType<JsonPropertyAttribute>().SingleOrDefault();
                if (jsonPropertyAttribute != null) return jsonPropertyAttribute.PropertyName;

                return _PropertyDescriptor.Name.LowercaseFirstCharacter();
            }
        }

        public ISet<object> CanonicalValues { get; protected set; }

        public bool CaseExact { get; protected set; }

        public string Description { get; protected set; }

        public bool MultiValued { get; protected set; }

        public Mutability Mutability { get; protected set; }

        public IEnumerable<string> ReferenceTypes { get; protected set; }

        public bool Required { get; protected set; }

        public Returned Returned { get; protected set; }

        public Uniqueness Uniqueness { get; protected set; }

        public PropertyDescriptor AttributeDescriptor
        {
            get { return _PropertyDescriptor; }
        }

        public virtual IScimTypeDefinition DeclaringTypeDefinition
        {
            get { return _DeclaringTypeDefinition; }
        }

        protected internal IEqualityComparer CanonicalValueComparer { get; protected set; }

        public IEnumerable<ICanonicalizationRule> GetCanonicalizationRules()
        {
            return _CanonicalizationRules;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetMutability(Mutability mutability)
        {
            Mutability = mutability;
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetRequired(bool required)
        {
            Required = required;
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetReturned(Returned returned)
        {
            Returned = returned;
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetUniqueness(Uniqueness uniqueness)
        {
            Uniqueness = uniqueness;
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> AddCanonicalizationRule(CanonicalizationAction<TAttribute> rule)
        {
            var func = new StatefulCanonicalizationFunc<TAttribute>(
                (TAttribute value, ref object state) =>
                {
                    rule.Invoke(value);
                    return value;
                });

            return AddCanonicalizationRule(func);
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> AddCanonicalizationRule(CanonicalizationFunc<TAttribute> rule)
        {
            var func = new StatefulCanonicalizationFunc<TAttribute>((TAttribute value, ref object state) => rule.Invoke(value));

            return AddCanonicalizationRule(func);
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> AddCanonicalizationRule(StatefulCanonicalizationAction<TAttribute> rule)
        {
            var func = new StatefulCanonicalizationFunc<TAttribute>(
                (TAttribute value, ref object state) =>
                {
                    rule.Invoke(value, ref state);
                    return value;
                });

            return AddCanonicalizationRule(func);
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> AddCanonicalizationRule(StatefulCanonicalizationFunc<TAttribute> rule)
        {
            _CanonicalizationRules.Add(new CanonicalizationRule<TAttribute>(_PropertyDescriptor, rule));
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> ClearCanonicalizationRules()
        {
            _CanonicalizationRules.Clear();
            return this;
        }

        public ScimTypeAttributeDefinitionBuilder<T, TAttribute> SetCanonicalValues(
            IEnumerable<TAttribute> acceptableValues,
            EqualityComparer<TAttribute> comparer = null)
        {
            if (comparer == null)
                comparer = EqualityComparer<TAttribute>.Default;

            CanonicalValues = new HashSet<object>(acceptableValues.Distinct(comparer).Cast<object>());
            CanonicalValueComparer = comparer;

            return this;
        }
    }
}