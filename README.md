# gun fishing

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

![image](https://github.com/user-attachments/assets/91622d36-1935-490d-a71a-970d205c714b)


Updating player position through ServerRPC with Unreliable Delivery + NetworkVariable<Vector2>:

![image](https://github.com/user-attachments/assets/fa4a7133-14e1-4f28-9986-c70d6cd52496)


Catching fish through ServerRPC and ClientRPC with Reliable Delivery and without Ownership requirements:

![image](https://github.com/user-attachments/assets/3cdcec18-ed85-458f-b3a5-ad222d6e8ff8)

---

All network scripts are separated from UI logic. The lobby code with Relay or Dedicated server can be found in the start scene.

Dedicated Server (Multiplay Hosting):

![image](https://github.com/user-attachments/assets/09a3d4ee-194d-4085-b790-50dc956c0ce5)
![image](https://github.com/user-attachments/assets/07edde31-860e-441e-9aaa-09fa81161dba)
![image](https://github.com/user-attachments/assets/7d9a11ce-e6c7-4637-b93e-569c771419db)
![image](https://github.com/user-attachments/assets/092fbd4a-b57a-42df-8e1f-4a7e8902af54)


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


