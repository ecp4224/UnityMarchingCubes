using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class Extensions
{
    public static Vector3 Abs(this Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    public static Vector3 Round(this Vector3 vec)
    {
        return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
    }
    
    public static T RandomEnumValue<T> (params T[] exclude)
    {
        while (true)
        {
            var v = Enum.GetValues(typeof(T));
            var t = (T) v.GetValue(new Random().Next(v.Length));

            if (exclude.Contains(t))
                continue;

            return t;
        }
    }
    
    public static bool IsGenericList(this Type oType)
    {
        return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
    }
    
    public static string Md5Sum(this string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);
 
        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
 
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";
 
        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }
 
        return hashString.PadLeft(32, '0');
    }
    
    /// <summary>
    /// Test if a type implements IList of T, and if so, determine T.
    /// </summary>
    public static Type ListOfWhat(this Type type)
    {
        if (type == null)
            return null;

        if (!type.IsGenericList())
            return null;

        var interfaceTest = new Func<Type, Type>(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>) ? i.GetGenericArguments().Single() : null);

        var innerType = interfaceTest(type);
        if (innerType != null)
        {
            return innerType;
        }

        foreach (var i in type.GetInterfaces())
        {
            innerType = interfaceTest(i);
            if (innerType != null)
            {
                return innerType;
            }
        }

        return null;
    }
}