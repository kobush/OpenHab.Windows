namespace OpenHab.Client
{
    public enum ItemType
    {
        Unknown,
        /// <summary>
        /// Item to nest other items / collect them in groups
        /// </summary>
        GroupItem,
        /// <summary>
        /// Item storing status of e.g. door/window contacts
        /// </summary>
        ContactItem,
        /// <summary>
        /// Typically used for lights (on/off)
        /// </summary>
        SwitchItem,
        /// <summary>
        /// Item carrying a percentage value for dimmers
        /// </summary>
        DimmerItem,
        /// <summary>
        /// Typically used for blinds
        /// </summary>
        RollershutterItem,
        /// <summary>
        /// Color information (RGB)
        /// </summary>
        ColorItem,
        /// <summary>
        /// Stores values in number format
        /// </summary>
        NumberItem,
        /// <summary>
        /// Stores texts
        /// </summary>
        StringItem,
        /// <summary>
        /// Stores date and time 
        /// </summary>
        DateTimeItem,
    }
}