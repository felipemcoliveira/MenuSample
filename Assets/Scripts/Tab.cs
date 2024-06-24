using UnityEngine;
using UnityEngine.EventSystems;

public class Tab : UIBehaviour, IPointerClickHandler
{
   [SerializeField]
   private RectTransform m_Content;

   [SerializeField]
   private SlidingContentContainer m_SlidingContentContainer;

   public void OnPointerClick(PointerEventData eventData)
   {
      if (m_SlidingContentContainer == null || m_Content == null)
      {
         return;
      }

      m_SlidingContentContainer.Goto(m_Content);
   }
}