using System;
using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace MobileTools.IAPshop
{
    public class GoldShop : ScreenUI, IDetailedStoreListener
    {
        public ScreenUI successUI;
        public GoldShopListener listener;
        public string noAdsID = "no_ads";
        [Header("Last purchase")]
        //   public Data data;
        //  public Payload payload;
        //  public PayloadData payData;
        IStoreController _controller;

        void Start()
        {
            InitItems();
            InitBuilder();
            //  EventManager.Instance.OnTryPurchaseRemoveAds += TryPurchaseRemoveAds;
            if (successUI) successUI.Hide();
        }

        void OnDisable()
        {
            // if (!EventManager.Instance) return;
            //  EventManager.Instance.OnTryPurchaseRemoveAds -= TryPurchaseRemoveAds;
        }

        void InitBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var item in listener.consumables)
                builder.AddProduct(item.Id, ProductType.Consumable);
            builder.AddProduct(noAdsID, ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            //  Debug.Log("Shop Initialized");
            _controller = controller;
            CheckNoAds(noAdsID);
        }

        void InitItems()
        {
            if (!listener) Debug.LogError("No shop listener");
            foreach (var item in listener.consumables)
                item.OnTryPurchase += TryPurchaseConsumable;
        }

        void TryPurchaseConsumable(ConsumableItem item) => _controller.InitiatePurchase(item.Id);

        void TryPurchaseRemoveAds()
        {
            _controller.InitiatePurchase(noAdsID);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            var productID = product.definition.id;

            // EventManager.Instance.ProductPurchase(productID);
            // AudioPlayer.Instance.PlaySound(Game.Instance.gameplay.so.sounds.purchaseSuccess);

            var msg = "Purchase complete: " + productID;
            Debug.Log(msg);

            if (successUI) successUI.Show();
            return PurchaseProcessingResult.Complete;
        }

        void CheckNoAds(string id)
        {
            var product = _controller?.products.WithID(id);
            if (product == null) return;
            if (product.hasReceipt)
                listener.RemoveAds();
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