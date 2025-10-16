using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindowScript : MonoBehaviour
{
    private VisualElement root;
    private Dictionary<Button, VisualElement> actions;
    private VisualElement currentAction;
    
    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        actions = new Dictionary<Button, VisualElement>
        {
            { root.Q<Button>("ViewActionButton"), root.Q<VisualElement>("ViewAction") }
        };

        foreach (var pair in actions)
        {
            pair.Key.clicked += () => OnActionButtonClicked(pair.Key);
        }
    }
    
    private void OnActionButtonClicked(Button button)
    {
        if (currentAction != null)
        {
            currentAction.style.display = DisplayStyle.None;
        }

        currentAction = actions[button];
        currentAction.style.display = DisplayStyle.Flex;
    }
}
