using System;
using ColorBlockJam.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelShortcuts : IInitializable, ITickable, IDisposable
    {
        private readonly InputAction next = new("Next Level", InputActionType.Button, "<Keyboard>/rightArrow");
        private readonly InputAction previous = new("Previous Level", InputActionType.Button, "<Keyboard>/leftArrow");
        private readonly ILevelProvider levels;
        private readonly IProgressionService progression;
        private readonly ILevelFlow flow;

        public LevelShortcuts(ILevelProvider levels, IProgressionService progression, ILevelFlow flow)
        {
            this.levels = levels;
            this.progression = progression;
            this.flow = flow;
        }

        public void Initialize()
        {
            if (!Debug.isDebugBuild || levels.IsEditorTest)
            {
                return;
            }

            next.Enable();
            previous.Enable();
        }

        public void Tick()
        {
            if (next.WasPressedThisFrame())
            {
                Open(progression.CurrentLevel + 1);
            }
            else if (previous.WasPressedThisFrame() && progression.CurrentLevel > 1)
            {
                Open(progression.CurrentLevel - 1);
            }
        }

        public void Dispose()
        {
            next.Dispose();
            previous.Dispose();
        }

        private void Open(int level)
        {
            progression.SetCurrentLevel(level);
            flow.PlayNext();
        }
    }
}
