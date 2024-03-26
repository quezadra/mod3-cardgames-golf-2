using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]//makes slotdefs visible in inspector
public class slotDef
{
    public float x;
    public float y;
    public bool faceup = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}
public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;//faster xml access
    public Vector2 multiplier; //tableau offset
    //slot def ref
    public List<slotDef> slotDefs;//row 0-3
    public slotDef drawPile;
    public slotDef discardPile;
    //all possible layer names
    public string[] sortingLayerNames = new string[] { "Row0", "Row1",
    "Row2", "Row3", "Discard", "Draw"};

    //reads LayountXML.xml when called
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];//xml set as shortcut to the xml

        //set card pacing by reading in multiplier
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        //read in slots
        slotDef tSD;
        //slotsX used as shortcut for all <slots>s
        PT_XMLHashList slotsX = xml["slot"];

        for(int i = 0; i< slotsX.Count; i++)
        {
            tSD = new slotDef();
            if (slotsX[i].HasAtt("type"))
            {
                //parse if slot has a type attribute
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                //if not, set type to slot so its a card in the rows
                tSD.type = "slot";
            }
            //parse attributes into numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            //converts number of layerID to a text LayerName
            tSD.layerName = sortingLayerNames[tSD.layerID];

            switch(tSD.type)
            {
                //pull attributes based on slot type
                case "slot":
                    tSD.faceup = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach(string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;

                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }

    }
}
