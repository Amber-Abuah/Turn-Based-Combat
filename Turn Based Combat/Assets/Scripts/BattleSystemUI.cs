using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class BattleSystemUI : MonoBehaviour
{
    [SerializeField] Transform optionMenu;
    [SerializeField] Transform attackMenu;
    [SerializeField] Transform abilitiesMenu;
    [SerializeField] Transform itemsMenu;
    [SerializeField] Transform characterSelectMenu;

    List<Transform> itemButtons;
    List<Transform> characterSelectButtons;

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Transform ultimate;
    [SerializeField] Image ultimateBar;
    BattleSystem battleSystem;
    PlayerInventory playerInventory;

    Item selectedItem;

    private void Start()
    {
        battleSystem = BattleSystem.Instance;
        playerInventory = PlayerInventory.Instance;

        itemButtons = new List<Transform>();
        characterSelectButtons = new List<Transform>();

        for (int i = 0; i < itemsMenu.childCount; i++)
            itemButtons.Add(itemsMenu.GetChild(i));

        for (int i = 0; i < characterSelectMenu.childCount; i++)
            characterSelectButtons.Add(characterSelectMenu.GetChild(i));

        ultimate.gameObject.SetActive(false);
    }

    public void StartBattleUI()
    {
        DisableAllMenus();
        dialogueText.text = "A horde of enemies draw near!";
    }

    public void DisableAllMenus()
    {
        optionMenu.gameObject.SetActive(false);
        attackMenu.gameObject.SetActive(false);
        abilitiesMenu.gameObject.SetActive(false);
        itemsMenu.gameObject.SetActive(false);
        characterSelectMenu.gameObject.SetActive(false);
    }

    void DisableSubMenus()
    {
        attackMenu.gameObject.SetActive(false);
        abilitiesMenu.gameObject.SetActive(false);
        itemsMenu.gameObject.SetActive(false);
        characterSelectMenu.gameObject.SetActive(false);
    }

    public void DisableAllUI()
    {
        DisableAllMenus();
        ultimate.gameObject.SetActive(false);
    }

    public void PlayerTurnUI()
    {
        dialogueText.text = "";
        ultimate.gameObject.SetActive(true);
        optionMenu.gameObject.SetActive(true);
    }

    public void EnemyTurnUI()
    {
        dialogueText.text = "";
        DisableAllMenus();
    }

    public void ShowSubMenu(int menuIndex)
    {
        DisableSubMenus();

        switch (menuIndex)
        {
            case 0:
                attackMenu.gameObject.SetActive(true);
                attackMenu.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = battleSystem.PlayerAttackName();
                break;
            case 1:
                abilitiesMenu.gameObject.SetActive(true);
                abilitiesMenu.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = battleSystem.PlayerAbilities().Name;
                break;
            case 2:
                itemsMenu.gameObject.SetActive(true);
                ShowItems();
                break;
        }
    }

    void ShowItems()
    {
        List<Item> items = playerInventory.GetItems();

        for (int i = 0; i < itemButtons.Count; i++)
        {
            if (i < items.Count)
            {
                itemButtons[i].gameObject.SetActive(true);
                itemButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = items[i].ItemName 
                    + " x" + playerInventory.GetItemQuantity(items[i]);
            }
            else
                itemButtons[i].gameObject.SetActive(false);
        }
    }

    public void ShowDialogue(string text)
    {
        dialogueText.text = text;
    }

    public void ShowCharacterSelectMenu(int itemIndex)
    {
        selectedItem = playerInventory.GetItems()[itemIndex];

        characterSelectMenu.gameObject.SetActive(true);

        List<PlayerUnit> eligiblePlayers = battleSystem.EligiblePlayersForItem(selectedItem);

        if(eligiblePlayers.Count == 0)
            SFXManager.Instance.PlayMenuSFX(1);

        for (int i = 0; i < characterSelectButtons.Count; i++)
        {
            if (i < eligiblePlayers.Count)
            {
                characterSelectButtons[i].gameObject.SetActive(true);
                characterSelectButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = eligiblePlayers[i].Name;
            }
            else
                characterSelectButtons[i].gameObject.SetActive(false);
        }
    }
    
    public void UpdateUltimateBar(float amount)
    {
        ultimateBar.fillAmount = amount;
    }

    // Used for button events
    public void Attack()
    {
        battleSystem.PlayerAttack();
    }

    public void Abilities()
    {
        battleSystem.PlayerAbility();
    }

    public void Ultimate()
    {
        battleSystem.ActivateUltimate();
    }

    public void UseItemOnCharacter(int characterIndex)
    {
        List<PlayerUnit> eligiblePlayers = battleSystem.EligiblePlayersForItem(selectedItem);

        battleSystem.PlayerUseItem(selectedItem, eligiblePlayers[characterIndex]);
        playerInventory.UseItem(selectedItem);

        selectedItem = null;
    }
}
