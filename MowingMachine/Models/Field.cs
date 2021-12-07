
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class Field
    {
        public Field(FieldType fieldType, Offset offset, List<Field> neighborFields)
        {
            Type = fieldType;
            NeighborFields = neighborFields;
            Offset = offset;
        }
        
        public Field(FieldType fieldType, Offset offset)
        {
            Type = fieldType;
            NeighborFields = null;
            Offset = offset;
        }

        public FieldType Type { get; }
        public Offset Offset { get;  }
        public List<Field>? NeighborFields { get; private set;  }
        public bool IsVisited { get; set;  }

        public void UpdateFieldNeighbor(Field field)
        {
            if (NeighborFields is null)
                throw new ArgumentException("Tried to update neighbors but list is null.");
            
            if (!Offset.AreNeighbors(field.Offset))
                return;

            // Update neighbor
            if (Offset.X == field.Offset.X)
            {
                int index = Offset.Y == field.Offset.Y + 1 ? 2 : 0;
                NeighborFields[index] = field;
            }
            else
            {
                int index = Offset.X == field.Offset.X + 1 ? 3 : 1;
                NeighborFields[index] = field;
            }
        }

        public bool CanBeWalkedOn()
        {
            if ((int) Type == -1)
                return false;

            return Type is not FieldType.Water;
        }
    }
}