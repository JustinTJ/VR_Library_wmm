using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Feedback
{
    [CreateAssetMenu(fileName = "Feedback Sheet Setting", menuName = "Feedback Sheet Setting")]
    public class FeedbackSheetSetting : ScriptableObject
    {
        public List<string> verticalNames = new List<string>();
        public List<string> horizontalNames = new List<string>();
    }
}