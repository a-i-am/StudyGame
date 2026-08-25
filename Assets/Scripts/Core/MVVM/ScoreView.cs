using TMPro;
using UnityEngine;

public class ScoreView : BaseView<ScoreViewModel>
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public override void Bind(ScoreViewModel vm)
    {
        base.Bind(vm);

        viewModel.Score.OnValueChanged += UpdateScoreUI;

        UpdateScoreUI(viewModel.Score.Value);
    }

    private void UpdateScoreUI(int newScore)
    {
        scoreText.text = $"현재 점수: {newScore}";
    }
    public void OnClickPlusButton()
    {
        viewModel.AddScore(10);
    }

    protected override void OnDestroy()
    {
        if (viewModel != null)
        {
            viewModel.Score.OnValueChanged -= UpdateScoreUI;
        }
        base.OnDestroy();
    }
}
