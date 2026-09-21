using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using StudyGame.Data;
using System.Linq;

namespace StudyGame.Editor.Director
{
    public class InspectorView : VisualElement
    {
        private VisualElement _contentContainer;
        private EpisodeNode _activeNode;

        public InspectorView()
        {
            style.width = 420;
            style.flexGrow = 1;
            AddToClassList("workspace-panel");

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.justifyContent = Justify.SpaceBetween;
            
            var title = new Label("동적 작업대 (Inspector)");
            title.AddToClassList("workspace-title");
            titleRow.Add(title);

            var saveTplBtn = new Button(SaveAsTemplate) { text = "💾 템플릿으로 저장" };
            titleRow.Add(saveTplBtn);
            Add(titleRow);

            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("inspector-content");
            
            var scroll = new ScrollView();
            scroll.Add(_contentContainer);
            Add(scroll);
        }

        public void BindNode(EpisodeNode node)
        {
            _activeNode = node;
            RenderDynamicProperties();
        }

        private void RenderDynamicProperties()
        {
            _contentContainer.Clear();
            if (_activeNode == null || _activeNode.NodeData == null)
            {
                var l = new Label("노드를 선택해주세요.");
                l.style.color = Color.gray;
                l.style.marginTop = 20;
                l.style.unityTextAlign = TextAnchor.MiddleCenter;
                _contentContainer.Add(l);
                return;
            }

            var data = _activeNode.NodeData;

            // Title Editor
            var header = new Label($"[{data.TemplateType}] 노드 설정");
            header.AddToClassList("workspace-section-header");
            _contentContainer.Add(header);

            var titleField = new TextField("노드 제목") { value = data.NodeTitle };
            titleField.RegisterValueChangedCallback(evt => {
                data.NodeTitle = evt.newValue;
                _activeNode.title = data.NodeTitle;
                EditorUtility.SetDirty(data);
            });
            _contentContainer.Add(titleField);

            // Job-Specific Smart Viewers
            if (data.TemplateType.Contains("캐릭터 DNA"))
            {
                RenderDNAPreview();
            }
            else if (data.TemplateType.Contains("보스 퍼즐"))
            {
                RenderPuzzleSimulator();
            }

            // Dynamic Properties
            var propHeader = new Label("▼ 동적 속성 (Dynamic Properties)");
            propHeader.AddToClassList("workspace-section-header");
            propHeader.style.marginTop = 15;
            _contentContainer.Add(propHeader);

            foreach (var prop in data.Properties)
            {
                RenderSingleProperty(prop, data);
            }

            // Add Property Button (Dropdown Simulation)
            var addBtnContainer = new VisualElement();
            addBtnContainer.style.flexDirection = FlexDirection.Row;
            addBtnContainer.style.marginTop = 20;
            
            var addBtn = new Button(() => ShowAddPropertyMenu(data)) { text = "➕ 속성 추가 (Add Property)" };
            addBtn.style.flexGrow = 1;
            addBtn.style.height = 30;
            addBtnContainer.Add(addBtn);
            _contentContainer.Add(addBtnContainer);
        }

