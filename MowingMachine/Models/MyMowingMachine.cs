using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using MowingMachine.Services;

namespace MowingMachine.Models
{
    public class MyMowingMachine
    {
        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Field> _knownFields = new();

        // Contains information about the map
        private readonly MapManager _mapManager;

        // This field preserves the most recent field the mowing machine was on. Initially it will be the charging station.
        private FieldType _currentFieldType = FieldType.ChargingStation;
        
        private MoveDirection _currentFacingDirection = MoveDirection.Left;
        private MoveDirection _currentDirection = MoveDirection.Left;
        private Field _currentField;

        private bool _isGoingToChargingStation;
        private bool _isMowing;

        public MyMowingMachine(MapManager mapManager)
        {
            _mapManager = mapManager;
            
            // Add initial fields
            _currentField = GetField(_mapManager.GetFieldsOfView(), new Offset(0, 0), _currentFieldType);
            _knownFields.Add(_currentField);
        }

        private Field GetField(FieldOfView fov, Offset offset, FieldType test)
        {
            if (!_knownFields.Any())
            {
                test = FieldType.ChargingStation;
            }
            else
            {
                if (test == FieldType.ChargingStation)
                {
                    
                }
            }
            
            var offsetTop = offset.Add(0, 1);
            var offsetRight = offset.Add(1, 0);
            var offsetBottom = offset.Add(0, -1);
            var offsetLeft = offset.Add(-1, 0);
            
            var neighbors = new List<Field>
            {
                _knownFields.SingleOrDefault(f => f.Offset.CompareTo(offsetTop)) ?? new Field(fov.Top, offsetTop),
                _knownFields.SingleOrDefault(f => f.Offset.CompareTo(offsetRight)) ?? new Field(fov.Right, offsetRight),
                _knownFields.SingleOrDefault(f => f.Offset.CompareTo(offsetBottom)) ?? new Field(fov.Bottom, offsetBottom),
                _knownFields.SingleOrDefault(f => f.Offset.CompareTo(offsetLeft)) ?? new Field(fov.Left, offsetLeft),
            };
            
            // var neighbors = new List<Field>
            // {
            //     new(fov.Top, offset.Add(0, 1)),
            //     new(fov.Right, offset.Add(1, 0)),
            //     new(fov.Bottom, offset.Add(0, -1)),
            //     new(fov.Left, offset.Add(-1, 0)),
            // };
            
            return new Field((int)test, offset, neighbors);
        }

        public bool PerformMove()
        {
            if (!_mapManager.Verify())
                return false;
            
            if (_knownFields.All(f => f.IsVisited))
            {
                Complete();
                return true;
            }
            
            MowGrass();
            return false;
        }
        
        private MowingStep CalculateMove(MoveDirection direction)
        {
            var turns = new Queue<MoveDirection>();
            
            if (_currentFacingDirection != direction)
                turns = CalculateTurn(_currentFacingDirection, direction, turns);

            return new MowingStep(turns, direction, Constants.TranslateMoveToExpense(_currentFieldType) + turns.Count * Constants.TurnExpense);
        }
        
        private Field Move(MowingStep step, Offset newOffset, FieldType? updatePrevFieldType = null)
        {
            NoteNextMove(step);
            _currentFieldType = _mapManager.MoveMowingMachine(step, updatePrevFieldType ?? _currentFieldType);

            var type = updatePrevFieldType ?? _currentFieldType;
            
            if (_knownFields.Any() && type == FieldType.ChargingStation)
                return GetField(_mapManager.GetFieldsOfView(), newOffset, FieldType.MowedLawn);
            return GetField(_mapManager.GetFieldsOfView(), newOffset, type);
        }

        private static Queue<MoveDirection> CalculateTurn(MoveDirection direction, MoveDirection finalDirection, Queue<MoveDirection> moves)
        {
            if (direction == finalDirection)
                return moves;

            int currentDir = (int) direction;
            int finalDir = (int) finalDirection;

            var x = Math.Min(Math.Abs(currentDir - finalDir), Math.Abs(finalDir - finalDir));

            if (x == 6)
            {
                switch (direction)
                {
                    case MoveDirection.Top:
                    case MoveDirection.Bottom:
                        direction = MoveDirection.Left;
                        moves.Enqueue(MoveDirection.Left);
                        break;
                    case MoveDirection.Right:
                    case MoveDirection.Left:
                        direction = MoveDirection.Top;
                        moves.Enqueue(MoveDirection.Top);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }

                return CalculateTurn(direction, finalDirection, moves);
            }
            
            moves.Enqueue(finalDirection);
            return moves;
        }

