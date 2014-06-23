using UnityEngine;
using System.Collections;


public class Behaviour : MonoBehaviour {		
	public string npcScriptName = "Please, type name of NPC script.";
	private string currentState;
	
	public enum AttackTypes{Melee, Range};
	public AttackTypes AttackType;
	
	public enum Moods{Friendly, Enemy, ForeverEnemy};
	public Moods Mood;
	
	public enum ActivityStates{Idle, Active};
	public ActivityStates Activity;
	private bool canWalk;
	public bool sociality;
	
	private MonoBehaviour attack;		
	private MonoBehaviour npc;		
	
	private Sociality mySocial;
	private Walkman myWalkman;
	private Animator myAnimator;
	private AIFollow myPathfinder;	
	private npcCharacteristics me;
	private EnemyAttack playerAttack;
	private EnemySearchProtocol myEnemySearcher;
	
	// Use this for initialization
	void Start () {		
		myPathfinder = GetComponent<AIFollow>();
		myEnemySearcher = GetComponent<EnemySearchProtocol>();			
		myAnimator = transform.GetChild(0).GetComponent<Animator>();
		playerAttack = GetComponent<EnemyAttack>();
		
		if(sociality){
			mySocial = GetComponent<Sociality>();			
		}
		
		switch (Activity){
		case ActivityStates.Idle:	
			canWalk = false;
			break;
		case ActivityStates.Active:
			canWalk = true;
			myWalkman = GetComponent<Walkman>();
			break;
		}
		
		switch (AttackType){
		case AttackTypes.Melee:
			attack = GetComponent("Melee") as Attacks;
			break;
		case AttackTypes.Range:
			attack = GetComponent("Range") as Attacks;
			break;
		};
		
		switch (Mood){
		case Moods.Friendly:
			npc = GetComponent(npcScriptName) as NPC;
			State("idle");
			break;
		case Moods.Enemy:
			npc = GetComponent(npcScriptName) as NPC;
			State ("attack");
			break;
		case Moods.ForeverEnemy:
			State ("attack");
			break;
		};		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public string State(string state){
		switch (state){
			
		case "idle":			
			if(canWalk){
				State("walk");
				break;
			}
			currentState = state;
			SetAllScriptsDisabled();		
			
			npc.enabled = true;			
			this.gameObject.tag = "NPC";
			return currentState;	
			
		case "walk":					
			currentState = state;
			SetAllScriptsDisabled();
			if(sociality){
				State("social");
				break;
			}
			
			myPathfinder.enabled = true;
			myWalkman.enabled = true;
			npc.enabled = true;			
			this.gameObject.tag = "NPC";
			return currentState;		
			
		case "attack":			
			currentState = state;
			SetAllScriptsDisabled();
			
			attack.enabled = true;
			myEnemySearcher.enabled = true;
			myEnemySearcher.EnableEnemyMode();
			playerAttack.enabled = true;
			
			this.gameObject.tag = "Enemy";
			
			Destroy(npc);			
			return currentState;
			
		case "defence": 			
			currentState = state;
			SetAllScriptsDisabled();
			
			myEnemySearcher.enabled = true;
			myEnemySearcher.EnableNPCMode();			
			attack.enabled = true;
			npc.enabled = true;				
			
			this.gameObject.tag = "NPC";
			return currentState;			
		
		case "social":								
			currentState = state;
			SetAllScriptsDisabled();		
			
			myPathfinder.enabled = true;
			myWalkman.enabled = true;
			mySocial.enabled = true;
			npc.enabled = true;			
			this.gameObject.tag = "NPC";
			return currentState;	
			
		case "state": 
			return currentState;
//			
//		case 6: 
//			Debug.Log ("runaway");
//			break;	
//			
//		case 7:
//			me.SetKilled();
//			break;	
		}
		return "You're fucking kidding me?";
	}
	
	void SetAllScriptsDisabled(){
		MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour script in allScripts){
			switch(script.GetType().ToString()){
			case "Behaviour":				
			case "npcCharacteristics":
				break;
			default:
				script.enabled = false;
				break;
			}
		}
	}
	
	public void Scream(string comradesTag, string state){
		GameObject[] comrades = GameObject.FindGameObjectsWithTag(comradesTag);
		foreach (GameObject comrade in comrades){
			Behaviour his = comrade.GetComponent<Behaviour>() as Behaviour;
			his.State (state);
		}
	}	
}

