﻿using System;
using Grace.Data;
using System.Reflection;
using System.Linq;
using Grace.Utilities;

namespace Grace.DependencyInjection.Impl
{
    /// <summary>
    /// Represents a configuration for a type filter
    /// </summary>
    public class TypesThatConfiguration : GenericFilterGroup<Type>
    {

        /// <summary>
        /// Creates a type filter that returns true if a type has a particular property name
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration HaveProperty(string propertyName)
        {
            return HaveProperty(null, propertyName);
        }

        /// <summary>
        /// Creates a type filter that returns true if a type has a particular property name
        /// </summary>
        /// <typeparam name="T">property type</typeparam>
        /// <param name="propertyName">property name</param>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration HaveProperty<T>(string propertyName = null)
        {
            return HaveProperty(typeof(T), propertyName);
        }

        /// <summary>
        /// Creates a type filter that returns true if a type has a particular property name
        /// </summary>
        /// <param name="propertyType">property type</param>
        /// <param name="propertyName">property name</param>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration HaveProperty(Type propertyType, string propertyName = null)
        {
            bool notValue = GetNotAndingValue();

            if (propertyType == null)
            {
                Add(t => t.GetRuntimeProperties().Any(x => x.Name == propertyName) == notValue);
            }
            else
            {
                Type tempType = propertyType;

                Add(
                    t => t.GetRuntimeProperties().Any(
                        x => ReflectionService.CheckTypeIsBasedOnAnotherType(x.PropertyType, tempType) &&
                             x.Name == propertyName) == notValue);
            }

            return this;
        }

        /// <summary>
        /// Filters types based on a particular
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TypesThatConfiguration AreBasedOn<T>()
        {
            return AreBasedOn(typeof(T));
        }

        /// <summary>
        /// Filters types that are based on
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public TypesThatConfiguration AreBasedOn(Type baseType)
        {
            bool notValue = GetNotAndingValue();

            Func<Type, bool> basedOnFilter =
                type => ReflectionService.CheckTypeIsBasedOnAnotherType(type, baseType) == notValue;

            Add(basedOnFilter);

            return this;
        }

        /// <summary>
        /// Allows you to provide a method that will test a classes base classes (base class and interfaces)
        /// </summary>
        /// <param name="typeFilter">based on type filter</param>
        /// <returns>type filter</returns>
        public TypesThatConfiguration AreBasedOn(Func<Type, bool> typeFilter)
        {
            bool notValue = GetNotAndingValue();

            Func<Type, bool> basedOnFilter =
                type =>
                {
                    var baseType = type;

                    while (baseType != null && baseType != typeof(object))
                    {
                        if (typeFilter(baseType))
                        {
                            return true == notValue;
                        }

                        baseType = baseType.GetTypeInfo().BaseType;
                    }

                    foreach (Type implementedInterface in type.GetTypeInfo().ImplementedInterfaces)
                    {
                        if (typeFilter(implementedInterface))
                        {
                            return true == notValue;
                        }
                    }

                    return false == notValue;
                };

            Add(basedOnFilter);

            return this;
        }

        /// <summary>
        /// Adds a type filter directly
        /// </summary>
        /// <param name="typeFilter">type filter</param>
        /// <returns>type filter</returns>
        public TypesThatConfiguration Match(Func<Type, bool> typeFilter)
        {
            bool notValue = GetNotAndingValue();

            Add(t => typeFilter(t) == notValue);

            return this;
        }


        /// <summary>
        /// Adds a type filter that returns true if the type is public
        /// </summary>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration ArePublic()
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.GetTypeInfo().IsPublic == notValue);

