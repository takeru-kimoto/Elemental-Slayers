using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private bool isHovering = false;
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public bool isMulliganSelected = false;

    void Update()
    {
        if (!isDragging) {
            Vector3 targetP = targetPosition; Quaternion targetR = targetRotation; Vector3 targetS = Vector3.one;
            if (isHovering) { targetP += new Vector3(0, 30f, 0); targetR = Quaternion.identity; targetS = new Vector3(1.2f, 1.2f, 1.2f); }
            if (isMulliganSelected) { targetP += new Vector3(0, 50f, 0); targetS = new Vector3(1.1f, 1.1f, 1.1f); }
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetP, Time.deltaTime * 15f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetR, Time.deltaTime * 15f);
            transform.localScale = Vector3.Lerp(transform.localScale, targetS, Time.deltaTime * 15f);
        }
    }

    // クリック（マリガンの選択）
    public void OnPointerClick(PointerEventData eventData)
    {
        DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
        CardDisplay cd = GetComponent<CardDisplay>();

        // マリガンフェーズ中でない、または「使用不可(呪い)」なら選択できない
        if (dm == null || !dm.isMulliganPhase || cd.cardData.isUnusable) return;

        isMulliganSelected = !isMulliganSelected;
        GetComponent<Image>().color = isMulliganSelected ? Color.gray : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData) { if(!isDragging) { isHovering = true; transform.SetAsLastSibling(); } }
    public void OnPointerExit(PointerEventData eventData) { isHovering = false; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
        CardDisplay cd = GetComponent<CardDisplay>();

        // ドラッグ不可条件：マリガン中、敵のターン中、または「使用不可(呪い)」カード
        if (dm == null || dm.isMulliganPhase || dm.isEnemyTurn || cd.cardData.isUnusable) return;

        isDragging = true; isHovering = false;
        originalPosition = targetPosition; originalRotation = targetRotation;
        transform.SetAsLastSibling(); transform.localRotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData) { if (isDragging) transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        // フィールドに放り投げた判定
        if (transform.localPosition.y > 150f) {
            CardDisplay display = GetComponent<CardDisplay>();
            ManaManager manaManager = Object.FindFirstObjectByType<ManaManager>();
            
            // 使用不可チェック（念のためここでもチェック）
            if (display.cardData.isUnusable) {
                ResetPosition();
                return;
            }

            if (display != null && manaManager != null && manaManager.TryConsumeMana(display.cardData.cost)) {
                if (display.cardData.cardType == CardType.Attack) {
                    EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
                    if (enemy != null) enemy.TakeDamage(display.cardData.damage);
                }
                else if (display.cardData.cardType == CardType.Skill) {
                    PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                    if (player != null) player.AddBlock(display.cardData.block);
                }
                Object.FindFirstObjectByType<DeckManager>().discardPile.Add(display.cardData);
                Destroy(gameObject); 
                Object.FindFirstObjectByType<DeckManager>().UpdateHandLayout();
            } else { ResetPosition(); }
        } else { ResetPosition(); }
    }

    void ResetPosition() {
        targetPosition = originalPosition;
        targetRotation = originalRotation;
    }
}