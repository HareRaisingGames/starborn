using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;

public static class ReflectionUtils
{
    public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
    {
        return
          assembly.GetTypes()
                  .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                  .ToArray();
    }

    public static Type FindClassAcrossAllNamespaces(string className)
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            // Get all types defined in the assembly
            Type[] types = assembly.GetTypes();

            // Find the type with the matching name
            Type found = types.FirstOrDefault(t => t.Name == className);
            if (found != null)
            {
                return found;
            }
        }

        return null; // Return null if the class is not found
    }
}
