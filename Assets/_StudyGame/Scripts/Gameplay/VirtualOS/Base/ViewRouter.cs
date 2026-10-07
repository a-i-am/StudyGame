using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRouter : MonoBehaviour
{
    public static ViewRouter Instance { get; private set; }

    private Stack<BasePanel> panelStack = new Stack<BasePanel>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Push(BasePanel panel)
    {
        // 옵션: 기존에 열려있던 최상단 패널을 잠시 숨기고 싶다면 여기서 처리
        /*
        if (panelStack.Count > 0)
        {
            panelStack.Peek().Hide(); 
        }
        */

        panelStack.Push(panel);
        panel.Show();
    }

    // 최상단 패널을 닫고 스택에서 제거
    public void Pop()
    {
        if (panelStack.Count == 0) return;

        BasePanel topPanel = panelStack.Pop();
        topPanel.Hide();

        // 옵션: 아래에 깔려있던 패널을 다시 보여주고 싶다면 여기서 처리
        /*
        if (panelStack.Count > 0)
        {
            panelStack.Peek().Show();
        }
        */
    }

    // 스택에 있는 모든 패널을 한 번에 닫기 (메인 화면으로 돌아갈 때 유용)
    public void PopAll()
    {
        while (panelStack.Count > 0)
        {
            BasePanel panel = panelStack.Pop();
            panel.Hide();
        }
    }
}
