using UnityEngine;

public class Player : MonoBehaviour
{
    private static GameObject _player;
    public static GameObject Instance => _player;

    public void Awake()
    {
        _player = this.gameObject;
    }
}
