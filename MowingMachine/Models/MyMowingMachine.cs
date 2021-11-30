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
        
        private MoveDirection _currentFacingDirection = MoveDirection.Center;
        private MoveDirection _currentDirection = MoveDirection.Center;
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

            var x = Math.Min(currentDir, finalDir);

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

        private bool NeedsToRefuel()
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

            if (!FieldIsInFov(nextField, out var direction))
            {
                // If not in field of view. Move one closer to the next field...
                // Remember to change the direction then.
            }

            var moves = CalculateMove(direction);
            
            var needsToRefuelFirst = NeedsToRefuel();

            if (needsToRefuelFirst)
            {
                MoveToChargingStation();
                return;
            }

            _currentFacingDirection = direction;
            var field = Move(moves, FieldType.MowedLawn);

            nextField.IsVisited = true;
            _knownFields.Add(field);
        }

        private bool FieldIsInFov(Field nextField, out MoveDirection moveDirection)
        {
            if (_currentField.NeighborFields is not null)
            {
                for (int i = 0; i < _currentField.NeighborFields.Count; i++)
                {
                    if (_currentField.NeighborFields[i].Id != nextField.Id)
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

            moveDirection = MoveDirection.Center;

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