using System.Collections.Generic;
using Neyro.AppMetrics.Extensions;
using NUnit.Framework;

namespace UnitTests
{
    public class CounterPayloadTests
    {
        public class WhenPayloadContainsMetadata
        {
            public class AndMetadataUseIsEnabled
            {
                [Test]
                public void MetadataIsCopiedToCounterPayload()
                {
                    var input = new Dictionary<string, object>()
                    {
                        {"Name", "test-counter-event"}, {"CounterType", "Sum"}, {"Increment", 1.0},
                        {"Metadata", "key1:value1,key2:value2"}
                    };
                    var counterPayload = new CounterPayload(input, true);
                    Assert.That(counterPayload.Metadata.Keys.Count, Is.EqualTo(2));
                    Assert.That(counterPayload.Metadata.Values.Count, Is.EqualTo(2));
                }

                [Test]
                public void CounteryKeyContainsMetadata()
                {
                    var input = new Dictionary<string, object>()
                    {
                        {"Name", "test-counter-event"}, {"CounterType", "Sum"}, {"Increment", 1.0},
                        {"Metadata", "key1:value1,key2:value2"}
                    };
                    var counterPayload = new CounterPayload(input, true);
                    Assert.That(counterPayload.Key, Is.EqualTo("test-counter-eventkey1:value1,key2:value2"));
                }
            }
            
            public class AndMetadataUseIsDisabled
            {
                [Test]
                public void MetadataIsNotCopiedToCounterPayload()
                {
                    var input = new Dictionary<string, object>()
                    {
                        {"Name", "test-counter-event"}, {"CounterType", "Sum"}, {"Increment", 1.0},
                        {"Metadata", "key1:value1,key2:value2"}
                    };
                    var counterPayload = new CounterPayload(input, false);
                    Assert.IsNull(counterPayload.Metadata);
                }

                [Test]
                public void CounteryKeyDoesNotContainMetadata()
                {
                    var input = new Dictionary<string, object>()
                    {
                        {"Name", "test-counter-event"}, {"CounterType", "Sum"}, {"Increment", 1.0},
                        {"Metadata", "key1:value1,key2:value2"}
                    };
                    var counterPayload = new CounterPayload(input, false);
                    Assert.That(counterPayload.Key, Is.EqualTo("test-counter-event"));
                }
            }
        }
    }
}