using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
    public enum InteractionType
    {
        Primary,
        Secondary,
        Parkour,
    }
    public bool active = true;
    public bool disableOnInteract;
    public UnityEvent onInteract;
    public InteractionType interactionType = InteractionType.Primary;
}