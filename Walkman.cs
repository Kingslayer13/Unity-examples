using UnityEngine;
using System.Collections;


public class Walkman : MonoBehaviour {
	private AIFollow myPathfinder;
	private Animator myAnimator;	
	
	private Transform myBody;
	private Vector3 myHomePosition;
	private Vector3 myNewWay;
	public Vector3 myTarget;
	private GameObject objTarget;	
	
	private Vector3 friendPosition;
	private bool visitingFriend;
	private bool canGoHome;
	
	public enum RestTime{Long = 20, Medium = 15, Fast = 10, GiperActive = 5};
	public RestTime TimeBetweenWalking;	
	
	public enum PromenadeTypes{Wander, Patrol};
	public PromenadeTypes PromenadeType;
	private bool beginPatrol;	
	
	public enum Distances{ Near = 5, Far = 15, WhereverYouAre = 20, Medium = 10};
	public Distances DistanceToWalk;
	private int distanceToWalk;
	
	private string currentDirection;
	private float waiting = 20;
	private float lastWalk = 0;	
	
	void Start () {
		myPathfinder = GetComponent<AIFollow>();
		myBody = transform.GetChild(0);
		myAnimator = myBody.GetComponent<Animator>();		
		myHomePosition = transform.position;	
		
		switch (PromenadeType){
		case PromenadeTypes.Wander:
			beginPatrol = false;
			break;
		case PromenadeTypes.Patrol:
			beginPatrol = true;
			break;
		}
		
		switch (TimeBetweenWalking){
		case RestTime.Long:
			waiting = (int)TimeBetweenWalking;
			break;
		case RestTime.Medium:
			waiting = (int)TimeBetweenWalking;
			break;		
		case RestTime.Fast:
			waiting = (int)TimeBetweenWalking;
			break;
		case RestTime.GiperActive:
			waiting = (int)TimeBetweenWalking;
			break;
		}
		
		switch (DistanceToWalk){
		case Distances.Near:
			distanceToWalk = (int)Distances.Near;
			break;
		case Distances.Medium:
			distanceToWalk = (int)Distances.Medium;
			break;
		case Distances.Far:
			distanceToWalk = (int)Distances.Far;
			break;
		case Distances.WhereverYouAre:
			distanceToWalk = (int)Distances.WhereverYouAre;
			break;
		}
		
		objTarget = new GameObject();	
		objTarget.tag = "NPC-Target";
		objTarget.name = transform.name + "-target" + GameObject.FindGameObjectsWithTag("NPC-Target").Length.ToString();
		objTarget.transform.position = transform.position;	
		
		myPathfinder.target = objTarget.transform;
		myPathfinder.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {				
		if(! beginPatrol && Time.time > (lastWalk + waiting )){		
			Wander();
				
		}else if(beginPatrol && Time.time > (lastWalk + waiting )){
			Patrol();
		}			
		
		if(myBody.localPosition != new Vector3 (0,myBody.localPosition.y,0)){
			myBody.localPosition = new Vector3 (0,myBody.localPosition.y,0);
		}	
		
		AnimationControl();
	}
	
	void Wander(){				
		myNewWay = MakeMyWay();
		transform.LookAt(myNewWay);	
		Vector3 direction = transform.TransformDirection(Vector3.forward) * 10;
		direction.y = 2;
		RaycastHit hit;
				
		Debug.DrawRay(transform.position,transform.TransformDirection(Vector3.forward)*10);
				
		if (Physics.Raycast(transform.position, direction, out hit, 1000)){				
			if (hit.transform.tag == "Untagged"){						
				myAnimator.SetBool("forWalk", true);				
				objTarget.transform.position = myNewWay;			
				lastWalk = Time.time;
				currentDirection = "wander";
			}
		}		
	}
	
	void Patrol(){
		Debug.DrawRay(transform.position, transform.TransformDirection(myTarget)*100);		
		if( IamHome() ){			
			transform.LookAt(myTarget);			
			objTarget.transform.position = myTarget;	
			lastWalk = Time.time;
			currentDirection = "home";
		}else{
			myAnimator.SetBool("forWalk", true);			
		}
		if( IamOnTarget() ){
			transform.LookAt(myHomePosition);			
			objTarget.transform.position = myHomePosition;	
			lastWalk = Time.time;	
			currentDirection = "target";
		}else{
			myAnimator.SetBool("forWalk", true);			
		}	
	}	
	
	private bool IamHome(){
		float targetDistance = Vector3.Distance(myHomePosition, transform.position);
		if(targetDistance <= myPathfinder.targetReached){		
			return true;
		}
		return false;
	}
	
	private bool IamOnTarget(){
		float targetDistance = Vector3.Distance(myTarget, transform.position);
		if(targetDistance <= myPathfinder.targetReached){			
			return true;
		}
		return false;
	}
	
	private bool IamWander(){
		float targetDistance = Vector3.Distance(myNewWay, transform.position);
		if(targetDistance <= myPathfinder.targetReached){			
			return true;
		}
		return false;
	}	
	
	void AnimationControl(){		
		switch (currentDirection){
		case "home":			
			if( IamOnTarget() ){				
				myAnimator.SetBool("forWalk", false);	
				currentDirection = "calm";
			}
			break;
		case "target":
			if( IamHome() ){				
				myAnimator.SetBool("forWalk", false);
				currentDirection = "calm";
			}
			break;
		case "wander":
			if( IamWander() ){				
				myAnimator.SetBool("forWalk", false);
				currentDirection = "calm";
			}
			break;				
		case "calm":
			break;
		}
		
	}
	
	private Vector3 MakeMyWay(){
		float angle = Random.Range(0.0f, Mathf.PI*2); 		
		Vector3 way = new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));		
		way *= distanceToWalk;
		return way + myHomePosition;
	}
	
	public void SetPathReached(){		
		myAnimator.SetBool("forWalk", false); 
	}
	
	public void RestoreTarget(){		
		myPathfinder.target = objTarget.transform;
	}
	
}