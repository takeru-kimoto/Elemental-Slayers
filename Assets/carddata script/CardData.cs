using UnityEngine;

public enum CardType { Attack, Skill }

// ★エラーで怒られていた「Uncommon」「Special」を追加しました
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Special } 

// ★エラーで怒られていた「Ice」「Thunder」「Rock」を追加しました
public enum ElementType { None, Normal, Fire, Water, Wood, Light, Dark, Ice, Thunder, Rock } 

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("基本設定")]
    public string cardName;
    public int cost;
    public CardType cardType;
    public int damage;
    public int block;
    
    [Header("レアリティ・属性")]
    public Rarity rarity;
    public ElementType elementType;

    [Header("見た目")]
    [TextArea]
    public string description;
    public Sprite cardImage;

    [Header("特殊設定")]
    public bool isUnusable; // チェックを入れると手札で使えなくなります
}