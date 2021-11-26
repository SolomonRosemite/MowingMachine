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
        private readonly ObservableCollection<KeyValuePair<int, int>> unreachableCoordinates = new();
        private readonly List<KeyValuePair<int, int>> visitedCoordinates = new();
        private readonly List<KeyValuePair<int, int>> unvisitedCoordinates = new();

        private readonly List<MyMowingMachineMove> moves = new();
        private readonly MapManager mapManager;

        private KeyValuePair<int, int> currentCoordinate;
        private Goal currentGoal = Goal.Calibrate;
        
        
        // Defines the count of fields we are apart from the calculated track
        private int currentOffset = 0;

        public MyMowingMachine(int mapSize, MapManager mapManager)
        {
            this.mapManager = mapManager;
            
            var xs = Enumerable.Range(0, mapSize);
            var ys = Enumerable.Range(0, mapSize);

            foreach (var x in xs)
                foreach (var y in ys)
                    unvisitedCoordinates.Add(new KeyValuePair<int, int>(x, y));
        }

        public void MakeMove()
        {
            switch (currentGoal)
            {
                case Goal.Calibrate:
                    Calibrate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Move(MoveDirection direction)
        {
            mapManager.MoveMowingMachine(direction, FieldType.Water);
        }

        private void Calibrate()
        {
            var fis = mapManager.GetFieldsInSight();
            var bottomLeft = fis.GetTranslatedField(1, 1, MoveDirection.BottomLeft);

            Console.WriteLine(bottomLeft);
            
            if (bottomLeft != -1)
            {
                Move(MoveDirection.BottomLeft);
            }
        }
    }
}