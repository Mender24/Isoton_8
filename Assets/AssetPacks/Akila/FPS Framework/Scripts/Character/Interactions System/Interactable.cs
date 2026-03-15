using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Interactable")]
    public class Interactable : MonoBehaviour, IInteractable
    {
        public bool isOneUse = false;
        public bool instant = true;
        public string interactionName = "Interact";
        public UnityEvent OnInteract;

        public bool isInstant => instant;

        public string GetInteractionName()
        {
            return interactionName;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void OffInteraction()
        {
            instant = false;
        }

        public void Interact(InteractionsManager source)
        {
            if (isOneUse)
                instant = false;

            OnInteract?.Invoke();
        }
    }
}