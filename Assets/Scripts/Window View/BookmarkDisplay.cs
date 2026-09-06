using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class BookmarkDisplay : MonoBehaviour
{
    [Header("데이터 소스")]
    [SerializeField] private BookmarkData myData;

    [Header("UI 연결")]
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button bookmarkButton;

    private void Start()
    {
        if (myData != null)
        {
            thumbnailImage.sprite = myData.thumbnail;
            titleText.text = myData.title;
            descText.text = myData.description;
        }

        bookmarkButton.onClick.AddListener(OnBookmarkClicked);
    }

    private void OnBookmarkClicked()
    {
        if (myData != null && WebManager.Instance != null)
        {
            WebManager.Instance.OpenWeb(myData.youtubeVideoID);
        }
    }

}
