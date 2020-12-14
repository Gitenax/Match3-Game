using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KilledPiece : MonoBehaviour
{
    private bool _falling;
    public bool Falling => _falling;

    private float _speed = 16f;
    private float _gravity = 32f;
    private Vector2 _moveDirection;
    private RectTransform _rect;
    private Image _image;

    public void Initialize(Sprite piece, Vector2 start)
    {
        _falling = true;
        
        _moveDirection = Vector2.up;
        _moveDirection.x = Random.Range(-1f, 1f);
        _moveDirection *= _speed / 2;

        _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
        _image.sprite = piece;
        _rect.anchoredPosition = start;
    }

    void Update()
    {
        if (_falling)
        {
            _moveDirection.y -= Time.deltaTime * _gravity;
            _moveDirection.x = Mathf.Lerp(_moveDirection.x, 0, Time.deltaTime);
            _rect.anchoredPosition += (_moveDirection * Time.deltaTime * _speed);
            if (_rect.position.x < -32f
                || _rect.position.x > Screen.width + 32f
                || _rect.position.y < -32f
                || _rect.position.y > Screen.height + 32f)
                _falling = false;
        }
    }
}
