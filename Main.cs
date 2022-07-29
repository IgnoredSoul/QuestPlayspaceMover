#region
using MelonLoader;
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
        #region Settings
     
      
        #endregion
        
        public override void OnApplicationStart()
        {
  

            MelonCoroutines.Start(WaitInitialization());
        }

 
      
        
        private OVRCameraRig Camera;
        private OVRInput.Controller LastPressed; 
        private Vector3 startingOffset;
        private Vector3 StartPosition;

        private IEnumerator WaitInitialization()
        {
            // Wait for the VRCUiManager
            while (VRCUiManager.prop_VRCUiManager_0 == null)
            {
                yield return new WaitForFixedUpdate();
                MelonLogger.Warning("-------------------------------------------------------------------------------------------------------");
            }
   

            var objects = Object.FindObjectsOfType(UnhollowerRuntimeLib.Il2CppType.Of<OVRCameraRig>());

            MelonLogger.Warning(objects.Count);
            if (objects != null && objects.Length > 0)
            {
             
                   Camera = objects[0].TryCast<OVRCameraRig>();
                StartPosition = Camera.trackingSpace.localPosition;
                yield break;
            }
            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
            OVRManager.useDynamicFixedFoveatedRendering = true;
            startingOffset = new Vector3(0, 0, 0);
            MelonLogger.Error("OVRCameraRig not found, this mod only work on Oculus! If u are using SteamVR, use the OVR Advanced Settings!");
        }
     
        public override void OnUpdate()
        {
            if (Camera == null)
            {
            
                return;
            }
           
            if ((HasDoubleClicked(OVRInput.Button.PrimaryThumbstick, 0.25f) || HasDoubleClicked(OVRInput.Button.SecondaryThumbstick, 0.25f)))
            {
              
                Camera.trackingSpace.localPosition = StartPosition;
                return;
            }

            bool isLeftPressed = IsKeyJustPressed(OVRInput.Button.PrimaryThumbstick);
            bool isRightPressed = IsKeyJustPressed(OVRInput.Button.SecondaryThumbstick);
            if (isLeftPressed || isRightPressed)
            {



                if (isLeftPressed)
                {
                    startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);

                    LastPressed = OVRInput.Controller.LTouch;
                }
                else if (isRightPressed)
                {
                    startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

                    LastPressed = OVRInput.Controller.RTouch;
                }
            }



            bool leftTrigger = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

            if (leftTrigger && LastPressed == OVRInput.Controller.LTouch )
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (startingOffset * 1) -( currentOffset * 1) ;
  
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
               
            }

            if (rightTrigger && LastPressed == OVRInput.Controller.RTouch )
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (startingOffset * 5) - (currentOffset * 5); ;
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
               
            }

        
           
        }

        private static readonly Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>
        {
            { OVRInput.Button.PrimaryThumbstick, false }, { OVRInput.Button.SecondaryThumbstick, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
       
            if (!PreviousStates.ContainsKey(key))
            {
                PreviousStates.Add(key, false);
            }

            return PreviousStates[key] = OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key];
        }

        private static readonly Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

        // Thanks to Psychloor!
        // https://github.com/Psychloor/DoubleTapRunner/blob/master/DoubleTapSpeed/Utilities.cs#L30
        public static bool HasDoubleClicked(OVRInput.Button keyCode, float threshold)
        {
            if (!OVRInput.GetDown(keyCode, OVRInput.Controller.Touch))
            {
                return false;
            }

            if (!lastTime.ContainsKey(keyCode))
            {
                lastTime.Add(keyCode, Time.time);
            }

            if (Time.time - lastTime[keyCode] <= threshold)
            {
                lastTime[keyCode] = threshold * 2;
                return true;
            }

            lastTime[keyCode] = Time.time;
            return false;
        }
    }
}
