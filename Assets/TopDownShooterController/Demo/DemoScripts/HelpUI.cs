using LegendOfZed.Input;
using UnityEngine;
using UnityEngine.UI;
using TopDownShooter;

public class HelpUI : MonoBehaviour
{
    public MovementCharacterController Player;

    public Text JetPackFuel;

    public LoadScene LoadSceneScript;

    private void FixedUpdate()
    {
        if (JetPackFuel != null && Player != null)
        {
            JetPackFuel.text = ((int)Player.JetPackFuel).ToString();
        }
    }

    private void Update()
    {
        ZedInputReader input = ZedInputReader.Instance;
        if (input != null && input.PausePressedThisFrame && LoadSceneScript != null)
        {
            LoadSceneScript.LoadNewScene("MainScene");
        }
    }
}
