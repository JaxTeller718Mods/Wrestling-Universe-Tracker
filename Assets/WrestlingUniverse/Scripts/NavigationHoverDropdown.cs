using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WrestlingUniverse.UI
{
    public sealed class NavigationHoverDropdown : MonoBehaviour
    {
        [SerializeField] private GameObject menu;
        private int hoverCount;

        public void Configure(GameObject dropdownMenu)
        {
            menu = dropdownMenu;
            menu.SetActive(false);
        }

        public void PointerEntered()
        {
            hoverCount++;
            if (menu != null) menu.SetActive(true);
        }

        public void PointerExited()
        {
            hoverCount = Mathf.Max(0, hoverCount - 1);
            StartCoroutine(HideAfterPointerEventsSettle());
        }

        private IEnumerator HideAfterPointerEventsSettle()
        {
            yield return null;
            if (hoverCount == 0 && menu != null) menu.SetActive(false);
        }
    }

    public sealed class NavigationHoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private NavigationHoverDropdown dropdown;

        public void Configure(NavigationHoverDropdown target)
        {
            dropdown = target;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (dropdown != null) dropdown.PointerEntered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (dropdown != null) dropdown.PointerExited();
        }
    }
}
