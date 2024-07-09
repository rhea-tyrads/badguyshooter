using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using com.adjust.sdk;
using Firebase.Analytics;
using MobileTools.SDK;
using MobileTools.SDK.Firebase;
using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace MobileTools.IAPshop
{
    public class Shop : ScreenUI, IDetailedStoreListener
    {
        public bool testMode;
        public Sprite goldIcon;
        public Sprite critIcon;
        public Sprite respawnIcon;
        public Sprite hpIcon;
        public ScreenUI successUI;
        public ShopListener listener;
        public string noAdsID = "no_ads";

        [Header("Last purchase")]
        //public Data data;
        //public Payload payload;
        //public PayloadData payData;
        IStoreController _controller;

        void Start()
        {
            Hide();
            InitItems();
            InitBuilder();
            SDKEvents.Instance.OnTryPurchaseNoAds += TryPurchaseRemoveAds;
            if (successUI) successUI.Hide();
            RestorePurchases();
        }

        void OnDisable()
        {
            if (!SDKEvents.Instance) return;
            SDKEvents.Instance.OnTryPurchaseNoAds -= TryPurchaseRemoveAds;
        }

        void InitBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var item in listener.items)
                builder.AddProduct(item.Id, ProductType.Consumable);
            builder.AddProduct(noAdsID, ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            //  Debug.Log("Shop Initialized");
            _controller = controller;
            //  CheckNoAds(noAdsID);
        }

        void InitItems()
        {
            if (!listener) Debug.LogError("No shop listener");

            foreach (var item in listener.items)
            {
                PackData data = new()
                {
                    goldIcon = goldIcon,
                    critIcon = critIcon,
                    hpIcon = hpIcon,
                    respawnIcon = respawnIcon
                };

                item.Set(data);
                item.OnTryPurchase += TryPurchase;
            }
        }

        private const string LastRestoreTimeKey = "LastRestoreTime";

        private bool ShouldRestorePurchases()
        {
            var lastRestoreTimeString = PlayerPrefs.GetString(LastRestoreTimeKey, string.Empty);

            if (string.IsNullOrEmpty(lastRestoreTimeString))
            {
                // No restore time recorded, so we should restore purchases
                return true;
            }

            var lastRestoreTime = DateTime.Parse(lastRestoreTimeString);
            var currentTime = DateTime.Now;

            // Check if more than a day has passed since the last restore
            return (currentTime - lastRestoreTime).TotalDays >= 1;
        }

        void TryPurchase(ShopItem item)
        {
            Debug.LogWarning("[IAP] TRY PURCHASE: " + item.Id);
            if (testMode)
                Purchase(item.Id);
            else
                _controller.InitiatePurchase(item.Id);
        }

        void TryPurchaseRemoveAds()
        {
            _controller.InitiatePurchase(noAdsID);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            var productID = product.definition.id;
            Purchase(productID);

            var item = listener.Find(productID);
            var token = product.receipt;
            var transaction = product.transactionID;

            var adjustEvent = new AdjustEvent("eb9edt");
            adjustEvent.setProductId(productID);
            adjustEvent.setPurchaseToken(token);
            adjustEvent.setRevenue(item == null ? productID == "no_ads" ? 47700 : 1 : item.priceIDR, "IDR");
            adjustEvent.setTransactionId(transaction);
            Adjust.trackEvent(adjustEvent);

            FirebaseEvent(purchaseEvent.purchasedProduct.definition.id);


            return PurchaseProcessingResult.Complete;
        }

        void FirebaseEvent(string productId)
        {
            FirebaseAnalytics.LogEvent("in_app_purchase", new Parameter[]
            {
                new("product_id", productId),
                new("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture))
            });

            Debug.Log("In-App Purchase event sent to Firebase");
        }

        void Purchase(string id)
        {
            if (id == noAdsID)
            {
                Keys.PurchaseNoAds();
                return;
            }

            Debug.Log("Purchase complete: " + id);

            SDKEvents.Instance.ProductPurchase(id);
            if (successUI) successUI.Show();
            var item = listener.Find(id);
            var token = item.adjustToken;
            var send = new AdjustEvent(token);
            Adjust.trackEvent(send);
        }

        List<string> GetPurchasedProducts()
            => (from product in _controller.products.all
                where product.hasReceipt
                select product.definition.id).ToList();

        
        void RestorePurchases()
        {
            if (!ShouldRestorePurchases()) return;
            
            var purchasedProducts = GetPurchasedProducts();
            foreach (var id in purchasedProducts)
            {
                if (id != noAdsID)
                {
                    Keys.PurchaseNoAds();
                    continue;
                }

                var item = listener.Find(id);
                if (item == null) continue;

                listener.weapons.UnlockWeapon(item.weapon);
                listener.weapons.UnlockWeapon(item.weapon_2);
                var character = listener.FindCharacter(item.skin);
                if (character.onlyShop) character.Purchase();
                var character2 = listener.FindCharacter(item.skin_2);
                if (character2.onlyShop) character2.Purchase();

                Debug.Log($"Purchased product: {id}");
            }

            PlayerPrefs.SetString(LastRestoreTimeKey, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
 

        void CheckSubscription(string id)
        {
            var product = _controller.products.WithID(id);
            if (product == null)
                print("product not found !!");
            else
            {
                try
                {
                    if (product.hasReceipt)
                    {
                        var manager = new SubscriptionManager(product, null);
                        var info = manager.getSubscriptionInfo();
                        /*print(info.getCancelDate());
                        print(info.getExpireDate());
                        print(info.getFreeTrialPeriod());
                        print(info.getIntroductoryPrice());
                        print(info.getProductId());
                        print(info.getPurchaseDate());
                        print(info.getRemainingTime());
                        print(info.getSkuDetails());
                        print(info.getSubscriptionPeriod());
                        print(info.isAutoRenewing());
                        print(info.isCancelled());
                        print(info.isExpired());
                        print(info.isFreeTrial());
                        print(info.isSubscribed());*/
                        Debug.Log(info.isSubscribed() == Result.True ? "We are subscribed" : "Un subscribed");
                    }
                    else
                    {
                        Debug.Log("receipt not found !!");
                    }
                }
                catch (Exception)
                {
                    Debug.Log("It only work for Google store, app store, amazon store, you are using fake store!!");
                }
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
            => Debug.Log("failed" + error);

        public void OnInitializeFailed(InitializationFailureReason error, string message) =>
            Debug.Log("initialize failed" + error + message);

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var msg = "purchase failed: " + failureReason;
            Debug.Log(msg);
            // EventManager.Instance.ProductPurchaseFAIL(msg);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var msg = "purchase failed: " + failureDescription.reason;
            Debug.Log(msg);
            // EventManager.Instance.ProductPurchaseFAIL(msg);
        }
    }
}