using _02_Scripts.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier : MonoBehaviour  //패시브로 인한 스텟 변동치
{
    private float moveSpeed; 
    private float maxHealth;
    private float maxMana;
    private float attackPower;  //공격력
    private float recoverManaPerSecond;  
    private float damageIncome = 1;   //받는 데미지
    private float damageOutcome = 1;  //주는 데미지
    private float healEfficiency = 1; //힐 증감량
    
    private bool onOffMoneyShot; private float moneyShotBonus; private float moneyShotBonusAtk = 0;
    private bool onOffSingleTone; private float singleToneBonus;   private float singleToneBonusAtk;
    private bool onOffHarmony; private float harmonyBonus; private bool harmonyed;
    private bool onOffElementalOverCharge; private bool elementalOverChargeCheck; private float elementalOverChargeBonus;
    private bool onOffStickyMagic; private float stickyMagicBonus = 1;
    private bool onOffCurseOfDestruction; private float curseOfDestructionBonus = 1;
    private bool onOffCurseOfShackle; private float curseOfShackleBonus = 1;
    private int onOffEbonyMagic; private float ebonyMagicBonus; private int ebonyMagicCount;
    private int onOffHeavyWallet; private float heavyWalletBonus; private int heavyWalletStack;



    void OnEnable()
    {
        EventBus.Subscribe(GameState.GameOver, ResetStatModifier);
        EventBus.Subscribe(GameState.Ending, ResetStatModifier);
    }
    
    void OnDisable()
    {
        EventBus.Unsubscribe("MonsterDead", EbonyMagicStack);
        EventBus.Unsubscribe("GetItem", GetItem);
    }

    public void ResetStatModifier(object param)
    {
        moveSpeed = 0;
        maxHealth = 0;
        maxMana = 0;
        attackPower = 0;
        recoverManaPerSecond = 0;
        damageIncome = 1;
        damageOutcome = 1;
        healEfficiency = 1;

        onOffMoneyShot = false; moneyShotBonus = 0; moneyShotBonusAtk = 0;

        onOffSingleTone = false; singleToneBonus = 0; singleToneBonusAtk = 0;

        onOffHarmony = false; harmonyBonus = 0; harmonyed = false;

        onOffElementalOverCharge = false; elementalOverChargeBonus = 0; elementalOverChargeCheck = false;

        onOffStickyMagic = false; stickyMagicBonus = 1;

        onOffCurseOfDestruction = false; curseOfDestructionBonus = 1;

        onOffCurseOfShackle = false; curseOfShackleBonus = 1;

        onOffEbonyMagic = 0; ebonyMagicBonus = 0; ebonyMagicCount = 0;
        EventBus.Unsubscribe("MonsterDead", EbonyMagicStack);
        
        onOffHeavyWallet =  0; heavyWalletBonus = 0; heavyWalletStack = 0;
        EventBus.Unsubscribe("GetItem", GetItem);
    }
    
    #region 프로퍼티, set 함수 모음집
    public float AttackPower
    {
        get { return attackPower + (onOffMoneyShot ? moneyShotBonusAtk : 0);}
        set {attackPower = value;}
    }

    public float MoveSpeed
    {
        get{  return moveSpeed; }
        set {moveSpeed = value;}
    }

    public float DamageIncome
    {
        get{  return damageIncome; }
        set {damageIncome = value;}
    }
    
    public float DamageOutcome
    {
        get{  return damageOutcome * 
                     (onOffSingleTone ? (singleToneBonusAtk) : 1) * (elementalOverChargeCheck ? elementalOverChargeBonus : 1);
            
        }
        set {damageOutcome = value;}
    }

    public float MaxHealth()
    {
        return maxHealth;
    }
    
    public float HealEfficiency()
    {
        return healEfficiency;
    }
    
    public void SetMoneyShot(float bonus)
    {
        onOffMoneyShot = true;
        moneyShotBonus =  bonus;
    }

    public void SetMoneyShotAtk(int money)
    {
        moneyShotBonusAtk = InventoryManager.Instance.Gold * moneyShotBonus;
    }
    
    public void SetSingleTone(float bonus)
    {
        onOffSingleTone = true;
        singleToneBonus =  bonus;
        SkillSlotChanged();
    }
    
    public void SetHarmony(float bonus)
    {
        onOffHarmony = true;
        harmonyBonus =  bonus;
        SkillSlotChanged();
    }
    
    public void SetElementalOverCharge(float bonus)
    {
        onOffElementalOverCharge = true;
        elementalOverChargeBonus =  bonus;
        SkillSlotChanged();
    }

    public float StickyMagicBonus()
    {
        if (!onOffStickyMagic)
            return 1f;
        return stickyMagicBonus;
    }
    
    public void SetStickyMagicBonus( float bonus)
    {
        onOffStickyMagic = true;
        stickyMagicBonus *= bonus;
    }
    
    public float CurseOfDestructionBonus()
    {
        if (!onOffCurseOfDestruction)
            return 1f;
        return curseOfDestructionBonus;
    }
    
    public float CurseOfShackleBonus()
    {
        if (!onOffCurseOfShackle)
            return 1f;
        return curseOfShackleBonus;
    }
    
    public void SetCurseOfDestruction(float bonus)
    {
        onOffCurseOfDestruction = true;
        curseOfDestructionBonus *= bonus;
    }
    
    public void SetCurseOfShackle(float bonus)
    {
        onOffCurseOfShackle = true;
        curseOfShackleBonus *= bonus;
    }

    public void SetPacifist(float healBonus, float damageOutcome)
    {
        healEfficiency *= healBonus;
        damageOutcome *= damageOutcome;
    }

    public void SetEbonyMagic(float bonus, float startAtk)
    {
        onOffEbonyMagic++;
        ebonyMagicBonus = bonus;
        attackPower += startAtk;
        if (attackPower <= -2)
        {
            attackPower = -2;
        }
        ebonyMagicCount =  0;
        EventBus.Subscribe("MonsterDead", EbonyMagicStack);
    }

    private void EbonyMagicStack(object value)
    {
        ebonyMagicCount++;
        attackPower += onOffEbonyMagic * ebonyMagicBonus;
        EventBus.Publish("StackablePassiveChanged", this);
    }

    public int GetEbonyMagicCount()
    {
        return ebonyMagicCount;
    }
    
    public void SetOnOffHeavyWallet(float bonus)
    {
        onOffHeavyWallet++;
        heavyWalletBonus = bonus;
        EventBus.Subscribe("GetItem", GetItem);
    }
    
    public int GetHeavyWalletStack()
    {
        return heavyWalletStack;
    }
    
    #endregion

    
    public void SkillSlotChanged() // 스킬슬롯에 스킬이 바뀌었을 때, 패시브 발동조건 체크
    {
        // 조화, 속성 과부화 체크
        SkillSlotController[] slotSkills = InventoryManager.Instance.inventoryUIManager.skillSlots;
        int elementalCheck = 1;
        
        if (slotSkills.Length != 4)
        {
            harmonyed = false;
            elementalOverChargeCheck = false;
        }
        else
        {
            HashSet<ElementalType> typeSet = new();
            ElementalType type = ElementalType.Normal;
            foreach (var kvp in slotSkills)
            {
                bool hasElemental = false;
                foreach (RuneSlotController runeSlot in kvp.runeSlots)
                {
                    if (runeSlot.transform.childCount == 0) continue;

                    var runeItem = runeSlot.transform.GetChild(0).GetComponent<DraggableItem>();
                    var runeData = runeItem?.itemData as RuneData;
                    var rune = runeData?.runeSO;

                    if (rune is IElementalRune elementalRune)
                    {
                        ElementalType tempType = elementalRune.GetElemental();
                        typeSet.Add(tempType);
                        
                        if(type == ElementalType.Normal)
                            type = tempType;
                        else
                        {
                            if (type == tempType)
                            {
                                elementalCheck++;
                            }
                        }
                    }
                }

            }

            if (onOffHarmony)
                harmonyed = typeSet.Count == 4;

            if (onOffElementalOverCharge)
            {
                if(elementalCheck == 4 && typeSet.Count == 1)
                    elementalOverChargeCheck = true;
                else
                    elementalOverChargeCheck = false;
                
            }
        }

        // 싱글톤
        singleToneBonusAtk = 2.5f - PlayerSkillManager.Instance.slotSkills.Count * singleToneBonus;
    }

    
    public float CheckHarmony()  //각각의 스킬의 속성이 모두 다른지 확인
    {
        if (!onOffHarmony || !harmonyed || PlayerSkillManager.Instance.slotSkills.Count != 4 )
            return 1f;
        
        return harmonyBonus;
    }

    public void GetItem(object param)
    {
        if(onOffHeavyWallet > 0)
        {
            heavyWalletStack += onOffHeavyWallet;
            maxHealth += heavyWalletBonus * onOffHeavyWallet;
        }
        PlayerController.Instance.statHandler.Heal(0);
    }
}