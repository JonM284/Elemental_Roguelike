using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class LevelGenerationManager: MonoBehaviour
    {
        
        #region Nested Classes

        [Serializable]
        public class RoomDecorations
        {
            public Color associatedColor = Color.white;
            public bool isEnemySpawnPosition;
            public GameObject associatedPrefab;
        }

        #endregion

        #region Actions

        public static Action<RoomTracker> LevelGenerationFinished;

        #endregion

        #region Serialized Fields

        [SerializeField] private float checkerRadius;

        [SerializeField] private List<RoomTracker> allRooms = new List<RoomTracker>();
        
        [SerializeField] private RoomTracker startingRoom;
        
        [SerializeField] private GameObject roomPrefab;
        
        [SerializeField] private LevelGenerationRulesData levelGenerationRulesData;

        [SerializeField] private BattleGenerationRulesData battleGenerationRulesData;

        [Range(1,10)]
        [SerializeField] private int levelOfDifficulty = 1;

        [SerializeField] private LayerMask roomCheckLayer;
        
        [SerializeField] private LayerMask doorCheckLayer;

        [SerializeField] private Texture2D roomLayout;

        [SerializeField] private List<RoomDecorations> roomDecorations = new List<RoomDecorations>();

        #endregion

        #region Private Fields

        private RoomTracker _currentProcessRoom;

        private List<RoomTracker> _cachedRoomTrackers = new List<RoomTracker>();

        private Queue<RoomTracker> _roomsToCheck = new Queue<RoomTracker>();

        private LevelGenerationRulesData.RoomByPercentage _cachedRoomByPercentage;

        private Transform _checkerTransform;

        private Vector3 _doorCheckerPosition;

        private bool _isGeneratingRooms;

        private bool _isGeneratingLevel;

        private float _calcPercentage;

        private int _currentLevel;

        private Transform _inactiveRoomPool;

        private Transform _activeRoomPool;

        #endregion

        #region Accessors

        public Transform inactiveRoomPool =>
            CommonUtils.GetRequiredComponent(ref _inactiveRoomPool, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });
        
        public Transform activeRoomPool => CommonUtils.GetRequiredComponent(ref _activeRoomPool, ()=>
        {
            var poolTransform = TransformUtils.CreatePool(this.transform, true);
            poolTransform.RenameTransform("LevelRoomPool");
            return poolTransform;
        });

        public List<RoomTracker> levelRooms => allRooms;

        #endregion
        
        #region Unity Events

        private void OnDrawGizmos()
        {
            if (_checkerTransform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_checkerTransform.position, checkerRadius);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_doorCheckerPosition, checkerRadius);
            
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Level")]
        public void GenerateLevel()
        {
            if (!startingRoom && roomPrefab == null)
            {
                Debug.LogError("Starting room doesn't contain reference");
                return;
            }

            if (!startingRoom && roomPrefab != null)
            {
                var newStartingRoom = roomPrefab.Clone(activeRoomPool);
                var newStartingRoomTracker = newStartingRoom.GetComponent<RoomTracker>();
                startingRoom = newStartingRoomTracker;
            }

            if (allRooms.Count > 0)
            {
                allRooms.ForEach(rt =>
                {
                    rt.ResetRoom();
                    if (rt != startingRoom)
                    {
                        _cachedRoomTrackers.Add(rt);
                        rt.transform.ResetTransform(inactiveRoomPool);   
                        rt.gameObject.SetActive(true);
                    }
                });
                allRooms.Clear();
            }

            if (_roomsToCheck.Count > 0)
            {
                _roomsToCheck.Clear();
            }

            _cachedRoomByPercentage = null;
            
            allRooms.Add(startingRoom);
            _roomsToCheck.Enqueue(startingRoom);
            
            startingRoom.modifiableDoorCheckers.ForEach(dc => dc.AssignWallDoor(DoorType.DOOR));

            _isGeneratingLevel = true;

            StartCoroutine(GenerateRooms());
        }

        private IEnumerator GenerateRooms()
        {
            while (_isGeneratingLevel)
            {
                var currentRoom = _roomsToCheck.Dequeue();
                if (!currentRoom)
                {
                    yield break;
                }

                _currentLevel = currentRoom.level;
                _calcPercentage = _currentLevel / levelOfDifficulty;
                if (_currentLevel <= levelOfDifficulty)
                {
                    
                    //One door rooms can not generate new rooms
                    if (currentRoom.roomType == RoomType.ONE_DOOR)
                    {
                        yield return null;
                    }
                    
                    _currentProcessRoom = currentRoom;
                
                    //Generate rooms connected to current room
                    //Current room puts up doors
                    foreach (var doorChecker in currentRoom.modifiableDoorCheckers)
                    {
                        _isGeneratingRooms = true;
                        _checkerTransform = doorChecker.roomChecker; 
                        GenerateConnectingRoom(doorChecker);
                        yield return new WaitUntil(() => !_isGeneratingRooms);
                    }
                
                }

                yield return null;
                
                
            
                if (_roomsToCheck.Count == 0)
                {
                    _isGeneratingLevel = false;
                    Debug.Log("Level generation finished");
                    LevelGenerationFinished?.Invoke(startingRoom);
                    yield break;
                }

            }

        }
        
        private void GenerateConnectingRoom(DoorChecker m_doorChecker)
        {
            Collider[] colliders = Physics.OverlapSphere(_checkerTransform.position, checkerRadius, roomCheckLayer);
            
            //If there is another room already in this direction
            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    var _roomTracker = collider.GetComponent<RoomTracker>();
                    if (_roomTracker != null)
                    {
                        CheckDoorArea(m_doorChecker, _roomTracker);
                    }
                }
                _isGeneratingRooms = false;
                return;
            }
            
            m_doorChecker.AssignWallDoor(DoorType.DOOR);
            SpawnNewRoom(m_doorChecker);
        }

        private void SpawnNewRoom(DoorChecker _currentDoorChecker)
        {
            var m_newRoom = _cachedRoomTrackers.Count > 0
                ? _cachedRoomTrackers[0].gameObject
                : roomPrefab.Clone(activeRoomPool);

            if (m_newRoom.transform.parent != activeRoomPool)
            {
                m_newRoom.transform.ResetTransform(activeRoomPool);
            }
            
            m_newRoom.transform.ResetPRS(_checkerTransform);
            
            var m_newRoomTracker = m_newRoom.GetComponent<RoomTracker>();
            if (!m_newRoomTracker)
            {
                Debug.LogError("New Room Tracker doesn't exist");
                _isGeneratingRooms = false;
                return;
            }

            if (_cachedRoomTrackers.Count > 0 && _cachedRoomTrackers.Contains(m_newRoomTracker))
            {
                _cachedRoomTrackers.Remove(m_newRoomTracker);
            }
            
            var m_randomRoomTypeByRule = GetRoomTypeByRule();
            var m_roomTypeByWeight = GetRoomTypeByWeight(m_randomRoomTypeByRule);

            m_newRoomTracker.roomType = m_roomTypeByWeight;
            m_newRoomTracker.level = _currentLevel + 1;

            //If the room is single door, it does not have to be processed
            if (m_newRoomTracker.roomType != RoomType.ONE_DOOR)
            {
                _roomsToCheck.Enqueue(m_newRoomTracker);
            }

            allRooms.Add(m_newRoomTracker);
            _currentDoorChecker.AssignConnectedRoom(m_newRoomTracker);
            
            var battleEnemies = battleGenerationRulesData.GetBattleEnemies();
            m_newRoomTracker.AssignBattle(battleEnemies);

            ManageRoomDoors(m_newRoomTracker);
        }

        /// <summary>
        /// Place door type depending on rule
        /// Found: DOOR => place: DOORWAY
        /// Found: WALL => place: WALL
        /// Found: Nothing => place: DOOR
        /// </summary>
        /// <param name="m_doorChecker">Check door in direction to this room</param>
        private void CheckDoorArea(DoorChecker currentCheckDoor, RoomTracker _roomInDirection)
        {
            var _dirToCurrentRoom =  _roomInDirection.transform.position.FlattenVector3Y() - _currentProcessRoom.transform.position.FlattenVector3Y();

            DoorChecker m_connectedDoor = null;
            
            //Walls face in towards parent room
            foreach (var m_doorChecker in _roomInDirection.modifiableDoorCheckers)
            {
                var dot = Vector3.Dot(m_doorChecker.transform.forward, _dirToCurrentRoom);
                if (dot >= 0.9f)
                {
                    m_connectedDoor = m_doorChecker;
                    break;
                }
            }

            if (m_connectedDoor == null || m_connectedDoor.doorType == DoorType.WALL)
            {
                currentCheckDoor.AssignWallDoor(DoorType.WALL);
                return;
            }
            
            currentCheckDoor.AssignWallDoor(DoorType.DOOR);
            currentCheckDoor.AssignConnectedRoom(_roomInDirection);
        }

        private void ManageRoomDoors(RoomTracker m_roomTracker)
        {
            if (!m_roomTracker)
            {
                _isGeneratingRooms = false;
                return;
            }

            //Find connecting door between current room and created room
            var m_dirToCurrentRoom =  m_roomTracker.transform.position.FlattenVector3Y() - _currentProcessRoom.transform.position.FlattenVector3Y();

            DoorChecker m_connectedDoor = null;
            
            //Walls face in towards parent room
            foreach (var m_doorChecker in m_roomTracker.modifiableDoorCheckers)
            {
                var dot = Vector3.Dot(m_doorChecker.transform.forward, m_dirToCurrentRoom);
                if (dot >= 0.9f)
                {
                    m_connectedDoor = m_doorChecker;
                    break;
                }
            }
            
            //remove door from door checkers
            if (m_connectedDoor)
            {
                m_connectedDoor.AssignWallDoor(DoorType.DOOR);
                m_connectedDoor.AssignConnectedRoom(_currentProcessRoom);
                m_roomTracker.modifiableDoorCheckers.Remove(m_connectedDoor);
            }

            for (int x = 0; x < roomLayout.width; x++)
            {
                for (int z = 0;z < roomLayout.height; z++)
                {
                    Color pixelColor = roomLayout.GetPixel(x, z);
                    if (pixelColor.a == 0)
                    {
                        continue;
                    }

                    foreach (var decoration in roomDecorations)
                    {
                        if (pixelColor == decoration.associatedColor)
                        {
                           var newObject = decoration.associatedPrefab.Clone(m_roomTracker.decorationTransform);
                           if (decoration.isEnemySpawnPosition)
                           {
                               m_roomTracker.AddEnemySpawnPos(newObject.transform);
                           }
                           var newY = newObject.transform.localScale.y / 2;
                           newObject.transform.localPosition = new Vector3(x, newY, z);
                        }
                        
                    }
                }
            }

            //If the room only has one door, the one door is the connecting door
            if (m_roomTracker.roomType == RoomType.ONE_DOOR || m_roomTracker.roomType == RoomType.FOUR_DOOR)
            {
                
                //If the room has only one door, this door is the connecting door -> all other sides become walls
                if (m_roomTracker.roomType == RoomType.ONE_DOOR)
                {
                    m_roomTracker.modifiableDoorCheckers.ForEach(dc => dc.AssignWallDoor(DoorType.WALL));
                }
                
                
                _isGeneratingRooms = false;
                return;
            }

            //Int from enum type will determine how many doors to remove
            var m_roomTypeInt = (int)m_roomTracker.roomType;

            //Add random walls depending on room type
            for (int i = 0; i < m_roomTypeInt; i++)
            {
                int m_randomInt = Random.Range(0, m_roomTracker.modifiableDoorCheckers.Count);
                var m_selectedDoor = m_roomTracker.modifiableDoorCheckers[m_randomInt];
                m_selectedDoor.AssignWallDoor(DoorType.WALL);
                m_roomTracker.modifiableDoorCheckers.Remove(m_selectedDoor);
            }
            
            _isGeneratingRooms = false;
            
        }

        private LevelGenerationRulesData.RoomByPercentage GetRoomTypeByRule()
        {
            //default value
            LevelGenerationRulesData.RoomByPercentage m_roomByPercentages = levelGenerationRulesData.defaultRoomByPercentage;

            if (_cachedRoomByPercentage != null && _calcPercentage <= _cachedRoomByPercentage.percentage)
            {
                return _cachedRoomByPercentage;
            }

            for (int i = 0; i < levelGenerationRulesData.roomByPercentages.Count; i++)
            {
                if (i == 0)
                {
                    //Less than lowest percentage
                    if (_calcPercentage <= levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }else if(i == levelGenerationRulesData.roomByPercentages.Count - 1)
                {
                    //higher than second highest percentage
                    if (_calcPercentage >= levelGenerationRulesData.roomByPercentages[i].percentage ||
                        _calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
                else
                {
                    //falls in between two percentages = higher percentage rule
                    if (_calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage &&
                        _calcPercentage < levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
            }

            _cachedRoomByPercentage = m_roomByPercentages;
            return m_roomByPercentages;
        }

        public RoomType GetRoomTypeByWeight(LevelGenerationRulesData.RoomByPercentage m_roomByPercentage)
        {
            //default value
            RoomType endValue = RoomType.FOUR_DOOR;

            var listOfPossibleRooms = m_roomByPercentage;
            
            //get random weight value from room weight
            var totalWeight = 0;
            foreach (var roomByWeight in listOfPossibleRooms.roomsByWeights)
            {
                totalWeight += roomByWeight.weight;
            }
            
            //+1 because max value is exclusive
            var randomValue = Random.Range(1, totalWeight + 1);

            var currentWeight = 0;
            foreach (var roomByWeight in listOfPossibleRooms.roomsByWeights)
            {
                currentWeight += roomByWeight.weight;
                if (randomValue <= currentWeight)
                {
                    endValue = roomByWeight.roomType;
                    break;
                }
            }

            return endValue;
        }

        #endregion

    }
}