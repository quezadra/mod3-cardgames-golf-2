using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
namespace Golf
{


    public class Prospector : MonoBehaviour
    {
        static public Prospector S;

        [Header("Set in Inspector")]
        public TextAsset deckXML;
        public TextAsset layoutXML;
        public float xOffset = 3;
        public float yOffset = -2.5f;
        public Vector3 layoutCenter;
        public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
        public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
        public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
        public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
        public float reloadDelay = 2f;
        public Text gameOverText, roundResultText, highScoreText;

        [Header("Set Dynamically")]
        public Deck deck;
        public Layout layout;
        public List<CardProspector> drawPile = new List<CardProspector>();
        public Transform layoutAnchor;
        public CardProspector target;
        public List<CardProspector> tableau = new List<CardProspector>();
        public List<CardProspector> discardPile = new List<CardProspector>();
        public FloatingScore fsRun;

        void Awake()
        {
            S = this;
            SetUpUITexts();
        }

        void SetUpUITexts()
        {
            GameObject go = GameObject.Find("HighScore");
            if (go != null)
            {
                highScoreText = go.GetComponent<Text>();
            }
            if (highScoreText != null)
            {
                int highScore = ScoreManager.HIGH_SCORE;
                string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
                highScoreText.text = hScore;
            }

            go = GameObject.Find("GameOver");
            if (go != null)
            {
                gameOverText = go.GetComponent<Text>();
            }

            go = GameObject.Find("RoundResult");
            if (go != null)
            {
                roundResultText = go.GetComponent<Text>();
            }

            ShowResultsUI(false);
        }

