namespace Player.Jokers.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(JokerPlayerUI))]
    public class JokerHandInspector : Editor
    {
        private JokerPlayerUI _playerUI;
        private List<JokerPlayerUI.Placement> _transforms;
        private int _itemCount;

        private void OnEnable()
        {
            _playerUI = (JokerPlayerUI)target;
            _transforms = new List<JokerPlayerUI.Placement>(_itemCount);
        }

        private void OnSceneGUI()
        {
            
            Vector3 center = _playerUI.transform.position;
            Handles.color = Color.cyan;
            Vector3 normal = -_playerUI.transform.forward;
            Vector3 start = Quaternion.AngleAxis(_playerUI.StartAngle, -normal) * _playerUI.transform.right + new Vector3(0,_playerUI.Radius, 0);
            Handles.DrawDottedLine(center, center + start, 3f);
            Handles.DrawWireDisc(center, normal, _playerUI.Radius);

            for (int i = 0; i < _transforms.Count; i++)
            {
                _transforms[i] = _playerUI.CalculateTransforms(i, _itemCount);
            }
            
            for (int i = 0; i < _transforms.Count; i++)
            {
                JokerPlayerUI.Placement placement = _transforms[i];
                float lineExtent = 10f;
                Vector3 h1 = placement.Position - placement.Rotation * new Vector3(-lineExtent, 0, 0);
                Vector3 h2 = placement.Position - placement.Rotation * new Vector3(lineExtent, 0, 0);
                Vector3 v1 = placement.Position - placement.Rotation * new Vector3(0, -lineExtent, 0);
                Vector3 v2 = placement.Position - placement.Rotation * new Vector3(0, lineExtent, 0);
                Vector3 offset = placement.Rotation * new Vector3(lineExtent, lineExtent, 0);
                Handles.color = Color.red;
                Handles.DrawLine(h1, h2, 5);
                Handles.color = Color.green;
                Handles.DrawLine(v1, v2, 5);
                Handles.Label(placement.Position + offset, i.ToString());
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(15);
            EditorGUI.BeginChangeCheck();
            _itemCount = EditorGUILayout.IntField("Item Count", _itemCount);
            if (EditorGUI.EndChangeCheck())
            {
                _transforms.Clear();
                for (int i = 0; i < _itemCount; i++)
                {
                    _transforms.Add(_playerUI.CalculateTransforms(i, _itemCount));
                }
            }
        }
    }
}