        private void NoteNextMove(MowingStep step)
        {
            // Todo: Save the move in there giving data structure.
        }

        private void Complete()
        {
            // Todo: Maybe double check if all the grass was mowed.
            Console.WriteLine("Mowing complete!");
        }

        private bool NeedsToRefuel(MowingStep step)
        {
            // Todo: Check one step ahead if the fuel would be enough to go back to the charging station.
            return false;
        }

        private void MowGrass()
        {
            // If we are in the process of going back to the charging station, we dont want to interrupt
            if (_isGoingToChargingStation)
            {
                MoveToChargingStation();
                return;
            }

            var nextField = _knownFields.FindLast(f => !f.IsVisited);
            if (nextField is null)
                return;

            // Get next field to move on
            if (!GetNearbyField(nextField, out var direction))
            {
                nextField.IsVisited = true;
                
                var lastKnownSplit = _knownFields.LastOrDefault(f =>
                    f.NeighborFields != null &&
                    f.NeighborFields.Any(nf => nf.Type is FieldType.Grass or FieldType.Sand));
                
                if (lastKnownSplit == null)
                {
                    // We are complete
                    return;
                }

                var index = _knownFields.FindIndex(f => f.Id == lastKnownSplit.Id);
                
                
                // Todo: Continue here. Maybe we should only add items that are not visited? 
                // If so, dont use the visited variable, use the offset and see if there are any other with the same offset as the current neighbor.
                _knownFields.AddRange(lastKnownSplit.NeighborFields!);
                
                MowGrass();
                return;
            }

            // Calculate if the fuel is enough for performing the step 
            var step = CalculateMove(direction!.Value);
            var needsToRefuelFirst = NeedsToRefuel(step);

            if (needsToRefuelFirst)
            {
                MoveToChargingStation();
                return;
            }

            // Update values
            _currentFacingDirection = direction.Value;
            _currentField = Move(step, _knownFields.Last().Offset.Add(new Offset(direction.Value)),
                _currentFieldType is FieldType.Grass ? FieldType.MowedLawn : _currentFieldType);
            // _currentField.IsVisited = true;
            nextField.IsVisited = true;

            // Update neighbor fields
            _knownFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
            _knownFields.Add(_currentField);
        }

        private bool GetNearbyField(Field nextField, out MoveDirection? moveDirection)
        {
            // Todo: Are _currentField and nextField not always the same anyways??
            if (_currentField != nextField)
            {
                
            }
            
            if (_currentField.NeighborFields is not null)
            {
                for (int i = 0; i < _currentField.NeighborFields.Count; i++)
                {
                    var field = _currentField.NeighborFields[i];
                    
                    if ((int) field.Type == -1 || field.Type is FieldType.Water or FieldType.MowedLawn or FieldType.ChargingStation)
                    // if ((int) field.Type == -1 || field.Type is FieldType.Water or FieldType.MowedLawn)
                        continue;
                    
                    moveDirection = i switch
                    {
                        0 => MoveDirection.Top,
                        1 => MoveDirection.Right,
                        2 => MoveDirection.Bottom,
                        3 => MoveDirection.Left,
                        _ => throw new Exception(),
                    };

                    return true;
                }
            }

            // The direction here doesn't effect anything. It just needs it be initialized for the code to compile. 
            moveDirection = MoveDirection.Top;
            return false;
        }

        private void MoveToChargingStation()
        {
            _isGoingToChargingStation = true;
            
            var fov = _mapManager.GetFieldsOfView();

            if (fov.CenterCasted is FieldType.ChargingStation)
            {
                // Todo: Recharge here.

                _isGoingToChargingStation = false;
                return;
            }
            
            // Todo: Keep moving to the charging station.
        }
    }
}