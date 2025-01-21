using System.Collections;
using UnityEngine;
using Mirror;
using MultiFPS.PrisMulti;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/Gun")]
    public class Gun : Item
    {
        [Header("Gun")]
        [SerializeField] protected ParticleSystem _particleSystem;
        [SerializeField] protected ParticleSystem _huskSpawner_particleSystem;
        [SerializeField] protected AudioClip fireClip;
        [SerializeField] protected AudioClip reloadClip;
        [SerializeField] private GunFire gunFire;
        [SerializeField] private GunFire secondaryFire;

        [Header("Base gun properties")]
        public float ReloadTime = 1.5f;

        protected Coroutine _c_reload;
        protected Coroutine _c_serverReload;
        protected Transform _firePoint;


        [SerializeField] int _bulletPuncture = 2;

        [SerializeField] protected GameObject _bulletPrefab;
        ObjectPool _bulletPooler;

        [SerializeField] protected GameObject _decalPrefab;
        ObjectPool _decalPool;
        [SerializeField] protected GameObject _bloodPrefab;
        ObjectPool _bloodPool;
        
        public Transform FirePoint => _firePoint;
        public int Penetration => _bulletPuncture;

        protected override void Awake()
        {
            base.Awake();
            if (_bulletPrefab)
                _bulletPooler = ObjectPooler.Instance.GetPoolByName(_bulletPrefab.name);

            if (_decalPrefab)
                _decalPool = ObjectPooler.Instance.GetPoolByName(_decalPrefab.name);

            if (_bloodPrefab)
                _bloodPool = ObjectPooler.Instance.GetPoolByName(_bloodPrefab.name);
        }

        protected override void Update()
        {
            if (!MyOwner || secondaryFire) return;
            base.Update();

            _currentRecoilScopeMultiplier = _isScoping ? _recoil_scopeMultiplier : 1;

            if (_currentlyInUse)
            {
                if (CurrentRecoil > _recoil_minAngle)
                    CurrentRecoil -= _recoil_stabilizationSpeed * Time.deltaTime;
                else
                    CurrentRecoil = _recoil_minAngle;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!CanBeUsed()) return;

            if (!isServer) return;

            if (Server_CurrentAmmo <= 0 && Server_CurrentAmmoSupply > 0 && !_server_isReloading)
                PushReload(); //set _reloadTrigger to true

            if(_reloadTrigger)
                ProcessReloadRequest();
        }

        private void OnDestroy()
        {
            _firePoint = null;
            _c_reload = null;
            _c_serverReload = null;
        }

        public override void Take()
        {
            base.Take();
            _firePoint = MyOwner.characterFirePoint;

            OnCurrentAmmoChanged();

            //if previous weapons was scoping then we want to reset visual side of that for newly taken item
            if (MyOwner.FPP)
            {
                Camera _fppCamera = GameplayCamera._instance.FPPCamera;
                _fppCamera.transform.localPosition = Vector3.zero;
            }
        }

        public override void PutDown()
        {
            base.PutDown();

            _firePoint = null;

            //there is a chance player puts down weapon while its reloading, so we need to make sure 
            //to properly cancel that procedure
            CancelReloading();

            if (isServer)
                ServerCancelReloading();
        }

        protected override void SingleUse()
        {
            base.SingleUse();
            Client_OnShoot?.Invoke();
        }

        #region shooting
        public override void Use()
        {
            if (CurrentAmmo <= 0 || _isReloading || _doingMelee) return;

            var hitscan = Hitscan.Fire(this, _damage);
            Use(hitscan);
        }

        public void Use(Hitscan hitscan)
        {
            if (CurrentAmmo <= 0 || _isReloading || _doingMelee) return;

            base.Use();
            float finalRecoil = CurrentRecoil * MyOwner.RecoilFactor_Movement * _currentRecoilScopeMultiplier;

            _firePoint.localRotation = Quaternion.Euler(Random.Range(-finalRecoil, finalRecoil), Random.Range(-finalRecoil, finalRecoil), 0);

            if (isOwned)
            {
                Shoot(hitscan);
                CmdShoot(hitscan);
            }
            else if (isServer)
            {
                Server_CurrentAmmo--;
                RpcShoot(hitscan);
            }

            ClientChangeCurrentAmmoCount(CurrentAmmo - 1);   
        }

        [Command]
        protected void CmdShoot(Hitscan info)
        {
            if (Server_CurrentAmmo > 0)
            {
                Server_CurrentAmmo--;
                RpcShoot(info);
            }
        }
        [ClientRpc(includeOwner = false)]
        protected void RpcShoot(Hitscan info)
        {
            if (MyOwner)
            {
                Shoot(info);

                if (!isServer)
                    ClientChangeCurrentAmmoCount(CurrentAmmo-1);
            }
        }

        //paper shot, no damage, no game logic, only visuals
        protected void Shoot(Hitscan info)
        {
            AddAimRecoild(_recoil_aimCamera_recoil);

            _audioSource.PlayOneShot(fireClip);
            _particleSystem.Play();

            if (_huskSpawner_particleSystem)
            {
                //when in fpp view this would be renderer in top of everything, to avoid this we set husk spawner to default
                //layer again
                _huskSpawner_particleSystem.gameObject.layer = 0;
                _huskSpawner_particleSystem.Play();
            }

            SpawnVisualEffectsForHitscan(info);

            //if players shoots grenade, and dies in its explosion, then player will drop all of this items, so we no longer have 
            //acces to _myOwner, so simply return
            if (!MyOwner) return;

            MyOwner.CharacterAnimator.AddRecoil(7f);

            if (MyOwner.FPP)
            {
                if (!MyOwner.IsScoping)
                    MyOwner.CharacterAnimator.SetTrigger(AnimationNames.ITEM_FIRE);
            }

            if (_myAnimator.runtimeAnimatorController)
                if (!MyOwner.IsScoping)
                    _myAnimator.SetTrigger(AnimationNames.ITEM_FIRE);
        }

        protected void SpawnBullet(Vector3[] decalPos, Quaternion decalRot)
        {
            //spawn bullet from barrel
            _bulletPooler.ReturnObject((_isScoping ? MyOwner.FPPCameraTarget.position - MyOwner.FPPCameraTarget.up * 0.2f : _particleSystem.transform.position), decalRot).StartBullet(decalPos);
        }
        #endregion

        #region reloading
        //launched by player or bot input, just tells weapoan that user has intention to reload
        public override void PushReload()
        {
            if (isOwned)
                CmdClientRequestReload();
            else if (Server_CurrentAmmo < MagazineCapacity && CurrentAmmoSupply > 0 && !_server_isReloading)
                _reloadTrigger = true;
        }

        //game logic side of reload, server only
        protected void ProcessReloadRequest()
        {
            //dont start realod procedure if magazine is full or if we are already reloading, or if we dont have ammo to load in
            if (Server_CurrentAmmoSupply > 0 && Server_CurrentAmmo < MagazineCapacity && !_server_isReloading && _coolDownTimer < Time.time)
            {
                ServerReload();
                _reloadTrigger = false;
            }
        }

        //client visual part of reload, no affect on game logic, game logic is handled on server
        protected virtual void Reload() 
        {
            CurrentRecoil = _recoil_minAngle;
            CancelReloading();
            _isReloading = true;

            _c_reload = StartCoroutine(ReloadProcedure());
            IEnumerator ReloadProcedure()
            {
                _audioSource.PlayOneShot(reloadClip);

                MyOwner.IsReloading = true; //for character TPP animations

                //play animations
                MyOwner.CharacterAnimator.ResetTrigger(AnimationNames.ITEM_ENDRELOAD);
                _myAnimator.ResetTrigger(AnimationNames.ITEM_ENDRELOAD);
                MyOwner.CharacterAnimator.PlayAnimation(AnimationNames.ITEM_RELOAD); //TODO: fix: this lasts even then owner no longer exist, but item was still in use by owner
                _myAnimator.Play(AnimationNames.ITEM_RELOAD);

                yield return new WaitForSeconds(ReloadTime);

                if(!MyOwner)
                    yield break;

                //end reload animations
                MyOwner.CharacterAnimator.SetTrigger(AnimationNames.ITEM_ENDRELOAD);
                _myAnimator.SetTrigger(AnimationNames.ITEM_ENDRELOAD); 
            }
        }

        [Command]
        void CmdClientRequestReload() 
        {
            if (Server_CurrentAmmo < MagazineCapacity && CurrentAmmoSupply > 0 && !_server_isReloading)
                _reloadTrigger = true;
        }
        protected virtual void ServerReload()
        {
            if (_server_isReloading)
                return;

            //play reload animation and sound on all clients
            RpcReload();

            ServerCancelReloading();
            _c_serverReload = StartCoroutine(ReloadProcedure());

            _server_isReloading = true;

            IEnumerator ReloadProcedure()
            {
                yield return new WaitForSeconds(ReloadTime);

                int neededAmmo = MagazineCapacity - Server_CurrentAmmo;
                int finalMagazine;

                if (Server_CurrentAmmoSupply == int.MaxValue)
                {
                    finalMagazine = MagazineCapacity; // consider this to have infinite ammo
                }
                else if (neededAmmo > Server_CurrentAmmoSupply)
                {
                    finalMagazine = Server_CurrentAmmo + Server_CurrentAmmoSupply;
                    Server_CurrentAmmoSupply = 0;
                }
                else
                {
                    finalMagazine = MagazineCapacity;
                    Server_CurrentAmmoSupply -= neededAmmo;
                }
                
                Server_CurrentAmmo = finalMagazine;
                SendAmmoDataToClient(Server_CurrentAmmo, Server_CurrentAmmoSupply);

                ServerFinishClientReload();
                _server_isReloading = false;
            }
        }

        [ClientRpc]
        protected void ServerFinishClientReload() 
        {
            if(_currentlyInUse && _isReloading)
                OnReloadEnded();
        }

        /// <summary>
        /// called from the server
        /// </summary>
        protected virtual void OnReloadEnded() 
        {
            MyOwner.IsReloading = false;
            _isReloading = false;
        }

        protected void SendAmmoDataToClient(int currentAmmo, int supplyAmmo) 
        {
            RpcEndReload(currentAmmo, supplyAmmo);
        }

        [ClientRpc]
        void RpcEndReload(int currentAmmo, int supplyAmmo) 
        {
            CurrentAmmoSupply = supplyAmmo;
            ClientChangeCurrentAmmoCount(currentAmmo);
        }

        [ClientRpc]
        protected void RpcReload() 
        {
            if(MyOwner)
                Reload();
        }

        protected void CancelReloading() 
        {
            if (_c_reload != null)
            {
                StopCoroutine(_c_reload);
                _c_reload = null;
            }
            _isReloading = false;
            _reloadTrigger = false;
        }
        protected void ServerCancelReloading()
        {
            if (_c_serverReload != null)
            {
                StopCoroutine(_c_serverReload);
                _c_serverReload = null;
            }

            _server_isReloading = false;
        }

        #endregion

        protected override bool PrimaryFireAvailable()
        {
            return !_isReloading && CurrentAmmo > 0;
        }

        #region impact

        protected void SpawnImpact(Vector3 decalPos, Quaternion decalRot, byte hittedMaterialID) 
        {
            //spawn appropriate decal
            if (hittedMaterialID == 0)
                _decalPool.ReturnObject(decalPos, decalRot);
            else
                _bloodPool.ReturnObject(decalPos, decalRot);
        }
        #endregion

        public override void HoldLeftTrigger()
        {
            if (!MyOwner || gunFire) return;

            base.HoldLeftTrigger();
        }

        public override void HoldRightTrigger()
        {
            if (!MyOwner || secondaryFire) return;

            base.HoldRightTrigger();
        }

        public override void PressLeftTrigger()
        {
            if ( !MyOwner
                || !MyOwner.IsAbleToUseItem 
                || !PrimaryFireAvailable()
                || !gunFire)
                return;

            gunFire.PressTrigger();
        }

        public override void ReleaseLeftTrigger()
        {
            if (!MyOwner
                || !gunFire)
                return;
            
            gunFire.ReleaseTrigger();
        }

        public override void PressRightTrigger()
        {
            if ( !MyOwner
                 || !MyOwner.IsAbleToUseItem 
                 || !SecondaryFireAvailable()
                 || !secondaryFire)
                return;

            secondaryFire.PressTrigger();
        }

        public override void ReleaseRightTrigger()
        {
            if (!MyOwner
                || !secondaryFire)
                return;
            
            secondaryFire.ReleaseTrigger();
        }

        //spawn impact and bullet for given hitscan that happened
        protected void SpawnVisualEffectsForHitscan(Hitscan info) 
        {
            SpawnBullet(info.PenetrationPositions, info.FirstHitRotation);
            for (int i = 0; i < info.PenetratedObjectMaterialsIDs.Length; i++)
            {
                SpawnImpact(info.PenetrationPositions[i], info.FirstHitRotation, info.PenetratedObjectMaterialsIDs[i]);
            }
        }
    }
}