
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using MowingMachine.Services;

namespace MowingMachine.Models
{
    public class Field
    {
        public Field(int fieldType, Offset offset, List<Field> neighborFields)
        {
            Id = Guid.NewGuid();
            Type = (FieldType) fieldType;
            NeighborFields = neighborFields;
            Offset = offset;
        }
        
        public Field(int fieldType, Offset offset)
        {
            Id = Guid.NewGuid();
            Type = (FieldType) fieldType;
            NeighborFields = null;
            Offset = offset;
        }

        public Guid Id { get; }
        public FieldType Type { get; }
        public Offset Offset { get;  }
        public List<Field>? NeighborFields { get; private set;  }
        public bool IsVisited { get; set;  }

        public void UpdateNeighbors(List<Field> neighborFields)
        {
            if (NeighborFields is not null)
                throw new ArgumentException("Tried to update existing neighbors.");
            
            NeighborFields = neighborFields;
        }
        
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
                int index = Offset.X == field.Offset.X + 1 ? 1 : 3;
                NeighborFields[index] = field;
            }
        }
    }
}