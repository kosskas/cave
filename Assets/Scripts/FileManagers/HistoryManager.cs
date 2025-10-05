using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.FileManagers
{
    public class HistoryManager
    {

#if UNITY_EDITOR
        private const string PathToDirectoryWithCheckpoints = "./Assets/History";
#else
    private const string PathToDirectoryWithCheckpoints = "./History";
#endif

        private readonly Regex _nameRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        private const string Extension = ".json";

        private readonly long _maxCheckpoints;

        private readonly string _dir;

        private long _current;


        /*   C O N S T R U C T O R S   */

        /// <summary>
        /// Domyślny katalog.
        /// Brak limitu checkpointów.
        /// </summary>
        public HistoryManager() : this(PathToDirectoryWithCheckpoints) { }

        /// <summary>
        /// Domyślny katalog.
        /// Własny limit checkpointów.
        /// </summary>
        public HistoryManager(long maxCheckpoints) : this(PathToDirectoryWithCheckpoints, maxCheckpoints) { }

        /// <summary>
        /// Własny katalog i limit checkpointów (0 oznacza brak limitu).
        /// </summary>
        public HistoryManager(string directory, long maxCheckpoints = 0L)
        {
            _dir = directory;
            _maxCheckpoints = maxCheckpoints;

            Directory.CreateDirectory(_dir);

            _EnforceRetention();

            _NormalizeIndexes();

            _current = _FindHighestIndexOrZero();

            Debug.Log($"[History] Dir='{_dir}', current={_current}, maxCheckpoints={_maxCheckpoints}");
        }


        /*   P R I V A T E   M E T H O D S   */

        private string _PathFor(long index)
        {
            return Path.Combine(_dir, index + Extension);
        }

        private bool _Exists(long index)
        {
            return File.Exists(_PathFor(index));
        }

        private void _Rename(long fromIdx, long toIdx)
        {
            File.Move(_PathFor(fromIdx), _PathFor(toIdx));
        }

        private void _TryDelete(long idx)
        {
            var path = _PathFor(idx);

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[History] Failed to delete '{path}': {ex}");
            }
        }

        private long _FindHighestIndexOrZero()
        {
            var all = _EnumerateNumericFiles().ToList();
            return all.Any() ? all.Max() : 0;
        }

        private long _FindLowestIndexOrZero()
        {
            var all = _EnumerateNumericFiles().ToList();
            return all.Any() ? all.Min() : 0;
        }

        private IEnumerable<long> _EnumerateNumericFiles()
        {
            if (!Directory.Exists(_dir))
                yield break;

            foreach (var path in Directory.EnumerateFiles(_dir, "*" + Extension, SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(path);

                if (!_nameRegex.IsMatch(name)) 
                    continue;

                long n;
                if (long.TryParse(name, out n) && n > 0)
                    yield return n;
            }
        }

        /// <summary>
        /// Retencja: usuń najstarsze pliki tak, by zostało co najwyżej _maxCheckpoints najnowszych.
        /// Zostawia ciąg końcowy {max-k+1, ..., max}, gdzie max = najwyższy indeks.
        /// </summary>
        private void _EnforceRetention()
        {
            if (_maxCheckpoints <= 0) 
                return;

            var checkpoints = _EnumerateNumericFiles().OrderBy(c => c).ToList();
            var count = checkpoints.Count;

            if (count <= _maxCheckpoints) 
                return;

            var toDelete = count - _maxCheckpoints;

            for (var i = 0; i < toDelete; i++)
            {
                var idx = checkpoints[i];
                _TryDelete(idx);
            }
        }

        /// <summary>
        /// Przenumerowuje pliki z x+1...x+N do 1...N
        /// </summary>
        private void _NormalizeIndexes()
        {
            var lowest = _FindLowestIndexOrZero();
            var highest = _FindHighestIndexOrZero();

            if (lowest == 0 || highest == 0)
                return;

            if (lowest == 1)
                return;

            try
            {
                for (long from = lowest, to = 1L; from <= highest; from++, to++)
                {
                    if (_Exists(to))
                        _TryDelete(to);

                    _Rename(from, to);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[History] Normalizowanie nazw plików nie powiodło się: {ex}");
            }
        }


        /*   P U B L I C   M E T H O D S   */

        /// <summary>
        /// Sprawdza, czy możliwe jest cofnięcie do poprzedniego punktu kontrolnego.
        /// </summary>
        /// <returns>
        /// <c>true</c>, jeśli istnieje wcześniejszy punkt kontrolny; w przeciwnym razie <c>false</c>.
        /// </returns>
        public bool CanUndo()
        {
            var target = _current - 1L;
            Debug.Log($"[History] CanUndo: {_current > 1L && _Exists(target)}");

            return (_current > 1L && _Exists(target));
        }

        /// <summary>
        /// Sprawdza, czy możliwe jest przejście do następnego punktu kontrolnego.
        /// </summary>
        /// <returns>
        /// <c>true</c>, jeśli istnieje kolejny punkt kontrolny; w przeciwnym razie <c>false</c>.
        /// </returns>
        public bool CanRedo()
        {
            var target = _current + 1L;
            Debug.Log($"[History] CanRedo: {_current < long.MaxValue && _Exists(target)}");

            return (_current < long.MaxValue && _Exists(target));
        }

        /// <summary> Cofnij do poprzedniego stanu, jeśli istnieje. </summary>
        public void Undo()
        {
            var target = _current - 1L;

            if (_current <= 1L || !_Exists(target))
            {
                Debug.Log("[History] Undo: no previous state.");
                return;
            }

            var path = _PathFor(target);

            try
            {
                StateManager.Exp.Load(path);
                _current = target;

                Debug.Log($"[History] Undo → loaded #{_current}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[History] Undo failed for '{path}': {ex}");
            }
        }

        /// <summary> Przejdź do następnego stanu, jeśli istnieje. </summary>
        public void Redo()
        {
            var target = _current + 1L;

            if (_current == long.MaxValue || !_Exists(target))
            {
                Debug.Log("[History] Redo: no next state.");
                return;
            }

            var path = _PathFor(target);

            try
            {
                StateManager.Exp.Load(path);
                _current = target;

                Debug.Log($"[History] Redo → loaded #{_current}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[History] Redo failed for '{path}': {ex}");
            }
        }

        /// <summary>
        /// Dodaj checkpoint:
        /// - usuń wszystkie pliki o numerach &gt; _current (czyli "przyszłość")
        /// - zapisz nowy plik o numerze (_current + 1)
        /// - zastosuj retencję (zostaw ostatnie N)
        /// </summary>
        public bool AddCheckpoint()
        {
            try
            {
                // Usuń przyszłość (wszystko > _current)
                var higherIndexes = _EnumerateNumericFiles().Where(i => i > _current).OrderBy(i => i).ToList();

                foreach (var idx in higherIndexes)
                    _TryDelete(idx);

                // Zapisz nowy checkpoint
                var next = _current + 1L;

                StateManager.Exp.Save(PathToDirectoryWithCheckpoints, next.ToString(), false);
                _current = next;

                Debug.Log($"[History] Checkpoint saved #{_current}");

                // Retencja: zostaw N najnowszych
                if (_maxCheckpoints > 0)
                    _EnforceRetention();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[History] AddCheckpoint failed: {ex}");
                return false;
            }
        }

    }
}
