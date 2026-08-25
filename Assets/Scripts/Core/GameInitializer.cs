using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField]public ScoreView scoreView;
    void Start()
    {
        ScoreModel model = new ScoreModel();
        ScoreViewModel viewModel = new ScoreViewModel(model);

        scoreView.Bind(viewModel);
    }
}
