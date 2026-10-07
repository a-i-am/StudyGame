using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class PhoneUIController : MonoBehaviour
{
    [SerializeField] private PhoneUIDocumentController phoneView;
    public UIDocument phoneUIDocument;

    private readonly WaitForSeconds messageDelay = new WaitForSeconds(0.8f);
    private readonly WaitForSeconds closeDelay = new WaitForSeconds(2.0f);
    private Coroutine displayCoroutine;

    private void Awake()
    {
        EnsurePhoneView();
    }

    private void OnEnable()
    {
        // if (StageManager.Instance != null)
        // {
        //     StageManager.Instance.OnSequenceStarted += HandleSequence;
        // }
    }

    private void OnDisable()
    {
        // if (StageManager.Instance != null)
        // {
        //     StageManager.Instance.OnSequenceStarted -= HandleSequence;
        // }

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
    }

    private void Start()
    {
        EnsurePhoneView();
        if (phoneView != null)
        {
            phoneView.SetVisible(false);
        }
    }

    private void EnsurePhoneView()
    {
        if (phoneView == null)
        {
            phoneView = GetComponent<PhoneUIDocumentController>();
        }

        if (phoneView == null && phoneUIDocument != null)
        {
            phoneView = phoneUIDocument.GetComponent<PhoneUIDocumentController>();
        }

        if (phoneView == null)
        {
            phoneView = FindFirstObjectByType<PhoneUIDocumentController>(FindObjectsInactive.Include);
        }
    }

    // private void HandleSequence(SequenceNode node)
    // {
    //     if (node.sequenceType == SeqType.Narrative && node.snsData != null)
    //     {
    //         if (displayCoroutine != null)
    //         {
    //             StopCoroutine(displayCoroutine);
    //         }
    //         displayCoroutine = StartCoroutine(DisplayMessages(node.snsData));
    //     }
    // }

    // private IEnumerator DisplayMessages(SNSData data)
    // {
    //     if (phoneView == null)
    //     {
    //         EnsurePhoneView();
    //     }

    //     if (phoneView == null)
    //     {
    //         yield break;
    //     }

    //     phoneView.SetVisible(true);
    //     phoneView.SetSender(data.senderName, data.profileImage);
    //     phoneView.ClearMessages();

    //     if (data.messages != null)
    //     {
    //         for (int i = 0; i < data.messages.Count; i++)
    //         {
    //             yield return messageDelay;
    //             phoneView.AddMessage(data.messages[i]);
    //         }
    //     }

    //     yield return closeDelay;
    //     phoneView.SetVisible(false);
    //     displayCoroutine = null;
    // }
}