using System;
using SheetProcessor;
using UnityEngine;

namespace ZoneDetectionNamespace
{
    public class ZoneDetectionBehavior : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("New Area entered");
                Debug.Log(gameObject.name);
                SheetManager.Instance.CurrentLocation = gameObject.name;
            }
        }
    }
}
