using UnityEngine;
using UnityEngine.UIElements;

public partial class VisualNovelView : VisualElement
{
    public void Bind()
    {
        // Example of memory cleanup pattern
        // Button_Next.clicked += OnNextClicked;

        this.RegisterCallback<DetachFromPanelEvent>(e =>
        {
            // Button_Next.clicked -= OnNextClicked;
        });
    }

    private void OnNextClicked()
    {
        Debug.Log("Next Dialog");
    }
}
