using System;
using System.Collections;
using UnityEngine;

public class AnimateMenu : MonoBehaviour
{
   [SerializeField]
   private ExpandableVerticalLayoutGroup m_BodyLayoutGroup;

   private RectTransform m_RectTransform;
   private float m_OriginalWidth;

   private void Awake()
   {
      m_RectTransform = GetComponent<RectTransform>();

      m_OriginalWidth = m_RectTransform.sizeDelta.x;
      StartCoroutine(Animate());
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         StopAllCoroutines();
         StartCoroutine(Animate());
      }
   }

   private IEnumerator Animate()
   {
      const float headerAnimationDuration = 0.5f;
      const float bodyAnimationDuration = 0.5f;

      m_BodyLayoutGroup.NormalizedExpandedHeight = 0;

      var animateHeaderCoroutine = StartCoroutine(Animate(headerAnimationDuration, AnimateHeader));
      yield return new WaitForSeconds(headerAnimationDuration * 0.5f);

      yield return Animate(bodyAnimationDuration, AnimateBody);
      yield return animateHeaderCoroutine;
   }

   private void AnimateHeader(float t)
   {
      m_RectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, m_OriginalWidth, EaseOutQuart(t)), m_RectTransform.sizeDelta.y);
   }

   private void AnimateBody(float t)
   {
      m_BodyLayoutGroup.NormalizedExpandedHeight = EaseOutQuart(t);
   }

   private IEnumerator Animate(float duration, Action<float> func)
   {
      func(0);

      float headerAnimationStartTime = Time.time;

      while (Time.time - headerAnimationStartTime < duration)
      {
         float t = (Time.time - headerAnimationStartTime) / duration;
         func(t);
         yield return null;
      }

      func(1);
   }

   private float EaseOutQuart(float t)
   {
      return 1 - Mathf.Pow(1 - t, 4);
   }
}
