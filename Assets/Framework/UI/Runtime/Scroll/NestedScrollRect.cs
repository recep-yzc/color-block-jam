using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI.Scroll
{
    public sealed class NestedScrollRect : ScrollRect
    {
        private bool isRoutingToParent;

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            base.OnInitializePotentialDrag(eventData);
            ExecuteOnParent(eventData, ExecuteEvents.initializePotentialDrag);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            isRoutingToParent = IsAcrossScrollAxis(eventData.position - eventData.pressPosition);

            if (isRoutingToParent)
            {
                ExecuteOnParent(eventData, ExecuteEvents.beginDragHandler);
            }
            else
            {
                base.OnBeginDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isRoutingToParent)
            {
                ExecuteOnParent(eventData, ExecuteEvents.dragHandler);
            }
            else
            {
                base.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (isRoutingToParent)
            {
                ExecuteOnParent(eventData, ExecuteEvents.endDragHandler);
            }
            else
            {
                base.OnEndDrag(eventData);
            }

            isRoutingToParent = false;
        }

        private bool IsAcrossScrollAxis(Vector2 dragDelta)
        {
            var isMostlyHorizontal = Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y);

            if (vertical && !horizontal)
            {
                return isMostlyHorizontal;
            }

            if (horizontal && !vertical)
            {
                return !isMostlyHorizontal;
            }

            return false;
        }

        private void ExecuteOnParent<THandler>(PointerEventData eventData, ExecuteEvents.EventFunction<THandler> handler)
            where THandler : IEventSystemHandler
        {
            if (transform.parent != null)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, handler);
            }
        }
    }
}
