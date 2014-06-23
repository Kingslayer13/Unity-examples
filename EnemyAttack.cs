using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour {
    private Transform _thisTransform;   // трансформ этого врага
    private Transform _playerTransform; // трансформ игрока
	
	private GameObject Target;      
	private GameObject playerObj;          
	private GameObject weapon; 		 	// переменная для хранения оружия игрока	
	private GameObject InventoryObj;
	public GameObject blood;
	public GameObject shoot;             // переменная для префаба выстрела оружия игрока
	public GameObject EnergoShoot;       // переменная для префаба выстрела энерго оружия игрока
	
	private Animator playerAnimator;	      
	private ChangePositionOnClick ChPOnCl;	
	private PlayerCharacteristics PlCh;
	private GLOBAL Player;
	private Inventory inventory;        // Inventory	
	private MainInterface mainGUI;
	
	private npcCharacteristics my;
	private EnemySearchProtocol myEnemySearch;
	
	private AudioSource punchSound;	    // звук удара кулаком игрока	                    
	private AudioSource shotSound;      // переменная для хранения звука текущего оружия игрока	
	
	private float r_lastAttack;         // переменная для времени последней атаки игрока
    private double r_cooldown;          // переменная для задержки между атаками игрока	

	private int distanceToPlayer;       // дистанция до игрока

	private bool showTooltip;
    private int TooltipHeight = 100;
    private int TooltipWidth = 150;
    
	private bool bleeding = false;
	private bool energoWeapon;
	public bool isPlayer = false;
	private float bleedingTime;
	
	// Use this for initialization
	void Start () {
		// получаем данные из GLOBAL и playerCharacteristics		
		PlCh = GameObject.Find("-Characteristics-").GetComponent<PlayerCharacteristics>();		
		Player = GameObject.Find("GLOBAL").GetComponent<GLOBAL>();
		my = GetComponent<npcCharacteristics>();
		myEnemySearch = GetComponent<EnemySearchProtocol>();
		mainGUI = GameObject.Find ("GUI").GetComponent<MainInterface>();
		
		// получения даных объекта 	Target
		Target = GameObject.Find("Target");
		ChPOnCl = Target.GetComponent<ChangePositionOnClick>();
		
		// Получаем компонент трансформации объекта, к которому привязан данный компонент
        _thisTransform = transform;
 
        // Получаем компонент трансформации игрок
       _playerTransform = GameObject.Find("GLOBAL").transform;
		
	   InventoryObj = GameObject.Find ("Inventory");
	   inventory = InventoryObj.GetComponent<Inventory>();		
	}
	
	// Update is called once per frame
	void Update () {	
		playerObj = GameObject.FindGameObjectWithTag("Player");
		weapon = GameObject.FindGameObjectWithTag("Weapon");		
		
		if(bleeding){
			if (Time.time > (bleedingTime + 5)){		
	     		my.SetCurrHealth(my.CurrentHealth() - PlCh.GetColdSteal() / 10);		
	     		Instantiate(blood, new Vector3(_thisTransform.position.x, 0.1f, _thisTransform.position.z), transform.rotation);
		 		bleedingTime = Time.time;
			}	
		}
	}
	
	// скрипт срабатывает по клику мышью на враге
	void OnMouseDown(){
		if(playerAnimator == null){
			playerAnimator = playerObj.GetComponent<Animator>();
			return;
		}
		
		if(my.CurrentHealth() <= 0){
			my.SetKilled();
		}
		
    	isPlayer = true;		
		
		playerAnimator.SetBool("forThrow",false);	
		playerAnimator.SetBool("forReload",false);	
		
		punchSound = playerObj.GetComponent<AudioSource>();
		
		if(weapon != null){
			shotSound = weapon.GetComponent<AudioSource>();
		}	
    	// измеряем дистанцию до игрока
    	distanceToPlayer =  (int)Vector3.Distance(_playerTransform.position, _thisTransform.position);
		
		// получаем данные о времени последней атаки и задержке между атаками игрока для счетчика выстрела	
		r_lastAttack = Player.GetLastAttack();
		r_cooldown = Player.GetCooldown();
		
		// передаем игроку данные о расстоянии до врага и броне этого врага	
		Player.SetDist(distanceToPlayer);		
		Player.SetEnemyArmor(my.Armor());	
		Player.SetEnemyPhysics(my.Physic());
		
		// строим луч от врага к игроку, чтобы проверить нет ли препятствий
    	Vector3 _direction = _playerTransform.position - _thisTransform.position;
		RaycastHit hit;
		Physics.Raycast(_thisTransform.position, _direction, out hit, 1000);		
	 	if (hit.transform.tag == "Global"){				
			// если преград нет, поворачиваем игрока на врага	
			_playerTransform.LookAt(new Vector3(_thisTransform.position.x, 0, (_thisTransform.position.z)));
			
	    	// если здоровье врага больше нуля...
			if (my.CurrentHealth() > 0){
				
		  		//проверяем чтобы дитсанция до игрока не превышала дальности выстрела его оружия
		  		if (Player.WeaponRange() >= distanceToPlayer){
					
	       			// проверяем его патроны		
		   			if(Player.GetCurrentAmmo() > 0 ^ weapon == null){						
		
		    			// и прошло достаточно времени с предыдущего выстрела...		
		    			if (Time.time > (r_lastAttack + r_cooldown)){	
							if(Player.useEnergoWeapon() ){
								EnergoAttack();
							}else if(Player.useRangeWeapon() ){
								Shot();
							}else if(Player.useMeleeWeapon() ){
								Punch();
							}		      		
						}
					}
					
					else{
						if (InventoryObj.audio.isPlaying == false) {	
				    		InventoryObj.audio.clip = inventory.empty;
							InventoryObj.audio.Play();
						}
					}
		       }
	         } 
		}
	}
	
	void Punch(){
		int rnd = (int)Random.Range(1,  200-(PlCh.GetLuck()*10));
		
		if(rnd <= 10){
			my.SetCurrHealth(my.CurrentHealth() - PlCh.GetColdWeaponDamage() );	
			bleeding = true;		
		}
		
		if(rnd > 10){		
			my.SetCurrHealth(my.CurrentHealth() - PlCh.GetColdWeaponDamage() );	
		}
		
		playerAnimator.SetBool("forSmash",true);							      // (завичит от дистанции поражения оружия)
		ChPOnCl.target.position = Player.transform.position;              // объект target ставим возле игрока, чтобы он не шел при клике
		punchSound.Play();	
		
		// обновляем таймер атаки		
		r_lastAttack = Time.time;
		Player.SetLastAttack(r_lastAttack);	
	}
	
	void Shot(){
		int rnd = (int)Random.Range(1,  200-(PlCh.GetLuck()*10));
		
		if(rnd <= 10){
			my.SetCurrHealth(my.CurrentHealth() - (PlCh.GetDamage()*2) );				
		}
		
		if(rnd > 10){		
			my.SetCurrHealth(my.CurrentHealth() - PlCh.GetDamage() );
		}
		
		playerAnimator.SetBool("forShoot",true);					
		ChPOnCl.target.position = Player.transform.position;	
		Instantiate(shoot);                                               // помещаем на сцену объект shoot - это эффект выстрела					
		inventory.SetCurrAmmo(Player.GetCurrentAmmo() - 1);			      // отнимаем патроны
									
		if(weapon != null){                                               // звук выстрела
			shotSound.Play();
		}
		
		// обновляем таймер атаки		
		r_lastAttack = Time.time;
		Player.SetLastAttack(r_lastAttack);	
	}
	
	void EnergoAttack(){
		int rnd = (int)Random.Range(1,  200-(PlCh.GetLuck()*10));
		
		if(rnd <= 10){
			my.SetCurrHealth(my.CurrentHealth() - (PlCh.GetEnergoDamage()*2) );					
		}
		
		if(rnd > 10){		
			my.SetCurrHealth(my.CurrentHealth() - PlCh.GetEnergoDamage() );				
		}
		
		playerAnimator.SetBool("forShoot",true);					
		ChPOnCl.target.position = Player.transform.position;	
		Instantiate(EnergoShoot);
		shotSound.Play();				
		inventory.SetCurrAmmo(Player.GetCurrentAmmo() - 1);	
		
		// обновляем таймер атаки		
		r_lastAttack = Time.time;
		Player.SetLastAttack(r_lastAttack);				
	}
	
	// отображение здоровья врага над ним и смена курсора
	void OnMouseEnter(){
    	showTooltip = true;		
    }
	
    void OnMouseExit(){
    	showTooltip = false;
	}

    void OnGUI(){
    	if(showTooltip){
    		Vector3 screenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
    		Vector3 cameraRelative = Camera.main.transform.InverseTransformPoint(transform.position);
    			if (cameraRelative.z > 0){
    				Rect position = new Rect(screenPosition.x, Screen.height - screenPosition.y, 100f, 20f);
    				GUI.Label(position, ((int)my.CurrentHealth()).ToString()+"/"+ my.Health().ToString() );
    			}
    	}
	}
		
	public bool Attacking(){
		return isPlayer;
	}
}