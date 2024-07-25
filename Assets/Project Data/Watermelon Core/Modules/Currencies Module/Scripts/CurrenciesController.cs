using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CurrenciesController : MonoBehaviour
    {
        static CurrenciesController _currenciesController;

        [SerializeField] CurrenciesDatabase currenciesDatabase;
        public CurrenciesDatabase CurrenciesDatabase => currenciesDatabase;
        static Currency[] _currencies;
        public static Currency[] Currencies => _currencies;
        static Dictionary<CurrencyType, int> _dict;
        static bool _isInitialised;
        static event SimpleCallback OnOnModuleInitialised;

        public virtual void Initialise()
        {
            if (_isInitialised) return;
            _isInitialised = true;
            
            _currenciesController = this;
            currenciesDatabase.Initialise();
            _currencies = currenciesDatabase.Currencies;

            _dict = new Dictionary<CurrencyType, int>();
            for (var i = 0; i < _currencies.Length; i++)
            {
                if (!_dict.ContainsKey(_currencies[i].CurrencyType))
                    _dict.Add(_currencies[i].CurrencyType, i);
                else
                    Debug.LogError($"[Currency]: Currency with type {_currencies[i].CurrencyType} exist!");

                var save = SaveController.GetSaveObject<Currency.Save>("currency" + ":" + (int) _currencies[i].CurrencyType);
                _currencies[i].SetSave(save);
            }

            OnOnModuleInitialised?.Invoke();
            OnOnModuleInitialised = null;
        }

        public static bool Has(CurrencyType currencyType, int amount) =>
            _currencies[_dict[currencyType]].Amount >= amount;

        public static int Get(CurrencyType currencyType) => _currencies[_dict[currencyType]].Amount;

        public static Currency GetCurrency(CurrencyType currencyType) => _currencies[_dict[currencyType]];

        public static void Set(CurrencyType currencyType, int amount)
        {
            var currency = _currencies[_dict[currencyType]];
            currency.Amount = amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(0);
        }

        public static void Add(CurrencyType currencyType, int amount)
        {
            if (amount == 0) return;

            var currency = _currencies[_dict[currencyType]];
            currency.Amount += amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(amount);
        }

        public static void Substract(CurrencyType currencyType, int amount)
        {
            var currency = _currencies[_dict[currencyType]];
            currency.Amount -= amount;
            SaveController.MarkAsSaveIsRequired();
            currency.InvokeChangeEvent(-amount);
        }

        public static void SubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            foreach (var currency in _currencies)
                currency.OnCurrencyChanged += currencyChange;
        }

        public static void UnsubscribeGlobalCallback(CurrencyChangeDelegate currencyChange)
        {
            foreach (var t in _currencies)
                t.OnCurrencyChanged -= currencyChange;
        }

        public static void InvokeOrSubscribe(SimpleCallback callback)
        {
            if (_isInitialised)
                callback?.Invoke();
            else
                OnOnModuleInitialised += callback;
        }
    }

    public delegate void CurrencyChangeDelegate(Currency currency, int difference);
}