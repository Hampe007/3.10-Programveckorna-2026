using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class uiInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Selectable elementToSelect;

    [Header("Visualization")]
    [SerializeField] private bool showVisualization;
    [SerializeField] private Color navigationColor = Color.green;

    private void OnDrawGizmos()
    {
        if (!showVisualization || elementToSelect == null)
            return;

        Gizmos.color = navigationColor;
        Gizmos.DrawLine(gameObject.transform.position, elementToSelect.gameObject.transform.position);
    }

    private void Reset()
    {
        eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
            Debug.Log("Did not find an Event System in your Scene", this);
    }

    public void JumpToElement()
    {
        if (eventSystem == null)
            eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.Log("No EventSystem available (create one via GameObject > UI > Event System).", this);
            return;
        }

        if (elementToSelect == null)
        {
            Debug.Log("This should jump where?", this);
            return;
        }

        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
    }
}
