using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
    public class EntityTests
    {
        private class TestDomainId : DomainId
        {
            public TestDomainId(string value) : base(value)
            {
            }
        }

        private class TestEntity : Entity
        {
            public TestEntity(IDomainId id) : base(id)
            {
            }
        }

        [Fact]
        public void Entity_Equals_ReturnsTrue()
        {
            var id = new TestDomainId("123");
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            Assert.True(entity1.Equals(entity2));
        }

        [Fact]
        public void Entity_Equals_ReturnsFalse_WhenIdsDiffer()
        {
            var entity1 = new TestEntity(new TestDomainId("123"));
            var entity2 = new TestEntity(new TestDomainId("456"));

            Assert.False(entity1.Equals(entity2));
        }

        [Fact]
        public void Entity_EqualityOperator_ReturnsTrue()
        {
            var id = new TestDomainId("123");
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            Assert.True(entity1 == entity2);
        }

        [Fact]
        public void Entity_EqualityOperator_ReturnsFalse_WhenIdsDiffer()
        {
            var entity1 = new TestEntity(new TestDomainId("123"));
            var entity2 = new TestEntity(new TestDomainId("456"));

            Assert.False(entity1 == entity2);
        }

        [Fact]
        public void Entity_InequalityOperator_ReturnsFalse()
        {
            var id = new TestDomainId("123");
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            Assert.False(entity1 != entity2);
        }

        [Fact]
        public void Entity_InequalityOperator_ReturnsTrue_WhenIdsDiffer()
        {
            var entity1 = new TestEntity(new TestDomainId("123"));
            var entity2 = new TestEntity(new TestDomainId("456"));

            Assert.True(entity1 != entity2);
        }

        [Fact]
        public void Entity_BothNull_ReturnsTrue()
        {
            TestEntity? entity1 = null;
            TestEntity? entity2 = null;

            Assert.True(entity1 == entity2);
        }

        [Fact]
        public void Entity_OneIsNull_ReturnsFalse()
        {
            TestEntity? entity1 = null;
            var entity2 = new TestEntity(new TestDomainId("123"));

            Assert.False(entity1 == entity2);
        }

        [Fact]
        public void Entity_GetHashCode_ReturnsIdValueHashCode()
        {
            string value = "123";
            var id = new TestDomainId(value);
            var entity = new TestEntity(id);

            Assert.Equal(value.GetHashCode(), entity.GetHashCode());
        }
    }
}