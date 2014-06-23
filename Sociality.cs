using UnityEngine;
using System.Collections;

public class Sociality : MonoBehaviour {
	private AIFollow myPathfinder;
	private Walkman myWalkman;
	private Animator myAnimator;
	private Behaviour myBehaviour;		
	
	public enum SocialRoles{Visitor, Listerner};
	public SocialRoles Role;
	
	public enum PhrasesCounter{ThreePhrases, FourPhrases, FivePhrases};
	public PhrasesCounter LengthOfDialog;
	
	public enum CooldownTime{Twenty, Thirty, Forty, Fifty};
	public CooldownTime SecondsBetweenVisit;
	
	private bool visitFriend;	
	private int conversationCount = 0;	
	private int maxPhrases;
	private float timeBetweenVisit;
	
	public GameObject myFriend;
	private Transform myPosition;
	private Sociality myFriendSocial;
	private Behaviour friendBehaviour;	
	private int answerNumber;
	private bool isListerner;
	private bool hasGuest;
	
	public bool useStandartPhrases;	
	private string labelText = "";
	public string[] myCustomPhrases;
	public string[] myCustomAnswers;
	private string[] myStandartPhrases = new string[5];
	private string[] myStandartAnswers = new string[5];
	
	public GUIStyle styleVisitor;
	public GUIStyle styleListerner;	
	private int xCoord = 50;					// xCoord коррекция положения появляющегося сообщения когда NPC не хочет говорить
	private int yCoord = 70;					// yCoord коррекция положения появляющегося сообщения когда NPC не хочет говорить	

	void Start () {
		// Получаем компоненты непися
		myPathfinder = GetComponent<AIFollow>();
		myBehaviour = GetComponent<Behaviour>();
		myWalkman = GetComponent<Walkman>();	
		myAnimator = transform.GetChild(0).GetComponent<Animator>();
		myFriendSocial = myFriend.GetComponent<Sociality>();
		
		// Узнаем роль - Посетитель или Слушатель
		switch (Role){
		case SocialRoles.Listerner:
			isListerner = true;
			break;
		case SocialRoles.Visitor:
			isListerner = false;
			break;
		}
		
		switch (LengthOfDialog){
		case PhrasesCounter.ThreePhrases:
			maxPhrases = 3;
			break;
		case PhrasesCounter.FourPhrases:
			maxPhrases = 4;
			break;
		case PhrasesCounter.FivePhrases:
			maxPhrases = 5;
			break;
		}
		
		switch (SecondsBetweenVisit){
		case CooldownTime.Twenty:
			timeBetweenVisit = 20;
			break;
		case CooldownTime.Thirty:
			timeBetweenVisit = 30;
			break;
		case CooldownTime.Forty:
			timeBetweenVisit = 40;
			break;
		case CooldownTime.Fifty:
			timeBetweenVisit = 50;
			break;
		}
		
		// Задаем стиля для Посетителя и Слушателя
		styleListerner.font = (Font)Resources.Load("Fonts/Cuprum-Regular");
		styleListerner.fontSize = 14;
		styleListerner.fontStyle = FontStyle.Normal;
		styleListerner.normal.textColor = Color.white;
		styleListerner.alignment = TextAnchor.LowerCenter;
		
		styleVisitor.font = (Font)Resources.Load("Fonts/Cuprum-Regular");
		styleVisitor.fontSize = 14;
		styleVisitor.fontStyle = FontStyle.Bold;
		styleVisitor.normal.textColor = Color.white;
		styleListerner.alignment = TextAnchor.UpperCenter;
		
		// Стандартные фразы и ответы
		myStandartPhrases[0] = "Как жизнь?";
		myStandartPhrases[1] = "Что слышно нового?";
		myStandartPhrases[2] = "Пора валить с этой работы.";
		myStandartPhrases[3] = "Что по зарплате слышно?";
		myStandartPhrases[4] = "Заходи вечерком на чай.";
		
		myStandartAnswers[0] = "Жизнь - это скучно. Я читал.";
		myStandartAnswers[1] = "Говорят, Джонсон погиб во время последнего ограбления. Жаль парня...";
		myStandartAnswers[2] = "Да уж, устроюсь лучше охранником в город, там спокойнее. Жену найду.";
		myStandartAnswers[3] = "Говорят, опять задерживают.";
		myStandartAnswers[4] = "Посмотрим, если не будет дежурства - зайду.";
		
		StartCoroutine(Timer());
	}	
	
