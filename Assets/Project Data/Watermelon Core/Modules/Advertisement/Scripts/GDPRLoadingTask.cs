using UnityEngine;

namespace Watermelon
{
    public sealed class GDPRLoadingTask : LoadingTask
    {
        GDPRPanel gdprPanel;

        public override void Activate()
        {
            isActive = true;

            var gdprPanelObject = GameObject.Instantiate(AdsManager.InitModule.GDPRPrefab);
            gdprPanelObject.transform.ResetGlobal();

            gdprPanel = gdprPanelObject.GetComponent<GDPRPanel>();
            gdprPanel.Initialise(this);
        }
    }
}
