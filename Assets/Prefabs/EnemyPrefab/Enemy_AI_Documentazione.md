# Sistema di IA Nemico – Documentazione Tecnica

## 1. Logica di Navigazione e Stato

Abbiamo utilizzato un **NavMeshAgent** per gestire il movimento intelligente sul terreno.

- **Patrol (Pattugliamento)**  
  Il nemico si muove tra una serie di punti (`patrolPoints`) in sequenza.

- **Chase (Inseguimento)**  
  Appena il giocatore entra nel raggio d'azione, il nemico cambia la destinazione dell'agente verso la sua posizione in tempo reale, modificando autonomamente l'animazione mostrata'.

- **Transizioni di Velocità**  
  Sono state impostate animazioni e velocità differenti per la camminata e la corsa per rendere chiaro l'inizio del chase dell'NPC'.

---

## 2. Sistema di Combattimento (Decoupling)

Seppur prefab di base sia uguale a quello dell'arma del player, è stato rivisitato completamente per includere logica apposita per l'NPC.

- **EnemyGun.cs**  
  Arma dedicata all'IA che non dipende dalla telecamera, ma spara direttamente dal `firePoint` verso la posizione del giocatore.

- **Mira Verticale**  
  Introduzione di un `aimVerticalOffset` per far sì che il nemico miri al petto del giocatore invece che ai piedi.

- **Puntamento Realistico**  
  Il nemico spara solo se è effettivamente rivolto verso il bersaglio (entro un certo `shootingAngleThreshold`), evitando che i proiettili vengano generati mentre l'IA sta ruotando.

---

## 3. Visione Realistica (Cono Visivo)

Per superare il semplice rilevamento sferico, è stato implementato un vero sistema di visione.

- **Field of View (FOV)**  
  Il nemico rileva il giocatore solo se si trova all'interno del suo cono visivo frontale (ad esempio 90°). È quindi possibile sorprenderlo arrivando da dietro.

- **Raycasting e Ostacoli**  
  Viene utilizzato un raycast per verificare la linea di vista, impedendo al nemico di vedere o sparare attraverso muri o oggetti solidi.  
  I `LayerMask` permettono di distinguere correttamente tra ambiente e giocatore.

---

## 4. Animazioni e Stabilità Fisica

Questa fase è stata fondamentale per evitare scatti, slittamenti e movimenti innaturali.

- **Animazioni In-Place**  
  Le animazioni di corsa sono state configurate come *in-place* (Bake Into Pose su XZ) per evitare che la mesh si disallinei dal centro del modello.

- **Fix del Fluttuamento**  
  Il posizionamento verticale delle animazioni è stato impostato su **Original** nelle impostazioni di importazione, garantendo il corretto contatto dei piedi con il terreno.

- **Animator Controller**  
  È stato creato il parametro `isRunning` con transizioni senza *Exit Time*, rendendo immediato il passaggio tra camminata e corsa.

---

## 5. Gestione della Morte

La morte del nemico è stata progettata per essere pulita, coerente e priva di effetti collaterali.

- **Disabilitazione Totale**  
  Alla morte vengono disattivati:
  - `NavMeshAgent`
  - Script `EnemyAI_NavMesh`
  - `CapsuleCollider`

- **Gestione dei Renderer**  
  Sono supportati sia `MeshRenderer` che `SkinnedMeshRenderer`, evitando che il modello rimanga visibile dopo la morte.

- **VFX e Despawn**  
  Viene attivato un effetto visivo di morte prima della distruzione definitiva del `GameObject`, che avviene dopo un breve delay.
