using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MazinGames.GoogleSheetsDatabase.Editor
{
    [CustomEditor(typeof(DataContainerBase), true)]
    public class DataContainerCe : UnityEditor.Editor
    {
        private bool _foldout = true;
        private Dictionary<string, bool> _pagesToggles;

        private ImportQueue _importQueue;

        public override void OnInspectorGUI()
        {
            var container = (DataContainerBase)target;

            _foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout, "Import settings");
            if (_foldout)
            {
                DrawGUI(container);
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(16);

            base.OnInspectorGUI();
        }

        private void DrawGUI(DataContainerBase container)
        {
            var listsInfos = container.GetType().GetFields()
                .Where(fi => Attribute.IsDefined(fi, typeof(PageNameAttribute)))
                .OrderBy(i => i.Name).ToArray();

            if (_pagesToggles == null)
            {
                _pagesToggles = new Dictionary<string, bool>();
                foreach (var info in listsInfos)
                {
                    _pagesToggles.Add(info.Name, EditorPrefs.GetBool(info.Name));
                }
            }

            EditorGUILayout.BeginVertical("box");

            #region Draw doc ID field

            container._documentID = EditorGUILayout.TextField(
                new GUIContent(
                    "Document Id",
                    "The XXXX part in 'https://docs.google.com/spreadsheets/d/XXXX/edit' URL. NOTE: The document must be accessable by link."),
                container._documentID);

            #endregion

            #region Draw controll buttons

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("All"))
            {
                SelectAll(true);
            }

            if (GUILayout.Button("None"))
            {
                SelectAll(false);
            }

            if (GUILayout.Button("Import"))
            {
                if (_pagesToggles.Any(t => t.Value == true))
                {
                    if (string.IsNullOrEmpty(container._documentID))
                    {
                        Debug.LogError($"Document ID is not specified!");
                        return;
                    }

                    EditorUtility.DisplayProgressBar("Downloading definitions", "Initializing...", 0);

                    _importQueue = new ImportQueue(container, listsInfos.Where(i => _pagesToggles[i.Name] == true).ToArray());

                    _importQueue.onComplete += OnImportQueueComplete;
                    _importQueue.onOutputChanged += OnOutputChanged;
                    _importQueue.onProgressChanged += OnProgressChanged;

                    _importQueue.Run();
                }
                else
                {
                    Debug.LogWarning("Nothing is selected to import");
                }
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Draw import flags

            EditorGUILayout.LabelField("Pages to import:");

            EditorGUI.indentLevel += 1;

            var keys = new List<string>(_pagesToggles.Keys);
            foreach (var key in keys)
            {
                _pagesToggles[key] = EditorGUILayout.Toggle($"{key}", _pagesToggles[key]);
                EditorPrefs.SetBool(key, _pagesToggles[key]);
            }

            EditorGUI.indentLevel -= 1;

            #endregion

            EditorGUILayout.EndVertical();
        }

        private void SelectAll(bool mode)
        {
            var keys = new List<string>(_pagesToggles.Keys);
            foreach (var key in keys)
            {
                _pagesToggles[key] = mode;
            }
        }

        private void OnProgressChanged()
        {
            EditorUtility.DisplayProgressBar("Downloading definitions", _importQueue.Output, _importQueue.Progress);
        }

        private void OnOutputChanged()
        {
            EditorUtility.DisplayProgressBar("Downloading definitions", _importQueue.Output, _importQueue.Progress);
        }

        private void OnImportQueueComplete(DataContainerBase container)
        {
            EditorUtility.SetDirty(container);

            EditorUtility.ClearProgressBar();

            _importQueue.onComplete -= OnImportQueueComplete;
            _importQueue.onOutputChanged -= OnOutputChanged;
            _importQueue.onProgressChanged -= OnProgressChanged;
            _importQueue = null;
        }
    }
}