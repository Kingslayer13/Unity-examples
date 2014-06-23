using UnityEngine;
using System.Collections;

public class Range : Attacks {
	
	private GameObject myEnemy;
	private npcCharacteristics his;
	private Behaviour hisBehaviour;
	private bool isPlayer = true;
	
	private double enemyHealth;		    // переменная для здоровья игрока
	private float enemyArmor;            // переменная для брони игрока	
	private string enemyTag;	
	private int armorBlockChance;
	private int rnd;
	private int distanceToEnemy;           // расстояние до игрока
	private bool playerAttack = false; 
	private bool walking = false;
	
    private CharacterController controller; // получение контролера у игрока
	
    private Transform thisTransform;   // трансформ этого врага
    private Transform myEnemyTransform; // трансформ игрока
	public bool test;
	// скорость поворота в секунду
    public float turnSpeed = 90;
	
	public GameObject npcWeapon;       // оружие Идо	
	public GameObject weaponSpawn;
	public GameObject myBody;
	public float xPos;
	public float yPos;
	public float zPos;
	public float xRot;
	public float yRot;
	public float zRot;
	
	public int bullets = 10;
	private int currBullets;
	private bool reloading;
	
	private npcCharacteristics my;	
	private Behaviour myBehaviour;
	private Transform myModel;
	private NPCWeapon myWeapon;
	private Animator myAnimator;      		//Animator	
	private AIFollow myPathfinder; 	        // получение компонента пасфайндера ЭТОГО врага
	private EnemySearchProtocol myEnemySearch;
	private PlayerCharacteristics PlCh; 	//данные для получения параметров игрока
	private EnemyAttack aggressivePlayer;
	
	private float lastAttack;            	// время последней атаки  	
	
	public AudioClip steps;                 // звук шагов врага
	public AudioClip punch;                 // звук атаки врага
	public AudioClip reloadSound; 
	
    public void Start(){
		// размещаем оружие Идо в его руку
		GameObject Weapon = (GameObject)Instantiate(npcWeapon);
		Weapon.transform.parent = weaponSpawn.transform;
		Weapon.transform.localPosition = new Vector3(xPos, yPos, zPos);
		Weapon.transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
		
		// получаем компоненты у игрока и его характеристики		
		PlCh = GameObject.Find("-Characteristics-").GetComponent<PlayerCharacteristics>();	
		myAnimator = myBody.GetComponent<Animator>();
		myPathfinder = GetComponent<AIFollow>();
		my = GetComponent<npcCharacteristics>();
		myBehaviour = GetComponent<Behaviour>();		
		myWeapon = Weapon.GetComponent<NPCWeapon>();
       	myEnemySearch = GetComponent<EnemySearchProtocol>();
		aggressivePlayer = GetComponent<EnemyAttack>();
		
        controller = GetComponent<CharacterController>(); 		// Получаем контроллер    
        thisTransform = transform;    							// Получаем компонент трансформации объекта, к которому привязан данный компонент      
       	//myEnemyTransform = GameObject.Find("GLOBAL").transform;	// Получаем компонент трансформации игрока
		
		myModel = transform.GetChild(0);
		
		//выключаем пасфайндер, чтобы не ходил пока
		myPathfinder.enabled = false;	
		//myPathfinder.target = myEnemyTransform;	
		
		currBullets = bullets;		
    } 
   
	public void Update(){
		if(test){
			SetRetreat();
		}	
		
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
			switch (myEnemy.tag){
			case "Global":
				SetRetreat();
				break;
			case "Untagged":
				myBehaviour.State("idle");
				break;
			default:
				SetMyEnemy(myEnemy);
				break;
			};			
		}
		
		if(isPlayer){
			enemyHealth = PlCh.GetCurrentHealth();				  // получаем здоровье игрока
			enemyArmor = (float)PlCh.GetArmor();                        // получаем броню игрока
			armorBlockChance = PlCh.GetArmorBlock();		
		}else if(! isPlayer){
			enemyHealth = his.CurrentHealth();
			enemyArmor = his.Armor();		
			armorBlockChance = his.ArmorBlockChance();
		}		
		
		thisTransform.LookAt(myEnemyTransform);		
		distanceToEnemy =  (int)Vector3.Distance(myEnemyTransform.position, thisTransform.position);			
		rnd = (int)Random.Range(1, 150);		
	    // посылаем луч к игроку	
		Vector3 _direction = transform.TransformDirection(Vector3.forward) * my.Range();
		_direction.y = 2;
		RaycastHit _hit;
		
		Debug.DrawRay(thisTransform.position,transform.TransformDirection(Vector3.forward)*my.Range());
		
	if (Physics.Raycast(transform.position, _direction, out _hit, 1000)){		
		
		// Если луч пройдет через объект с тэгом "Global"...
		if (_hit.transform.tag == enemyTag){
			
			// ...и если дистанция до игрока меньше 50 метров...
           if (distanceToEnemy < my.DetectRange() && distanceToEnemy > my.Range()){ // включаем пасфайндер и анимацию ходьбы + звук шагов					    
				Walk(true);
			}else if (distanceToEnemy >  my.DetectRange() ){// если меньше N метров - враг не идет, анимации и звуков нет
					Debug.Log("why");
				Walk(false);				
			}				
		     // если дистанция до игрока меньше радиуса атаки EnemyRange			
            if (distanceToEnemy <= my.Range()){					
				//thisTransform.LookAt(myEnemyTransform);		
				
				if(!reloading){
					if(currBullets > 0){// останавливаем анимацию ходьбы и звуки										
               			if (enemyHealth > 0){ // ...и здоровье игрока больше нуля...
							if (Time.time > (lastAttack + my.Cooldown() )){   // Создаем задержку выстрела...
                				Shot ();	
							}
			        	}
		        	}else if(currBullets == 0){
						StartCoroutine(ReloadWeapon());	
					}		
				}
				
			}

	   }			
	}
}
	
	void Shot(){
		Walk (false);
		myAnimator.SetBool("forShoot", true);	
		
		if(rnd > armorBlockChance){ // ...и отнимаем здоровье у игрока, включая анимацию и звук удара
			enemyHealth = (enemyHealth - my.Damage(distanceToEnemy, enemyArmor) );						
		}
		currBullets = currBullets - 1;		
		myWeapon.makeShot = true;	
		audio.PlayOneShot(punch);					
		lastAttack = Time.time;			
		
		if(isPlayer){
			PlCh.SetHealth(enemyHealth);
		}else{
			his.SetCurrHealth(enemyHealth);
		}
	}
	
	void Walk(bool w){
		if(w){
			if(! walking){
				myPathfinder.enabled = true;
				myAnimator.SetBool("forShoot", false);
				myAnimator.SetBool("forReload", false);	
				myAnimator.SetBool("forWalk", true);
				walking = true;
				if (audio.isPlaying == false) {	
					audio.clip = steps;
					audio.Play();
				}	
			}
		}else if(! w){
			walking = false;
			myPathfinder.enabled = false;
			audio.Stop();	
            myAnimator.SetBool("forWalk", false);
			myAnimator.SetBool("forShoot", false);
			myAnimator.SetBool("forReload", false);	
		}
	}
	
	IEnumerator ReloadWeapon(){
		reloading = true;
		myAnimator.SetBool("forReload", true);
		myAnimator.SetBool("forWalk", false);
		myAnimator.SetBool("forShoot", false);
		if (audio.isPlaying == false) {	
			audio.clip = reloadSound;
			audio.Play();
		}	
		yield return new WaitForSeconds(2f);	
		currBullets = bullets;
		reloading = false;
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