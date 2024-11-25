using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Walls
{
    public class UIWall
    {
        public class ExportSolidToVisualButton
        {
            private static GameObject _exportSolidToVisualButton;

            private static void GetHandler()
            {
                if (_exportSolidToVisualButton == null)
                    _exportSolidToVisualButton = GameObject.Find("ExportSolidToVisualButton");
            }

            public static void Show()
            {
                GetHandler();
                _exportSolidToVisualButton.SetActive(true);
            }

            public static void Hide()
            {
                GetHandler();
                _exportSolidToVisualButton.SetActive(false);
            }
        }

    }
}
