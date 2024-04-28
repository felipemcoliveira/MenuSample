using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(10)]
public class RectTransformFramer : UIBehaviour
{
   private static readonly Vector3[] s_Corners = new Vector3[4];

   [SerializeField]
   private RectOffset m_Padding;

   [SerializeField]
   private RectTransform m_Container;

   protected override void OnRectTransformDimensionsChange()
   {
      Frame();
   }

   private void LateUpdate()
   {
      if (transform.hasChanged)
      {
         Frame();
         transform.hasChanged = false;
      }
   }

   private void Frame()
   {
      var container = GetContainerOrParent();
      if (container == null)
      {
         return;
      }

      container.GetWorldCorners(s_Corners);
      Vector3 containerMin = s_Corners[0], containerMax = s_Corners[2];

      var rectTrasnform = (RectTransform)transform;
      rectTrasnform.GetWorldCorners(s_Corners);
      Vector3 min = s_Corners[0], max = s_Corners[2];

      containerMin.x += m_Padding.left;
      containerMin.y += m_Padding.bottom;
      containerMax.x -= m_Padding.right;
      containerMax.y -= m_Padding.top;

      var position = rectTrasnform.position;

      Vector3 relativeMin = position - min, relativeMax = max - position;
      position.x = Mathf.Clamp(position.x, containerMin.x + relativeMin.x, containerMax.x - relativeMax.x);
      position.y = Mathf.Clamp(position.y, containerMin.y + relativeMin.y, containerMax.y - relativeMax.y);

      rectTrasnform.position = position;
   }

   private void OnDrawGizmos()
   {
      var container = GetContainerOrParent();
      if (container == null)
      {
         return;
      }

      container.GetWorldCorners(s_Corners);

      s_Corners[0].x += m_Padding.left;
      s_Corners[0].y += m_Padding.bottom;
      s_Corners[1].x += m_Padding.right;
      s_Corners[1].y -= m_Padding.bottom;
      s_Corners[2].x -= m_Padding.right;
      s_Corners[2].y -= m_Padding.top;
      s_Corners[3].x -= m_Padding.left;
      s_Corners[3].y += m_Padding.top;

      Handles.color = new Color(1, 0, 0.3f, 0.9f);

      for (int i = 0; i < 4; i++)
      {
         Handles.DrawDottedLine(s_Corners[i], s_Corners[(i + 1) % 4], 5f);
      }

      Handles.color = Color.white;
   }

   private RectTransform GetContainerOrParent()
   {
      if (m_Container != null)
      {
         return m_Container;
      }

      return transform.parent as RectTransform;
   }
}
