#region
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace QuestPlayspaceMover
{
    public static class ModInfo
    {
        public const string Name = "QuestPlayspaceMover";
        public const string Description = "PlayspaceMover to oculus Quest2";
        public const string Author = "Rafa (original for Desktop)/Solexid(fixes for quest)";
        public const string Company = "";
        public const string Version = "1.0.0";
        public const string DownloadLink = "";
    }

    public class Main : MelonMod
    {
        public override void OnApplicationStart()
        {

            MelonCoroutines.Start(ModInit());

            IEnumerator ModInit()
            {
                // Wait For UI
                while (!VRCUiManager.field_Private_Static_VRCUiManager_0) yield return null; // Wait Till VRCUIManger Isnt Null
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
                    if (obj.name.Contains("UserInterface"))
                        UserInterfaceObj = obj;

                // Wait For QM
                while (!UserInterfaceRef.GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true)) yield return null;
                new WaitForSeconds(0.7f); // Waits to Prevent Breakage
                MenuStart();

                // Init The Rest Lol
                var objects = UnityEngine.Object.FindObjectsOfType(UnhollowerRuntimeLib.Il2CppType.Of<OVRCameraRig>());
                MelonLogger.Warning(objects.Count);

                if (objects != null && objects.Length > 0)
                {
                    Camera = objects[0].TryCast<OVRCameraRig>();
                    StartPosition = Camera.trackingSpace.localPosition;
                }
                OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
                OVRManager.useDynamicFixedFoveatedRendering = true;
                startingOffset = new Vector3(0, 0, 0);
                MelonLogger.Error("OVRCameraRig not found, this mod only work on Oculus! If u are using SteamVR, use the OVR Advanced Settings!");

                yield break;
            }
        }

        private OVRCameraRig Camera;
        private OVRInput.Controller LastPressed;
        private Vector3 startingOffset;
        private Vector3 StartPosition;
        public static GameObject UserInterfaceObj = null;
        public int leftspeed = 5;
        public int rightspeed = 5;

        public override void OnUpdate()
        {
            if (!Camera) return;

            if ((HasDoubleClicked(OVRInput.Button.PrimaryThumbstick, 0.25f) || HasDoubleClicked(OVRInput.Button.SecondaryThumbstick, 0.25f)))
                Camera.trackingSpace.localPosition = StartPosition; 

            bool isLeftPressed = IsKeyJustPressed(OVRInput.Button.PrimaryThumbstick);
            bool isRightPressed = IsKeyJustPressed(OVRInput.Button.SecondaryThumbstick);
            if(isLeftPressed)
            {
                startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                LastPressed = OVRInput.Controller.LTouch;
            }
            if (isRightPressed)
            {
                startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                LastPressed = OVRInput.Controller.LTouch;
            }

            bool leftTrigger = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
            if (leftTrigger && LastPressed == OVRInput.Controller.LTouch)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (startingOffset * leftspeed) - (currentOffset * leftspeed);

                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
            }

            if (rightTrigger && LastPressed == OVRInput.Controller.RTouch)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (startingOffset * rightspeed) - (currentOffset * rightspeed);

                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
            }
        }

        public void MenuStart()
        {
            new Wings("Playspace +", "Wing_Left", () =>
            {
               leftspeed++;
            });
            new Wings("Playspace -", "Wing_Left", () =>
            {
               leftspeed--;
            });
            new Wings("Playspace Reset", "Wing_Left", () =>
            {
                leftspeed = 5;
            });
            new Wings("Playspace +", "Wing_Right", () =>
            {
               rightspeed++;
            });
            new Wings("Playspace -", "Wing_Right", () =>
            {
               rightspeed--;
            });
            new Wings("Playspace Reset", "Wing_Right", () =>
            {
                rightspeed = 5;
            });
        }
      

        private static readonly Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>
        {
            { OVRInput.Button.PrimaryThumbstick, false }, { OVRInput.Button.SecondaryThumbstick, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
       
            if (!PreviousStates.ContainsKey(key))
                PreviousStates.Add(key, false);

            return PreviousStates[key] = OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key];
        }

        private static readonly Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

        // Thanks to Psychloor!
        // https://github.com/Psychloor/DoubleTapRunner/blob/master/DoubleTapSpeed/Utilities.cs#L30
        public static bool HasDoubleClicked(OVRInput.Button keyCode, float threshold)
        {
            if (!OVRInput.GetDown(keyCode, OVRInput.Controller.Touch))
                return false;

            if (!lastTime.ContainsKey(keyCode))
                lastTime.Add(keyCode, Time.time);

            if (Time.time - lastTime[keyCode] <= threshold)
            {
                lastTime[keyCode] = threshold * 2;
                return true;
            }

            lastTime[keyCode] = Time.time;
            return false;
        }
    }
    public class Wings
    {
        public Wings(string name, string side, Action onClick)
        {
            var toinst = Main.UserInterfaceObj.transform.Find($"Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/{side}/Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Emotes");
            var inst = GameObject.Instantiate(toinst, toinst.parent).gameObject;

            var txt = inst.transform.Find("Container/Text_QM_H3").GetComponent<TMPro.TextMeshProUGUI>();
            txt.richText = true;
            txt.text = $"{name}-{Random.Range(100, 99999)}";

            GameObject.DestroyImmediate(inst.transform.Find("Container/Icon").gameObject);

            var btn = inst.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(new Action(onClick));
        }
    }
}