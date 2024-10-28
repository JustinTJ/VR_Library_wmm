using System;
using SheetProcessor;
using UnityEngine;

namespace ZoneDetection
{
    public class ZoneDetection : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SheetManager.Instance.CurrentLocation = gameObject.name;
            }
        }
    }
}
