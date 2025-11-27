using System;
using BaseClasses.Model;
using UnityEngine.UIElements;

public interface IAction
{
    public void Initiate(VisualElement root, Plot plot);

    public Action GetUpdateAction();
}
