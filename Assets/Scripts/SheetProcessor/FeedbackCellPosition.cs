using System;

namespace SheetProcessor
{
    public class FeedbackCellPosition : ICellPosition
    {
        public override bool Equals(object obj)
        {
            return obj is ICellPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HorizontalName, VerticalName);
        }

        public bool Equals(ICellPosition other)
        {
            return other != null && HorizontalName == other.HorizontalName && VerticalName == other.VerticalName;
        }

        public FeedbackCellPosition(string horizontalValue, string verticalValue)
        {
            HorizontalName = horizontalValue;
            VerticalName = verticalValue;
        }

        public string HorizontalName { get; }
        public string VerticalName { get; }
    }
}