        void ShowResultsUI(bool show)
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(show);
            }
            if (roundResultText != null)
            {
                roundResultText.gameObject.SetActive(show);
            }
        }

        void Start()
        {
            Scoreboard.S.score = ScoreManager.SCORE;

            deck = GetComponent<Deck>();
            deck.InitDeck(deckXML.text);
            Deck.Shuffle(ref deck.cards);

            layout = GetComponent<Layout>();
            layout.ReadLayout(layoutXML.text);

            drawPile = ConvertListCardsToListCardProspectors(deck.cards);
            LayoutGame();
        }

        List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
        {
            List<CardProspector> lCP = new List<CardProspector>();
            foreach (Card tCD in lCD)
            {
                CardProspector tCP = tCD as CardProspector;
                lCP.Add(tCP);
            }
            return lCP;
        }

        CardProspector Draw()
        {
            if (drawPile.Count == 0) return null;
            CardProspector cd = drawPile[0];
            drawPile.RemoveAt(0);
            return cd;
        }

        void LayoutGame()
        {
            if (layoutAnchor == null)
            {
                GameObject tGO = new GameObject("_LayoutAnchor");
                layoutAnchor = tGO.transform;
                layoutAnchor.transform.position = layoutCenter;
            }
            foreach (SlotDef tSD in layout.slotDefs)
            {
                CardProspector cp = Draw();
                if (cp != null)
                {
                    cp.faceUp = tSD.faceUp;
                    cp.transform.parent = layoutAnchor;
                    cp.transform.localPosition = new Vector3(
                        layout.multiplier.x * tSD.x,
                        layout.multiplier.y * tSD.y,
                        -tSD.layerID);
                    cp.layoutID = tSD.id;
                    cp.slotDef = tSD;
                    cp.state = eCardState.tableau;
                    cp.SetSortingLayerName(tSD.layerName);
                    tableau.Add(cp);
                }
            }
            foreach (CardProspector tCP in tableau)
            {
                foreach (int hid in tCP.slotDef.hiddenBy)
                {
                    CardProspector cover = FindCardByLayoutID(hid);
                    if (cover != null)
                    {
                        tCP.hiddenBy.Add(cover);
                    }
                }
            }
            MoveToTarget(Draw());
            UpdateDrawPile();
        }

        CardProspector FindCardByLayoutID(int layoutID)
        {
            foreach (CardProspector tCP in tableau)
            {
                if (tCP.layoutID == layoutID)
                {
                    return tCP;
                }
            }
            return null;
        }

        void SetTableauFaces()
        {
            foreach (CardProspector cd in tableau)
            {
                bool faceUp = true;
                foreach (CardProspector cover in cd.hiddenBy)
                {
                    if (cover.state == eCardState.tableau)
                    {
                        faceUp = false;
                        break;
                    }
                }
                cd.faceUp = faceUp;
            }
        }

        void MoveToDiscard(CardProspector cd)
        {
            cd.state = eCardState.discard;
            discardPile.Add(cd);
            cd.transform.parent = layoutAnchor;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * layout.discardPile.x,
                layout.multiplier.y * layout.discardPile.y,
                -layout.discardPile.layerID + 0.5f);
            cd.faceUp = true;
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(-100 + discardPile.Count);
        }

        void MoveToTarget(CardProspector cd)
        {
            if (target != null) MoveToDiscard(target);
            target = cd;
            cd.state = eCardState.target;
            cd.transform.parent = layoutAnchor;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * layout.discardPile.x,
                layout.multiplier.y * layout.discardPile.y,
                -layout.discardPile.layerID);
            cd.faceUp = true;
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(0);
        }

        void UpdateDrawPile()
        {
            for (int i = 0; i < drawPile.Count; i++)
            {
                CardProspector cd = drawPile[i];
                cd.transform.parent = layoutAnchor;
                Vector2 dpStagger = layout.drawPile.stagger;
                cd.transform.localPosition = new Vector3(
                    layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                    layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                    -layout.drawPile.layerID + 0.1f * i);
                cd.faceUp = false;
                cd.state = eCardState.drawpile;
                cd.SetSortingLayerName(layout.drawPile.layerName);
                cd.SetSortOrder(-10 * i);
            }
        }

        public void CardClicked(CardProspector cd)
        {
            switch (cd.state)
            {
                case eCardState.target:
                    break;
                case eCardState.drawpile:
                    MoveToDiscard(target);
                    MoveToTarget(Draw());
                    UpdateDrawPile();
                    ScoreManager.EVENT(eScoreEvent.draw);
                    FloatingScoreHandler(eScoreEvent.draw);
                    break;
                case eCardState.tableau:
                    bool validMatch = true;
                    if (!cd.faceUp || !AdjacentRank(cd, target))
                    {
                        validMatch = false;
                    }
                    if (!validMatch) return;
                    tableau.Remove(cd);
                    MoveToTarget(cd);
                    SetTableauFaces();
                    ScoreManager.EVENT(eScoreEvent.mine);
                    FloatingScoreHandler(eScoreEvent.mine);
                    break;
            }
            CheckForGameOver();
        }

        void CheckForGameOver()
        {
            if (tableau.Count == 0)
            {
                GameOver(true);
                return;
            }
            if (drawPile.Count == 0)
            {
                foreach (CardProspector cd in tableau)
                {
                    if (AdjacentRank(cd, target))
                    {
                        return;
                    }
                }
                GameOver(false);
            }
        }

        void GameOver(bool won)
        {
            int score = ScoreManager.SCORE;
            if (fsRun != null) score += fsRun.score;

            if (gameOverText != null)
            {
                gameOverText.text = won ? "Round Over" : "Game Over";
            }

            if (roundResultText != null)
            {
                if (won)
                {
                    roundResultText.text = "You won this round!\nRound Score: " + score;
                }
                else
                {
                    roundResultText.text = ScoreManager.HIGH_SCORE <= score ?
                        "You got the high score!\nHigh score: " + score :
                        "Your final score was: " + score;
                }
            }

            ShowResultsUI(true);
            ScoreManager.EVENT(won ? eScoreEvent.gameWin : eScoreEvent.gameLoss);
            FloatingScoreHandler(won ? eScoreEvent.gameWin : eScoreEvent.gameLoss);

            Invoke("ReloadLevel", reloadDelay);
        }

        void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool AdjacentRank(CardProspector c0, CardProspector c1)
        {
            if (!c0.faceUp || !c1.faceUp) return false;

            int diff = Mathf.Abs(c0.rank - c1.rank);
            return diff == 1 || (c0.rank == 1 && c1.rank == 13) || (c0.rank == 13 && c1.rank == 1);
        }

        void FloatingScoreHandler(eScoreEvent evt)
        {
            switch (evt)
            {
                case eScoreEvent.draw:
                case eScoreEvent.gameWin:
                case eScoreEvent.gameLoss:
                    if (fsRun != null)
                    {
                        List<Vector2> fsPts = new List<Vector2>();
                        fsPts.Add(fsPosRun);
                        fsPts.Add(fsPosMid2);
                        fsPts.Add(fsPosEnd);
                        fsRun.reportFinishTo = Scoreboard.S.gameObject;
                        fsRun.Init(fsPts, 0, 1);
                        fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                        fsRun = null;
                    }
                    break;
                case eScoreEvent.mine:
                    Vector2 p0 = Input.mousePosition;
                    p0.x /= Screen.width;
                    p0.y /= Screen.height;
                    List<Vector2> mineFsPts = new List<Vector2>(); // Rename fsPts to mineFsPts
                    mineFsPts.Add(p0);
                    mineFsPts.Add(fsPosMid);
                    mineFsPts.Add(fsPosRun);
                    FloatingScore fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, mineFsPts); // Use mineFsPts
                    fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                    if (fsRun == null)
                    {
                        fsRun = fs;
                        fsRun.reportFinishTo = null;
                    }
                    else
                    {
                        fs.reportFinishTo = fsRun.gameObject;
                    }
                    break;
            }
        }

    }
}

