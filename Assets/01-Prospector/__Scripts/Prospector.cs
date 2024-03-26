using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
    public TextAsset layoutXML;



    [Header("Set Dynamically")]
	public Deck	deck;
    public Layout layout;

    void Awake(){
		S = this;
	}

	void Start() 
	{
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>(); // Get the Layout component
        layout.ReadLayout(layoutXML.text);
    }

}
