using System;
using System.Reflection;

namespace Celeste.Mod.DJMapHelper.Extensions {
    public static class Extensions {
        public static FieldInfo GetPrivateField(this Type type, string fieldName) {
            return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        public static void SetPrivateFieldValue(this object obj, string name, object value) {
            obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(obj, value);
        }
        
        public static MethodInfo GetPrivateMethod(this Type type, string methodName) {
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        public static object InvokePrivateMethod(this object obj, string methodName, params object[] parameters) {
            return obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(obj, parameters);
        }
    }
}