using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Linq.Expressions
{
    public static partial class ExpTemplate
    {
        internal class EmplaceVisitor : ExpressionVisitor
        {
            public IReadOnlyDictionary<string, Expression> Values { get; }

            public EmplaceVisitor(IReadOnlyDictionary<string, Expression> values)
                => Values = values ?? throw new ArgumentNullException(nameof(values));

            bool TryGetValue(Expression expression, out object value)
            {
                switch (expression)
                {
                    case ConstantExpression constantExpression:
                        value = constantExpression.Value;
                        return true;
                    case MemberExpression memberExpression:
                        switch (memberExpression.Member)
                        {
                            case FieldInfo field:
                                if (field.IsStatic)
                                {
                                    value = field.GetValue(null);
                                    return true;
                                }
                                if (TryGetValue(memberExpression.Expression, out var fieldInstance))
                                {
                                    value = field.GetValue(fieldInstance);
                                    return true;
                                }
                                break;
                            case PropertyInfo property:
                                if (null != property.GetMethod && property.GetMethod.GetParameters().Length == 0)
                                {
                                    if (property.GetMethod.IsStatic)
                                    {
                                        value = property.GetValue(null, null);
                                        return true;
                                    }
                                    if (TryGetValue(memberExpression.Expression, out var propertyInstance))
                                    {
                                        value = property.GetValue(propertyInstance, null);
                                        return true;
                                    }
                                }
                                break;
                        }
                        break;
                }
                value = default(object);
                return false;
            }

            protected override Expression VisitExtension(Expression expression)
            {
                switch (expression)
                {
                    case ExpVar variable:
                        if (Values.TryGetValue(variable.Key, out var valueExpression))
                        {
                            if (!variable.Type.GetTypeInfo().IsAssignableFrom(valueExpression.Type))
                            {
                                throw new InvalidOperationException($"Variable {variable.Key} has type {variable.Type.FullName} which is not assignable from emplaced expression of type {valueExpression.Type.FullName}");
                            }
                            var visitedExpression = Visit(valueExpression);
                            return visitedExpression;
                        }
                        break;
                }
                return expression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression callExpression)
            {
                if (callExpression.Method.DeclaringType.Equals(typeof(ExpVar)))
                {
                    if (nameof(ExpVar.Call) == callExpression.Method.Name)
                    {
                        var args = callExpression.Arguments;
                        var funArg = Visit(args[0]);

                        if (TryGetValue(funArg, out var funValue) && funValue is LambdaExpression funExpr)
                        {
                            var newArgs = args.Skip(1).Select(Visit);
                            var substitutions = funExpr.Parameters
                                .Zip(newArgs, (k, v) => new KeyValuePair<ParameterExpression, Expression>(k, v))
                                .ToImmutableDictionary();

                            var substitutedExpression = new Substitution(substitutions).Visit(funExpr.Body);
                            return Visit(substitutedExpression);
                        }
                    }
                    if (nameof(ExpVar.Value) == callExpression.Method.Name)
                    {
                        var nameExpr = callExpression.Arguments[0];
                        if (nameExpr is ConstantExpression cnameExpr && cnameExpr.Value is string name)
                        {
                            return VisitExtension(ExpVar.Create(name, callExpression.Type));
                        }
                        throw new InvalidCastException("Misformed template variable");
                    }
                }
                return callExpression;
            }
        }
    }
}