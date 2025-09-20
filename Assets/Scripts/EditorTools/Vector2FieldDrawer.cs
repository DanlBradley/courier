using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    // Attribute to mark Vector2 fields that should use the visual field drawer
    public class Vector2FieldAttribute : PropertyAttribute
    {
        public readonly bool showGrid;
        public readonly Color fieldColor;
        public readonly Color gridColor;
    
        public Vector2FieldAttribute(bool showGrid = true)
        {
            this.showGrid = showGrid;
            fieldColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            gridColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Vector2FieldAttribute))]
    public class Vector2FieldDrawer : PropertyDrawer
    {
        private const float FieldSize = 100f;
        private const float PointSize = 8f;
        private const float LabelHeight = 16f;
        private const float Spacing = 2f;
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                return EditorGUIUtility.singleLineHeight;
            }
        
            return LabelHeight + Spacing + FieldSize + Spacing + EditorGUIUtility.singleLineHeight;
        }
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.LabelField(position, label.text, "Use Vector2Field with Vector2 only");
                return;
            }
        
            Vector2FieldAttribute fieldAttr = attribute as Vector2FieldAttribute;
        
            EditorGUI.BeginProperty(position, label, property);
        
            // Draw label
            Rect labelRect = new Rect(position.x, position.y, position.width, LabelHeight);
            EditorGUI.LabelField(labelRect, label);
        
            // Calculate field rect
            float fieldX = position.x + (position.width - FieldSize) * 0.5f;
            Rect fieldRect = new Rect(fieldX, position.y + LabelHeight + Spacing, FieldSize, FieldSize);
        
            // Draw the visual field
            DrawVisualField(fieldRect, property, fieldAttr);
        
            // Draw the actual Vector2 fields below for precise input
            Rect vector2Rect = new Rect(position.x, position.y + LabelHeight + Spacing + FieldSize + Spacing, 
                position.width, EditorGUIUtility.singleLineHeight);
        
            Vector2 currentValue = property.vector2Value;
            Vector2 newValue = EditorGUI.Vector2Field(vector2Rect, GUIContent.none, currentValue);
        
            // Clamp values to 0-1 range
            newValue.x = Mathf.Clamp01(newValue.x);
            newValue.y = Mathf.Clamp01(newValue.y);
        
            if (newValue != currentValue)
            {
                property.vector2Value = newValue;
            }
        
            EditorGUI.EndProperty();
        }
    
        private void DrawVisualField(Rect fieldRect, SerializedProperty property, Vector2FieldAttribute fieldAttr)
        {
            Event currentEvent = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
        
            // Draw background
            EditorGUI.DrawRect(fieldRect, fieldAttr.fieldColor);
        
            // Draw grid if enabled
            if (fieldAttr.showGrid)
            {
                DrawGrid(fieldRect, fieldAttr.gridColor);
            }
        
            // Draw border
            Handles.color = Color.gray;
            Vector3 topLeft = new Vector3(fieldRect.x, fieldRect.y);
            Vector3 topRight = new Vector3(fieldRect.x + fieldRect.width, fieldRect.y);
            Vector3 bottomLeft = new Vector3(fieldRect.x, fieldRect.y + fieldRect.height);
            Vector3 bottomRight = new Vector3(fieldRect.x + fieldRect.width, fieldRect.y + fieldRect.height);
        
            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
        
            // Get current value and convert to field coordinates
            Vector2 currentValue = property.vector2Value;
            Vector2 pointPos = new Vector2(
                fieldRect.x + currentValue.x * fieldRect.width,
                fieldRect.y + (1f - currentValue.y) * fieldRect.height // Flip Y for intuitive top-down positioning
            );
        
            // Draw the point
            Rect pointRect = new Rect(pointPos.x - PointSize * 0.5f, pointPos.y - PointSize * 0.5f, PointSize, PointSize);
            EditorGUI.DrawRect(pointRect, Color.red);
        
            // Handle mouse input
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (fieldRect.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
                    {
                        GUIUtility.hotControl = controlID;
                        UpdateValueFromMouse(fieldRect, property, currentEvent.mousePosition);
                        currentEvent.Use();
                    }
                    break;
                
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        UpdateValueFromMouse(fieldRect, property, currentEvent.mousePosition);
                        currentEvent.Use();
                    }
                    break;
                
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;
            }
        
            // Show coordinates as tooltip when hovering
            if (fieldRect.Contains(currentEvent.mousePosition))
            {
                Vector2 mouseNormalized = new Vector2(
                    (currentEvent.mousePosition.x - fieldRect.x) / fieldRect.width,
                    1f - (currentEvent.mousePosition.y - fieldRect.y) / fieldRect.height
                );
                mouseNormalized.x = Mathf.Clamp01(mouseNormalized.x);
                mouseNormalized.y = Mathf.Clamp01(mouseNormalized.y);
            
                GUI.tooltip = $"({mouseNormalized.x:F3}, {mouseNormalized.y:F3})";
            }
        }
    
        private void UpdateValueFromMouse(Rect fieldRect, SerializedProperty property, Vector2 mousePos)
        {
            Vector2 normalizedPos = new Vector2(
                (mousePos.x - fieldRect.x) / fieldRect.width,
                1f - (mousePos.y - fieldRect.y) / fieldRect.height // Flip Y
            );
        
            // Clamp to 0-1 range
            normalizedPos.x = Mathf.Clamp01(normalizedPos.x);
            normalizedPos.y = Mathf.Clamp01(normalizedPos.y);
        
            property.vector2Value = normalizedPos;
            property.serializedObject.ApplyModifiedProperties();
        }
    
        private void DrawGrid(Rect fieldRect, Color gridColor)
        {
            Handles.color = gridColor;
        
            // Draw vertical lines
            for (int i = 1; i < 4; i++)
            {
                float x = fieldRect.x + (fieldRect.width / 4f) * i;
                Handles.DrawLine(new Vector3(x, fieldRect.y), new Vector3(x, fieldRect.y + fieldRect.height));
            }
        
            // Draw horizontal lines
            for (int i = 1; i < 4; i++)
            {
                float y = fieldRect.y + (fieldRect.height / 4f) * i;
                Handles.DrawLine(new Vector3(fieldRect.x, y), new Vector3(fieldRect.x + fieldRect.width, y));
            }
        
            // Draw center cross
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridColor.a * 1.5f);
            float centerX = fieldRect.x + fieldRect.width * 0.5f;
            float centerY = fieldRect.y + fieldRect.height * 0.5f;
            Handles.DrawLine(new Vector3(centerX, fieldRect.y), new Vector3(centerX, fieldRect.y + fieldRect.height));
            Handles.DrawLine(new Vector3(fieldRect.x, centerY), new Vector3(fieldRect.x + fieldRect.width, centerY));
        }
    }
#endif
}