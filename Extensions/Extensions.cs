using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.DJMapHelper.Extensions; 

internal static class Extensions {
    public static FieldInfo GetPrivateField(this Type type, string fieldName) {
        return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static MethodInfo GetPrivateMethod(this Type type, string methodName) {
        return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
    }
}

internal static class ReflectionExtensions {
    private const BindingFlags StaticInstanceAnyVisibility =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private const BindingFlags InstanceAnyVisibilityDeclaredOnly =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> CachedFieldInfos = new();
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> CachedPropertyInfos = new();
    private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> CachedMethodInfos = new();

    public static FieldInfo GetFieldInfo(this Type type, string name, bool includeSuperClassPrivate = false) {
        if (!CachedFieldInfos.ContainsKey(type)) {
            CachedFieldInfos[type] = new Dictionary<string, FieldInfo>();
        }

        if (!CachedFieldInfos[type].ContainsKey(name)) {
            FieldInfo result = type.GetField(name, StaticInstanceAnyVisibility);
            if (result == null && type.BaseType != null && includeSuperClassPrivate) {
                result = type.BaseType.GetFieldInfo(name, true);
            }

            return CachedFieldInfos[type][name] = result;
        } else {
            return CachedFieldInfos[type][name];
        }
    }

    public static PropertyInfo GetPropertyInfo(this Type type, string name, bool includeSuperClassPrivate = false) {
        if (!CachedPropertyInfos.ContainsKey(type)) {
            CachedPropertyInfos[type] = new Dictionary<string, PropertyInfo>();
        }

        if (!CachedPropertyInfos[type].ContainsKey(name)) {
            PropertyInfo result = type.GetProperty(name, StaticInstanceAnyVisibility);
            if (result == null && type.BaseType != null && includeSuperClassPrivate) {
                result = type.BaseType.GetPropertyInfo(name, true);
            }

            return CachedPropertyInfos[type][name] = result;
        } else {
            return CachedPropertyInfos[type][name];
        }
    }

    public static MethodInfo GetMethodInfo(this Type type, string name, bool includeSuperClassPrivate = false) {
        if (!CachedMethodInfos.ContainsKey(type)) {
            CachedMethodInfos[type] = new Dictionary<string, MethodInfo>();
        }

        if (!CachedMethodInfos[type].ContainsKey(name)) {
            MethodInfo result = type.GetMethod(name, StaticInstanceAnyVisibility);
            if (result == null && type.BaseType != null && includeSuperClassPrivate) {
                result = type.BaseType.GetMethodInfo(name, true);
            }

            return CachedMethodInfos[type][name] = result;
        } else {
            return CachedMethodInfos[type][name];
        }
    }

    public static IEnumerable<FieldInfo> GetAllFieldInfos(this Type type, bool includeStatic = false, bool filterBackingField = false) {
        BindingFlags bindingFlags = InstanceAnyVisibilityDeclaredOnly;
        if (includeStatic) {
            bindingFlags |= BindingFlags.Static;
        }

        List<FieldInfo> result = new();
        while (type != null && type.IsSubclassOf(typeof(object))) {
            IEnumerable<FieldInfo> fieldInfos = type.GetFields(bindingFlags);
            if (filterBackingField) {
                fieldInfos = fieldInfos.Where(info => !info.Name.EndsWith("k__BackingField"));
            }

            foreach (FieldInfo fieldInfo in fieldInfos) {
                if (result.Contains(fieldInfo)) {
                    continue;
                }

                result.Add(fieldInfo);
            }

            if (type.BaseType == null) {
                break;
            } else {
                type = type.BaseType;
            }
        }

        return result;
    }

    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type, bool includeStatic = false) {
        BindingFlags bindingFlags = InstanceAnyVisibilityDeclaredOnly;
        if (includeStatic) {
            bindingFlags |= BindingFlags.Static;
        }

        List<PropertyInfo> result = new();
        while (type != null && type.IsSubclassOf(typeof(object))) {
            IEnumerable<PropertyInfo> properties = type.GetProperties(bindingFlags);
            foreach (PropertyInfo fieldInfo in properties) {
                if (result.Contains(fieldInfo)) {
                    continue;
                }

                result.Add(fieldInfo);
            }

            if (type.BaseType == null) {
                break;
            } else {
                type = type.BaseType;
            }
        }

        return result;
    }

