using System;
using System.Linq.Expressions;

namespace NCoreUtils.Linq.Expressions
{
    public abstract class ExpVar : Expression
    {
        internal static ExpVar Create(string key, Type type)
        {
            return (ExpVar)Activator.CreateInstance(typeof(ExpVar<>).MakeGenericType(type), new object[] { key });
        }

        public static T Value<T>(string key) => throw new InvalidOperationException("Method not intended to be invoked at runtime.");

        public static TResult Call<TArg, TResult>(Expression<Func<TArg, TResult>> expr, TArg arg)
            => throw new InvalidOperationException("Method not intended to be invoked at runtime.");

        public static TResult Call<TArg1, TArg2, TResult>(Expression<Func<TArg1, TArg2, TResult>> expr, TArg1 arg1, TArg2 arg2)
            => throw new InvalidOperationException("Method not intended to be invoked at runtime.");

        public static TResult Call<TArg1, TArg2, TArg3, TResult>(Expression<Func<TArg1, TArg2, TArg3, TResult>> expr, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            => throw new InvalidOperationException("Method not intended to be invoked at runtime.");

        public override ExpressionType NodeType => ExpressionType.Extension;

        public string Key { get; }

        public ExpVar(string key)
            => Key = key ?? throw new ArgumentNullException(nameof(key));

        public override string ToString() => $"TemplateVar({Key})";
    }

    public class ExpVar<T> : ExpVar
    {
        public override Type Type => typeof(T);

        public ExpVar(string key) : base(key) { }
    }
}