using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using UnityEngine.UI;

public enum CardType { Question, Answer}
public enum Action { Add = 0, Subtract = 1 }//, Multiply = 2, Divide = 3}

public class Card : MonoBehaviour {

    #region Properties
    public Texture backSprite;
    //public Texture frontSprite;
    [SerializeField] private Text cardText;
    private int result;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private GameObject _v;
    [SerializeField] private Button _button;
    [SerializeField] private RawImage _cardImage;
    [SerializeField] private Texture _backTexture;
    [SerializeField] private Texture _frontTexture;
    private CardType cardType;
    [HideInInspector] public RectTransform rTransform;
    public bool isShowing;
    public bool foundMatch;
    public bool openedOnce;
    
    
    
    #endregion      

    #region MonoBehaviour Callbacks
    void Awake()
    {
        isShowing = false;
        Flip(false);
        foundMatch = false;
        openedOnce = false;
        
        rTransform = GetComponent<RectTransform>();             
    }

    void Start()
    {
        _button.onClick.AddListener(
            () =>
            {
                if (!GameManager.S.isAnimating && !GameManager.S.areTwoCardsOpen)
                    StartCoroutine(GameManager.S.Flip(this));
            });
    }
    #endregion

    #region Methods
    public override bool Equals(object other)
    {
        Card a = other as Card;
        return this.result == a.GetResult();       
    }       

    public CardType GetCardType()
    {
        return cardType;
    }

    public void SetCardType(CardType ct)
    {
        cardType = ct;
    }

    public int GetResult()
    {
        return result;
    }

    public void SetResult(int r)
    {
        result = r;
    }

    public string GetCardText()
    {
        return cardText.text;
    }

    public void SetCardText(string s)
    {
        cardText.text = s;
    }

    public void CreateQuestion(Action act)
    {
        int a = 0;
        int b = 0;
        switch (act)
        {
            case Action.Add:  
                a = UnityEngine.Random.Range(0, result);
                b = result - a;
                SetCardText(string.Format("{0} + {1}", a, b));
                break;
            case Action.Subtract:
                a = UnityEngine.Random.Range(result, result*2);
                b = a - result;
                SetCardText(string.Format("{0} - {1}", a, b));
                break;
            /*case Action.Multiply:
                if (result == 0 || result == 1)
                    goto Add;
                List<int> resultMultipliers = new List<int>();                
                for (int i = 2; i < result; i++)
                {                    
                    if(i == 2 && result % 2 == 0)
                    {
                        resultMultipliers.Add(i);
                        Debug.Log("multiplier i: " + i + "result: " + result);
                    }
                    else if (result % i == 0)
                    {
                        resultMultipliers.Add(i);
                        Debug.Log("multiplier i: " + i + "result: " + result);
                    }
                        
                }
                if (resultMultipliers.Count > 0)
                {
                    a = UnityEngine.Random.Range(1, resultMultipliers[resultMultipliers.Count - 1]);
                    b = result / a;
                    SetCardText(string.Format("{0} X {1}", a, b));
                }
                
                               
                              
                break;
            case Action.Divide:
                b = UnityEngine.Random.Range(1, 15);
                a = result * b;                
                SetCardText(string.Format("{0} / {1}", a, b));
                break;
            default:
                break;*/
        }
    }

    public bool GetIsShowing()
    {
        return isShowing;
    }

    public void SetIsShowing(bool show) 
    {
        isShowing = show;
    }

    public void Flip(bool show) 
    {
        if (show)
        {
            if (!openedOnce)
                openedOnce = true;
            _cardImage.texture = _frontTexture;
            cardText.gameObject.SetActive(true);
            isShowing = true;            
        }
        else
        {
            _cardImage.texture = _backTexture;
            cardText.gameObject.SetActive(false);
            isShowing = false;
        }
    }
    #endregion

}
