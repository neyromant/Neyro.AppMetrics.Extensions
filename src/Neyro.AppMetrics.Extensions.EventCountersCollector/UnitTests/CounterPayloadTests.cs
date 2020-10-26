using System.Collections.Generic;
using Neyro.AppMetrics.Extensions;
using NUnit.Framework;

namespace UnitTests
{
    public class CounterPayloadTests
    {
        public class CanExtractMetadata
        {
            [Test]
            public void Test1()
            {
                var input = new Dictionary<string, object>()
                {
                    {"Name", "test-counter-event"}, {"CounterType", "Sum"}, {"Increment", 1.0},
                    {"Metadata", "key1:value1,key2:value2"}
                };
                var counterPayload = new CounterPayload(input);
                Assert.That(counterPayload.Metadata.Keys.Count, Is.EqualTo(2));
                Assert.That(counterPayload.Metadata.Values.Count, Is.EqualTo(2));
            }
        }
    }
}