        private void RenderSingleProperty(DynamicProperty prop, WorkspaceNodeData data)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 5;
            row.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.25f));
            row.style.paddingTop = row.style.paddingBottom = row.style.paddingLeft = row.style.paddingRight = 5;
            row.style.borderTopLeftRadius = 5;
            row.style.borderTopRightRadius = 5;
            row.style.borderBottomLeftRadius = 5;
            row.style.borderBottomRightRadius = 5;

            // Delete Btn
            var delBtn = new Button(() => {
                data.Properties.Remove(prop);
                EditorUtility.SetDirty(data);
                RenderDynamicProperties();
                UpdateNodeBadges();
            }) { text = "X" };
            delBtn.style.width = 20;
            row.Add(delBtn);

            // Badge Toggle
            var badgeToggle = new Toggle("★") { value = prop.ShowAsBadge, tooltip = "노드 캔버스에 뱃지로 표출" };
            badgeToggle.RegisterValueChangedCallback(evt => {
                prop.ShowAsBadge = evt.newValue;
                EditorUtility.SetDirty(data);
                UpdateNodeBadges();
            });
            row.Add(badgeToggle);

            var nameField = new TextField { value = prop.PropertyName };
            nameField.style.width = 120;
            nameField.RegisterValueChangedCallback(e => { 
                prop.PropertyName = e.newValue; 
                EditorUtility.SetDirty(data); 
                UpdateNodeBadges(); 
            });
            row.Add(nameField);

            var fieldContainer = new VisualElement();
            fieldContainer.style.flexGrow = 1;
            fieldContainer.style.marginLeft = 5;

            switch (prop.Type)
            {
                case PropertyType.Text:
                    var txt = new TextField() { value = prop.StringValue, multiline = true };
                    txt.RegisterValueChangedCallback(e => { prop.StringValue = e.newValue; EditorUtility.SetDirty(data); UpdateNodeBadges(); });
                    fieldContainer.Add(txt);
                    break;
                case PropertyType.Number:
                    var num = new FloatField() { value = prop.FloatValue };
                    num.RegisterValueChangedCallback(e => { prop.FloatValue = e.newValue; EditorUtility.SetDirty(data); UpdateNodeBadges(); });
                    fieldContainer.Add(num);
                    break;
                case PropertyType.Color:
                    var col = new ColorField() { value = prop.ColorValue };
                    col.RegisterValueChangedCallback(e => { prop.ColorValue = e.newValue; EditorUtility.SetDirty(data); UpdateNodeBadges(); });
                    fieldContainer.Add(col);
                    break;
                case PropertyType.Asset:
                    var obj = new ObjectField() { value = prop.AssetValue, objectType = typeof(UnityEngine.Object) };
                    obj.RegisterValueChangedCallback(e => { prop.AssetValue = e.newValue; EditorUtility.SetDirty(data); UpdateNodeBadges(); });
                    fieldContainer.Add(obj);
                    break;
                case PropertyType.Dropdown:
                    var drop = new TextField() { value = prop.StringValue };
                    drop.RegisterValueChangedCallback(e => { prop.StringValue = e.newValue; EditorUtility.SetDirty(data); UpdateNodeBadges(); });
                    fieldContainer.Add(drop);
                    break;
                case PropertyType.Table:
                    var tableBox = new VisualElement();
                    tableBox.style.flexDirection = FlexDirection.Column;
                    tableBox.style.marginTop = 10;
                    
                    var headerRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };
                    foreach(var colName in prop.TableColumns)
                    {
                        var colLabel = new Label(colName) { style = { flexGrow = 1, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter } };
                        headerRow.Add(colLabel);
                    }
                    tableBox.Add(headerRow);

                    foreach(var rowData in prop.TableRows)
                    {
                        var dataRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
                        for(int i=0; i<prop.TableColumns.Count; i++)
                        {
                            if(i >= rowData.Cells.Count) rowData.Cells.Add("");
                            int colIdx = i;
                            var cellField = new TextField() { value = rowData.Cells[colIdx] };
                            cellField.style.flexGrow = 1;
                            cellField.RegisterValueChangedCallback(e => { rowData.Cells[colIdx] = e.newValue; EditorUtility.SetDirty(data); });
                            dataRow.Add(cellField);
                        }
                        var delRowBtn = new Button(() => { prop.TableRows.Remove(rowData); EditorUtility.SetDirty(data); RenderDynamicProperties(); }) { text = "X" };
                        delRowBtn.style.width = 20;
                        dataRow.Add(delRowBtn);
                        tableBox.Add(dataRow);
                    }

                    var btnRow = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginTop = 5 } };
                    var addRowBtn = new Button(() => { prop.TableRows.Add(new TableRowData()); EditorUtility.SetDirty(data); RenderDynamicProperties(); }) { text = "+ 행 추가 (Row)" };
                    addRowBtn.style.flexGrow = 1;
                    btnRow.Add(addRowBtn);

                    var previewBtn = new Button(() => { 
                        DirectorStateManager.SetActiveContext(_activeNode, prop);
                        var wnd = EditorWindow.GetWindow<DirectorEditorWindow>();
                        if (wnd != null) wnd.StartSandboxTest(prop);
                    }) { text = "▶ 샌드박스 미리보기" };
                    previewBtn.style.flexGrow = 1;
                    previewBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
                    btnRow.Add(previewBtn);

                    tableBox.Add(btnRow);
                    fieldContainer.Add(tableBox);
                    break;
            }

            row.Add(fieldContainer);
            _contentContainer.Add(row);
        }

        private VisualElement _previewContainer;

        private void ShowPreview(DynamicProperty prop)
        {
            if (_previewContainer != null && _contentContainer.Contains(_previewContainer))
                _contentContainer.Remove(_previewContainer);

            _previewContainer = new VisualElement();
            _previewContainer.style.marginTop = 15;
            _previewContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f));
            _previewContainer.style.paddingTop = _previewContainer.style.paddingBottom = 10;
            _previewContainer.style.paddingLeft = _previewContainer.style.paddingRight = 10;
            _previewContainer.style.borderTopLeftRadius = 5; _previewContainer.style.borderTopRightRadius = 5;
            _previewContainer.style.borderBottomLeftRadius = 5; _previewContainer.style.borderBottomRightRadius = 5;

            var title = new Label("📺 미리보기 (Preview)");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.style.color = new Color(0.6f, 1f, 0.6f);
            _previewContainer.Add(title);

            var scroll = new ScrollView();
            scroll.style.height = 300;

            foreach(var row in prop.TableRows)
            {
                if (row.Cells.Count >= 2)
                {
                    string speaker = row.Cells[0];
                    string text = row.Cells[1];
                    
                    var bubble = new VisualElement();
                    bubble.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.3f));
                    bubble.style.paddingTop = bubble.style.paddingBottom = 8;
                    bubble.style.paddingLeft = bubble.style.paddingRight = 10;
                    bubble.style.marginBottom = 8;
                    bubble.style.borderTopLeftRadius = 5; bubble.style.borderTopRightRadius = 5;
                    bubble.style.borderBottomLeftRadius = 5; bubble.style.borderBottomRightRadius = 5;

                    var nameLabel = new Label(speaker);
                    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    nameLabel.style.color = new Color(0.8f, 0.8f, 1f);
                    nameLabel.style.marginBottom = 4;
                    bubble.Add(nameLabel);

                    var textLabel = new Label(text);
                    textLabel.style.whiteSpace = WhiteSpace.Normal;
                    bubble.Add(textLabel);

                    scroll.Add(bubble);
                }
            }
            _previewContainer.Add(scroll);
            _contentContainer.Add(_previewContainer);
        }

        private void RenderDNAPreview()
        {
            var box = new VisualElement();
            box.style.height = 150;
            box.style.backgroundColor = Color.black;
            box.style.marginBottom = 10;
            box.style.marginTop = 10;
            box.style.justifyContent = Justify.Center;
            box.style.alignItems = Align.Center;

            var label = new Label("🎨 3D 아바타 실시간 프리뷰 (RenderTexture)");
            label.style.color = Color.gray;
            box.Add(label);

            _contentContainer.Add(box);
        }

        private void RenderPuzzleSimulator()
        {
            var box = new VisualElement();
            box.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.2f));
            box.style.paddingTop = box.style.paddingBottom = box.style.paddingLeft = box.style.paddingRight = 10;
            box.style.marginBottom = 10;
            box.style.marginTop = 10;
            box.style.borderTopLeftRadius = 5; box.style.borderTopRightRadius = 5;
            box.style.borderBottomLeftRadius = 5; box.style.borderBottomRightRadius = 5;

            var l1 = new Label("[1. 전제] 슬롯"); l1.AddToClassList("puzzle-slot");
            var l2 = new Label("[2. 모순] 슬롯"); l2.AddToClassList("puzzle-slot");
            var l3 = new Label("[3. 종결] 슬롯"); l3.AddToClassList("puzzle-slot");

            box.Add(l1); box.Add(l2); box.Add(l3);

            var simBtn = new Button(() => { Debug.Log("시뮬레이션 실행 (NaN 체크)"); }) { text = "▶ 검증 시뮬레이션" };
            simBtn.style.marginTop = 10;
            box.Add(simBtn);

            _contentContainer.Add(box);
        }

        private void ShowAddPropertyMenu(WorkspaceNodeData data)
        {
            var menu = new GenericMenu();
            foreach (PropertyType type in System.Enum.GetValues(typeof(PropertyType)))
            {
                menu.AddItem(new GUIContent(type.ToString()), false, () => {
                    data.Properties.Add(new DynamicProperty { Type = type, PropertyName = $"New {type}" });
                    EditorUtility.SetDirty(data);
                    RenderDynamicProperties();
                });
            }
            menu.ShowAsContext();
        }

        private void SaveAsTemplate()
        {
            if (_activeNode == null || _activeNode.NodeData == null) return;
            
            var sourceData = _activeNode.NodeData;
            var newTemplate = ScriptableObject.CreateInstance<WorkspaceNodeData>();
            newTemplate.NodeTitle = sourceData.NodeTitle;
            newTemplate.TemplateType = sourceData.NodeTitle; // Use title as template type name
            newTemplate.ThemeColorHex = sourceData.ThemeColorHex;
            newTemplate.ThemeIcon = sourceData.ThemeIcon;
            
            foreach (var prop in sourceData.Properties)
            {
                newTemplate.Properties.Add(new DynamicProperty {
                    PropertyName = prop.PropertyName,
                    Type = prop.Type,
                    StringValue = prop.StringValue,
                    FloatValue = prop.FloatValue,
                    ColorValue = prop.ColorValue,
                    AssetValue = prop.AssetValue,
                    ShowAsBadge = prop.ShowAsBadge
                });
            }

            string safeName = sourceData.NodeTitle.Replace(" ", "_").Replace(":", "_");
            string path = $"Assets/Editor/StudyGameDirector/Workspace/Templates/{safeName}_Template.asset";
            
            AssetDatabase.CreateAsset(newTemplate, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[{sourceData.NodeTitle}] 템플릿 저장 완료! 경로: {path}");
        }

        private void UpdateNodeBadges()
        {
            if (_activeNode != null && _activeNode.NodeData != null)
            {
                var badges = _activeNode.NodeData.Properties
                    .Where(p => p.ShowAsBadge)
                    .Select(p => $"{p.PropertyName}: {GetPropertyValueAsString(p)}")
                    .ToArray();
                
                _activeNode.SetBadges(badges);
            }
        }

        private string GetPropertyValueAsString(DynamicProperty p)
        {
            switch (p.Type)
            {
                case PropertyType.Text: return p.StringValue;
                case PropertyType.Number: return p.FloatValue.ToString();
                case PropertyType.Color: return $"#{ColorUtility.ToHtmlStringRGB(p.ColorValue)}";
                case PropertyType.Asset: return p.AssetValue != null ? p.AssetValue.name : "None";
                case PropertyType.Dropdown: return p.StringValue;
                default: return "";
            }
        }
    }
}
