using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlideDirection
{
   Left,
   Right
}

[ExecuteAlways]
[DefaultExecutionOrder(-1000)]
public class SlidingContentContainer : UIBehaviour, ILayoutElement
{
   private class ActiveContent
   {
      public RectTransform Content { get; set; }
      public int NormalizedTranslation { get; set; }
   }

   float ILayoutElement.flexibleWidth => 1;
   int ILayoutElement.layoutPriority => 1;

   float ILayoutElement.preferredHeight
   {
      get
      {
         if (m_SelectedContent == null)
         {
            return -1;
         }

         int selectedContentIndex = m_Contents.IndexOf(m_SelectedContent);
         if (selectedContentIndex < 0)
         {
            return -1;
         }

         if (!Application.IsPlaying(this))
         {
            return LayoutUtility.GetPreferredHeight(m_SelectedContent);
         }

         return m_SmoothPreferredHeight;
      }
   }


   float ILayoutElement.minWidth => -1;
   float ILayoutElement.preferredWidth => -1;
   float ILayoutElement.minHeight => -1;
   float ILayoutElement.flexibleHeight => -1;

   public RectTransform SelectedContent => m_SelectedContent;

   public RectTransform RectTransform => transform as RectTransform;

   [SerializeField]
   private RectTransform m_SelectedContent;

   [SerializeField]
   private float m_Spacing;

   [SerializeField]
   private float m_SlideSpeed = 1500;

   [SerializeField]
   private float m_SmoothPreferredHeightSpeed = 15;

   private DrivenRectTransformTracker m_Tracker;
   private List<RectTransform> m_Contents = new();
   private LinkedList<ActiveContent> m_ActiveContents = new();
   private float m_SmoothPreferredHeight;
   private float m_NormalizedTranslation;
   private int m_TargetNormalizeTranslation;

   void ILayoutElement.CalculateLayoutInputHorizontal()
   {
      m_Tracker.Clear();
      m_Contents.Clear();

      for (int i = 0; i < transform.childCount; i++)
      {
         var child = transform.GetChild(i);
         if (child.TryGetComponent(out RectTransform content))
         {
            m_Contents.Add(content);

            m_Tracker.Add(this, content,
               DrivenTransformProperties.Anchors |
               DrivenTransformProperties.AnchoredPosition |
               DrivenTransformProperties.SizeDelta |
               DrivenTransformProperties.Pivot);

            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
            content.pivot = Vector2.up;
         }
      }
   }

   void ILayoutElement.CalculateLayoutInputVertical()
   { }

   public void Goto(RectTransform content)
   {
      int siblingIndex = content.GetSiblingIndex();
      int selectedSiblingIndex = m_SelectedContent.GetSiblingIndex();

      if (siblingIndex == selectedSiblingIndex)
      {
         return;
      }

      Slide(content, siblingIndex > selectedSiblingIndex ? SlideDirection.Right : SlideDirection.Left);
   }

   public void Slide(RectTransform content, SlideDirection slideDirection)
   {
      if (!m_Contents.Contains(content))
      {
         throw new ArgumentException("Content not found in the list of contents", nameof(content));
      }

      foreach (var activeContent in m_ActiveContents)
      {
         if (activeContent.Content == content)
         {
            return;
         }
      }

      content.gameObject.SetActive(true);
      m_SelectedContent = content;
      switch (slideDirection)
      {
         case SlideDirection.Left:
         {
            var firstNode = m_ActiveContents.First;
            var node = m_ActiveContents.AddFirst(new ActiveContent
            {
               Content = content,
               NormalizedTranslation = firstNode.Value.NormalizedTranslation - 1
            });

            m_TargetNormalizeTranslation = node.Value.NormalizedTranslation;

            break;
         }

         case SlideDirection.Right:
         {
            var lastNode = m_ActiveContents.Last;
            var node = m_ActiveContents.AddLast(new ActiveContent
            {
               Content = content,
               NormalizedTranslation = lastNode.Value.NormalizedTranslation + 1
            });

            m_TargetNormalizeTranslation = node.Value.NormalizedTranslation;

            break;
         }

         default:
         {
            throw new NotImplementedException();
         }
      }

      SetDirty();
   }

