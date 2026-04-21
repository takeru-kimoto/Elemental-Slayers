using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    [Header("カードデータ")]
    public List<CardData> drawPile = new List<CardData>();    
    public List<CardData> discardPile = new List<CardData>(); 
    private List<CardMovement> handCards = new List<CardMovement>(); 

    [Header("UI設定")]
    public GameObject cardPrefab;
    public Transform handArea;
    public Button endTurnButton;    
    public Button mulliganButton;   

    [Header("状態管理")]
    public bool isMulliganPhase = false; 
    public bool isEnemyTurn = false; 

    void Start() { Shuffle(drawPile); StartFirstTurn(); }

    void StartFirstTurn()
    {
        PrepareMulligan();
    }

    public void EndTurn()
    {
        if (isEnemyTurn || isMulliganPhase) return; 

        // ★修正ポイント：呪いカード以外を捨てる
        List<CardMovement> keptCards = new List<CardMovement>();

        foreach (var card in handCards) {
            if (card != null) {
                if (card.GetComponent<CardDisplay>().cardData.isUnusable) {
                    // 呪いカードは捨てずに残す
                    keptCards.Add(card);
                } else {
                    // 通常のカードは捨て札へ送って破棄
                    discardPile.Add(card.GetComponent<CardDisplay>().cardData);
                    Destroy(card.gameObject);
                }
            }
        }

        // 手札リストを残ったカード（呪い）だけに更新
        handCards = keptCards;

        // 次の手札補充（ドロー）とマリガンへ
        PrepareMulligan();
    }

    void PrepareMulligan()
    {
        isMulliganPhase = true;
        isEnemyTurn = false;

        // ★修正ポイント：手札が「合計5枚」になるまで不足分をドローする
        // 例：呪いが1枚残っていれば、引くのは4枚
        int currentCount = handCards.Count;
        int cardsToDraw = 5 - currentCount;

        for (int i = 0; i < cardsToDraw; i++) {
            DrawCard();
        }

        if(mulliganButton != null) mulliganButton.gameObject.SetActive(true);
        if(endTurnButton != null) endTurnButton.interactable = false;

        // 手札の並びを綺麗に整える
        UpdateHandLayout();
    }

    public void ConfirmMulligan()
    {
        List<CardMovement> toRemove = new List<CardMovement>();
        foreach (var card in handCards) {
            if (card != null && card.isMulliganSelected) toRemove.Add(card);
        }
        
        foreach (var card in toRemove) {
            discardPile.Add(card.GetComponent<CardDisplay>().cardData);
            handCards.Remove(card); 
            Destroy(card.gameObject); 
            DrawCard(); // 交換した分を補充
        }

        isMulliganPhase = false;
        if(mulliganButton != null) mulliganButton.gameObject.SetActive(false);
        
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        isEnemyTurn = true;
        yield return new WaitForSeconds(0.5f);

        EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
        if (enemy != null && enemy.gameObject.activeSelf) {
            enemy.ResetBlock(); 
            enemy.ExecuteAction();
        }

        yield return new WaitForSeconds(1.0f);
        StartPlayerActionPhase();
    }

    void StartPlayerActionPhase()
    {
        isEnemyTurn = false;
        isMulliganPhase = false;

        ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
        if (mm != null) mm.ResetMana();
        PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm != null) pm.ResetBlock();

        if(endTurnButton != null) endTurnButton.interactable = true;
        UpdateHandLayout();
    }

    public void DrawCard()
    {
        if (drawPile.Count == 0) {
            if (discardPile.Count == 0) return; 
            drawPile.AddRange(discardPile); discardPile.Clear(); Shuffle(drawPile);
        }
        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        GenerateCardToHand(drawnCard);
    }

    public void GenerateCardToHand(CardData data)
    {
        GameObject newCard = Instantiate(cardPrefab, handArea);
        newCard.transform.localPosition = new Vector3(0, 500f, 0f); 
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.Setup(data);
        handCards.Add(newCard.GetComponent<CardMovement>());
        // レイアウト更新はDrawCardを全て終えた後に行うためここでは呼ばない
    }

    void Shuffle(List<CardData> list) { 
        for (int i = list.Count - 1; i > 0; i--) { 
            int j = Random.Range(0, i + 1); 
            var temp = list[i]; list[i] = list[j]; list[j] = temp; 
        } 
    }

    public void UpdateHandLayout()
    {
        handCards.RemoveAll(card => card == null || !card.gameObject.activeSelf);
        float xStep = 120f; float angleStep = 5f; float yCurve = 15f;
        for (int i = 0; i < handCards.Count; i++) {
            float normalizedIndex = i - (handCards.Count - 1) * 0.5f;
            handCards[i].targetPosition = new Vector3(normalizedIndex * xStep, -Mathf.Pow(normalizedIndex, 2) * yCurve, 0);
            handCards[i].targetRotation = Quaternion.Euler(0, 0, -normalizedIndex * angleStep);
        }
    }
}