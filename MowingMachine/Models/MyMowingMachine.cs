using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using MowingMachine.Services;

namespace MowingMachine.Models
{
    public class MyMowingMachine
    {
        // Idk but this is probably not necessary.
        private readonly ObservableCollection<Coordinate> _unreachableCoordinates = new();
        
        // These are going to be all the coordinates we go, to mow the grass at that coordinate.
        private readonly List<Coordinate> _reachableGrassCoordinates;
        
        // Here we keep track on what coordinates the grass was already mowed at.
        private readonly List<Coordinate> _mowedCoordinates = new();

        // Contains information about the map
        private readonly MapManager _mapManager;

        // This field preserves the most recent field the mowing machine was on. Initially it will be the charging station.
        private FieldType _prevFieldType = FieldType.ChargingStation;
        
        private MoveDirection _currentFacingDirection = MoveDirection.Center;
        private MoveDirection _currentDirection = MoveDirection.Center;

        private List<Coordinate> _pathToTargetCoordinate;

        private bool _isMowing;

        public MyMowingMachine(List<Coordinate> reachableGrassCoordinates, MapManager mapManager)
        {
            _mapManager = mapManager;
            _reachableGrassCoordinates = reachableGrassCoordinates;
        }

        public bool MakeMove()
        {
            if (_reachableGrassCoordinates.Count == 0)
            {
                Complete();
                return true;
            }
            
            MowGrass();
            return false;
        }
        
        private void Move(MoveDirection direction, FieldType? updatePrevFieldType = null)
        {
            if (_currentFacingDirection != direction)
                Turn(direction);
            
            NoteNextMove(direction);
            _prevFieldType = _mapManager.MoveMowingMachine(direction, updatePrevFieldType ?? _prevFieldType);
        }

        private void Turn(MoveDirection direction)
        {
            NoteNextMove(direction);
            
            // Todo: Turn here.
            // Call turn method again if needed
        }

        private void NoteNextMove(MoveDirection direction)
        {
            // Todo: Save the move in there giving data structure.
        }

        private void Complete()
        {
            Console.WriteLine("Mowing complete!");
            // Todo: Maybe double check if all the grass was mowed.
        }
        
        private void MowGrass()
        {
            _isMowing = false;

            var mowingMachinesCoordinate = _mapManager.GetMowingMachineCoordinate();

            if (_pathToTargetCoordinate == null)
            {
                var goalCoordinate = _reachableGrassCoordinates.First();
                
                _pathToTargetCoordinate = _mapManager.PathToGoalCoordinate(mowingMachinesCoordinate, goalCoordinate, true);
                _pathToTargetCoordinate.Reverse();
            }

            var directions = Enum.GetValues<MoveDirection>();
            for (int i = 0; i < directions.Length; i++)
            {
                var translatedCoordinate = mowingMachinesCoordinate.GetTranslatedCoordinate(directions[i]);
                Coordinate coordinate = _pathToTargetCoordinate.First();

                if (translatedCoordinate.ToString() == coordinate.ToString())
                {
                    _pathToTargetCoordinate.Remove(coordinate);
                    
                    if (_pathToTargetCoordinate.Count == 0)
                    {
                        _isMowing = true;
                        _pathToTargetCoordinate = null;
                        _mowedCoordinates.Add(coordinate);
                        _reachableGrassCoordinates.Remove(_reachableGrassCoordinates.First());
                    }
                    
                    Move(directions[i]);
                    _prevFieldType = _isMowing ? FieldType.MowedLawn : _prevFieldType;
                    return;
                }
            }

            throw new Exception("Could not find any matching translation.");
        }
    }
}