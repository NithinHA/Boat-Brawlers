using System.Collections;
using System.Collections.Generic;
using BaseObjects;
using UnityEngine;

public class InteractablesController : Singleton<InteractablesController>
{
    public Transform Container;
    public List<InteractableObject> AllInteractables = new List<InteractableObject>();
    public InteractableObject HighlightedObject = null;
    public InteractableObject HeldObject = null;

}
