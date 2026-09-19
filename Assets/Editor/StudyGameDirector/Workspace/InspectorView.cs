using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using StudyGame.Data;

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

            var title = new Label("동적 작업대 (Inspector)");
            title.AddToClassList("workspace-title");
            Add(title);

            // Tabs
            var tabContainer = new VisualElement();
            tabContainer.AddToClassList("inspector-tab-container");

            var tabScenario = new Button(() => ShowTab("Scenario")) { text = "✍️ 시나리오" };
            var tabDNA = new Button(() => ShowTab("DNA")) { text = "🎨 아트/DNA" };
            var tabPuzzle = new Button(() => ShowTab("Puzzle")) { text = "🧩 룰/퍼즐" };

            tabScenario.AddToClassList("inspector-tab");
            tabDNA.AddToClassList("inspector-tab");
            tabPuzzle.AddToClassList("inspector-tab");

            tabContainer.Add(tabScenario);
            tabContainer.Add(tabDNA);
            tabContainer.Add(tabPuzzle);
            Add(tabContainer);

            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("inspector-content");
            Add(_contentContainer);

            ShowTab("Scenario"); // Default
        }

        public void BindNode(EpisodeNode node)
        {
            _activeNode = node;
            ShowTab("Scenario"); // Refresh
        }

        private void ShowTab(string tabName)
        {
            _contentContainer.Clear();
            if (_activeNode == null)
            {
                _contentContainer.Add(new Label("노드를 선택해주세요.") { style = { color = Color.gray, marginTop = 20, unityTextAlign = TextAnchor.MiddleCenter } });
                return;
            }

            switch (tabName)
            {
                case "Scenario":
                    RenderScenarioTab();
                    break;
                case "DNA":
                    RenderDNATab();
                    break;
                case "Puzzle":
                    RenderPuzzleTab();
                    break;
            }
        }

        private void RenderScenarioTab()
        {
            var title = new Label("만담 대본 에디터");
            title.AddToClassList("workspace-section-header");
            _contentContainer.Add(title);
            
            // Dummy example of a chat block
            var block = new VisualElement();
            block.AddToClassList("chat-block");
            block.AddToClassList("chat-block-doyoung");

            var header = new VisualElement();
            header.AddToClassList("chat-header");
            var nameLabel = new Label("도영 (Doyoung)");
            nameLabel.AddToClassList("chat-name");
            header.Add(nameLabel);
            
            var emotionEnum = new EnumField(Speaker.Doyoung);
            header.Add(emotionEnum);

            block.Add(header);

            var textArea = new TextField();
            textArea.multiline = true;
            textArea.value = "이 이상현상의 궤도는 비유클리드 기하학적 형태를 띄고 있어.";
            block.Add(textArea);

            _contentContainer.Add(block);

            // Add button
            var addBtn = new Button() { text = "+ 대사 블록 추가" };
            _contentContainer.Add(addBtn);
        }

        private void RenderDNATab()
        {
            var title = new Label("캐릭터 DNA 튜너");
            title.AddToClassList("workspace-section-header");
            _contentContainer.Add(title);

            if (_activeNode.LinkedDNA != null)
            {
                var so = new SerializedObject(_activeNode.LinkedDNA);
                
                var meshField = new PropertyField(so.FindProperty("HeadMeshId"));
                var capeField = new PropertyField(so.FindProperty("CapeLengthScale"));
                var sleeveField = new PropertyField(so.FindProperty("SleeveWidthScale"));
                var rimField = new PropertyField(so.FindProperty("RimLightColor"));
                var emissiveField = new PropertyField(so.FindProperty("FaceEmissiveColor"));
                var decalField = new PropertyField(so.FindProperty("DecalTexture"));

                meshField.Bind(so);
                capeField.Bind(so);
                sleeveField.Bind(so);
                rimField.Bind(so);
                emissiveField.Bind(so);
                decalField.Bind(so);

                _contentContainer.Add(meshField);
                _contentContainer.Add(capeField);
                _contentContainer.Add(sleeveField);
                _contentContainer.Add(rimField);
                _contentContainer.Add(emissiveField);
                _contentContainer.Add(decalField);
            }
            else
            {
                _contentContainer.Add(new Label("연결된 DNA 데이터가 없습니다."));
                if (GUILayout.Button("Create DNA Asset"))
                {
                    // Logic to create SO...
                }
            }
        }

        private void RenderPuzzleTab()
        {
            var title = new Label("질문 조립기 (보스전 검증)");
            title.AddToClassList("workspace-section-header");
            _contentContainer.Add(title);

            var slot1 = new VisualElement();
            slot1.AddToClassList("puzzle-slot");
            var l1 = new Label("[1. 전제]");
            l1.AddToClassList("puzzle-slot-label");
            slot1.Add(l1);
            
            var slot2 = new VisualElement();
            slot2.AddToClassList("puzzle-slot");
            var l2 = new Label("[2. 모순]");
            l2.AddToClassList("puzzle-slot-label");
            slot2.Add(l2);

            var slot3 = new VisualElement();
            slot3.AddToClassList("puzzle-slot");
            var l3 = new Label("[3. 종결]");
            l3.AddToClassList("puzzle-slot-label");
            slot3.Add(l3);

            _contentContainer.Add(slot1);
            _contentContainer.Add(slot2);
            _contentContainer.Add(slot3);

            var simBtn = new Button() { text = "▶ 검증 시뮬레이션 실행" };
            simBtn.style.marginTop = 10;
            simBtn.style.height = 30;
            _contentContainer.Add(simBtn);

            // Dummy result
            simBtn.clicked += () => {
                var result = new Label("무한 루프 (NaN) 발생! 보스 기믹 돌파 성공!");
                result.AddToClassList("puzzle-result-nan");
                result.style.marginTop = 10;
                _contentContainer.Add(result);
            };
        }
    }
}
