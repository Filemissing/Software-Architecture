using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Card : ScriptableObject
{
    public string title;
    public Texture2D texture;
    public Sprite backgroundSprite;
    public Sprite borderSprite;

    public abstract void Play();

    public virtual void OnHover() { }
    public virtual void OnStartDrag() { }
    public virtual void OnDrag() { }
    public virtual void OnEndDrag() { }
}
