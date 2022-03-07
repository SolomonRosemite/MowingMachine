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
        private readonly MainWindow _mainWindow;
        
        private readonly Queue<MowingStep> _mowingSteps = new();

        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Field> _discoveredFields = new();

        private readonly List<(MowingStep, List<Field>, Offset)> _moves = new();

        private readonly Queue<MowingStep> _pathFromChargingStationToRecentPosition = new();

        private readonly Queue<MowingStep> _pathTheChargingStation = new();

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

        public MyMowingMachine(MainWindow mainWindow, MapManager mapManager, double maxChange)
        {
            _mainWindow = mainWindow;
            
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
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetTop)) ?? new Field(fov.TopCasted, offsetTop),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetRight)) ?? new Field(fov.RightCasted, offsetRight),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetBottom)) ?? new Field(fov.BottomCasted, offsetBottom),
                _discoveredFields.SingleOrDefault(f => f.Offset.CompareTo(offsetLeft)) ?? new Field(fov.LeftCasted, offsetLeft),
            };
            
            return new Field(fieldType, offset, neighbors);
        }

        public bool PerformMove()
        {
            // Console.WriteLine("----------------------------");
            // Console.WriteLine(_currentField.Offset);
            // Console.WriteLine(_currentField.Type);
            // var y = _discoveredFields
            //     .SelectMany(x => x.NeighborFields)
            //     .FirstOrDefault(nf => nf.Offset.CompareTo(new Offset(-2, -3)));
            // if (y is not null)
            // {
            //     if (y.Type is not FieldType.Water)
            //     {
            //         Console.WriteLine("*************************");
            //         Console.WriteLine(y.Offset);
            //         Console.WriteLine(y.Type);
            //         Console.WriteLine(y.IsVisited);
            //     }
            //     else
            //     {
            //         Console.WriteLine("#################################");
            //         Console.WriteLine(y.Offset);
            //         Console.WriteLine(y.Type);
            //         Console.WriteLine(y.IsVisited);
            //     }
            // }
            // Console.WriteLine("----------------------------");
            
            if (_mowingSteps.Any(s => s.FieldType is FieldType.Water) || _discoveredFields.Any(f => f.Type is FieldType.Water))
            {
                
            }
            
            if (!_mapManager.Verify())
            {
                return false;
            }
            else if (_mowingSteps.Any())
            {
                PerformOngoingSteps();
                return false;
            } 
            else if (_pathTheChargingStation.Any() || _finalRunToChargingStation)
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

            if (calculatedSteps.Any(s => s.FieldType == FieldType.Water))
            {
                
            }
            
            if (!calculatedSteps.Any())
            {
                Complete();
                return true;
            }

            var stepsToChargingStation = CalculateStepsToGoal(calculatedSteps.Last().MoveDirection, new Offset(0, 0));

            var needsToRefuelFirst = NeedsToRefuel(calculatedSteps, stepsToChargingStation);
            if (needsToRefuelFirst)
            {
                foreach (var mowingStep in stepsToChargingStation)
                    _pathTheChargingStation.Enqueue(mowingStep);
                
                MoveToChargingStation();
                return false;
            }
            
            PerformStep(calculatedSteps);
            return false;
        }

        private void MoveBackToRecentPosition()
        {
            var nextStep = _pathFromChargingStationToRecentPosition.Dequeue();

            // if (!_pathFromChargingStationToRecentPosition.Any())
            //     return;
            
            _currentFacingDirection = nextStep.MoveDirection;

            var newOffset = _currentField.Offset.Add(new Offset(nextStep.MoveDirection));
            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }

        private void PerformOngoingSteps()
        {
            _tempOffset ??= _currentField.Offset;
            var currentOffset = _currentField.Offset;
            var currentOffset2 = _tempOffset;

            var step = _mowingSteps.Dequeue();
            var (x, y) = step.MoveDirection.TranslateDirectionToOffset();
            _tempOffset = _tempOffset.Add(new Offset(x, y));

            _currentFieldType = _mapManager.MoveMowingMachine(step,
                _currentFieldType is FieldType.Grass ? FieldType.MowedLawn : _currentFieldType, _charge);

            if (!_mowingSteps.Any())
            {
                _currentField = GetField(_mapManager.GetFieldsOfView(), _tempOffset, _currentFieldType);
                _currentField.IsVisited = true;

                if (_discoveredFields.Any(f => f.Offset.CompareTo(_currentField.Offset)))
                {
                    _discoveredFields.Move(_discoveredFields.First(f => f.Offset.CompareTo(_currentField.Offset)),
                        _discoveredFields.Count);
                }
                else
                {
                    // Update neighbor fields
                    _discoveredFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
                    _discoveredFields.Add(_currentField);

                    _tempOffset = null;
                }
            }
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
                    _discoveredFields.First(f => f.Offset.CompareTo(path[i])).Type);

                currentDirection = direction;
                steps.Add(step);
            }

            return steps;
        }
        
        private IEnumerable<MowingStep> CalculateStepsToGoal(MoveDirection startingDirection, List<Offset> path)
        {
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

            Console.WriteLine("--------------------");
            Console.WriteLine("Offset: " + newOffset);
            Console.WriteLine("Type: " + updatePrevFieldType);

            var x = _discoveredFields.Where(f => f.Offset.CompareTo(new Offset(-2, -3)));
            var y = _discoveredFields
                .Where(f => !f.Offset.CompareTo(new Offset(-1,-3)) && !f.Offset.CompareTo(new Offset(-2,-2)) && f.NeighborFields.Any(nf => nf.Offset.CompareTo(new Offset(-2, -3))));

            if (x.Any() || y.Any())
            {
                
            }
            
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
            
            _mainWindow.Restart();
        }

        private bool NeedsToRefuel(IEnumerable<MowingStep> stepsToNextField, IEnumerable<MowingStep> stepsToChangingStation)
        {
            var totalRequestEnergy = stepsToNextField
                                         .Select(s => s.TotalEnergyExpense).Sum() +
                                     stepsToChangingStation
                                         .Select(s => s.TotalEnergyExpense).Sum();

            bool hasEnoughFuel = _charge >= totalRequestEnergy;
            
            return !hasEnoughFuel;
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
                return;
            }
            
            // Update neighbor fields
            _discoveredFields.ForEach(f => f.UpdateFieldNeighbor(_currentField));
            _discoveredFields.Add(_currentField);
        }

        private List<MowingStep> CalculateNextMove()
        {
            var successful = HasWalkableNeighborField(_currentField, out var values);
            var (_, direction) = values;
            
            if (!successful)
            {
                var allUnvisitedFields = _discoveredFields
                    .Where(field => HasWalkableNeighborField(field, out values))
                    .Select(_ => values.Item1).ToList();
                
                // var allUnvisitedFields = _discoveredFields
                //     .Where(f => f.NeighborFields.Any(nf =>
                //         !nf.IsVisited && nf.CanBeWalkedOn() && nf.Type != FieldType.ChargingStation))
                //     .ToList();
                
                var availablePaths = allUnvisitedFields.Select(unvisitedField =>
                    CalculatePathToGoal(_discoveredFields.Concat(_discoveredFields.SelectMany(f => f.NeighborFields)), _currentField.Offset, unvisitedField.Offset))
                    .ToList();
                // var allUnvisitedFields = _discoveredFields
                //     .Where(f => f.NeighborFields.Any(nf =>
                //         !nf.IsVisited && nf.CanBeWalkedOn() && nf.Type != FieldType.ChargingStation))
                //     .ToList();
                //
                // var availablePaths = allUnvisitedFields.Select(unvisitedField =>
                //     CalculatePathToGoal(_currentField.Offset, unvisitedField.Offset))
                //     .ToList();

                // TODO: Continue here
                // TODO: Why is path directing us to offset: x=0 y=3 if there is water?
                // Also we should be going right then, not left.
                var path = availablePaths.OrderBy(p => p.Count).FirstOrDefault();

                if (path?.FirstOrDefault(o => o.CompareTo(new Offset(0, -3))) is not null)
                {
                    
                }
                
                // var path = availablePaths.OrderBy(p => p.Count).FirstOrDefault();

                
                
                return path is not null
                    ? CalculateStepsToGoal(_currentFacingDirection, path.Last()).ToList()
                    : new List<MowingStep>();
            }
            
            var nextStep = CalculateStepExpense(direction, _currentFacingDirection, _currentFieldType);

            return new List<MowingStep> { nextStep };

            bool HasWalkableNeighborField(Field field,out (Field, MoveDirection) result)
            {
                var fieldIndex = field.NeighborFields?.FindIndex(f => !f.IsVisited && f.CanBeWalkedOn() && f.Type is not FieldType.ChargingStation);
                result = (null, MoveDirection.Bottom);
                
                if (fieldIndex is null or -1)
                    return false;

                result = (field.NeighborFields[fieldIndex.Value], fieldIndex switch
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

                var offset = _discoveredFields.Last().Offset;
                
                var stepsToChargingStation = CalculateStepsToGoal(_currentFacingDirection, offset);

                foreach (var step in stepsToChargingStation)
                    _pathFromChargingStationToRecentPosition.Enqueue(step);

                _charge = _maxChange;
                return;
            }
            
            var nextStep = _pathTheChargingStation.Dequeue();

            _finalRunToChargingStation = !_pathTheChargingStation.Any();
            
            _currentFacingDirection = nextStep.MoveDirection;

            var newOffset = _currentField.Offset.Add(new Offset(nextStep.MoveDirection));
            // var newOffset = _currentField.Offset.Subtract(new Offset(nextStep.MoveDirection));
            
            _currentField = Move(nextStep, newOffset, _currentFieldType);
        }
        
        // Breadth first search (bfs)
        private List<Offset> CalculatePathToGoal(IEnumerable<Field> fields, Offset start, Offset goal)
        {
            var visitedCoordinates = new Dictionary<string, Offset>();
            var nextCoordinatesToVisit = new Queue<OffsetInfo>();
            
            nextCoordinatesToVisit.Enqueue(new OffsetInfo(start, null));
        
            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();

                // Console.WriteLine(nextCoordinatesToVisit.Count);
        
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
                var value = fields
                    .FirstOrDefault(f => f.Offset.CompareTo(new Offset(offset.X, offset.Y)))?.Type;

                if (value is null)
                {
                    
                }
                
                value ??= FieldType.Water;
                
                return (int) value != -1 && (FieldType) value is not FieldType.Water;
            }
        }
    }
}