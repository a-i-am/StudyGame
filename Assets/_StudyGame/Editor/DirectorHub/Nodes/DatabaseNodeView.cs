using UnityEditor;
using UnityEngine.UIElements;

namespace StudyGame.Editor.Director
{
    public class DatabaseNodeView : WindowNode
    {
        public DatabaseNodeView(string title = "Database Editor") : base(title)
        {
            // Left pane: Categories, Right pane: Details
            var splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);
            
            var leftPane = new VisualElement();
            var rightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rightPane.name = "RightPane";

            var btnItems = new Button(() => LoadCategory("Items")) { text = "Items/Clues" };
            var btnCharacters = new Button(() => LoadCategory("Characters")) { text = "Characters/NPCs" };
            var btnEnemies = new Button(() => LoadCategory("Enemies")) { text = "Enemies" };

            leftPane.Add(btnItems);
            leftPane.Add(btnCharacters);
            leftPane.Add(btnEnemies);

            var inspectorTitle = new Label("Select a category to edit data...");
            inspectorTitle.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            inspectorTitle.style.marginBottom = 10;
            rightPane.Add(inspectorTitle);

            splitView.Add(leftPane);
            splitView.Add(rightPane);
            
            ContentContainer.style.width = 500;
            ContentContainer.style.height = 350;

            ContentContainer.Add(splitView);
        }

        private void LoadCategory(string categoryName)
        {
            var rightPane = ContentContainer.Q<ScrollView>("RightPane");
            if (rightPane == null) return;

            rightPane.Clear();
            var title = new Label($"{categoryName} Data");
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.fontSize = 14;
            title.style.marginBottom = 10;
            rightPane.Add(title);

            if (categoryName == "Enemies")
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:EnemyProfileSO");
                if (guids.Length == 0)
                {
                    rightPane.Add(new Label("No EnemyProfileSO found."));
                }
                foreach (var guid in guids)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(path);
                    if (asset != null)
                    {
                        var btn = new Button(() => UnityEditor.Selection.activeObject = asset) 
                        { 
                            text = asset.name 
                        };
                        btn.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
                        rightPane.Add(btn);
                    }
                }
            }
            else
            {
                rightPane.Add(new Label($"[Placeholder] List of {categoryName} would appear here."));
            }
        }
    }
}
