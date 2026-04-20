using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Development.UI;
using Development.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Development.Managers
{
    public class PanelManager : Singleton<PanelManager>
    {
        public Dictionary<string, Panel> panels = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _instanceHandles = new();
        private readonly Dictionary<string, UniTask<Panel>> _loadingTasks = new();

        private void Start()
        {
            var list = GetComponentsInChildren<Panel>();
            foreach (var panel in list)
            {
                panels[panel.name] = panel;
            }
        }

        public void OpenPanel(string name)
        {
            OpenPanelAsync(name).Forget();
        }

        public void ClosePanel(string name)
        {
            if (!IsExisted(name))
            {
                return;
            }

            panels[name].Close();
        }

        public async UniTask<Panel> GetPanelAsync(string panelName)
        {
            if (IsExisted(panelName))
            {
                return panels[panelName];
            }

            if (_loadingTasks.TryGetValue(panelName, out UniTask<Panel> loadingTask))
            {
                return await loadingTask;
            }

            UniTask<Panel> task = LoadPanelInternal(panelName);
            _loadingTasks[panelName] = task;
            try
            {
                return await task;
            }
            finally
            {
                _loadingTasks.Remove(panelName);
            }
        }

        public void RemovePanel(string name)
        {
            panels.TryGetValue(name, out Panel panel);

            if (_instanceHandles.TryGetValue(name, out var handle))
            {
                Addressables.ReleaseInstance(handle);
                _instanceHandles.Remove(name);
            }
            else if (panel != null)
            {
                Destroy(panel.gameObject);
            }

            if (IsExisted(name))
            {
                panels.Remove(name);
            }
        }

        public async UniTask OpenPanelAsync(string name)
        {
            Panel panel = await GetPanelAsync(name);
            panel.Open();
        }

        public void CloseAllPanels()
        {
            List<Panel> panelList = new(panels.Values);
            foreach (Panel panel in panelList)
            {
                panel.Close();
            }
        }

        private bool IsExisted(string name)
        {
            return panels.ContainsKey(name);
        }

        public void Unregister(string name)
        {
            _loadingTasks.Remove(name);
            RemovePanel(name);
        }

        private async UniTask<Panel> LoadPanelInternal(string panelName)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(panelName, transform);

            try
            {
                GameObject panelObject = await handle.ToUniTask();
                Panel newPanel = panelObject.GetComponent<Panel>();
                newPanel.name = panelName;
                newPanel.gameObject.SetActive(false);

                panels[panelName] = newPanel;
                _instanceHandles[panelName] = handle;
                return newPanel;
            }
            catch
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw;
            }
        }
    }
}
