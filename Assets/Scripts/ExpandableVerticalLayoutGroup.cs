using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableVerticalLayoutGroup : VerticalLayoutGroup
{
   public float NormalizedExpandedHeight
   {
      get => m_NormalizedExpandedHeight;
      set
      {
         float clampedValue = Mathf.Clamp01(value);
         if (Mathf.Approximately(m_NormalizedExpandedHeight, clampedValue))
         {
            return;
         }

         m_NormalizedExpandedHeight = clampedValue;
         LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
      }
   }

   [Range(0, 1)]
   [SerializeField]
   private float m_NormalizedExpandedHeight = 1;

   [SerializeField]
   private float m_PreferredHeightDamper = 20;

   private float m_SmoothPreferredHeight;

   public override float preferredHeight
   {
      get
      {
         if (!Application.IsPlaying(this))
         {
            return base.preferredHeight * m_NormalizedExpandedHeight;
         }

         m_SmoothPreferredHeight = Mathf.MoveTowards(m_SmoothPreferredHeight, base.preferredHeight, Time.deltaTime * m_PreferredHeightDamper);
         if (Mathf.Approximately(m_SmoothPreferredHeight, base.preferredHeight))
         {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
         }

         return m_SmoothPreferredHeight * m_NormalizedExpandedHeight;
      }
   }
   public override float minHeight => 0;
}

[CustomEditor(typeof(ExpandableVerticalLayoutGroup))]
public class ExpandableVerticalLayoutGroupEditor : HorizontalOrVerticalLayoutGroupEditor
{
   private SerializedProperty m_NormalizedExpandedHeightProperty;
   private SerializedProperty m_PreferredHeightDamperProperty;

   protected override void OnEnable()
   {
      base.OnEnable();
      m_PreferredHeightDamperProperty = serializedObject.FindProperty("m_PreferredHeightDamper");
      m_NormalizedExpandedHeightProperty = serializedObject.FindProperty("m_NormalizedExpandedHeight");
   }

   public override void OnInspectorGUI()
   {
      serializedObject.Update();

      EditorGUILayout.PropertyField(m_NormalizedExpandedHeightProperty);
      EditorGUILayout.PropertyField(m_PreferredHeightDamperProperty);

      serializedObject.ApplyModifiedProperties();

      base.OnInspectorGUI();
   }
}
