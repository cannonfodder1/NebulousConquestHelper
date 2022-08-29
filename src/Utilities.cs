using System;
using System.Reflection;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
    public class Utilities
    {
        public static object GetPrivateField(object instance, string fieldName)
        {
            static object GetPrivateFieldInternal(object instance, string fieldName, Type type)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    return field.GetValue(instance);
                }
                else if (type.BaseType != null)
                {
                    return GetPrivateFieldInternal(instance, fieldName, type.BaseType);
                }
                else
                {
                    return null;
                }
            }

            return GetPrivateFieldInternal(instance, fieldName, instance.GetType());
        }

        public static void SetPrivateField(object instance, string fieldName, object value)
        {
            static void SetPrivateFieldInternal(object instance, string fieldName, object value, Type type)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }
                else if (type.BaseType != null)
                {
                    SetPrivateFieldInternal(instance, fieldName, value, type.BaseType);
                    return;
                }
            }

            SetPrivateFieldInternal(instance, fieldName, value, instance.GetType());
        }

        public static void CallPrivateInstanceMethod(object instance, string methodName, object[] parameters)
        {
            static void CallPrivateMethodInternal(object instance, string methodName, object[] parameters, Type type)
            {
                MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(instance, parameters);
                    return;
                }
                else if (type.BaseType != null)
                {
                    CallPrivateMethodInternal(instance, methodName, parameters, type.BaseType);
                    return;
                }
            }

            CallPrivateMethodInternal(instance, methodName, parameters, instance.GetType());
        }

        public static object CallPrivateStaticMethod(Type type, string methodName, object[] parameters)
        {
            static object CallPrivateMethodInternal(Type type, string methodName, object[] parameters)
            {
                MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

                if (method != null)
                {
                    return method.Invoke(type, parameters);
                }
                else if (type.BaseType != null)
                {
                    return CallPrivateMethodInternal(type.BaseType, methodName, parameters);
                }
                else
                {
                    return null;
                }
            }

            return CallPrivateMethodInternal(type, methodName, parameters);
        }

        public static void PrintAllPrivateMethods(object instance)
        {
            static void PrintAllPrivateMethodsInternal(object instance, Type type)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (MethodInfo method in methods)
                {
                    Console.WriteLine(" - " + method.Name);
                }

                if (type.BaseType != null)
                {
                    PrintAllPrivateMethodsInternal(instance, type.BaseType);
                }

                return;
            }

            Console.WriteLine("Methods of " + instance.GetType().Name + ":");

            PrintAllPrivateMethodsInternal(instance, instance.GetType());

            Console.WriteLine($"--- PRINTING COMPLETE ---");
        }

        public static object GetPrivateProperty(object instance, string propertyName)
        {
            static object GetPrivatePropertyInternal(object instance, string propertyName, Type type)
            {
                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (property != null)
                {
                    return property.GetValue(instance);
                }
                else if (type.BaseType != null)
                {
                    return GetPrivatePropertyInternal(instance, propertyName, type.BaseType);
                }
                else
                {
                    return null;
                }
            }

            return GetPrivatePropertyInternal(instance, propertyName, instance.GetType());
        }

        public static ValueType EditPrivateStructFields(object structure, Dictionary<string, object> values)
        {
            ValueType workaround = (ValueType)structure;
            Type type = structure.GetType();

            foreach (KeyValuePair<string, object> value in values)
            {
                FieldInfo field = type.GetField(value.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(workaround, value.Value);
            }

            return workaround;
        }
    }
}
