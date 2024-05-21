using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class CurrencyDropItem : IDropItem
    {
        public DropableItemType DropItemType => DropableItemType.Currency;

        Currency[] availableCurrencies;

        public GameObject GetDropObject(DropData dropData)
        {
            var currencyType = dropData.currencyType;
            for(var i = 0; i < availableCurrencies.Length; i++)
            {
                if(availableCurrencies[i].CurrencyType == currencyType)
                {
                    return availableCurrencies[i].Pool.Get();
                }
            }

            return null;
        }

        public void SetCurrencies(Currency[] currencies)
        {
            availableCurrencies = currencies;
        }

        public void Initialise()
        {

        }

        public void Unload()
        {

        }
    }
}
