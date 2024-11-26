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

            private static GameObject GetHandler()
            {
                if (_exportSolidToVisualButton == null)
                    _exportSolidToVisualButton = GameObject.Find("ExportSolidToVisualButton");

                return _exportSolidToVisualButton;
            }

            public static void Show()
            {
                GetHandler()?.SetActive(true);
            }

            public static void Hide()
            {
                GetHandler()?.SetActive(false);
            }
        }

        public class SaveLoadStateButtons
        {
            private static GameObject _saveStateButton;
            private static GameObject _loadStateButton;

            private static List<GameObject> GetHandler()
            {
                if (_saveStateButton == null)
                    _saveStateButton = GameObject.Find("SaveStateButton");

                if (_loadStateButton == null)
                    _loadStateButton = GameObject.Find("LoadStateButton");

                return new List<GameObject>()
                {
                    _saveStateButton,
                    _loadStateButton
                };
            }

            public static void Show()
            {
                GetHandler().ForEach(btn => btn?.SetActive(true));
            }

            public static void Hide()
            {
                GetHandler().ForEach(btn => btn?.SetActive(false));
            }
        }

        public class BackToMenuButton
        {
            private static GameObject _backToMenuButton;

            private static GameObject GetHandler()
            {
                if (_backToMenuButton == null)
                    _backToMenuButton = GameObject.Find("BackToMenuButton");


                return _backToMenuButton;
            }

            public static void Show()
            {
                GetHandler()?.SetActive(true);
            }

            public static void Hide()
            {
                GetHandler()?.SetActive(false);
            }
        }

        public class MenuButtons
        {
            private static GameObject _visualizationButton;
            private static GameObject _reconstructionWithGridButton;
            private static GameObject _reconstructionButton;

            private static List<GameObject> GetHandler()
            {
                if (_visualizationButton == null)
                    _visualizationButton = GameObject.Find("WizButton");

                if (_reconstructionWithGridButton == null)
                    _reconstructionWithGridButton = GameObject.Find("KreaButton");

                if (_reconstructionButton == null)
                    _reconstructionButton = GameObject.Find("ExpButton");

                return new List<GameObject>()
                {
                    _visualizationButton,
                    _reconstructionWithGridButton,
                    _reconstructionButton
                };
            }

            public static void Show()
            {
                GetHandler().ForEach(btn => btn?.SetActive(true));
            }

            public static void Hide()
            {
                GetHandler().ForEach(btn => btn?.SetActive(false));
            }
        }
    }
}
