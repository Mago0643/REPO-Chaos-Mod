using UnityEngine;

namespace Lego.ChaosMod.Backend;
internal class EventListData
{
    /// <summary>
    /// The target event of this data.
    /// </summary>
    internal ChaosEvent evt;
    /// <summary>
    /// The text of event name.
    /// </summary>
    internal TextLerp evtNameText;
    /// <summary>
    /// The text that shows time. can be null.
    /// </summary>
    internal EventTimerBar evtTimeText;

    internal EventListData(ChaosEvent evt, TextLerp evtNameText)
    {
        this.evt = evt;
        this.evtNameText = evtNameText;
    }
    
    internal EventListData(ChaosEvent evt, TextLerp evtNameText, EventTimerBar evtTimeText)
    {
        this.evt = evt;
        this.evtNameText = evtNameText;
        this.evtTimeText = evtTimeText;
    }

    /// <summary>
    /// Safely disposes all text.
    /// <para>
    /// <c>evtNameText</c>, <c>evtTimeText</c> cannot be used after calling this.
    /// </para>
    /// </summary>
    internal void DisposeTexts()
    {
        if (evtNameText == null) return;
        
        Object.Destroy(evtNameText.gameObject);
        // evtTimeText's parent is evtNameText, so we don't need to destroy that one.
    }
}