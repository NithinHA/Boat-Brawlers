using System;
using System.Collections.Generic;
using BaseObjects;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private List<AttachmentPivot> m_Pivots;
        [SerializeField] private Transform m_Rh, m_Lh;
        [SerializeField] private Rig m_PlayerRig;

        private Dictionary<PivotType, Transform> _pivotMap = new Dictionary<PivotType, Transform>();

        private void Start()
        {
            for (int i = 0; i < m_Pivots.Count; i++)
                _pivotMap.Add(m_Pivots[i].Type, m_Pivots[i].Pivot);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                PickItem();
            else if (Input.GetKeyDown(KeyCode.L))
                DropItem();
        }

        /// <summary>
        /// If no object is highlighted => return; If any object is held => Drop it;
        /// Set Pivot, RightHand, LeftHand positions.
        /// </summary>
        public void PickItem()
        {
            InteractableObject item = InteractablesController.Instance.HighlightedObject;
            if (item == null)
                return;
            
            if(InteractablesController.Instance.HeldObject != null)
                DropItem();

            Transform targetPivot = _pivotMap[item.PivotType];
            AlignPivotAndHandPositions(targetPivot, item);
            item.transform.SetParent(targetPivot);
            item.Pick();
            m_PlayerRig.weight = item.RigTargetWeight;     // try Tweening this value
        }

        public void DropItem()
        {
            InteractableObject item = InteractablesController.Instance.HeldObject;
            if (item == null)
                return;

            item.transform.SetParent(null);
            ResetPivotAndHandPositions(_pivotMap[item.PivotType]);
            m_PlayerRig.weight = 0;     // try Tweening this value
            item.Drop();
        }

        private void AlignPivotAndHandPositions(Transform pivot, InteractableObject item)
        {
            pivot.localPosition = item.RefPivot.localPosition;
            pivot.localRotation = item.RefPivot.localRotation;
            
            m_Rh.localPosition = item.Ref_Rh.localPosition;
            m_Rh.localRotation = item.Ref_Rh.localRotation;
            
            m_Lh.localPosition = item.Ref_Lh.localPosition;
            m_Lh.localRotation = item.Ref_Lh.localRotation;
        }

        private void ResetPivotAndHandPositions(Transform pivot)
        {
            pivot.localPosition = m_Rh.localPosition = m_Lh.localPosition = Vector3.zero;
            pivot.localRotation = m_Rh.localRotation = m_Lh.localRotation = Quaternion.identity;
        }


#region OnTrigger Highlight

        private void OnTriggerEnter(Collider other)
        {
            InteractableObject obj = other.GetComponent<InteractableObject>();
            if (obj == null)
                return;

            if (InteractablesController.Instance.HighlightedObject == null)
            {
                obj.SetHighlight();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            InteractableObject obj = other.GetComponent<InteractableObject>();
            if (obj == null)
                return;

            if (InteractablesController.Instance.HighlightedObject == obj)
            {
                obj.RemoveHighlight();
            }
        }

#endregion

    }

    public enum PivotType
    {
        Default, Shoulder
    }

    [System.Serializable]
    public class AttachmentPivot
    {
        public PivotType Type;
        public Transform Pivot;
    }
}
