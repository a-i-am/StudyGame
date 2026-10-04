using UnityEditor;
using Endava.Editor.UxmlBinding;

public static class BindingTrigger
{
    [MenuItem("Tools/Run Bindings Now")]
    public static void Run()
    {
        UxmlBindingGenerator.GenerateUxmlBinding("Assets/UI/UXML/AnimeLobbyView.uxml");
        UxmlBindingGenerator.GenerateUxmlBinding("Assets/UI/UXML/StreamExperienceView.uxml");
        UxmlBindingGenerator.GenerateUxmlBinding("Assets/UI/UXML/VisualNovelView.uxml");
        AssetDatabase.Refresh();
    }
}