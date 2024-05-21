namespace Watermelon.SquadShooter
{
    // note add here only reusable items
    // to drop a custom item please use DropableItemSettings.Drop(IDropableItem item...)
    
    public enum DropableItemType
    {
        None = -1,

        Currency = 0,
        WeaponCard = 1,

        Heal = 2,
        AtkSpeedBooster=3,
        MultishotBooster = 4,
        MoveSpeedBooster = 5
    }
}