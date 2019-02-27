using System;
using System.Reflection;

namespace Celeste.Mod.DJMapHelper {
    public static class Extensions {
        public static FieldInfo GetPrivateField(this Type type, string fieldName) {
            return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}