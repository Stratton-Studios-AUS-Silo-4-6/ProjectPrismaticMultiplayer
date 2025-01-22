using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
using Mirror;
using MultiFPS.Gameplay;
using MultiFPS;

namespace MTPSKIT.Gameplay
{
    /// <summary>
    /// AI that controls bots
    /// </summary>
    public class BotAI : NetworkBehaviour
    {
        public bool Passive = false;

        protected Health targetedEnemy;
        protected CharacterInstance _characterInstance;

        public float RotationToTargetSpeed = 350f;

        private bool _pushTrigger = false;
        private bool _isFiring = false;

        public float fireTime = 1.55f;
        public float waitForFireTime = 0.75f; //gap between firing

        public bool SwitchItemsRandomly = false;

        
        
        [SerializeField] float DistanceFromTarget = 34f;

        
        Quaternion mindRotation;

        #region navigation
        //executing this method will make bot travel to given destination. It can be stopped by giving him new destination or 
        //simply by reaching given target at some point in time
        int _currentCorner;
        bool _isMoving;
        bool _rotateToDestination;
        int _pathCorners;
        #endregion



        float _focusEnemyTimer = 0;
        float _focusEnemyTime = 1.5f; //time that enemy must be in sight to be noticed by us

        bool _maintainPosition;
        Vector3 _positionToMaintain;
        float _maxAllowedDistanceFromPositionToMaintain;

        bool _enemyInSight;

        float _takeRandomItemTimer;

        //firing
        float _fireTime;

        float _stepAsideCooldownTimer;
        RaycastHit[] _freePoint;

        private void Awake()
        {
            _characterInstance = GetComponent<CharacterInstance>();
            _characterInstance.Server_SetAsBOT += SetAsBot;

            enabled = false;

            _path = new NavMeshPath();
        }
        private void Start()
        {
            _characterInstance.Health.Server_OnHealthDepleted += OnDeath;
            _characterInstance.Health.Server_Resurrect += ServerOnResurrect;
        }

        private void FixedUpdate()
        {
            AIUpdate(Time.fixedDeltaTime);
        }


        private void OnDestroy()
        {
            if (_characterInstance)
            {
                _characterInstance.Health.Server_OnHealthDepleted -= OnDeath;
                _characterInstance.Server_SetAsBOT -= SetAsBot;
                _characterInstance = null;
            }

            targetedEnemy = null;
            _path = null;
        }

        void SetAsBot(bool _set)
        {

            enabled = _set;
        }


        public void ServerMantainPosition(Vector3 pos, float maxDistance = 0.5f) 
        {
            _positionToMaintain = pos;
            _maintainPosition = true;
            _maxAllowedDistanceFromPositionToMaintain = maxDistance;
        }
        public void ServerForgetPositionToMaintain() 
        {
            _maintainPosition = false;
        }


        void TakeRandomItem()
        {
            int randomSlot = UnityEngine.Random.Range(0, _characterInstance.CharacterItemManager.Slots.Count);

            _characterInstance.CharacterItemManager.ServerCommandTakeItem(randomSlot);
        }
        void OnDeath(CharacterPart hittedPartID, AttackType attackType, Health killer, int attackForce)
        {
            ServerForgetPositionToMaintain();
        }
        void ServerOnResurrect(int health) 
        {
            enabled = _characterInstance.BOT;
        }

