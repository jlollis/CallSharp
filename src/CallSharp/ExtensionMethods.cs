﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CallSharp
{
  public static class ExtensionMethods
  {
    public static bool AllAreOptional(this ParameterInfo[] ps)
    {
      return ps.All(p => p.IsOptional);
    }

    public static bool IsParams(this ParameterInfo pi)
    {
      return pi.GetCustomAttribute<ParamArrayAttribute>()
             != null;
    }

    public static object InvokeStaticWithSingleArgument<T>(this MethodInfo mi, T arg)
    {
      return mi.Invoke(null /*static*/, new object[] {arg});
    }

    public static object InvokeWithNoArgument<T>(this MethodInfo mi, T subject)
    {
      var pars = mi.GetParameters();
      if (pars.IsSingleParamsArgument())
        return mi.Invoke(subject, new[]
        {
          Activator.CreateInstance(pars[0].ParameterType.UnderlyingSystemType, 0)
        });
      else
      {
        return mi.Invoke(subject, new object[] {});
      }
    }

    public static bool IsSingleParamsArgument(this ParameterInfo[] ps)
    {
      return ps.Length == 1 && ps[0].IsParams();
    }

    public static IReadOnlyList<object> InferTypes(this string text)
    {
      var result = new List<object>();

      foreach (var type in TypeDatabase.ParseableTypes)
      {
        foreach (var m in type.GetMethods().Where(
          x => x.Name.Equals("TryParse") 
          && x.GetParameters().Length == 2))
        {
          // see http://stackoverflow.com/questions/569249/methodinfo-invoke-with-out-parameter
          object[] pars = {text, null};
          bool ok = (bool) m.Invoke(null, pars);
          if (ok)
          {
            result.Add(pars[1]);
          }
        }
      }
      return result;
    }

    /// <summary>Adds a single element to the end of an IEnumerable.</summary>
    /// <typeparam name="T">Type of enumerable to return.</typeparam>
    /// <returns>IEnumerable containing all the input elements, followed by the
    /// specified additional element.</returns>
    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      return concatIterator(element, source, false);
    }

    /// <summary>Adds a single element to the start of an IEnumerable.</summary>
    /// <typeparam name="T">Type of enumerable to return.</typeparam>
    /// <returns>IEnumerable containing the specified additional element, followed by
    /// all the input elements.</returns>
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> tail, T head)
    {
      if (tail == null)
        throw new ArgumentNullException("tail");
      return concatIterator(head, tail, true);
    }

    private static IEnumerable<T> concatIterator<T>(T extraElement,
        IEnumerable<T> source, bool insertAtStart)
    {
      if (insertAtStart)
        yield return extraElement;
      foreach (var e in source)
        yield return e;
      if (!insertAtStart)
        yield return extraElement;
    }
  }
}