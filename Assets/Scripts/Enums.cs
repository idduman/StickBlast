using UnityEngine;

namespace GarawellCase
{
    public enum GridElementType
    {
        Dot,
        HorizontalLine,
        VerticalLine,
        Fill
    }

    public enum GridElementState
    {
        Empty,
        Highlight,
        Filled
    }

    public enum AudioFxType
    {
        ItemPickup,
        Click,
        Fill,
        MultiFill,
        Completion,
        Pop,
        Wrong,
        GameOver,
        Win,
    }

    public enum MedalType
    {
        None,
        Bronze,
        Silver,
        Gold,
    }
    
    public enum VibrationType
    {
        None,
        Light,
        Strong,
        Pulse,
    }
}
