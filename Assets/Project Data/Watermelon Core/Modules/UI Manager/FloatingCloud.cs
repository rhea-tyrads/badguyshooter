using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Watermelon
{
    [System.Serializable]
    public class FloatingCloud
    {
        static FloatingCloud floatingCloud;

        [SerializeField] Data[] floatingCloudCases;

        [Space]
        [SerializeField] TextMeshProUGUI floatingText;

        static Dictionary<int, Data> floatingCloudLink = new();
        static List<Animation> activeClouds = new();

        public void Initialise()
        {
            floatingCloud = this;

            for (var i = 0; i < floatingCloudCases.Length; i++)
            {
                RegisterCase(floatingCloudCases[i]);
            }
        }

        public void Clear()
        {
            for(var i = 0; i < activeClouds.Count; i++)
            {
                activeClouds[i].Clear();
            }

            foreach(var floatingCloudData in floatingCloudLink.Values)
            {
                floatingCloudData.Clear();
            }

            floatingCloudLink.Clear();
            activeClouds.Clear();
        }

        public static void RegisterCase(FloatingCloudSettings floatingCloudSettings)
        {
            var cloudHash = StringToHash(floatingCloudSettings.Name);

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {floatingCloudSettings.Name} already registered!");

                return;
            }

            var floatingCloudCase = new Data(floatingCloudSettings);
            floatingCloudCase.Initialise();

            floatingCloudLink.Add(cloudHash, floatingCloudCase);
        }

        public static void RegisterCase(Data floatingCloudCase)
        {
            var cloudHash = StringToHash(floatingCloudCase.Name);

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {floatingCloudCase.Name} already registered!");

                return;
            }

            floatingCloudCase.Initialise();

            floatingCloudLink.Add(cloudHash, floatingCloudCase);
        }

        public static void SpawnCurrency(string key, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, SimpleCallback onCurrencyHittedTarget = null)
        {
            SpawnCurrency(key.GetHashCode(), rectTransform, targetTransform, elementsAmount, text, onCurrencyHittedTarget);
        }

        public static void SpawnCurrency(int hash, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, SimpleCallback onCurrencyHittedTarget = null)
        {
            if (!floatingCloudLink.ContainsKey(hash))
            {
                Debug.LogError($"Cloud with hash {hash} isn't registered!");

                return;
            }

            var animation = new Animation(floatingCloudLink[hash], rectTransform, targetTransform, elementsAmount, onCurrencyHittedTarget);
            animation.PlayAnimation();

            activeClouds.Add(animation);

            if (!string.IsNullOrEmpty(text))
            {
                floatingCloud.floatingText.gameObject.SetActive(true);
                floatingCloud.floatingText.text = text;
                floatingCloud.floatingText.transform.localScale = Vector3.zero;
                floatingCloud.floatingText.transform.SetParent(targetTransform);
                floatingCloud.floatingText.transform.SetAsLastSibling();
                floatingCloud.floatingText.transform.position = rectTransform.position;
                floatingCloud.floatingText.color = floatingCloud.floatingText.color.SetAlpha(1.0f);
                floatingCloud.floatingText.transform.DOScale(1, 0.3f, unscaledTime: true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    floatingCloud.floatingText.DOFade(0, 0.5f, unscaledTime: true).SetEasing(Ease.Type.ExpoIn);
                    floatingCloud.floatingText.transform.DOMove(floatingCloud.floatingText.transform.position.AddToY(0.1f), 0.5f, unscaledTime: true).SetEasing(Ease.Type.QuadIn).OnComplete(delegate
                    {
                        floatingCloud.floatingText.gameObject.SetActive(false);
                    });
                });
            }
        }

        public static void FloatingText(string text, RectTransform targetRectTransform, Vector3 position, int fontSize = 130)
        {
            if (!string.IsNullOrEmpty(text))
            {
                floatingCloud.floatingText.gameObject.SetActive(true);
                floatingCloud.floatingText.text = text;
                floatingCloud.floatingText.fontSize = fontSize;
                floatingCloud.floatingText.transform.localScale = Vector3.zero;
                floatingCloud.floatingText.transform.SetParent(targetRectTransform);
                floatingCloud.floatingText.transform.SetAsLastSibling();
                floatingCloud.floatingText.transform.position = position;
                floatingCloud.floatingText.color = floatingCloud.floatingText.color.SetAlpha(1.0f);
                floatingCloud.floatingText.transform.DOScale(1, 0.3f, unscaledTime: true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    floatingCloud.floatingText.DOFade(0, 1.2f, unscaledTime: true).SetEasing(Ease.Type.ExpoIn);
                    floatingCloud.floatingText.transform.DOMove(floatingCloud.floatingText.transform.position.AddToY(0.1f), 1.2f, unscaledTime: true).SetEasing(Ease.Type.QuadIn).OnComplete(delegate
                    {
                        floatingCloud.floatingText.gameObject.SetActive(false);
                    });
                });
            }
        }

        public static void OnAnimationFinished(Animation animation)
        {
            activeClouds.Remove(animation);
        }

        public static int StringToHash(string cloudName)
        {
            return cloudName.GetHashCode();
        }

        [System.Serializable]
        public class Data
        {
            [SerializeField] string name;
            public string Name => name;

            [SerializeField] GameObject prefab;
            public GameObject Prefab => prefab;

            [SerializeField] AudioClip appearAudioClip;
            public AudioClip AppearAudioClip => appearAudioClip;

            [SerializeField] AudioClip collectAudioClip;
            public AudioClip CollectAudioClip => collectAudioClip;

            [Space]
            [SerializeField] float cloudRadius;
            public float CloudRadius => cloudRadius;

            Pool pool;
            public Pool Pool => pool;

            public Data(FloatingCloudSettings settings)
            {
                name = settings.Name;
                prefab = settings.Prefab;

                cloudRadius = settings.CloudRadius;

                appearAudioClip = settings.AppearAudioClip;
                collectAudioClip = settings.CollectAudioClip;
            }

            public void Initialise()
            {
                pool = new Pool(new PoolSettings("FloatingCloud_" + name, prefab, 10, true));
            }

            public void Clear()
            {
                pool.Clear();
            }
        }

        public class Animation
        {
            Data floatingCloudData;
            RectTransform rectTransform;
            RectTransform targetTransform;
            int elementsAmount;
            SimpleCallback onCurrencyHittedTarget;

            TweenCaseCollection tweenCaseCollection;

            public Animation(Data floatingCloudData, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, SimpleCallback onCurrencyHittedTarget)
            {
                this.floatingCloudData = floatingCloudData;
                this.rectTransform = rectTransform;
                this.targetTransform = targetTransform;
                this.elementsAmount = elementsAmount;
                this.onCurrencyHittedTarget = onCurrencyHittedTarget;
            }

            public void PlayAnimation()
            {
                var targetRectTransform = targetTransform;

                tweenCaseCollection = Tween.BeginTweenCaseCollection();

                floatingCloudData.Pool.ReturnToPoolEverything(true);

                // Play appear sound
                if (floatingCloudData.AppearAudioClip != null)
                    AudioController.PlaySound(floatingCloudData.AppearAudioClip);

                var cloudRadius = floatingCloudData.CloudRadius;
                var centerPoint = rectTransform.position;

                var finishedElementsAmount = 0;

                var defaultPitch = 0.9f;
                var currencyHittedTarget = false;
                for (var i = 0; i < elementsAmount; i++)
                {
                    var elementObject = floatingCloudData.Pool.Get();
                    elementObject.transform.SetParent(targetRectTransform);

                    elementObject.transform.position = centerPoint;
                    elementObject.transform.localRotation = Quaternion.identity;
                    elementObject.transform.localScale = Vector3.one;

                    var elementImage = elementObject.GetComponent<Image>();
                    elementImage.color = Color.white.SetAlpha(0);

                    var moveTime = Random.Range(0.6f, 0.8f);

                    TweenCase currencyTweenCase = null;
                    var elementRectTransform = (RectTransform)elementObject.transform;

                    elementImage.DOFade(1, 0.2f, unscaledTime: true);
                    elementRectTransform.DOAnchoredPosition(elementRectTransform.anchoredPosition + (Random.insideUnitCircle * cloudRadius), moveTime, unscaledTime: true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                    {
                        tweenCaseCollection.AddTween(Tween.DelayedCall(0.1f, delegate
                        {
                            tweenCaseCollection.AddTween(elementRectTransform.DOScale(0.3f, 0.5f, unscaledTime: true).SetEasing(Ease.Type.ExpoIn));
                            tweenCaseCollection.AddTween(elementRectTransform.DOLocalMove(Vector3.zero, 0.5f, unscaledTime: true).SetEasing(Ease.Type.SineIn).OnComplete(delegate
                            {
                                if (!currencyHittedTarget)
                                {
                                    if (onCurrencyHittedTarget != null)
                                        onCurrencyHittedTarget.Invoke();

                                    currencyHittedTarget = true;
                                }

                                var punchTarget = true;
                                if (currencyTweenCase != null)
                                {
                                    if (currencyTweenCase.State < 0.8f)
                                    {
                                        punchTarget = false;
                                    }
                                    else
                                    {
                                        currencyTweenCase.Kill();
                                    }
                                }

                                if (punchTarget)
                                {
                                    // Play collect sound
                                    if (floatingCloudData.CollectAudioClip != null)
                                        AudioController.PlaySound(floatingCloudData.CollectAudioClip, pitch: defaultPitch);

                                    defaultPitch += 0.01f;

                                    currencyTweenCase = targetRectTransform.DOScale(1.2f, 0.15f, unscaledTime: true).OnComplete(delegate
                                    {
                                        currencyTweenCase = targetRectTransform.DOScale(1.0f, 0.1f, unscaledTime: true);

                                        tweenCaseCollection.AddTween(currencyTweenCase);
                                    });

                                    tweenCaseCollection.AddTween(currencyTweenCase);
                                }

                                elementObject.transform.SetParent(targetRectTransform);
                                elementRectTransform.gameObject.SetActive(false);

                                finishedElementsAmount++;
                                if(finishedElementsAmount >= elementsAmount)
                                {
                                    FloatingCloud.OnAnimationFinished(this);
                                }
                            }));
                        }, unscaledTime: true));
                    });
                }

                Tween.EndTweenCaseCollection();
            }

            public void Clear()
            {
                tweenCaseCollection.Kill();
            }
        }
    }
}
