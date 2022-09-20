using UnityEngine;
using TMPro;

public class StateMachine : MonoBehaviour
{
    private State currentState;

    public TextMeshProUGUI textMesh;

    private void Start()
    {
        ChangeState(new Idle(this)); //default state
    }
    public void Update()
    {
        if (currentState != null)
        {
            currentState.DoState();
        }
        else
        {
            Debug.LogError("State is : " + currentState);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log(currentState);
        }
    }

    public void ChangeState(State newState)
    {
        if (currentState != null)
            currentState.ExitState();

        if (textMesh != null)
        {
            textMesh.text = newState.ToString();
        }

        currentState = newState;

        currentState.EnterState();
    }
}