            return this;
        }

        /// <summary>
        /// Adds a type filter that returns true if the type is private
        /// </summary>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration AreNotPublic()
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.GetTypeInfo().IsNotPublic == notValue);

            return this;
        }

        /// <summary>
        /// Adds a type filter that returns true if the type is constructed generic
        /// </summary>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration AreConstructedGeneric()
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.IsConstructedGenericType == notValue);

            return this;
        }

        /// <summary>
        /// Adds a type filter that returns true if the type is an open generic
        /// </summary>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration AreOpenGeneric()
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.GetTypeInfo().IsGenericTypeDefinition == notValue);

            return this;
        }

        /// <summary>
        /// Creates a new type filter based on the types namespace
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="includeSubnamespaces"></param>
        /// <returns></returns>
        public TypesThatConfiguration AreInTheSameNamespace(string @namespace, bool includeSubnamespaces = false)
        {
            bool notValue = GetNotAndingValue();
            Func<Type, bool> newFilter;

            if (includeSubnamespaces)
            {
                newFilter = type => (type.Namespace == @namespace ||
                                         (type.Namespace != null &&
                                          type.Namespace.StartsWith(@namespace + "."))) == notValue;
            }
            else
            {
                newFilter = type => (type.Namespace == @namespace) == notValue;
            }

            Add(newFilter);

            return this;
        }

        /// <summary>
        /// Creates a new type filter that fiters based on if it's in the same namespace as another class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeSubnamespaces"></param>
        /// <returns></returns>
        public TypesThatConfiguration AreInTheSameNamespaceAs(Type type, bool includeSubnamespaces = false)
        {
            return AreInTheSameNamespace(type.Namespace, includeSubnamespaces);
        }

        /// <summary>
        /// Creates a new type filter that fiters based on if it's in the same namespace as another class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeSubnamespaces"></param>
        /// <returns></returns>
        public TypesThatConfiguration AreInTheSameNamespaceAs<T>(bool includeSubnamespaces = false)
        {
            return AreInTheSameNamespaceAs(typeof(T), includeSubnamespaces);
        }


        /// <summary>
        /// Tests to see if a type has an attribute
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="attributeFilter"></param>
        /// <returns></returns>
        public TypesThatConfiguration HaveAttribute(Type attributeType, Func<Attribute, bool> attributeFilter = null)
        {
            bool notValue = GetNotAndingValue();
            Func<Type, bool> newFilter;

            if (attributeFilter != null)
            {
                Func<Attribute, bool> localFunc = attributeFilter;

                newFilter = t => t.GetTypeInfo().GetCustomAttributes(true).
                                                 Where(a => ReflectionService.CheckTypeIsBasedOnAnotherType(a.GetType(), attributeType)).
                                                 Any(a => attributeFilter((Attribute)a)) == notValue;
            }
            else
            {
                newFilter = t => t.GetTypeInfo().GetCustomAttributes(attributeType, true).
                                                 Any(a => ReflectionService.CheckTypeIsBasedOnAnotherType(a.GetType(), attributeType))
                                                 == notValue;
            }

            Add(newFilter);

            return this;
        }

        /// <summary>
        /// Tests to see if a type has an attribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="attributeFilter"></param>
        /// <returns></returns>
        public TypesThatConfiguration HaveAttribute<TAttribute>(Func<TAttribute, bool> attributeFilter = null)
            where TAttribute : Attribute
        {
            bool notValue = GetNotAndingValue();
            Func<Type, bool> newFilter;

            if (attributeFilter != null)
            {
                newFilter = t => t.GetTypeInfo().GetCustomAttributes(true).
                    Where(a => ReflectionService.CheckTypeIsBasedOnAnotherType(a.GetType(), typeof(TAttribute))).
                    Any(
                    x =>
                    {
                        bool returnValue = false;
                        TAttribute attribute =
                            x as TAttribute;

                        if (attribute != null)
                        {
                            returnValue = attributeFilter(attribute);
                        }

                        return returnValue;
                    })
                    == notValue;
            }
            else
            {
                newFilter = t => t.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), true).
                                                 Any(a => ReflectionService.CheckTypeIsBasedOnAnotherType(a.GetType(), typeof(TAttribute)))
                                                 == notValue;
            }

            Add(newFilter);

            return this;
        }

        /// <summary>
        /// Adds type filter that filters based uppon a classes attribute
        /// </summary>
        /// <param name="consider"></param>
        /// <returns></returns>
        public TypesThatConfiguration HaveAttribute(Func<Type, bool> consider)
        {
            bool notValue = GetNotAndingValue();

            Func<Type, bool> newFilter =
                type =>
                {
                    foreach (Attribute customAttribute in type.GetTypeInfo().GetCustomAttributes())
                    {
                        if (consider(customAttribute.GetType()))
                        {
                            return notValue;
                        }
                    }

                    return false == notValue;
                };

            Add(newFilter);

            return this;
        }

        /// <summary>
        /// Creates a new type filter method that returns true if the Name of the type starts with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TypesThatConfiguration StartWith(string name)
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.Name.StartsWith(name) == notValue);

            return this;
        }

        /// <summary>
        /// Creates a new type filter that returns true if the Name ends with the provided string
        /// </summary>
        /// <param name="name">test string</param>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration EndWith(string name)
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.Name.EndsWith(name) == notValue);

            return this;
        }

        /// <summary>
        /// Creates a new type filter that returns true if the name contains the provided string
        /// </summary>
        /// <param name="name">string to test for</param>
        /// <returns>configuration object</returns>
        public TypesThatConfiguration Contains(string name)
        {
            bool notValue = GetNotAndingValue();

            Add(t => t.Name.Contains(name) == notValue);

            return this;
        }

        /// <summary>
        /// Reverses the logic for the next type filter
        /// </summary>
        public TypesThatConfiguration Not
        {
            get
            {
                notLogicValue = false;

                return this;
            }
        }

        /// <summary>
        /// Or together the filters rather than using And
        /// </summary>
        public TypesThatConfiguration Or
        {
            get
            {
                UseOr = true;

                return this;
            }
        }

        /// <summary>
        /// And together filters rather than using Or
        /// </summary>
        public TypesThatConfiguration And
        {
            get
            {
                if (UseOr)
                {
                    throw new Exception("Cannot use And with Or");
                }

                UseOr = false;

                return this;
            }
        }

        private bool notLogicValue = true;
        
        private bool GetNotAndingValue()
        {
            bool tempValue = notLogicValue;

            notLogicValue = true;

            return tempValue;
        }
    }
}
