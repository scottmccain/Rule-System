using System.Collections.Generic;

namespace RuleSystem
{
    public interface IMemoryStore
    {
        bool ModifiedByLastAction { get; }

        void Add(string element);

        void Remove(string element);

        IEnumerable<MemoryElement> Elements { get;  }

        int Length { get;  }

        MemoryElement this[int index] { get; }
    }
}