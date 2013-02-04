using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesProcessing
{
    public class WorkingMemory
    {
        private const int ELEMENT_BLOCK_SIZE = 40;
        private MemoryElement [] elements = new MemoryElement[ELEMENT_BLOCK_SIZE];

        private int FindEmptySlot()
        {
            int i;

            for (i = 0; i < elements.Length; i++)
            {
                if (!elements[i].IsActive) break;
            }

            return i;
        }

        public bool AddCommand(string name)
        {
            int slot;

            /* Check to ensure that this element isn't already in working memory */
            for (slot = 0; slot < elements.Length; slot++)
            {
                if (elements[slot].IsActive)
                {
                    if (elements[slot].Element == name)
                    {
                        /* Element is already here, return */
                        return false;
                    }
                }
            }


            /* Add this element to working memory */
            slot = FindEmptySlot();

            if (slot > elements.Length)
            {
                GrowMemory();
            }

            //if (slot < MAX_ELEMENTS)
            {

                elements[slot].IsActive = true;
                elements[slot].Element = name;

                return true;
            }

            //return false;

        }

        private void GrowMemory()
        {
            MemoryElement[] tempArray = new MemoryElement[elements.Length];
            elements.CopyTo(tempArray, 0);

            int newSize = elements.Length + ELEMENT_BLOCK_SIZE;
            elements = new MemoryElement[newSize];

            tempArray.CopyTo(elements, 0);
        }
    }
}
