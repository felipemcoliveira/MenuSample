using BandoWare.GameplayTags;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnTagAddEvent : UnityEvent<bool> { }

public class MenuTagsListener : MonoBehaviour
{
   [SerializeField]
   private GameplayTagRequirements m_Requirements;

   [Space(10)]
   [SerializeField]
   private OnTagAddEvent m_OnStateChange = new();

   private bool m_PreviousMatch;
   private MenuTags m_MenuTags;

   private void Start()
   {
      m_MenuTags = GetComponentInParent<MenuTags>();
      m_MenuTags.Tags.OnAnyTagNewOrRemove += OnAnyTagNewOrRemove;

      m_PreviousMatch = MatchesRequirements();
      m_OnStateChange.Invoke(m_PreviousMatch);
   }

   public void OnAnyTagNewOrRemove(GameplayTag tag, int newCount)
   {
      bool match = MatchesRequirements();

      Debug.Log($"GameObjects: {gameObject}");
      Debug.Log($"HasAllExact: {m_Requirements.RequiredTags.HasAllExact(m_MenuTags.Tags)}");
      Debug.Log($"HasAnyExact: {m_Requirements.ForbiddenTags.HasAnyExact(m_MenuTags.Tags)}");
      if (match == m_PreviousMatch)
      {
         return;
      }

      m_OnStateChange.Invoke(match);
   }

   private bool MatchesRequirements()
   {
      var tags = m_MenuTags.Tags;
      if (m_Requirements.ForbiddenTags.TagCount > 0 && m_Requirements.ForbiddenTags.HasAnyExact(tags))
      {
         return false;
      }

      if (m_Requirements.RequiredTags.TagCount > 0 && tags.TagCount > 0 && !tags.HasAll(m_Requirements.RequiredTags))
      {
         return false;
      }

      return true;
   }
}