	IEnumerator Timer(){
		yield return new WaitForSeconds(timeBetweenVisit);	
		switch (isListerner){
		case true:			// если Слушатель			
			break;
		case false:			// если Посетитель	
			if(visitFriend == false){				
				visitFriend = true;			// Давно не навещали друга! Пора навестить
				myWalkman.enabled = false;	
				StartCoroutine( VisitFriend() );	// Идем к другу	
				break;
			}		
			break;
		}		
	}
	
	// Функция для начала визита
	IEnumerator VisitFriend(){
		yield return new WaitForSeconds(1f);	
		Debug.DrawRay(transform.position, transform.TransformDirection(myFriend.transform.position)*100);		
		if( ! NearFriend() ){	// если не возле Друга, включаем анимацию и хотьбу		
			transform.LookAt(myFriend.transform.position);	
			myPathfinder.enabled = true;
			myPathfinder.target = myFriend.transform;			
			myAnimator.SetBool("forWalk", true);
			StartCoroutine( VisitFriend() );
		}
		if ( NearFriend() ){	// если уже дошли - останавливаем анимацию, начинаем разговор
			myPathfinder.enabled = false;
			myAnimator.SetBool("forWalk", false);
			if(useStandartPhrases){
				StartCoroutine( BeginStandartConversation() );
			}else{
				StartCoroutine( BeginCustomConversation() );
			}			
		}			
	}	
	
	// Конец визита - возвращаемся к заданию Walkman
	void EndVisit(){
		labelText = " ";			
		conversationCount = 0;
		myPathfinder.enabled = true;		
		myWalkman.enabled = true;
		myWalkman.RestoreTarget();
		visitFriend = false;
		myFriendSocial.GoodBye();
		StartCoroutine(Timer());
	}
	
	// Функция для проверки расстояния до Друга
	private bool NearFriend(){
		float targetDistance = Vector3.Distance(myFriend.transform.position, transform.position);		
		if(targetDistance <= myPathfinder.targetReached){	
			return true;
		}
		return false;			
	}
	
	// Conversational functions
	IEnumerator BeginStandartConversation(){			
		if( conversationCount < maxPhrases){
			myFriendSocial.KnockKnock();			
			int random = (int)Random.Range(0, 4);
			labelText = myStandartPhrases[random];
			myFriendSocial.SetAnswerNumber(random);
			conversationCount++;			
			yield return new WaitForSeconds(5f);
			StartCoroutine( BeginStandartConversation() );
		}else if(conversationCount >= maxPhrases){
			 EndVisit();
		}
	}	
	
	IEnumerator BeginCustomConversation(){		
		if( conversationCount < maxPhrases){
			myFriendSocial.KnockKnock();			
			int random = (int)Random.Range(0, 4);
			labelText = myCustomPhrases[random];
			myFriendSocial.SetAnswerNumber(random);
			conversationCount++;	
			yield return new WaitForSeconds(5f);
			StartCoroutine( BeginCustomConversation() );
		}else if(conversationCount >= maxPhrases){
			 EndVisit();
		}		
	}
	
	IEnumerator WaitForGuest(){
		yield return new WaitForSeconds(1f);
		if(hasGuest){	// ... и есть Гость				
			if(useStandartPhrases){ 
				labelText = myStandartAnswers[answerNumber];	// отвечаем стандартно...
				StartCoroutine(WaitForGuest());
			}else{
				labelText = myCustomAnswers[answerNumber];		// или кастомными ответами
				StartCoroutine(WaitForGuest());
			} 
		}
	}
	
	// Function for notify NPC about guset-mode
	public void KnockKnock(){		
		hasGuest = true;
		myWalkman.enabled = false;
		transform.LookAt(myFriend.transform.position);
		StartCoroutine(WaitForGuest());
	}
	
	// Exit NPC guset-mode
	public void GoodBye(){
		labelText = "";
		hasGuest = false;
		myWalkman.enabled = true;
		StartCoroutine(Timer());
	}	
	
	public void SetAnswerNumber(int num){		
		answerNumber = num;
	}
	
	void OnGUI(){		
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
	    Vector3 cameraRelative = Camera.main.transform.InverseTransformPoint(transform.position);
		Rect position = new Rect(screenPosition.x - xCoord, Screen.height - screenPosition.y - yCoord, 100f, 20f);
		if(isListerner){
			GUI.Label(position, labelText, styleListerner);	
		}else GUI.Label(position, labelText, styleVisitor);		
	}
	
}
