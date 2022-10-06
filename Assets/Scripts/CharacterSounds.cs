using UnityEngine;

public class CharacterSounds : MonoBehaviour
{
    [SerializeField] private Player player = null;

    public void RunSound()
    {
        player.RunSound();
    }
}
