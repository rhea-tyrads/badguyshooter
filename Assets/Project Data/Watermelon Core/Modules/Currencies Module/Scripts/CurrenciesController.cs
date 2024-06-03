using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CurrenciesController : MonoBehaviour
    {
        static CurrenciesController currenciesController;

        [SerializeField] CurrenciesDatabase currenciesDatabase;
        public CurrenciesDatabase CurrenciesDatabase => currenciesDatabase;

        static Currency[] currencies;
        public static Currency[] Currencies => currencies;

        static Dictionary<CurrencyType, int> _dict;

        static bool _isInitialised;
        static event SimpleCallback onModuleInitialised;

        public virtual void Initialise()
        {
            if (_isInitialised) return;
            _isInitialised = true;
            
            currenciesController = this;
            currenciesDatabase.Initialise();
            currencies = currenciesDatabase.Currencies;

            _dict = new Dictionary<CurrencyType, int>();
            for (var i = 0; i < currencies.Length; i++)
            {
                if (!_dict.ContainsKey(currencies[i].CurrencyType))
                    _dict.Add(currencies[i].CurrencyType, i);
                else
                    Debug.LogError($"[Currency]: Currency with type {currencies[i].CurrencyType} exist!");

                var save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int) currencies[i].CurrencyType);
                currencies[i].SetSave(save);
            }

            onModuleInitialised?.Invoke();
            onModuleInitialised = null;
        }

        public static bool HasAmount(CurrencyType currencyType, int amount) =>
            currencies[_dict[currencyType]].Amount >= amount;

        public static int Get(CurrencyType currencyType) => currencies[_dict[currencyType]].Amount;

        public static Currency GetCurrency(CurrencyType currencyType) => currencies[_dict[currencyType]];

        public static void Set(CurrencyType currencyType, int amount)
        {
            var currency = currencies[_dict[currencyType]];
            currency.Amount = amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(0);
        }

        public static void Add(CurrencyType currencyType, int amount)
        {
            if (amount == 0) return;

            var currency = currencies[_dict[currencyType]];
            currency.Amount += amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(amount);
        }

        public static void Substract(CurrencyType currencyType, int amount)
        {
            var currency = currencies[_dict[currencyType]];
            currency.Amount -= amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(-amount);
        }

        public static void SubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            foreach (var currency in currencies)
                currency.OnCurrencyChanged += currencyChange;
        }

        public static void UnsubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            foreach (var t in currencies)
                t.OnCurrencyChanged -= currencyChange;
        }

        public static void InvokeOrSubcrtibe(SimpleCallback callback)
        {
            if (_isInitialised)
                callback?.Invoke();
            else
                onModuleInitialised += callback;
        }
    }

    public delegate void CurrencyChangeDelegate(Currency currency, int difference);
}