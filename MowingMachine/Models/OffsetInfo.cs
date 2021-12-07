namespace MowingMachine.Models
{
    public class OffsetInfo
    {
        public OffsetInfo(int x, int y, Offset prevOffset)
        {
            CurrentOffset = new Offset(x, y);
            PrevOffset = prevOffset;
        }
        
        public OffsetInfo(Offset offset, Offset prevOffset)
        {
            CurrentOffset = offset;
            PrevOffset = prevOffset;
        }

        public Offset CurrentOffset { get; }
        public Offset PrevOffset { get; }
    }
}