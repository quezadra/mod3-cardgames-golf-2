using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//enum, defines variable type with prenamed values
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}


public class CardProspector : Card //extension of card class
{
    [Header("Set Dynamically: CardProspector")]
    public eCardState state = eCardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();//stores which cards keep this one face down
    public int layoutID;//matches this card to to the XML tableau if it's a tableau card
    public slotDef slotDef;
}
