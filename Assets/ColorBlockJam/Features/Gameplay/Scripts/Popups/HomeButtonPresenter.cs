using System;
using Framework.UI.Buttons;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class HomeButtonPresenter : IInitializable, IDisposable
    {
        private readonly ActionButton button;
        private readonly ILevelFlow flow;

        public HomeButtonPresenter(ActionButton button, ILevelFlow flow)
        {
            this.button = button;
            this.flow = flow;
        }

        public void Initialize()
        {
            button.Clicked += flow.GoHome;
        }

        public void Dispose()
        {
            button.Clicked -= flow.GoHome;
        }
    }
}
