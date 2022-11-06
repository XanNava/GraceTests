using Grace.DependencyInjection.Exceptions;
using System;
using System.Reflection;
using Grace.Data;
using Grace.Utilities;

namespace Grace.DependencyInjection.Impl
{
	using System.Text;

	/// <summary>
    /// Interface for getting data from extra data 
    /// </summary>
    public interface IInjectionContextValueProvider
    {
        /// <summary>
        /// Get data from injection context
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        object GetValueFromInjectionContext(IExportLocatorScope scope,Type type, object key, IInjectionContext context,
            bool isRequired);

        /// <summary>
        /// Get data from injection context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locator"></param>
        /// <param name="staticContext"></param>
        /// <param name="key"></param>
        /// <param name="dataProvider"></param>
        /// <param name="defaultValue"></param>
        /// <param name="useDefault"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        T GetValueFromInjectionContext<T>(
            IExportLocatorScope locator,
            StaticInjectionContext staticContext,
            object key,
            IInjectionContext dataProvider,
            object defaultValue,
            bool useDefault,
            bool isRequired);
    }

    /// <summary>
    /// Implementation for fetching data from context value
    /// </summary>
    public class InjectionContextValueProvider : IInjectionContextValueProvider
    {
        /// <summary>
        /// Get data from injection context
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public virtual object GetValueFromInjectionContext(IExportLocatorScope locator, Type type, object key, IInjectionContext context, bool isRequired)
        {
            object value = null;

            if (context != null)
            {
                GetValueFromExtraDataProvider(type, key, context, out value);

                if (value == null && context.ExtraData != null)
                {
                    if (type.GetTypeInfo().IsAssignableFrom(context.ExtraData.GetType().GetTypeInfo()))
                    {
                        value = context.ExtraData;
                    }
                    else
                    {
                        var delegateInstance = context.ExtraData as Delegate;

                        if (delegateInstance != null && delegateInstance.GetMethodInfo().ReturnType == type)
                        {
                            value = delegateInstance;
                        }
                    }
                }
            }

            if (value == null)
            {
                var currentLocator = locator;

                while (currentLocator != null)
                {
                    if (GetValueFromExtraDataProvider(type, key, currentLocator, out value))
                    {
                        break;
                    }

                    currentLocator = currentLocator.Parent;
                }
            }
            
            if (value != null)
            {
                if (value is Delegate)
                {
                    value =
                        ReflectionService.InjectAndExecuteDelegate(locator, new StaticInjectionContext(type), context, value as Delegate);
                }

                if (!(type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo())))
                {
                    try
                    {
                        value = Convert.ChangeType(value, type);
                    }
                    catch (Exception exp)
                    {
                        // to do fix up exception
                        throw new LocateException(new StaticInjectionContext(type), exp);
                    }
                }
            }
            else if (isRequired)
            {
                throw new LocateException(new StaticInjectionContext(type));
            }

            return value;
        }

        public static StringBuilder Logs = new StringBuilder();

        /// <summary>
        /// Get data from injection context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locator"></param>
        /// <param name="staticContext"></param>
        /// <param name="key"></param>
        /// <param name="dataProvider"></param>
        /// <param name="defaultValue"></param>
        /// <param name="useDefault"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public virtual T GetValueFromInjectionContext<T>(IExportLocatorScope locator,
                                                 StaticInjectionContext staticContext, 
                                                 object key,
                                                 IInjectionContext dataProvider, 
                                                 object defaultValue, 
                                                 bool useDefault, 
                                                 bool isRequired) {
	        Logs.AppendLine("-----GetValueFromInjectionContext-----");
	        Logs.AppendLine(locator.ToString());
	        Logs.AppendLine("staticContext null:" + (staticContext != null).ToString());
			Logs.AppendLine("key val:" + (string)key);
			Logs.AppendLine("dataProvider null:" + (dataProvider != null).ToString());
			Logs.AppendLine("defaultValue null:" + (defaultValue != null).ToString());
			Logs.AppendLine("useDefault val:" + useDefault.ToString());
			Logs.AppendLine("isRequired val:" + isRequired.ToString());

			key = (object)"ServiceA";
			Logs.AppendLine("key val:" + (string)key);

			object value = null;

			Logs.AppendLine((dataProvider != null).ToString());

			if (dataProvider != null) {
				Logs.AppendLine("a1 stake sauce");

				GetValueFromExtraDataProvider<T>(key, dataProvider, out value);
				Logs.AppendLine("Value type: " + value?.GetType().ToString());
				Logs.AppendLine("value val: " + (value != null));

                if (value == null)
                {
					Logs.AppendLine("a2");
					if (dataProvider.ExtraData is T)
                    {
						Logs.AppendLine("a3");
						value = dataProvider.ExtraData;
                    }
                    else
                    {
						Logs.AppendLine("a4");
						var delegateInstance = dataProvider.ExtraData as Delegate;
						
                        if (delegateInstance != null && delegateInstance.GetMethodInfo().ReturnType == typeof(T))
                        {
							Logs.AppendLine("a5");
							value = delegateInstance;
                        }
                    }
                }
            }

            if (value == null)
            {
				Logs.AppendLine("b1");
				var currentLocator = locator;

                while (currentLocator != null)
                {
					Logs.AppendLine("b2");
					if (GetValueFromExtraDataProvider<T>(key, currentLocator, out value))
                    {
						Logs.AppendLine("b3");
						break;
                    }

                    currentLocator = currentLocator.Parent;
                }
            }

            if (value == null && useDefault)
            {
				Logs.AppendLine("c1");
				value = defaultValue;
            }

            Logs.AppendLine(value?.ToString());
            if (value != null)
            {
				Logs.AppendLine("d1");

				if (value is Delegate)
                {
					Logs.AppendLine("d2");

					value =
                        ReflectionService.InjectAndExecuteDelegate(locator, staticContext, dataProvider, value as Delegate);
                }

                if(!(value is T))
                {
					Logs.AppendLine("d3");

					try {
                        if (typeof(T).IsConstructedGenericType &&
                            typeof(T).GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
							Logs.AppendLine("d4");

							var type = typeof(T).GetTypeInfo().GenericTypeArguments[0];

                            if (type.GetTypeInfo().IsEnum)
                            {
								Logs.AppendLine("d5");

								value = Enum.ToObject(type, value);
                            }
                            else
                            {
								Logs.AppendLine("d6");

								value = Convert.ChangeType(value, typeof(T).GetTypeInfo().GenericTypeArguments[0]);
                            }
                        }
                        else
                        {
							Logs.AppendLine("d7");

							value = Convert.ChangeType(value, typeof(T));
                        }
                    }
                    catch (Exception exp)
                    {
						Logs.AppendLine("d8!");

						// to do fix up exception
						throw new LocateException(staticContext, exp);
                    }
                }
            }
            else if (isRequired && !useDefault) {
				Logs.AppendLine("d9");

				Logs.AppendLine(isRequired.ToString());
	            Logs.AppendLine((!useDefault).ToString());
				Logs.AppendLine("-----END-----");

// !!-- Issue.
				throw new LocateException(staticContext);
            }
            Logs.AppendLine("-----END-----");

		return (T)value;
        }

        /// <summary>
        /// Get value from extra data provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataProvider"></param>
        /// <param name="tValue"></param>
        /// <returns></returns>
        protected virtual bool GetValueFromExtraDataProvider<T>(object key, IExtraDataContainer dataProvider, out object tValue) {
	        Logs.AppendLine("-----GetValueFromExtraDataProvider-----");
	        Logs.AppendLine("Key null: " + (key != null).ToString());
            object value = null;

            if (key != null)
            {
                value = dataProvider.GetExtraData(key);
                Logs.Append(InjectionContext.Logs);
				Logs.AppendLine("WHAT");

Logs.AppendLine("dataProvider source = " + dataProvider.GetType().ToString());
                Logs.AppendLine("A1 value = " + value?.GetType().ToString());
            }

            if (value != null) {
	            Logs.AppendLine("B1 value != null");
                tValue = value;
                return true;
            }

            foreach (var o in dataProvider.KeyValuePairs)
            {
				Logs.AppendLine("C1");

				if (o.Key is string stringKey && 
                    stringKey.StartsWith(UniqueStringId.Prefix))
                {
					Logs.AppendLine("C2");


					continue;
                }

                if (o.Value is T)
                {
					Logs.AppendLine("C3");


					tValue = o.Value;

                    return true;
                }

                if (o.Value is Delegate delegateInstance && 
                    delegateInstance.GetMethodInfo().ReturnType == typeof(T))
                {
					Logs.AppendLine("C4");

					tValue = o.Value;

                    return true;
                }
            }

			Logs.AppendLine("Z1");

			Logs.AppendLine("-----END-----");

			tValue = null;

            return false;
        }

        protected virtual bool GetValueFromExtraDataProvider(Type type, object key, IExtraDataContainer dataProvider, out object tValue)
        {
            object value = null;

            if (key != null)
            {
                value = dataProvider.GetExtraData(key);
            }

            if (value != null)
            {
                tValue = value;
                return true;
            }

            foreach (var o in dataProvider.Values)
            {
                if (type.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo()))
                {
                    tValue = o;

                    return true;
                }

                var delegateInstance = o as Delegate;

                if (delegateInstance != null &&
                    delegateInstance.GetMethodInfo().ReturnType == type)
                {
                    tValue = o;

                    return true;
                }
            }

            tValue = null;

            return false;
        }
    }
}
