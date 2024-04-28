using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : UIBehaviour, IDragHandler, IBeginDragHandler
{
   private RectTransform m_RectTransform;
   private Vector2 m_MouseInitialPosition;
   private Vector2 m_ObjectIntialPosition;

   protected override void Awake()
   {
      base.Awake();
      m_RectTransform = GetComponent<RectTransform>();
   }

   public void OnBeginDrag(PointerEventData eventData)
   {
      m_MouseInitialPosition = eventData.position;
      m_ObjectIntialPosition = m_RectTransform.anchoredPosition;
   }

   public void OnDrag(PointerEventData eventData)
   {
      m_RectTransform.anchoredPosition = m_ObjectIntialPosition + (eventData.position - m_MouseInitialPosition);
   }
}
