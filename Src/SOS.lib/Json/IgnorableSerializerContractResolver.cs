using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SOS.Lib.Json
{
    /// <summary>
    /// JsonConvert resolver that allows you to ignore properties.
    /// </summary>
    public class IgnorableSerializerContractResolver : DefaultContractResolver
    {
        protected readonly Dictionary<Type, HashSet<string>> Ignores = new Dictionary<Type, HashSet<string>>();
        protected readonly Dictionary<Type, DefaultValueHandling> KeepTypesWithDefaultValue = new Dictionary<Type, DefaultValueHandling>();

        /// <summary>
        /// Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void Ignore(Type type, params string[] propertyName)
        {
            // start bucket if DNE
            if (!Ignores.ContainsKey(type)) Ignores[type] = new HashSet<string>();

            foreach (var prop in propertyName)
            {
                Ignores[type].Add(prop);
            }
        }

        /// <summary>
        /// Is the given property for the given type ignored?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsIgnored(Type type, string propertyName)
        {
            if (!Ignores.ContainsKey(type)) return false;

            // if no properties provided, ignore the type entirely
            if (Ignores[type].Count == 0) return true;

            return Ignores[type].Contains(propertyName);
        }

        /// <summary>
        /// The decision logic goes here
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            
            // Exclude ignored properties
            if (IsIgnored(property.DeclaringType, property.PropertyName)
                || IsIgnored(property.DeclaringType.BaseType, property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            // Preserve specified properties even though they have a default value
            if (KeepTypesWithDefaultValue.TryGetValue(property.PropertyType, out var handling))
            {
                property.DefaultValueHandling = handling;
            }
            else if(KeepTypesWithDefaultValue.TryGetValue(property.DeclaringType, out handling))
            {
                property.DefaultValueHandling = handling;
            }

            return property;
        }

        public IgnorableSerializerContractResolver KeepTypeWithDefaultValue(Type type)
        {
            KeepTypesWithDefaultValue.Add(type, DefaultValueHandling.Include);
            return this;
        }


        public IgnorableSerializerContractResolver Ignore<TModel>(Expression<Func<TModel, object>> selector)
        {
            MemberExpression body = selector.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)selector.Body;
                body = ubody.Operand as MemberExpression;

                if (body == null)
                {
                    throw new ArgumentException("Could not get property name", nameof(selector));
                }
            }

            string propertyName = body.Member.Name;
            Ignore(body.Member.DeclaringType, propertyName);
            return this;
        }
    }
}