   protected override void OnEnable()
   {
      if (!Application.IsPlaying(this))
      {
         return;
      }

      for (int i = 0; i < transform.childCount; i++)
      {
         var child = transform.GetChild(i) as RectTransform;
         if (m_SelectedContent != null && child == m_SelectedContent)
         {
            continue;
         }

         child.gameObject.SetActive(false);
      }

      if (m_SelectedContent == null)
      {
         return;
      }

      m_SelectedContent.gameObject.SetActive(true);
      m_ActiveContents.AddLast(new ActiveContent
      {
         Content = m_SelectedContent,
         NormalizedTranslation = 0
      });

      m_NormalizedTranslation = 0;
      m_TargetNormalizeTranslation = 0;
   }

   private void Update()
   {
      if (m_SelectedContent == null)
      {
         return;
      }

      int selectedIndex = m_Contents.IndexOf(m_SelectedContent);
      if (selectedIndex < 0)
      {
         return;
      }

      float selectedContentPreferredHeight = LayoutUtility.GetPreferredHeight(m_SelectedContent);
      var rect = RectTransform.rect;

      if (!Application.IsPlaying(this))
      {
         UpdateImmediate(selectedIndex, selectedContentPreferredHeight, rect);
         return;
      }

      foreach (var activeContent in m_ActiveContents)
      {
         var content = activeContent.Content;
         float x = (activeContent.NormalizedTranslation - m_NormalizedTranslation) * (rect.width + m_Spacing);
         content.anchoredPosition = new Vector2(x, 0);
         content.sizeDelta = new Vector2(rect.width, LayoutUtility.GetPreferredHeight(content));
      }

      m_SmoothPreferredHeight = Mathf.MoveTowards(m_SmoothPreferredHeight,
         selectedContentPreferredHeight, Time.deltaTime * m_SmoothPreferredHeightSpeed);

      if (!Mathf.Approximately(m_SmoothPreferredHeight, selectedContentPreferredHeight))
      {
         LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
      }

      if (m_ActiveContents.Count == 1)
      {
         return;
      }

      m_NormalizedTranslation = Mathf.MoveTowards(m_NormalizedTranslation, m_TargetNormalizeTranslation, Time.deltaTime * m_SlideSpeed);
      if (Mathf.Approximately(m_NormalizedTranslation, m_TargetNormalizeTranslation))
      {
         m_NormalizedTranslation = m_TargetNormalizeTranslation;
         var node = m_ActiveContents.First;

         while (node != null)
         {
            if (node.Value.Content == m_SelectedContent)
            {
               node = node.Next;
               continue;
            }

            var nextNode = node.Next;
            node.Value.Content.gameObject.SetActive(false);
            m_ActiveContents.Remove(node);
            node = nextNode;
         }
      }
   }

   internal void SetDirty()
   {
      if (!IsActive())
      {
         return;
      }

      LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
   }

   private void UpdateImmediate(int selectedIndex, float selectedContentPreferredHeight, Rect rect)
   {
      m_SelectedContent.anchoredPosition = new Vector2(0, 0);
      m_SelectedContent.sizeDelta = new Vector2(rect.width, selectedContentPreferredHeight);

      int pos = 1;
      for (int i = 0; i < m_Contents.Count; i++)
      {
         if (i == selectedIndex)
         {
            continue;
         }

         var content = m_Contents[i];
         float x = pos * (rect.width + m_Spacing);
         pos++;

         content.anchoredPosition = new Vector2(x, 0);
         content.sizeDelta = new Vector2(rect.width, LayoutUtility.GetPreferredHeight(content));
      }
   }
}


[CustomEditor(typeof(SlidingContentContainer))]
public class SlidingContentEditor : Editor
{
   public override void OnInspectorGUI()
   {
      EditorGUI.BeginChangeCheck();
      base.OnInspectorGUI();

      if (EditorGUI.EndChangeCheck())
      {
         var slidingContent = (SlidingContentContainer)target;
         slidingContent.SetDirty();
      }
   }
}