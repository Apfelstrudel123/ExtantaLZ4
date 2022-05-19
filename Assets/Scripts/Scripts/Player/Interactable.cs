using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interaction
{
    public enum InteractionType
    {
        Primary,
        Secondary,
        Parkour,
        Individual = 3
    }

    public interface IInteractable
    {
        public string Name();
        public bool Active();
        public InteractionType Type();
    }

    public class Interactable : MonoBehaviour, IInteractable
    {
        public bool active = true;
        public bool disableOnInteract;
        public UnityEvent onInteract;
        public InteractionType type = InteractionType.Primary;

        public string Name()
        {
            return name;
        }
        public bool Active()
        {
            return active;
        }
        public InteractionType Type()
        {
            return type;
        }
    }

    public class Control : IInteractable
    {
        public string name;
        public bool active = true;
        public bool disableOnInteract;
        public UnityEvent onInteract;
        public InteractionType type = InteractionType.Primary;

        public Control(string _name, bool _active, InteractionType _type)
        {
            name = _name;
            active = _active;
            type = _type;
        }

        public string Name()
        {
            return name;
        }
        public bool Active()
        {
            return active;
        }
        public InteractionType Type()
        {
            return type;
        }
    }
}