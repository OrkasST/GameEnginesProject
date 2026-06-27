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

namespace Assets.Scripts.Business.Services
{
    public class LevelSetUp : Service
    {
        //private Action<GameObject, GameObject, ActorController, GameObject, Action> _onDoorEnterAction;
        private Action<SpawnManager, GameObject> _playerDeathHandler;
        private Func<GameObject, GameObject> _instantiateFunction;
        private Action _quitGame;

        private float _globalVolume;

        public void Init(
            //Action<GameObject, GameObject, ActorController, GameObject, Action> onDoorEnterAction,
            Action<SpawnManager, GameObject> playerDeathHandler,
            Func<GameObject, GameObject> instantiateFunc, Action quitGame)
        {
            //_onDoorEnterAction = onDoorEnterAction;
            _playerDeathHandler = playerDeathHandler;
            _instantiateFunction = instantiateFunc;
            _quitGame = quitGame;
        }

        public void SetUp(Scene level, float volumeModifier)
        {
            _globalVolume = volumeModifier;

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
                    var cam = obj.GetComponentInChildren<CinemachineCamera>().Follow = player.transform;
                }
            }

            var actorContoller = player.GetComponent<ActorController>();
            actorContoller.Init();//"Data/ActorsData/PlayerData", globalVolume, staminaBar);
            //actorContoller.SetVolume(_globalVolume);

            //player.GetComponent<>().Menu = escapeMenu.gameObject;

            escapeMenu.SetUp(new Action<float>[] {
                (float value) => { _quitGame(); }, //Quite game button
                (float value) => { _globalVolume = value; /*actorContoller.SetVolume(value); */ }, //adjust Volume
            });
            escapeMenu.gameObject.SetActive(false);

            if (structure == null) Debug.LogError("Null Structure");
            //for (int i = 0; i < structure.transform.childCount; i++)
            //{
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
            //}
            //player.GetComponent<ActorController>().OnDeath = () =>
            //{
            //    _playerDeathHandler(spawnManager, player);
            //};
            //spawnManager.SpawnPlayer(player);

            OnWorkFinished();
        }
    }
}
