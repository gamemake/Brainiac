﻿using UnityEngine;
using UnityEditor;
using Brainiac;
using System;

namespace BrainiacEditor
{
	public class BehaviourTreeEditor : EditorWindow
	{
		[SerializeField]
		private Texture m_gridTexture;
		[SerializeField]
		private GUISkin m_editorSkin;
		[SerializeField]
		private BTAsset m_btAsset;

		private BTEditorGrid m_grid;
		private BTEditorGraph m_graph;
		private BTEditorCanvas m_canvas;
		private bool m_isDisposed;

		private void OnEnable()
		{
			if(m_gridTexture == null)
			{
				m_gridTexture = Resources.Load<Texture>("Brainiac/background");
			}
			if(m_editorSkin == null)
			{
				m_editorSkin = Resources.Load<GUISkin>("Brainiac/editor_style");
			}

			if(m_graph == null)
			{
				m_graph = BTEditorGraph.Create();
			}
			if(m_canvas == null)
			{
				m_canvas = new BTEditorCanvas();
				m_canvas.Window = this;
				BTEditorCanvas.Current = m_canvas;
			}
			if(m_grid == null)
			{
				m_grid = new BTEditorGrid(m_gridTexture);
			}

			if(m_btAsset != null)
			{
				m_graph.SetBehaviourTree(m_btAsset.GetEditModeTree());
				m_canvas.Position = m_btAsset.CanvasPosition;
				m_canvas.Size = m_btAsset.CanvasSize;
			}

			m_isDisposed = false;
			m_canvas.OnRepaint += Repaint;
		}

		private void OnDisable()
		{
			Dispose();
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void Dispose()
		{
			if(!m_isDisposed)
			{
				if(m_graph != null)
				{
					BTEditorGraph.DestroyImmediate(m_graph);
					m_graph = null;
				}

				SaveBehaviourTree();
				m_isDisposed = true;
			}
		}

		private void SetBTAsset(BTAsset asset)
		{
			if(asset != null && asset != m_btAsset)
			{
				SaveBehaviourTree();

				m_btAsset = asset;
				m_graph.SetBehaviourTree(m_btAsset.GetEditModeTree());
				m_canvas.Position = m_btAsset.CanvasPosition;
				m_canvas.Size = m_btAsset.CanvasSize;
			}
		}

		private void SaveBehaviourTree()
		{
			if(m_btAsset != null)
			{
				m_btAsset.CanvasPosition = m_canvas.Position;
				m_btAsset.CanvasSize = m_canvas.Size;
				m_btAsset.Serialize();
				m_btAsset.Dispose();
				EditorUtility.SetDirty(m_btAsset);
			}
		}

		private void OnGUI()
		{
			if(m_btAsset != null)
			{
				BTEditorStyle.EnsureStyle(m_editorSkin);
				m_grid.DrawGUI();
				m_graph.DrawGUI();
				m_canvas.HandleEvents(this);
			}
		}

		public static void Open(BTAsset behaviourTree)
		{
			var window = EditorWindow.GetWindow<BehaviourTreeEditor>("Brainiac");
			window.SetBTAsset(behaviourTree);
		}
	}
}