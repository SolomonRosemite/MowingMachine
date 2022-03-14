using System;
using System.Collections.Generic;
using System.Linq;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MyMowingMachine
    {
        private readonly Queue<MowingStep> _mowingSteps = new();

        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Field> _discoveredFields = new();

        private readonly Queue<MowingStep> _pathFromChargingStationToRecentPosition = new();

        private readonly Queue<MowingStep> _pathToChargingStation = new();

        // Contains information about the map
        private readonly MapManager _mapManager;

        // This field preserves the most recent field the mowing machine was on. Initially it will be the charging station.
        private FieldType _currentFieldType = FieldType.ChargingStation;
        
        private MoveDirection _currentFacingDirection = MoveDirection.Left;
        private Field _currentField;

        private bool _finalRunToChargingStation;
        private Offset _tempOffset;
        private readonly double _maxChange;
        private double _charge;
        
        public MyMowingMachine(MapManager mapManager, double maxChange)
        {
            _mapManager = mapManager;
            _maxChange = maxChange;
            _charge = maxChange;
            
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
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetTop) ?? new Field(fov.TopCasted, offsetTop),
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetRight) ?? new Field(fov.RightCasted, offsetRight),
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetBottom) ?? new Field(fov.BottomCasted, offsetBottom),
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetLeft) ?? new Field(fov.LeftCasted, offsetLeft),
            };
            
            return new Field(fieldType, offset, neighbors);
        }

        public bool PerformMove()
        {
            if (!_mapManager.Verify())
            {
                return false;
            }
            else if (_mowingSteps.Any())
            {
                PerformOngoingSteps();
                return false;
            } 
            else if (_pathToChargingStation.Any() || _finalRunToChargingStation)
            {
                MoveToChargingStation();
                return false;
            }
            else if (_pathFromChargingStationToRecentPosition.Any())
            {
                MoveBackToRecentPosition();
                return false;
            }

            var calculatedSteps = CalculateNextMove();
            
            if (!calculatedSteps.Any())
            {
                Complete();
                return true;
            }

            var stepsToChargingStation = CalculateStepsToGoal(calculatedSteps.Last().MoveDirection, new Offset(0, 0));

            var hasEnoughFuel = HasEnoughFuel(calculatedSteps, stepsToChargingStation);
            if (!hasEnoughFuel)
            {
                foreach (var mowingStep in stepsToChargingStation)
                    _pathToChargingStation.Enqueue(mowingStep);
                
                MoveToChargingStation();
                return false;
            }
            
            PerformStep(calculatedSteps);
            return false;
        }

        private void MoveBackToRecentPosition()
        {
            var nextStep = _pathFromChargingStationToRecentPosition.Dequeue();
            _currentFacingDirection = nextStep.MoveDirection;

            var newOffset = _currentField.Offset.Add(new Offset(nextStep.MoveDirection));
            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }

        private void PerformOngoingSteps()
        {
            _tempOffset ??= _currentField.Offset;

            var step = _mowingSteps.Dequeue();
            var (x, y) = step.MoveDirection.TranslateDirectionToOffset();
            _tempOffset = _tempOffset.Add(new Offset(x, y));

            _currentFieldType = _mapManager.MoveMowingMachine(step,
                _currentFieldType is FieldType.Grass ? FieldType.MowedLawn : _currentFieldType, _charge);

            if (!_mowingSteps.Any())
            {
                _currentField = GetField(_mapManager.GetFieldsOfView(), _tempOffset, _currentFieldType);
                _currentField.IsVisited = true;

                if (_discoveredFields.Any(f => f.Offset == _currentField.Offset))
                {
                    _discoveredFields.Move(_discoveredFields.First(f => f.Offset == _currentField.Offset),
                        _discoveredFields.Count);
                }
                else
                {
                    // Update neighbor fields
                    UpdateNeighbors();
                    _discoveredFields.Add(_currentField);

                    _tempOffset = null;
                }
            }
        }

        private void UpdateNeighbors()
        {
            _discoveredFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
        }

        private IEnumerable<MowingStep> BetterCalculateStepsToGoal(MoveDirection startingDirection, Func<Field, bool> predicate)
        {
            var path = BetterOldWay(_currentField.Offset, predicate);
            
            path.Add(_currentField.Offset);

            var fields = _discoveredFields.Concat(_discoveredFields.SelectMany(f => f.NeighborFields)).ToList();

            var steps = new List<MowingStep>();
            var currentDirection = startingDirection;
            for (int i = path.Count - 2; i >= 0; i--)
            {
                var direction = path[i].Subtract(path[i + 1]).TranslateOffsetToDirection();

                var step = CalculateStepExpense(direction,
                    currentDirection,
                    fields.First(f => f.Offset == path[i]).Type);

                currentDirection = direction;
                steps.Add(step);
            }

            return steps;
        }

        private IEnumerable<MowingStep> CalculateStepsToGoal(MoveDirection startingDirection, Offset finalOffset)
        {
            var path = CalculatePathToGoal(_discoveredFields, _currentField.Offset, finalOffset);
            
            path.Add(_currentField.Offset);

            var steps = new List<MowingStep>();
            var currentDirection = startingDirection;
            for (int i = path.Count - 2; i >= 0; i--)
            {
                var direction = path[i].Subtract(path[i + 1]).TranslateOffsetToDirection();

                var step = CalculateStepExpense(direction,
                    currentDirection,
                    _discoveredFields.First(f => f.Offset == path[i]).Type);

                currentDirection = direction;
                steps.Add(step);
            }

            return steps;
        }
        
        private static MowingStep CalculateStepExpense(MoveDirection direction, MoveDirection currentFacingDirection,
            FieldType currentFieldType)
        {
            var turns = new Queue<MoveDirection>();
            
            if (currentFacingDirection != direction)
                turns = CalculateTurn(currentFacingDirection, direction, turns);

            return new MowingStep(turns, direction, currentFieldType);
        }

        private Field Move(MowingStep step, Offset newOffset, FieldType updatePrevFieldType)
        {
            _charge -= step.TotalEnergyExpense;

            _currentFieldType = _mapManager.MoveMowingMachine(step, updatePrevFieldType, _charge);

            if (_discoveredFields.Any() && updatePrevFieldType == FieldType.ChargingStation)
                return GetField(_mapManager.GetFieldsOfView(), newOffset, FieldType.MowedLawn);
         
            return GetField(_mapManager.GetFieldsOfView(), newOffset, updatePrevFieldType);
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
        
        private void Complete()
        {
            Console.WriteLine("Mowing complete!");
        }

        private bool HasEnoughFuel(IEnumerable<MowingStep> stepsToNextField, IEnumerable<MowingStep> stepsToChangingStation)
        {
            var totalRequestEnergy = stepsToNextField
                                         .Select(s => s.TotalEnergyExpense).Sum() +
                                     stepsToChangingStation
                                         .Select(s => s.TotalEnergyExpense).Sum();

            return _charge >= totalRequestEnergy;
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

            if (_discoveredFields.Any(f => f.Offset == _currentField.Offset))
            {
                _discoveredFields.Move(_discoveredFields.First(f => f.Offset == _currentField.Offset), _discoveredFields.Count);
                return;
            }
            
            // Update neighbor fields
            UpdateNeighbors();
            _discoveredFields.Add(_currentField);
        }

        private List<MowingStep> CalculateNextMove()
        {
            var (successful, direction) = HasWalkableNeighborField(_currentField);
            
            if (!successful)
            {
                var steps = BetterCalculateStepsToGoal(_currentFacingDirection,
                    f => !f.IsVisited && f.Type is not FieldType.ChargingStation);
                return steps.ToList();
            }
            
            var nextStep = CalculateStepExpense(direction, _currentFacingDirection, _currentFieldType);

            return new List<MowingStep> { nextStep };
        }

        private static (bool, MoveDirection) HasWalkableNeighborField(Field field)
        {
            var fieldIndex = field.NeighborFields?.FindIndex(f => !f.IsVisited && f.CanBeWalkedOn() && f.Type is not FieldType.ChargingStation);
                
            if (fieldIndex is null or -1)
                return (false, MoveDirection.Bottom);

            return (true, fieldIndex switch
            {
                0 => MoveDirection.Top,
                1 => MoveDirection.Right,
                2 => MoveDirection.Bottom,
                3 => MoveDirection.Left,
                _ => throw new Exception(),
            });
        }
        
        private void MoveToChargingStation()
        {
            if (!_pathToChargingStation.Any())
            {
                _finalRunToChargingStation = false;
                _currentField.Offset.UpdateOffset(new Offset(0, 0));

                var offset = _discoveredFields.Last().Offset;
                
                var stepsToChargingStation = CalculateStepsToGoal(_currentFacingDirection, offset);

                foreach (var step in stepsToChargingStation)
                    _pathFromChargingStationToRecentPosition.Enqueue(step);

                _charge = _maxChange;
                return;
            }
            
            var nextStep = _pathToChargingStation.Dequeue();

            _finalRunToChargingStation = !_pathToChargingStation.Any();
            
            _currentFacingDirection = nextStep.MoveDirection;

            var newOffset = _currentField.Offset.Add(new Offset(nextStep.MoveDirection));
            
            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }
        
        private List<Offset> CalculatePathToGoal(IEnumerable<Field> fields, Offset start, Offset goal)
        {
            return OldWay(fields, start, goal);
        }

        // Breadth first search (bfs)
        private List<Offset> BetterOldWay(Offset start, Func<Field, bool> predicate)
        {
            var visitedCoordinates = new Dictionary<Offset, Offset>();
            var nextCoordinatesToVisit = new Queue<OffsetInfo>();
            
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(start, null));

            Offset result = null;
            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();

                if (BetterGetNeighborCells(visitedCoordinates, nextCoordinatesToVisit, cellInfo, predicate, out result))
                    break;
            }

            if (result == null)
                return new List<Offset>();
            
            var tracedPath = new List<Offset>();
        
            var currenCoordinate = result;
            while (visitedCoordinates.TryGetValue(currenCoordinate, out var coord))
            {
                if (coord == null)
                    break;
                
                tracedPath.Add(currenCoordinate);
                currenCoordinate = coord;
            }
            
            return tracedPath;
        }

        private bool BetterGetNeighborCells(IDictionary<Offset, Offset> visitedCoordinates,
            Queue<OffsetInfo> nextCoordinatesToVisit,
            OffsetInfo info,
            Func<Field, bool> predicate,
            out Offset offset)
        {
            var (isValid, field) = BetterIsValidField(info.CurrentOffset);
            offset = null;
            
            if (!isValid)
                return false;
            
            // If it already exists, dont add again
            if (visitedCoordinates.ContainsKey(info.CurrentOffset))
                return false;
        
            visitedCoordinates[info.CurrentOffset] = info.PrevOffset;

            // Use expression here
            if (predicate.Invoke(field))
            // if (!field.IsVisited && field.Type is not FieldType.ChargingStation)
            {
                offset = field.Offset;
                return true;
            }
                
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y + 1, info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X + 1, info.CurrentOffset.Y, info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y - 1, info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X - 1, info.CurrentOffset.Y, info.CurrentOffset));
            return false;
        }

        private (bool, Field) BetterIsValidField(Offset offset)
        {
            var field = _discoveredFields
                            .FirstOrDefault(f => f.Offset == offset)
                        ?? _discoveredFields.SelectMany(f => f.NeighborFields)
                            .FirstOrDefault(f => f.Offset == offset);
            
            var value = field?.Type ?? FieldType.Water;
                
            return ((int) value != -1 && value is not FieldType.Water, field);
        }
        
        // Breadth first search (bfs)
        private List<Offset> OldWay(IEnumerable<Field> fields, Offset start, Offset goal)
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
        
            var currentCoordinate = goal;
            while (visitedCoordinates.TryGetValue(currentCoordinate.ToString(), out var coord))
            {
                if (coord == null)
                    break;
                
                tracedPath.Add(currentCoordinate);
                currentCoordinate = coord;
            }
            
            return tracedPath;
        
            bool GetNeighborCells(OffsetInfo info)
            {
                if (!IsValidField(fields, info.CurrentOffset))
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
        }
        
        bool IsValidField(IEnumerable<Field> fields, Offset offset)
        {
            var value = fields
                .FirstOrDefault(f => f.Offset == offset)?.Type;
                
            value ??= FieldType.Water;
                
            return (int) value != -1 && (FieldType) value is not FieldType.Water;
        }
    }
}