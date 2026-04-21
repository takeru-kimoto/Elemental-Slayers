using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public EnemyData enemyData;
    public int currentHP;
    public int currentBlock;

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public Image enemyImage;
    public TextMeshProUGUI blockText;

    void Start() { SetupEnemy(); }

    public void SetupEnemy()
    {
        if (enemyData != null)
        {
            currentHP = enemyData.maxHP;
            currentBlock = 0;
            if (enemyImage != null) enemyImage.sprite = enemyData.enemyImage;
            UpdateUI();
        }
    }

    public void ExecuteAction()
    {
        if (enemyData == null || enemyData.actionList.Count == 0) return;

        int randomIndex = Random.Range(0, enemyData.actionList.Count);
        EnemyAction chosenAction = enemyData.actionList[randomIndex];

        Debug.Log("<color=red>敵の行動: " + chosenAction.actionName + "</color>");

        switch (chosenAction.actionType)
        {
            case EnemyActionType.Attack:
                PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                if (player != null) player.TakeDamage(chosenAction.value);
                break;

            case EnemyActionType.Defend:
                currentBlock += chosenAction.value;
                break;

            case EnemyActionType.AddStatusCard:
                DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
                if (dm != null && chosenAction.statusCard != null)
                {
                    dm.GenerateCardToHand(chosenAction.statusCard);
                }
                break;
        }
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }
        currentHP -= damage;
        if (currentHP <= 0) 
        { 
            currentHP = 0; 
            // 勝利処理を呼び出す
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.WinGame();
            gameObject.SetActive(false); 
        }
        UpdateUI();
    }

    public void ResetBlock() { currentBlock = 0; UpdateUI(); }

    private void UpdateUI()
    {
        if (hpText != null && enemyData != null)
            hpText.text = "Enemy HP: " + currentHP + " / " + enemyData.maxHP;
        if (blockText != null) {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}