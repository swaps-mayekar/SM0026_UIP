using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIP.UI
{
    /// <summary>
    /// Horizontal chip rows live inside a vertical page scroller. Unused drag
    /// axes are forwarded so a sideways swipe scrolls chips and a vertical swipe
    /// still moves the page.
    /// </summary>
    public sealed class NestedScrollRect : ScrollRect
    {
        bool _routeToParent;

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Forward<IInitializePotentialDragHandler>(handler => handler.OnInitializePotentialDrag(eventData));
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _routeToParent = ShouldRouteToParent(eventData.delta);
            if (_routeToParent)
            {
                Forward<IBeginDragHandler>(handler => handler.OnBeginDrag(eventData));
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                Forward<IDragHandler>(handler => handler.OnDrag(eventData));
                return;
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                Forward<IEndDragHandler>(handler => handler.OnEndDrag(eventData));
            }
            else
            {
                base.OnEndDrag(eventData);
            }

            _routeToParent = false;
        }

        public override void OnScroll(PointerEventData eventData)
        {
            var horizontalWheel = Mathf.Abs(eventData.scrollDelta.x) > Mathf.Abs(eventData.scrollDelta.y);
            if ((horizontalWheel && !horizontal) || (!horizontalWheel && !vertical))
            {
                Forward<IScrollHandler>(handler => handler.OnScroll(eventData));
                return;
            }

            base.OnScroll(eventData);
        }

        bool ShouldRouteToParent(Vector2 delta)
        {
            if (!horizontal && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return true;
            }

            return !vertical && Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
        }

        void Forward<T>(System.Action<T> action) where T : IEventSystemHandler
        {
            var current = transform.parent;
            while (current != null)
            {
                var handlers = current.GetComponents<Component>();
                for (var i = 0; i < handlers.Length; i++)
                {
                    if (handlers[i] is T handler)
                    {
                        action(handler);
                        return;
                    }
                }

                current = current.parent;
            }
        }
    }
}
