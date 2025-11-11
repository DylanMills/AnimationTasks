// UIPrompt.cs (very small utility - optional; replace with your game's UI system)
using UnityEngine;

public static class UIPrompt
{
    // NOTE: This is a tiny placeholder. Replace with your own UI manager.
    static GameObject promptGO;

    public static void Show(string text)
    {
        // naive implementation: use Debug.Log for visibility in example projects
        Debug.Log("UI Prompt: " + text);
        // In your game, hook this up to enable a Canvas element: e.g., PromptText.text = text; PromptCanvas.enabled = true;
    }

    public static void Hide()
    {
        Debug.Log("UI Prompt: hide");
    }
}
