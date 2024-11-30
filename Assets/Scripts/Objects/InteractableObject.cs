using System.Collections;
using BaseObjects.Player;
using UI;
using UnityEngine;

namespace BaseObjects
{
    public class InteractableObject : BaseObject
    {
        [Header("Rig alignment")]
        public PivotType PivotType;
        public Transform RefPivot;
        [Space]
        public float RigTargetWeight = 1;
        public Transform Ref_Rh;
        public Transform Ref_Lh;
        [Space]
        [SerializeField] private InventorySelectionBubble m_InventorySelectionBubblePrefab;
        [SerializeField] private RectTransform m_BubbleParent;
        [SerializeField] private Vector3 m_HighlightBubbleOffset = Vector3.up;
        [SerializeField] private Renderer m_Renderer;
        [Space]
        [SerializeField] private Rigidbody m_Rb;
        [SerializeField] private Collider[] m_Colliders;

        private InventorySelectionBubble _currentBubble = null;
        private Material _materialInstance;
        private const string _materialProperty = "_switch";

        protected override void Start()
        {
            base.Start();
            InteractablesController.Instance.AllInteractables.Add(this);

            if (m_Rb == null)
                m_Rb = GetComponent<Rigidbody>();

            _materialInstance = m_Renderer.material;
        }

        public void SetHighlight()
        {
            if (InteractablesController.Instance.HeldObject == this)
                return;

            // code to highlight the object
            _currentBubble = Instantiate(m_InventorySelectionBubblePrefab, m_BubbleParent);
            _currentBubble.SetTarget(this.transform, m_HighlightBubbleOffset);
            InteractablesController.Instance.HighlightedObject = this;

            _materialInstance.SetFloat(_materialProperty, 1.0f);
        }

        public void RemoveHighlight()
        {
            // code to remove highlighter
            if (_currentBubble != null)
            {
                Destroy(_currentBubble.gameObject);
                _currentBubble = null;
            }

            InteractablesController.Instance.HighlightedObject = null;

            _materialInstance.SetFloat(_materialProperty, 0.0f);
        }

        public void Pick()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            m_Rb.isKinematic = true;
            for (int i = 0; i < m_Colliders.Length; i++)
                m_Colliders[i].enabled = false;

            RemoveHighlight();
            InteractablesController.Instance.HeldObject = this;
        }

        public void Drop()
        {
            m_Rb.isKinematic = false;
            for (int i = 0; i < m_Colliders.Length; i++)
                m_Colliders[i].enabled = true;
            InteractablesController.Instance.HeldObject = null;
        }
    }
}
