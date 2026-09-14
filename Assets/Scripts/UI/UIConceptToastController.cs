using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace StudyGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIConceptToastController : MonoBehaviour
    {
        [SerializeField] private float displayDuration = 3.0f;

        private UIDocument uiDocument;
        private VisualElement toastContainer;

        private Queue<ConceptData> toastQueue = new Queue<ConceptData>();
        private bool isDisplayingToast = false;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;

            toastContainer = root.Q<VisualElement>("toast-container");
            if (toastContainer != null)
            {
                toastContainer.pickingMode = PickingMode.Ignore;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(SubscribeToManager());
        }

        private IEnumerator SubscribeToManager()
        {
            while (ConceptArchiveManager.Instance == null)
            {
                yield return null;
            }
            ConceptArchiveManager.Instance.OnConceptUnlocked += EnqueueToast;
        }

        private void OnDisable()
        {
            if (ConceptArchiveManager.Instance != null)
            {
                ConceptArchiveManager.Instance.OnConceptUnlocked -= EnqueueToast;
            }
        }

        private void EnqueueToast(ConceptData concept)
        {
            if (concept == null) return;
            toastQueue.Enqueue(concept);

            if (!isDisplayingToast)
            {
                StartCoroutine(ProcessToastQueue());
            }
        }

        private IEnumerator ProcessToastQueue()
        {
            isDisplayingToast = true;

            while (toastQueue.Count > 0)
            {
                ConceptData concept = toastQueue.Dequeue();
                yield return ShowToast(concept);
            }

            isDisplayingToast = false;
        }

        private IEnumerator ShowToast(ConceptData concept)
        {
            if (toastContainer == null) yield break;

            VisualElement toastBox = new VisualElement();
            toastBox.AddToClassList("toast-box");
            toastBox.pickingMode = PickingMode.Ignore;

            VisualElement iconElem = new VisualElement();
            iconElem.AddToClassList("toast-icon");
            iconElem.pickingMode = PickingMode.Ignore;
            if (concept.diaryIcon != null)
            {
                iconElem.style.backgroundImage = new StyleBackground(concept.diaryIcon);
            }
            toastBox.Add(iconElem);

            VisualElement textContainer = new VisualElement();
            textContainer.AddToClassList("toast-text-container");
            textContainer.pickingMode = PickingMode.Ignore;

            Label subtitle = new Label("★ 수학 개념 해금 완료");
            subtitle.AddToClassList("toast-subtitle");
            subtitle.pickingMode = PickingMode.Ignore;
            textContainer.Add(subtitle);

            Label title = new Label(concept.title);
            title.AddToClassList("toast-title");
            title.pickingMode = PickingMode.Ignore;
            textContainer.Add(title);

            toastBox.Add(textContainer);
            toastContainer.Add(toastBox);

            yield return new WaitForEndOfFrame();
            toastBox.AddToClassList("toast-visible");

            yield return new WaitForSeconds(displayDuration);

            toastBox.RemoveFromClassList("toast-visible");
            yield return new WaitForSeconds(0.4f);

            toastContainer.Remove(toastBox);
        }
    }
}
