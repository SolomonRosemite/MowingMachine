using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Windows;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MyMowingMachine
    {
        private readonly Queue<MowingStep> _mowingSteps = new();

        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Field> _discoveredFields = new();

        private readonly List<(MowingStep, List<Field>, Offset)> _moves = new();

        // Contains information about the map
        private readonly MapManager _mapManager;

        // This field preserves the most recent field the mowing machine was on. Initially it will be the charging station.
        private FieldType _currentFieldType = FieldType.ChargingStation;
        
        private MoveDirection _currentFacingDirection = MoveDirection.Left;
        private Field _currentField;

        private Queue<MowingStep> _pathTheChargingStation = new();
        
        private bool _finalRunToChargingStation;
        private Offset _tempOffset;
        private double _charge;

        public MyMowingMachine(MapManager mapManager, double charge)
        {
            _mapManager = mapManager;
            _charge = charge;
            
            // Add initial fields
            _currentField = GetField(_mapManager.GetFieldsOfView(), new Offset(0, 0), FieldType.ChargingStation);
            _discoveredFields.Add(_currentField);
        }

        private Field GetField(FieldOfView fov, Offset offset, FieldType fieldType)
        {
            var offsetTop = offset.Add(0, 1);
            var offsetRight = offset.Add(1, 0);
            var offsetBottom = offset.Add(0, -1);
            var offsetLeft = offset.Add(-1, 0);

            var neighbors = new List<Field>
            {
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetTop)) ?? new Field(fov.TopCasted, offsetTop),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetRight)) ?? new Field(fov.RightCasted, offsetRight),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetBottom)) ?? new Field(fov.BottomCasted, offsetBottom),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetLeft)) ?? new Field(fov.LeftCasted, offsetLeft),
            };
            
            return new Field(fieldType, offset, neighbors);
        }

        private int test2 = 40;
        
        public bool PerformMove()
        {
            if (!_mapManager.Verify())
                return false;

            if (_mowingSteps.Any())
            {
                _tempOffset ??= _currentField.Offset;
                
                var step = _mowingSteps.Dequeue();
                var (x, y) = step.MoveDirection.TranslateDirectionToOffset();
                _tempOffset = _tempOffset.Add(new Offset(x, y));
                
                _currentFieldType = _mapManager.MoveMowingMachine(step, _currentFieldType is FieldType.Grass ? FieldType.MowedLawn : _currentFieldType);

                if (!_mowingSteps.Any())
                {
                    _currentField = GetField(_mapManager.GetFieldsOfView(), _tempOffset, _currentFieldType);
                    _currentField.IsVisited = true;
                    
                    if (_discoveredFields.Any(f => f.Offset.CompareTo(_currentField.Offset)))
                    {
                        _discoveredFields.Move(_discoveredFields.First(f => f.Offset.CompareTo(_currentField.Offset)), _discoveredFields.Count);
                    }
                    else
                    {
                        // Update neighbor fields
                        _discoveredFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
                        _discoveredFields.Add(_currentField);

                        _tempOffset = null;
                    }
                }
                
                return false;
            }
            
            if (_discoveredFields.All(f => f.IsVisited))
                return true;
            
            if (_pathTheChargingStation.Any() || _finalRunToChargingStation)
            {
                MoveToChargingStation();
                return false;
            }

            var calculatedSteps = CalculateNextMove();

            if (!calculatedSteps.Any())
            {
                Complete();
                return true;
            }

            var a = A(calculatedSteps.Last().MoveDirection, _currentField.Offset);
            
            var needsToRefuelFirst = NeedsToRefuel(calculatedSteps, a);
            if (needsToRefuelFirst)
            {
                foreach (var mowingStep in a)
                {
                    _pathTheChargingStation.Enqueue(mowingStep);
                }
                
                MoveToChargingStation();
                return false;
            }
            
            PerformStep(calculatedSteps);
            return false;
        }

        private IEnumerable<MowingStep> A(MoveDirection startingDirection, Offset offset)
        {
            var path = CalculatePathToGoal(_currentField.Offset,
                _discoveredFields.First(f => f.Type == FieldType.ChargingStation)
                    .Offset);
            
            path.Add(_currentField.Offset);

            var steps = new List<MowingStep>();
            var currentDirection = startingDirection;
            for (int i = path.Count - 2; i >= 0; i--)
            {
                var direction = path[i].Subtract(path[i + 1]).TranslateOffsetToDirection();

                var step = CalculateStepExpense(direction,
                    currentDirection,
                    _discoveredFields.First(f => f.Offset.CompareTo(path[i])).Type);

                currentDirection = direction;
                steps.Add(step);
            }

            return steps;
        }

        private static MowingStep CalculateStepExpense(MoveDirection direction, MoveDirection currentFacingDirection, FieldType currentFieldType)
        {
            var turns = new Queue<MoveDirection>();
            
            if (currentFacingDirection != direction)
                turns = CalculateTurn(currentFacingDirection, direction, turns);

            return new MowingStep(turns, direction, currentFieldType);
        }
        
        private Field Move(MowingStep step, Offset newOffset, FieldType? updatePrevFieldType = null)
        {
            _currentFieldType = _mapManager.MoveMowingMachine(step, updatePrevFieldType ?? _currentFieldType);

            var type = updatePrevFieldType ?? _currentFieldType;
            
            if (_discoveredFields.Any() && type == FieldType.ChargingStation)
                return GetField(_mapManager.GetFieldsOfView(), newOffset, FieldType.MowedLawn);
         
            return GetField(_mapManager.GetFieldsOfView(), newOffset, type);
        }

        private static Queue<MoveDirection> CalculateTurn(MoveDirection direction, MoveDirection finalDirection, Queue<MoveDirection> moves)
        {
            if (direction == finalDirection)
                return moves;

            if (direction.InvertDirection() == finalDirection)
            {
                switch (direction)
                {
                    case MoveDirection.Top:
                    case MoveDirection.Bottom:
                        moves.Enqueue(MoveDirection.Left);
                        break;
                    case MoveDirection.Right:
                    case MoveDirection.Left:
                        moves.Enqueue(MoveDirection.Top);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }
            
            moves.Enqueue(finalDirection);
            return moves;
        }

        private void NoteNextMove(MowingStep step, Offset offset)
        {
            for (var i = 0; i < _moves.Count; i++)
            {
                var (_, offsets, relatedOffset) = _moves[i];
                
                var neighbors = _discoveredFields.Single(f => f.Offset.CompareTo(relatedOffset)).NeighborFields;

                if (neighbors is null)
                    continue;
                
                offsets.Clear();
                offsets.AddRange(neighbors.Where(nf => !nf.IsVisited && nf.CanBeWalkedOn() && nf.Type != FieldType.ChargingStation));
            }
            
            var unvisitedFields = _discoveredFields.Single(f => f.Offset.CompareTo(offset))
                .NeighborFields!.Where(nf => !nf.IsVisited && nf.CanBeWalkedOn() && nf.Type != FieldType.ChargingStation)
                .ToList();
            
            _moves.Add((step, unvisitedFields, offset));
        }

        private void Complete()
        {
            // Todo: Maybe double check if all the grass was mowed.
            Console.WriteLine("Mowing complete!");
        }

        private bool NeedsToRefuel(IEnumerable<MowingStep> stepsToNextField, IEnumerable<MowingStep> stepsToChangingStation)
        {
            var totalRequestEnergy = stepsToNextField
                                         .Select(s => s.TotalEnergyExpense).Sum() +
                                     stepsToChangingStation
                                         .Select(s => s.TotalEnergyExpense).Sum();
            return test2-- == 1;
            // return _charge >= totalRequestEnergy;
        }

        private void PerformStep(List<MowingStep> steps)
        {
            steps.ForEach(_mowingSteps.Enqueue);
            
            var nextStep = _mowingSteps.Dequeue();
            
            _currentFacingDirection = nextStep.MoveDirection;

            var oldOffset = _discoveredFields.Last().Offset;
            var newOffset = oldOffset.Add(new Offset(nextStep.MoveDirection));
            
            _currentField = Move(nextStep, newOffset,
                _currentFieldType is FieldType.Grass ? FieldType.MowedLawn : _currentFieldType);
            
            _currentField.IsVisited = true;

            if (_discoveredFields.Any(f => f.Offset.CompareTo(_currentField.Offset)))
            {
                _discoveredFields.Move(_discoveredFields.First(f => f.Offset.CompareTo(_currentField.Offset)), _discoveredFields.Count);
                NoteNextMove(nextStep, newOffset);
                return;
            }
            
            // Update neighbor fields
            _discoveredFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
            _discoveredFields.Add(_currentField);
            
            NoteNextMove(nextStep, newOffset);
        }

        private List<MowingStep> CalculateNextMove()
        {
            var successful = GetNextNeighborField(out var values);
            var (_, direction) = values;
            
            if (!successful)
            {
                var steps = new List<MowingStep>();
                var currentDirection = _currentFacingDirection;
                var currentlyStandFieldType = _currentFieldType;

                for (int i = _moves.Count - 1; i >= 0; i--)
                {
                    var (prevFieldStep, neighbors, offset) = _moves[i];

                    if (!neighbors.Any())
                    {
                        var step = CalculateStepExpense(prevFieldStep.MoveDirection.InvertDirection(), currentDirection, currentlyStandFieldType);
                        currentlyStandFieldType = prevFieldStep.FieldType;
                        steps.Add(step);

                        currentDirection = step.MoveDirection;
                    }
                    
                    if (neighbors.Any())
                    {
                        var calculatedOffset = neighbors
                            .Select(nf => nf.Offset.Subtract(offset)).First();
                        
                        var finalDirection = calculatedOffset.TranslateOffsetToDirection();
                        
                        var stepFinal = CalculateStepExpense(finalDirection, currentDirection, prevFieldStep.FieldType);
                        steps.Add(stepFinal);
                        return steps;
                    }
                }

                return new List<MowingStep>();
            }
            
            var nextStep = CalculateStepExpense(direction, _currentFacingDirection, _currentFieldType);

            return new List<MowingStep> { nextStep };

            bool GetNextNeighborField(out (Field, MoveDirection) result)
            {
                var fieldIndex = _currentField.NeighborFields?.FindIndex(f => !f.IsVisited && f.CanBeWalkedOn() && f.Type is not FieldType.ChargingStation);
                result = (null, MoveDirection.Bottom);
                
                if (fieldIndex is null or -1)
                    return false;

                result = (_currentField.NeighborFields[fieldIndex.Value], fieldIndex switch
                {
                    0 => MoveDirection.Top,
                    1 => MoveDirection.Right,
                    2 => MoveDirection.Bottom,
                    3 => MoveDirection.Left,
                    _ => throw new Exception(),
                });
                return true;
            }
        }
        
        private void MoveToChargingStation()
        {
            if (!_pathTheChargingStation.Any())
            {
                _finalRunToChargingStation = false;
                _currentField.Offset.UpdateOffset(new Offset(0, 0));
                return;
            }
            
            var nextStep = _pathTheChargingStation.Dequeue();

            _finalRunToChargingStation = !_pathTheChargingStation.Any();
            
            _currentFacingDirection = nextStep.MoveDirection;

            // var oldOffset = _discoveredFields.Last().Offset;
            var newOffset = _currentField.Offset.Subtract(new Offset(nextStep.MoveDirection));
            
            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }
        
        // Breadth first search
        private List<Offset> CalculatePathToGoal(Offset start, Offset goal)
        {
            var visitedCoordinates = new Dictionary<string, Offset>();
            var nextCoordinatesToVisit = new Queue<OffsetInfo>();
            
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(start, null));
        
            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();
        
                if (GetNeighborCells(cellInfo))
                    break;
            }
            
            var tracedPath = new List<Offset>();
        
            var currenCoordinate = goal;
            while (visitedCoordinates.TryGetValue(currenCoordinate.ToString(), out var coord))
            {
                if (coord == null)
                    break;
                
                tracedPath.Add(currenCoordinate);
                currenCoordinate = coord;
            }
            
            return tracedPath;
        
            bool GetNeighborCells(OffsetInfo info)
            {
                if (!IsValidField(info.CurrentOffset))
                    return false;
        
                // If it already exists, dont add again
                if (visitedCoordinates.ContainsKey(info.CurrentOffset.ToString()))
                   return false;
        
                visitedCoordinates[info.CurrentOffset.ToString()] = info.PrevOffset;
        
                if (info.CurrentOffset.X == goal.X && info.CurrentOffset.Y == goal.Y)
                    return true;
            
                nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y + 1, info.CurrentOffset));
                nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X + 1, info.CurrentOffset.Y, info.CurrentOffset));
                nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y - 1, info.CurrentOffset));
                nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X - 1, info.CurrentOffset.Y, info.CurrentOffset));
                return false;
            }
        
            bool IsValidField(Offset offset)
            {
                var value = _discoveredFields
                    .FirstOrDefault(f => f.Offset.CompareTo(new Offset(offset.X, offset.Y)))?.Type;

                value ??= FieldType.Water;
                
                return (int) value != -1 && (FieldType) value is not FieldType.Water;
            }
        }
    }
}