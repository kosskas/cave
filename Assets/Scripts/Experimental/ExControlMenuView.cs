using Assets.Scripts.Experimental.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class ExControlMenuView
    {
        private readonly TextMeshPro _controlMenu;

        public ExControlMenuView()
        {
            _controlMenu = GameObject.Find("ExperimentalModeControlMenu")?.GetComponent<TextMeshPro>();

            if (_controlMenu == null)
                return;

            _controlMenu.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");

            var text = new StringBuilder();

            text.AppendLine("1 - zmień rysowany obiekt");
            text.AppendLine("2 - start/stop rysowania");
            text.AppendLine("3 - usuń wskazany obiekt");
            text.AppendLine("4 - budowanie ścian");
            text.AppendLine("8/9 - zmień etykietę");

            _controlMenu.text = text.ToString();
        }
    }
}