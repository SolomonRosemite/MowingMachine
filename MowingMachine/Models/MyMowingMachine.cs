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
        private readonly ObservableCollection<Coordinate> unreachableCoordinates = new();
        private readonly List<Coordinate> visitedCoordinates = new();
        private readonly List<Coordinate> unvisitedCoordinates = new();

        private readonly List<MyMowingMachineMove> moves = new();
        private readonly MapManager mapManager;

        private FieldType prevFieldType = FieldType.ChargingStation;
        private Coordinate currentCoordinate;
        private MoveDirection currentDirection = MoveDirection.Top;
        private Goal currentGoal = Goal.Calibrate;

        private bool isMowing = false;
        
        // Defines the count of fields we are apart from the calculated track
        private int currentOffset = 0;

        public MyMowingMachine(int mapSize, MapManager mapManager)
        {
            this.mapManager = mapManager;
            
            var xs = Enumerable.Range(0, mapSize);
            var ys = Enumerable.Range(0, mapSize);

            foreach (var x in xs)
                foreach (var y in ys)
                    unvisitedCoordinates.Add(new Coordinate(x, y));
        }

        public void MakeMove()
        {
            switch (currentGoal)
            {
                case Goal.Calibrate:
                    Calibrate();
                    break;
                case Goal.CorrectPath:
                    CorrectPath();
                    break;
                case Goal.MowGrass:
                    MowGrass();
                    break;
                case Goal.Complete:
                    Complete();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Move(MoveDirection direction, FieldType? updatePrevFieldType = null)
        {
            NoteNextMove(direction);
            prevFieldType = mapManager.MoveMowingMachine(direction, updatePrevFieldType ?? prevFieldType);
        }
        
        private void CorrectPath()
        {
            // Todo: ...
        }

        private void NoteNextMove(MoveDirection direction)
        {
            // Todo: Save the move in there giving data structure.
        }

        private void MowGrass()
        {
            isMowing = prevFieldType is FieldType.Grass;
            
            if (IsValidMove(currentDirection, out var value))
                Move(currentDirection, isMowing ? FieldType.MowedLawn : null);
            else if (value == -1)
            {
                
                // Turn around
                // Also check if this is was the last column
            }
            else
            {
                // Go around obstacle
                // Note if we go off track (which we probaly have to do anyways) make sure to change the goal to currecting path
            }
        }

        private bool IsValidMove(MoveDirection direction, out int val)
        {
            var fis = mapManager.GetFieldsInSight();

            var value = fis.GetTranslatedField(1, 1, direction);

            val = value;
            
            return value != -1 && (FieldType) value != FieldType.Water;

        }
        private void Complete()
        {
            // Todo: Maybe double check if all the grass was mowed.
        }
        
        private void Calibrate()
        {
            var fis = mapManager.GetFieldsInSight();
            var bottomLeft = fis.GetTranslatedField(1, 1, MoveDirection.BottomLeft);
            
            if (bottomLeft != -1 && (FieldType)bottomLeft != FieldType.Water)
            {
                Move(MoveDirection.BottomLeft);
                return;
            }

            var leftCenter = fis.GetTranslatedField(1, 1, MoveDirection.LeftCenter);
            if (leftCenter != -1 && (FieldType)bottomLeft != FieldType.Water)
            {
                Move(MoveDirection.LeftCenter);
                return;
            }

            var bottom = fis.GetTranslatedField(1, 1, MoveDirection.Bottom);
            if (bottom != -1 && (FieldType)bottomLeft != FieldType.Water)
            {
                Move(MoveDirection.Bottom);
                return;
            }

            if (leftCenter == -1 && bottom == -1)
            {
                currentCoordinate = new Coordinate(0, 0);
                currentGoal = Goal.MowGrass;
                MakeMove();
                return;
            }

            // Todo: Implement case when system cant calibrate
            // The only way we get here is if the machine is stuck because of water.
            // In that case we can recursively try to get out
            
            // For example
            // [0, 0, 0]
            // [6, 5, 0]
            // [6, 6, 0]
            
            // In that case we need to try to get one up. Then we also can't go down again because that wouldn't make
            // any sense... 
            // Have fun!
        }
    }
}