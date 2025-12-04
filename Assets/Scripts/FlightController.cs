using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlightController : MonoBehaviour
{
    [SerializeField] float MaxThrust;
    [SerializeField] float Mass;
    [SerializeField] float MaxAileronDeflection;
    [SerializeField] float AileronRollFactor;
    [SerializeField] float MaxElevatorDeflection;
    [SerializeField] float ElevatorPitchFactor;
    [SerializeField] float MaxRudderDeflection;
    [SerializeField] float RudderYawFactor;

    private Rigidbody Rigidbody;
    private ControlScheme Controls;

    private float Thrust;

    private void Start()
    {
        Controls = new ControlScheme();
        Controls.Enable();
        Rigidbody = GetComponent<Rigidbody>();
        Thrust = 0;
        Rigidbody.mass = Mass;
    }

    void StabalizingForce()
    {
        float deltaRoll = transform.rotation.eulerAngles.z;
        
    }

    private void Update()
    {

        Thrust += Controls.Aircraft.ThrottleAdjust.ReadValue<float>();
        Thrust = Mathf.Clamp(Thrust, 0, MaxThrust);
        Rigidbody.AddForce(transform.forward * Thrust, ForceMode.Force);

        float pitchCommand = Controls.Aircraft.Pitch.ReadValue<float>();
        Rigidbody.AddTorque(transform.right * pitchCommand, ForceMode.Force);

        float rollCommand = Controls.Aircraft.Roll.ReadValue<float>();
        Rigidbody.AddTorque(transform.forward * rollCommand, ForceMode.Force);

        float yawCommand = Controls.Aircraft.Yaw.ReadValue<float>();
        Rigidbody.AddTorque(transform.up * yawCommand, ForceMode.Force);
    }

}
