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
        internal class DiscoverVisitor : ExpressionVisitor
        {
            public Dictionary<string, Type> Variables { get; } = new Dictionary<string, Type>();

            void AddVariable(string key, Type type)
            {
                if (Variables.TryGetValue(key, out var type0))
                {
                    if (!type0.Equals(type))
                    {
                        throw new InvalidOperationException($"Variable {key} is already present in the template with different type.");
                    }
                }
                else
                {
                    Variables.Add(key, type);
                }
            }

            protected override Expression VisitExtension(Expression expression)
            {
                switch (expression)
                {
                    case ExpVar variable:
                        AddVariable(variable.Key, variable.Type);
                        break;
                }
                return expression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression callExpression)
            {
                if (callExpression.Method.DeclaringType.Equals(typeof(ExpVar)))
                {
                    if (nameof(ExpVar.Value) == callExpression.Method.Name)
                    {
                        var nameExpr = callExpression.Arguments[0];
                        if (nameExpr is ConstantExpression cnameExpr && cnameExpr.Value is string name)
                        {
                            AddVariable(name, callExpression.Type);
                        }
                        else
                        {
                            throw new InvalidCastException("Misformed template variable");
                        }
                    }
                }
                return callExpression;
            }
        }
    }
}