using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("プレイヤーステータス")]
    public int maxHp = 50;
    public int currentHp;
    public int currentBlock;

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI blockText;

    void Start()
    {
        currentHp = maxHp;
        currentBlock = 0;
        UpdateUI();
    }

    public void AddBlock(int amount)
    {
        currentBlock += amount;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 1. まずブロックで受ける
        if (currentBlock > 0)
        {
            if (currentBlock >= damage)
            {
                currentBlock -= damage;
                damage = 0;
            }
            else
            {
                damage -= currentBlock;
                currentBlock = 0;
            }
        }

        // 2. 残ったダメージをHPから引く
        currentHp -= damage;
        
        // ★修正ポイント：HPが0以下になったら敗北（Game Over）処理を呼ぶ
        if (currentHp <= 0) 
        {
            currentHp = 0;
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.LoseGame();
        }

        UpdateUI();
    }

    // ターン開始時にブロックをリセットする
    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hpText != null) hpText.text = "Player HP: " + currentHp + " / " + maxHp;
        
        if (blockText != null)
        {
            blockText.text = "Block: " + currentBlock;
            // ブロックがない時はテキストを隠すと見やすい
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}