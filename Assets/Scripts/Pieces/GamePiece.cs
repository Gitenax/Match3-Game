using Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pieces
{
    /// <summary>
    /// Базовый класс представляющий игровой элемент
    /// </summary>
    public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        protected int _score = 0;
        protected Point _index;

        protected Vector2 _position;
        protected RectTransform _rect;

        protected Image _pieceImage;
        
        [SerializeField]
        protected PieceType _type;

        public PiecesData _data;
        
        // Скорость линейной интерполяции при перемещении(свайпу элемента)
        protected float _moveSpeed = 8f;

        protected bool _updating;

        private GamePiece _flipped;
        
        // Свойства
        /// <summary>
        /// Положение элемента на игровой сетке
        /// </summary>
        public Point Index
        {
            get => _index;
            set
            {
                _index = value;
                ResetPosition();
                UpdateName();
            }
        }

        public PieceType Type
        {
            get => _type;
            set => _type = value;
        }

        public RectTransform Rect
        {
            get => _rect;
            set => _rect = value;
        }

        public GamePiece Flipped
        {
            get => _flipped;
            set => _flipped = value;
        }
        
        public void Initialize(PieceType type, Point point)
        {
            _flipped = null;
            _pieceImage = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _type = type;
            Index = point;

            Sprite image = _data.GetImage(_type);
            _pieceImage.sprite = image;
        }
        
        
        public void ResetPosition()
        {
            _position = new Vector2(32 + (64 * _index.X), -32 - (64 * _index.Y));
        }

        

        public bool UpdatePiece()
        {
            if (Vector2.Distance(_rect.anchoredPosition, _position) > 1)
            {
                MoveTo(_position);
                _updating = true;
                return true;
            }

            _rect.anchoredPosition = _position;
            _updating = false;
            return false;
        }
        

        
        public void MoveTo(Vector2 newPosition)
        {
            _rect.anchoredPosition = Vector2.Lerp(
                _rect.anchoredPosition, newPosition, Time.deltaTime * _moveSpeed);
        }
        
        
        
        #region Обработка нажатий
        public void OnPointerDown(PointerEventData eventData)
        {
            if(_updating) return;
            PieceMover.Instance.MovePiece(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PieceMover.Instance.DropPiece();
        }
        #endregion
        
        
        
        
#if UNITY_EDITOR
        public void UpdateName()
        {
            transform.name = $"Piece [{_index.X}, {_index.Y}]";
        }
#endif
    }
}