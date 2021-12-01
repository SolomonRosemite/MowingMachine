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
        
        // Here we keep track on what coordinates the grass was already mowed at.
        private readonly List<Coordinate> _mowedCoordinates = new();

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
            _currentField = GetField(_mapManager.GetFieldsOfView());
            _knownFields.Add(_currentField);
        }

        private Field GetField(FieldOfView fov)
        {
            var neighbors = new List<Field>
            {
                new(fov.Top),
                new(fov.Right),
                new(fov.Bottom),
                new(fov.Left),
            };
            
            return new Field(fov.Center, neighbors);
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
        
        private Field Move(MowingStep step, FieldType? updatePrevFieldType = null)
        {
            NoteNextMove(step);
            _currentFieldType = _mapManager.MoveMowingMachine(step, updatePrevFieldType ?? _currentFieldType);

            return GetField(_mapManager.GetFieldsOfView());
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
            if (_isGoingToChargingStation)
            {
                MoveToChargingStation();
                return;
            }

            var nextField = _knownFields.FindLast(f => !f.IsVisited);

            if (nextField is null)
                return;

            if (!GetNearbyField(nextField, out var direction))
            {
                nextField.IsVisited = true;

                var lastKnownSplit = _knownFields.LastOrDefault(f =>
                    f.NeighborFields != null &&
                    f.NeighborFields.Any(nf =>
                        nf.Type is FieldType.Grass or FieldType.Sand or FieldType.ChargingStation or FieldType
                            .ChargingStation or FieldType.MowingMachine));
                    // f.NeighborFields != null && f.NeighborFields.Any(nf => nf.Type is not FieldType.Water or FieldType.MowedLawn || (int) nf.Type == -1));
                
                if (lastKnownSplit == null)
                {
                    // We are complete
                    return;
                }

                var index = _knownFields.FindIndex(f => f.Id == lastKnownSplit.Id);
                
                _knownFields.AddRange(lastKnownSplit.NeighborFields!);
                
                MowGrass();
                return;
            }

            if (!direction.HasValue)
                throw new Exception("Direction was null.");

            var step = CalculateMove(direction.Value);
            
            var needsToRefuelFirst = NeedsToRefuel(step);

            if (needsToRefuelFirst)
            {
                MoveToChargingStation();
                return;
            }

            _currentFacingDirection = direction.Value;
            _currentField = Move(step, FieldType.MowedLawn);

            nextField.IsVisited = true;

            if (_knownFields.Count > 0)
            {
                _knownFields.Last().UpdateFieldNeighbor(_currentField, direction.Value);
                _knownFields.Add(_currentField);
            }
        }

        private bool GetNearbyField(Field nextField, out MoveDirection? moveDirection)
        {
            // Are not _currentField and nextField always the same??
            if (_currentField != nextField)
            {
                
            }
            
            if (_currentField.NeighborFields is not null)
            {
                for (int i = 0; i < _currentField.NeighborFields.Count; i++)
                {
                    var field = _currentField.NeighborFields[i];
                    if ((int) field.Type == -1 || field.Type is FieldType.Water or FieldType.MowedLawn)
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

            moveDirection = null;

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