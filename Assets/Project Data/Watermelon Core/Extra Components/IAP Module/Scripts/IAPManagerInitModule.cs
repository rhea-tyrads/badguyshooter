using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Monetization/IAP Manager")]
    public class IAPManagerInitModule : InitModule
    {
        [SerializeField] IAPSettings settings;
        [SerializeField] GameObject canvas;

        public IAPManagerInitModule()
        {
            moduleName = "IAP Manager";
        }

        public override void CreateComponent(Initialiser Initialiser)
        {
            return;
            IAPManager.Initialise(Initialiser.gameObject, settings);

            var canvasGameObject = Instantiate(canvas);
            canvasGameObject.transform.SetParent(Initialiser.InitialiserGameObject.transform);
            canvasGameObject.transform.localScale = Vector3.one;
            canvasGameObject.transform.localPosition = Vector3.zero;
            canvasGameObject.transform.localRotation = Quaternion.identity;
            canvasGameObject.GetComponent<IAPCanvas>().Init();
        }
    }
}