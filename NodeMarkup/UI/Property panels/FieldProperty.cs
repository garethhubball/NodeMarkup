﻿using ColossalFramework.UI;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public abstract class FieldPropertyPanel<ValueType> : EditorPropertyPanel
    {
        protected UITextField Field { get; set; }

        public event Action<ValueType> OnValueChanged;
        public event Action OnHover;
        public event Action OnLeave;

        protected abstract bool CanUseWheel { get; }
        public bool UseWheel { get; set; }
        public ValueType Step { get; set; }
        public float FieldWidth
        {
            get => Field.width;
            set => Field.width = value;
        }

        private bool ValueProgress { get; set; } = false;
        public virtual ValueType Value
        {
            get
            {
                try
                {
                    return (ValueType)TypeDescriptor.GetConverter(typeof(ValueType)).ConvertFromString(Field.text);
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                if (!ValueProgress)
                {
                    ValueProgress = true;
                    Field.text = value.ToString();
                    OnValueChanged?.Invoke(value);
                    ValueProgress = false;
                }
            }
        }

        public FieldPropertyPanel()
        {
            Field = Control.AddUIComponent<UITextField>();
            Field.atlas = NodeMarkupPanel.InGameAtlas;
            Field.normalBgSprite = "TextFieldPanel";
            Field.hoveredBgSprite = "TextFieldPanelHovered";
            Field.focusedBgSprite = "TextFieldPanel";
            Field.selectionSprite = "EmptySprite";
            Field.allowFloats = true;
            Field.isInteractive = true;
            Field.enabled = true;
            Field.readOnly = false;
            Field.builtinKeyNavigation = true;
            Field.cursorWidth = 1;
            Field.cursorBlinkTime = 0.45f;
            Field.selectOnFocus = true;
            Field.tooltip = CanUseWheel ? NodeMarkup.Localize.FieldPanel_ScrollWheel : string.Empty;
            Field.eventMouseWheel += FieldMouseWheel;
            Field.eventTextSubmitted += FieldTextSubmitted;
            Field.eventMouseHover += FieldHover;
            Field.eventMouseLeave += FieldLeave;
            Field.textScale = 0.7f;
            Field.verticalAlignment = UIVerticalAlignment.Middle;
            Field.padding = new RectOffset(0, 0, 6, 0);

        }



        protected abstract ValueType Increment(ValueType value, ValueType step);
        protected abstract ValueType Decrement(ValueType value, ValueType step);

        protected virtual void FieldTextSubmitted(UIComponent component, string value) => Value = Value;
        private void FieldHover(UIComponent component, UIMouseEventParameter eventParam) => OnHover?.Invoke();
        private void FieldLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke();
        protected virtual void FieldMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (CanUseWheel && UseWheel)
            {
                if (eventParam.wheelDelta < 0)
                    Value = Increment(Value, Step);
                else
                    Value = Decrement(Value, Step);
            }
        }
    }
    public abstract class ComparableFieldPropertyPanel<ValueType> : FieldPropertyPanel<ValueType>
        where ValueType : IComparable<ValueType>
    {
        public ValueType MinValue { get; set; } = default;
        public ValueType MaxValue { get; set; } = default;
        public bool CheckMax { get; set; } = false;
        public bool CheckMin { get; set; } = false;

        public override ValueType Value 
        { 
            get => base.Value;
            set
            {
                var newValue = value;

                if (CheckMin && newValue.CompareTo(MinValue) < 0)
                    newValue = MinValue;

                if (CheckMax && newValue.CompareTo(MaxValue) > 0)
                    newValue = MaxValue;

                base.Value = newValue;
            }
        }
    }
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float>
    {
        protected override bool CanUseWheel => true;

        protected override float Decrement(float value, float step) => (value + step).RoundToNearest(step);
        protected override float Increment(float value, float step) => (value - step).RoundToNearest(step);
    }
    public class StringPropertyPanel : FieldPropertyPanel<string>
    {
        protected override bool CanUseWheel => false;

        protected override string Decrement(string value, string step) => throw new NotSupportedException();
        protected override string Increment(string value, string step) => throw new NotSupportedException();
    }
}