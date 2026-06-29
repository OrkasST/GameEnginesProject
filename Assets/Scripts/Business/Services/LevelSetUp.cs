using Assets.Scripts.Business.Lists;
using Assets.Scripts.Objects.Actors;

using Assets.Scripts.Business.Menus.Options;
//using Assets.Scripts.Objects.Door;
//using Assets.Scripts.Objects.Interactive;
using Assets.Scripts.Objects.Spawners;
//using Assets.Scripts.Objects.UI.StatusBars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Assets.Scripts.Objects.Actors.Enemy;
using Assets.Scripts.Business.SaveManagement;
using Assets.Scripts.Objects.Portals;

namespace Assets.Scripts.Business.Services
{
    public class LevelSetUp : Service
    {
        private Action<GameObject> _playerDeathHandler;
        private Func<GameObject, GameObject> _instantiateFunction;
        private Action _quitGame;
        private Action _proceedToNextLevel;
        public List<GameObject> Enemies = new List<GameObject>();
        public ActorController Player;

        private float _globalVolume;

        public void Init(
            Action<GameObject> playerDeathHandler,
            Func<GameObject, GameObject> instantiateFunc,
            Action quitGame, Action proceedToNextLevel)
        {
            _playerDeathHandler = playerDeathHandler;
            _instantiateFunction = instantiateFunc;
            _quitGame = quitGame;
            _proceedToNextLevel = proceedToNextLevel;
        }

        public void SetUp(Scene level, float volumeModifier, bool? startNewGame = true)
        {
            _globalVolume = volumeModifier;
            Enemies = new List<GameObject>();

            var objects = level.GetRootGameObjects();
            GameObject structure = null;
            GameObject player = null; // _instantiateFunction(Resources.Load<GameObject>("Prefabs/Actors/Player"));
            //SpawnManager spawnManager = null;
            Volume globalVolume = null;

            //StaminaBar staminaBar = null;
            OptionsMenuManager escapeMenu = null;

            foreach (var obj in objects)
            {
                if (obj.tag == Tags.LevelStructure)
                    structure = obj;
                else if (obj.tag == Tags.Player)
                    player = obj;
                else if (obj.tag == Tags.NPC)
                {
                    obj.GetComponent<ActorController>().Init(obj.transform.position);
                    obj.GetComponent<EnemyLogic>().SetID(Enemies.Count);
                    Enemies.Add(obj);
                    obj.GetComponent<ActorController>().SetDeathAction(() =>
                    {
                        Enemies[obj.GetComponent<EnemyLogic>().ID] = null;
                    });

                    if ((!SaveManager.HasSave && startNewGame.Value) || (SaveManager.HasSave && startNewGame.Value)
                        || level.name != SaveManager.GetLevel()) continue;

                    Debug.Log("SaveManager.GetEnemiesIDs(): " + SaveManager.GetEnemiesIDs().Length);
                    Debug.Log("obj.GetComponent<EnemyLogic>().ID: " + obj.GetComponent<EnemyLogic>().ID);

                    if (SaveManager.GetEnemiesIDs()[obj.GetComponent<EnemyLogic>().ID] < 0)
                        obj.GetComponent<ActorController>().StateMachine.RegisterStateChange(ActorState.Dieing);
                    else {
                        int index = obj.GetComponent<EnemyLogic>().ID * 3;

                        obj.transform.position = new Vector3(
                            SaveManager.GetEnemiesPositions()[index],
                            SaveManager.GetEnemiesPositions()[index + 1],
                            SaveManager.GetEnemiesPositions()[index + 2]
                            );

                        obj.GetComponent<ActorController>().StateMachine.RegisterStateChange((LookDirection)SaveManager.GetEnemiesLookDirection()[obj.GetComponent<EnemyLogic>().ID]);
                    }
                }
                else if (obj.name == "Global")
                {
                    globalVolume = obj.GetComponentInChildren<Volume>();
                }
                else if (obj.name == "Canvas")
                {
                    //staminaBar = obj.GetComponentInChildren<StaminaBar>();
                    escapeMenu = obj.GetComponentInChildren<OptionsMenuManager>();
                }
                else if (obj.name == "Cams")
                {
                    var cam = obj.GetComponentInChildren<CinemachineCamera>();
                    Debug.Log("cam: " + cam);
                    //cam.Follow = player.transform;
                }
            }

            Player = player.GetComponent<ActorController>();
            Player.Init(player.transform.position);//"Data/ActorsData/PlayerData", globalVolume, staminaBar);
            if (SaveManager.HasSave && !startNewGame.Value)
            {
                if (level.name == SaveManager.GetLevel())
                {
                    player.transform.position = new Vector3(SaveManager.GetPlayerPosition()[0], SaveManager.GetPlayerPosition()[1], SaveManager.GetPlayerPosition()[2]);

                    Player.StateMachine.RegisterStateChange(SaveManager.GetPlayerState());
                }

                Player.StateMachine.RegisterStateChange(SaveManager.GetPlayerLookDirection());
                Player.SetDamage(SaveManager.GetPlayerDamage());

                foreach (int i in SaveManager.GetPlayerInventory()) Player.ActorInventory.StoreItem(i);
            }
            //actorContoller.SetVolume(_globalVolume);

            //player.GetComponent<>().Menu = escapeMenu.gameObject;

            escapeMenu.SetUp(new Action<float>[] {
                (float value) => { _quitGame(); } //Quite game button
            });
            escapeMenu.gameObject.SetActive(false);

            if (structure == null) Debug.LogError("Null Structure");
            for (int i = 0; i < structure.transform.childCount; i++)
            {
                if (structure.transform.GetChild(i).tag == Tags.NextLevelPortal)
                {
                    structure.transform.GetChild(i).gameObject.SetActive(false);
                    structure.transform.GetChild(i).GetComponent<Portal>().Init(_proceedToNextLevel);
                }
                //    if (structure.transform.GetChild(i).gameObject.layer == Layers.Spawners)
                //        spawnManager = structure.transform.GetChild(i).GetComponent<SpawnManager>();
                //else structure.transform.GetChild(i).gameObject.SetActive(false);

                //var doors = structure.transform.GetChild(i).GetComponentsInChildren<Door>();
                //for (int j = 0; j < doors.Length; j++)
                //{
                //    doors[j].SetInteractionAreaEnterCallback((GameObject interactive) =>
                //    {
                //        actorContoller.CanInteractWithObject = true;
                //        actorContoller.ObjectToInteract = interactive.GetComponent<InteractiveObject>();
                //    });

                //    doors[j].SetInteractionAreaExitCallback(() =>
                //    {
                //        actorContoller.CanInteractWithObject = false;
                //        actorContoller.ObjectToInteract = null;
                //    });

                //    doors[j].Init(
                //        (GameObject currentRoom, GameObject targetRoom, ActorController actor, GameObject door, Action onFaded) =>
                //        {
                //            //player.GetComponent<ActorController>().SetCurrentRoom(targetRoom);
                //            actor.SetCurrentRoom(targetRoom);
                //            _onDoorEnterAction(currentRoom, targetRoom, actor, door, onFaded);
                //        });
                //}
            }
            player.GetComponent<ActorController>().SetDeathAction(() =>
            {
                _playerDeathHandler(player);
            });
            //spawnManager.SpawnPlayer(player);

            OnWorkFinished();
        }


    }
}
