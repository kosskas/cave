﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class ExContextMenuView
    {
        private readonly TextMeshPro _contextMenu;

        public ExContextMenuView()
        {
            _contextMenu = GameObject.Find("ExperimentalModeContextMenu")?.GetComponent<TextMeshPro>();

            if (_contextMenu == null)
                return;

            _contextMenu.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
        }

        public void SetCurrentContext(ExContext currentContext)
        {
            if (_contextMenu == null)
                return;

            var text = new StringBuilder();

            foreach (ExContext context in Enum.GetValues(typeof(ExContext)))
            {
                text.AppendLine(context == currentContext
                    ? $"<color=#FFFFFF>{context.GetDescription()}</color>"
                    : $"<color=#000000>{context.GetDescription()}</color>");
            }

            _contextMenu.text = text.ToString();
        }
    }
}
