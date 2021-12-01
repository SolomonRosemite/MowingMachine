
using System;
using System.Collections;
using System.Collections.Generic;

namespace MowingMachine.Models
{
    public class Field
    {
        public Field(int fieldType, List<Field> neighborFields)
        {
            Id = Guid.NewGuid();
            Type = (FieldType) fieldType;
            NeighborFields = neighborFields;
        }
        
        public Field(int fieldType)
        {
            Id = Guid.NewGuid();
            Type = (FieldType) fieldType;
            NeighborFields = null;
        }

        public Guid Id { get; }
        public FieldType Type { get; }
        public List<Field>? NeighborFields { get; private set;  }
        public bool IsVisited { get; set;  }

        public void UpdateNeighbors(List<Field> neighborFields)
        {
            if (NeighborFields is not null)
            {
                throw new ArgumentException("Tried to update existing neighbors.");
            }
            
            NeighborFields = neighborFields;
        }
        
        public void UpdateFieldNeighbor(Field field, MoveDirection direction)
        {
            if (NeighborFields is null)
                throw new ArgumentException("Tried to update neighbors when list is null.");
            
            switch (direction)
            {
                case MoveDirection.Top:
                    NeighborFields[0] = field;
                    break;
                case MoveDirection.Right:
                    NeighborFields[1] = field;
                    break;
                case MoveDirection.Bottom:
                    NeighborFields[2] = field;
                    break;
                case MoveDirection.Left:
                    NeighborFields[3] = field;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}