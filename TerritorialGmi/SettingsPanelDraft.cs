using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace FrontlineLogistics.TerritorialGmi
{
    internal sealed class SettingsPanelDraft
    {
        private readonly TMP_InputField[] fields = new TMP_InputField[Rates.FieldCount];
        private readonly bool[] invalidFields = new bool[Rates.FieldCount];
        private readonly int[] rejectedFrames = new int[Rates.FieldCount];
        private readonly Action<int> setStatus;

        internal SettingsPanelDraft(Action<int> setStatus)
        {
            this.setStatus = setStatus;
        }

        internal void Bind(int index, TMP_InputField field)
        {
            fields[index] = field;
            field.onValidateInput = (text, position, character) => ValidateCharacter(index, text, position, character);
            field.onValueChanged.AddListener(value => DraftChanged(index));
        }

        internal bool TryRead(out Rates rates)
        {
            rates = new Rates();
            return ReadRate(0, out rates.Minimum) && ReadRate(1, out rates.Tile)
                && ReadRate(2, out rates.Road) && ReadRate(3, out rates.Rail)
                && ReadRate(4, out rates.Bridge) && ReadRate(5, out rates.Building);
        }

        private bool ReadRate(int index, out int value)
        {
            value = 0;
            return !invalidFields[index] && TryRate(fields[index].text, out value);
        }

        private static bool TryRate(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text) || text.Length > Rates.MaximumDigits) return false;
            foreach (char character in text)
            {
                if (character < '0' || character > '9') return false;
                value = value * 10 + character - '0';
            }
            return value <= Rates.MaximumValue;
        }

        private char ValidateCharacter(int index, string text, int position, char character)
        {
            if (character >= '0' && character <= '9' && text.Length < Rates.MaximumDigits)
            {
                int value = 0;
                for (int i = 0; i <= text.Length; i++)
                {
                    char digit = i == position ? character : text[i < position ? i : i - 1];
                    value = value * 10 + digit - '0';
                }
                if (value <= Rates.MaximumValue) return character;
            }
            invalidFields[index] = true;
            rejectedFrames[index] = Time.frameCount;
            setStatus(Localization.InvalidRange);
            return '\0';
        }

        private void DraftChanged(int index)
        {
            if (Time.frameCount == rejectedFrames[index]) return;
            invalidFields[index] = false;
            setStatus(0);
        }

        internal void SetDraft(Rates rates)
        {
            SetDraftField(0, rates.Minimum);
            SetDraftField(1, rates.Tile);
            SetDraftField(2, rates.Road);
            SetDraftField(3, rates.Rail);
            SetDraftField(4, rates.Bridge);
            SetDraftField(5, rates.Building);
        }

        private void SetDraftField(int index, int value)
        {
            fields[index].SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
            invalidFields[index] = false;
            rejectedFrames[index] = -1;
        }

        internal void RestoreDefaults()
        {
            SetDraft(Plugin.DefaultRates);
            setStatus(0);
        }
    }
}
