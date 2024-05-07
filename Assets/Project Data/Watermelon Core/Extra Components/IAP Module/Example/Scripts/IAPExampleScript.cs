#pragma warning disable 0649

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class IAPExampleScript : MonoBehaviour
    {
        [SerializeField] Text logText;
        [SerializeField] GameObject itemPrefab;
        [SerializeField] RectTransform contentParent;

        void Start()
        {
            logText.text = string.Empty;

            GameLoading.MarkAsReadyToHide();

            InitItems();
        }

        void InitItems()
        {
#if MODULE_IAP
            var values = (ProductKeyType[])Enum.GetValues(typeof(ProductKeyType));
            ItemPanelScript itemPanelScript;
            ProductData product;
            GameObject itemGameObject;

            for (var i = 0; i < values.Length; i++)
            {
                product = IAPManager.GetProductData(values[i]);

                if (product == null)
                {
                    Debug.Log("Null product :" + values[i]);
                    continue;
                }

                itemGameObject = Instantiate(itemPrefab, contentParent);
                itemGameObject.transform.position = new Vector3(0, -200 * i);
                itemPanelScript = itemGameObject.GetComponent<ItemPanelScript>();
                itemPanelScript.Item = values[i];
                itemPanelScript.Type = product.ProductType.ToString();
                itemPanelScript.Name =  values[i].ToString();
                itemPanelScript.Price = string.Format("({0} {1})", product.Price, product.ISOCurrencyCode);

                itemPanelScript.SetPurchasedTextActive(product.IsPurchased);
                
                if((product.ProductType == ProductType.Subscription) && (IAPManager.IsSubscribed(values[i])))
                {
                    itemPanelScript.Purchased = "(subscribed)";
                }

            }
            contentParent.sizeDelta = new Vector2(contentParent.sizeDelta.x, values.Length * 200);
#else
            Log("IAP Define is disabled!");
#endif
        }

        public void RestoreButton()
        {
            IAPManager.RestorePurchases();
        }

        #region Handle logs

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        void Log(string condition, string stackTrace, LogType type)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }

        void Log(string condition)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }
        #endregion
    }
}
