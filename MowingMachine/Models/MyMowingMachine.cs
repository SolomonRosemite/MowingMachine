using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MyMowingMachine
    {
        private readonly Offset _offsetToChargingStation = new(0, 0);
        
        private readonly Queue<MowingStep> _mowingSteps = new();

        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Field> _discoveredFields = new();

        private readonly Queue<MowingStep> _pathFromChargingStationToRecentPosition = new();

        private readonly Queue<MowingStep> _pathToChargingStation = new();

        // TODO: (Note) _pathToChargingStation might not be needed anymore if this works
        private readonly Stack<MowingStep> _stepsRequiredToGoToChargingStation = new();

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
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetRight) ??
                new Field(fov.RightCasted, offsetRight),
                _discoveredFields.SingleOrDefault(f => f.Offset == offsetBottom) ??
                new Field(fov.BottomCasted, offsetBottom),
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

            var (hasEnoughFuel, stepsToChargingStation) = HasEnoughFuel(calculatedSteps);
            if (!hasEnoughFuel)
            {
                foreach (var mowingStep in stepsToChargingStation)
                    _pathToChargingStation.Enqueue(mowingStep.Copy(true));
                
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

        private IEnumerable<MowingStep> CalculateStepsToGoal(MoveDirection startingDirection,
            Func<Field, bool> predicate)
        {
            var path = BreadthFirstSearch(_currentField.Offset, predicate);

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

        private static Queue<MoveDirection> CalculateTurn(MoveDirection direction, MoveDirection finalDirection,
            Queue<MoveDirection> moves)
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

        private (bool, IEnumerable<MowingStep>) HasEnoughFuel(IEnumerable<MowingStep> stepsToNextField)
        {
            foreach (var step in stepsToNextField)
                _stepsRequiredToGoToChargingStation.Push(step.InvertMowingStep(true));
            
            // For some reason, the energy sometimes drops below zero. To account for this we create this buffer that will be included
            // in the calculation. This buffer represents the worse possible move in terms of energy consumption.
            var energyBuffer = Constants.TranslateMoveToExpense(FieldType.Sand) + 2 * Constants.TurnExpense;

            var totalRequiredEnergy = _stepsRequiredToGoToChargingStation
                .Select(s => s.TotalEnergyExpense).Sum();

            var hasEnoughFuel = _charge - energyBuffer >= totalRequiredEnergy;

            if (!hasEnoughFuel)
            {
                // When is not enough fuel based on our calculation, use bfs to check if there is a new shorter path.
                // If there is a new shorter path return true. Else return false and return back to charging station.

                var stepsToChargingStation =
                    CalculateStepsToGoal(_stepsRequiredToGoToChargingStation.Last().MoveDirection,
                        field => field.Offset == _offsetToChargingStation);
                
                totalRequiredEnergy = stepsToChargingStation
                                              .Select(s => s.TotalEnergyExpense).Sum()
                                          + stepsToNextField
                                              .Select(s => s.TotalEnergyExpense).Sum();

                if (_charge - energyBuffer >= totalRequiredEnergy)
                {
                    _stepsRequiredToGoToChargingStation.Clear();
                    
                    foreach (var step in stepsToChargingStation)
                        _stepsRequiredToGoToChargingStation.Push(step.InvertMowingStep(true));
                    
                    return (true, null);
                }
                
                return (false, stepsToChargingStation);
            }
            
            return (true, null);
        }

        private void PerformStep(IEnumerable<MowingStep> steps)
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
                _discoveredFields.Move(_discoveredFields.First(f => f.Offset == _currentField.Offset),
                    _discoveredFields.Count);
                return;
            }

            // Update neighbor fields
            UpdateNeighbors();
            _discoveredFields.Add(_currentField);
        }

        private IEnumerable<MowingStep> CalculateNextMove()
        {
            var (successful, direction) = HasWalkableNeighborField(_currentField);

            if (!successful)
            {
                var steps = CalculateStepsToGoal(_currentFacingDirection,
                    f => !f.IsVisited && f.Type is not FieldType.ChargingStation);
                return steps;
            }

            var nextStep = CalculateStepExpense(direction, _currentFacingDirection, _currentFieldType);

            return new List<MowingStep> {nextStep};
        }

        private static (bool, MoveDirection) HasWalkableNeighborField(Field field)
        {
            var fieldIndex = field.NeighborFields?.FindIndex(f =>
                !f.IsVisited && f.CanBeWalkedOn() && f.Type is not FieldType.ChargingStation);

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

                var stepsToChargingStation =
                    CalculateStepsToGoal(_currentFacingDirection, field => field.Offset == offset);

                foreach (var step in stepsToChargingStation)
                    _pathFromChargingStationToRecentPosition.Enqueue(step);

                _charge = _maxChange;
                _stepsRequiredToGoToChargingStation.Clear();
                return;
            }

            var nextStep = _pathToChargingStation.Dequeue();

            _finalRunToChargingStation = !_pathToChargingStation.Any();

            _currentFacingDirection = nextStep.MoveDirection;

            var newOffset = _currentField.Offset.Add(new Offset(nextStep.MoveDirection));

            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }

        // Breadth first search (bfs)
        private List<Offset> BreadthFirstSearch(Offset start, Func<Field, bool> predicate)
        {
            var lazyNeighborFields =
                new Lazy<List<Field>>(() => _discoveredFields.SelectMany(f => f.NeighborFields).ToList());
            
            var visitedCoordinates = new Dictionary<Offset, Offset>();
            var nextCoordinatesToVisit = new Queue<OffsetInfo>();

            nextCoordinatesToVisit.Enqueue(new OffsetInfo(start, null));

            (bool, Offset) result = default;
            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();

                result = GetNeighborCells(lazyNeighborFields, visitedCoordinates, nextCoordinatesToVisit, cellInfo, predicate);
                
                if (result.Item1)
                    break;
            }

            if (result.Equals(default) || !result.Item1)
                return new List<Offset>();

            var tracedPath = new List<Offset>();

            var currentCoordinate = result.Item2;
            while (visitedCoordinates.TryGetValue(currentCoordinate, out var coord))
            {
                if (coord == null)
                    break;

                tracedPath.Add(currentCoordinate);
                currentCoordinate = coord;
            }
            
            return tracedPath;
        }

        private (bool, Offset) GetNeighborCells(Lazy<List<Field>> neighborFields,
            IDictionary<Offset, Offset> visitedCoordinates,
            Queue<OffsetInfo> nextCoordinatesToVisit,
            OffsetInfo info,
            Func<Field, bool> predicate)
        {
            var (isValid, field) = IsValidField(neighborFields, info.CurrentOffset);

            // If it already exists, dont add again
            if (!isValid || visitedCoordinates.ContainsKey(info.CurrentOffset))
                return (false, null);

            visitedCoordinates[info.CurrentOffset] = info.PrevOffset;

            if (predicate.Invoke(field))
                return (true, field.Offset);

            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y + 1,
                info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X + 1, info.CurrentOffset.Y,
                info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X, info.CurrentOffset.Y - 1,
                info.CurrentOffset));
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(info.CurrentOffset.X - 1, info.CurrentOffset.Y,
                info.CurrentOffset));
            return (false, null);
        }

        private (bool, Field) IsValidField(Lazy<List<Field>> neighborFields, Offset offset)
        {
            Field field = null;
            for (var i = 0; i < _discoveredFields.Count; i++)
            {
                if (_discoveredFields[i].Offset == offset)
                {
                    field = _discoveredFields[i];
                }
            }

            if (field is null)
            {
                for (var i = 0; i < neighborFields.Value.Count; i++)
                {
                    if (neighborFields.Value[i].Offset == offset)
                    {
                        field = neighborFields.Value[i];
                    }
                }
            }

            var value = field?.Type ?? FieldType.Water;

            return ((int) value != -1 && value is not FieldType.Water, field);
        }
    }
}