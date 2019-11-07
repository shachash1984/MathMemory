using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    #region Properties
    public bool gameOver;
    public bool gamePaused;
    public int pairsFound;
    public float timeLeft;
    public int score;
    public GameObject endGamePanel;
    [SerializeField] private Card CardPrefab;
    [SerializeField] private List<Texture> cardFaces;
    /*[SerializeField]*/
    private List<Vector3> cardPositions = new List<Vector3>();
    [SerializeField] private GameObject panel;
    [SerializeField] private Texture BGSprite;
    [SerializeField] private Texture startSprite;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private List<Card> cards = new List<Card>();
    private int cardAmt = 12;
    [SerializeField] private Text timeText;
    [SerializeField] private Text scoreText;
    private int _score;
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip successSound;
    public AudioClip winMusic;
    public AudioClip failSound;
    [HideInInspector] public bool isAnimating = false;
    [HideInInspector] public bool areTwoCardsOpen = false;
    #endregion

    #region Constructor (Singleton)
    static public GameManager S;
    
    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        if (S != null)
            Destroy(this.gameObject);
        else
            S = this;
        LoadStartScreen();  
    }    

    void Update()
    {        
        if (timeLeft <= 0f && !gameOver && !gamePaused)
            timeLeft = 0;
        else if (!gameOver && !gamePaused)
            timeLeft -= Time.deltaTime;

        try
        {
            if (timeLeft < 100)
                timeText.text = "Time Left: " + timeLeft.ToString().Remove(4);
            else
                timeText.text = "Time Left: " + timeLeft.ToString().Remove(3);
        }
        catch (ArgumentOutOfRangeException)
        {
            timeText.text = "Time Left: " + timeLeft.ToString();
        }        

        if (!gameOver &&(pairsFound >= 6 || timeLeft == 0f))
            StartCoroutine(EndGame());

        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();

    }
    #endregion

    #region Methods
    void LoadStartScreen()
    {
        gameOver = true;        
        endGamePanel.SetActive(true);
        //endGamePanel.GetComponentInChildren<Text>().text = "";
        
    }

    private int GetScore()
    {
        return _score;
    }

    private void AddScore(int scoreToAdd)
    {
        _score += scoreToAdd;
        if (_score < 0)
            _score = 0;
    }

    private void SetScoreText()
    {
        scoreText.text = string.Format("Score: {0}", GetScore());
    }

    public void Init()
    {
        Card[] oldCards = FindObjectsOfType<Card>();
        for (int i = 0; i < oldCards.Length; i++)
        {
            Destroy(oldCards[i].gameObject);
        }
        _score = 0;
        SetScoreText();
        StartCoroutine(SetCards());
    }

    public IEnumerator SetCards()
    {
        Debug.Log("Init");
        ResetTimer();
        foreach (Card c in cards)
        {            
            Destroy(c.gameObject);
        }
        cards.Clear();
        cardPositions.Clear();
        gameOver = false;
        pausePanel.SetActive(false);
        
        endGamePanel.SetActive(false);
        for (int i = 0; i < cardAmt; i++) //restting position list
        {
            if (i < 3)
                cardPositions.Add(new Vector3((i * 130) - 160, 160));
            else if (i < 6)            
                cardPositions.Add(new Vector3(((i % 3) * 130) - 160, 60f));         
            else if (i < 9)            
                cardPositions.Add(new Vector3(((i % 3) * 130) - 160, -40f));            
            else
                cardPositions.Add(new Vector3(((i % 3) * 130) - 160, -140f));

        }
        //Debug.Log("cardPositions.Count: " + cardPositions.Count);
        //Sequence seq = DOTween.Sequence();
        for (int i = 0; i < cardAmt; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Card tempCard = Instantiate(CardPrefab, new Vector3(0,-340), Quaternion.identity, panel.transform) as Card;
            tempCard.GetComponent<RawImage>().texture = tempCard.backSprite;
            if (i % 2 == 0)
            {
                tempCard.SetCardType(CardType.Answer);
                tempCard.SetResult(UnityEngine.Random.Range(0, 100));
                tempCard.SetCardText(tempCard.GetResult().ToString());
            }
            else
            {
                tempCard.SetCardType(CardType.Question);
                tempCard.SetResult(cards[i - 1].GetResult());
                int rand = UnityEngine.Random.Range(0, 2);
                Action a = (Action)rand;
               // Debug.Log(a.ToString());
                tempCard.CreateQuestion(a);
            }
            //int index = UnityEngine.Random.Range(0, cardPositions.Count);
            //Debug.Log("index: " + index);
            /*seq.Append(*/
            tempCard.rTransform.DOLocalMove(cardPositions[i], 0.75f);                       
            //cardPositions.RemoveAt(index);
            cards.Add(tempCard);     
            if(cards.Count == cardAmt)
            {
                StartCoroutine(ShuffleCards());
            }
        }
        
        //seq.Play<Sequence>();

            
    }

    private void ResetTimer()
    {
        timeLeft = 120f;        
    }

    IEnumerator EndGame()
    {        
        score = pairsFound;        
        gameOver = true;
        pairsFound = 0;        
        yield return new WaitForSeconds(1f);
        //endGamePanel.GetComponent<RawImage>().texture = BGSprite;
        endGamePanel.SetActive(true);
        if (timeLeft > 0)
        {
            endGamePanel.GetComponentInChildren<Text>().text = string.Format("Good Job! \n You scored {0} points!", GetScore());
            audioSource.clip = winMusic;
            audioSource.Play();
        }

        else
        {
            endGamePanel.GetComponentInChildren<Text>().text = string.Format("Time's Up! \nYou scored {0} points", GetScore());
            audioSource.clip = failSound;
            audioSource.Play();
        }
            
        StopCoroutine(EndGame());
    }

    public bool CompareCards(Card c, Card d)
    {        
        if (c.isShowing && d.isShowing && (!c.foundMatch || !d.foundMatch))
        {
            c.foundMatch = c.Equals(d);
            d.foundMatch = c.foundMatch;
            if(c.foundMatch)
            {
                audioSource.clip = successSound;
                audioSource.Play();
            }
            return c.foundMatch;
        }                
        else
            return false;        
    }

    public IEnumerator Flip(Card c)
    {
        
        if (!gameOver)
        {
            isAnimating = true;
            List<Card> openCards = new List<Card>();
            int openCardsCounter = 0;
            c.SetIsShowing(true);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].isShowing && !cards[i].foundMatch)
                {
                    openCards.Add(cards[i]);
                    openCardsCounter++;
                    //Debug.Log("openCards.Count: " + openCards.Count);
                }
                    
            }
            if (openCardsCounter <= 2)
            {
                if (openCardsCounter == 2)
                    areTwoCardsOpen = true;
                c.GetComponent<Animator>().Play("Flip");
                audioSource.clip = flipSound;
                audioSource.Play();
                yield return new WaitForSeconds(0.33f);
                c.Flip(true);                     
                CheckOpenCards(openCards);
            }
            yield return new WaitUntil(() => c.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            isAnimating = false;
        }        
    }

    public void CheckOpenCards(List<Card> openCards)
    {
        if (!gameOver)
        {
            if(openCards.Count > 1)
            {
                if (CompareCards(openCards[0], openCards[1]))
                {
                    isAnimating = true;
                    foreach (Card t in openCards)
                    {
                        t.foundMatch = true;
                        t.GetComponent<Button>().enabled = false;
                        t.GetComponent<Animator>().Play("Correct");
                        AddScore(100);
                        SetScoreText();
                        cards.Remove(t);                        
                    }
                    pairsFound++;
                    isAnimating = false;
                    areTwoCardsOpen = false;
                    return;
                }
                else
                    StartCoroutine(HideUnMatchingCards(openCards.ToArray()));
            }
            
            /*List<Card> pair = new List<Card>();
            for (int i = 0; i < cards.Count; i++)
            {
                if (!cards[i].foundMatch && cards[i].isShowing)
                    pair.Add(cards[i]);

                if (pair.Count == 2)
                {
                    if (CompareCards(pair[0], pair[1]))
                    {
                        foreach (Card t in pair)
                        {
                            t.foundMatch = true;
                            t.GetComponent<Button>().enabled = false;
                            t.GetComponent<Animator>().Play("Correct");
                        }
                        pairsFound++;                        
                        return;
                    }
                    else
                        StartCoroutine(HideUnMatchingCards(pair.ToArray()));
                }
            }*/
        }        
    }

    public IEnumerator HideUnMatchingCards(params Card[] pair)
    {
        isAnimating = true;
        yield return new WaitForSeconds(1f);
        int counter = 0;
        for (int i = 0; i < pair.Length; i++)
        {
            pair[i].GetComponent<Animator>().Play("Flip");
            yield return new WaitForSeconds(0.33f);
            pair[i].SetIsShowing(false);
            pair[i].Flip(false);
            if (pair[i].openedOnce)
                counter++;
        }
        if (counter > 0)
            AddScore(-50 * counter);
        SetScoreText();
        yield return new WaitForSeconds(0.33f);
        isAnimating = false;
        areTwoCardsOpen = false;
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }
    
    public void Quit()
    {
        Application.Quit();
    }

    public IEnumerator ShuffleCards()
    {
        yield return new WaitForSeconds(1f);
        isAnimating = true;
        DOTween.CompleteAll();        
        List<Vector3> positions = new List<Vector3>(cardPositions.ToArray());
        List<Card> tempCards = new List<Card>(cards.ToArray());
        int rand = 0;
        foreach (Card c in tempCards)
        {
            rand = UnityEngine.Random.Range(0, positions.Count);
            c.rTransform.localPosition = positions[rand];
            positions.Remove(positions[rand]);
        }
        isAnimating = false;
    }
    #endregion
}
