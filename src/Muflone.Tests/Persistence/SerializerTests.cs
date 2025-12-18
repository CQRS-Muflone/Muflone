using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Muflone.Tests.Persistence
{
	public class SerializerTests
	{
		private class TestId : DomainId
		{
			public TestId(string value) : base(value)
			{
			}
		}

		private class ImageId : DomainId
		{
			public ImageId(string value) : base(value)
			{
			}
		}

		private class TestEvent : DomainEvent
		{
			public string StringProperty { get; }
			public int IntProperty { get; }
			public Guid GuidProperty { get; }
			public DateTime DateTimeProperty { get; }
			// Aggiungiamo una nuova proprietà di tipo IDomainId
			public IDomainId? ImageId { get; }

			[JsonConstructor]
			public TestEvent(TestId aggregateId, string stringProperty, int intProperty, Guid guidProperty, DateTime dateTimeProperty, ImageId? imageId = null)
					: base(aggregateId)
			{
				StringProperty = stringProperty;
				IntProperty = intProperty;
				GuidProperty = guidProperty;
				DateTimeProperty = dateTimeProperty;
				ImageId = imageId;
			}

			public TestEvent(TestId aggregateId, string stringProperty, int intProperty, Guid guidProperty, DateTime dateTimeProperty, Account who, ImageId? imageId = null)
					: base(aggregateId, who)
			{
				StringProperty = stringProperty;
				IntProperty = intProperty;
				GuidProperty = guidProperty;
				DateTimeProperty = dateTimeProperty;
				ImageId = imageId;
			}
		}

		[Fact]
		public async Task Serializer_CanSerializeAndDeserialize_ComplexDomainEvent()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateId = new TestId(Guid.NewGuid().ToString());
			var who = new Account("user-123", "Test User");
			var guidValue = Guid.NewGuid();
			var dateTimeValue = DateTime.UtcNow;
			var originalEvent = new TestEvent(aggregateId, "TestString", 42, guidValue, dateTimeValue, who);

			// Act
			var serialized = await serializer.SerializeAsync(originalEvent);
			var deserialized = await serializer.DeserializeAsync<TestEvent>(serialized);

			// Assert
			Assert.NotNull(deserialized);
			Assert.Equal(originalEvent.AggregateId.Value, deserialized.AggregateId.Value);
			Assert.Equal(originalEvent.Headers.AggregateType, deserialized.Headers.AggregateType);
			Assert.Equal(originalEvent.Headers.CorrelationId, deserialized.Headers.CorrelationId);
			Assert.Equal(originalEvent.Headers.Who.Id, deserialized.Headers.Who.Id);
			Assert.Equal(originalEvent.Headers.Who.Name, deserialized.Headers.Who.Name);
			Assert.Equal(originalEvent.Headers.When.Value.ToString("o"),
									 deserialized.Headers.When.Value.ToString("o")); // Compare formatted date strings

			// Check custom properties
			Assert.Equal(originalEvent.StringProperty, deserialized.StringProperty);
			Assert.Equal(originalEvent.IntProperty, deserialized.IntProperty);
			Assert.Equal(originalEvent.GuidProperty, deserialized.GuidProperty);
			Assert.Equal(originalEvent.DateTimeProperty.ToUniversalTime().ToString("o"),
									 deserialized.DateTimeProperty.ToUniversalTime().ToString("o"));
		}

		[Fact]
		public async Task Serializer_WithTypeNameHandling_CanDeserializeMultipleDomainIds()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateId = new TestId(Guid.NewGuid().ToString());
			var imageId = new ImageId(Guid.NewGuid().ToString());
			var originalEvent = new TestEvent(aggregateId, "TestString", 42, Guid.NewGuid(), DateTime.UtcNow, imageId);

			// Act
			var serialized = await serializer.SerializeAsync(originalEvent);
			// Assert: Il JSON serializzato ora contiene le informazioni sul tipo ($type)
			Assert.Contains("Muflone.Tests.Persistence.SerializerTests+TestId", serialized);
			Assert.Contains("Muflone.Tests.Persistence.SerializerTests+ImageId", serialized);

			var deserialized = await serializer.DeserializeAsync<TestEvent>(serialized);

			// Assert
			Assert.NotNull(deserialized);
			Assert.IsType<TestId>(deserialized.AggregateId);
			Assert.Equal(originalEvent.AggregateId.Value, deserialized.AggregateId.Value);

			Assert.NotNull(deserialized.ImageId);
			Assert.IsType<ImageId>(deserialized.ImageId);
			Assert.Equal(originalEvent.ImageId!.Value, deserialized.ImageId.Value);
		}

		[Fact]
		public async Task Serializer_IsBackwardCompatible_WithOldJsonFormat()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateIdValue = Guid.NewGuid().ToString();
			// Questo è un esempio di JSON come sarebbe stato salvato in produzione (senza $type)
			var oldJson = $@"{{
                ""StringProperty"": ""OldEvent"",
                ""IntProperty"": 101,
                ""GuidProperty"": ""{Guid.NewGuid()}"",
                ""DateTimeProperty"": ""{DateTime.UtcNow:o}"",
                ""ImageId"": null,
                ""AggregateId"": {{
                    ""Value"": ""{aggregateIdValue}"",
                }},
                ""Headers"": {{
                    ""Standards"": {{
                        ""CorrelationId"": ""{Guid.NewGuid()}"",
                        ""AccountId"": ""legacy-user"",
                        ""Who"": ""Legacy User"",
                        ""When"": ""{DateTime.UtcNow.Ticks}"",
                        ""AggregateType"": ""TestEvent""
                    }},
                    ""Customs"": {{}}
                }},
                ""Version"": 1,
                ""MessageId"": ""{Guid.NewGuid()}"",
            }}";

			// Act
			var deserialized = await serializer.DeserializeAsync<TestEvent>(oldJson);

			// Assert
			Assert.NotNull(deserialized);
			// Poiché il vecchio JSON non ha $type, il deserializzatore si basa sul tipo concreto
			// nel costruttore [JsonConstructor], che è TestId.
			Assert.IsType<TestId>(deserialized.AggregateId);
			Assert.Equal(aggregateIdValue, deserialized.AggregateId.Value);
			Assert.Equal("OldEvent", deserialized.StringProperty);
			Assert.Equal(101, deserialized.IntProperty);
		}

		[Fact]
		public async Task Serializer_SerializesTypeName_ForTypeResolution()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateId = new TestId(Guid.NewGuid().ToString());
			var originalEvent = new TestEvent(aggregateId, "TestString", 42, Guid.NewGuid(), DateTime.UtcNow);

			// Act
			var serialized = await serializer.SerializeAsync(originalEvent);

			// Assert
			// Verifichiamo che il JSON contenga i valori delle proprietà principali
			Assert.Contains("TestString", serialized);
			Assert.Contains("42", serialized);
			Assert.Contains("StringProperty", serialized);
			Assert.Contains("IntProperty", serialized);
		}

		[Fact]
		public async Task Serializer_PreservesMessageId_AfterDeserialization()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateId = new TestId(Guid.NewGuid().ToString());
			var originalEvent = new TestEvent(aggregateId, "TestString", 42, Guid.NewGuid(), DateTime.UtcNow);
			var messageId = originalEvent.MessageId;

			// Act
			var serialized = await serializer.SerializeAsync(originalEvent);
			var deserialized = await serializer.DeserializeAsync<TestEvent>(serialized);

			// Assert
			Assert.Equal(messageId, deserialized!.MessageId);
		}

		[Fact]
		public async Task Serializer_PreservesUserProperties_AfterDeserialization()
		{
			// Arrange
			var serializer = new Serializer();
			var aggregateId = new TestId(Guid.NewGuid().ToString());
			var originalEvent = new TestEvent(aggregateId, "TestString", 42, Guid.NewGuid(), DateTime.UtcNow);

			// Add custom user property
			originalEvent.UserProperties["CustomKey"] = "CustomValue";
			originalEvent.UserProperties["NumericProperty"] = 100;

			// Act
			var serialized = await serializer.SerializeAsync(originalEvent);
			var deserialized = await serializer.DeserializeAsync<TestEvent>(serialized);

			// Assert
			Assert.Equal("CustomValue", deserialized!.UserProperties["CustomKey"]);
			Assert.Equal(100L, Convert.ToInt64(deserialized.UserProperties["NumericProperty"])); // JSON serialization may change numeric types
		}
	}
}