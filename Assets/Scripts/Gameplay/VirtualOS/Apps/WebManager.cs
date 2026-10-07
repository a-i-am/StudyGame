using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine;
using VoltstroStudios.UnityWebBrowser;
using System.Collections.Generic;
using Unity.VisualScripting;



#if UNITY_EDITOR || UNITY_STANDALONE
using UnityWebBrowser;
#endif

public class WebManager : MonoBehaviour
{
    [Header("공통 설정")]
    [SerializeField] private string targetUrl = "https://google.com";
    [SerializeField] private RectTransform WebModalWindow; // 웹브라우저가 띄워질 UI 패널 영역

    public static WebManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

#if !UNITY_ANDROID && !UNITY_IOS
    [Header("PC / 에디터 (UWB) 설정")]
    // 이미지 띄울 유니티 기본 컴포넌트
    [SerializeField] private RawImage uwbRawImage;
    // UWB 에셋 내의 브라우저 UI 컴포넌트 혹은 텍스처를 렌더링할 RawImage 컴포넌트 연결
    [SerializeField] private WebBrowserUIBasic uwbClient;
#endif

    public void OpenWeb(string url)
    {
        string finalUrl = url;
        bool isYoutube = false;

        if (url.Contains("youtube.com") || url.Contains("youtu.be"))
        {
            isYoutube = true;
            // Match idMatch = Regex.Match(url, @"(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))([\w-]{11})");

            // if (idMatch.Success)
            // {
            //     string videoID = idMatch.Groups[1].Value;
            //     finalUrl = $"https://www.youtube-nocookie.com/embed/{videoID}?";

            //     // 시간 파라미터 (t= 또는 start=) 추출 및 임베드용 변환
            //     Match timeMatch = Regex.Match(url, @"[?&](t|start)=([^&#\s]+)");
            //     if (timeMatch.Success)
            //     {
            //         string timeStr = timeMatch.Groups[2].Value;
            //         int seconds = ParseYoutubeTime(timeStr);
            //         finalUrl += $"start={seconds}&"; // 임베드 플레이어는 'start'만 지원함
            //     }

            //     // 재생목록 파라미터 (list=) 추출
            //     Match listMatch = Regex.Match(url, @"[?&]list=([^&#\s]+)");
            //     if (listMatch.Success)
            //     {
            //         finalUrl += $"list={listMatch.Groups[1].Value}&";
            //     }

            //     // 문자열 끝에 남은 ?, & 기호 제거
            //     finalUrl = finalUrl.TrimEnd('?', '&');
            // }
        }

#if UNITY_EDITOR || UNITY_STANDALONE

        // [PC / 컴패니언 앱 환경]
        // - 유튜브는 코덱 제약 및 데스크톱 UX를 고려해 OS 기본 브라우저(Application.OpenURL)로 호출
        // - 위키피디아 등 일반 웹문서는 인게임 UWB 창으로 부드럽게 렌더링
        if (isYoutube)
        {
            Debug.Log($"[PC Companion] 유튜브 외부 브라우저 호출: {finalUrl}");
            Application.OpenURL(finalUrl);
        }
        else if (uwbClient != null)
        {
            Debug.Log($"[PC Companion] 인게임 웹 문서 렌더링: {finalUrl}");
            uwbClient.NavigateUrl(finalUrl);
        }
#elif UNITY_ANDROID || UNITY_IOS
// [모바일 환경]
        // - 게임 앱 이탈 방지: 유튜브든 위키든 가리지 않고 무조건 인게임 웹뷰(UniWebView 등)로 호출
        Debug.Log($"[Mobile In-App] 인게임 웹뷰 호출: {finalUrl}");
        
        /* UniWebView 연동 로직
        if (uniWebView == null)
        {
            uniWebView = gameObject.AddComponent<UniWebView>();
            
            // 앞서 만든 BrowserWindowBase의 ContentArea(RectTransform) 영역에 웹뷰를 맞춥니다.
            // uniWebView.ReferenceRectTransform = contentAreaRect; 
            
            uniWebView.SetShowSpinnerWhileLoading(true);
        }

        uniWebView.Load(url);
        uniWebView.Show();
        */

#endif
    }

    // 유튜브 시간 포맷(예: 1h30m10s)을 초(Seconds) 단위로 일괄 계산하는 헬퍼 함수
    private int ParseYoutubeTime(string timeStr)
    {
        int totalSeconds = 0;
        Match h = Regex.Match(timeStr, @"(\d+)h");
        Match m = Regex.Match(timeStr, @"(\d+)m");
        Match s = Regex.Match(timeStr, @"(\d+)s");

        if (h.Success) totalSeconds += int.Parse(h.Groups[1].Value) * 3600;
        if (m.Success) totalSeconds += int.Parse(h.Groups[1].Value) * 60;
        if (s.Success) totalSeconds += int.Parse(h.Groups[1].Value);

        // h, m, s 단위 없이 숫자만 들어온 경우 (예: t=120)
        if (totalSeconds == 0 && int.TryParse(timeStr, out int rawSeconds))
        {
            totalSeconds = rawSeconds;
        }

        return totalSeconds;
    }
}