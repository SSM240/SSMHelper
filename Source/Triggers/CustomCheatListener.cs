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
        private int currentIndex;

        private List<(char, VirtualButton)> inputs;

        private string cheatCode;
        private Action OnEntered;

        private string _currentCheatInput;
        public string CurrentCheatInput
        {
            get
            {
                return _currentCheatInput;
            }
            set
            {
                _currentCheatInput = value;
                currentIndex = _currentCheatInput.Length;
            }
        }

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
            foreach ((char id, VirtualButton button) in inputs)
            {
                if (!inputsLastFrame.Contains(id) && PressedThisFrameSpecifically(button))
                {
                    inputsThisFrame += id;
                }
            }

            inputsLastFrame = inputsThisFrame;
            UpdateButtonsHeldLastFrame();
            if (inputsThisFrame == "")
            {
                return;
            }
            if (ContainsConflictingInputs(inputsThisFrame))
            {
                CurrentCheatInput = "";
            }
            else if (inputsThisFrame.Contains(cheatCode[currentIndex]))
            {
                CurrentCheatInput += cheatCode[currentIndex];
            }
            // if it's the wrong input, reset, but still count it if it matches the first input
            // not perfect but keeping track of it better would be a headache
            else if (inputsThisFrame.Contains(cheatCode[0]))
            {
                CurrentCheatInput = cheatCode[0].ToString();
            }
            else
            {
                CurrentCheatInput = "";
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

        // this shit sucks ass
        // (this is the only way to ignore the buffer system entirely)
        private HashSet<VirtualButton> buttonsHeldLastFrame = new();
        private bool PressedThisFrameSpecifically(VirtualButton button)
        {
            return !buttonsHeldLastFrame.Contains(button) && button.Pressed;
        }
        private void UpdateButtonsHeldLastFrame()
        {
            buttonsHeldLastFrame.Clear();
            foreach ((_, VirtualButton button) in inputs)
            {
                if (button.Check)
                {
                    buttonsHeldLastFrame.Add(button);
                }
            }
        }
    }
}
