using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Facebook.Unity;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarromGameManager : MonoBehaviourPun, IPunTurnManagerCallbacks
{
	private const int FIRST_PLAYER = 0;

	private const int SECOND_PLAYER = 1;

	public Color disabledColor;

	public Color enabledColor;

	public Sprite whiteCoin;

	public Sprite blackCoin;

	public GameObject strikerMoverUp;

	public GameObject strikerMoverDown;

	public Text leftScoreView;

	public Text rightScoreView;

	public Image playerCoin;

	public Image opponentCoin;

	public Image playerImageView;

	public Image opponentImageView;

	public Transform redCoinLeft;

	public Transform redCoinRight;

	public Transform gemLeft;

	public Transform gemRight;

	public GameObject gem;

	public GameObject gemAnimator;

	public GameObject backgroundPattern;

	public GameObject gemParticleObject;

	public Text playerNameView;

	public Text opponentNameView;

	public Text foulDetail;

	public Sprite[] avatars;

	private Text playerScoreView;

	private Text opponentScoreView;

	public Text dealMessage;

	public Text winnercoins;

	public Text snackbarMessage;

	public Text playerPuckMessage;

	public Text leftChatText;

	public Text rightChatText;

	public Text queenRecoverLeft;

	public Text queenRecoverRight;

	public static CarromGameManager Instance;

	public GameObject movementPadBottom;

	public GameObject movementPadTop;

	public GameObject gameOverPanel;

	public GameObject searchingPanel;

	public GameObject boardShadow;

	public GameObject gameOverParticle;

	public GameObject leftAvatar;

	public GameObject rightAvatar;

	public GameObject adRewardedPanel;

	public GameObject doubleCoins;

	public GameObject home;

	public GameObject share;

	public GameObject chatMessagePanel;

	public GameObject chatButton;

	public GameObject quitPanel;

	public GameObject blackPuck;

	public GameObject whitePuck;

	public GameObject redPuck;

	public Transform restPositionTop;

	public Transform restPositionBottom;

	public Animator dealAnimator;

	public Animator snackBarAnimator;

	public Animator foulAnimator;

	public Animator cameraAnimator;

	public Animator playerPuckAnimator;

	public Animator queenPuckAnimator;

	public Animator queenPuckAnimatorRight;

	public Animator chatAnimatorLeft;

	public Animator chatAnimatorRight;

	public CarromCoin[] puckPositions;

	public List<Puck> pucks = new List<Puck>();

	private Player player;

	private Player opponentPlayer;

	private Player playingPlayer;

	private StrikerAnimator strikerAnimator;

	public bool isPlayerTurn = true;

	private bool strikedAnyPucks;

	private PunTurnManager turnManager;

	private float TURN_DELAY = 30f;

	private Image currentLoader;

	public Image playerLoader;

	public Image opponentLoader;

	public PuckColor.Color playerColor;

	public PuckColor.Color opponentColor;

	private List<GoaledPuck> goaledColors;

	private StrikerMover downStrikerMover;

	private StrikerMover upStrikerMover;

	public trajectoryScript strikerScript;

	public Button ShareButton;

	public Button GOShareButton;

	public float OPPONENT_WAITING_TIME = 5f;

	private bool deductedCoins;

	private int wonCoins;

	private bool isPlayerStriked = true;

	private bool IsPlayerWon;

	public bool timesup = false;

	public bool isGameOver;

	private int playerScore;

	private int opponentScore;

	private bool playerPutQueen;

	private int winscore;

	private Vector3 redpuckposition;

	private GameObject queenPuck;

	private bool queenAcquired;

	public Text rewardMessage;

	public bool showCustomMessages;

	public InputField messageBox;

	public Button sendMessageButton;

	private bool IsGameStarted;

	public Text quitMessage;

	public Text quitSubMessage;

	public Text quitButton;

	private void Start()
	{
		downStrikerMover = strikerMoverDown.GetComponent<StrikerMover>();
		upStrikerMover = strikerMoverUp.GetComponent<StrikerMover>();
		SetWinScore((puckPositions.Length - 1) / 2);
		Instance = this;
		goaledColors = new List<GoaledPuck>();
		SetPlayerImage();
		playerNameView.text = PlayerPrefs.GetString("name", string.Empty);
		OPPONENT_WAITING_TIME = LevelManager.getInstance().opponentWaitingTime;
	}

	private void Update()
	{
		if (turnManager != null)
		{

			if (timesup && currentLoader.fillAmount<0.01f)
            {
				currentLoader.fillAmount = 0f;
				turnManager.BeginTurn();
			}
            else
            {
				currentLoader.fillAmount = this.turnManager.RemainingSecondsInTurn / this.turnManager.TurnDuration;
			}

		}
	}

	private void AddTunrManager()
	{
		turnManager = base.gameObject.AddComponent<PunTurnManager>();
		turnManager.TurnManagerListener = this;
		TURN_DELAY = LevelManager.getInstance().turnTime;
		LogFeedback("Turn time " + TURN_DELAY);
		turnManager.TurnDuration = TURN_DELAY;
	}

	public void SetStrikerAnimator(StrikerAnimator strikerAnimator)
	{
		this.strikerAnimator = strikerAnimator;
	}

	public void OpponentJoined()
	{
		searchingPanel.SetActive(value: false);
		Invoke("WaitingForOpponent", OPPONENT_WAITING_TIME);
	}

	public void StartGame(Player newPlayer)
	{
		opponentPlayer = newPlayer;
		int num = UnityEngine.Random.Range(0, 2);
		InstantiatePucks();
		if (num == 0)
		{
			playingPlayer = PhotonNetwork.MasterClient;
		}
		else
		{
			playingPlayer = newPlayer;
			ChangeOwnerShip(newPlayer);
		}
		Invoke("WaitingForOpponent", OPPONENT_WAITING_TIME);
		base.photonView.RPC("BeginGame", RpcTarget.All, num);
		base.photonView.RPC("PlayerTurn", playingPlayer, null);
	}

	private void ChangeOwnerShip(Player newPlayer)
	{
		foreach (Puck puck in pucks)
		{
			puck?.photonView.TransferOwnership(newPlayer);
		}
	}

	[PunRPC]
	private void BeginGame(int playetToPlay)
	{
		IsGameStarted = true;
		chatButton.SetActive(value: true);
		searchingPanel.SetActive(value: false);
		CancelInvoke("WaitingForOpponent");
		if (PhotonNetwork.IsMasterClient)
		{
			player = PhotonNetwork.MasterClient;
			isPlayerTurn = ((playetToPlay == 0) ? true : false);
			playerColor = ((playetToPlay == 0) ? PuckColor.Color.WHITE : PuckColor.Color.BLACK);
			currentLoader = ((playetToPlay != 0) ? opponentLoader : playerLoader);
			playerCoin.sprite = ((playerColor != PuckColor.Color.WHITE) ? blackCoin : whiteCoin);
			opponentCoin.sprite = ((playerColor != PuckColor.Color.WHITE) ? whiteCoin : blackCoin);
			opponentColor = ((playerColor != PuckColor.Color.WHITE) ? PuckColor.Color.WHITE : PuckColor.Color.BLACK);
			playingPlayer = ((playetToPlay != 0) ? opponentPlayer : PhotonNetwork.MasterClient);
			playerScoreView = leftScoreView;
			opponentScoreView = rightScoreView;
			movementPadBottom.SetActive(value: true);
			ShowPlayerPuck("Collect " + playerColor + " coins");
		}
		else
		{
			player = PhotonNetwork.LocalPlayer;
			isPlayerTurn = ((playetToPlay == 1) ? true : false);
			playingPlayer = ((playetToPlay != 1) ? PhotonNetwork.MasterClient : PhotonNetwork.LocalPlayer);
			currentLoader = ((playetToPlay != 1) ? opponentLoader : playerLoader);
			playerColor = ((playetToPlay == 1) ? PuckColor.Color.WHITE : PuckColor.Color.BLACK);
			playerCoin.sprite = ((playerColor != PuckColor.Color.WHITE) ? blackCoin : whiteCoin);
			opponentCoin.sprite = ((playerColor != PuckColor.Color.WHITE) ? whiteCoin : blackCoin);
			opponentColor = ((playerColor != PuckColor.Color.WHITE) ? PuckColor.Color.WHITE : PuckColor.Color.BLACK);
			opponentPlayer = PhotonNetwork.MasterClient;
			movementPadTop.SetActive(value: true);
			playerScoreView = leftScoreView;
			opponentScoreView = rightScoreView;
			InstantiateOpponetStriker();
			ShowPlayerPuck("Collect " + playerColor + " coins");
			backgroundPattern.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		playerCoin.enabled = true;
		opponentCoin.enabled = true;
		playerNameView.text = player.NickName;
		opponentNameView.text = opponentPlayer.NickName;
		leftScoreView.text = "0";
		rightScoreView.text = "0";
		SetOpponentImage();
		DeductBetCoins();
		AddTunrManager();
	}

	private void DeductBetCoins()
	{
		deductedCoins = true;
		int @int = PlayerPrefs.GetInt("coins", FacebookLogin.DEFAULT_COINS);
		int gameBet = (int)LevelManager.getInstance().gameBet;
		if (@int >= gameBet)
		{
			@int -= gameBet;
			PlayerPrefs.SetInt("coins", @int);
			LeaderBoardManager instance = LeaderBoardManager.getInstance();
			if (instance != null)
			{
				instance.UpdatePlayerCoins(@int);
			}
		}
	}

	private void RewardPlayer(int coins)
	{
		if (!deductedCoins)
		{
			return;
		}
		LevelManager instance = LevelManager.instance;
		if (instance.gameMode == LevelManager.GameMode.MULTIPLAYER)
		{
			wonCoins = coins;
			int @int = PlayerPrefs.GetInt("coins", FacebookLogin.DEFAULT_COINS);
			@int += coins;
			PlayerPrefs.SetInt("coins", @int);
			LeaderBoardManager instance2 = LeaderBoardManager.getInstance();
			if (instance2 != null)
			{
				instance2.UpdatePlayerCoins(@int);
			}
		}
	}

	private void InstantiatePucks()
	{
		CarromCoin[] array = puckPositions;
		GameObject gameObject;
		foreach (CarromCoin carromCoin in array)
		{
			gameObject = PhotonNetwork.Instantiate(carromCoin.prefab.name, carromCoin.prefab.transform.position, Quaternion.identity, 0);
		}
		gameObject = PhotonNetwork.Instantiate("Ball", restPositionBottom.position, Quaternion.identity, 0);
		strikerScript = gameObject.GetComponent<trajectoryScript>();
		strikerAnimator = gameObject.GetComponent<StrikerAnimator>();
		SetStrikerAnimator(strikerAnimator);
		gameObject.GetComponent<CircleCollider2D>().enabled = true;
		downStrikerMover.SetStriker(gameObject, (int)PhotonNetwork.LocalPlayer.CustomProperties["striker"]);
		gameObject.transform.GetChild(0).name = "BallMaster";
		gameObject = PhotonNetwork.Instantiate("Trajectory Dots", new Vector3(8.803918f, -2.840898f, 0f), Quaternion.identity, 0);
		gameObject.name = "Trajectory Dots";
	}

	private void InstantiateOpponetStriker()
	{
		GameObject gameObject = PhotonNetwork.Instantiate("Ball", restPositionTop.position, Quaternion.Euler(0f, 0f, 180f), 0);
		strikerScript = gameObject.GetComponent<trajectoryScript>();
		gameObject.GetComponent<CircleCollider2D>().enabled = true;
		strikerAnimator = gameObject.GetComponent<StrikerAnimator>();
		SetStrikerAnimator(strikerAnimator);
		upStrikerMover.SetStriker(gameObject, (int)PhotonNetwork.LocalPlayer.CustomProperties["striker"]);
		gameObject.transform.GetChild(0).name = "BallMaster";
		gameObject = PhotonNetwork.Instantiate("Trajectory Dots", new Vector3(8.803918f, 2.840898f, 0f), Quaternion.identity, 0);
		gameObject.name = "Trajectory Dots";
		Camera.main.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
		boardShadow.transform.position = new Vector3(0f, 0.2f, 0f);
	}

	private void SetOpponentImage()
	{
		opponentImageView.enabled = true;
		setProfilePicture(opponentPlayer, opponentImageView);
	}

	private void SetPlayerImage()
	{
		Sprite playerImage = LevelManager.instance.GetPlayerImage();
		if (playerImage != null)
		{
			playerImageView.sprite = playerImage;
		}
		else
		{
			setProfilePicture(PhotonNetwork.LocalPlayer, playerImageView);
		}
	}

	[PunRPC]
	public void PlayerTurn()
	{
		base.photonView.RPC("BeginNewRound", RpcTarget.All, null);
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_PLAYER_TURN);
		EnableStrikerHandler();
		strikerAnimator.photonView.RPC("MoveStrikerIn", RpcTarget.All, (!PhotonNetwork.IsMasterClient) ? new Vector3(0f, 1.741f, 0f) : new Vector3(0f, -1.741f, 0f));
		RoundBegin();
	}

	public void RoundBegin()
	{
		strikedAnyPucks = false;
		isPlayerTurn = true;
		isPlayerStriked = false;
		timesup = false;
		strikerScript.MakeStrikerStrike();
		if (turnManager != null)
		{
			turnManager.BeginTurn();
		}
	}

	public void FinishTurn()
	{
		isPlayerStriked = true;
		timesup = true;
		Invoke("WaitingForOpponent", 2f * OPPONENT_WAITING_TIME);
	}

	public void BeginWaitForOpponent()
	{
		Invoke("WaitingForOpponent", 2f * OPPONENT_WAITING_TIME);
	}

	[PunRPC]
	private void MakeChangesToOtherPlayer()
	{
	}

	[PunRPC]
	private void MakeChangesToGameBeginner()
	{
	}

	[PunRPC]
	private void HandleOwnerShip()
	{
		Player player = (playingPlayer != PhotonNetwork.MasterClient) ? PhotonNetwork.MasterClient : opponentPlayer;
		ChangeOwnerShip(player);
		playingPlayer = player;
		base.photonView.RPC("PlayerTurn", player, null);
	}

	[PunRPC]
	public void BeginNewRound()
	{
		dealAnimator.SetBool("show", value: false);
		CancelInvoke("WaitingForOpponent");
	}

	[PunRPC]
	public void OpponentScoredPoint(int opponnentId, int score, Vector3 position, PuckColor.Color puckcolor)
	{
		UnityEngine.Debug.LogError("OpponentScoredPoint");
		GameObject gameObject = null;
		gameObject = ((puckcolor != 0) ? UnityEngine.Object.Instantiate(whitePuck, position, Quaternion.identity) : UnityEngine.Object.Instantiate(blackPuck, position, Quaternion.identity));
		if (opponnentId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			gameObject.GetComponent<PuckAnimator>().AnimatePuckToScore(Camera.main.ScreenToWorldPoint(playerCoin.transform.position));
		}
		else
		{
			gameObject.GetComponent<PuckAnimator>().AnimatePuckToScore(Camera.main.ScreenToWorldPoint(opponentCoin.transform.position));
		}
		if (opponnentId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			playerScore = score;
		}
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_POINT_SCORED);
		Text text = (opponnentId == PhotonNetwork.LocalPlayer.ActorNumber) ? playerScoreView : opponentScoreView;
		text.text = score.ToString();
	}

	[PunRPC]
	public void PlayerScoredPoint(int playerId, int score, Vector3 position, PuckColor.Color puckcolor)
	{
		GameObject gameObject = null;
		gameObject = ((puckcolor != 0) ? UnityEngine.Object.Instantiate(whitePuck, position, Quaternion.identity) : UnityEngine.Object.Instantiate(blackPuck, position, Quaternion.identity));
		if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			gameObject.GetComponent<PuckAnimator>().AnimatePuckToScore(Camera.main.ScreenToWorldPoint(playerCoin.transform.position));
		}
		else
		{
			gameObject.GetComponent<PuckAnimator>().AnimatePuckToScore(Camera.main.ScreenToWorldPoint(opponentCoin.transform.position));
		}
		if (playerId != PhotonNetwork.LocalPlayer.ActorNumber)
		{
			opponentScore = score;
		}
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_POINT_SCORED);
		Text text = (playerId != PhotonNetwork.LocalPlayer.ActorNumber) ? opponentScoreView : playerScoreView;
		text.text = score.ToString();
	}

	[PunRPC]
	public void GameOver(int playerId)
	{
		if (isGameOver)
		{
			return;
		}
		gameOverPanel.SetActive(value: true);
		isGameOver = true;
		IsPlayerWon = (playerId == PhotonNetwork.LocalPlayer.ActorNumber);
		if (IsPlayerWon)
		{
			gameOverParticle.SetActive(value: true);
			AudioManager.getInstance().PlaySound(AudioManager.GAME_OVER);
			leftAvatar.GetComponent<GameOverAnimator>().AnimateWinner();
			rightAvatar.GetComponent<GameOverAnimator>().AnimateLooser();
			RewardPlayer((int)LevelManager.getInstance().gameBet * 2);
			LeaderBoardManager instance = LeaderBoardManager.getInstance();
			if (instance != null)
			{
				instance.UpdatePlayerPoints(instance.WIN_MATCH_XP);
			}
		}
		else
		{
			gameOverParticle.SetActive(value: true);
			AudioManager.getInstance().PlaySound(AudioManager.GAME_OVER);
			rightAvatar.GetComponent<GameOverAnimator>().AnimateWinner();
			leftAvatar.GetComponent<GameOverAnimator>().AnimateLooser();
			LeaderBoardManager instance2 = LeaderBoardManager.getInstance();
			if (instance2 != null)
			{
				instance2.UpdatePlayerPoints(instance2.LOST_MATCH_XP);
			}
		}
		turnManager = null;
		if (currentLoader != null)
		{
			currentLoader.fillAmount = 1f;
		}
	}

	[PunRPC]
	public void PlayerLostPoint(int playerId, int score)
	{
		if (playerId != PhotonNetwork.LocalPlayer.ActorNumber)
		{
			opponentScore = score;
		}
		Text text = (playerId != PhotonNetwork.LocalPlayer.ActorNumber) ? opponentScoreView : playerScoreView;
		text.text = score.ToString();
	}

	[PunRPC]
	public void AnimatePuck(PuckAnimator animator)
	{
	}

	private void MoveStrikerHandlerToCenter()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			downStrikerMover.AnimateStrikerHandlerToCenter();
		}
		else
		{
			upStrikerMover.AnimateStrikerHandlerToCenter();
		}
	}

	private Foul IsFoul()
	{
		int num = 0;
		int num2 = 0;
		foreach (Puck puck in pucks)
		{
			if (puck.photonView != null)
			{
				if (puck.puckcolor.suit == playerColor)
				{
					num++;
				}
				if (puck.puckcolor.suit == opponentColor)
				{
					num2++;
				}
			}
		}
		foulDetail.text = string.Empty;
		foreach (GoaledPuck goaledColor in goaledColors)
		{
			if (goaledColor.color == PuckColor.Color.STRIKER_COLOR)
			{
				foulDetail.text = "You cannot put the Striker";
				return new Foul(num, num2, isFoul: true, "Foul");
			}
		}
		if (num == 1 || num2 == 1)
		{
			int num3 = 0;
			int num4 = 0;
			foreach (GoaledPuck goaledColor2 in goaledColors)
			{
				if (goaledColor2.color == playerColor)
				{
					num3++;
				}
				else if (goaledColor2.color == opponentColor)
				{
					num4++;
				}
			}
			if ((!queenAcquired || IsQueenGoaled()) && num2 == 1 && num4 == 1)
			{
				foulDetail.text = "Putting opponent's last puck when\n Red is not recovered is Foul";
				return new Foul(num, num2, isFoul: true, "Plotting opponent's last puck when\n Red is not recovered is Foul");
			}
			if (IsQueenPresent() && !IsQueenGoaled() && num == 1 && num3 == 1)
			{
				foulDetail.text = "First Put Red Puck";
				return new Foul(num, num2, isFoul: true, "First Put Queen(Red) Puck");
			}
			if (IsQueenPresent() && !IsQueenGoaled() && num2 == 1 && num4 == 1)
			{
				foulDetail.text = "First Put Red Puck";
				return new Foul(num, num2, isFoul: true, "First Put Queen(Red) Puck");
			}
		}
		return new Foul(num, num2, isFoul: false, string.Empty);
	}

	private bool IsQueenPresent()
	{
		foreach (Puck puck in pucks)
		{
			if (puck.photonView != null && puck.puckcolor.suit == PuckColor.Color.RED)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsQueenGoaled()
	{
		foreach (GoaledPuck goaledColor in goaledColors)
		{
			if (goaledColor.color == PuckColor.Color.RED)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasStrikedAnyPucksToHole()
	{
		return goaledColors.Count > 0;
	}

	public void SetWinScore(int score)
	{
		winscore = score;
	}

	private bool IsGameOver()
	{
		if (playerScore >= winscore)
		{
			base.photonView.RPC("GameOver", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
			return true;
		}
		if (opponentScore >= winscore)
		{
			base.photonView.RPC("GameOver", RpcTarget.All, opponentPlayer.ActorNumber);
			return true;
		}
		return false;
	}

	private void ShowSnackBar(string message)
	{
		snackbarMessage.text = message;
		snackBarAnimator.SetTrigger("show");
	}

	private void ShowPlayerPuck(string message)
	{
		playerPuckAnimator.gameObject.SetActive(value: true);
		playerPuckMessage.text = message;
		playerPuckAnimator.SetTrigger("show");
	}

	[PunRPC]
	public void ShowFoulMessage(string message)
	{
		foulDetail.text = message;
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_FOUL);
		foulAnimator.SetTrigger("foul");
	}

	[PunRPC]
	public void HideWaitingMessage()
	{
		dealAnimator.SetBool("show", value: false);
		CancelInvoke("WaitingForOpponent");
	}

	public void RoundComplete()
	{
		base.photonView.RPC("HideWaitingMessage", RpcTarget.All, null);
		if (isGameOver)
		{
			return;
		}
		Foul foul = IsFoul();
		if (foul.isFoul)
		{
			base.photonView.RPC("ShowFoulMessage", RpcTarget.All, foulDetail.text);
			if (playerPutQueen)
			{
				RespawnQueen();
			}
			foreach (GoaledPuck goaledColor in goaledColors)
			{
				if (goaledColor.color != PuckColor.Color.STRIKER_COLOR)
				{
					if (goaledColor.color == playerColor || goaledColor.color == PuckColor.Color.RED)
					{
						goaledColor.gameObject.GetComponent<PhotonView>().RPC("AnimatePuck", RpcTarget.All, Vector3.zero);
					}
					else if (foul.opponentCoinsCount == 1 && IsQueenPresent())
					{
						goaledColor.gameObject.GetComponent<PhotonView>().RPC("AnimatePuck", RpcTarget.All, Vector3.zero);
					}
					else
					{
						opponentScore++;
						base.photonView.RPC("OpponentScoredPoint", RpcTarget.All, opponentPlayer.ActorNumber, opponentScore, goaledColor.gameObject.transform.position, goaledColor.color);
						DestroyPuck(goaledColor.gameObject);
					}
				}
			}
			int num = playerScore;
			if (num > 0)
			{
				PuckAnimator puckAnimator = (playerColor != 0) ? PhotonNetwork.Instantiate("White", (!PhotonNetwork.IsMasterClient) ? strikerMoverUp.transform.position : strikerMoverDown.transform.position, Quaternion.identity, 0).GetComponent<PuckAnimator>() : PhotonNetwork.Instantiate("Black", (!PhotonNetwork.IsMasterClient) ? strikerMoverUp.transform.position : strikerMoverDown.transform.position, Quaternion.identity, 0).GetComponent<PuckAnimator>();
				puckAnimator.photonView.RPC("AnimatePuck", RpcTarget.All, Vector3.zero);
				playerScore--;
				base.photonView.RPC("PlayerLostPoint", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, playerScore);
			}
			if (!IsGameOver())
			{
				goaledColors.Clear();
				Invoke("NextPlayerTurn", 2f);
			}
			return;
		}
		strikedAnyPucks = HasStrikedAnyPucksToHole();
		if (strikedAnyPucks)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (GoaledPuck goaledColor2 in goaledColors)
			{
				if (goaledColor2.color == playerColor)
				{
					flag = true;
					playerScore++;
					base.photonView.RPC("PlayerScoredPoint", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, playerScore, goaledColor2.gameObject.transform.position, goaledColor2.color);
					DestroyPuck(goaledColor2.gameObject);
				}
				else if (goaledColor2.color == PuckColor.Color.RED)
				{
					playerPutQueen = true;
					flag2 = true;
					redpuckposition = goaledColor2.gameObject.transform.position;
					base.photonView.RPC("AnimateQueenPuck", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, goaledColor2.gameObject.transform.position);
					DestroyPuck(goaledColor2.gameObject);
				}
				else
				{
					opponentScore++;
					base.photonView.RPC("OpponentScoredPoint", RpcTarget.All, opponentPlayer.ActorNumber, opponentScore, goaledColor2.gameObject.transform.position, goaledColor2.color);
					DestroyPuck(goaledColor2.gameObject);
				}
			}
			MoveStrikerHandlerToCenter();
			if (flag && playerPutQueen)
			{
				playerPutQueen = false;
				SynchPucks();
				RoundBegin();
				strikerAnimator.photonView.RPC("MoveBackStriker", RpcTarget.All, (!PhotonNetwork.IsMasterClient) ? new Vector3(0f, 1.741f, 0f) : new Vector3(0f, -1.741f, 0f));
				base.photonView.RPC("QueenAcquired", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
			}
			else if (flag || flag2)
			{
				SynchPucks();
				RoundBegin();
				strikerAnimator.photonView.RPC("MoveBackStriker", RpcTarget.All, (!PhotonNetwork.IsMasterClient) ? new Vector3(0f, 1.741f, 0f) : new Vector3(0f, -1.741f, 0f));
			}
			else
			{
				if (playerPutQueen)
				{
					RespawnQueen();
				}
				Invoke("NextPlayerTurn", 2f);
			}
			if (IsGameOver())
			{
				return;
			}
		}
		else if (playerPutQueen)
		{
			RespawnQueen();
			Invoke("NextPlayerTurn", 2f);
		}
		else
		{
			NextPlayerTurn();
		}
		goaledColors.Clear();
	}

	[PunRPC]
	public void AnimateQueenPuck(int playerId, Vector3 position)
	{
		queenPuck = UnityEngine.Object.Instantiate(redPuck, position, Quaternion.identity);
		if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			queenPuck.GetComponent<PuckAnimator>().AnimateQueen(Camera.main.ScreenToWorldPoint(redCoinLeft.position));
		}
		else
		{
			queenPuck.GetComponent<PuckAnimator>().AnimateQueen(Camera.main.ScreenToWorldPoint(redCoinRight.position));
		}
	}

	[PunRPC]
	public void QueenAcquired(int playerId)
	{
		queenAcquired = true;
		if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			UpdatePlayerGems();
		}
		AnimateGem(playerId);
		if (queenPuck != null)
		{
			queenPuck.GetComponent<SpriteRenderer>().color = Color.white;
			if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
			{
				queenRecoverLeft.text = "Red recovered";
				queenPuckAnimator.gameObject.SetActive(value: true);
				queenPuckAnimator.SetTrigger("show");
			}
			else
			{
				queenRecoverRight.text = "Red recovered";
				queenPuckAnimatorRight.gameObject.SetActive(value: true);
				queenPuckAnimatorRight.SetTrigger("show");
			}
		}
	}

	private void AnimateGem(int playerId)
	{
		GameObject gameObject;
		GameObject gameObject2;
		if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			gameObject = UnityEngine.Object.Instantiate(gemParticleObject, Camera.main.ScreenToWorldPoint(gemLeft.position), Quaternion.identity);
			Vector3 position = Camera.main.ScreenToWorldPoint(gemLeft.position);
			position.z = 0f;
			gameObject2 = UnityEngine.Object.Instantiate(gem, position, Quaternion.identity);
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(gemParticleObject, Camera.main.ScreenToWorldPoint(gemRight.position), Quaternion.identity);
			Vector3 position2 = Camera.main.ScreenToWorldPoint(gemRight.position);
			position2.z = 0f;
			gameObject2 = UnityEngine.Object.Instantiate(gem, position2, Quaternion.identity);
		}
		if (!PhotonNetwork.IsMasterClient)
		{
			//gemAnimator.GetComponent<ParticleSystem>().main.gravityModifierMultiplier = -0.5f;
			//gameObject.GetComponent<ParticleSystem>().main.gravityModifierMultiplier = -0.5f;
			gameObject2.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_GEM_AQUIRED);
	}

	private void UpdatePlayerGems()
	{
		int @int = PlayerPrefs.GetInt("gems", 0);
		@int++;
		PlayerPrefs.SetInt("gems", @int);
	}

	[PunRPC]
	public void QueenNotAcquired(int playerId)
	{
		if (queenPuck != null)
		{
			queenPuck.GetComponent<SpriteRenderer>().color = Color.white;
			if (playerId == PhotonNetwork.LocalPlayer.ActorNumber)
			{
				queenRecoverLeft.text = "Red not recovered";
				queenPuckAnimator.gameObject.SetActive(value: true);
				queenPuckAnimator.SetTrigger("show");
			}
			else
			{
				queenRecoverRight.text = "Red not recovered";
				queenPuckAnimatorRight.gameObject.SetActive(value: true);
				queenPuckAnimatorRight.SetTrigger("show");
			}
		}
	}

	private void RespawnQueen()
	{
		playerPutQueen = false;
		PuckAnimator component = PhotonNetwork.Instantiate("Red", (!PhotonNetwork.IsMasterClient) ? strikerMoverUp.transform.position : strikerMoverDown.transform.position, Quaternion.identity, 0).GetComponent<PuckAnimator>();
		component.photonView.RPC("AnimatePuck", RpcTarget.All, Vector3.zero);
		base.photonView.RPC("QueenNotAcquired", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
		base.photonView.RPC("DestroyQueen", RpcTarget.All, null);
	}

	[PunRPC]
	public void DestroyQueen()
	{
		if (queenPuck != null)
		{
			UnityEngine.Object.Destroy(queenPuck);
		}
	}

	public void DestroyPuck(GameObject puck)
	{
		if (Instance.isPlayerTurn)
		{
			PhotonNetwork.Destroy(puck);
		}
	}

	private void NextPlayerTurn()
	{
		disableStrikerMover();
		strikedAnyPucks = false;
		isPlayerTurn = false;
		SynchPucks();
		strikerAnimator.photonView.RPC("MoveStrikerOut", RpcTarget.All, (!PhotonNetwork.IsMasterClient) ? restPositionTop.position : restPositionBottom.position);
		MoveStrikerHandlerToCenter();
		base.photonView.RPC("HandleOwnerShip", PhotonNetwork.MasterClient, null);
		base.photonView.RPC("UpdateLoader", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
		Invoke("WaitingForOpponent", OPPONENT_WAITING_TIME);
	}

	public void WaitingForOpponent()
	{
		dealMessage.text = "Please wait...Waiting for opponent";
		dealAnimator.SetBool("show", value: true);
	}

	private void SynchPucks()
	{
		foreach (Puck puck in pucks)
		{
			if (puck.photonView != null)
			{
				puck.photonView.RPC("SyncPuck", RpcTarget.Others, puck.photonView.transform.position, puck.photonView.transform.rotation);
			}
		}
	}

	[PunRPC]
	public void UpdateLoader(int currentPlayer)
	{
		currentLoader.fillAmount = 1f;
		if (currentPlayer != PhotonNetwork.LocalPlayer.ActorNumber)
		{
			currentLoader = playerLoader;
		}
		else
		{
			currentLoader = opponentLoader;
		}
	}

	private void disableStrikerMover()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			strikerMoverDown.GetComponent<SpriteRenderer>().color = disabledColor;
		}
		else
		{
			strikerMoverUp.GetComponent<SpriteRenderer>().color = disabledColor;
		}
	}

	private void EnableStrikerHandler()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			strikerMoverDown.GetComponent<SpriteRenderer>().color = enabledColor;
		}
		else
		{
			strikerMoverUp.GetComponent<SpriteRenderer>().color = enabledColor;
		}
	}

	public void SetScorePoint()
	{
	}

	public void SetFoul()
	{
	}

	public void GoaledColor(GoaledPuck puck)
	{
		goaledColors.Add(puck);
	}

	private void LogFeedback(string message)
	{
	}

	public void OnTurnBegins(int turn)
	{
	}

	public void OnTurnCompleted(int turn)
	{
	}

	public void OnPlayerMove(Player player, int turn, object move)
	{
	}

	public void OnPlayerFinished(Player player, int turn, object move)
	{
	}

	public void OnTurnTimeEnds(int turn)
	{
		if (!isPlayerStriked)
		{
			isPlayerTurn = false;
			isPlayerStriked = true;
			timesup = false;
			RoundComplete();
			turnManager.BeginTurn();
		}
	}

	public void PlayOffline()
	{
		AudioManager.getInstance().StopSound(AudioManager.PLAY_TICK);
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		SceneManager.LoadScene("CarromOffline");
	}

	public void LeaveAndGoHome()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		AudioManager.getInstance().StopSound(AudioManager.GAME_OVER);
		PhotonNetwork.LeaveRoom();
		GoBackToMenu();
	}

	private void GoBackToMenu()
	{
		if (isGameOver && IsPlayerWon)
		{
			LevelManager.getInstance().showRateUs();
		}
		AudioManager.getInstance().StopSound(AudioManager.PLAY_TICK);
		SceneManager.LoadScene("menu");
		AdManager.getInstance().ShowInterstitial();
		AdManager.getInstance().showAd();
	}

	private void setProfilePicture(Player player, Image profile)
	{
		if (player == null)
		{
			return;
		}
		ExitGames.Client.Photon.Hashtable customProperties = player.CustomProperties;
		string text = (string)customProperties["pic"];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (text.Contains("Empty"))
		{
			string[] array = text.Split(':');
			int num = int.Parse(array[1]);
			if (num >= 0 && num < avatars.Length)
			{
				profile.sprite = avatars[num];
				profile.gameObject.SetActive(value: true);
			}
		}
		else
		{
			loadFacebookProfile(text, profile);
		}
	}

	public void loadFacebookProfile(string userId, Image pic)
	{
		FB.API("/" + userId + "/picture?type=square&width=100&height=100&redirect=false", HttpMethod.GET, delegate(IGraphResult result)
		{
			if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
			{
				IDictionary dictionary = result.ResultDictionary["data"] as IDictionary;
				string url = dictionary["url"] as string;
				StartCoroutine(fetchProfilePic(url, pic));
			}
		});
	}

	public IEnumerator fetchProfilePic(string url, Image pic)
	{
		WWW www = new WWW(url);
		yield return www;
		pic.gameObject.SetActive(value: true);
		pic.sprite = Sprite.Create(www.texture, new Rect(0f, 0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
	}

	public void ShareScore()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		ShareManager.insatnce.ShareRoomId("I earned " + wonCoins + " coins.I'm playing the most popular family game - Carrom Master.\nDownload today and join me!\n" + FacebookLogin.SHARE_URL);
	}

	public void showDoubledCoins(int xp)
	{
		int num = (int)LevelManager.getInstance().gameBet * 2 * 2;
		int @int = PlayerPrefs.GetInt("coins", 10000);
		@int += num;
		PlayerPrefs.SetInt("coins", @int);
		LeaderBoardManager instance = LeaderBoardManager.getInstance();
		if (instance != null)
		{
			instance.UpdatePlayerCoins(@int);
		}
		showCoinRewardPanel(num, xp);
	}

	public void showCoinRewardPanel(int coins, int xp)
	{
		adRewardedPanel.SetActive(value: true);
		rewardMessage.text = "You have received " + coins + " coins.";
		Invoke("PlayRewardedAudio", 1f);
	}

	private void PlayRewardedAudio()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_COIN_COLLECT);
	}

	public void CurtainClosed()
	{
		if (AdManager.getInstance().IsRewardedVideoAvailable() && IsPlayerWon)
		{
			int @int = PlayerPrefs.GetInt("double_coin", 100);
			if (Mathf.Abs(@int - DateTime.Now.Day) > 1)
			{
				doubleCoins.SetActive(value: true);
			}
		}
		winnercoins.gameObject.SetActive(value: true);
		home.SetActive(value: true);
		share.SetActive(value: true);
		winnercoins.text = MenuManager.getRepresentationcoins((int)LevelManager.getInstance().gameBet * 2) + " coins";
	}

	public void OnDoubleCoinsClicked()
	{
		PlayerPrefs.SetInt("double_coin", DateTime.Now.Day);
		doubleCoins.SetActive(value: false);
		AudioManager.getInstance().StopSound(AudioManager.GAME_OVER);
		AdManager.getInstance().showRewardedVideoToDoubleCoins();
	}

	public void OnCloseRewardedPanel()
	{
		adRewardedPanel.SetActive(value: false);
	}

	public void sendChatMessage()
	{
		if (!string.IsNullOrEmpty(messageBox.text))
		{
			chatMessagePanel.SetActive(value: false);
			AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
			base.photonView.RPC("ShowMessage", RpcTarget.All, messageBox.text, PhotonNetwork.LocalPlayer.ActorNumber);
			messageBox.text = string.Empty;
		}
	}

	public void sendChatMessage(Text button)
	{
		chatMessagePanel.SetActive(value: false);
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		base.photonView.RPC("ShowMessage", RpcTarget.All, button.text, PhotonNetwork.LocalPlayer.ActorNumber);
	}

	[PunRPC]
	public void ShowMessage(string message, int senderID)
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CHAT);
		if (senderID == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			leftChatText.text = message;
			chatAnimatorLeft.SetTrigger("chat");
		}
		else
		{
			rightChatText.text = message;
			chatAnimatorRight.SetTrigger("chat");
		}
	}

	public void OpenChatWindow()
	{
		LevelManager instance = LevelManager.getInstance();
		if (instance != null && instance.gameMode == LevelManager.GameMode.MULTIPLAYER && !showCustomMessages)
		{
			messageBox.gameObject.SetActive(value: false);
			sendMessageButton.gameObject.SetActive(value: false);
		}
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		chatMessagePanel.SetActive(value: true);
	}

	public void CloseChatWindow()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		chatMessagePanel.SetActive(value: false);
	}

	public void ShowQuitDialog()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		quitPanel.SetActive(value: true);
		QuitGame();
	}

	public void QuitGame()
	{
		if (!IsGameStarted)
		{
			quitMessage.text = "Do you want to quit the game?";
			quitSubMessage.text = string.Empty;
			quitButton.text = "Quit";
		}
		else
		{
			quitMessage.text = "Do you want to Give Up?";
			quitSubMessage.text = "You will lose " + (int)LevelManager.getInstance().gameBet + " bet coins";
			quitButton.text = "Give Up";
		}
	}

	public void HideQuitDialog()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		quitPanel.SetActive(value: false);
	}

	public void ShareMatchCode()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		ShareManager.insatnce.ShareRoomId("Join me for a round of Carrom game using the PRIVATE MATCH code " + PhotonNetwork.CurrentRoom.Name + ".\nClick on the link to download this awesome,fun game CARROM MASTER!\n" + FacebookLogin.SHARE_URL);
	}

	public void ShareMatchCodeFacebook()
	{
		AudioManager.getInstance().PlaySound(AudioManager.PLAY_CLICK);
		FB.AppRequest("Lets play Carrom private match", null, null, null, 2, PhotonNetwork.CurrentRoom.Name, "Private Match");
	}

	public bool IsChatPannelOpen()
	{
		return chatMessagePanel.activeSelf;
	}
}
