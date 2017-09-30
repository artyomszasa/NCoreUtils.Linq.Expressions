using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Linq.Expressions
{
    public static partial class ExpTemplate
    {
        internal class Substitution : ExpressionVisitor
        {
            public IReadOnlyDictionary<ParameterExpression, Expression> Substitutions { get; }

            public Substitution(IReadOnlyDictionary<ParameterExpression, Expression> substitutions)
                => Substitutions = substitutions ?? throw new ArgumentNullException(nameof(substitutions));

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (Substitutions.TryGetValue(parameterExpression, out var substitution))
                {
                    return substitution;
                }
                return parameterExpression;
            }
        }
    }
}