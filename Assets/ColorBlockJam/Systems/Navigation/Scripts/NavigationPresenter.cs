using System;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Navigation
{
    public sealed class NavigationPresenter : IStartable, IDisposable
    {
        private readonly NavigationConfig config;
        private readonly PageNavigator navigator;
        private readonly TabBar tabBar;

        private int[] tabIndexByPage;

        public NavigationPresenter(NavigationConfig config, PageNavigator navigator, TabBar tabBar)
        {
            this.config = config;
            this.navigator = navigator;
            this.tabBar = tabBar;
        }

        public void Start()
        {
            navigator.Initialize(config);
            tabBar.Initialize(config);
            MapPagesToTabs();

            navigator.PageOpening += OnPageOpening;
            navigator.ScrollPositionChanged += OnScrollPositionChanged;
            tabBar.TabClicked += OnTabClicked;

            navigator.Open(config.DefaultPageId, instant: true);
            tabBar.Select(config.DefaultPageId, instant: true);
        }

        public void Dispose()
        {
            navigator.PageOpening -= OnPageOpening;
            navigator.ScrollPositionChanged -= OnScrollPositionChanged;
            tabBar.TabClicked -= OnTabClicked;
        }

        private void MapPagesToTabs()
        {
            tabIndexByPage = new int[navigator.Pages.Count];

            for (var i = 0; i < tabIndexByPage.Length; i++)
            {
                tabIndexByPage[i] = tabBar.IndexOf(navigator.Pages[i].PageId);
            }
        }

        private void OnTabClicked(string pageId)
        {
            navigator.Open(pageId, instant: false);
        }

        private void OnPageOpening(NavigationPage page)
        {
            tabBar.Select(page.PageId, instant: false);
        }

        private void OnScrollPositionChanged(float pagePosition)
        {
            if (tabIndexByPage.Length == 0)
            {
                return;
            }

            var lastPage = tabIndexByPage.Length - 1;
            var fromPage = Mathf.Clamp(Mathf.FloorToInt(pagePosition), 0, lastPage);
            var toPage = Mathf.Min(fromPage + 1, lastPage);
            var fromTab = tabIndexByPage[fromPage];
            var toTab = tabIndexByPage[toPage];

            if (fromTab < 0 || toTab < 0)
            {
                return;
            }

            tabBar.MoveHighlight(fromTab, toTab, Mathf.Clamp01(pagePosition - fromPage));
        }
    }
}
