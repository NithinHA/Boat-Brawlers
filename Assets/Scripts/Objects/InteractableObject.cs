using BaseObjects.Player;
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
        [SerializeField] private Rigidbody m_Rb;
        [SerializeField] private Collider[] m_Colliders;

        private GameObject _highlightIndicator = null;

        protected override void Start()
        {
            base.Start();
            InteractablesController.Instance.AllInteractables.Add(this);

            if (m_Rb == null)
                m_Rb = GetComponent<Rigidbody>();
        }

        public void SetHighlight()
        {
            if (InteractablesController.Instance.HeldObject == this)
                return;

            // code to highlight the object
            _highlightIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _highlightIndicator.transform.SetParent(transform);
            _highlightIndicator.transform.localPosition = Vector3.zero;
            _highlightIndicator.transform.rotation = Quaternion.identity;
            _highlightIndicator.GetComponent<Collider>().enabled = false;

            InteractablesController.Instance.HighlightedObject = this;
        }

        public void RemoveHighlight()
        {
            // code to remove highlighter
            if (_highlightIndicator != null)
            {
                Destroy(_highlightIndicator.gameObject);
                _highlightIndicator = null;
            }

            InteractablesController.Instance.HighlightedObject = null;
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
