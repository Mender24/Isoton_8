using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Managers/Spwan Manager")]
    public class SpawnManager : MonoBehaviour
    {
        public bool _isDeleteSave = false;
        public InventoryItem _prefab;
        [SerializeField] private List<InventoryItem> _itemsPrefab = new();
        [SerializeField] private int _maxWeaponCount = 3;

        [FormerlySerializedAs("spwanableObjects")]
        public List<SpwanableObject> spawnableObjects = new List<SpwanableObject>();

        public string NameSearchObjectToNewScene = "SpawnPoints";
        public float spawnRadius = 5;
        public float respawnDelay = 5;

        [Separator]
        public List<SpwanSide> sides;

        public static SpawnManager Instance;

        public int _currentSpawnPointId = 0;

        public bool isActive { get; set; } = true;
        public int CurrentSpawnPointId => _currentSpawnPointId;

        public UnityEvent<GameObject> onPlayerSpwanWithObj { get; set; } = new UnityEvent<GameObject>();
        public UnityEvent<string> onPlayerSpwanWithObjName { get; set; } = new UnityEvent<string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (_isDeleteSave)
            {
                Debug.Log("DeleteAll");
                _isDeleteSave = false;
                PlayerPrefs.DeleteAll();
            }
        }

        public void SetNewSpawnPoint()
        {
            _currentSpawnPointId++;
        }

        public void UpdateSpawnPoint(int indexNewScene)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(indexNewScene);

            GameObject SpawnPoints = scene.GetRootGameObjects()
                .SelectMany(t => t.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(x => x.gameObject.transform.name == NameSearchObjectToNewScene)?.gameObject;

            if (SpawnPoints != null)
            {
                Transform[] newArray = SpawnPoints.GetComponentsInChildren<Transform>().Skip(1).ToArray();
                sides[0].points = newArray;
                _currentSpawnPointId = 0;
            }
        }

        public void MovePlayerStartPositionAndOn(Player player)
        {
            player.transform.position = sides[0].points[0].transform.position;
            player.gameObject.SetActive(true);
        }

        public async void SpawnActor(IActor actorSelf, string actorObjName, float delay)
        {
            if (Application.isPlaying == false)
                return;

            IActor copyOfActor = Instantiate(actorSelf.gameObject).gameObject.GetComponent<IActor>();

            //copyOfActor.gameObject.hideFlags = HideFlags.HideInHierarchy;

            copyOfActor.gameObject.SetActive(false);

            float time = 0;

            while (time < delay)
            {
                time += Time.deltaTime;

                if (Application.isPlaying == false)
                    return;

                await Task.Yield();
            }

            if (Application.isPlaying == false) return;

            SpawnActor(copyOfActor, actorObjName);

            Destroy(copyOfActor.gameObject);
        }

        public void SpawnActor(IActor actorSelf, string actorObjName)
        {
            GameObject obj = spawnableObjects.Find(x => x.name == actorObjName).obj;

            GameObject newPlayer = SpawnActor(actorSelf, obj);

            Actor newPlayerActorComponent = newPlayer.GetComponent<Actor>();
            Actor actorSelfActorComponent = actorSelf.gameObject.GetComponent<Actor>();

            if (newPlayerActorComponent && actorSelfActorComponent)
            {
                newPlayerActorComponent.kills = actorSelfActorComponent.kills;
                newPlayerActorComponent.deaths = actorSelfActorComponent.deaths;
            }

            onPlayerSpwanWithObjName?.Invoke(actorObjName);
        }

        public GameObject SpawnActor(IActor actorSelf, GameObject actorObj)
        {
            onPlayerSpwanWithObj?.Invoke(actorObj);

            if (!isActive) return null;

            Vector3 actorPosition = GetPlayerPosition(actorSelf.teamId);
            Quaternion actorRotation = GetPlayerRotation(actorSelf.teamId);

            GameObject newActorObject = Instantiate(actorObj, actorPosition, actorRotation);

            Actor newPlayerActorComponent = newActorObject.GetComponent<Actor>();
            Actor actorSelfActorComponent = actorSelf.gameObject.GetComponent<Actor>();

            //----
            Inventory inventory = newPlayerActorComponent.GetComponentsInChildren<MonoBehaviour>()
                .OfType<Inventory>()
                .FirstOrDefault();

            //SaveManager.LoadPlayer(inventory, _itemsPrefab);
            ReadPlayerWeapon(inventory);
            //----

            if (newPlayerActorComponent && actorSelfActorComponent)
            {
                newPlayerActorComponent.kills = actorSelfActorComponent.kills;
                newPlayerActorComponent.deaths = actorSelfActorComponent.deaths;
            }

            Vector3 position = GetPlayerPosition(actorSelf.teamId);
            Quaternion rotation = GetPlayerRotation(actorSelf.teamId);

            newActorObject.transform.SetPositionAndRotation(position, rotation);
            newActorObject.transform.parent = transform.parent;
            newActorObject.SetActive(true);

            return newActorObject;
        }

        private List<string> _weaponSave = new();

        public void WritePlayerWeapon(Actor player)
        {
            Firearm[] weapons = player.GetComponentsInChildren<Firearm>();

            for (int i = 0; i < weapons.Length; i++)
            {
                _weaponSave.Add(weapons[i].Name);
                Debug.Log("Weapon" + i.ToString() + " " + weapons[i].Name);
            }
        }

        public void ReadPlayerWeapon(Inventory inventory)
        {
            for (int i = 0; i < _maxWeaponCount; i++)
            {
                if (i >= _weaponSave.Count)
                    break;

                InventoryItem prefab = _itemsPrefab.FirstOrDefault(x => x.Name == _weaponSave[i]);

                if (prefab == null)
                    break;

                InventoryItem newWeapon = Instantiate(prefab, inventory.transform);
            }
        }

        public void SaveWeaponPlayer(Actor player)
        {
            Firearm[] weapons = player.GetComponentsInChildren<Firearm>();

            for (int i = 0; i < weapons.Length; i++)
            {
                PlayerPrefs.SetString("Weapon" + i, weapons[i].Name);
                Debug.Log("Weapon" + i.ToString() + " " + weapons[i].Name);
            }
        }

        public void LoadPlayerWeapon(Inventory inventory)
        {
            for (int i = 0; i < _maxWeaponCount; i++)
            {
                if (PlayerPrefs.HasKey("Weapon" + i))
                {
                    InventoryItem prefab = _itemsPrefab.FirstOrDefault(x => x.Name == PlayerPrefs.GetString("Weapon" + i));
                    InventoryItem newWeapon = Instantiate(prefab, inventory.transform);
                }
            }
        }

        public Transform GetPlayerSpawnPoint(int sideId)
        {
            //int pointIndex = Random.Range(0, sides[sideId].points.Length);

            return sides[sideId].points[_currentSpawnPointId];
        }

        public Vector3 GetPlayerPosition(int sideId)
        {
            Vector3 addedPosition = UnityEngine.Random.insideUnitCircle * spawnRadius;

            addedPosition.z = addedPosition.y;

            addedPosition.y = 0;

            return GetPlayerSpawnPoint(sideId).position + addedPosition;
        }

        public Quaternion GetPlayerRotation(int sideId)
        {
            return GetPlayerSpawnPoint(sideId).rotation;
        }

        private void OnDrawGizmos()
        {
            if (sides.Count == 0)
                return;

            foreach (SpwanSide point in sides)
            {
                foreach (Transform transform in point.points)
                {
                    if (transform == null)
                        continue;

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(transform.position, spawnRadius * transform.lossyScale.magnitude);
                }
            }
        }

        [ContextMenu("Setup/Network Components")]
        private void SetupNetworkComponents()
        {
            FPSFrameworkCore.InvokeConvertMethod("ConvertSpawnManager", this, new object[] { this });
        }

        [System.Serializable]
        public class SpwanSide
        {
            public Transform[] points;
        }

        [System.Serializable]
        public class SpwanableObject
        {
            public string name;
            public GameObject obj;
        }
    }
}