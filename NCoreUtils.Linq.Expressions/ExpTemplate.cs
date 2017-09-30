using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Linq.Expressions
{
    public static partial class ExpTemplate
    {
        public static ExpTemplate<TDelegate> Create<TDelegate>(Expression<TDelegate> source)
        {
            var discoverVisitor = new DiscoverVisitor();
            discoverVisitor.Visit(source);
            return new ExpTemplate<TDelegate>(source, discoverVisitor.Variables.ToImmutableDictionary());
        }
    }

    public sealed class ExpTemplate<TDelegate>
    {
        public Expression<TDelegate> Template { get; }

        public ImmutableDictionary<string, Type> Variables { get; }

        internal ExpTemplate(Expression<TDelegate> template, ImmutableDictionary<string, Type> variables)
        {
            Template = template;
            Variables = variables;
        }

        public ExpTemplate<TDelegate> Emplace(IReadOnlyDictionary<string, Expression> values)
        {
            foreach (var kv in values)
            {
                if (Variables.TryGetValue(kv.Key, out var expectedType))
                {
                    if (!expectedType.GetTypeInfo().IsAssignableFrom(kv.Value.Type))
                    {
                        throw new ArgumentException("Eplaced value has incompatible type.", $"values[{kv.Key}]");
                    }
                }
            }
            var emplacedExpression = (Expression<TDelegate>)new ExpTemplate.EmplaceVisitor(values).Visit(Template);
            return ExpTemplate.Create(emplacedExpression);
        }

        public ExpTemplate<TDelegate> Emplace(string key, Expression value)
            => Emplace(new Dictionary<string, Expression> { { key,  value } });

        public Expression<TDelegate> ToExpression(IReadOnlyDictionary<string, Expression> values = null)
        {
            var template = null == values ? this : Emplace(values);
            if (template.Variables.Count > 0)
            {
                throw new InvalidOperationException($"Missing values for following variables: {string.Join(", ", template.Variables.Keys)}");
            }
            return template.Template;
        }

        public Expression<TDelegate> ToExpression(string key, Expression value)
            => ToExpression(new Dictionary<string, Expression> { { key,  value } });
    }
}