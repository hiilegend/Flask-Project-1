using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public int TOTAL_CARDS_LOW = 39;

	public int TOTAL_CARDS_HIGH = 46;

	private int TOTAL_CARDS = 42;

	private const float OFFSET_MOVEMENT = 0.25f;

	private const float CARD_VERTICAL_OFFSET = 0.35f;

	private const float CARD_DRAG_DURATION = 0.5f;

	private const float CARD_HIT_DRAG_DURATION = 2f;

	public Transform computerCardLocation;

	public Transform computerCardStartLocation;

	public Transform cardDesitnation;

	public GameObject parent;

	public Sprite[] avatars;

	public GameObject[] cards;

	public GameObject[] defaultCards;

	public GameObject spadeContainer;

	public GameObject heartContainer;

	public GameObject diamondContainer;

	public GameObject clubesContainer;

	public GameObject youWinPanel;

	public GameObject winParticle;

	public GameObject cardHolder;

	public GameObject gameDrawText;

	public Text dealMessage;

	public Text winCoins;

	public Text playerName;

	public Text quitMessage;

	public Text quitButton;

	public Text xps;

	public GameObject startButton;

	public GameObject addButton;

	public GameObject pushButton;

	public GameObject closeButton;

	public GameObject noteButton;

	public GameObject moveContainer;

	public GameObject compCardContainer;

	public GameObject quitDialog;

	public GameObject arrowImage;

	public GameObject dealDialog;

	public GameObject yourCardsPanel;

	public GameObject aceCardsPanel;

	public GameObject dealButtonPanel;

	public GameObject initialHelpPanel;

	public GameObject cardHelpPanel;

	public GameObject userTurnPanel;

	public GameObject hithelpPanel;

	public GameObject opponentHithelpPanel;

	public GameObject gameOverPanel;

	public GameObject trophy;

	public Button homeButton;

	public Button retryButton;

	public Image profilePic;

	public Image winProfilePic;

	public Animator animator;

	public Animator dealAnimator;

	public Animator canvasScaleAnimator;

	public RuntimeAnimatorController leftAnimController;

	public RuntimeAnimatorController rightAnimController;

	private List<int> comcards = new List<int>();

	private List<int> usercards = new List<int>();

	private bool isusercard = true;

	private bool firstLaunch = true;

	private bool userTurnHelpShown = true;

	private bool hitHelpShown = true;

	private bool OpponentHitHelpShown = true;

	private GameObject lastCard;

	private GameObject thrownCard;

	private GameObject userTouchedCard;

	private Dictionary<Card.Suit, bool> userSuitAvailablity = new Dictionary<Card.Suit, bool>();

	private AudioSource audioSource;

	private bool userTurn;

	private bool gameStarted;

	private bool addExtraCardToUser = true;

	public Color disabledColor;

	public Color enabledColor;

	private Card lastUserCard;

	private Card lastCompCard;

	private Vector3 hitDesitnation;

	private Vector3 firstHitCardPosition;

	private Vector3 secondHitCardPosition;

	private Vector3 hitUserDesitnation;

	private bool starAnimateUserCard;

	private bool starAnimateCompCard;

	private bool startFirstCardHitAnim;

	private bool startHitDragging;

	private float endDuration = 0.5f;

	private float duration;

	private bool playerTurn;

	private bool canPushCard;

	private bool animateCardReturn;

	private float returnDuration;

	private float returnEndDuration;

	private bool hittingUser;

	public GameObject glow;

	private bool turnOne;

	private GameObject draggedCard;

	private Vector3 unoCardOriginalPosition;

	private Vector3 initialTouchPosition;

	private GameObject compMovedCardGO;

	private GameObject userMovedCardGO;

	private GameObject lastThrownCard;

	private void Awake()
	{
		PlayerPrefs.SetInt("show_help", 0);
		if (LevelManager.getInstance().gameMode == LevelManager.GameMode.HELP_MODE)
		{
			firstLaunch = true;
			userTurnHelpShown = false;
			hitHelpShown = false;
			OpponentHitHelpShown = false;
			quitMessage.text = "Do you want to\nexit the tutorials?";
			quitButton.text = "Exit";
		}
		else
		{
			firstLaunch = ((PlayerPrefs.GetInt("first_launch", 1) == 1) ? true : false);
			userTurnHelpShown = ((PlayerPrefs.GetInt("user_turn", 1) != 1) ? true : false);
			hitHelpShown = ((PlayerPrefs.GetInt("hit_help", 1) != 1) ? true : false);
			OpponentHitHelpShown = ((PlayerPrefs.GetInt("opponent_hit_help", 1) != 1) ? true : false);
			PlayerPrefs.SetInt("first_launch", 0);
		}
		TOTAL_CARDS = UnityEngine.Random.Range(TOTAL_CARDS_LOW, TOTAL_CARDS_HIGH);
	}

	private void Start()
	{
		userSuitAvailablity.Add(Card.Suit.SPADE, value: true);
		userSuitAvailablity.Add(Card.Suit.CLUB, value: true);
		userSuitAvailablity.Add(Card.Suit.DIAMOND, value: true);
		userSuitAvailablity.Add(Card.Suit.HEART, value: true);
		audioSource = GetComponent<AudioSource>();
		if (AdManager.getInstance() != null)
		{
			AdManager.getInstance().showAd();
		}
		homeButton.enabled = false;
		retryButton.enabled = false;
		if (firstLaunch)
		{
			initialHelpPanel.SetActive(value: true);
		}
		else
		{
			AddCard();
		}
		loadProfilePic();
		shiftCardsOnLargeDevice();
	}

	private void shiftCardsOnLargeDevice()
	{
		canvasScaleAnimator.enabled = false;
		int num = Screen.height / Screen.width;
		if (num >= 2)
		{
			Vector3 position = spadeContainer.transform.position;
			spadeContainer.transform.position = new Vector3(-1.9f, position.y, position.z);
			UnityEngine.Debug.LogError("Aspecr:" + spadeContainer.transform.position);
			position = heartContainer.transform.position;
			heartContainer.transform.position = new Vector3(-0.6f, position.y, position.z);
			position = clubesContainer.transform.position;
			clubesContainer.transform.position = new Vector3(0.69f, position.y, position.z);
			position = diamondContainer.transform.position;
			diamondContainer.transform.position = new Vector3(2f, position.y, position.z);
		}
	}

	private void loadProfilePic()
	{
		int @int = PlayerPrefs.GetInt("avatar", 0);
		if (FB.IsLoggedIn && @int == -1)
		{
			FB.API("me/picture?type=square&width=200&height=200&redirect=false", HttpMethod.GET, ProfiilePicCallBack);
		}
		else
		{
			profilePic.sprite = avatars[@int];
		}
	}

	public void ProfiilePicCallBack(IGraphResult result)
	{
		UnityEngine.Debug.Log(result.RawResult);
		if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
		{
			IDictionary dictionary = result.ResultDictionary["data"] as IDictionary;
			string url = dictionary["url"] as string;
			StartCoroutine(fetchProfilePic(url));
		}
		else
		{
			UnityEngine.Debug.Log("ProfiilePicCallBack Error");
		}
	}

	private IEnumerator fetchProfilePic(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		profilePic.gameObject.SetActive(value: true);
		profilePic.sprite = Sprite.Create(www.texture, new Rect(0f, 0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
	}

	private bool IsInteractable(GameObject touchedCard)
	{
		UnityEngine.Debug.LogError("IsInteractable startHitDragging" + startHitDragging);
		UnityEngine.Debug.LogError("IsInteractable playerTurn" + playerTurn);
		if (startHitDragging || !playerTurn)
		{
			return false;
		}
		if (thrownCard != null)
		{
			Card component = touchedCard.GetComponent<Card>();
			Card component2 = thrownCard.GetComponent<Card>();
			if (userHasThisSuit(component2) && component.suit != component2.suit)
			{
				dealMessage.text = getSuitMessage(component2);
				dealAnimator.SetTrigger("move");
				return false;
			}
			if (userHasThisSuit(component2) && turnOne && component.value != Card.CardValue.ACE)
			{
				return false;
			}
		}
		UnityEngine.Debug.LogError("IsInteractable true");
		return true;
	}

	private string getSuitMessage(Card thrownSuit)
	{
		switch (thrownSuit.suit)
		{
		case Card.Suit.SPADE:
			return "Choose a Spade <color=yellow>(♠)</color> card and press 'DEAL'";
		case Card.Suit.DIAMOND:
			return "Choose a Diamond <color=yellow>(♦)</color> card and press 'DEAL'";
		case Card.Suit.CLUB:
			return "Choose a Club <color=yellow>(♣)</color> card and press 'DEAL'";
		default:
			return "Choose a Heart <color=yellow>(♥)</color> card and press 'DEAL'";
		}
	}

	private bool userHasThisSuit(Card card)
	{
		foreach (int usercard in usercards)
		{
			Card component = cards[usercard].GetComponent<Card>();
			if (component.suit == card.suit)
			{
				return true;
			}
		}
		return false;
	}

	public void AddCard()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_ADD_CARD);
		addButton.SetActive(value: false);
		usercards.Clear();
		comcards.Clear();
		int i = 0;
		if (firstLaunch)
		{
			usercards.Add(0);
			i++;
			turnOne = true;
			isusercard = false;
		}
		for (; i < TOTAL_CARDS; i++)
		{
			int item = UnityEngine.Random.Range(0, 52);
			while (usercards.Contains(item) || comcards.Contains(item))
			{
				item = UnityEngine.Random.Range(0, 52);
			}
			if (isusercard)
			{
				usercards.Add(item);
			}
			else
			{
				comcards.Add(item);
			}
			isusercard = !isusercard;
		}
		if (!usercards.Contains(0) && !comcards.Contains(0))
		{
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				usercards.Add(0);
			}
			else
			{
				comcards.Add(0);
			}
		}
		usercards.Sort();
		comcards.Sort();
		StartCoroutine(distributeCards());
	}

	private void difficultMode()
	{
		int i = 0;
		int num = TOTAL_CARDS / 2 - 3;
		int num2 = TOTAL_CARDS / 2 + TOTAL_CARDS % 2 + 3;
		UnityEngine.Debug.LogError(num2 + ":" + num);
		for (; i < num2; i++)
		{
			int item = UnityEngine.Random.Range(0, 52);
			while (usercards.Contains(item))
			{
				item = UnityEngine.Random.Range(0, 52);
			}
			usercards.Add(item);
		}
		for (i = 0; i < num; i++)
		{
			int item2 = UnityEngine.Random.Range(0, 52);
			while (usercards.Contains(item2) || comcards.Contains(item2))
			{
				item2 = UnityEngine.Random.Range(0, 52);
			}
			comcards.Add(item2);
		}
	}

	private IEnumerator distributeCompCard()
	{
		GameObject[] array = defaultCards;
		foreach (GameObject card in array)
		{
			card.SetActive(value: true);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public IEnumerator distributeCards()
	{
		compCardContainer.SetActive(value: true);
		cardHolder.SetActive(value: true);
		closeButton.SetActive(value: true);
		foreach (int num in usercards)
		{
			GameObject gamecard = cards[num];
			Card card = gamecard.GetComponent<Card>();
			switch (card.suit)
			{
			case Card.Suit.SPADE:
				addSpadeSuits(gamecard, spadeContainer, num - num / 13 * 13, num);
				break;
			case Card.Suit.DIAMOND:
				addSpadeSuits(gamecard, diamondContainer, num - num / 13 * 13, num);
				break;
			case Card.Suit.CLUB:
				addSpadeSuits(gamecard, clubesContainer, num - num / 13 * 13, num);
				break;
			case Card.Suit.HEART:
				addSpadeSuits(gamecard, heartContainer, num - num / 13 * 13, num);
				break;
			}
			yield return new WaitForSeconds(0.05f);
		}
		AudioManager.getInstance().StopSound(AudioManager.PLAY_ADD_CARD);
		OnStartClicked();
	}

	public void OnStartClicked()
	{
		if (firstLaunch)
		{
			yourCardsPanel.SetActive(value: true);
			gameStarted = true;
			turnOne = true;
			showPushControl();
			lastCompCard = null;
		}
		else
		{
			startGame();
			ShowNote();
		}
	}

	private void startGame()
	{
		gameStarted = true;
		if (userHasAce())
		{
			UnityEngine.Debug.LogError("userHasAce");
			playerTurn = true;
			turnOne = true;
			showPushControl();
			lastCompCard = null;
			shiftAceToRight();
		}
		else if (compHasAce())
		{
			UnityEngine.Debug.LogError("compHasAce");
			turnOne = false;
			moveCompCard(0);
			userTurn = true;
			showPushControl();
		}
	}

	private void addSpadeSuits(GameObject gamecard, GameObject parentContainer, int num, int cardIndex)
	{
		int childCount = parentContainer.transform.childCount;
		GameObject gameObject = UnityEngine.Object.Instantiate(gamecard, parentContainer.transform.position, Quaternion.identity);
		gameObject.transform.parent = parentContainer.transform;
		gameObject.transform.localPosition = new Vector3(0f, (float)childCount * 0.35f, num);
		if (cardIndex != 0)
		{
		}
	}

	private void moveCompCard(int cardIndex)
	{
		GameObject gameObject = cards[cardIndex];
		lastCompCard = gameObject.GetComponent<Card>();
		compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(gameObject, computerCardStartLocation.position, Quaternion.identity));
		comcards.RemoveAt(cardIndex);
		startAnimateCompCard();
	}

	private int moveUserCard(int usercardIndex)
	{
		int num = usercards[usercardIndex];
		lastUserCard = cards[num].GetComponent<Card>();
		thrownCard.transform.parent = null;
		startAnimateUserCard();
		switch (lastUserCard.suit)
		{
		case Card.Suit.SPADE:
			moveCardsDown(spadeContainer, lastUserCard);
			break;
		case Card.Suit.DIAMOND:
			moveCardsDown(diamondContainer, lastUserCard);
			break;
		case Card.Suit.CLUB:
			moveCardsDown(clubesContainer, lastUserCard);
			break;
		case Card.Suit.HEART:
			moveCardsDown(heartContainer, lastUserCard);
			break;
		}
		usercards.RemoveAt(usercardIndex);
		return num;
	}

	public void startAnimateUserCard()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_PLACE_CARD);
		starAnimateUserCard = true;
		duration = 0f;
		endDuration = 0.5f;
	}

	public void stopAnimateUserCard()
	{
		starAnimateUserCard = false;
	}

	public void startAnimateCompCard()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_PLACE_CARD);
		starAnimateCompCard = true;
		duration = 0f;
		endDuration = 0.5f;
	}

	public void stopAnimateCompCard()
	{
		starAnimateCompCard = false;
		if (!hitHelpShown && userTurn && thrownCard != null && !userHasThisSuit(thrownCard.GetComponent<Card>()))
		{
			hitHelpShown = true;
			PlayerPrefs.SetInt("hit_help", 0);
			hithelpPanel.SetActive(value: true);
		}
	}

	public void startAnimateFirstHitCard()
	{
		startFirstCardHitAnim = true;
		duration = 0f;
		endDuration = 0.5f;
	}

	public void stopAnimateFirstHitCard()
	{
		startFirstCardHitAnim = false;
	}

	public void startHitDrag()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_HIT_CARD);
		startHitDragging = true;
		duration = 0f;
		endDuration = 2f;
	}

	public void stopHitDrag()
	{
		startHitDragging = false;
		thrownCard = null;
		lastCard = null;
		if (usercards.Count != 0 && comcards.Count != 0)
		{
			if (hittingUser && !OpponentHitHelpShown)
			{
				OpponentHitHelpShown = true;
				PlayerPrefs.SetInt("opponent_hit_help", 0);
				opponentHithelpPanel.SetActive(value: true);
			}
			Invoke("showPushControl", 0.25f);
		}
	}

	public void pushUserCard()
	{
		if (userTouchedCard == null)
		{
			dealMessage.text = getDealHelpMessage();
			dealAnimator.SetTrigger("move");
			return;
		}
		playerTurn = false;
		turnOne = false;
		pushButton.SetActive(value: false);
		arrowImage.SetActive(value: false);
		aceCardsPanel.SetActive(value: false);
		dealButtonPanel.SetActive(value: false);
		userMovedCardGO = (thrownCard = userTouchedCard);
		int cardIndexFromUserCards = getCardIndexFromUserCards(thrownCard.GetComponent<Card>());
		int num = moveUserCard(cardIndexFromUserCards);
		userTouchedCard = null;
		glow.SetActive(value: false);
		HighlightAllCards();
	}

	private string getDealHelpMessage()
	{
		if (thrownCard == null)
		{
			return "Choose a card before pressing ‘DEAL’";
		}
		Card component = thrownCard.GetComponent<Card>();
		if (component != null)
		{
			if (!userHasThisSuit(component))
			{
				return "Choose a card before pressing ‘DEAL’";
			}
			switch (component.suit)
			{
			case Card.Suit.SPADE:
				return "Choose a Spade card and press 'deal'";
			case Card.Suit.CLUB:
				return "Choose a Club card and press 'deal'";
			case Card.Suit.DIAMOND:
				return "Choose a Diamond card and press 'deal'";
			case Card.Suit.HEART:
				return "Choose a Heart card and press 'deal'";
			}
		}
		return "Choose a card before pressing ‘DEAL’";
	}

	private bool userHasAce()
	{
		return usercards.Contains(0);
	}

	private bool compHasAce()
	{
		return comcards.Contains(0);
	}

	private bool IsAnyPanelOpen()
	{
		return opponentHithelpPanel.activeInHierarchy || hithelpPanel.activeInHierarchy || gameOverPanel.activeInHierarchy || quitDialog.activeInHierarchy || aceCardsPanel.activeInHierarchy || yourCardsPanel.activeInHierarchy;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && !IsAnyPanelOpen())
		{
			highlightSelectedCard();
		}
		if (Input.GetMouseButton(0) && draggedCard != null)
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
			Vector3 position2 = userTouchedCard.transform.position;
			position.z = position2.z;
			userTouchedCard.transform.position = position;
			object[] array = new object[5];
			Vector3 localPosition = userTouchedCard.transform.localPosition;
			array[0] = localPosition.y;
			array[1] = " | ";
			float y = unoCardOriginalPosition.y;
			Vector3 localPosition2 = userTouchedCard.transform.localPosition;
			array[2] = Mathf.Abs(y - localPosition2.y);
			array[3] = " | ";
			array[4] = Mathf.Abs(initialTouchPosition.y - position.y);
			UnityEngine.Debug.LogError(string.Concat(array));
			UnityEngine.Debug.LogError("--------");
			Vector3 localPosition3 = userTouchedCard.transform.localPosition;
			if (localPosition3.y > 2.5f)
			{
			}
			float y2 = unoCardOriginalPosition.y;
			Vector3 localPosition4 = userTouchedCard.transform.localPosition;
			if (Mathf.Abs(y2 - localPosition4.y) > 1f)
			{
			}
			if (Mathf.Abs(initialTouchPosition.y - position.y) > 0.5f)
			{
				canPushCard = true;
			}
		}
		else if (Input.GetMouseButtonUp(0) && draggedCard != null)
		{
			if (canPushCard)
			{
				pushUserCard();
				canPushCard = false;
				draggedCard = null;
			}
			else
			{
				draggedCard = null;
				StartCardReturnAnimation();
			}
		}
		if (returnDuration > returnEndDuration && animateCardReturn)
		{
			StopCardReturnAnimation();
		}
		if (animateCardReturn)
		{
			returnDuration += Time.deltaTime;
			float t = returnDuration / returnEndDuration;
			userTouchedCard.transform.localPosition = Vector3.Lerp(userTouchedCard.transform.localPosition, unoCardOriginalPosition, t);
		}
		if (duration > endDuration && starAnimateUserCard)
		{
			UnityEngine.Debug.Log("Drag end");
			stopAnimateUserCard();
			processInput();
		}
		if (duration > endDuration && startHitDragging)
		{
			stopHitDrag();
		}
		if (duration > endDuration && starAnimateCompCard)
		{
			stopAnimateCompCard();
		}
		if (starAnimateUserCard)
		{
			duration += Time.deltaTime;
			float t2 = duration / endDuration;
			userMovedCardGO.transform.position = Vector3.Lerp(userMovedCardGO.transform.position, cardDesitnation.position, t2);
			userMovedCardGO.transform.localScale = Vector3.Lerp(userMovedCardGO.transform.localScale, new Vector3(0.8f, 0.8f, 1f), t2);
		}
		if (starAnimateCompCard)
		{
			duration += Time.deltaTime;
			float t3 = duration / endDuration;
			compMovedCardGO.transform.position = Vector3.Lerp(compMovedCardGO.transform.position, computerCardLocation.position, t3);
			compMovedCardGO.transform.localScale = Vector3.Lerp(compMovedCardGO.transform.localScale, new Vector3(0.8f, 0.8f, 1f), t3);
		}
		if (startHitDragging)
		{
			duration += Time.deltaTime;
			float t4 = duration / endDuration;
			compMovedCardGO.transform.localPosition = Vector3.Lerp(compMovedCardGO.transform.localPosition, hitDesitnation, t4);
			float num = (!hittingUser) ? 0.8f : 1f;
			compMovedCardGO.transform.localScale = Vector3.Lerp(compMovedCardGO.transform.localScale, new Vector3(num, num, 1f), t4);
			userMovedCardGO.transform.localPosition = Vector3.Lerp(userMovedCardGO.transform.localPosition, hitUserDesitnation, t4);
			userMovedCardGO.transform.localScale = Vector3.Lerp(userMovedCardGO.transform.localScale, new Vector3(num, num, 1f), t4);
		}
	}

	public void StartCardReturnAnimation()
	{
		animateCardReturn = true;
		returnDuration = 0f;
		returnEndDuration = 0.25f;
	}

	public void StopCardReturnAnimation()
	{
		animateCardReturn = false;
	}

	private void startNextRound()
	{
		if (lastUserCard.value < lastCompCard.value)
		{
			lastUserCard = null;
			userTurn = true;
			Invoke("computerRandomMove", 1.6f);
			Invoke("showPushControl", 1.6f);
			return;
		}
		userTurn = false;
		lastUserCard = null;
		lastCompCard = null;
		thrownCard = null;
		if (userTurnHelpShown)
		{
			showPushControl();
			return;
		}
		userTurnHelpShown = true;
		userTurnPanel.SetActive(value: true);
		PlayerPrefs.SetInt("user_turn", 0);
	}

	private void processInput()
	{
		if (isUserMovedCardFirst())
		{
			if (computerMoveBasedOnUserCard())
			{
				userTurnHelpShown = true;
				hitUser(thrownCard.GetComponent<Card>());
				lastUserCard = null;
				lastCompCard = null;
				hittingUser = true;
				startHitDrag();
				endDuration = 1f;
				if (!isGameOver())
				{
				}
			}
			else
			{
				clearCards();
				if (!isGameOver())
				{
					Invoke("startNextRound", 1.7f);
				}
			}
			return;
		}
		Card component = userMovedCardGO.GetComponent<Card>();
		if (component.suit != lastCompCard.suit)
		{
			hitComputer(getCardIndex(component));
			hitDesitnation = computerCardStartLocation.position;
			hitUserDesitnation = computerCardStartLocation.position;
			hittingUser = false;
			startHitDrag();
			UnityEngine.Object.Destroy(userMovedCardGO, 2.1f);
			UnityEngine.Object.Destroy(compMovedCardGO, 2.1f);
			if (!isGameOver())
			{
				Invoke("computerRandomMove", 2.2f);
				Invoke("showPushControl", 2.2f);
			}
			return;
		}
		clearCards();
		if (!isGameOver())
		{
			if (lastUserCard.value < lastCompCard.value)
			{
				Invoke("computerRandomMove", 1.6f);
				Invoke("showPushControl", 2f);
				return;
			}
			Invoke("showPushControl", 1.6f);
			userTurn = false;
			lastUserCard = null;
			lastCompCard = null;
			thrownCard = null;
		}
	}

	private void showPushControl()
	{
		playerTurn = true;
		glow.SetActive(value: true);
		HighlightPlaybleCards();
		pushButton.SetActive(value: true);
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_PLAYER_TURN);
		if (PlayerPrefs.GetInt("vibrate", 1) != 1 || 1 == 0)
		{
		}
	}

	private void HighlightPlaybleCards()
	{
		if (thrownCard != null)
		{
			Card component = thrownCard.GetComponent<Card>();
			if (userHasThisSuit(component))
			{
				HighlightSuit(component);
			}
			else
			{
				HighlightAllCards();
			}
		}
		else
		{
			HighlightAllCards();
		}
	}

	private void HighlightSuit(Card thrownSuit)
	{
		GameObject cardContainer = getCardContainer(thrownSuit);
		EnableContainer(cardContainer);
		if (cardContainer != spadeContainer)
		{
			DisableContainer(spadeContainer);
		}
		if (cardContainer != clubesContainer)
		{
			DisableContainer(clubesContainer);
		}
		if (cardContainer != diamondContainer)
		{
			DisableContainer(diamondContainer);
		}
		if (cardContainer != heartContainer)
		{
			DisableContainer(heartContainer);
		}
	}

	private void HighlightAllCards()
	{
		EnableContainer(spadeContainer);
		EnableContainer(clubesContainer);
		EnableContainer(diamondContainer);
		EnableContainer(heartContainer);
	}

	private void EnableContainer(GameObject container)
	{
		for (int i = 0; i < container.transform.childCount; i++)
		{
			container.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = enabledColor;
		}
	}

	private void DisableContainer(GameObject container)
	{
		for (int i = 0; i < container.transform.childCount; i++)
		{
			container.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = disabledColor;
		}
	}

	private void DisableAllCards()
	{
		DisableContainer(spadeContainer);
		DisableContainer(clubesContainer);
		DisableContainer(diamondContainer);
		DisableContainer(heartContainer);
	}

	private void doComputerMoveAfterHittingUser()
	{
		UnityEngine.Object.Destroy(compMovedCardGO);
		UnityEngine.Object.Destroy(userMovedCardGO);
		if (!isGameOver() && !isDraw())
		{
			computerRandomMove();
		}
	}

	private void highlightSelectedCard()
	{
		Collider2D collider2D = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition));
		if (collider2D != null && collider2D.gameObject != null && IsInteractable(collider2D.gameObject))
		{
			shiftCard(collider2D.gameObject);
		}
	}

	private void shiftCard(GameObject pushcardObject)
	{
		Vector3 localPosition;
		if (lastCard != null)
		{
			localPosition = lastCard.transform.localPosition;
			if (lastCard.GetComponent<Card>().suit == Card.Suit.DIAMOND)
			{
				localPosition.x += 0.25f;
			}
			else
			{
				localPosition.x -= 0.25f;
			}
			lastCard.transform.localPosition = localPosition;
		}
		lastCard = (draggedCard = pushcardObject);
		userTouchedCard = lastCard;
		draggedCard = null;
		localPosition = pushcardObject.transform.localPosition;
		if (lastCard.GetComponent<Card>().suit == Card.Suit.DIAMOND)
		{
			localPosition.x -= 0.25f;
		}
		else
		{
			localPosition.x += 0.25f;
		}
		pushcardObject.transform.localPosition = localPosition;
		unoCardOriginalPosition = pushcardObject.transform.localPosition;
		initialTouchPosition = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
		if (PlayerPrefs.GetInt("audio", 1) == 1)
		{
			audioSource.Play();
		}
		arrowImage.SetActive(value: true);
	}

	private bool computerMoveBasedOnUserCard()
	{
		Card component = thrownCard.GetComponent<Card>();
		int num = -1;
		int minCardIndex = -1;
		int compCardIndex = -1;
		for (int i = 0; i < comcards.Count; i++)
		{
			int num2 = comcards[i];
			GameObject gameObject = cards[num2];
			Card component2 = gameObject.GetComponent<Card>();
			if (component2.suit == component.suit)
			{
				if (num == -1)
				{
					minCardIndex = i;
					compCardIndex = i;
				}
				num = i;
			}
		}
		if (num != -1)
		{
			dragComputerCardToCenter(minCardIndex, num, compCardIndex);
			return false;
		}
		return true;
	}

	public void dragComputerCardToCenter(int minCardIndex, int maxCardIndex, int compCardIndex)
	{
		compCardIndex = UnityEngine.Random.Range(minCardIndex, maxCardIndex + 1);
		int num = comcards[compCardIndex];
		GameObject gameObject = cards[num];
		compMovedCardGO = UnityEngine.Object.Instantiate(gameObject, computerCardStartLocation.position, Quaternion.identity);
		lastCompCard = gameObject.GetComponent<Card>();
		comcards.RemoveAt(compCardIndex);
		startAnimateCompCard();
		updateCompCards();
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_PLACE_CARD);
	}

	private int getComputerHitCardIndex(Card thrownCard)
	{
		int cardIndex = getCardIndex(thrownCard);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		int num8 = -1;
		Card card = null;
		int result = 0;
		for (int i = 0; i < comcards.Count; i++)
		{
			Card component = cards[comcards[i]].GetComponent<Card>();
			if (card == null)
			{
				card = component;
				result = i;
			}
			else if (component.value > card.value)
			{
				card = component;
				result = i;
			}
			switch (component.suit)
			{
			case Card.Suit.SPADE:
				num++;
				if (num5 == -1)
				{
					num5 = i;
				}
				break;
			case Card.Suit.CLUB:
				num2++;
				if (num6 == -1)
				{
					num6 = i;
				}
				break;
			case Card.Suit.DIAMOND:
				num3++;
				if (num7 == -1)
				{
					num7 = i;
				}
				break;
			case Card.Suit.HEART:
				num4++;
				if (num8 == -1)
				{
					num8 = i;
				}
				break;
			}
		}
		if (!userSuitAvailablity[Card.Suit.SPADE] && num > 0)
		{
			hitDesitnation = spadeContainer.transform.position;
			return num5;
		}
		if (!userSuitAvailablity[Card.Suit.CLUB] && num2 > 0)
		{
			hitDesitnation = clubesContainer.transform.position;
			return num6;
		}
		if (!userSuitAvailablity[Card.Suit.DIAMOND] && num3 > 0)
		{
			hitDesitnation = diamondContainer.transform.position;
			return num7;
		}
		if (!userSuitAvailablity[Card.Suit.HEART] && num4 > 0)
		{
			hitDesitnation = heartContainer.transform.position;
			return num8;
		}
		hitDesitnation = getCardContainer(card).transform.position;
		return result;
	}

	private void hitUser(Card thrownCard)
	{
		int cardIndex = getCardIndex(thrownCard);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		int num8 = -1;
		Card card = null;
		int sentCardIndex = 0;
		for (int i = 0; i < comcards.Count; i++)
		{
			Card component = cards[comcards[i]].GetComponent<Card>();
			if (card == null)
			{
				card = component;
				sentCardIndex = i;
			}
			else if (component.value > card.value)
			{
				card = component;
				sentCardIndex = i;
			}
			switch (component.suit)
			{
			case Card.Suit.SPADE:
				num++;
				if (num5 == -1)
				{
					num5 = i;
				}
				break;
			case Card.Suit.CLUB:
				num2++;
				if (num6 == -1)
				{
					num6 = i;
				}
				break;
			case Card.Suit.DIAMOND:
				num3++;
				if (num7 == -1)
				{
					num7 = i;
				}
				break;
			case Card.Suit.HEART:
				num4++;
				if (num8 == -1)
				{
					num8 = i;
				}
				break;
			}
		}
		if (!userSuitAvailablity[Card.Suit.SPADE] && num > 0)
		{
			userSuitAvailablity[Card.Suit.SPADE] = true;
			sendCardsToUser(spadeContainer, num5, cardIndex, thrownCard);
		}
		else if (!userSuitAvailablity[Card.Suit.CLUB] && num2 > 0)
		{
			userSuitAvailablity[Card.Suit.CLUB] = true;
			sendCardsToUser(clubesContainer, num6, cardIndex, thrownCard);
		}
		else if (!userSuitAvailablity[Card.Suit.DIAMOND] && num3 > 0)
		{
			userSuitAvailablity[Card.Suit.DIAMOND] = true;
			sendCardsToUser(diamondContainer, num7, cardIndex, thrownCard);
		}
		else if (!userSuitAvailablity[Card.Suit.HEART] && num4 > 0)
		{
			userSuitAvailablity[Card.Suit.HEART] = true;
			sendCardsToUser(heartContainer, num8, cardIndex, thrownCard);
		}
		else
		{
			sendCardsToUser(getCardContainer(card), sentCardIndex, cardIndex, thrownCard);
		}
	}

	private GameObject getCardContainer(Card card)
	{
		GameObject result = spadeContainer;
		switch (card.suit)
		{
		case Card.Suit.SPADE:
			result = spadeContainer;
			break;
		case Card.Suit.CLUB:
			result = clubesContainer;
			break;
		case Card.Suit.DIAMOND:
			result = diamondContainer;
			break;
		case Card.Suit.HEART:
			result = heartContainer;
			break;
		}
		return result;
	}

	private void sendCardsToUser(GameObject parentContainer, int sentCardIndex, int hitcardIndex, Card card)
	{
		int childCount = parentContainer.transform.childCount;
		int depth = comcards[sentCardIndex] - comcards[sentCardIndex] / 13 * 13;
		compMovedCardGO = UnityEngine.Object.Instantiate(cards[comcards[sentCardIndex]], computerCardLocation.position, Quaternion.identity);
		compMovedCardGO.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
		compMovedCardGO.transform.parent = parentContainer.transform;
		hitDesitnation = parentContainer.transform.position;
		hitDesitnation = reorderCards(compMovedCardGO, parentContainer, depth);
		usercards.Add(comcards[sentCardIndex]);
		if (hitcardIndex != -1)
		{
			usercards.Add(hitcardIndex);
			switch (card.suit)
			{
			case Card.Suit.SPADE:
				parentContainer = spadeContainer;
				break;
			case Card.Suit.CLUB:
				parentContainer = clubesContainer;
				break;
			case Card.Suit.DIAMOND:
				parentContainer = diamondContainer;
				break;
			case Card.Suit.HEART:
				parentContainer = heartContainer;
				break;
			}
			hitUserDesitnation = parentContainer.transform.position;
			userMovedCardGO.transform.parent = parentContainer.transform;
			depth = hitcardIndex - hitcardIndex / 13 * 13;
			hitUserDesitnation = reorderCards(userMovedCardGO, parentContainer, depth);
		}
		usercards.Sort();
		comcards.RemoveAt(sentCardIndex);
		updateCompCards();
	}

	private Vector3 reorderCards(GameObject cardThrown, GameObject parentContainer, int depth)
	{
		Card component = cardThrown.GetComponent<Card>();
		int childCount = parentContainer.transform.childCount;
		int num = 0;
		for (int i = 0; i < childCount; i++)
		{
			Card component2 = parentContainer.transform.GetChild(i).GetComponent<Card>();
			if (component.value > component2.value)
			{
				break;
			}
			num++;
		}
		cardThrown.transform.SetSiblingIndex(num);
		Vector3 result = Vector3.zero;
		for (int j = 0; j < childCount; j++)
		{
			GameObject gameObject = parentContainer.transform.GetChild(j).gameObject;
			if (cardThrown != gameObject)
			{
				gameObject.transform.localPosition = new Vector3(0f, (float)j * 0.35f, j);
			}
			else
			{
				result = new Vector3(0f, (float)j * 0.35f, j);
			}
		}
		return result;
	}

	private bool isDraw()
	{
		if (usercards.Count <= 2 || comcards.Count <= 2)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			foreach (int usercard in usercards)
			{
				Card component = cards[usercard].GetComponent<Card>();
				switch (component.suit)
				{
				case Card.Suit.SPADE:
					num++;
					break;
				case Card.Suit.CLUB:
					num2++;
					break;
				case Card.Suit.DIAMOND:
					num4++;
					break;
				case Card.Suit.HEART:
					num3++;
					break;
				}
			}
			num = normalizeValue(num);
			num2 = normalizeValue(num2);
			num3 = normalizeValue(num3);
			num4 = normalizeValue(num4);
			foreach (int comcard in comcards)
			{
				Card component2 = cards[comcard].GetComponent<Card>();
				switch (component2.suit)
				{
				case Card.Suit.SPADE:
					num5++;
					break;
				case Card.Suit.CLUB:
					num6++;
					break;
				case Card.Suit.DIAMOND:
					num8++;
					break;
				case Card.Suit.HEART:
					num7++;
					break;
				}
			}
			num5 = normalizeValue(num5);
			num6 = normalizeValue(num6);
			num8 = normalizeValue(num8);
			num7 = normalizeValue(num7);
			bool flag = num * num5 + num2 * num6 + num4 * num8 + num3 * num8 == 0;
			if (flag)
			{
				gameDrawText.SetActive(value: true);
			}
			return flag;
		}
		return false;
	}

	private bool isGameOver()
	{
		if (usercards.Count == 0 || comcards.Count == 0)
		{
			if (usercards.Count == 0)
			{
				youWinPanel.SetActive(value: true);
				winParticle.SetActive(value: true);
				AudioManager.getInstance().PlaySound(AudioManager.PLAY_WIN);
				AudioManager.getInstance().PlaySound(AudioManager.GAME_OVER);
				int @int = PlayerPrefs.GetInt("coins", 10000);
				PlayerPrefs.SetInt("coins", @int + 250);
				Invoke("showGameOverPanel", 2f);
			}
			else
			{
				UnityEngine.Debug.LogError("Comp won");
				showGameOverPanel();
				AudioManager.getInstance().PlaySound(AudioManager.PLAY_LOSE);
			}
			return true;
		}
		return false;
	}

	private int normalizeValue(int value)
	{
		if (value != 0)
		{
			return 1;
		}
		return value;
	}

	private bool isUserMovedCardFirst()
	{
		return lastCompCard == null;
	}

	private void saveThrownCard()
	{
		lastThrownCard = thrownCard;
	}

	private void clearCards()
	{
		Invoke("clear", 1f);
	}

	private void clear()
	{
		userMovedCardGO.transform.parent = moveContainer.transform;
		compMovedCardGO.transform.parent = moveContainer.transform;
		animator.SetBool("slide", value: true);
		UnityEngine.Object.Destroy(userMovedCardGO, 2f);
		UnityEngine.Object.Destroy(compMovedCardGO, 2f);
		Invoke("resetSlider", 2.2f);
	}

	public void resetSlider()
	{
		animator.SetBool("slide", value: false);
	}

	private void hitComputer(int cardIndex)
	{
		int cardIndex2 = getCardIndex(lastCompCard);
		if (cardIndex2 != -1)
		{
			comcards.Add(cardIndex2);
		}
		comcards.Add(cardIndex);
		comcards.Sort();
		Card component = cards[cardIndex2].GetComponent<Card>();
		userSuitAvailablity[component.suit] = false;
		userTurn = true;
		lastUserCard = null;
		lastCompCard = null;
		thrownCard = null;
		updateCompCards();
	}

	private void computerRandomMove()
	{
		userTurn = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = -1;
		int num10 = -1;
		int num11 = -1;
		int num12 = -1;
		int min = -1;
		int min2 = -1;
		int min3 = -1;
		int min4 = -1;
		for (int i = 0; i < comcards.Count; i++)
		{
			Card component = cards[comcards[i]].GetComponent<Card>();
			switch (component.suit)
			{
			case Card.Suit.SPADE:
				num5++;
				if (num9 == -1)
				{
					num9 = i;
				}
				min = i;
				break;
			case Card.Suit.CLUB:
				num6++;
				if (num10 == -1)
				{
					num10 = i;
				}
				min2 = i;
				break;
			case Card.Suit.DIAMOND:
				num7++;
				if (num11 == -1)
				{
					num11 = i;
				}
				min3 = i;
				break;
			case Card.Suit.HEART:
				num8++;
				if (num12 == -1)
				{
					num12 = i;
				}
				min4 = i;
				break;
			}
		}
		num = num5;
		num2 = num6;
		num3 = num7;
		num4 = num8;
		if (!userSuitAvailablity[Card.Suit.SPADE])
		{
			num5 = 0;
		}
		if (!userSuitAvailablity[Card.Suit.CLUB])
		{
			num6 = 0;
		}
		if (!userSuitAvailablity[Card.Suit.DIAMOND])
		{
			num7 = 0;
		}
		if (!userSuitAvailablity[Card.Suit.HEART])
		{
			num8 = 0;
		}
		if (num5 > num6 && num5 > num7 && num5 > num8)
		{
			int num13 = UnityEngine.Random.Range(min, num9 + 1);
			num9 = num13;
			compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num9]], computerCardStartLocation.position, Quaternion.identity));
			lastCompCard = cards[comcards[num9]].GetComponent<Card>();
			comcards.RemoveAt(num9);
			startAnimateCompCard();
		}
		else if (num6 > num5 && num6 > num7 && num6 > num8)
		{
			int num14 = UnityEngine.Random.Range(min2, num10 + 1);
			num10 = num14;
			compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num10]], computerCardStartLocation.position, Quaternion.identity));
			lastCompCard = cards[comcards[num10]].GetComponent<Card>();
			comcards.RemoveAt(num10);
			startAnimateCompCard();
		}
		else if (num7 > num5 && num7 > num6 && num7 > num8)
		{
			int num15 = UnityEngine.Random.Range(min3, num11 + 1);
			num11 = num15;
			compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num11]], computerCardStartLocation.position, Quaternion.identity));
			lastCompCard = cards[comcards[num11]].GetComponent<Card>();
			comcards.RemoveAt(num11);
			startAnimateCompCard();
		}
		else if (num8 > num5 && num8 > num6 && num8 > num7)
		{
			int num16 = UnityEngine.Random.Range(min4, num12 + 1);
			num12 = num16;
			compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num12]], computerCardStartLocation.position, Quaternion.identity));
			lastCompCard = cards[comcards[num12]].GetComponent<Card>();
			comcards.RemoveAt(num12);
			startAnimateCompCard();
		}
		else if (num8 == num5 && num5 == num6 && num6 == num7)
		{
			if (num8 == 0)
			{
				if (num != 0)
				{
					int num17 = UnityEngine.Random.Range(min, num9 + 1);
					num9 = num17;
				}
				else if (num2 != 0)
				{
					int num18 = UnityEngine.Random.Range(min2, num10 + 1);
					num9 = num18;
				}
				else if (num3 != 0)
				{
					int num19 = UnityEngine.Random.Range(min3, num11 + 1);
					num9 = num19;
				}
				else if (num4 != 0)
				{
					int num20 = UnityEngine.Random.Range(min4, num12 + 1);
					num9 = num20;
				}
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num9]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num9]].GetComponent<Card>();
				comcards.RemoveAt(num9);
				startAnimateCompCard();
			}
			else
			{
				int num21 = UnityEngine.Random.Range(min, num9 + 1);
				num9 = num21;
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num9]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num9]].GetComponent<Card>();
				comcards.RemoveAt(num9);
				startAnimateCompCard();
			}
		}
		else
		{
			int a = Mathf.Max(num5, num6);
			int b = Mathf.Max(num7, num8);
			int num22 = Mathf.Max(a, b);
			if (num22 == num5)
			{
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num9]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num9]].GetComponent<Card>();
				comcards.RemoveAt(num9);
			}
			else if (num22 == num6)
			{
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num10]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num10]].GetComponent<Card>();
				comcards.RemoveAt(num10);
			}
			else if (num22 == num7)
			{
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num11]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num11]].GetComponent<Card>();
				comcards.RemoveAt(num11);
			}
			else
			{
				compMovedCardGO = (thrownCard = UnityEngine.Object.Instantiate(cards[comcards[num12]], computerCardStartLocation.position, Quaternion.identity));
				lastCompCard = cards[comcards[num12]].GetComponent<Card>();
				comcards.RemoveAt(num12);
			}
			startAnimateCompCard();
		}
		updateCompCards();
	}

	private void moveCardsDown(GameObject parentContainer, Card pushedCard)
	{
		int childCount = parentContainer.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GameObject gameObject = parentContainer.transform.GetChild(i).gameObject;
			if (gameObject.GetComponent<Card>().value < pushedCard.value)
			{
				Vector3 localPosition = gameObject.transform.localPosition;
				localPosition.y -= 0.35f;
				if (localPosition.y < 0f)
				{
					localPosition.y = 0f;
				}
				gameObject.transform.localPosition = localPosition;
			}
		}
	}

	private int getCardIndexFromUserCards(Card usercard)
	{
		for (int i = 0; i < usercards.Count; i++)
		{
			Card component = cards[usercards[i]].GetComponent<Card>();
			if (component.suit == usercard.suit && component.value == usercard.value)
			{
				return i;
			}
		}
		return -1;
	}

	private int getCardIndex(Card thrownCard)
	{
		for (int i = 0; i < cards.Length; i++)
		{
			Card component = cards[i].GetComponent<Card>();
			if (component.suit == thrownCard.suit && component.value == thrownCard.value)
			{
				return i;
			}
		}
		return -1;
	}

	private void updateCompCards()
	{
		if (comcards.Count < defaultCards.Length)
		{
			for (int i = 0; i < defaultCards.Length; i++)
			{
				if (i + comcards.Count < defaultCards.Length)
				{
					defaultCards[i].SetActive(value: false);
				}
				else
				{
					defaultCards[i].SetActive(value: true);
				}
			}
		}
		else
		{
			GameObject[] array = defaultCards;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public void GoBackToMenu()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		SceneManager.LoadScene("menu");
	}

	public void showQuitDialog()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		quitDialog.SetActive(value: true);
	}

	public void CancelQuit()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		quitDialog.SetActive(value: false);
	}

	public void OKDealDialogClick()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		dealDialog.SetActive(value: false);
	}

	public void OKCardHelpClick()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		yourCardsPanel.SetActive(value: false);
		aceCardsPanel.SetActive(value: true);
	}

	public void OKCardAceHelpClick()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		shiftAceToRight();
		aceCardsPanel.SetActive(value: false);
		dealButtonPanel.SetActive(value: true);
	}

	private void shiftAceToRight()
	{
		if (lastCard == null)
		{
			GameObject gameObject = spadeContainer.transform.GetChild(0).gameObject;
			Card component = gameObject.GetComponent<Card>();
			if (component.suit == Card.Suit.SPADE && component.value == Card.CardValue.ACE)
			{
				shiftCard(gameObject);
			}
		}
	}

	public void OKDealButtonHelpClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		dealButtonPanel.SetActive(value: false);
	}

	public void NextButtonHelpClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		initialHelpPanel.SetActive(value: false);
		cardHelpPanel.SetActive(value: true);
	}

	public void BeginClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		cardHelpPanel.SetActive(value: false);
		AddCard();
	}

	public void UserTurnOkClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		userTurnPanel.SetActive(value: false);
		showPushControl();
	}

	public void HitHelpOkClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		hithelpPanel.SetActive(value: false);
	}

	public void OpponentHitClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		opponentHithelpPanel.SetActive(value: false);
	}

	public void showGameOverPanel()
	{
		gameOverPanel.SetActive(value: true);
		playerName.text = PlayerPrefs.GetString("name");
		winProfilePic.sprite = profilePic.sprite;
		LeaderBoardManager instance = LeaderBoardManager.getInstance();
		if (usercards.Count == 0)
		{
			winCoins.text = "250 coins";
			trophy.SetActive(value: true);
			xps.text = "200 XP";
			if (instance != null)
			{
				instance.UpdatePlayerPoints(instance.WIN_MATCH_XP);
			}
		}
		else
		{
			winCoins.text = "0 coins";
			trophy.SetActive(value: false);
			xps.text = "50 XP";
			if (instance != null)
			{
				instance.UpdatePlayerPoints(instance.WIN_MATCH_XP);
			}
		}
		Invoke("showAd", 1f);
	}

	public void showAd()
	{
		homeButton.enabled = true;
		retryButton.enabled = true;
		AdManager.getInstance().ShowInterstitial();
	}

	public void ReplayClicked()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		SceneManager.LoadScene("offline");
	}

	public void ShowNote()
	{
		dealMessage.text = "All cards are not distributed\n in a 2-player game";
		dealAnimator.SetTrigger("move");
	}
}
