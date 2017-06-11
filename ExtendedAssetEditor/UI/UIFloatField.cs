using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtendedAssetEditor.UI
{
    public struct UIFloatField
    {
        public UIPanel panel;
        public UITextField textField;
        public UILabel label;
        public UIButton buttonUp;
        public UIButton buttonDown;

        public void SetValue(float value)
        {
            if(textField != null)
            {
                textField.text = value.ToString();
            }
        }

        public void FloatFieldHandler(ref float target)
        {
            FloatFieldHandler(this.textField, this.textField.text, ref target);
        }

        public static UIFloatField CreateField(string label, UIComponent parent, bool buttons = true)
        {
            UIFloatField field = new UIFloatField();
            field.panel = parent.AddUIComponent<UIPanel>();

            field.label = field.panel.AddUIComponent<UILabel>();
            field.label.text = label;
            field.label.relativePosition = new Vector3(0, 2);

            field.textField = UIUtils.CreateTextField(field.panel);
            field.textField.relativePosition = new Vector3(field.label.width + 10, 0);

            if(buttons)
            {

                field.buttonDown = UIUtils.CreateButton(field.panel);
                field.buttonDown.text = "-";
                field.buttonDown.height = 20;
                field.buttonDown.width = 30;
                field.buttonDown.relativePosition = field.textField.relativePosition + new Vector3(field.textField.width + 10, 0);

                field.buttonUp = UIUtils.CreateButton(field.panel);
                field.buttonUp.text = "+";
                field.buttonUp.height = 20;
                field.buttonUp.width = 30;
                field.buttonUp.relativePosition = field.buttonDown.relativePosition + new Vector3(field.buttonDown.width + 10, 0);

            }

            field.panel.width = buttons ? field.buttonUp.relativePosition.x + field.buttonUp.width : field.textField.relativePosition.x + field.textField.width;
            field.panel.height = buttons ? field.buttonUp.relativePosition.y + field.buttonUp.height : field.textField.relativePosition.y + field.textField.height;

            return field;
        }

        public static UIFloatField CreateField(string label, int labelWidth, UIComponent parent, bool buttons = true)
        {
            var field = CreateField(label, parent, buttons);
            field.label.width = labelWidth;
            field.textField.relativePosition = new Vector3(labelWidth + 10, 0);
            return field;
        }

        public static void FloatFieldHandler(UITextField field, string value, ref float target)
        {
            float v;
            if(float.TryParse(value, out v))
            {
                target = v;
                field.color = Color.white;
            }
            else
            {
                field.color = Color.red;
            }
        }
    }
}
