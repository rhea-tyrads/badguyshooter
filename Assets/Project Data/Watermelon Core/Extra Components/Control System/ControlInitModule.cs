﻿using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Modules/Control Manager")]
    public class ControlInitModule : InitModule
    {
        [SerializeField] bool selectAutomatically = true;

        [HideIf("selectAutomatically")]
        [SerializeField] InputType inputType;

        [HideIf("IsJoystickCondition")]
        [SerializeField] GamepadData gamepadData;

        public ControlInitModule()
        {
            moduleName = "Control Manager";
        }

        public override void CreateComponent(Initialiser Initialiser)
        {
            if (selectAutomatically)
                inputType = ControlUtils.GetCurrentInputType();

            Control.Initialise(inputType, gamepadData);

            if(inputType == InputType.Keyboard)
            {
                var keyboardControl = Initialiser.InitialiserGameObject.AddComponent<KeyboardControl>();
                keyboardControl.Initialise();
            } 
            else if(inputType == InputType.Gamepad)
            {
                var gamepadControl = Initialiser.InitialiserGameObject.AddComponent<GamepadControl>();
                gamepadControl.Initialise();
            }
        }

        bool IsJoystickCondition()
        {
            return selectAutomatically ? ControlUtils.GetCurrentInputType() == InputType.UIJoystick : inputType == InputType.UIJoystick;
        }
    }
}