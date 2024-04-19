using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Triggers
{
    /// <summary>
    /// A reworked version of <see cref="CheatListener"/> that's more lenient 
    /// with overlapping inputs.
    /// </summary>
    public class CustomCheatListener : Entity
    {
        public string CurrentCheatInput;
        private int currentIndex;

        private List<(char, VirtualButton)> inputs;

        private string cheatCode;
        private Action OnEntered;

        public CustomCheatListener(string code, Action onEntered, bool ignoreDirections = false)
        {
            cheatCode = code;
            OnEntered = onEntered;
            Visible = false;
            CurrentCheatInput = "";

            inputs = new();

            if (!ignoreDirections)
            {
                AddInput('u', Input.MenuUp);
                AddInput('d', Input.MenuDown);
                AddInput('l', Input.MenuLeft);
                AddInput('r', Input.MenuRight);
            }
            AddInput('c', Input.MenuConfirm);
            AddInput('b', Input.MenuCancel);
            AddInput('j', Input.MenuJournal);
            AddInput('G', Input.Grab);
            AddInput('J', Input.Jump);
            AddInput('D', Input.Dash);
            AddInput('Z', Input.CrouchDash);
            AddInput('T', Input.Talk);
        }

        private string inputsLastFrame = "";
        private string lastInput = "";

        public override void Update()
        {
            string inputsThisFrame = "";
            foreach ((char id, VirtualButton input) in inputs)
            {
                if (!inputsLastFrame.Contains(id) && input.Pressed)
                {
                    input.ConsumeBuffer();
                    inputsThisFrame += id;
                }
            }

            inputsLastFrame = inputsThisFrame;
            if (inputsThisFrame == "")
            {
                return;
            }
            if (ContainsConflictingInputs(inputsThisFrame))
            {
                CurrentCheatInput = "";
                currentIndex = 0;
            }
            else if (inputsThisFrame.Contains(cheatCode[currentIndex]))
            {
                CurrentCheatInput += cheatCode[currentIndex];
                currentIndex++;
            }
            // if it's the wrong input, reset, but still count it if it matches the first input
            // not perfect but keeping track of it better would be a headache
            else if (inputsThisFrame.Contains(cheatCode[0]))
            {
                CurrentCheatInput = cheatCode[0].ToString();
                currentIndex = 1;
            }
            else
            {
                CurrentCheatInput = "";
                currentIndex = 0;
            }

            if (lastInput != CurrentCheatInput)
            {
                Logger.Log(LogLevel.Verbose, nameof(SSMHelperModule), $"Current cheat input: {CurrentCheatInput}");
            }
            lastInput = CurrentCheatInput;

            if (CurrentCheatInput.Contains(cheatCode))
            {
                if (OnEntered != null)
                {
                    OnEntered();
                }
                Logger.Log(LogLevel.Verbose, nameof(SSMHelperModule), $"Cheat entered successfully: {CurrentCheatInput}");
                CurrentCheatInput = "";
                currentIndex = 0;
            }
        }

        public void AddInput(char id, VirtualButton button)
        {
            inputs.Add(new(id, button));
        }

        // we're intentionally allowing keys with multiple actions to count for any of them in a code
        // but there are a few combos where i don't think anyone should expect them to work
        // this should help prevent cheesing
        private static readonly (char, char)[] conflictingInputs = [
            ('l', 'r'),
            ('u', 'd'),
            ('c', 'b'),
            ('J', 'D'),
        ];
        private static bool ContainsConflictingInputs(string inputs)
        {
            if (inputs.Length > 4) // this is also very unlikely naturally i think
            {
                return true;
            }
            foreach ((char char1, char char2) in conflictingInputs)
            {
                if (inputs.Contains(char1) && inputs.Contains(char2))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
