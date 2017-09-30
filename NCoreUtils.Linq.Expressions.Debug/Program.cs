using System;
using System.Linq.Expressions;

namespace NCoreUtils.Linq.Expressions.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var template0 = ExpTemplate.Create<Func<int, int>>(i => ExpVar.Value<int>("x") + i);
            var expr0 = template0.Emplace("x", Expression.Constant(4));
            Console.WriteLine(expr0.ToExpression());

            var templateExpr = template0.Template;
            var template1 = ExpTemplate.Create<Func<int, int>>(x => ExpVar.Call(templateExpr, x) + ExpVar.Value<int>("x"));
            var expr1 = template1.Emplace("x", Expression.Constant(4));
            Console.WriteLine(expr1.ToExpression());

        }
    }
}
