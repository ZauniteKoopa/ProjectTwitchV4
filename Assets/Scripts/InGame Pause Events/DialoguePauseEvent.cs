using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePauseEvent : IGamePauseEvent
{
    // Connect to the dialogue executor on the player
    [SerializeField]
    private DialogueExecutor mainDialogueExecutor;
    [SerializeField]
    private SimpleDialogueScene dialogueScene;


    // Main function to start up event with no consideration for pause
    protected override void startEventHelper() {
        mainDialogueExecutor.dialogueSceneEnd.AddListener(endEvent);
        mainDialogueExecutor.startScene(dialogueScene);
    }


    // Main function to end event with no consideration for pause
    protected override void endEventHelper() {
        mainDialogueExecutor.dialogueSceneEnd.RemoveListener(endEvent);
    }
}
