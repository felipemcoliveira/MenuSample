using BandoWare.GameplayTags;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class MenuTags : MonoBehaviour
{
   public GameplayTagCountContainer Tags => m_Tags;

   public GameplayTagContainer PersistentTags => m_PersistentTags;

   [SerializeField]
   private GameplayTagContainer m_PersistentTags;

   private GameplayTagCountContainer m_Tags;

   private void Awake()
   {
      m_Tags = new GameplayTagCountContainer();
      m_Tags.AddTags(m_PersistentTags);
   }
}

[CustomEditor(typeof(MenuTags))]
public class MenuTagsEditor : Editor
{
   private class Style
   {
      public readonly GUIStyle TagLabel;

      public Style()
      {
         TagLabel = new GUIStyle();
         TagLabel.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
      }
   }

   protected MenuTags Target => (MenuTags)target;

   [SerializeField]
   private GameplayTagContainer m_Tags;

   private static Style s_Style;


   public override void OnInspectorGUI()
   {
      s_Style ??= new();

      base.OnInspectorGUI();

      if (!Application.IsPlaying(target))
      {
         return;
      }


      GUILayout.BeginHorizontal();

      EditorGUILayout.GetControlRect(false, 0, GUILayout.Width(EditorGUIUtility.labelWidth));

      if (GUILayout.Button("Reapply Persistent Tags"))
      {
         Target.Tags.Clear();
         Target.Tags.AddTags(Target.PersistentTags);
      }

      GUILayout.EndHorizontal();
   }
}