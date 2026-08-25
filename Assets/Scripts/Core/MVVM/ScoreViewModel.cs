using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreViewModel : BaseViewModel
{
    private ScoreModel _model;
    public BindableProperty<int> Score { get; private set; }
    public ScoreViewModel(ScoreModel model)
    {
        _model = model;
        Score = new BindableProperty<int>(_model.currentScore);
    }
    public void AddScore(int amount)
    {
        _model.currentScore += amount;
        Score.Value = _model.currentScore;
    }
    public override void Dispose()
    {
        Score.OnValueChanged = null;
    }
}
