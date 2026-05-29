using System;
using System.Collections.Generic;

namespace LostInAForgottenCity.Engine
{
    public class DialogueEngine
    {
        private Dictionary<string, DialogueScene> _scenes = new();

        public event Action<DialogueLine, Action>? OnLine;
        public event Action<List<DialogueChoice>>? OnChoices;
        public event Action? OnSceneComplete;
        public event Action<string>? OnAutoNext;
        public event Action<string>? OnSceneStart;

        public void LoadDialogue(
            Dictionary<string, DialogueScene> scenes)
        {
            _scenes = scenes;
        }

        public void StartScene(string sceneId)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
                return;

            PlayLines(scene, 0);
        }

        private void PlayLines(DialogueScene scene, int index)
        {
            if (index == 0)
                OnSceneStart?.Invoke(scene.Id);

            if (index >= scene.Lines.Count)
            {
                if (scene.Choices.Count > 0)
                    OnChoices?.Invoke(scene.Choices);
                else if (!string.IsNullOrEmpty(scene.AutoNextId))
                    OnAutoNext?.Invoke(scene.AutoNextId);
                else
                    OnSceneComplete?.Invoke();
                return;
            }

            var line = scene.Lines[index];
            OnLine?.Invoke(line, () => PlayLines(scene, index + 1));
        }
        
        public void SelectChoice(int index)
        {
            // Will be called from outside
        }

        public void GoToScene(string sceneId)
        {
            StartScene(sceneId);
        }
        public void UnsubscribeAll()
        {
            OnLine = null;
            OnChoices = null;
            OnAutoNext = null;
            OnSceneComplete = null;
        }
    }
}