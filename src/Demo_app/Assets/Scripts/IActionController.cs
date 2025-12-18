using System;
using BaseClasses.Model;
using UnityEngine.UIElements;

public interface IActionController
{
    public void Initiate(VisualElement root, Plot plot);

    public Action GetUpdateAction();
}
