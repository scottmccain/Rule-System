using System.Collections.Generic;
using System.Linq;

namespace RuleSystem
{
    public class ListMemoryStore : IMemoryStore
    {
        private readonly List<MemoryElement> _elements = new List<MemoryElement>();

        public bool ModifiedByLastAction { get; private set; }

        public void Add(string element)
        {
            var memoryElement = _elements.FirstOrDefault(p => p.Element.Equals(element));
            if (memoryElement != null)
            {
                memoryElement.IsActive = true;
                ModifiedByLastAction = false;
            }
            else
            {
                _elements.Add(new MemoryElement { IsActive = true, Element = element });
                ModifiedByLastAction = true;
            }
        }

        public void Remove(string element)
        {
            var query = from m in _elements
                        where m.IsActive && m.Element.Equals(element)
                        select m;

            ModifiedByLastAction = false;

            var listToRemove = query.ToList();
            foreach (var mem in listToRemove)
            {
                _elements.Remove(mem);
            }

            ModifiedByLastAction = listToRemove.Count > 0;
        }

        public IEnumerable<MemoryElement> Elements
        { get { return _elements.AsReadOnly().AsEnumerable(); } }

        public int Length
        {
            get { return _elements.Count;  }
        }

        public MemoryElement this[int index]
        {
            get { return _elements[index]; }
        }

    }
}
