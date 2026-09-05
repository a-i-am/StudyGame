using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 또는 TMPro

public class AlgorithmVisualizer : MonoBehaviour
{
    public Transform[] uiBars; // 유니티 인스펙터에서 할당할 UI 막대들
    private int[] data = { 5, 2, 4, 1, 3 };

    void Start()
    {
        // 1. 초기 UI 세팅
        UpdateVisuals();

        // 2. 알고리즘 코루틴 시작
        StartCoroutine(BubbleSortVisualized());
    }

    IEnumerator BubbleSortVisualized()
    {
        for (int i = 0; i < data.Length - 1; i++)
        {
            for (int j = 0; j < data.Length - 1 - i; j++)
            {
                // [시각화 포인트 1] 현재 비교 중인 인덱스 강조 (예: 색상 변경)

                if (data[j] > data[j + 1])
                {
                    // 데이터 스왑
                    int temp = data[j];
                    data[j] = data[j + 1];
                    data[j + 1] = temp;

                    // [시각화 포인트 2] UI 갱신 및 대기
                    UpdateVisuals();

                    // DOTween 애니메이션 재생 시간이나 사용자가 볼 수 있는 딜레이 부여
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < data.Length; i++)
        {
            RectTransform rt = uiBars[i].GetComponent<RectTransform>();

            // 너비(x)는 기존 값을 유지하고, 높이(y)만 데이터에 비례하여 픽셀 단위로 적용
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, data[i] * 150f);
        }
    }
}