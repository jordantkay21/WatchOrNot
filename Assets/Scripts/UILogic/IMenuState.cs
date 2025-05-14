using UnityEngine;

public interface IMenuState
{
    void Enter();
    void Exit();
    void UpdateHeader();
    void UpdateFooter();
}
