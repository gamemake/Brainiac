﻿using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Brainiac;

namespace BrainiacEditor
{
	public class NodeInspector
	{
		private BehaviourNode m_target;

		protected BehaviourNode Target
		{
			get { return m_target; }
		}

		public virtual void SetTarget(BehaviourNode target)
		{
			m_target = target;
		}

		public virtual void OnInspectorGUI()
		{
			if(m_target != null)
			{
				m_target.Name = EditorGUILayout.TextField("Name", m_target.Name);
				EditorGUILayout.LabelField("Description");
				m_target.Description = EditorGUILayout.TextArea(m_target.Description, BTEditorStyle.MultilineTextArea);

				EditorGUILayout.Space();
				DrawProperties();

				if(BTEditorCanvas.Current != null)
				{
					BTEditorCanvas.Current.Repaint();
				}
			}
		}

		protected void DrawProperties()
		{
			Type nodeType = m_target.GetType();
			var fields = from fi in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
						 select fi;
			var properties = from pi in nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
							 select pi;

			foreach(var field in fields)
			{
				object[] attributes = field.GetCustomAttributes(typeof(BTPropertyAttribute), true);
				if(attributes.Length == 0)
					continue;

				BTPropertyAttribute attribute = attributes[0] as BTPropertyAttribute;
				string label = string.IsNullOrEmpty(attribute.PropertyName) ? field.Name : attribute.PropertyName;
				
				if(field.FieldType == typeof(MemoryVar))
				{
					DrawMemoryVarField(label, (MemoryVar)field.GetValue(m_target));
				}
				else
				{
					object value = null;
					if(TryToDrawField(label, field.GetValue(m_target), field.FieldType, out value))
					{
						field.SetValue(m_target, value);
					}	
				}
			}
			foreach(var property in properties)
			{
				object[] attributes = property.GetCustomAttributes(typeof(BTPropertyAttribute), true);
				if(attributes.Length == 0)
					continue;

				BTPropertyAttribute attribute = attributes[0] as BTPropertyAttribute;
				string label = string.IsNullOrEmpty(attribute.PropertyName) ? property.Name : attribute.PropertyName;
				
				if(property.PropertyType == typeof(MemoryVar))
				{
					DrawMemoryVarField(label, (MemoryVar)property.GetValue(m_target, null));
				}
				else
				{
					object value = null;
					if(TryToDrawField(label, property.GetValue(m_target, null), property.PropertyType, out value))
					{
						property.SetValue(m_target, value, null);
					}
				}
			}
		}

		private bool TryToDrawField(string label, object currentValue, Type type, out object value)
		{
			bool success = true;

			if(type == typeof(bool))
			{
				value = EditorGUILayout.Toggle(label, (bool)currentValue);
			}
			else if(type == typeof(int))
			{
				value = EditorGUILayout.IntField(label, (int)currentValue);
			}
			else if(type == typeof(float))
			{
				value = EditorGUILayout.FloatField(label, (float)currentValue);
			}
			else if(type == typeof(string))
			{
				value = EditorGUILayout.TextField(label, (string)currentValue);
			}
			else if(type == typeof(Vector2))
			{
				value = EditorGUILayout.Vector2Field(label, (Vector2)currentValue);
			}
			else if(type == typeof(Vector3))
			{
				value = EditorGUILayout.Vector3Field(label, (Vector3)currentValue);
			}
			else if(type.IsEnum)
			{
				value = EditorGUILayout.EnumPopup(label, (Enum)currentValue);
			}
			else
			{
				value = null;
				success = false;
			}

			return success;
		}

		private void DrawMemoryVarField(string label, MemoryVar memVar)
		{
			if(memVar != null)
			{
				memVar.Content = EditorGUILayout.TextField(label, memVar.Content);
			}
		}
	}
}