namespace MowingMachine.Models
{
    public class FieldOfView
    {
        private readonly int[][] _fov;

        public FieldOfView(int[][] fov)
        {
            _fov = fov;
        }

        public FieldType TopCasted => (FieldType) _fov[0][1];
        public FieldType LeftCasted => (FieldType) _fov[1][0];
        public FieldType RightCasted => (FieldType) _fov[1][2];
        public FieldType BottomCasted => (FieldType) _fov[2][1];
    }
}