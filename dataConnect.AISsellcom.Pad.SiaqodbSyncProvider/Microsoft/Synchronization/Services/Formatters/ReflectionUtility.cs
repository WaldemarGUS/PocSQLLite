// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.ReflectionUtility
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class ReflectionUtility
  {
    private static object _lockObject = new object();
    private static Dictionary<string, IEnumerable<PropertyInfo>> _stringToPropInfoMapping = new Dictionary<string, IEnumerable<PropertyInfo>>();
    private static Dictionary<string, IEnumerable<PropertyInfo>> _stringToPKPropInfoMapping = new Dictionary<string, IEnumerable<PropertyInfo>>();
    private static Dictionary<string, ConstructorInfo> _stringToCtorInfoMapping = new Dictionary<string, ConstructorInfo>();

    public static IEnumerable<PropertyInfo> GetPropertyInfoMapping(Type type)
    {
      IEnumerable<PropertyInfo> source;
      if (!ReflectionUtility._stringToPropInfoMapping.TryGetValue(type.FullName, out source))
      {
        lock (ReflectionUtility._lockObject)
        {
          if (!ReflectionUtility._stringToPropInfoMapping.TryGetValue(type.FullName, out source))
          {
            source = (IEnumerable<PropertyInfo>) type.GetProperties();
            source = (IEnumerable<PropertyInfo>) source.Where<PropertyInfo>((Func<PropertyInfo, bool>) (e =>
            {// Begin Change dC DataConnect
              if (!e.Name.Equals("ServiceMetadata", StringComparison.Ordinal) && !e.CustomAttributes.Any(a => a.AttributeType == typeof(Sqo.Attributes.IgnoreAttribute)) && e.GetMethod != (MethodInfo) null && e.SetMethod != (MethodInfo) null)
                return e.DeclaringType == type; // Ende Change dC dataConnectc
              return false;
            })).ToArray<PropertyInfo>();
            ReflectionUtility._stringToPropInfoMapping[type.FullName] = source;
            PropertyInfo[] array = source.Where<PropertyInfo>((Func<PropertyInfo, bool>) (e => ((IEnumerable<object>) e.GetCustomAttributes(typeof (KeyAttribute), true)).Any<object>())).ToArray<PropertyInfo>();
            if (array.Length == 0)
              throw new InvalidOperationException(string.Format("Entity {0} does not have the any property marked with the [DataAnnotations.KeyAttribute]. or [SQLite.PrimaryKeyAttribute]", (object) type.Name));
            ReflectionUtility._stringToPKPropInfoMapping[type.FullName] = (IEnumerable<PropertyInfo>) array;
            ConstructorInfo constructorInfo = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>) (e => !((IEnumerable<ParameterInfo>) e.GetParameters()).Any<ParameterInfo>()));
            if (constructorInfo == (ConstructorInfo) null)
              throw new InvalidOperationException(string.Format("Type {0} does not have a public parameterless constructor.", (object) type.FullName));
            ReflectionUtility._stringToCtorInfoMapping[type.FullName] = constructorInfo;
          }
        }
      }
      return source;
    }

    public static IEnumerable<PropertyInfo> GetPrimaryKeysPropertyInfoMapping(
      Type type)
    {
      IEnumerable<PropertyInfo> propertyInfos;
      if (!ReflectionUtility._stringToPKPropInfoMapping.TryGetValue(type.FullName, out propertyInfos))
      {
        ReflectionUtility.GetPropertyInfoMapping(type);
        ReflectionUtility._stringToPKPropInfoMapping.TryGetValue(type.FullName, out propertyInfos);
      }
      return propertyInfos;
    }

    public static string GetPrimaryKeyString(IOfflineEntity live)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = string.Empty;
      foreach (PropertyInfo propertyInfo in ReflectionUtility.GetPrimaryKeysPropertyInfoMapping(live.GetType()))
      {
        if (propertyInfo.PropertyType == FormatterConstants.GuidType)
          stringBuilder.AppendFormat("{0}{1}=guid'{2}'", (object) str, (object) propertyInfo.Name, propertyInfo.GetValue((object) live, (object[]) null));
        else if (propertyInfo.PropertyType == FormatterConstants.StringType)
          stringBuilder.AppendFormat("{0}{1}='{2}'", (object) str, (object) propertyInfo.Name, propertyInfo.GetValue((object) live, (object[]) null));
        else
          stringBuilder.AppendFormat("{0}{1}={2}", (object) str, (object) propertyInfo.Name, propertyInfo.GetValue((object) live, (object[]) null));
        if (string.IsNullOrEmpty(str))
          str = ", ";
      }
      return stringBuilder.ToString();
    }

    public static IOfflineEntity GetObjectForType(
      EntryInfoWrapper wrapper,
      Type[] knownTypes)
    {
      ConstructorInfo constructorInfo;
      Type type;
      if (!ReflectionUtility._stringToCtorInfoMapping.TryGetValue(wrapper.TypeName, out constructorInfo))
      {
        if (knownTypes == null)
          throw new InvalidOperationException(string.Format("Unable to find a matching type for entry '{0}' in the loaded assemblies. Specify the type name in the KnownTypes argument to the SyncReader instance.", (object) wrapper.TypeName));
        type = ((IEnumerable<Type>) knownTypes).FirstOrDefault<Type>((Func<Type, bool>) (e => e.FullName.Equals(wrapper.TypeName, StringComparison.CurrentCultureIgnoreCase)));
        if (type == (Type) null)
          throw new InvalidOperationException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to find a matching type for entry '{0}' in list of KnownTypes.", (object) wrapper.TypeName));
        ReflectionUtility.GetPropertyInfoMapping(type);
        constructorInfo = ReflectionUtility._stringToCtorInfoMapping[wrapper.TypeName];
      }
      else
        type = constructorInfo.DeclaringType;
      object obj = constructorInfo.Invoke((object[]) null);
      if (!wrapper.IsTombstone)
      {
        foreach (PropertyInfo propertyInfo in ReflectionUtility.GetPropertyInfoMapping(type))
        {
          string str;
          if (wrapper.PropertyBag.TryGetValue(propertyInfo.Name, out str))
            propertyInfo.SetValue(obj, ReflectionUtility.GetValueFromType(propertyInfo.PropertyType, str), (object[]) null);
        }
      }
      IOfflineEntity offlineEntity = (IOfflineEntity) obj;
      offlineEntity.ServiceMetadata = new OfflineEntityMetadata(wrapper.IsTombstone, wrapper.Id, wrapper.ETag, wrapper.EditUri);
      return offlineEntity;
    }

    private static object GetValueFromType(Type type, string value)
    {
      if (value == null)
      {
        if (type.IsGenericType() || !type.IsPrimitive())
          return (object) null;
        throw new InvalidOperationException("Error in deserializing type " + type.FullName);
      }
      if (type.IsGenericType() && type.GetGenericTypeDefinition() == FormatterConstants.NullableType)
        type = type.GetGenericArguments()[0];
      if (FormatterConstants.StringType.IsAssignableFrom(type))
        return (object) value;
      if (FormatterConstants.ByteArrayType.IsAssignableFrom(type))
        return (object) Convert.FromBase64String(value);
      if (FormatterConstants.GuidType.IsAssignableFrom(type))
        return (object) new Guid(value);
      if (FormatterConstants.DateTimeType.IsAssignableFrom(type) || FormatterConstants.DateTimeOffsetType.IsAssignableFrom(type) || FormatterConstants.TimeSpanType.IsAssignableFrom(type))
        return FormatterUtilities.ParseDateTimeFromString(value, type);
      if (type.IsPrimitive() || FormatterConstants.DecimalType.IsAssignableFrom(type) || FormatterConstants.FloatType.IsAssignableFrom(type))
        return Convert.ChangeType((object) value, type, (IFormatProvider) CultureInfo.InvariantCulture);
      return (object) value;
    }
  }
}
