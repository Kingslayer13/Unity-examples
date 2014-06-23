using UnityEngine;
using System.Collections;
 
public class Melee : Attacks{
	
	// получение контролера у игрока
    private CharacterController _controller;
	
	private PlayerCharacteristics PlCh; 	//данные для получения параметров игрока	
	
    private Transform myTransform;   // трансформ этого врага
    private Transform myEnemyTransform; // трансформ игрока
	
	private GameObject myEnemy;
	private npcCharacteristics his;
	private Behaviour hisBehaviour;
	private bool isPlayer = true;
	private bool playerAttack = false; 
	
	private double enemyHealth;		    // переменная для здоровья игрока
	private float enemyArmor;            // переменная для брони игрока	
	private string enemyTag;	
	private int armorBlockChance;
	private int rnd;
	private int distanceToEnemy;           // расстояние до игрока
	
	public GameObject myBody;
	public Transform myModel;
	private Behaviour myBehaviour;
	private npcCharacteristics my;   		// характеристики врага
	private Animator myAnimator;      		// Animator	врага
	private AIFollow myPathfinder; 	        // получение компонента пасфайндера ЭТОГО врага	
	private EnemySearchProtocol myEnemySearch;
	private EnemyAttack aggressivePlayer;
	
    public float myTurnSpeed = 90;			// скорость поворота в секунду
	public float myLastAttack;            	// время последней атаки		
	
	public AudioClip steps;                 // звук шагов врага
	public AudioClip punch;                 // звук атаки врага	
	
    public void Start(){
		// получаем компоненты у игрока и его характеристики		
		PlCh = GameObject.Find("-Characteristics-").GetComponent<PlayerCharacteristics>();
		
		myPathfinder = GetComponent<AIFollow>();
		my = GetComponent<npcCharacteristics>();		
		myAnimator = myBody.GetComponent<Animator>();
		myBehaviour = GetComponent<Behaviour>();
		myEnemySearch = GetComponent<EnemySearchProtocol>();
		aggressivePlayer = GetComponent<EnemyAttack>();
		
        // Получаем контроллер
        _controller = GetComponent<CharacterController>();
 
        // Получаем компонент трансформации объекта, к которому привязан данный компонент
        myTransform = transform;
 
        // Получаем компонент трансформации игрока
       	myEnemyTransform = GameObject.Find("GLOBAL").transform;
		
		myModel = transform.GetChild(0);
		
		//выключаем пасфайндер, чтобы не ходил пока
		Walk(false);		
    }
    
	public void Update(){		
		
		if(! playerAttack){
			if(aggressivePlayer.Attacking()){
				SetRetreat();
				playerAttack = true;
			}			
		}
		
		if(myModel.localPosition != new Vector3 (0,myModel.localPosition.y,0)){
			myModel.localPosition = new Vector3 (0,myModel.localPosition.y,0);
		}	
		
		if(myEnemy == null){
			myEnemy = myEnemySearch.GetMeEnemy();
			if(myEnemy.tag == "Global"){
				SetRetreat();
			}else{
				SetMyEnemy(myEnemy);
			}
		}
		
		if(isPlayer){
			enemyHealth = PlCh.GetCurrentHealth();				  // получаем здоровье игрока
			enemyArmor = (float)PlCh.GetArmor();                           // получаем броню игрока					
		}else if(! isPlayer){
			enemyHealth = his.CurrentHealth();				  // получаем здоровье игрока
			enemyArmor = his.Armor();                           // получаем броню игрока	
		}
		
	    transform.LookAt(myEnemyTransform);			
		Vector3 _direction = transform.TransformDirection(Vector3.forward) * 1000;
		_direction.y = 2;
		RaycastHit _hit;
		
	if (Physics.Raycast(transform.position, _direction, out _hit, 1000)){
		
		// Если луч пройдет через объект с тэгом "Global"...
		if (_hit.transform.tag == enemyTag){
		
			// ...и если дистанция до игрока меньше 50 метров...
			if (Vector3.Distance(myEnemyTransform.position, myTransform.position) < my.DetectRange()){			
				Walk(true);						
			}else{  // если меньше 50 метров - враг не идет, анимации и звуков нет		
				Walk (false);				
			}
				
		    // если дистанция до игрока меньше радиуса атаки Range		
			if (Vector3.Distance(myEnemyTransform.position, myTransform.position) <= my.Range()){
					
	          	// останавливаем анимацию ходьбы и звуки
				Walk(false);
					
				// ...и здоровье игрока больше нуля...
				if (enemyHealth > 0){                                              
                    MakeShot();					    
			   }
		    }
			  
	   }			
   }
}
	
	void MakeShot(){
		if (Time.time > (myLastAttack + my.Cooldown() )){  // Создаем задержку выстрела... 
			enemyHealth = (enemyHealth - my.ColdWeaponDamage(enemyArmor) );
					
			myAnimator.SetBool("forSmash", true);	
			audio.PlayOneShot(punch);
					
			myLastAttack = Time.time;	
		}
		
		if(isPlayer){
			PlCh.SetHealth(enemyHealth);
		}else{
			his.SetCurrHealth(enemyHealth);
		}
		
	}
	
	void Walk(bool w){
		if(w){ // включаем пасфайндер и анимацию ходьбы + звук шагов	    
			myPathfinder.enabled = true;
			
			myAnimator.SetBool("forWalk", true);
			myAnimator.SetBool("forSmash", false);
			
			if (audio.isPlaying == false) {	
				audio.clip = steps;
				audio.Play();
			}	
			
		}else{ // враг не идет, анимации и звуков нет		
			myPathfinder.enabled = false;			
			
            myAnimator.SetBool("forWalk", false);
			myAnimator.SetBool("forSmash", false);
			
			audio.Stop();	
		}
	}
	
	public void SetMyEnemy(GameObject enemy){
		his = enemy.GetComponent<npcCharacteristics>();
		myEnemyTransform = enemy.transform;
		myPathfinder.target = enemy.transform;
		enemyTag = enemy.tag;
		isPlayer = false;
	}
	
	public void SetRetreat(){
		myEnemyTransform = GameObject.Find("GLOBAL").transform;		
		myPathfinder.target = myEnemyTransform;	
		enemyTag = "Global";
		isPlayer = true;
	}
}

