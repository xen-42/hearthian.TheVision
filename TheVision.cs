﻿using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using TheVision.Utilities.ModAPIs;
using static NewHorizons.External.Modules.PropModule;
using NewHorizons.Builder.Props;
using System.Linq;
using TheVision.CustomProps;
using HarmonyLib;
using System.Reflection;
using NewHorizons.Utility;


namespace TheVision
{
    public class TheVision : ModBehaviour
    {
        public static INewHorizons newHorizonsAPI;
        public static TheVision Instance;

        public OWAudioSource PlayerHeadsetAudioSource;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            var newHorizonsAPI = ModHelper.Interaction.GetModApi<INewHorizons>("xen.NewHorizons");
            newHorizonsAPI.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
            newHorizonsAPI.LoadConfigs(this);

            ModHelper.Console.WriteLine($"My mod {nameof(TheVision)} is loaded!", MessageType.Success);

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                ModHelper.Console.WriteLine($"Found player body, and it's called {playerBody.name}!",
                MessageType.Success);

            };

        }

        private static void SpawnSolanumProps()
        {                 
           
            GameObject visionTarget = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/VisionStaffDetector");

            visionTarget.transform.parent =
                GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon")
                .GetComponentsInChildren<Transform>(true)
                .Where(t => t.gameObject.name == "State_EYE")
                .First(); // All because Find doesn't work on inactive game objects :/


            //parenting particles to Solanum
            var QMparticles = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/Effects_NOM_WarpParticles(Clone)");
            QMparticles.transform.parent = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot").transform.Find("Character_NOM_Solanum");

            //renaming TH recorder idk why but I needed it for some reason lol
            var THrecorder = GameObject.Find("TimberHearth_Body/Sector_TH/Prefab_NOM_Recorder(Clone)");
            THrecorder.transform.name = "Prefab_NOM_Recorder(Clone)_TH";

            // Making custom text for reply           
            NomaiWallText responseText = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/NomaiWallText").GetComponent<NomaiWallText>();
            responseText.HideTextOnStart();
            responseText.transform.parent = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon").transform.Find("State_EYE");

            //parenting QM ground text to TH state
            var QMgroundText = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/NomaiWallText");
            QMgroundText.transform.parent = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon").transform.Find("State_TH");

            var nomaiConversationManager = Resources.FindObjectsOfTypeAll<NomaiConversationManager>().First(); //GameObject.FindObjectOfType<NomaiConversationManager>();
            var myConversationManager = nomaiConversationManager.gameObject.AddComponent<TheVision_SolanumVisionResponse>();
            myConversationManager._nomaiConversationManager = nomaiConversationManager;
            myConversationManager._solanumAnimController = nomaiConversationManager._solanumAnimController;
            myConversationManager.solanumVisionResponse = responseText;

            visionTarget.GetComponent<VisionTorchTarget>().onSlidesComplete = myConversationManager.OnVisionEnd;

            // Replacing new Hologram
            var origHologram = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/VesselHologram_EyeSignal");
            var hologramClone = GameObject.Instantiate(origHologram);
            hologramClone.transform.parent = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension").transform.Find("Sector_VesselBridge");
            hologramClone.transform.position = origHologram.transform.position;
            hologramClone.transform.rotation = origHologram.transform.rotation;
            var mat = hologramClone.GetComponent<MeshRenderer>().material;
            mat.SetTexture("_MainTex", TheVision.Instance.ModHelper.Assets.GetTexture("images/NewHologram.png"));
            hologramClone.GetComponent<MeshRenderer>().sharedMaterial = mat;
            hologramClone.SetActive(false);


            // I don't like it. It's better place for torch
            GameObject.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Props_SecretEntrance/OtherComponentsGroup/Props_IP_WrenchStaff").SetActive(false);

        }

        // Load SolanumProps
        public void OnStarSystemLoaded(string systemName)
        {
            ModHelper.Console.WriteLine("LOADED SYSTEM " + systemName);

            if (systemName == "SolarSystem")
            {
                SpawnSolanumProps();
                SpawnVisionTorch(); // then DELETE when everything is ready
                DisabledPropsOnStart(false);

            }
        }

        //Bars to spawn SolanumCopies
        public void SpawnSolanumCopy(INewHorizons newHorizonsAPI)
        {
            // Spawning Solanum on TH
            string path = "QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot/Character_NOM_Solanum/Nomai_ANIM_SkyWatching_Idle";
            Vector3 position = new Vector3(48.5018f, 15.1183f, 249.9972f);
            Vector3 rotation = new Vector3(332.5521f, 279.0402f, 275.7439f);
            newHorizonsAPI.SpawnObject(Locator._timberHearth.gameObject, Locator._timberHearth.GetRootSector(), path, position, rotation, 1, false);

            // Spawning Solanum on GD
            string path2 = "QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot/Character_NOM_Solanum/Nomai_ANIM_SkyWatching_Idle";
            Vector3 position2 = new Vector3(-43.62191f, -68.5414f, -31.2553654f);
            Vector3 rotation2 = new Vector3(350.740326f, 50.80401f, 261.666534f);
            newHorizonsAPI.SpawnObject(Locator._giantsDeep.gameObject, Locator._giantsDeep.GetRootSector(), path2, position2, rotation2, 1, false);

            

           

        }
        // Spawning Vision Torch with code
        public void SpawnVisionTorch()
        {

            var path = "DreamWorld_Body/Sector_DreamWorld/Sector_Underground/Sector_PrisonCell/Interactibles_PrisonCell/PrisonerSequence/VisionTorchWallSocket/Prefab_IP_VisionTorchItem";
            Vector3 position = new Vector3(18.06051f, -50.64357f, 183.141f);
            Vector3 rotation = new Vector3(311.8565f, 287.9388f, 254.72f);
            GameObject staff = DetailBuilder.MakeDetail(Locator._timberHearth.gameObject, Locator._timberHearth.GetRootSector(), path, position, rotation, 1, false);

        }
        public NewHorizons.External.Modules.SignalModule.SignalInfo MakeSolanumSignalInfo(Vector3 position)
        {

            //Solanum signal parameters, reveal ship log about new quantum rule
            return new NewHorizons.External.Modules.SignalModule.SignalInfo()
            {
                audioFilePath = "planets/quantum.wav",
                frequency = "Quantum Consciousness",
                detectionRadius = 20000,
                identificationRadius = 2000,
                sourceRadius = 2f,
                name = "Solanum",
                position = position,
                onlyAudibleToScope = false,
                reveals = "WHAT_IS_NEW_QR",

            };
        }

        public void SpawnSignals()

        {
            // Playing SFX on Vision End
            PlayerHeadsetAudioSource = GameObject.Find("Player_Body").AddComponent<OWAudioSource>();
            PlayerHeadsetAudioSource.enabled = true;
            PlayerHeadsetAudioSource.AssignAudioLibraryClip((AudioType)2400);
            PlayerHeadsetAudioSource.Play();

            //Enabling props that spawned with json I guess 
            DisabledPropsOnStart(true);

            //placing orb on GD to the slot (1)
            var nomaiSlot = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Interactables_Module_Sunken/OrbInterface/Slots/Slot (1)");
            var nomaiInterfaceOrb = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Interactables_Module_Sunken/OrbInterface/Prefab_NOM_InterfaceOrb");
            var nomaiCorrectSlot = nomaiInterfaceOrb.GetComponent<NomaiInterfaceOrb>();
            var nomaiCorrectSlot2 = nomaiCorrectSlot.GetComponent<OWRigidbody>();
            nomaiCorrectSlot.SetOrbPosition(nomaiSlot.transform.position);
            nomaiCorrectSlot._orbBody.ChangeSuspensionBody(nomaiCorrectSlot2);

            //decloaking QM on signals spawn
            GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Clouds_QM_EyeState").SetActive(false);
            GameObject.Find("QuantumMoon_Body/Atmosphere_QM/FogSphere").SetActive(false);

            //disabling recorder on QM Solanum shuttle
            GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/QuantumShuttle/Prefab_NOM_Shuttle/Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder").SetActive(false);
                     

            //Spawning Solanum signals
            SignalBuilder.Make(Locator._timberHearth.gameObject, Locator._timberHearth.GetRootSector(), MakeSolanumSignalInfo(new Vector3(48.5018f, 15.1183f, 249.9972f)), TheVision.Instance);
            SignalBuilder.Make(Locator._quantumMoon.gameObject, Locator._quantumMoonAstroObj.GetRootSector(), MakeSolanumSignalInfo(new Vector3(-5.254965f, -70.73996f, 1.607201f)), TheVision.Instance);
            SignalBuilder.Make(Locator._giantsDeep.gameObject, Locator._giantsDeep.GetRootSector(), MakeSolanumSignalInfo(new Vector3(-43.62191f, -68.5414f, -31.2553654f)), TheVision.Instance);
            SignalBuilder.Make(Locator._darkBramble.gameObject, Locator._darkBramble.GetRootSector(), MakeSolanumSignalInfo(new Vector3(148.221f, 25.8914f, -0.2369f)), TheVision.Instance);



            //parenting QM signal to Solanum
            var QMsignal = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/Signal_Solanum");
            QMsignal.transform.parent = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot").transform.Find("Character_NOM_Solanum");

            //Enabling hologram
            GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/VesselHologram_EyeSignal").SetActive(false);
            GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/VesselHologram_EyeSignal(Clone)").SetActive(true);

            

        }

        //Props from Json files (recorders mostly)
        public void DisabledPropsOnStart(bool isActive)
        {
            GameObject QMgroundText = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/State_TH/NomaiWallText");            

            GameObject THrecorder = GameObject.Find("TimberHearth_Body/Sector_TH/Prefab_NOM_Recorder(Clone)_TH");            

            GameObject GDrecorder = GameObject.Find("GiantsDeep_Body/Sector_GD/Prefab_NOM_Recorder(Clone)");            

            GameObject DBrecorder = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Prefab_NOM_Recorder(Clone)");            

            GameObject solanumDB = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Nomai_ANIM_SkyWatching_Idle(Clone)");

            GameObject signalDB = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Signal_Solanum");

            GameObject particlesTH = GameObject.Find("TimberHearth_Body/Sector_TH/Effects_NOM_WarpParticles(Clone)");

            GameObject particlesGD = GameObject.Find("GiantsDeep_Body/Sector_GD/Effects_NOM_WarpParticles(Clone)");

            GameObject particlesQM = GameObject.Find("QuantumMoon_Body/Sector_QuantumMoon/State_EYE/Interactables_EYEState/ConversationPivot/Character_NOM_Solanum/Effects_NOM_WarpParticles(Clone)");

            GameObject particlesDB = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Effects_NOM_WarpParticles(Clone)");

            //GameObject particlesATP = GameObject.Find("TimeLoopRing_Body/Effects_NOM_WarpParticles(Clone)");

            //GameObject signalATP = GameObject.Find("TimeLoopRing_Body/Signal_Solanum");

            //GameObject solanumATP = GameObject.Find("TimeLoopRing_Body/Nomai_ANIM_SkyWatching_Idle(Clone)");

            //GameObject ATPrecorder = GameObject.Find("TimeLoopRing_Body/Prefab_NOM_Recorder(Clone)");

            if (isActive == false)
            {
                QMgroundText.SetActive(false);
                THrecorder.SetActive(false);
                GDrecorder.SetActive(false);
                DBrecorder.SetActive(false);
                solanumDB.SetActive(false);
                signalDB.SetActive(false);
                particlesTH.SetActive(false);
                particlesGD.SetActive(false);
                particlesQM.SetActive(false);
                particlesDB.SetActive(false);

                // particlesATP.SetActive(false);
                // solanumATP.SetActive(false);
                // signalATP.SetActive(false);
                // ATPrecorder.SetActive(false);
            }
            else
            {
                QMgroundText.SetActive(true);
                THrecorder.SetActive(true);
                GDrecorder.SetActive(true);                
                DBrecorder.SetActive(true);                
                solanumDB.SetActive(true);                
                signalDB.SetActive(true);
                particlesTH.SetActive(true);
                particlesGD.SetActive(true);
                particlesQM.SetActive(true);
                particlesDB.SetActive(true);

                // particlesATP.SetActive(false);
                // solanumATP.SetActive(true);
                // signalATP.SetActive(true);
                // ATPrecorder.SetActive(true);
            }
        }
       
    }
}




















