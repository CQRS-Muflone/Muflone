using System;
using System.Collections.Generic;
using System.Linq;

namespace Muflone.Core
{
    public abstract class CustomString<T> where T : CustomString<T>
    {
        public string Value { get; }

        protected CustomString(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new NullReferenceException(nameof(value));

            this.Value = value;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as T);
        }

        protected virtual IEnumerable<object> GetAttributesToIncludeInEqualityCheck()
        {
            return new List<object>();
        }

        public bool Equals(T other)
        {
            if (other == null) return false;

            return
                this.GetAttributesToIncludeInEqualityCheck()
                    .SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());
        }

        public static bool operator ==(CustomString<T> left, CustomString<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CustomString<T> left, CustomString<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return this.GetAttributesToIncludeInEqualityCheck()
                .Aggregate(17, (current, obj) => current * 31 + (obj?.GetHashCode() ?? 0));
        }

        public virtual Guid GetGuid()
        {
            Guid.TryParse(this.Value, out var valueGuid);
            return valueGuid;
        }
    }
}