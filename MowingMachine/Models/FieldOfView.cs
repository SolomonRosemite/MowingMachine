namespace MowingMachine.Models
{
    public class FieldOfView
    {
        private readonly int[][] fov;

        public FieldOfView(int[][] fov)
        {
            this.fov = fov;
        }

        public int Center => fov[1][1];
        public int Top => fov[2][1];
        public int Left => fov[1][0];
        public int Right => fov[1][2];
        public int Bottom => fov[0][1];

        public FieldType CenterCasted => (FieldType) fov[1][1];
        public FieldType TopCasted => (FieldType) fov[2][1];
        public FieldType LeftCasted => (FieldType) fov[1][0];
        public FieldType RightCasted => (FieldType) fov[1][2];
        public FieldType BottomCasted => (FieldType) fov[0][1];
    }
}