        void AIUpdate(float timestep)
        {
            if (!isServer) return;

            if (SwitchItemsRandomly && Time.time > _takeRandomItemTimer)
            {
                _takeRandomItemTimer = Time.time+UnityEngine.Random.Range(6f, 18f);
                TakeRandomItem();
            }

            if (Passive) return;

            //commit sepuku if stuck for more than 5 seconds
            if (_stuckTimer > 5f)
            {
                _stuckTimer = 0f;
                _characterInstance.Health.Server_ChangeHealthState(9999, CharacterPart.body, AttackType.falldamage, _characterInstance.Health, 255);
            }

            if (_characterInstance.Health.CurrentHealth <= 0) return;
            

            UpdatePath();

            _enemyInSight = EnemyInSight();

            if (_enemyInSight)
            {

                _focusEnemyTimer += timestep;               
            }
            else 
            {
                _focusEnemyTimer = 0;

            }

            //AI LOGIC
            if (!_enemyInSight || targetedEnemy && targetedEnemy.CurrentHealth <= 0)
                targetedEnemy = GetClosestEnemy();

            //If there is someone to attack
            if (targetedEnemy)
            {
                float distanceFromEnemy = Vector3.Distance(transform.position, targetedEnemy.transform.position);


                if (_focusEnemyTime < _focusEnemyTimer || distanceFromEnemy < 5f)
                {
                    ///ENEMY IN SIGHT -> attack
                    if (Time.time > _fireTime) 
                    {
                        _fireTime = Time.time + (_pushTrigger ? waitForFireTime : fireTime);
                        _pushTrigger = !_pushTrigger;

                        if (_isFiring)
                        {
                            _isFiring = false;
                            _characterInstance.CharacterItemManager.Fire1Release();
                        }
                    }

                    if (_pushTrigger && !_isFiring)
                    {
                        _isFiring = true;
                        _characterInstance.CharacterItemManager.Fire1Press();
                    }

                    _characterInstance.SetActionKeyCode(ActionCodes.Sprint, false);
                    //SetBurst(true);

                    Vector3 newFreeVector;


                    if (distanceFromEnemy < DistanceFromTarget)
                    {
                        if (_stepAsideCooldownTimer < Time.time) //decision if ai wants to increase distance of target and if its able to                                                                //    if (distance < minDistanceFromTarget && !increasingDistanceFromTarget) //decision if ai wants to increase distance of target and if its able to
                        {
                            //setting cooldown to prevent enemy from moving all the time
                            _stepAsideCooldownTimer += 2f;
                            

                            Vector3[] increasingDistancePossibleDirections = { -transform.right, transform.right, -transform.forward };
                            newFreeVector = freeSpace(increasingDistancePossibleDirections, 8f);
                            if (Vector3.Distance(newFreeVector, transform.position) > 0.5f)
                                SetTravelDestinationByNavMesh(newFreeVector, false);
                        }
                    }
                    else
                    {
                        SetTravelDestinationByNavMesh(targetedEnemy.transform.position, false);
                    }

                    LookAt(targetedEnemy.GetPositionToAttack(), timestep);
                }
                else
                {
                    _characterInstance.SetActionKeyCode(ActionCodes.Sprint, distanceFromEnemy > 2f);

                    //if enemy is not in sight then do not shot and go to nearest enemy position
                    if (!_maintainPosition)
                        SetTravelDestinationByNavMesh(targetedEnemy.transform.position, true, true);
                    else 
                    {
                        float distanceFromTarget = Vector3.Distance(transform.position, _positionToMaintain);
                        if (distanceFromTarget > _maxAllowedDistanceFromPositionToMaintain)
                            SetTravelDestinationByNavMesh(_positionToMaintain, true, true);
                    }
                        
                }
            }
            else
            {

                if (_maintainPosition)
                {
                    float distanceFromTarget = Vector3.Distance(transform.position, _positionToMaintain);
                    if (distanceFromTarget > _maxAllowedDistanceFromPositionToMaintain)
                        SetTravelDestinationByNavMesh(_positionToMaintain, true, true);
                }else
                    SetTravelDestinationByNavMesh(transform.position, false);

                
            }
        }

        protected Vector3 freeSpace(Vector3[] direction, float range) {
            Vector3 bestVector = transform.position;
            for (int i = 0; i < direction.Length; i++)
            {
                Ray rayCheck = new Ray(transform.position + transform.up * 0.25f, direction[i]);
                
                if (0 > Physics.RaycastNonAlloc(rayCheck, _freePoint, range, GameManager.environmentLayer)) //if we hit something
                {
                    RaycastHit freePoint = _freePoint[0];
                    if (Vector3.Distance(transform.position, freePoint.point) > Vector3.Distance(transform.position, bestVector) && freePoint.collider.gameObject.layer != 13) //avoiding this layer to make bots not stucking on each other
                        bestVector = freePoint.point;
                }
                else
                {
                    float difference = Vector3.Distance(transform.position, transform.position + direction[i] * range) - Vector3.Distance(transform.position, bestVector);
                    if (Mathf.Abs(difference) < 2f)
                    {
                        if (UnityEngine.Random.Range(0, 3) == 1) bestVector = transform.position + direction[i] * range;
                    }
                    else if (difference > 0)
                        bestVector = transform.position + direction[i] * range;
                }
            }
            return bestVector;
        }

        /// <summary>
        /// coroutine for pulling trigger
        /// it makes bots not shoot all the time when they have enemy in range
        /// </summary>
        /// 
        /*private void SetBurst(bool _start)
        {
            if (_start == shooting) return;

            shooting = _start;

            if (triggerBurst != null)
            {
                StopCoroutine(triggerBurst);
                triggerBurst = null;
            }

            if (_start)
            {
                triggerBurst = StartCoroutine(c_pushTrigger());
            }
            pushTrigger = false;

            IEnumerator c_pushTrigger()
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                while (true)
                {
                    if (!pushTrigger)
                    {
                        pushTrigger = true;
                        yield return new WaitForSeconds(fireTime);
                    }
                    else
                    {
                        pushTrigger = false;
                        yield return new WaitForSeconds(waitForFireTime);
                    }
                }
            }
        }*/

