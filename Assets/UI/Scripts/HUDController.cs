using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Drives the HUD: scores, phase indicator, turn countdown, match result.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Score")]
    public TextMeshProUGUI lblP0Score;
    public TextMeshProUGUI lblP1Score;

    [Header("Phase")]
    public TextMeshProUGUI lblPhase;
    public TextMeshProUGUI lblTimer;
    public TextMeshProUGUI lblTurn;

    [Header("Match Over")]
    public GameObject   matchOverPanel;
    public TextMeshProUGUI lblMatchResult;
    public Button       btnPlayAgain;
    public Button       btnMainMenu;

    [Header("References")]
    public TurnManager  turnManager;
    public ScoreManager scoreManager;

    void Start()
    {
        if (matchOverPanel != null) matchOverPanel.SetActive(false);

        turnManager.OnPhaseChanged   += OnPhaseChanged;
        turnManager.OnSelectTimerTick += OnTimerTick;
        turnManager.OnMatchOver      += OnMatchOver;
    }

    void Update()
    {
        if (lblP0Score != null) lblP0Score.text = $"P1  {scoreManager.GetScore(0):F0}";
        if (lblP1Score != null) lblP1Score.text = $"P2  {scoreManager.GetScore(1):F0}";
        if (lblTurn    != null) lblTurn.text    = $"Turn {turnManager.CurrentTurn}";
    }

    void OnPhaseChanged(TurnPhase phase)
    {
        if (lblPhase != null) lblPhase.text = phase switch
        {
            TurnPhase.PlayerSelectPhase => "Select Moves",
            TurnPhase.SimulatePhase     => "Simulating...",
            TurnPhase.EvaluatePhase     => "Evaluating",
            TurnPhase.MatchOver         => "Match Over",
            _                           => ""
        };
    }

    void OnTimerTick(float remaining)
    {
        if (lblTimer != null)
            lblTimer.text = remaining > 0 ? $"{remaining:F1}s" : "GO!";
    }

    void OnMatchOver(MatchResult result)
    {
        if (matchOverPanel != null) matchOverPanel.SetActive(true);

        if (lblMatchResult != null)
            lblMatchResult.text = result switch
            {
                MatchResult.Player0Wins => "Player 1 Wins!",
                MatchResult.Player1Wins => "Player 2 Wins!",
                MatchResult.Draw        => "Draw!",
                _                       => "Match Over"
            };
    }

    void OnDestroy()
    {
        if (turnManager == null) return;
        turnManager.OnPhaseChanged    -= OnPhaseChanged;
        turnManager.OnSelectTimerTick -= OnTimerTick;
        turnManager.OnMatchOver       -= OnMatchOver;
    }
}
