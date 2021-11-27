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
        private readonly List<Coordinate> _reachableGrassCoordinates = new();
        
        // Here we keep track on what coordinates the grass was already mowed at.
        private readonly List<Coordinate> _mowedCoordinates = new();

        // Contains information about the map
        private readonly MapManager _mapManager;

        // This field preserves the most recent field the mowing machine was on. Initially it will be the charging station.
        private FieldType _prevFieldType = FieldType.ChargingStation;
        
        private MoveDirection _currentFacingDirection = MoveDirection.Center;
        private MoveDirection _currentDirection = MoveDirection.Center;
        
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
            // Maybe as we go to the starting point (calibration point) we can already start mowing.
            // Just keep in mind to also add the field the to _mowedCoordinates list and remove it from the _reachableGrassCoordinates
            _isMowing = _prevFieldType is FieldType.Grass;
            
            // Todo: Move closer to the current target coordinate...
            // We probably need dijkstra's algorithm here.
        }
    }
}