public struct KeyBindingChangedEvent
{
    public InputIndex slotIndex;

    public KeyBindingChangedEvent(InputIndex index)
    {
        slotIndex = index;
    }
}