    public static T GetFieldValue<T>(this object obj, string name) {
        object result = obj.GetType().GetFieldInfo(name)?.GetValue(obj);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static T GetFieldValue<T>(this Type type, string name) {
        object result = type.GetFieldInfo(name)?.GetValue(null);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static void SetFieldValue(this object obj, string name, object value) {
        obj.GetType().GetFieldInfo(name)?.SetValue(obj, value);
    }

    public static void SetFieldValue(this Type type, string name, object value) {
        type.GetFieldInfo(name)?.SetValue(null, value);
    }

    public static T GetPropertyValue<T>(this object obj, string name) {
        object result = obj.GetType().GetPropertyInfo(name)?.GetValue(obj, null);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static T GetPropertyValue<T>(Type type, string name) {
        object result = type.GetPropertyInfo(name)?.GetValue(null, null);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static void SetPropertyValue(this object obj, string name, object value) {
        if (obj.GetType().GetPropertyInfo(name) is {CanWrite: true} propertyInfo) {
            propertyInfo.SetValue(obj, value, null);
        }
    }

    public static void SetPropertyValue(this Type type, string name, object value) {
        if (type.GetPropertyInfo(name) is {CanWrite: true} propertyInfo) {
            propertyInfo.SetValue(null, value, null);
        }
    }

    public static T InvokeMethod<T>(this object obj, string name, params object[] parameters) {
        object result = obj.GetType().GetMethodInfo(name)?.Invoke(obj, parameters);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static T InvokeMethod<T>(this Type type, string name, params object[] parameters) {
        object result = type.GetMethodInfo(name)?.Invoke(null, parameters);
        if (result == null) {
            return default;
        } else {
            return (T)result;
        }
    }

    public static void InvokeMethod(this object obj, string name, params object[] parameters) {
        obj.GetType().GetMethodInfo(name)?.Invoke(obj, parameters);
    }

    public static void InvokeMethod(this Type type, string name, params object[] parameters) {
        type.GetMethodInfo(name)?.Invoke(null, parameters);
    }

    public static bool IsSameOrSubclassOf(this Type potentialDescendant, Type potentialBase) {
        return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
    }
}

internal delegate TReturn GetDelegate<in TInstance, out TReturn>(TInstance instance);

internal static class FastReflection {
    // ReSharper disable UnusedMember.Local
    private record struct DelegateKey(Type Type, string Name, Type InstanceType, Type ReturnType) {
        public readonly Type Type = Type;
        public readonly string Name = Name;
        public readonly Type InstanceType = InstanceType;
        public readonly Type ReturnType = ReturnType;
    }
    // ReSharper restore UnusedMember.Local

    private static readonly ConcurrentDictionary<DelegateKey, Delegate> CachedFieldGetDelegates = new();

    private static GetDelegate<TInstance, TReturn> CreateGetDelegateImpl<TInstance, TReturn>(Type type, string name) {
        FieldInfo field = type.GetFieldInfo(name);
        if (field == null) {
            return null;
        }

        Type returnType = typeof(TReturn);
        Type fieldType = field.FieldType;
        if (!returnType.IsAssignableFrom(fieldType)) {
            throw new InvalidCastException($"{field.Name} is of type {fieldType}, it cannot be assigned to the type {returnType}.");
        }

        var key = new DelegateKey(type, name, typeof(TInstance), typeof(TReturn));
        if (CachedFieldGetDelegates.TryGetValue(key, out var result)) {
            return (GetDelegate<TInstance, TReturn>) result;
        }

        if (field.IsLiteral && !field.IsInitOnly) {
            object value = field.GetValue(null);
            TReturn returnValue = value == null ? default : (TReturn) value;
            Func<TInstance, TReturn> func = _ => returnValue;

            GetDelegate<TInstance, TReturn> getDelegate =
                (GetDelegate<TInstance, TReturn>) func.Method.CreateDelegate(typeof(GetDelegate<TInstance, TReturn>), func.Target);
            CachedFieldGetDelegates[key] = getDelegate;
            return getDelegate;
        }

        var method = new DynamicMethod($"{field} Getter", returnType, new[] {typeof(TInstance)}, field.DeclaringType, true);
        var il = method.GetILGenerator();

        if (field.IsStatic) {
            il.Emit(OpCodes.Ldsfld, field);
        } else {
            il.Emit(OpCodes.Ldarg_0);
            if (field.DeclaringType.IsValueType && !typeof(TInstance).IsValueType) {
                il.Emit(OpCodes.Unbox_Any, field.DeclaringType);
            }

            il.Emit(OpCodes.Ldfld, field);
        }

        if (fieldType.IsValueType && !returnType.IsValueType) {
            il.Emit(OpCodes.Box, fieldType);
        }

        il.Emit(OpCodes.Ret);

        result = CachedFieldGetDelegates[key] = method.CreateDelegate(typeof(GetDelegate<TInstance, TReturn>));
        return (GetDelegate<TInstance, TReturn>) result;
    }

    public static GetDelegate<TInstance, TResult> CreateGetDelegate<TInstance, TResult>(this Type type, string fieldName) {
        return CreateGetDelegateImpl<TInstance, TResult>(type, fieldName);
    }

    public static GetDelegate<TInstance, TResult> CreateGetDelegate<TInstance, TResult>(string fieldName) {
        return CreateGetDelegate<TInstance, TResult>(typeof(TInstance), fieldName);
    }
}