# Gun Fishing

This small project describes working with a Unity Netcode.
WebGl temporary server link: https://builds.lazy.soccer:2053/netcode_test/

Game process:

![image](https://github.com/user-attachments/assets/600ad8a3-6953-4053-897f-4e065ec8be94)


Max shooting rate is 12 bullets per second; manual and auto-fire modes (press "Space" to switch modes).
Multiple fish types with random movement (Rust, Purple, Gold). Each type has its speed, volatility, scale, and image, and can have one of 3 rarities (Common, Rare, Legendary), lower volatility fish gives fewer points and is easier to hit.
All bullets move at the same speed and are destroyed upon hitting a fish only; bullets have a ricochet, so they remain on the map until they hit a fish (previous rule).
Recording and displaying the outcome of the last 12 shots for the local player and the total score. For other players, display only the total score.

Lobby as a guest, 4 players maximum per lobby:

![image](https://github.com/user-attachments/assets/b8286d22-f187-477e-a737-915b80da209e)


Bullets and fish have Network Transform.

![image](https://github.com/user-attachments/assets/36015edc-f51e-4ef6-950c-8b804b35d062)


Shooting through ServerRPC

        [ServerRpc]
        private void ShootServerRpc(ulong owner)
        {
            var bulletInstance = NetworkManager.Singleton.SpawnManager
                .InstantiateAndSpawn(bulletObject, ownerClientId: owner, position: _transform.position + Vector3.up, rotation: bulletPrefab.transform.rotation);
            
            bulletInstance.TryGetComponent(out GunBullet bullet);
            {
                bullet.SetHost(this);
                SuccessRateCheck();
            }
        }
        

Updating player position through ServerRPC with Unreliable Delivery + NetworkVariable<Vector2>:


        private void UpdatePosition(Vector2 worldPosition)
        {
            var newPosition = new Vector2(worldPosition.x, PosY);
            _transform.position = newPosition;
            UpdatePositionServerRpc(newPosition);
        }
        
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void UpdatePositionServerRpc(Vector2 newPosition)
        {
            if ((_networkPosition.Value - newPosition).sqrMagnitude > 0.01f)
            {
                _networkPosition.Value = newPosition;
            }
        }
        
        [ServerRpc]
        private void SetPlayerInfoServerRpc(ulong playerId)
        {
            if (RoomPlayersManager.Instance == null)
            {
                Debug.LogError("Failed to find RoomPlayersManager.Instance");
                return;
            }
            
            RoomPlayersManager.Instance.AddPlayer(playerId, this);
        }



Catching fish through ServerRPC and ClientRPC with Reliable Delivery and without Ownership requirements:


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                Debug.LogError("Failed to find trigger collider");
                return;
            }
            
            if (!other.CompareTag("Bullet")) return;

            if (other.TryGetComponent(out GunFishing.Gun.GunBullet bullet))
            {
                CatchFishServerRpc(bullet.OwnerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void CatchFishServerRpc(ulong playerId)
        {
            if (isCaught.Value) return;
            
            isCaught.Value = true;
            
            UpdateClientRpc(playerId);
            
            RoomInfoUi.Instance.AddTotalScore(points.Value);

            NetworkObject.Destroy(gameObject);
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void UpdateClientRpc(ulong playerId)
        {
            Debug.Log($"Fish caught by player {playerId}");

            RoomInfoUi.Instance.RegisterShot(points.Value, $"{volatility.Value} {fishType} fish", playerId);
            
            if (playerId == NetworkManager.Singleton.LocalClientId)
            {
                RoomPlayersManager.Instance.RegisterHit(playerId);
            }
        }

---

All network scripts are separated from UI logic. The lobby code with Relay or Dedicated server can be found in the start scene.

Dedicated Server (Multiplay Hosting):

![image](https://github.com/user-attachments/assets/09a3d4ee-194d-4085-b790-50dc956c0ce5)
![image](https://github.com/user-attachments/assets/7d9a11ce-e6c7-4637-b93e-569c771419db)
![image](https://github.com/user-attachments/assets/092fbd4a-b57a-42df-8e1f-4a7e8902af54)

        private async void Start()
        {
            #if SERVER
            
            try
            {
                Application.targetFrameRate = 60;

                await UnityServices.InitializeAsync();
                    
                _serverQueryHandler = await MultiplayService.Instance
                    .StartServerQueryHandlerAsync(maxPlayers, serverName, gameType, buildId, map);

                ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
                
                await UniTask.WaitUntil(() => serverConfig.AllocationId != string.Empty);

                var result = NetworkUnityServices.Instance.StartDedicatedServer(serverConfig.Port);

                if (result)
                {
                    Debug.Log("--- Server has started | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                    await MultiplayService.Instance.ReadyServerForPlayersAsync();
                    _serverHasStarted = true;
                }
                else
                {
                    Debug.LogError("--- Failed to start dedicated server | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("--- Failed to start linux dedicated server: " + e);
                throw;
            }
            
            #endif
        }


Unity Cloud Relay:

![image](https://github.com/user-attachments/assets/07724f41-de61-409a-9e2e-3df765d03d4f)
![image](https://github.com/user-attachments/assets/156db46c-31aa-4183-ae77-f146b5011a0b)


You can enable additional connection variants like direct Relay or Dedicated server connections in the Unity Editor:

![image](https://github.com/user-attachments/assets/ef6bf5c7-043e-46ed-8c67-d469632606a1)


Lobby dashboard:

![image](https://github.com/user-attachments/assets/a3d55fa1-42c1-4b9f-adf7-05d4769b054b)


Matchmaker dashboard:

![image](https://github.com/user-attachments/assets/2a57cd93-70de-4df9-b6cd-233b6888e0e6)


--


Creating lobby (Lobby name required):

![image](https://github.com/user-attachments/assets/f3078958-a3de-4e55-bf92-25e15209361d)


Lobby as host:

![image](https://github.com/user-attachments/assets/f002655e-f57b-4e66-bc96-b0f08f0e8571)

Joining lobby:

![image](https://github.com/user-attachments/assets/e3da7bec-d51b-41fa-b84e-e71481e636ad)


