using System;
using UnityEngine;
using Mirror;

// Useful for UI. Whether the player is, well, a player or an enemy.
public enum PlayerType { PLAYER, ENEMY };

[RequireComponent(typeof(Deck))]
[Serializable]
public class Player : Entity
{
    [Header("Player Info")]
    [SyncVar(hook = nameof(UpdatePlayerName))] public string username; // SyncVar hook to call a command whenever a username changes (like when players load in initially).

    [Header("Portrait")]
    public Sprite portrait; // For the player's icon at the top left of the screen & in the PartyHUD.

    [Header("Deck")]
    public Deck deck;
    public Sprite cardback;
    [SyncVar, HideInInspector] public int tauntCount = 0; // Amount of taunt creatures on your side of the board.

    [Header("Stats")]
    [SyncVar] public int maxMana = 10;
    [SyncVar] public int currentMax = 0;
    [SyncVar] public int _mana = 0;
    public int mana
    {
        get { return Mathf.Min(_mana, maxMana); }
        set { _mana = Mathf.Clamp(value, 0, maxMana); }
    }

    [HideInInspector] public static Player localPlayer;
    [HideInInspector] public bool hasEnemy = false;
    [HideInInspector] public PlayerInfo enemyInfo;
    [HideInInspector] public static GameManager gameManager;
    [SyncVar, HideInInspector] public bool firstPlayer = false;

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;

        CmdLoadPlayer(PlayerPrefs.GetString("Name"));
        CmdLoadDeck();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        deck.deckList.Callback += deck.OnDeckListChange;
        deck.graveyard.Callback += deck.OnGraveyardChange;
    }

    [Command]
    public void CmdLoadPlayer(string user) => username = user;

    private void UpdatePlayerName(string oldUser, string newUser)
    {
        username = newUser;
        gameObject.name = newUser;
    }

    [Command]
    public void CmdLoadDeck()
    {
        for (int i = 0; i < deck.startingDeck.Length; ++i)
        {
            CardAndAmount card = deck.startingDeck[i];
            for (int v = 0; v < card.amount; ++v)
            {
                deck.deckList.Add(card.amount > 0 ? new CardInfo(card.card, 1) : new CardInfo());
                if (deck.hand.Count < 7) deck.hand.Add(new CardInfo(card.card, 1));
            }
        }
        if (deck.hand.Count == 7)
            deck.hand.Shuffle();
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        health = gameManager.maxHealth;
        maxMana = gameManager.maxMana;
        deck.deckSize = gameManager.deckSize;
        deck.handSize = gameManager.handSize;
    }

    public override void Update()
    {
        base.Update();

        if (!hasEnemy && username != "")
            UpdateEnemyInfo();

        if (isLocalPlayer && Input.GetKeyDown(KeyCode.Return))
            gameManager.StartGame();
    }

    public void UpdateEnemyInfo()
    {
        Player[] onlinePlayers = FindObjectsOfType<Player>();

        foreach (Player players in onlinePlayers)
        {
            if (players.username != "")
            {
                if (players != this)
                {
                    // Get & Set PlayerInfo from our Enemy's gameObject
                    PlayerInfo currentPlayer = new PlayerInfo(players.gameObject);
                    enemyInfo = currentPlayer;
                    hasEnemy = true;
                    enemyInfo.data.casterType = Target.OPPONENT;
                }
            }
        }
    }

    public bool IsOurTurn() => gameManager.isOurTurn;
}