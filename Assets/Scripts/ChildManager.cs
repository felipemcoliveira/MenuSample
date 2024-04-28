using UnityEngine;
using UnityEngine.UI;

public class ChildManager : MonoBehaviour
{
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Q))
      {
         var lastChild = transform.GetChild(transform.childCount - 1);
         var newChild = Instantiate(lastChild, transform);
         newChild.gameObject.name = $"Child {transform.childCount}";

         var layoutElement = newChild.GetComponent<LayoutElement>();
         layoutElement.preferredHeight = Random.Range(50, 200);
         layoutElement.minHeight = Random.Range(50, 200);
      }

      if (Input.GetKeyDown(KeyCode.W))
      {
         if (transform.childCount > 1)
         {
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
         }
      }
   }
}
