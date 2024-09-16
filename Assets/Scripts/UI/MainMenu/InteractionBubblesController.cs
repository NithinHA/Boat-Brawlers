using System;
using UnityEngine;
using System.Collections.Generic;

namespace UI.Interaction
{
    public class InteractionBubblesController : MonoBehaviour
    {
        public InteractionBubble InteractionBubblePrefab;
        public int PoolSize = 5;

        private List<InteractionBubble> _bubblePool;
        private Camera _mainCamera;

        private Dictionary<Transform, InteractionBubble> _linkedBubbles = new Dictionary<Transform, InteractionBubble>();

        void Start()
        {
            _bubblePool = new List<InteractionBubble>();
            _mainCamera = Camera.main;

            for (int i = 0; i < PoolSize; i++)
            {
                InteractionBubble bubble = Instantiate(InteractionBubblePrefab, transform);
                bubble.gameObject.SetActive(false);
                _bubblePool.Add(bubble);
            }
        }

        public void ShowInteractionBubble(Transform t, Vector3 offset, Action onInteract = null)
        {
            InteractionBubble bubble = GetInactiveBubble();
            if (bubble != null)
            {
                bubble.SetupBubble(t, offset, _mainCamera, onInteract);
                _linkedBubbles.Add(t, bubble);
            }
        }

        private InteractionBubble GetInactiveBubble()
        {
            foreach (InteractionBubble bubble in _bubblePool)
            {
                if (!bubble.gameObject.activeInHierarchy)
                {
                    return bubble;
                }
            }

            // TODO: Expand the pool if no inactive bubbles are available
            return null;
        }

        // Function to hide an interaction bubble
        public void HideInteractionBubble(Transform t)
        {
            if (!_linkedBubbles.ContainsKey(t))
                return;

            InteractionBubble bubble = _linkedBubbles[t];
            if (bubble != null)
                bubble.ResetBubble();

            _linkedBubbles.Remove(t);
        }
    }
}