using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;
using Xunit.v3;

namespace Zammad.Client.IntegrationTests.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}

public class TestOrderer : ITestCaseOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase => testCases.OrderBy(t => t.GetOrder()).ToList();
}

public static class ITestCaseExtensions
{
    public static int GetOrder(this ITestCase testCase)
    {
        var fullName = testCase.TestClassName;
        if (fullName is null)
        {
            return int.MaxValue;
        }

        // get order attribute from class
        var type = Type.GetType(fullName);
        if (type is null)
        {
            throw new InvalidOperationException($"Type '{fullName}' not found.");
        }

        var method = type.GetMethod(testCase.TestMethod!.MethodName);
        if (method is null)
        {
            throw new InvalidOperationException(
                $"Method '{testCase.TestMethod.MethodName}' not found in type '{fullName}'."
            );
        }

        var orderAttribute =
            method.GetCustomAttributes(typeof(OrderAttribute), false).FirstOrDefault() as OrderAttribute;
        return orderAttribute?.Order ?? int.MaxValue;
    }
}

public static class IDictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
    {
        if (dictionary.TryGetValue(key, out var result) == false)
        {
            result = new TValue();
            dictionary.Add(key, result);
        }

        return result;
    }
}
