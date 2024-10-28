using System;
using UnityEngine.Timeline;

namespace SheetProcessor
{
    public interface ICellPosition : IEquatable<ICellPosition>
    {
        string HorizontalName { get; }
        string VerticalName { get; }

    }
}