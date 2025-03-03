using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Commons
{
    public static class GenericSpecification<T>
    {
        public static Expression<Func<T, bool>> HasEqual<TValue>(string attribute, TValue value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = GetPropertyExpression(parameter, attribute);
            var constant = Expression.Constant(value);
            var body = Expression.Equal(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> HasLike(string attribute, string value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = GetPropertyExpression(parameter, attribute);
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(value);
            var body = Expression.Call(property, method, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> HasGreaterThan<TValue>(string attribute, TValue value) where TValue : IComparable
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = GetPropertyExpression(parameter, attribute);
            var constant = Expression.Constant(value);
            var body = Expression.GreaterThan(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> HasLessThan<TValue>(string attribute, TValue value) where TValue : IComparable
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = GetPropertyExpression(parameter, attribute);
            var constant = Expression.Constant(value);
            var body = Expression.LessThan(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> AddSpecification<TValue>(Expression<Func<T, bool>> spec, TValue value, string attribute, string operatorType)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return spec;

            var newSpec = operatorType switch
            {
                "equal" => HasEqual(attribute, value),
                "like" => HasLike(attribute, value.ToString()),
                "greaterThan" => HasGreaterThan(attribute, (IComparable)value),
                "lessThan" => HasLessThan(attribute, (IComparable)value),
                _ => throw new ArgumentException("Unsupported operator: " + operatorType)
            };

            return spec == null ? newSpec : CombineExpressions(spec, newSpec);
        }

        public static Expression<Func<T, bool>> CombineExpressions(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression GetPropertyExpression(Expression parameter, string attribute)
        {
            Expression property = parameter;
            foreach (var part in attribute.Split('.'))
            {
                property = Expression.Property(property, part);
            }
            return property;
        }
    }
}
