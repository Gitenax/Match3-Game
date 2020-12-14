using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

	public int Value; // 0 - blank, 1 - cube, 2 - sphere, 3 - cylinder, 4 - pyramid, 5 - diamond, -1 - hole
	public Point Index;

	[HideInInspector] public NodePiece Flipped;
	[HideInInspector] public Vector2 Position;

	[HideInInspector]
	public RectTransform Rect;

	private Image _pieceImage;

	private bool _updating;


	public bool UpdatePiece()
	{
		if (Vector2.Distance(Rect.anchoredPosition, Position) > 1)
		{
			MovePositionTo(Position);
			_updating = true;
			return true;
		}

		Rect.anchoredPosition = Position;
		_updating = false;
		return false;
	}

	
	public void Initialize(int value, Point point, Sprite piece)
	{
		Flipped = null;
		_pieceImage = GetComponent<Image>();
		Rect = GetComponent<RectTransform>();

		Value = value;
		SetIndex(point);
		_pieceImage.sprite = piece;
	}

	public void SetIndex(Point point)
	{
		Index = point;
		ResetPosition();
		UpdateName();
	}


	void UpdateName()
	{
		transform.name = $"Node [{Index.x}, {Index.y}]";
	}


	public void ResetPosition()
	{
		Position = new Vector2(32 + (64 * Index.x), -32 - (64 * Index.y));
	}


	public void MovePosition(Vector2 position)
	{
		Rect.anchoredPosition +=  position * Time.deltaTime * 16f;
	}
	
	public void MovePositionTo(Vector2 position)
	{
		Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, position, Time.deltaTime * 16f);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if(_updating) return;
		MovePieces.Instance.MovePiece(this);
		// Debug.Log($"Grab {transform.name}");
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		MovePieces.Instance.DropPiece();
		// Debug.Log($"let go {transform.name}");
	}
}
