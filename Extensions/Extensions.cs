using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.DJMapHelper.Extensions {
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

        public static T CreateDelegate_Get<T>(this FieldInfo field) where T : Delegate {
            bool isStatic = field.IsStatic;
            Type[] param = isStatic ? Type.EmptyTypes : new[] {field.DeclaringType};

            DynamicMethod dyn = new($"{field.DeclaringType?.FullName}_{field.Name}_FastAccess", field.FieldType, param, field.DeclaringType);
            ILGenerator ilGen = dyn.GetILGenerator();
            if (isStatic) {
                ilGen.Emit(OpCodes.Ldsfld, field);
            } else {
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, field);
            }

            ilGen.Emit(OpCodes.Ret);
            return (T)dyn.CreateDelegate(typeof(T));
        }

        public static Func<T, TResult> CreateDelegate_Get<T, TResult>(this string fieldName) {
            FieldInfo field = typeof(T).GetFieldInfo(fieldName);
            if (field == null) {
                throw new Exception($"Field {typeof(T).Name}.{fieldName} not found");
            }

            return CreateDelegate_Get<Func<T, TResult>>(field);
        }

        public static Func<object, object> CreateDelegate_GetInstance(this FieldInfo field) {
            if (field.IsStatic) {
                throw new Exception("Not support static field.");
            }

            DynamicMethod dyn =
                new($"{field.DeclaringType?.FullName}_{field.Name}_FastAccess", typeof(object), new Type[] {typeof(object)}, field.DeclaringType);
            ILGenerator ilGen = dyn.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Castclass, field.DeclaringType);
            ilGen.Emit(OpCodes.Ldfld, field);
            ilGen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)dyn.CreateDelegate(typeof(Func<object, object>));
        }

        public static Func<object> CreateDelegate_GetStatic(this FieldInfo field) {
            if (!field.IsStatic) {
                throw new Exception("Not support non static field.");
            }

            DynamicMethod dyn = new($"{field.DeclaringType?.FullName}_{field.Name}_FastAccess", typeof(object), new Type[0]);
            ILGenerator ilGen = dyn.GetILGenerator();
            ilGen.Emit(OpCodes.Ldsfld, field);
            ilGen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object>)dyn.CreateDelegate(typeof(Func<object>));
        }
    }
}