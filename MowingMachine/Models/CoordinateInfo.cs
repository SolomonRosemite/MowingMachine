namespace MowingMachine.Models
{
    public class CoordinateInfo
    {
        public CoordinateInfo(int x, int y, Coordinate prevCoordinate)
        {
            CurrentCoordinate = new Coordinate(x, y);
            PrevCoordinate = prevCoordinate;
        }
        
        public CoordinateInfo(Coordinate coordinate, Coordinate prevCoordinate)
        {
            CurrentCoordinate = coordinate;
            PrevCoordinate = prevCoordinate;
        }

        public Coordinate CurrentCoordinate { get; }
        public Coordinate PrevCoordinate { get; }
    }
}