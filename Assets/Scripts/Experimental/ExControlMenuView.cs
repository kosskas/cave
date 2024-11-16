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
            text.AppendLine("5 - dodaj etykietę");
            text.AppendLine("6 - usuń zaznaczoną etykietę");
            text.AppendLine("7 - następna etykieta");
            text.AppendLine("8/9 - zmień tekst etykiety");

            _controlMenu.text = text.ToString();
        }
    }
}