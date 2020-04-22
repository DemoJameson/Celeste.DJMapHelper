using System;
using System.Reflection;

namespace Celeste.Mod.DJMapHelper.Extensions {
    public static class Extensions {
        public static FieldInfo GetPrivateField(this Type type, string fieldName) {
            return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static MethodInfo GetPrivateMethod(this Type type, string methodName) {
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}