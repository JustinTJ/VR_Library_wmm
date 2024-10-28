using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class TestOverlayClick : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update

public void OnPointerClick(PointerEventData eventData)
{
    GameObject clickedObject = eventData.pointerPress;
    string buttonName = clickedObject != null ? clickedObject.name : "Unknown";
    
    Debug.Log($"WOWOWOWOW OnPointerClick - Text Name: {buttonName}");

}
}