        //makes bot look at given spot, rotation is not instant
        protected void LookAt(Vector3 _lookTarget, float timestep)
        {
            _characterInstance.characterMind.position = _characterInstance.Health.GetPositionToAttack();

            Vector3 lookRotationVector = _lookTarget - _characterInstance.characterMind.position;

            if(lookRotationVector != Vector3.zero)
                mindRotation = Quaternion.Lerp(mindRotation, Quaternion.LookRotation(lookRotationVector), RotationToTargetSpeed * timestep);

            _characterInstance.characterMind.rotation = mindRotation;


            _characterInstance.Input.LookY = mindRotation.eulerAngles.y;

            //look up/down correction
            float lookx = -mindRotation.eulerAngles.x;
            float fixedLookX = lookx < -90 ? lookx += 360 : lookx;
            _characterInstance.Input.LookX = -fixedLookX;
        }
        protected void LookAtSharp(Vector3 _lookTarget)
        {
            _characterInstance.characterMind.position = _characterInstance.Health.GetPositionToAttack();
            _characterInstance.characterMind.LookAt(_lookTarget);           
            mindRotation = _characterInstance.characterMind.rotation;

            _characterInstance.Input.LookY = mindRotation.eulerAngles.y;

            //look up/down correction
            float lookx = -mindRotation.eulerAngles.x;
            float fixedLookX = lookx < -90 ? lookx += 360 : lookx;
            _characterInstance.Input.LookX = -fixedLookX;
        }


        RaycastHit[] _sightRayCastHit = new RaycastHit[1];
        /// <summary>
        /// Can we see nearest enemy or there is something beetwen us?
        /// </summary>
        protected bool EnemyInSight()
        {
            //TODO: just make raycast beetwen those two and check if there is something between them
            if (!targetedEnemy) return false;

            Vector3 direction = targetedEnemy.GetPositionToAttack() - _characterInstance.FPPLook.position;
            return Physics.RaycastNonAlloc(_characterInstance.FPPLook.position, direction, _sightRayCastHit, direction.magnitude, GameManager.environmentLayer) <= 0;
        }


        #region navigation tools

        bool followingPath;
        Vector3 desiredVelocity;
        Vector3 directTravelTargetPoint;
        NavMeshPath _path;
        protected virtual void MovementTick()
        {
            if (followingPath)
                desiredVelocity = transform.InverseTransformDirection(directTravelTargetPoint - transform.position).normalized;
            else
                desiredVelocity = Vector3.zero;
        }

       // bool _ableToReachDestination;
        //Vector3 _destinationPosition;
        protected void SetTravelDestinationByNavMesh(Vector3 destinationPosition, bool rotateToDestination, bool overrideCurrentIfExistPath = true)
        {
            if (_isMoving && !overrideCurrentIfExistPath) return;

            _rotateToDestination = rotateToDestination;

            _isMoving = true;

            NavMesh.CalculatePath(transform.position, destinationPosition, NavMesh.AllAreas, _path);
            _currentCorner = 1;
            _pathCorners = _path.corners.Length;


            //_destinationPosition = destinationPosition;
        }

        float _stuckTimer;

        void UpdatePath() 
        {
            if (!_isMoving) return;

            if (_currentCorner < _pathCorners)
            {
                directTravelTargetPoint = _path.corners[_currentCorner];

                desiredVelocity = transform.InverseTransformDirection(directTravelTargetPoint - transform.position).normalized;
                
                _characterInstance.Input.Movement = new Vector2(
                    Mathf.Clamp(desiredVelocity.x, -1.0f, 1.0f),
                    Mathf.Clamp(desiredVelocity.z, -1.0f, 1.0f)
                    );

                if (_rotateToDestination)
                    LookAtSharp(_path.corners[_currentCorner] + new Vector3(0, _characterInstance.characterMind.localPosition.y, 0));

                float distanceFromCurrentCorner = Vector3.Distance(transform.position, _path.corners[_currentCorner]);

                if (distanceFromCurrentCorner <= 0.45f)
                {
                    transform.position = _path.corners[_currentCorner];//+new Vector3(0,9,0);
                    _currentCorner++;
                }
            }
            else
            {
                _isMoving = false;
                _characterInstance.Input.Movement = new Vector2(0, 0);
            }
        }
#endregion

        Health _potentialNearestEnemy;
        Health _potentialEnemy;
        protected Health GetClosestEnemy() {
            _potentialNearestEnemy = null;
            _potentialEnemy = null;

            if (targetedEnemy && targetedEnemy.CurrentHealth <= 0) 
                targetedEnemy = null;

            if (GameManager.Gamemode.PeacefulBots)
            {
                targetedEnemy = null;
                return null;
            }

            float lastDistance = float.MaxValue;
            int charactersCount = CustomSceneManager.spawnedCharacters.Count;
            bool ffa = GameManager.Gamemode.FFA;
            int myTeam = _characterInstance.Health.Team;
            Vector3 we = transform.position;

            for (int characterUnit = 0; characterUnit < charactersCount; characterUnit++) {
                _potentialEnemy = CustomSceneManager.spawnedCharacters[characterUnit];
                if ((_potentialEnemy.Team != myTeam || ffa) && _potentialEnemy != _characterInstance.Health && _potentialEnemy.CurrentHealth > 0)
                {
                    float distance = Vector3.Distance(we, _potentialEnemy.transform.position);
                    if (distance < lastDistance)
                    {
                        _potentialNearestEnemy = _potentialEnemy;
                        lastDistance = distance;
                    }
                }
            }

            if (!_potentialNearestEnemy) return null;

            return _potentialNearestEnemy;
        }
    }
}