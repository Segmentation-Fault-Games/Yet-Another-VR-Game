using System;
using erulathra;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

public class HeartsSubsystem : SceneSubsystem
{
    public event Action<int /** hearts */, int /** delta */> OnHeartsNumberChanged;
    
    public int Hearts { get; private set; }
    
    private bool entryAdded;
    
    public override void Initialize()
    {
        Hearts = GameplaySettings.Global.StartHearts;
    }
    
    public void HandleBrokenMug()
    {
        DecrementHearts();
    }
    
    public void HandleUnsatisfiedClient()
    {
        DecrementHearts();
    }

    private void DecrementHearts()
    {
        Hearts--;
        OnHeartsNumberChanged?.Invoke(Hearts, -1);
        
        if (Hearts < 0)
        {
            HandleGameOver();
        }
    }
    
    
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void HandleGameOver()
    {
        if (entryAdded)
            return;
        
        TransitionsSceneManger.Get().LoadGameOver();

        ScoreboardSaveData scoreboardSaveData = SaveManager.LoadScores();
        
        ScoreSubsystem scoreSubsystem = SceneSubsystemManager.GetSubsystem<ScoreSubsystem>();
        ScoreboardEntryData scoreboardEntryData = new()
        {
            entryScore = scoreSubsystem.CurrentScore,
            entryName = "Test  "  + scoreboardSaveData.highScores.Count.ToString()
        };
        
        scoreboardSaveData.highScores.Add(scoreboardEntryData);
        entryAdded = true;
        SaveManager.SaveScores(scoreboardSaveData);
    }
}