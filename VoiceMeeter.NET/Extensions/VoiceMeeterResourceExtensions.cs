using System.Linq.Expressions;
using System.Reflection;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Configuration;

namespace VoiceMeeter.NET.Extensions;

internal  static class VoiceMeeterResourceExtensions
{
    internal static string GetPropertyParamName<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        where TSource: VoiceMeeterResource<TSource>
    {
        Type type = typeof(TSource);

        MemberExpression? member = propertyLambda.Body as MemberExpression;
        if (member == null)
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

        PropertyInfo? propInfo = member.Member as PropertyInfo;
        if (propInfo == null)
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

        if (type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType!))
            throw new ArgumentException(
                $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
        
        var attribute = propInfo.GetCustomAttribute<VoiceMeeterParameterAttribute>(inherit: true);

        if (attribute == null)
            throw new ArgumentException(
                $"Expression '{propertyLambda}' refers to a property that does not possess the Attribute {typeof(VoiceMeeterParameterAttribute)}.");

        return source.GetFullParamName(attribute.Name);
    }
}